namespace MusicPad.Core.Export;

/// <summary>
/// Fallback encoder that uses custom FLAC encoder when FFmpeg is unavailable.
/// </summary>
public class FallbackAudioEncoder : IAudioEncoder
{
    private readonly IAudioEncoder _ffmpegEncoder;
    
    public FallbackAudioEncoder(IAudioEncoder ffmpegEncoder)
    {
        _ffmpegEncoder = ffmpegEncoder;
    }
    
    /// <summary>
    /// Gets whether the underlying FFmpeg encoder is available.
    /// </summary>
    public bool IsAvailable => _ffmpegEncoder.IsAvailable;
    
    /// <summary>
    /// Gets whether fallback FLAC encoding is available (custom encoder).
    /// </summary>
    public bool CanEncodeFallbackFlac => true;
    
    /// <summary>
    /// Gets whether fallback MP3 encoding is available (ShineEncoder).
    /// </summary>
    public bool CanEncodeFallbackMp3 => true;
    
    /// <summary>
    /// Encodes to MP3. Uses ShineEncoder as fallback if FFmpeg unavailable.
    /// </summary>
    public async Task<bool> EncodeToMp3Async(float[] samples, int sampleRate, int channels, int bitrate, string outputPath)
    {
        // Try FFmpeg first
        if (_ffmpegEncoder.IsAvailable)
        {
            var result = await _ffmpegEncoder.EncodeToMp3Async(samples, sampleRate, channels, bitrate, outputPath);
            if (result) return true;
        }
        
        // Fall back to ShineEncoder (pure C# MP3 encoder)
        return await Task.Run(() =>
        {
            try
            {
                var encoder = new ShineEncoder(sampleRate, channels, bitrate);
                using var stream = File.Create(outputPath);
                encoder.Encode(samples, stream);
                return true;
            }
            catch
            {
                return false;
            }
        });
    }
    
    /// <summary>
    /// Encodes to FLAC. Falls back to custom encoder if FFmpeg unavailable.
    /// </summary>
    public async Task<bool> EncodeToFlacAsync(float[] samples, int sampleRate, int channels, int bitsPerSample, string outputPath)
    {
        // Try FFmpeg first
        if (_ffmpegEncoder.IsAvailable)
        {
            var result = await _ffmpegEncoder.EncodeToFlacAsync(samples, sampleRate, channels, bitsPerSample, outputPath);
            if (result) return true;
        }
        
        // Fall back to custom encoder
        return await Task.Run(() =>
        {
            try
            {
                var encoder = new FlacEncoder(sampleRate, channels, bitsPerSample);
                using var stream = File.Create(outputPath);
                encoder.Encode(samples, stream);
                return true;
            }
            catch
            {
                return false;
            }
        });
    }
}

/// <summary>
/// Stub encoder that always fails. Used when no encoder is available.
/// </summary>
public class StubAudioEncoder : IAudioEncoder
{
    public bool IsAvailable => false;
    
    public Task<bool> EncodeToMp3Async(float[] samples, int sampleRate, int channels, int bitrate, string outputPath)
        => Task.FromResult(false);
    
    public Task<bool> EncodeToFlacAsync(float[] samples, int sampleRate, int channels, int bitsPerSample, string outputPath)
        => Task.FromResult(false);
}

