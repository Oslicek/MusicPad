using MusicPad.Core.Audio;
using MusicPad.Core.Models;
using Xunit;

namespace MusicPad.Tests.Audio;

public class ReverbTests
{
    private const int SampleRate = 44100;

    [Fact]
    public void WhenDisabled_PassesThroughUnchanged()
    {
        var reverb = new Reverb(SampleRate);
        reverb.IsEnabled = false;
        
        float[] buffer = { 0.5f, 0.3f, -0.2f, 0.8f };
        float[] expected = { 0.5f, 0.3f, -0.2f, 0.8f };
        
        reverb.Process(buffer);
        
        Assert.Equal(expected, buffer);
    }

    [Fact]
    public void WhenEnabled_ModifiesSignal()
    {
        var reverb = new Reverb(SampleRate);
        reverb.IsEnabled = true;
        reverb.Level = 0.5f;
        
        // Create impulse
        float[] buffer = new float[SampleRate];
        buffer[0] = 1f;
        
        reverb.Process(buffer);
        
        // Should have reverb tail after the impulse
        bool hasReverbTail = false;
        for (int i = 1000; i < buffer.Length; i++)
        {
            if (Math.Abs(buffer[i]) > 0.001f)
            {
                hasReverbTail = true;
                break;
            }
        }
        
        Assert.True(hasReverbTail, "Reverb should produce a tail after impulse");
    }

    [Fact]
    public void Level_ControlsWetDryMix()
    {
        var reverb = new Reverb(SampleRate);
        reverb.IsEnabled = true;
        reverb.Type = ReverbType.Room;
        
        // Test with low level
        float[] bufferLow = new float[SampleRate / 2];
        bufferLow[0] = 1f;
        reverb.Level = 0.1f;
        reverb.Process(bufferLow);
        
        reverb.Reset();
        
        // Test with high level
        float[] bufferHigh = new float[SampleRate / 2];
        bufferHigh[0] = 1f;
        reverb.Level = 0.9f;
        reverb.Process(bufferHigh);
        
        // Calculate average tail energy
        float energyLow = 0f, energyHigh = 0f;
        for (int i = 1000; i < bufferLow.Length; i++)
        {
            energyLow += bufferLow[i] * bufferLow[i];
            energyHigh += bufferHigh[i] * bufferHigh[i];
        }
        
        Assert.True(energyHigh > energyLow, "Higher level should produce more reverb energy");
    }

    [Theory]
    [InlineData(ReverbType.Room)]
    [InlineData(ReverbType.Hall)]
    [InlineData(ReverbType.Plate)]
    [InlineData(ReverbType.Church)]
    public void AllTypes_ProduceOutput(ReverbType type)
    {
        var reverb = new Reverb(SampleRate);
        reverb.IsEnabled = true;
        reverb.Level = 0.5f;
        reverb.Type = type;
        
        float[] buffer = new float[SampleRate];
        buffer[0] = 1f;
        
        reverb.Process(buffer);
        
        // Should produce some reverb tail
        float maxTail = 0f;
        for (int i = 1000; i < buffer.Length; i++)
        {
            maxTail = Math.Max(maxTail, Math.Abs(buffer[i]));
        }
        
        Assert.True(maxTail > 0.001f, $"{type} reverb should produce audible tail");
    }

    [Fact]
    public void DifferentTypes_HaveDifferentDecayCharacteristics()
    {
        // Room should decay faster than Church
        var reverbRoom = new Reverb(SampleRate);
        reverbRoom.IsEnabled = true;
        reverbRoom.Level = 0.5f;
        reverbRoom.Type = ReverbType.Room;
        
        var reverbChurch = new Reverb(SampleRate);
        reverbChurch.IsEnabled = true;
        reverbChurch.Level = 0.5f;
        reverbChurch.Type = ReverbType.Church;
        
        float[] bufferRoom = new float[SampleRate * 2];
        float[] bufferChurch = new float[SampleRate * 2];
        bufferRoom[0] = 1f;
        bufferChurch[0] = 1f;
        
        reverbRoom.Process(bufferRoom);
        reverbChurch.Process(bufferChurch);
        
        // Measure energy in the second half (long tail)
        float energyRoom = 0f, energyChurch = 0f;
        int startSample = SampleRate; // Start at 1 second
        for (int i = startSample; i < bufferRoom.Length; i++)
        {
            energyRoom += bufferRoom[i] * bufferRoom[i];
            energyChurch += bufferChurch[i] * bufferChurch[i];
        }
        
        Assert.True(energyChurch > energyRoom, "Church should have longer tail than Room");
    }

    [Fact]
    public void Reset_ClearsReverbTail()
    {
        var reverb = new Reverb(SampleRate);
        reverb.IsEnabled = true;
        reverb.Level = 0.5f;
        reverb.Type = ReverbType.Hall;
        
        // Process some signal
        float[] buffer = new float[SampleRate];
        for (int i = 0; i < 1000; i++) buffer[i] = 0.5f;
        reverb.Process(buffer);
        
        // Reset
        reverb.Reset();
        
        // Process silence - should output near silence
        float[] silentBuffer = new float[SampleRate];
        reverb.Process(silentBuffer);
        
        float maxValue = silentBuffer.Max(Math.Abs);
        Assert.True(maxValue < 0.01f, "After reset, processing silence should produce silence");
    }

    [Fact]
    public void ChangingType_UpdatesReverbCharacter()
    {
        var reverb = new Reverb(SampleRate);
        reverb.IsEnabled = true;
        reverb.Level = 0.5f;
        
        // Process with Room
        reverb.Type = ReverbType.Room;
        float[] buffer1 = new float[SampleRate];
        buffer1[0] = 1f;
        reverb.Process(buffer1);
        
        reverb.Reset();
        
        // Process with Hall
        reverb.Type = ReverbType.Hall;
        float[] buffer2 = new float[SampleRate];
        buffer2[0] = 1f;
        reverb.Process(buffer2);
        
        // The outputs should be different
        bool areDifferent = false;
        for (int i = 0; i < buffer1.Length; i++)
        {
            if (Math.Abs(buffer1[i] - buffer2[i]) > 0.01f)
            {
                areDifferent = true;
                break;
            }
        }
        
        Assert.True(areDifferent, "Different reverb types should produce different outputs");
    }

    [Fact]
    public void ProcessDoesNotClip_WithinReasonableInput()
    {
        var reverb = new Reverb(SampleRate);
        reverb.IsEnabled = true;
        reverb.Level = 1.0f;
        reverb.Type = ReverbType.Church;
        
        float[] buffer = new float[SampleRate * 2];
        // Fill with moderate signal
        for (int i = 0; i < 10000; i++)
        {
            buffer[i] = 0.5f * MathF.Sin(2f * MathF.PI * 440f * i / SampleRate);
        }
        
        reverb.Process(buffer);
        
        // Check no extreme values
        float maxAbs = buffer.Max(Math.Abs);
        Assert.True(maxAbs < 3f, "Output should not explode");
    }
}

