using MusicPad.Core.Audio;
using Xunit;

namespace MusicPad.Tests.Audio;

public class DelayTests
{
    private const int SampleRate = 44100;

    [Fact]
    public void WhenDisabled_PassesThroughUnchanged()
    {
        var delay = new Delay(SampleRate);
        delay.IsEnabled = false;
        
        float[] buffer = { 0.5f, 0.3f, -0.2f, 0.8f };
        float[] expected = { 0.5f, 0.3f, -0.2f, 0.8f };
        
        delay.Process(buffer);
        
        Assert.Equal(expected, buffer);
    }

    [Fact]
    public void WhenEnabled_ProducesDelayedSignal()
    {
        var delay = new Delay(SampleRate);
        delay.IsEnabled = true;
        delay.Time = 0f; // Minimum delay (~50ms = ~2205 samples)
        delay.Feedback = 0f;
        delay.Level = 1f;
        
        // Create impulse
        float[] buffer = new float[SampleRate];
        buffer[0] = 1f;
        
        delay.Process(buffer);
        
        // With delay at minimum time, the delayed signal should appear after ~50ms
        int expectedDelaySamples = (int)(50f * SampleRate / 1000f);
        
        // The output should have the original impulse plus delayed version
        Assert.True(Math.Abs(buffer[0]) > 0.5f, "First sample should contain original signal");
        
        // Somewhere around the delay time, we should see some signal
        bool foundDelayedSignal = false;
        for (int i = expectedDelaySamples - 100; i < expectedDelaySamples + 100 && i < buffer.Length; i++)
        {
            if (i > 0 && Math.Abs(buffer[i]) > 0.1f)
            {
                foundDelayedSignal = true;
                break;
            }
        }
        Assert.True(foundDelayedSignal, "Should have delayed signal around expected time");
    }

    [Fact]
    public void Feedback_ProducesMultipleEchoes()
    {
        var delay = new Delay(SampleRate);
        delay.IsEnabled = true;
        delay.Time = 0f; // Minimum delay
        delay.Feedback = 0.5f;
        delay.Level = 1f;
        
        // Create impulse
        float[] buffer = new float[SampleRate / 2]; // 500ms of audio
        buffer[0] = 1f;
        
        delay.Process(buffer);
        
        // With feedback, we should see multiple peaks
        int peakCount = 0;
        for (int i = 1; i < buffer.Length - 1; i++)
        {
            if (Math.Abs(buffer[i]) > 0.05f && 
                Math.Abs(buffer[i]) > Math.Abs(buffer[i-1]) && 
                Math.Abs(buffer[i]) > Math.Abs(buffer[i+1]))
            {
                peakCount++;
            }
        }
        
        Assert.True(peakCount > 1, "Should have multiple echo peaks with feedback");
    }

    [Fact]
    public void Level_ControlsWetSignalAmount()
    {
        var delay = new Delay(SampleRate);
        delay.IsEnabled = true;
        delay.Time = 0f;
        delay.Feedback = 0f;
        
        float[] bufferLow = new float[SampleRate];
        float[] bufferHigh = new float[SampleRate];
        bufferLow[0] = 1f;
        bufferHigh[0] = 1f;
        
        delay.Level = 0.2f;
        delay.Process(bufferLow);
        
        delay.Reset();
        
        delay.Level = 0.8f;
        delay.Process(bufferHigh);
        
        // Find max delayed signal amplitude (after initial impulse)
        float maxLow = 0f, maxHigh = 0f;
        int delayOffset = (int)(50f * SampleRate / 1000f);
        
        for (int i = delayOffset; i < delayOffset + 200 && i < bufferLow.Length; i++)
        {
            maxLow = Math.Max(maxLow, Math.Abs(bufferLow[i]));
            maxHigh = Math.Max(maxHigh, Math.Abs(bufferHigh[i]));
        }
        
        Assert.True(maxHigh > maxLow, "Higher level should produce louder delayed signal");
    }

    [Fact]
    public void Time_AffectsDelayLength()
    {
        var delay = new Delay(SampleRate);
        delay.IsEnabled = true;
        delay.Feedback = 0f;
        delay.Level = 1f;
        
        // Test short delay
        delay.Time = 0f; // ~50ms
        float[] bufferShort = new float[SampleRate];
        bufferShort[0] = 1f;
        delay.Process(bufferShort);
        
        // Find first delayed peak
        int firstPeakShort = FindFirstPeak(bufferShort, 100);
        
        delay.Reset();
        
        // Test long delay
        delay.Time = 1f; // ~1000ms
        float[] bufferLong = new float[SampleRate * 2];
        bufferLong[0] = 1f;
        delay.Process(bufferLong);
        
        int firstPeakLong = FindFirstPeak(bufferLong, 100);
        
        Assert.True(firstPeakLong > firstPeakShort, "Longer delay time should produce later echo");
    }

    [Fact]
    public void Reset_ClearsDelayBuffer()
    {
        var delay = new Delay(SampleRate);
        delay.IsEnabled = true;
        delay.Time = 0f;
        delay.Feedback = 0.5f;
        delay.Level = 1f;
        
        // Process some signal
        float[] buffer = new float[SampleRate];
        for (int i = 0; i < 1000; i++) buffer[i] = 0.5f;
        delay.Process(buffer);
        
        // Reset
        delay.Reset();
        
        // Process silence
        float[] silentBuffer = new float[SampleRate];
        delay.Process(silentBuffer);
        
        // After reset, processing silence should output near-silence
        float maxValue = silentBuffer.Max(Math.Abs);
        Assert.True(maxValue < 0.01f, "After reset, processing silence should produce silence");
    }

    [Fact]
    public void SingleSampleProcess_WorksCorrectly()
    {
        var delay = new Delay(SampleRate);
        delay.IsEnabled = true;
        delay.Time = 0f;
        delay.Feedback = 0f;
        delay.Level = 0.5f;
        
        // Process individual samples
        float output = delay.Process(1f);
        
        // First sample should pass through (plus any immediate delay contribution)
        Assert.True(Math.Abs(output) > 0f, "Should have some output");
    }

    private static int FindFirstPeak(float[] buffer, int startIndex)
    {
        for (int i = startIndex; i < buffer.Length - 1; i++)
        {
            if (Math.Abs(buffer[i]) > 0.1f &&
                Math.Abs(buffer[i]) > Math.Abs(buffer[i-1]) &&
                Math.Abs(buffer[i]) >= Math.Abs(buffer[i+1]))
            {
                return i;
            }
        }
        return -1;
    }
}

