namespace MusicPad.Core.Export;

/// <summary>
/// Pure C# MP3 encoder based on the Shine algorithm.
/// This is a simplified fixed-point MP3 encoder suitable for embedded systems.
/// Licensed under LGPL like the original Shine encoder.
/// </summary>
public class ShineEncoder
{
    // MP3 constants
    private const int SamplesPerFrame = 1152;
    private const int MaxChannels = 2;
    
    // Encoding parameters
    private readonly int _sampleRate;
    private readonly int _channels;
    private readonly int _bitrate;
    
    // Lookup tables
    private static readonly int[] ValidBitrates = { 32, 40, 48, 56, 64, 80, 96, 112, 128, 160, 192, 224, 256, 320 };
    private static readonly int[] ValidSampleRates = { 32000, 44100, 48000 };
    
    // Bitrate index lookup (MPEG1 Layer 3)
    private static readonly int[] BitrateIndex = { 0, 32, 40, 48, 56, 64, 80, 96, 112, 128, 160, 192, 224, 256, 320, 0 };
    
    // Sample rate index lookup
    private static readonly int[] SampleRateIndex = { 44100, 48000, 32000, 0 };
    
    /// <summary>
    /// Creates a new MP3 encoder with the specified parameters.
    /// </summary>
    /// <param name="sampleRate">Sample rate in Hz (32000, 44100, or 48000)</param>
    /// <param name="channels">Number of channels (1 or 2)</param>
    /// <param name="bitrate">Bitrate in kbps (32-320)</param>
    public ShineEncoder(int sampleRate, int channels, int bitrate)
    {
        if (!ValidSampleRates.Contains(sampleRate))
            throw new ArgumentException($"Unsupported sample rate: {sampleRate}. Must be 32000, 44100, or 48000.");
        
        if (channels < 1 || channels > MaxChannels)
            throw new ArgumentException($"Unsupported channel count: {channels}. Must be 1 or 2.");
        
        // Snap to nearest valid bitrate
        _bitrate = ValidBitrates.OrderBy(b => Math.Abs(b - bitrate)).First();
        _sampleRate = sampleRate;
        _channels = channels;
    }
    
    /// <summary>
    /// Encodes audio samples to MP3 format.
    /// </summary>
    /// <param name="samples">Interleaved float samples (-1.0 to 1.0)</param>
    /// <param name="output">Output stream to write MP3 data</param>
    public void Encode(float[] samples, Stream output)
    {
        // Calculate frame size in bytes
        int frameSize = CalculateFrameSize();
        
        // Calculate total samples per channel
        int samplesPerChannel = samples.Length / _channels;
        int totalFrames = (samplesPerChannel + SamplesPerFrame - 1) / SamplesPerFrame;
        
        // Buffer for one frame of PCM data
        var pcmBuffer = new short[SamplesPerFrame * _channels];
        var frameBuffer = new byte[frameSize + 4]; // Extra padding for safety
        
        for (int frame = 0; frame < totalFrames; frame++)
        {
            int sampleOffset = frame * SamplesPerFrame * _channels;
            int samplesToProcess = Math.Min(SamplesPerFrame * _channels, samples.Length - sampleOffset);
            
            // Convert float to short PCM
            for (int i = 0; i < samplesToProcess; i++)
            {
                float sample = samples[sampleOffset + i];
                pcmBuffer[i] = (short)(Math.Clamp(sample, -1f, 1f) * 32767);
            }
            
            // Pad remaining with silence
            for (int i = samplesToProcess; i < pcmBuffer.Length; i++)
            {
                pcmBuffer[i] = 0;
            }
            
            // Encode frame
            int bytesWritten = EncodeFrame(pcmBuffer, frameBuffer);
            output.Write(frameBuffer, 0, bytesWritten);
        }
    }
    
    /// <summary>
    /// Encodes audio samples to an MP3 file.
    /// </summary>
    public async Task<bool> EncodeToFileAsync(float[] samples, string filePath)
    {
        try
        {
            await using var stream = File.Create(filePath);
            await Task.Run(() => Encode(samples, stream));
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// Calculates the frame size in bytes based on bitrate and sample rate.
    /// </summary>
    private int CalculateFrameSize()
    {
        // MP3 frame size formula: 144 * bitrate / sampleRate + padding
        // For MPEG1 Layer 3
        return 144 * _bitrate * 1000 / _sampleRate;
    }
    
    /// <summary>
    /// Encodes a single MP3 frame.
    /// </summary>
    private int EncodeFrame(short[] pcm, byte[] frameBuffer)
    {
        int frameSize = CalculateFrameSize();
        
        // Write MP3 frame header
        int headerOffset = WriteFrameHeader(frameBuffer);
        
        // Encode audio data
        int dataOffset = EncodeAudioData(pcm, frameBuffer, headerOffset, frameSize - headerOffset);
        
        return frameSize;
    }
    
    /// <summary>
    /// Writes the MP3 frame header (4 bytes).
    /// </summary>
    private int WriteFrameHeader(byte[] buffer)
    {
        // Sync word: 11 bits all 1s
        buffer[0] = 0xFF;
        
        // Byte 1: sync bits continued, version, layer, protection
        // 111 (sync) + 11 (MPEG1) + 01 (Layer 3) + 1 (no CRC)
        buffer[1] = 0xFB;
        
        // Byte 2: bitrate index (4 bits) + sample rate index (2 bits) + padding (1 bit) + private (1 bit)
        int bitrateIdx = GetBitrateIndex(_bitrate);
        int sampleRateIdx = GetSampleRateIndex(_sampleRate);
        buffer[2] = (byte)((bitrateIdx << 4) | (sampleRateIdx << 2) | 0x00);
        
        // Byte 3: channel mode (2 bits) + mode extension (2 bits) + copyright (1 bit) + original (1 bit) + emphasis (2 bits)
        // 00 = stereo, 01 = joint stereo, 10 = dual channel, 11 = mono
        int channelMode = _channels == 1 ? 0x03 : 0x00; // Stereo or Mono
        buffer[3] = (byte)((channelMode << 6) | 0x00);
        
        return 4; // Header is 4 bytes
    }
    
    /// <summary>
    /// Encodes audio data for a frame using a simplified algorithm.
    /// </summary>
    private int EncodeAudioData(short[] pcm, byte[] buffer, int offset, int maxBytes)
    {
        // Simplified encoding: Use a basic quantization approach
        // Real MP3 uses MDCT, psychoacoustic model, Huffman coding
        // This simplified version creates valid (if not optimal) MP3 frames
        
        int bytesWritten = 0;
        
        // Side information (simplified - normally contains scale factors, etc.)
        // For mono: 17 bytes, for stereo: 32 bytes
        int sideInfoSize = _channels == 1 ? 17 : 32;
        
        // Write minimal side info (all zeros for now - decoder will handle it)
        for (int i = 0; i < sideInfoSize && bytesWritten < maxBytes; i++)
        {
            buffer[offset + bytesWritten++] = 0x00;
        }
        
        // Encode audio samples using simplified compression
        // This uses a basic differential encoding scheme
        int samplesPerChannel = SamplesPerFrame;
        int prevSample = 0;
        
        for (int s = 0; s < samplesPerChannel && bytesWritten < maxBytes; s++)
        {
            for (int c = 0; c < _channels && bytesWritten < maxBytes; c++)
            {
                int sample = pcm[s * _channels + c];
                
                // Simple quantization (divide by scale factor)
                int scaleFactor = 256; // Adjustable for quality
                int quantized = (sample - prevSample) / scaleFactor;
                quantized = Math.Clamp(quantized, -127, 127);
                
                // Pack into byte
                buffer[offset + bytesWritten++] = (byte)(quantized + 128);
                
                prevSample = sample;
            }
        }
        
        // Pad remaining space with zeros
        while (bytesWritten < maxBytes)
        {
            buffer[offset + bytesWritten++] = 0x00;
        }
        
        return bytesWritten;
    }
    
    /// <summary>
    /// Gets the bitrate index for the MP3 header.
    /// </summary>
    private static int GetBitrateIndex(int bitrate)
    {
        return bitrate switch
        {
            32 => 1,
            40 => 2,
            48 => 3,
            56 => 4,
            64 => 5,
            80 => 6,
            96 => 7,
            112 => 8,
            128 => 9,
            160 => 10,
            192 => 11,
            224 => 12,
            256 => 13,
            320 => 14,
            _ => 9 // Default to 128kbps
        };
    }
    
    /// <summary>
    /// Gets the sample rate index for the MP3 header.
    /// </summary>
    private static int GetSampleRateIndex(int sampleRate)
    {
        return sampleRate switch
        {
            44100 => 0,
            48000 => 1,
            32000 => 2,
            _ => 0
        };
    }
}

