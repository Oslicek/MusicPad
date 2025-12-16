using MusicPad.Core.Sfz;

namespace MusicPad.Tests.Sfz;

public class WavLoaderTests
{
    [Fact]
    public void LoadSamples_ValidWav_ReturnsFloatArray()
    {
        // Create a minimal valid WAV file in memory
        var wavData = CreateTestWavFile(sampleRate: 44100, channels: 1, samples: new float[] { 0.5f, -0.5f, 0.25f });
        
        var result = WavLoader.LoadSamples(wavData);
        
        Assert.NotNull(result.Samples);
        Assert.True(result.Samples.Length > 0);
    }

    [Fact]
    public void LoadSamples_ReturnsSampleRate()
    {
        var wavData = CreateTestWavFile(sampleRate: 48000, channels: 1, samples: new float[] { 0.5f });
        
        var result = WavLoader.LoadSamples(wavData);
        
        Assert.Equal(48000, result.SampleRate);
    }

    [Fact]
    public void LoadSamples_ReturnsChannelCount()
    {
        var wavData = CreateTestWavFile(sampleRate: 44100, channels: 2, samples: new float[] { 0.5f, 0.5f });
        
        var result = WavLoader.LoadSamples(wavData);
        
        Assert.Equal(2, result.Channels);
    }

    [Fact]
    public void LoadSamples_16BitWav_ConvertsToFloat()
    {
        // 16-bit samples: max positive = 0.999..., max negative = -1.0
        var wavData = CreateTestWavFile(sampleRate: 44100, channels: 1, samples: new float[] { 1.0f, -1.0f, 0.0f });
        
        var result = WavLoader.LoadSamples(wavData);
        
        Assert.Equal(3, result.Samples.Length);
        Assert.True(result.Samples[0] > 0.9f); // Near 1.0
        Assert.True(result.Samples[1] < -0.9f); // Near -1.0
        Assert.True(Math.Abs(result.Samples[2]) < 0.01f); // Near 0
    }

    [Fact]
    public void LoadSamplesSlice_ReturnsOnlyRequestedRange()
    {
        var wavData = CreateTestWavFile(sampleRate: 44100, channels: 1, 
            samples: new float[] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f });
        
        // Load only samples 1-3 (offset=1, end=3)
        var result = WavLoader.LoadSamplesSlice(wavData, offset: 1, end: 3);
        
        Assert.Equal(3, result.Samples.Length); // samples at index 1, 2, 3
    }

    [Fact]
    public void LoadSamplesSlice_StereoFile_ReturnsCorrectSlice()
    {
        // Stereo: L, R, L, R, L, R = 3 sample frames
        var wavData = CreateTestWavFile(sampleRate: 44100, channels: 2, 
            samples: new float[] { 0.1f, -0.1f, 0.2f, -0.2f, 0.3f, -0.3f });
        
        // Offset/end are in sample frames for SFZ
        var result = WavLoader.LoadSamplesSlice(wavData, offset: 1, end: 2);
        
        // Should get 2 frames = 4 samples (L, R, L, R)
        Assert.Equal(4, result.Samples.Length);
    }

    /// <summary>
    /// Creates a minimal valid 16-bit WAV file for testing.
    /// </summary>
    private static byte[] CreateTestWavFile(int sampleRate, int channels, float[] samples)
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);
        
        int bitsPerSample = 16;
        int byteRate = sampleRate * channels * bitsPerSample / 8;
        int blockAlign = channels * bitsPerSample / 8;
        int dataSize = samples.Length * 2; // 16-bit = 2 bytes per sample
        
        // RIFF header
        writer.Write("RIFF"u8);
        writer.Write(36 + dataSize); // File size - 8
        writer.Write("WAVE"u8);
        
        // fmt chunk
        writer.Write("fmt "u8);
        writer.Write(16); // Chunk size
        writer.Write((short)1); // Audio format (PCM)
        writer.Write((short)channels);
        writer.Write(sampleRate);
        writer.Write(byteRate);
        writer.Write((short)blockAlign);
        writer.Write((short)bitsPerSample);
        
        // data chunk
        writer.Write("data"u8);
        writer.Write(dataSize);
        
        // Write samples as 16-bit PCM
        foreach (var sample in samples)
        {
            var clamped = Math.Clamp(sample, -1f, 1f);
            var int16Value = (short)(clamped * 32767);
            writer.Write(int16Value);
        }
        
        return ms.ToArray();
    }
}

