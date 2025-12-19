using System.Text.Json;
using Android.Content.Res;
using Android.Media;
using MusicPad.Core.Audio;
using MusicPad.Core.Models;
using MusicPad.Core.Sfz;

namespace MusicPad.Services;

/// <summary>
/// Android implementation of SFZ instrument service.
/// </summary>
public class SfzService : ISfzService, IDisposable
{
    private const int SampleRate = 44100;
    
    private readonly AssetManager _assets;
    private readonly SfzPlayer _player;
    private readonly LowPassFilter _lpf;
    private readonly Equalizer _eq;
    private readonly Chorus _chorus;
    private readonly Delay _delay;
    private readonly Reverb _reverb;
    private readonly List<string> _instrumentNames = new();
    private readonly Dictionary<string, (string folder, string sfzFile)> _instrumentPaths = new();
    
    private AudioTrack? _audioTrack;
    private CancellationTokenSource? _cts;
    private Task? _playTask;
    private SfzInstrument? _currentInstrument;

    public IReadOnlyList<string> AvailableInstruments => _instrumentNames;
    public string? CurrentInstrumentName => _currentInstrument?.Name;
    public (int minKey, int maxKey) CurrentKeyRange => _currentInstrument?.GetKeyRange() ?? (0, 127);
    public SfzInstrument? CurrentInstrument => _currentInstrument;
    
    private float _volume = 0.75f;
    public float Volume
    {
        get => _volume;
        set => _volume = Math.Clamp(value, 0f, 1f);
    }

    public bool LpfEnabled
    {
        get => _lpf.IsEnabled;
        set => _lpf.IsEnabled = value;
    }
    
    public float LpfCutoff
    {
        get => _lpf.Cutoff;
        set => _lpf.Cutoff = value;
    }
    
    public float LpfResonance
    {
        get => _lpf.Resonance;
        set => _lpf.Resonance = value;
    }

    public SfzService()
    {
        _assets = Android.App.Application.Context.Assets!;
        _player = new SfzPlayer(SampleRate);
        _lpf = new LowPassFilter(SampleRate);
        _eq = new Equalizer(SampleRate);
        _chorus = new Chorus(SampleRate);
        _delay = new Delay(SampleRate);
        _reverb = new Reverb(SampleRate);
        
        // Discover available instruments from assets
        DiscoverInstruments();
    }
    
    public void SetEqBandGain(int band, float normalizedGain)
    {
        _eq.SetGain(band, normalizedGain);
    }
    
    public bool ChorusEnabled
    {
        get => _chorus.IsEnabled;
        set => _chorus.IsEnabled = value;
    }
    
    public float ChorusDepth
    {
        get => _chorus.Depth;
        set => _chorus.Depth = value;
    }
    
    public float ChorusRate
    {
        get => _chorus.Rate;
        set => _chorus.Rate = value;
    }
    
    public bool DelayEnabled
    {
        get => _delay.IsEnabled;
        set => _delay.IsEnabled = value;
    }
    
    public float DelayTime
    {
        get => _delay.Time;
        set => _delay.Time = value;
    }
    
    public float DelayFeedback
    {
        get => _delay.Feedback;
        set => _delay.Feedback = value;
    }
    
    public float DelayLevel
    {
        get => _delay.Level;
        set => _delay.Level = value;
    }
    
    public bool ReverbEnabled
    {
        get => _reverb.IsEnabled;
        set => _reverb.IsEnabled = value;
    }
    
    public float ReverbLevel
    {
        get => _reverb.Level;
        set => _reverb.Level = value;
    }
    
    public ReverbType ReverbType
    {
        get => _reverb.Type;
        set => _reverb.Type = value;
    }

    private void DiscoverInstruments()
    {
        try
        {
            // Try to load instruments from config file first
            if (LoadInstrumentsFromConfig())
                return;
            
            // Fallback: auto-discover from folders
            DiscoverInstrumentsFromFolders();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error discovering instruments: {ex.Message}");
        }
    }

    private bool LoadInstrumentsFromConfig()
    {
        try
        {
            using var stream = _assets.Open("instruments/instruments.json");
            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();
            
            var config = JsonSerializer.Deserialize<InstrumentsConfig>(json);
            if (config?.Instruments == null || config.Instruments.Count == 0)
                return false;
            
            foreach (var entry in config.Instruments)
            {
                // Verify the instrument file exists
                var files = _assets.List($"instruments/{entry.Folder}");
                if (files == null || !files.Contains(entry.SfzFile))
                {
                    System.Diagnostics.Debug.WriteLine($"Instrument not found: {entry.Folder}/{entry.SfzFile}");
                    continue;
                }
                
                _instrumentNames.Add(entry.DisplayName);
                _instrumentPaths[entry.DisplayName] = (entry.Folder, entry.SfzFile);
            }
            
            return _instrumentNames.Count > 0;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading instruments config: {ex.Message}");
            return false;
        }
    }

    private void DiscoverInstrumentsFromFolders()
    {
        var folders = _assets.List("instruments");
        if (folders == null) return;
        
        foreach (var folder in folders)
        {
            if (folder.EndsWith(".json")) continue; // Skip config file
            
            var files = _assets.List($"instruments/{folder}");
            if (files == null) continue;
            
            var sfzFiles = files
                .Where(f => f.EndsWith(".sfz", StringComparison.OrdinalIgnoreCase))
                .OrderBy(f => f)
                .ToList();
            
            foreach (var sfzFile in sfzFiles)
            {
                var displayName = GetDisplayName(sfzFile);
                
                var uniqueName = displayName;
                int counter = 2;
                while (_instrumentNames.Contains(uniqueName))
                {
                    uniqueName = $"{displayName} ({counter++})";
                }
                
                _instrumentNames.Add(uniqueName);
                _instrumentPaths[uniqueName] = (folder, sfzFile);
            }
        }
    }
    
    private static string GetDisplayName(string sfzFileName)
    {
        var name = Path.GetFileNameWithoutExtension(sfzFileName);
        
        while (name.Length > 0 && (char.IsDigit(name[0]) || name[0] == '_'))
        {
            name = name.Substring(1);
        }
        
        name = name.Replace('_', ' ').Replace('-', ' ').Trim();
        
        return string.IsNullOrEmpty(name) ? sfzFileName : name;
    }

    public async Task LoadInstrumentAsync(string instrumentName)
    {
        try
        {
            // Stop any current playback
            StopAll();
            
            // Look up the folder and SFZ file for this instrument
            if (!_instrumentPaths.TryGetValue(instrumentName, out var pathInfo))
                throw new FileNotFoundException($"Instrument not found: {instrumentName}");
            
            var (folder, sfzFile) = pathInfo;
            var basePath = $"instruments/{folder}";

            // Read and parse SFZ file
            var sfzPath = $"{basePath}/{sfzFile}";
            string sfzContent;
            using (var stream = _assets.Open(sfzPath))
            using (var reader = new StreamReader(stream))
            {
                sfzContent = await reader.ReadToEndAsync();
            }

            var instrument = SfzParser.Parse(sfzContent, instrumentName, basePath);

            // Load the sample WAV file
            var samplePath = instrument.DefaultSample;
            if (!string.IsNullOrEmpty(samplePath))
            {
                var fullSamplePath = $"{basePath}/{samplePath}";
                using var sampleStream = _assets.Open(fullSamplePath);
                using var ms = new MemoryStream();
                await sampleStream.CopyToAsync(ms);
                
                var wavData = WavLoader.LoadSamples(ms.ToArray());
                instrument.LoadedSamples[instrument.GetSamplePath(instrument.Regions.FirstOrDefault() ?? new SfzRegion())] = wavData;
                
                // Store samples directly for all regions (they all use the same file with offsets)
                foreach (var region in instrument.Regions)
                {
                    var regionSamplePath = instrument.GetSamplePath(region);
                    if (!instrument.LoadedSamples.ContainsKey(regionSamplePath))
                    {
                        instrument.LoadedSamples[regionSamplePath] = wavData;
                    }
                }
            }

            _currentInstrument = instrument;
            _player.LoadInstrument(instrument);
            
            // Ensure audio playback thread is running
            EnsurePlaybackThread();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading instrument: {ex.Message}");
            throw;
        }
    }

    public void NoteOn(int midiNote) => NoteOn(midiNote, 100);
    
    public void NoteOn(int midiNote, int velocity)
    {
        if (_currentInstrument == null)
            return;

        _player.NoteOn(midiNote, velocity: velocity);
    }

    public void NoteOff(int midiNote)
    {
        _player.NoteOff(midiNote);
    }

    public void StopAll()
    {
        _player.StopAll();
    }
    
    public float GetNoteEnvelopeLevel(int midiNote)
    {
        return _player.GetEnvelopeLevel(midiNote);
    }

    private void EnsurePlaybackThread()
    {
        if (_playTask != null && !_playTask.IsCompleted)
            return;

        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        var minBufferSize = AudioTrack.GetMinBufferSize(
            SampleRate,
            ChannelOut.Mono,
            Encoding.PcmFloat);

        _audioTrack?.Release();
        _audioTrack = new AudioTrack.Builder()
            .SetAudioAttributes(new AudioAttributes.Builder()
                .SetUsage(AudioUsageKind.Media)!
                .SetContentType(AudioContentType.Music)!
                .Build()!)
            .SetAudioFormat(new AudioFormat.Builder()
                .SetEncoding(Encoding.PcmFloat)!
                .SetSampleRate(SampleRate)!
                .SetChannelMask(ChannelOut.Mono)!
                .Build()!)
            .SetBufferSizeInBytes(minBufferSize)
            .SetTransferMode(AudioTrackMode.Stream)
            .Build();

        _audioTrack.Play();

        _playTask = Task.Run(() => PlaybackLoop(minBufferSize / sizeof(float), token), token);
    }

    private void PlaybackLoop(int bufferSamples, CancellationToken token)
    {
        var buffer = new float[bufferSamples];

        while (!token.IsCancellationRequested)
        {
            _player.GenerateSamples(buffer);

            // Apply effects in order: LPF -> EQ -> Chorus -> Delay -> Reverb
            _lpf.Process(buffer);
            _eq.Process(buffer);
            _chorus.Process(buffer);
            _delay.Process(buffer);
            _reverb.Process(buffer);

            // Apply volume and soft limiting
            float vol = _volume;
            for (int i = 0; i < buffer.Length; i++)
            {
                float s = buffer[i] * vol;
                buffer[i] = (float)Math.Tanh(s);
            }

            try
            {
                _audioTrack?.Write(buffer, 0, bufferSamples, WriteMode.Blocking);
            }
            catch
            {
                break;
            }
        }
    }

    public void Dispose()
    {
        _cts?.Cancel();

        try
        {
            _playTask?.Wait(100);
        }
        catch { }

        _audioTrack?.Stop();
        _audioTrack?.Release();
        _audioTrack?.Dispose();
        _audioTrack = null;

        _cts?.Dispose();
        _cts = null;
        _playTask = null;
    }
}

