using MusicPad.Core.Audio;
using Xunit;

namespace MusicPad.Tests.Audio;

public class LowPassFilterTests
{
    [Fact]
    public void DefaultIsEnabled_IsFalse()
    {
        var filter = new LowPassFilter();
        
        Assert.False(filter.IsEnabled);
    }

    [Fact]
    public void WhenDisabled_PassesThroughUnchanged()
    {
        var filter = new LowPassFilter();
        filter.IsEnabled = false;
        
        float input = 0.5f;
        float output = filter.Process(input);
        
        Assert.Equal(input, output);
    }

    [Fact]
    public void WhenEnabled_ProcessesSignal()
    {
        var filter = new LowPassFilter();
        filter.IsEnabled = true;
        filter.Cutoff = 0.5f;
        
        // Process a step function (DC signal)
        float output = 0f;
        for (int i = 0; i < 100; i++)
        {
            output = filter.Process(1.0f);
        }
        
        // After processing, filter should converge toward input value
        Assert.True(output > 0.5f, $"Expected output > 0.5, got {output}");
    }

    [Fact]
    public void Cutoff_IsClamped()
    {
        var filter = new LowPassFilter();
        
        filter.Cutoff = -0.5f;
        Assert.Equal(0f, filter.Cutoff);
        
        filter.Cutoff = 1.5f;
        Assert.Equal(1f, filter.Cutoff);
    }

    [Fact]
    public void Resonance_IsClamped()
    {
        var filter = new LowPassFilter();
        
        filter.Resonance = -0.5f;
        Assert.Equal(0f, filter.Resonance);
        
        filter.Resonance = 1.5f;
        Assert.Equal(1f, filter.Resonance);
    }

    [Fact]
    public void ProcessBuffer_WhenDisabled_PassesThrough()
    {
        var filter = new LowPassFilter();
        filter.IsEnabled = false;
        
        var buffer = new float[] { 0.1f, 0.2f, 0.3f, 0.4f };
        var original = (float[])buffer.Clone();
        
        filter.Process(buffer);
        
        Assert.Equal(original, buffer);
    }

    [Fact]
    public void ProcessBuffer_WhenEnabled_ModifiesBuffer()
    {
        var filter = new LowPassFilter();
        filter.IsEnabled = true;
        filter.Cutoff = 0.3f;
        
        var buffer = new float[] { 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f };
        
        filter.Process(buffer);
        
        // First sample should be less than 1 due to filter
        Assert.True(buffer[0] < 1f);
    }

    [Fact]
    public void Reset_ClearsFilterState()
    {
        var filter = new LowPassFilter();
        filter.IsEnabled = true;
        filter.Cutoff = 0.5f;
        
        // Build up some state
        for (int i = 0; i < 100; i++)
        {
            filter.Process(1.0f);
        }
        
        // Reset the filter
        filter.Reset();
        
        // Process a zero - should output close to zero after reset
        float output = filter.Process(0f);
        Assert.True(Math.Abs(output) < 0.1f, $"Expected output near 0 after reset, got {output}");
    }

    [Fact]
    public void LowCutoff_AttenuatesHighFrequencies()
    {
        var filter = new LowPassFilter();
        filter.IsEnabled = true;
        filter.Cutoff = 0.1f; // Very low cutoff
        
        // Simulate a high-frequency oscillation
        var buffer = new float[100];
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = (i % 2 == 0) ? 1f : -1f;
        }
        
        filter.Process(buffer);
        
        // High-frequency content should be attenuated significantly
        float maxAbs = buffer.Max(Math.Abs);
        Assert.True(maxAbs < 0.5f, $"Expected max amplitude < 0.5, got {maxAbs}");
    }

    [Fact]
    public void HighCutoff_PassesLowFrequencies()
    {
        var filter = new LowPassFilter();
        filter.IsEnabled = true;
        filter.Cutoff = 1.0f; // Max cutoff (fully open)
        
        // With high cutoff, the average output should be close to input
        float sum = 0f;
        int samples = 1000;
        for (int i = 0; i < samples; i++)
        {
            sum += filter.Process(0.7f);
        }
        float average = sum / samples;
        
        // Average should be close to input value
        Assert.True(Math.Abs(average - 0.7f) < 0.3f, $"Expected average near 0.7, got {average}");
    }
}

