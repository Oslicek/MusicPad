using MusicPad.Core.Audio;
using Xunit;

namespace MusicPad.Tests.Audio;

public class ChorusTests
{
    [Fact]
    public void DefaultIsEnabled_IsFalse()
    {
        var chorus = new Chorus();
        Assert.False(chorus.IsEnabled);
    }

    [Fact]
    public void WhenDisabled_PassesThroughUnchanged()
    {
        var chorus = new Chorus();
        chorus.IsEnabled = false;
        
        float input = 0.5f;
        float output = chorus.Process(input);
        
        Assert.Equal(input, output);
    }

    [Fact]
    public void WhenEnabled_ProcessesSignal()
    {
        var chorus = new Chorus();
        chorus.IsEnabled = true;
        chorus.Depth = 0.5f;
        chorus.Rate = 0.5f;
        
        // Process multiple samples to build up delay buffer
        for (int i = 0; i < 1000; i++)
        {
            chorus.Process(0.5f);
        }
        
        float output = chorus.Process(0.5f);
        
        // Output should exist (not crash)
        Assert.True(output != float.NaN);
    }

    [Fact]
    public void Depth_IsClamped()
    {
        var chorus = new Chorus();
        
        chorus.Depth = -0.5f;
        Assert.Equal(0f, chorus.Depth);
        
        chorus.Depth = 1.5f;
        Assert.Equal(1f, chorus.Depth);
    }

    [Fact]
    public void Rate_IsClamped()
    {
        var chorus = new Chorus();
        
        chorus.Rate = -0.5f;
        Assert.Equal(0f, chorus.Rate);
        
        chorus.Rate = 1.5f;
        Assert.Equal(1f, chorus.Rate);
    }

    [Fact]
    public void ProcessBuffer_WhenDisabled_PassesThrough()
    {
        var chorus = new Chorus();
        chorus.IsEnabled = false;
        
        var buffer = new float[] { 0.1f, 0.2f, 0.3f, 0.4f };
        var original = (float[])buffer.Clone();
        
        chorus.Process(buffer);
        
        Assert.Equal(original, buffer);
    }

    [Fact]
    public void ProcessBuffer_WhenEnabled_ModifiesBuffer()
    {
        var chorus = new Chorus();
        chorus.IsEnabled = true;
        chorus.Depth = 0.8f;
        chorus.Rate = 0.5f;
        
        // Pre-fill the delay buffer
        for (int i = 0; i < 2000; i++)
        {
            chorus.Process(0.5f);
        }
        
        var buffer = new float[100];
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = 0.5f;
        }
        
        chorus.Process(buffer);
        
        // Buffer should be modified (mixed with delayed signal)
        // Due to chorus effect, some samples will differ from 0.5
        bool anyDifferent = false;
        for (int i = 0; i < buffer.Length; i++)
        {
            if (Math.Abs(buffer[i] - 0.5f) > 0.001f)
            {
                anyDifferent = true;
                break;
            }
        }
        Assert.True(anyDifferent, "Chorus should modify the signal");
    }

    [Fact]
    public void Reset_ClearsState()
    {
        var chorus = new Chorus();
        chorus.IsEnabled = true;
        chorus.Depth = 1.0f;
        
        // Build up state
        for (int i = 0; i < 1000; i++)
        {
            chorus.Process(1.0f);
        }
        
        chorus.Reset();
        
        // After reset, processing zero should return something close to zero
        // (since delay buffer is cleared)
        float output = chorus.Process(0f);
        Assert.True(Math.Abs(output) < 0.1f, $"Expected near 0 after reset, got {output}");
    }
}

