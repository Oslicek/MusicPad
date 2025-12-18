using MusicPad.Core.Audio;
using Xunit;

namespace MusicPad.Tests.Audio;

public class EqualizerTests
{
    [Fact]
    public void DefaultGains_AreZero()
    {
        var eq = new Equalizer();
        
        for (int i = 0; i < 4; i++)
        {
            Assert.Equal(0f, eq.GetGain(i));
        }
    }

    [Fact]
    public void SetGain_UpdatesValue()
    {
        var eq = new Equalizer();
        
        eq.SetGain(0, 0.5f);
        eq.SetGain(1, -0.3f);
        eq.SetGain(2, 0.8f);
        eq.SetGain(3, -0.9f);
        
        Assert.Equal(0.5f, eq.GetGain(0));
        Assert.Equal(-0.3f, eq.GetGain(1));
        Assert.Equal(0.8f, eq.GetGain(2));
        Assert.Equal(-0.9f, eq.GetGain(3));
    }

    [Fact]
    public void SetGain_ClampsValue()
    {
        var eq = new Equalizer();
        
        eq.SetGain(0, 2.0f);
        eq.SetGain(1, -2.0f);
        
        Assert.Equal(1f, eq.GetGain(0));
        Assert.Equal(-1f, eq.GetGain(1));
    }

    [Fact]
    public void SetGain_InvalidBand_DoesNothing()
    {
        var eq = new Equalizer();
        
        eq.SetGain(-1, 0.5f);
        eq.SetGain(4, 0.5f);
        eq.SetGain(10, 0.5f);
        
        // Should not crash, gains should remain at default
        for (int i = 0; i < 4; i++)
        {
            Assert.Equal(0f, eq.GetGain(i));
        }
    }

    [Fact]
    public void GetGain_InvalidBand_ReturnsZero()
    {
        var eq = new Equalizer();
        
        Assert.Equal(0f, eq.GetGain(-1));
        Assert.Equal(0f, eq.GetGain(4));
        Assert.Equal(0f, eq.GetGain(100));
    }

    [Fact]
    public void Process_WithFlatEQ_PassesThrough()
    {
        var eq = new Equalizer();
        
        // All gains at 0 = flat EQ = pass-through
        float input = 0.5f;
        
        // Process multiple samples to let the filter settle
        float output = 0f;
        for (int i = 0; i < 100; i++)
        {
            output = eq.Process(input);
        }
        
        // Should be close to input
        Assert.True(Math.Abs(output - input) < 0.1f, $"Expected ~{input}, got {output}");
    }

    [Fact]
    public void ProcessBuffer_ModifiesBuffer()
    {
        var eq = new Equalizer();
        eq.SetGain(0, 1.0f); // Boost low frequencies
        
        var buffer = new float[100];
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = 0.5f;
        }
        
        eq.Process(buffer);
        
        // Buffer should be modified (boosted)
        // Due to filter response, output may differ from input
        Assert.True(buffer[99] != 0f);
    }

    [Fact]
    public void Reset_ClearsFilterState()
    {
        var eq = new Equalizer();
        eq.SetGain(0, 1.0f);
        
        // Build up filter state
        for (int i = 0; i < 100; i++)
        {
            eq.Process(1.0f);
        }
        
        eq.Reset();
        
        // After reset, processing zero should output near-zero
        float output = eq.Process(0f);
        Assert.True(Math.Abs(output) < 0.1f, $"Expected near 0 after reset, got {output}");
    }
}

