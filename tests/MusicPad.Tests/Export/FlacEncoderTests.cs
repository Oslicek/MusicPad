using MusicPad.Core.Export;
using Xunit;

namespace MusicPad.Tests.Export;

public class FlacEncoderTests
{
    private const int SampleRate = 44100;
    private const int Channels = 2;
    private const int BitsPerSample = 16;

    [Fact]
    public void Constructor_ValidParameters_CreatesEncoder()
    {
        var encoder = new FlacEncoder(SampleRate, Channels, BitsPerSample);
        
        Assert.NotNull(encoder);
        Assert.Equal(SampleRate, encoder.SampleRate);
        Assert.Equal(Channels, encoder.Channels);
        Assert.Equal(BitsPerSample, encoder.BitsPerSample);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_InvalidSampleRate_ThrowsArgumentException(int invalidRate)
    {
        Assert.Throws<ArgumentException>(() => new FlacEncoder(invalidRate, Channels, BitsPerSample));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(-1)]
    public void Constructor_InvalidChannels_ThrowsArgumentException(int invalidChannels)
    {
        Assert.Throws<ArgumentException>(() => new FlacEncoder(SampleRate, invalidChannels, BitsPerSample));
    }

    [Theory]
    [InlineData(8)]
    [InlineData(24)]
    public void Constructor_ValidBitDepths_CreatesEncoder(int bitDepth)
    {
        var encoder = new FlacEncoder(SampleRate, Channels, bitDepth);
        Assert.Equal(bitDepth, encoder.BitsPerSample);
    }

    [Fact]
    public void Encode_SilentAudio_ProducesValidFlacFile()
    {
        var encoder = new FlacEncoder(SampleRate, Channels, BitsPerSample);
        var samples = new float[SampleRate * Channels]; // 1 second of silence
        
        using var stream = new MemoryStream();
        encoder.Encode(samples, stream);
        
        var bytes = stream.ToArray();
        
        // FLAC files start with "fLaC" magic number
        Assert.True(bytes.Length >= 4);
        Assert.Equal((byte)'f', bytes[0]);
        Assert.Equal((byte)'L', bytes[1]);
        Assert.Equal((byte)'a', bytes[2]);
        Assert.Equal((byte)'C', bytes[3]);
    }

    [Fact]
    public void Encode_SineWave_ProducesValidFlacFile()
    {
        var encoder = new FlacEncoder(SampleRate, Channels, BitsPerSample);
        
        // Generate 1 second of 440Hz sine wave (stereo)
        var samples = new float[SampleRate * Channels];
        for (int i = 0; i < SampleRate; i++)
        {
            float sample = MathF.Sin(2 * MathF.PI * 440 * i / SampleRate) * 0.5f;
            samples[i * 2] = sample;     // Left
            samples[i * 2 + 1] = sample; // Right
        }
        
        using var stream = new MemoryStream();
        encoder.Encode(samples, stream);
        
        var bytes = stream.ToArray();
        
        // Should produce valid FLAC with magic number
        Assert.True(bytes.Length >= 42); // Minimum FLAC size (header + metadata)
        Assert.Equal((byte)'f', bytes[0]);
        Assert.Equal((byte)'L', bytes[1]);
        Assert.Equal((byte)'a', bytes[2]);
        Assert.Equal((byte)'C', bytes[3]);
    }

    [Fact]
    public void Encode_EmptySamples_ProducesMinimalValidFlacFile()
    {
        var encoder = new FlacEncoder(SampleRate, Channels, BitsPerSample);
        var samples = Array.Empty<float>();
        
        using var stream = new MemoryStream();
        encoder.Encode(samples, stream);
        
        var bytes = stream.ToArray();
        
        // Even empty audio should have valid FLAC header
        Assert.True(bytes.Length >= 4);
        Assert.Equal((byte)'f', bytes[0]);
        Assert.Equal((byte)'L', bytes[1]);
        Assert.Equal((byte)'a', bytes[2]);
        Assert.Equal((byte)'C', bytes[3]);
    }

    [Fact]
    public void Encode_MonoAudio_ProducesValidFlacFile()
    {
        var encoder = new FlacEncoder(SampleRate, 1, BitsPerSample);
        var samples = new float[SampleRate]; // 1 second mono
        
        // Fill with test pattern
        for (int i = 0; i < samples.Length; i++)
        {
            samples[i] = MathF.Sin(2 * MathF.PI * 440 * i / SampleRate) * 0.3f;
        }
        
        using var stream = new MemoryStream();
        encoder.Encode(samples, stream);
        
        var bytes = stream.ToArray();
        Assert.True(bytes.Length >= 4);
        Assert.Equal((byte)'f', bytes[0]);
        Assert.Equal((byte)'L', bytes[1]);
        Assert.Equal((byte)'a', bytes[2]);
        Assert.Equal((byte)'C', bytes[3]);
    }

    [Fact]
    public void Encode_24BitAudio_ProducesValidFlacFile()
    {
        var encoder = new FlacEncoder(SampleRate, Channels, 24);
        var samples = new float[SampleRate * Channels];
        
        // Fill with test pattern
        for (int i = 0; i < SampleRate; i++)
        {
            float sample = MathF.Sin(2 * MathF.PI * 440 * i / SampleRate) * 0.5f;
            samples[i * 2] = sample;
            samples[i * 2 + 1] = sample;
        }
        
        using var stream = new MemoryStream();
        encoder.Encode(samples, stream);
        
        var bytes = stream.ToArray();
        Assert.True(bytes.Length >= 4);
        Assert.Equal((byte)'f', bytes[0]);
        Assert.Equal((byte)'L', bytes[1]);
        Assert.Equal((byte)'a', bytes[2]);
        Assert.Equal((byte)'C', bytes[3]);
    }

    [Fact]
    public void Encode_ClippedSamples_ClampedToValidRange()
    {
        var encoder = new FlacEncoder(SampleRate, Channels, BitsPerSample);
        
        // Create samples that exceed -1 to 1 range
        var samples = new float[1000 * Channels];
        for (int i = 0; i < 500; i++)
        {
            samples[i * 2] = 2.0f;      // Exceeds max
            samples[i * 2 + 1] = -2.0f; // Exceeds min
        }
        
        using var stream = new MemoryStream();
        
        // Should not throw - samples should be clamped
        encoder.Encode(samples, stream);
        
        Assert.True(stream.Length > 0);
    }

    [Fact]
    public void Encode_LongAudio_ProducesValidFlacFile()
    {
        var encoder = new FlacEncoder(SampleRate, Channels, BitsPerSample);
        
        // 10 seconds of audio
        var samples = new float[SampleRate * Channels * 10];
        for (int i = 0; i < SampleRate * 10; i++)
        {
            float sample = MathF.Sin(2 * MathF.PI * 440 * i / SampleRate) * 0.3f;
            samples[i * 2] = sample;
            samples[i * 2 + 1] = sample;
        }
        
        using var stream = new MemoryStream();
        encoder.Encode(samples, stream);
        
        var bytes = stream.ToArray();
        Assert.True(bytes.Length >= 4);
        Assert.Equal((byte)'f', bytes[0]);
        Assert.Equal((byte)'L', bytes[1]);
        Assert.Equal((byte)'a', bytes[2]);
        Assert.Equal((byte)'C', bytes[3]);
        
        // Our VERBATIM encoder doesn't compress, but the file should be valid
        // Just verify it produced output of reasonable size
        Assert.True(bytes.Length > 1000, "Should produce substantial output for 10 seconds of audio");
    }

    [Fact]
    public void Encode_ToFile_CreatesValidFile()
    {
        var encoder = new FlacEncoder(SampleRate, Channels, BitsPerSample);
        var samples = new float[SampleRate * Channels];
        
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.flac");
        
        try
        {
            using (var stream = File.Create(tempPath))
            {
                encoder.Encode(samples, stream);
            }
            
            Assert.True(File.Exists(tempPath));
            var bytes = File.ReadAllBytes(tempPath);
            Assert.True(bytes.Length >= 4);
            Assert.Equal((byte)'f', bytes[0]);
            Assert.Equal((byte)'L', bytes[1]);
            Assert.Equal((byte)'a', bytes[2]);
            Assert.Equal((byte)'C', bytes[3]);
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }
}

