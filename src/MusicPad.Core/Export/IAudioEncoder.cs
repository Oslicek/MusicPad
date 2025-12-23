namespace MusicPad.Core.Export;

/// <summary>
/// Interface for encoding audio to various formats.
/// </summary>
public interface IAudioEncoder
{
    /// <summary>
    /// Encodes audio samples to MP3 format.
    /// </summary>
    /// <param name="samples">Interleaved float samples (-1.0 to 1.0)</param>
    /// <param name="sampleRate">Sample rate in Hz (e.g., 44100)</param>
    /// <param name="channels">Number of channels (1 or 2)</param>
    /// <param name="bitrate">Bitrate in kbps (e.g., 192)</param>
    /// <param name="outputPath">Path to write the MP3 file</param>
    /// <returns>True if encoding succeeded, false otherwise</returns>
    Task<bool> EncodeToMp3Async(float[] samples, int sampleRate, int channels, int bitrate, string outputPath);
    
    /// <summary>
    /// Encodes audio samples to FLAC format.
    /// </summary>
    /// <param name="samples">Interleaved float samples (-1.0 to 1.0)</param>
    /// <param name="sampleRate">Sample rate in Hz (e.g., 44100)</param>
    /// <param name="channels">Number of channels (1 or 2)</param>
    /// <param name="bitsPerSample">Bits per sample (16 or 24)</param>
    /// <param name="outputPath">Path to write the FLAC file</param>
    /// <returns>True if encoding succeeded, false otherwise</returns>
    Task<bool> EncodeToFlacAsync(float[] samples, int sampleRate, int channels, int bitsPerSample, string outputPath);
    
    /// <summary>
    /// Gets whether FFmpeg is available on this platform.
    /// </summary>
    bool IsAvailable { get; }
}






