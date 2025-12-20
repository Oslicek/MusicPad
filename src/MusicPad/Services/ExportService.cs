using MusicPad.Core.Export;
using MusicPad.Core.Recording;
using MusicPad.Views;

namespace MusicPad.Services;

/// <summary>
/// Service for exporting songs to various formats.
/// </summary>
public class ExportService
{
    private readonly ISfzService _sfzService;
    
    public ExportService(ISfzService sfzService)
    {
        _sfzService = sfzService;
    }
    
    /// <summary>
    /// Exports a song to the specified format.
    /// </summary>
    /// <returns>Tuple of (file path, MIME type) or (null, null) on failure.</returns>
    public async Task<(string? FilePath, string? MimeType)> ExportAsync(
        Song song, 
        IReadOnlyList<RecordedEvent> events, 
        ExportFormat format)
    {
        var exportDir = Path.Combine(FileSystem.CacheDirectory, "Exports");
        Directory.CreateDirectory(exportDir);
        
        var safeName = SanitizeFileName(song.Name);
        
        return format switch
        {
            ExportFormat.MidiNaked => await ExportMidiNakedAsync(safeName, events, exportDir),
            ExportFormat.MidiEnhanced => await ExportMidiEnhancedAsync(safeName, events, exportDir),
            ExportFormat.MidiComplete => await ExportMidiCompleteAsync(song, safeName, events, exportDir),
            ExportFormat.Wav => await ExportWavAsync(song, safeName, events, exportDir),
            ExportFormat.Flac => await ExportFlacAsync(song, safeName, events, exportDir),
            _ => (null, null)
        };
    }
    
    /// <summary>
    /// MIDI export with just notes and timing.
    /// </summary>
    private async Task<(string?, string?)> ExportMidiNakedAsync(
        string name, 
        IReadOnlyList<RecordedEvent> events, 
        string exportDir)
    {
        var filePath = Path.Combine(exportDir, $"{name}_notes.mid");
        
        await Task.Run(() =>
        {
            using var stream = File.Create(filePath);
            var writer = new MidiWriter(stream);
            
            // Write header: Format 0, 1 track, 480 ticks per beat
            writer.WriteHeader(0, 1, 480);
            
            // Collect track events
            var trackEvents = new List<(long deltaTicks, byte[] data)>();
            long lastTicks = 0;
            
            foreach (var evt in events.Where(e => 
                e.EventType == RecordedEventType.NoteOn || 
                e.EventType == RecordedEventType.NoteOff))
            {
                // Convert ms to ticks (assuming 120 BPM = 500ms per beat = 480 ticks)
                long ticks = MsToTicks(evt.TimestampMs);
                long delta = ticks - lastTicks;
                lastTicks = ticks;
                
                if (evt.EventType == RecordedEventType.NoteOn)
                {
                    // Note On: 0x90 channel, note, velocity
                    trackEvents.Add((delta, new byte[] { 0x90, (byte)evt.MidiNote, (byte)evt.Velocity }));
                }
                else
                {
                    // Note Off: 0x80 channel, note, velocity 0
                    trackEvents.Add((delta, new byte[] { 0x80, (byte)evt.MidiNote, 0 }));
                }
            }
            
            // End of track
            trackEvents.Add((0, new byte[] { 0xFF, 0x2F, 0x00 }));
            
            writer.WriteTrack(trackEvents);
        });
        
        return (filePath, "audio/midi");
    }
    
    /// <summary>
    /// MIDI export with instrument changes and effect settings as MIDI CCs.
    /// </summary>
    private async Task<(string?, string?)> ExportMidiEnhancedAsync(
        string name, 
        IReadOnlyList<RecordedEvent> events, 
        string exportDir)
    {
        var filePath = Path.Combine(exportDir, $"{name}_enhanced.mid");
        
        await Task.Run(() =>
        {
            using var stream = File.Create(filePath);
            var writer = new MidiWriter(stream);
            
            // Write header: Format 0, 1 track, 480 ticks per beat
            writer.WriteHeader(0, 1, 480);
            
            var trackEvents = new List<(long deltaTicks, byte[] data)>();
            long lastTicks = 0;
            
            foreach (var evt in events)
            {
                long ticks = MsToTicks(evt.TimestampMs);
                long delta = ticks - lastTicks;
                lastTicks = ticks;
                
                switch (evt.EventType)
                {
                    case RecordedEventType.NoteOn:
                        trackEvents.Add((delta, new byte[] { 0x90, (byte)evt.MidiNote, (byte)evt.Velocity }));
                        break;
                        
                    case RecordedEventType.NoteOff:
                        trackEvents.Add((delta, new byte[] { 0x80, (byte)evt.MidiNote, 0 }));
                        break;
                        
                    case RecordedEventType.InstrumentChange:
                        // Program Change: 0xC0 channel, program number
                        // We'll use a simple hash of instrument ID for program number
                        var program = GetProgramNumber(evt.InstrumentId);
                        trackEvents.Add((delta, new byte[] { 0xC0, program }));
                        
                        // Also add a text event with the instrument name
                        if (!string.IsNullOrEmpty(evt.InstrumentId))
                        {
                            var textBytes = System.Text.Encoding.ASCII.GetBytes(evt.InstrumentId);
                            var metaEvent = new byte[3 + textBytes.Length];
                            metaEvent[0] = 0xFF;
                            metaEvent[1] = 0x01; // Text event
                            metaEvent[2] = (byte)textBytes.Length;
                            Array.Copy(textBytes, 0, metaEvent, 3, textBytes.Length);
                            trackEvents.Add((0, metaEvent));
                        }
                        break;
                        
                    case RecordedEventType.EffectChange:
                        // Encode effect settings as CC messages
                        // This is a simplified version - real implementation would parse the JSON
                        if (!string.IsNullOrEmpty(evt.EffectData))
                        {
                            // CC 91 = Reverb, CC 93 = Chorus (General MIDI)
                            // For now, just add a marker
                            var textBytes = System.Text.Encoding.ASCII.GetBytes($"FX:{evt.EffectData}");
                            var len = Math.Min(textBytes.Length, 127);
                            var metaEvent = new byte[3 + len];
                            metaEvent[0] = 0xFF;
                            metaEvent[1] = 0x01;
                            metaEvent[2] = (byte)len;
                            Array.Copy(textBytes, 0, metaEvent, 3, len);
                            trackEvents.Add((0, metaEvent));
                        }
                        break;
                }
            }
            
            // End of track
            trackEvents.Add((0, new byte[] { 0xFF, 0x2F, 0x00 }));
            
            writer.WriteTrack(trackEvents);
        });
        
        return (filePath, "audio/midi");
    }
    
    /// <summary>
    /// MIDI export with harmony and arpeggio baked into the output.
    /// </summary>
    private async Task<(string?, string?)> ExportMidiCompleteAsync(
        Song song,
        string name, 
        IReadOnlyList<RecordedEvent> events, 
        string exportDir)
    {
        var filePath = Path.Combine(exportDir, $"{name}_complete.mid");
        
        await Task.Run(() =>
        {
            using var stream = File.Create(filePath);
            var writer = new MidiWriter(stream);
            
            // Write header: Format 0, 1 track, 480 ticks per beat
            writer.WriteHeader(0, 1, 480);
            
            var trackEvents = new List<(long deltaTicks, byte[] data)>();
            long lastTicks = 0;
            
            // TODO: In a full implementation, we would:
            // 1. Parse the initial settings for harmony/arpeggio config
            // 2. For each NoteOn, apply harmony (generate additional notes)
            // 3. For each NoteOn with arpeggio, generate the arpeggiated sequence
            // 4. Track effect changes and update processing accordingly
            
            // For now, we output all notes as-is plus any baked-in processing
            foreach (var evt in events)
            {
                long ticks = MsToTicks(evt.TimestampMs);
                long delta = ticks - lastTicks;
                lastTicks = ticks;
                
                switch (evt.EventType)
                {
                    case RecordedEventType.NoteOn:
                        // Add the primary note
                        trackEvents.Add((delta, new byte[] { 0x90, (byte)evt.MidiNote, (byte)evt.Velocity }));
                        
                        // TODO: Add harmony notes here based on active harmony settings
                        // For example, if harmony is set to "thirds", add note+4 semitones
                        break;
                        
                    case RecordedEventType.NoteOff:
                        trackEvents.Add((delta, new byte[] { 0x80, (byte)evt.MidiNote, 0 }));
                        // TODO: Add harmony note-offs
                        break;
                        
                    case RecordedEventType.InstrumentChange:
                        var program = GetProgramNumber(evt.InstrumentId);
                        trackEvents.Add((delta, new byte[] { 0xC0, program }));
                        break;
                }
            }
            
            // End of track
            trackEvents.Add((0, new byte[] { 0xFF, 0x2F, 0x00 }));
            
            writer.WriteTrack(trackEvents);
        });
        
        return (filePath, "audio/midi");
    }
    
    /// <summary>
    /// WAV export - renders the song to audio.
    /// </summary>
    private async Task<(string?, string?)> ExportWavAsync(
        Song song,
        string name, 
        IReadOnlyList<RecordedEvent> events, 
        string exportDir)
    {
        var filePath = Path.Combine(exportDir, $"{name}.wav");
        
        const int sampleRate = 44100;
        const int channels = 2;
        const int bitsPerSample = 16;
        
        // Calculate total samples needed
        var durationMs = song.DurationMs + 1000; // Add 1 second for release tails
        var totalSamples = (int)(durationMs * sampleRate / 1000);
        
        await Task.Run(async () =>
        {
            // Load the initial instrument
            if (!string.IsNullOrEmpty(song.InitialInstrumentId))
            {
                await _sfzService.LoadInstrumentAsync(song.InitialInstrumentId);
            }
            
            // Generate audio
            var audioBuffer = new float[totalSamples * channels];
            var bufferSize = 512;
            var tempBuffer = new float[bufferSize];
            
            // Create a simple playback simulation
            var eventIndex = 0;
            var samplesGenerated = 0L;
            
            while (samplesGenerated < totalSamples)
            {
                var currentTimeMs = samplesGenerated * 1000 / sampleRate;
                
                // Trigger any events that should happen at this time
                while (eventIndex < events.Count && events[eventIndex].TimestampMs <= currentTimeMs)
                {
                    var evt = events[eventIndex];
                    switch (evt.EventType)
                    {
                        case RecordedEventType.NoteOn:
                            _sfzService.NoteOn(evt.MidiNote, evt.Velocity);
                            break;
                        case RecordedEventType.NoteOff:
                            _sfzService.NoteOff(evt.MidiNote);
                            break;
                        case RecordedEventType.InstrumentChange:
                            if (!string.IsNullOrEmpty(evt.InstrumentId))
                            {
                                await _sfzService.LoadInstrumentAsync(evt.InstrumentId);
                            }
                            break;
                    }
                    eventIndex++;
                }
                
                // Generate samples
                // Note: This is a simplified version - real implementation would
                // use the SfzPlayer directly for offline rendering
                var samplesToGenerate = Math.Min(bufferSize, (int)(totalSamples - samplesGenerated));
                Array.Clear(tempBuffer, 0, tempBuffer.Length);
                
                // TODO: Call into SfzPlayer for actual sample generation
                // For now, we'll create a placeholder that works with the service
                
                // Copy to output buffer (mono to stereo)
                for (int i = 0; i < samplesToGenerate; i++)
                {
                    var outIdx = (int)(samplesGenerated + i) * channels;
                    if (outIdx + 1 < audioBuffer.Length)
                    {
                        audioBuffer[outIdx] = tempBuffer[i];
                        audioBuffer[outIdx + 1] = tempBuffer[i];
                    }
                }
                
                samplesGenerated += samplesToGenerate;
            }
            
            // Write WAV file
            using var stream = File.Create(filePath);
            WriteWavFile(stream, audioBuffer, sampleRate, channels, bitsPerSample);
        });
        
        return (filePath, "audio/wav");
    }
    
    /// <summary>
    /// FLAC export - renders the song to lossless audio.
    /// </summary>
    private async Task<(string?, string?)> ExportFlacAsync(
        Song song,
        string name, 
        IReadOnlyList<RecordedEvent> events, 
        string exportDir)
    {
        var filePath = Path.Combine(exportDir, $"{name}.flac");
        
        const int sampleRate = 44100;
        const int channels = 2;
        const int bitsPerSample = 16;
        
        // Calculate total samples needed
        var durationMs = song.DurationMs + 1000; // Add 1 second for release tails
        var totalSamples = (int)(durationMs * sampleRate / 1000);
        
        await Task.Run(async () =>
        {
            // Load the initial instrument
            if (!string.IsNullOrEmpty(song.InitialInstrumentId))
            {
                await _sfzService.LoadInstrumentAsync(song.InitialInstrumentId);
            }
            
            // Generate audio (same as WAV)
            var audioBuffer = new float[totalSamples * channels];
            var bufferSize = 512;
            var tempBuffer = new float[bufferSize];
            
            var eventIndex = 0;
            var samplesGenerated = 0L;
            
            while (samplesGenerated < totalSamples)
            {
                var currentTimeMs = samplesGenerated * 1000 / sampleRate;
                
                while (eventIndex < events.Count && events[eventIndex].TimestampMs <= currentTimeMs)
                {
                    var evt = events[eventIndex];
                    switch (evt.EventType)
                    {
                        case RecordedEventType.NoteOn:
                            _sfzService.NoteOn(evt.MidiNote, evt.Velocity);
                            break;
                        case RecordedEventType.NoteOff:
                            _sfzService.NoteOff(evt.MidiNote);
                            break;
                        case RecordedEventType.InstrumentChange:
                            if (!string.IsNullOrEmpty(evt.InstrumentId))
                            {
                                await _sfzService.LoadInstrumentAsync(evt.InstrumentId);
                            }
                            break;
                    }
                    eventIndex++;
                }
                
                var samplesToGenerate = Math.Min(bufferSize, (int)(totalSamples - samplesGenerated));
                Array.Clear(tempBuffer, 0, tempBuffer.Length);
                
                // TODO: Call into SfzPlayer for actual sample generation
                
                for (int i = 0; i < samplesToGenerate; i++)
                {
                    var outIdx = (int)(samplesGenerated + i) * channels;
                    if (outIdx + 1 < audioBuffer.Length)
                    {
                        audioBuffer[outIdx] = tempBuffer[i];
                        audioBuffer[outIdx + 1] = tempBuffer[i];
                    }
                }
                
                samplesGenerated += samplesToGenerate;
            }
            
            // Write FLAC file
            var encoder = new FlacEncoder(sampleRate, channels, bitsPerSample);
            using var stream = File.Create(filePath);
            encoder.Encode(audioBuffer, stream);
        });
        
        return (filePath, "audio/flac");
    }
    
    private static void WriteWavFile(Stream stream, float[] samples, int sampleRate, int channels, int bitsPerSample)
    {
        using var writer = new BinaryWriter(stream);
        
        var bytesPerSample = bitsPerSample / 8;
        var dataSize = samples.Length * bytesPerSample;
        var fileSize = 36 + dataSize;
        
        // RIFF header
        writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
        writer.Write(fileSize);
        writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));
        
        // fmt chunk
        writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
        writer.Write(16); // Chunk size
        writer.Write((short)1); // Audio format (PCM)
        writer.Write((short)channels);
        writer.Write(sampleRate);
        writer.Write(sampleRate * channels * bytesPerSample); // Byte rate
        writer.Write((short)(channels * bytesPerSample)); // Block align
        writer.Write((short)bitsPerSample);
        
        // data chunk
        writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
        writer.Write(dataSize);
        
        // Convert float samples to 16-bit PCM
        foreach (var sample in samples)
        {
            var clamped = Math.Clamp(sample, -1f, 1f);
            var pcm = (short)(clamped * 32767);
            writer.Write(pcm);
        }
    }
    
    private static long MsToTicks(long ms)
    {
        // At 120 BPM: 500ms per beat, 480 ticks per beat
        // So: ticks = ms * 480 / 500 = ms * 0.96
        return (long)(ms * 480.0 / 500.0);
    }
    
    private static byte GetProgramNumber(string? instrumentId)
    {
        if (string.IsNullOrEmpty(instrumentId))
            return 0;
        
        // Simple hash to get a program number 0-127
        return (byte)(Math.Abs(instrumentId.GetHashCode()) % 128);
    }
    
    private static string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return new string(name.Where(c => !invalid.Contains(c)).ToArray()).Replace(' ', '_');
    }
}

/// <summary>
/// Helper class for writing MIDI files.
/// </summary>
internal class MidiWriter
{
    private readonly Stream _stream;
    
    public MidiWriter(Stream stream)
    {
        _stream = stream;
    }
    
    public void WriteHeader(int format, int numTracks, int division)
    {
        // MThd chunk
        WriteChunk("MThd", writer =>
        {
            writer.Write(ToBigEndian((short)format));
            writer.Write(ToBigEndian((short)numTracks));
            writer.Write(ToBigEndian((short)division));
        });
    }
    
    public void WriteTrack(List<(long deltaTicks, byte[] data)> events)
    {
        using var trackData = new MemoryStream();
        using var trackWriter = new BinaryWriter(trackData);
        
        foreach (var (delta, data) in events)
        {
            WriteVariableLength(trackWriter, delta);
            trackWriter.Write(data);
        }
        
        // MTrk chunk
        var bytes = trackData.ToArray();
        WriteBytes("MTrk");
        WriteBigEndian32((int)bytes.Length);
        _stream.Write(bytes, 0, bytes.Length);
    }
    
    private void WriteChunk(string id, Action<BinaryWriter> writeContent)
    {
        using var content = new MemoryStream();
        using var writer = new BinaryWriter(content);
        writeContent(writer);
        
        var bytes = content.ToArray();
        WriteBytes(id);
        WriteBigEndian32(bytes.Length);
        _stream.Write(bytes, 0, bytes.Length);
    }
    
    private void WriteBytes(string s)
    {
        var bytes = System.Text.Encoding.ASCII.GetBytes(s);
        _stream.Write(bytes, 0, bytes.Length);
    }
    
    private void WriteBigEndian32(int value)
    {
        _stream.WriteByte((byte)((value >> 24) & 0xFF));
        _stream.WriteByte((byte)((value >> 16) & 0xFF));
        _stream.WriteByte((byte)((value >> 8) & 0xFF));
        _stream.WriteByte((byte)(value & 0xFF));
    }
    
    private static byte[] ToBigEndian(short value)
    {
        return new byte[] { (byte)((value >> 8) & 0xFF), (byte)(value & 0xFF) };
    }
    
    private static void WriteVariableLength(BinaryWriter writer, long value)
    {
        if (value < 0) value = 0;
        
        var bytes = new List<byte>();
        bytes.Add((byte)(value & 0x7F));
        value >>= 7;
        
        while (value > 0)
        {
            bytes.Add((byte)((value & 0x7F) | 0x80));
            value >>= 7;
        }
        
        bytes.Reverse();
        foreach (var b in bytes)
        {
            writer.Write(b);
        }
    }
}

