using MusicPad.Core.Export;
using Xunit;

namespace MusicPad.Tests.Export;

/// <summary>
/// Tests for the audio encoder interface contract.
/// Note: These tests verify the interface contract, not FFmpeg accuracy.
/// </summary>
public class AudioEncoderTests
{
    [Fact]
    public void StubEncoder_IsAvailable_ReturnsFalse()
    {
        // Stub encoder is used when FFmpeg is not available
        var encoder = new StubAudioEncoder();
        Assert.False(encoder.IsAvailable);
    }
    
    [Fact]
    public async Task StubEncoder_EncodeToMp3_ReturnsFalse()
    {
        var encoder = new StubAudioEncoder();
        var samples = new float[44100 * 2]; // 1 second stereo
        
        var result = await encoder.EncodeToMp3Async(samples, 44100, 2, 192, "test.mp3");
        
        Assert.False(result);
    }
    
    [Fact]
    public async Task StubEncoder_EncodeToFlac_ReturnsFalse()
    {
        var encoder = new StubAudioEncoder();
        var samples = new float[44100 * 2]; // 1 second stereo
        
        var result = await encoder.EncodeToFlacAsync(samples, 44100, 2, 16, "test.flac");
        
        Assert.False(result);
    }
    
    [Fact]
    public void FallbackEncoder_FlacEncode_UsesCustomEncoder()
    {
        // Fallback encoder should use the custom FlacEncoder when FFmpeg is unavailable
        var fallback = new FallbackAudioEncoder(new StubAudioEncoder());
        
        // It should report as available because it can fall back to custom encoder
        Assert.True(fallback.CanEncodeFallbackFlac);
    }
    
    [Fact]
    public async Task FallbackEncoder_FlacEncode_CreatesFile()
    {
        var fallback = new FallbackAudioEncoder(new StubAudioEncoder());
        var samples = new float[4410 * 2]; // 0.1 second stereo silence
        
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.flac");
        try
        {
            var result = await fallback.EncodeToFlacAsync(samples, 44100, 2, 16, tempPath);
            
            Assert.True(result);
            Assert.True(File.Exists(tempPath));
            
            // Verify file has content (FLAC header starts with "fLaC")
            var bytes = await File.ReadAllBytesAsync(tempPath);
            Assert.True(bytes.Length > 42); // At least header size
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
    
    [Fact]
    public void FallbackEncoder_Mp3Encode_CanUseFallback()
    {
        // MP3 encoding can now fall back to ShineEncoder
        var fallback = new FallbackAudioEncoder(new StubAudioEncoder());
        
        Assert.True(fallback.CanEncodeFallbackMp3);
    }
    
    [Fact]
    public async Task FallbackEncoder_Mp3Encode_CreatesFile()
    {
        // MP3 encoding should work using ShineEncoder fallback
        var fallback = new FallbackAudioEncoder(new StubAudioEncoder());
        var samples = new float[4410 * 2]; // 0.1 second stereo
        
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.mp3");
        try
        {
            var result = await fallback.EncodeToMp3Async(samples, 44100, 2, 192, tempPath);
            
            Assert.True(result);
            Assert.True(File.Exists(tempPath));
            
            // Verify file has content (MP3 frame starts with sync word 0xFF 0xFB)
            var bytes = await File.ReadAllBytesAsync(tempPath);
            Assert.True(bytes.Length > 4);
            Assert.Equal(0xFF, bytes[0]);
            Assert.Equal(0xFB, bytes[1]);
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }
}

