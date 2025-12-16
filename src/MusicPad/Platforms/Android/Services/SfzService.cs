using Android.Content.Res;
using Android.Media;
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
    private readonly List<string> _instrumentNames = new();
    private readonly Dictionary<string, (string folder, string sfzFile)> _instrumentPaths = new();
    
    private AudioTrack? _audioTrack;
    private CancellationTokenSource? _cts;
    private Task? _playTask;
    private SfzInstrument? _currentInstrument;

    public IReadOnlyList<string> AvailableInstruments => _instrumentNames;
    public string? CurrentInstrumentName => _currentInstrument?.Name;
    public (int minKey, int maxKey) CurrentKeyRange => _currentInstrument?.GetKeyRange() ?? (0, 127);
    
    private float _volume = 0.75f;
    public float Volume
    {
        get => _volume;
        set => _volume = Math.Clamp(value, 0f, 1f);
    }

    public SfzService()
    {
        _assets = Android.App.Application.Context.Assets!;
        _player = new SfzPlayer(SampleRate);
        
        // Discover available instruments from assets
        DiscoverInstruments();
    }

    private void DiscoverInstruments()
    {
        try
        {
            var folders = _assets.List("instruments");
            if (folders != null)
            {
                foreach (var folder in folders)
                {
                    var files = _assets.List($"instruments/{folder}");
                    if (files == null) continue;
                    
                    // Find all SFZ files in the folder
                    var sfzFiles = files
                        .Where(f => f.EndsWith(".sfz", StringComparison.OrdinalIgnoreCase))
                        .OrderBy(f => f)
                        .ToList();
                    
                    foreach (var sfzFile in sfzFiles)
                    {
                        // Create a nice display name from the SFZ filename
                        // Remove leading numbers and extension, e.g. "000_Good_flute.sfz" -> "Good flute"
                        var displayName = GetDisplayName(sfzFile);
                        
                        // Ensure unique names
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
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error discovering instruments: {ex.Message}");
        }
    }
    
    private static string GetDisplayName(string sfzFileName)
    {
        // Remove .sfz extension
        var name = Path.GetFileNameWithoutExtension(sfzFileName);
        
        // Remove leading numbers and underscores (e.g., "000_", "001_")
        while (name.Length > 0 && (char.IsDigit(name[0]) || name[0] == '_'))
        {
            name = name.Substring(1);
        }
        
        // Replace underscores with spaces
        name = name.Replace('_', ' ').Replace('-', ' ');
        
        // Trim and ensure not empty
        name = name.Trim();
        if (string.IsNullOrEmpty(name))
            name = sfzFileName;
            
        return name;
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

    public void NoteOn(int midiNote)
    {
        if (_currentInstrument == null)
            return;

        _player.NoteOn(midiNote, velocity: 100);
    }

    public void NoteOff(int midiNote)
    {
        _player.NoteOff(midiNote);
    }

    public void StopAll()
    {
        _player.StopAll();
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

