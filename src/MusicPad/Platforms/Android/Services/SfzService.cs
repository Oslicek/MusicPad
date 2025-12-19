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
    private readonly HashSet<string> _userInstrumentFolders = new();
    
    private AudioTrack? _audioTrack;
    private CancellationTokenSource? _cts;
    private Task? _playTask;
    private SfzInstrument? _currentInstrument;

    public IReadOnlyList<string> AvailableInstruments => _instrumentNames;
    public string? CurrentInstrumentName => _currentInstrument?.Name;
    public (int minKey, int maxKey) CurrentKeyRange => _currentInstrument?.GetKeyRange() ?? (0, 127);
    public SfzInstrument? CurrentInstrument => _currentInstrument;
    
    public VoicingType VoicingMode
    {
        get => _player.VoicingMode;
        set => _player.VoicingMode = value;
    }
    
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

    public void RefreshInstruments()
    {
        _instrumentNames.Clear();
        _instrumentPaths.Clear();
        _userInstrumentFolders.Clear();
        DiscoverInstruments();
    }
    
    private void DiscoverInstruments()
    {
        try
        {
            var userInstrumentsPath = Path.Combine(FileSystem.AppDataDirectory, "instruments");
            
            // First, load all available instruments into temp dictionaries
            var userConfigs = LoadUserInstrumentConfigs(userInstrumentsPath);
            var bundledConfigs = LoadBundledInstrumentConfigs();
            
            // Check for unified order
            var unifiedOrderPath = Path.Combine(userInstrumentsPath, "unified-order.json");
            if (File.Exists(unifiedOrderPath))
            {
                try
                {
                    var orderJson = File.ReadAllText(unifiedOrderPath);
                    var orderConfig = JsonSerializer.Deserialize<InstrumentOrderConfig>(orderJson);
                    if (orderConfig?.Order != null && orderConfig.Order.Count > 0)
                    {
                        // Use unified order
                        var allConfigs = new Dictionary<string, (InstrumentConfig config, bool isUser)>();
                        foreach (var kvp in userConfigs)
                            allConfigs[kvp.Key] = (kvp.Value, true);
                        foreach (var kvp in bundledConfigs)
                            if (!allConfigs.ContainsKey(kvp.Key))
                                allConfigs[kvp.Key] = (kvp.Value, false);
                        
                        var remaining = new HashSet<string>(allConfigs.Keys);
                        
                        // Add in order
                        foreach (var fileName in orderConfig.Order)
                        {
                            if (allConfigs.TryGetValue(fileName, out var tuple))
                            {
                                AddInstrumentFromConfig(tuple.config, tuple.isUser);
                                remaining.Remove(fileName);
                            }
                        }
                        
                        // Add remaining
                        foreach (var fileName in remaining.OrderBy(f => f))
                        {
                            var tuple = allConfigs[fileName];
                            AddInstrumentFromConfig(tuple.config, tuple.isUser);
                        }
                        
                        return;
                    }
                }
                catch { }
            }
            
            // Default: user instruments first, then bundled
            foreach (var kvp in userConfigs)
                AddInstrumentFromConfig(kvp.Value, isUser: true);
            foreach (var kvp in bundledConfigs)
                AddInstrumentFromConfig(kvp.Value, isUser: false);
            
            // Fallback: if no instruments found, auto-discover from folders
            if (_instrumentNames.Count == 0)
            {
                DiscoverInstrumentsFromFolders();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error discovering instruments: {ex.Message}");
        }
    }
    
    private Dictionary<string, InstrumentConfig> LoadUserInstrumentConfigs(string userInstrumentsPath)
    {
        var result = new Dictionary<string, InstrumentConfig>();
        
        if (!Directory.Exists(userInstrumentsPath))
            return result;
        
        var excludedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "user-instrument-order.json",
            "bundled-order-override.json",
            "unified-order.json"
        };
        
        var configFiles = Directory.GetFiles(userInstrumentsPath, "*.json")
            .Select(Path.GetFileName)
            .Where(f => f != null && !excludedFiles.Contains(f))
            .Cast<string>();
        
        foreach (var configFile in configFiles)
        {
            try
            {
                var configPath = Path.Combine(userInstrumentsPath, configFile);
                var configJson = File.ReadAllText(configPath);
                var config = JsonSerializer.Deserialize<InstrumentConfig>(configJson);
                
                if (config != null && !string.IsNullOrEmpty(config.SfzPath))
                {
                    config.FileName = configFile; // Set actual filename
                    result[configFile] = config;
                }
            }
            catch { }
        }
        
        return result;
    }
    
    private Dictionary<string, InstrumentConfig> LoadBundledInstrumentConfigs()
    {
        var result = new Dictionary<string, InstrumentConfig>();
        
        try
        {
            using var orderStream = _assets.Open("instruments/instrument-order.json");
            using var orderReader = new StreamReader(orderStream);
            var orderJson = orderReader.ReadToEnd();
            
            var orderConfig = JsonSerializer.Deserialize<InstrumentOrderConfig>(orderJson);
            if (orderConfig?.Order == null)
                return result;
            
            foreach (var configFileName in orderConfig.Order)
            {
                try
                {
                    using var configStream = _assets.Open($"instruments/{configFileName}");
                    using var configReader = new StreamReader(configStream);
                    var configJson = configReader.ReadToEnd();
                    
                    var config = JsonSerializer.Deserialize<InstrumentConfig>(configJson);
                    if (config != null && !string.IsNullOrEmpty(config.SfzPath))
                    {
                        config.FileName = configFileName; // Set actual filename
                        result[configFileName] = config;
                    }
                }
                catch { }
            }
        }
        catch { }
        
        return result;
    }
    
    private void AddInstrumentFromConfig(InstrumentConfig config, bool isUser)
    {
        var pathParts = config.SfzPath.Split('/');
        if (pathParts.Length != 2)
            return;
        
        var folder = pathParts[0];
        var sfzFile = pathParts[1];
        
        if (!isUser)
        {
            // Verify bundled instrument exists
            var files = _assets.List($"instruments/{folder}");
            if (files == null || !files.Contains(sfzFile))
                return;
        }
        
        _instrumentNames.Add(config.DisplayName);
        _instrumentPaths[config.DisplayName] = (folder, sfzFile);
        
        if (isUser)
        {
            _userInstrumentFolders.Add(config.DisplayName);
        }
    }
    
    /// <summary>
    /// Helper class for instrument order config.
    /// </summary>
    private class InstrumentOrderConfig
    {
        [System.Text.Json.Serialization.JsonPropertyName("version")]
        public int Version { get; set; }
        
        [System.Text.Json.Serialization.JsonPropertyName("order")]
        public List<string>? Order { get; set; }
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
            var isUserInstrument = _userInstrumentFolders.Contains(instrumentName);
            
            string sfzContent;
            string basePath;
            
            if (isUserInstrument)
            {
                // Load from user storage
                var userInstrumentsPath = Path.Combine(FileSystem.AppDataDirectory, "instruments", folder);
                var sfzPath = Path.Combine(userInstrumentsPath, sfzFile);
                sfzContent = await File.ReadAllTextAsync(sfzPath);
                basePath = userInstrumentsPath;
            }
            else
            {
                // Load from assets
                basePath = $"instruments/{folder}";
                var sfzPath = $"{basePath}/{sfzFile}";
                using var stream = _assets.Open(sfzPath);
                using var reader = new StreamReader(stream);
                sfzContent = await reader.ReadToEndAsync();
            }

            var instrument = SfzParser.Parse(sfzContent, instrumentName, basePath);

            // Load the sample WAV file
            var samplePath = instrument.DefaultSample;
            if (!string.IsNullOrEmpty(samplePath))
            {
                byte[] wavBytes;
                
                if (isUserInstrument)
                {
                    var fullSamplePath = Path.Combine(basePath, samplePath);
                    wavBytes = await File.ReadAllBytesAsync(fullSamplePath);
                }
                else
                {
                    var fullSamplePath = $"{basePath}/{samplePath}";
                    using var sampleStream = _assets.Open(fullSamplePath);
                    using var ms = new MemoryStream();
                    await sampleStream.CopyToAsync(ms);
                    wavBytes = ms.ToArray();
                }
                
                var wavData = WavLoader.LoadSamples(wavBytes);
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
    
    public void Mute()
    {
        _player.Mute();
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

