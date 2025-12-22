using MusicPad.Core.Export;
using Xunit;

namespace MusicPad.Tests.Export;

/// <summary>
/// Tests for the ShineEncoder (pure C# MP3 encoder).
/// </summary>
public class Mp3EncoderTests
{
    [Fact]
    public void Constructor_WithValidParameters_Succeeds()
    {
        // Arrange & Act
        var encoder = new ShineEncoder(44100, 2, 128);
        
        // Assert
        Assert.NotNull(encoder);
    }
    
    [Theory]
    [InlineData(44100)]
    [InlineData(48000)]
    [InlineData(32000)]
    public void Constructor_WithSupportedSampleRates_Succeeds(int sampleRate)
    {
        // Arrange & Act
        var encoder = new ShineEncoder(sampleRate, 2, 128);
        
        // Assert
        Assert.NotNull(encoder);
    }
    
    [Fact]
    public void Encode_SilentAudio_ProducesValidMp3()
    {
        // Arrange
        var encoder = new ShineEncoder(44100, 2, 128);
        var samples = new float[44100 * 2]; // 1 second stereo silence
        
        // Act
        using var output = new MemoryStream();
        encoder.Encode(samples, output);
        
        // Assert
        Assert.True(output.Length > 0, "Output should not be empty");
        
        // Check MP3 sync word (frame starts with 0xFF 0xFB for MPEG1 Layer 3)
        output.Position = 0;
        var firstByte = output.ReadByte();
        Assert.Equal(0xFF, firstByte);
    }
    
    [Fact]
    public void Encode_SineWave_ProducesValidMp3()
    {
        // Arrange
        var encoder = new ShineEncoder(44100, 2, 128);
        var samples = GenerateSineWave(440, 44100, 1.0f, 2);
        
        // Act
        using var output = new MemoryStream();
        encoder.Encode(samples, output);
        
        // Assert
        Assert.True(output.Length > 0, "Output should not be empty");
        
        // Minimum size check: 128kbps for 1 second = ~16KB
        Assert.True(output.Length > 10000, $"Output too small: {output.Length} bytes");
    }
    
    [Fact]
    public void Encode_MonoAudio_ProducesValidMp3()
    {
        // Arrange
        var encoder = new ShineEncoder(44100, 1, 64);
        var samples = GenerateSineWave(440, 44100, 0.5f, 1);
        
        // Act
        using var output = new MemoryStream();
        encoder.Encode(samples, output);
        
        // Assert
        Assert.True(output.Length > 0, "Output should not be empty");
    }
    
    [Fact]
    public void Encode_ShortAudio_ProducesValidMp3()
    {
        // Arrange: 100ms of audio
        var encoder = new ShineEncoder(44100, 2, 128);
        var samples = GenerateSineWave(440, 44100, 0.1f, 2);
        
        // Act
        using var output = new MemoryStream();
        encoder.Encode(samples, output);
        
        // Assert
        Assert.True(output.Length > 0, "Output should not be empty");
    }
    
    [Fact]
    public void Encode_HighBitrate_ProducesLargerFile()
    {
        // Arrange
        var lowBitrateEncoder = new ShineEncoder(44100, 2, 64);
        var highBitrateEncoder = new ShineEncoder(44100, 2, 320);
        var samples = GenerateSineWave(440, 44100, 1.0f, 2);
        
        // Act
        using var lowOutput = new MemoryStream();
        using var highOutput = new MemoryStream();
        lowBitrateEncoder.Encode(samples, lowOutput);
        highBitrateEncoder.Encode(samples, highOutput);
        
        // Assert
        Assert.True(highOutput.Length > lowOutput.Length, 
            $"High bitrate ({highOutput.Length}) should be larger than low bitrate ({lowOutput.Length})");
    }
    
    [Fact]
    public void Encode_MultipleFrames_ProducesConsistentOutput()
    {
        // Arrange
        var encoder = new ShineEncoder(44100, 2, 128);
        var samples = GenerateSineWave(440, 44100, 2.0f, 2); // 2 seconds
        
        // Act
        using var output = new MemoryStream();
        encoder.Encode(samples, output);
        
        // Assert: Should have multiple frames
        // At 128kbps, ~1152 samples per frame, so 2 seconds = ~76 frames
        Assert.True(output.Length > 20000, $"Output too small for 2 seconds: {output.Length} bytes");
    }
    
    [Fact]
    public async Task EncodeToMp3Async_CreatesFile()
    {
        // Arrange
        var encoder = new ShineEncoder(44100, 2, 128);
        var samples = GenerateSineWave(440, 44100, 0.5f, 2);
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_mp3_{Guid.NewGuid()}.mp3");
        
        try
        {
            // Act
            var success = await encoder.EncodeToFileAsync(samples, tempPath);
            
            // Assert
            Assert.True(success);
            Assert.True(File.Exists(tempPath));
            Assert.True(new FileInfo(tempPath).Length > 0);
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }
    
    private static float[] GenerateSineWave(double frequency, int sampleRate, float durationSeconds, int channels)
    {
        var numSamples = (int)(sampleRate * durationSeconds);
        var samples = new float[numSamples * channels];
        
        for (int i = 0; i < numSamples; i++)
        {
            var sample = (float)(0.5 * Math.Sin(2 * Math.PI * frequency * i / sampleRate));
            for (int c = 0; c < channels; c++)
            {
                samples[i * channels + c] = sample;
            }
        }
        
        return samples;
    }
}


