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
    private const float MasterGain = 0.5f;
    
    private readonly AssetManager _assets;
    private readonly SfzPlayer _player;
    private readonly List<string> _instrumentNames = new();
    
    private AudioTrack? _audioTrack;
    private CancellationTokenSource? _cts;
    private Task? _playTask;
    private SfzInstrument? _currentInstrument;
    private int _currentNote;
    private bool _isPlaying;

    public IReadOnlyList<string> AvailableInstruments => _instrumentNames;
    public string? CurrentInstrumentName => _currentInstrument?.Name;

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
                    // Check if folder contains an SFZ file
                    var files = _assets.List($"instruments/{folder}");
                    if (files != null && files.Any(f => f.EndsWith(".sfz", StringComparison.OrdinalIgnoreCase)))
                    {
                        _instrumentNames.Add(folder);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error discovering instruments: {ex.Message}");
        }
    }

    public async Task LoadInstrumentAsync(string instrumentName)
    {
        try
        {
            // Stop any current playback
            StopNote();
            
            // Find SFZ file in the instrument folder
            var files = _assets.List($"instruments/{instrumentName}");
            var sfzFile = files?.FirstOrDefault(f => f.EndsWith(".sfz", StringComparison.OrdinalIgnoreCase));
            
            if (sfzFile == null)
                throw new FileNotFoundException($"No SFZ file found in instrument folder: {instrumentName}");

            // Read and parse SFZ file
            var sfzPath = $"instruments/{instrumentName}/{sfzFile}";
            string sfzContent;
            using (var stream = _assets.Open(sfzPath))
            using (var reader = new StreamReader(stream))
            {
                sfzContent = await reader.ReadToEndAsync();
            }

            var instrument = SfzParser.Parse(sfzContent, instrumentName, $"instruments/{instrumentName}");

            // Load the sample WAV file
            var samplePath = instrument.DefaultSample;
            if (!string.IsNullOrEmpty(samplePath))
            {
                var fullSamplePath = $"instruments/{instrumentName}/{samplePath}";
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

    public void PlayNote()
    {
        if (_currentInstrument == null)
            return;

        _currentNote = _currentInstrument.GetMiddleKey();
        _player.NoteOn(_currentNote, velocity: 100);
        _isPlaying = true;
    }

    public void StopNote()
    {
        if (_isPlaying)
        {
            _player.NoteOff(_currentNote);
            _isPlaying = false;
        }
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

            // Apply master gain and soft limiting
            for (int i = 0; i < buffer.Length; i++)
            {
                float s = buffer[i] * MasterGain;
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

