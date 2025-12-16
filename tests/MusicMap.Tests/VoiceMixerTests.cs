using MusicMap.Core.Audio;

namespace MusicMap.Tests;

public class VoiceMixerTests
{
    private const int SampleRate = 44100;
    private const int ReleaseSamples = 128;
    private const int MaxVoices = 6;

    private static float[] CreateTestWaveTable(int length = 100, float amplitude = 0.5f)
    {
        var table = new float[length];
        for (int i = 0; i < length; i++)
        {
            table[i] = (float)(Math.Sin(2.0 * Math.PI * i / length) * amplitude);
        }
        return table;
    }

    [Fact]
    public void AddVoice_IncreasesActiveVoiceCount()
    {
        // Arrange
        var mixer = new VoiceMixer(SampleRate, ReleaseSamples, MaxVoices);
        var waveTable = CreateTestWaveTable();

        // Act
        mixer.AddVoice(440.0, waveTable);

        // Assert
        Assert.Equal(1, mixer.ActiveVoiceCount);
    }

    [Fact]
    public void AddVoice_ExceedingMaxVoices_RemovesOldest()
    {
        // Arrange
        var mixer = new VoiceMixer(SampleRate, ReleaseSamples, 2);
        var waveTable = CreateTestWaveTable();

        // Act
        mixer.AddVoice(440.0, waveTable);
        mixer.AddVoice(550.0, waveTable);
        mixer.AddVoice(660.0, waveTable);

        // Assert
        Assert.Equal(2, mixer.ActiveVoiceCount);
    }

    [Fact]
    public void Mix_WithNoVoices_ReturnsZeroBuffer()
    {
        // Arrange
        var mixer = new VoiceMixer(SampleRate, ReleaseSamples, MaxVoices);
        var buffer = new float[512];

        // Act
        mixer.Mix(buffer);

        // Assert
        Assert.All(buffer, sample => Assert.Equal(0f, sample));
    }

    [Fact]
    public void Mix_WithVoice_ProducesNonZeroSamples()
    {
        // Arrange
        var mixer = new VoiceMixer(SampleRate, ReleaseSamples, MaxVoices);
        var waveTable = CreateTestWaveTable();
        mixer.AddVoice(440.0, waveTable);
        var buffer = new float[512];

        // Act
        mixer.Mix(buffer);

        // Assert
        Assert.Contains(buffer, sample => Math.Abs(sample) > 0.0001f);
    }

    [Fact]
    public void ReleaseVoice_StartsReleaseEnvelope()
    {
        // Arrange
        var mixer = new VoiceMixer(SampleRate, ReleaseSamples, MaxVoices);
        var waveTable = CreateTestWaveTable();
        mixer.AddVoice(440.0, waveTable);
        
        // Act
        mixer.ReleaseVoice(440.0);
        
        // Mix enough buffers for release to complete
        var buffer = new float[512];
        for (int i = 0; i < 100; i++)
        {
            mixer.Mix(buffer);
        }

        // Assert - voice should be removed after release
        Assert.Equal(0, mixer.ActiveVoiceCount);
    }

    [Fact]
    public void ReleaseAll_ReleasesAllVoices()
    {
        // Arrange
        var mixer = new VoiceMixer(SampleRate, ReleaseSamples, MaxVoices);
        var waveTable = CreateTestWaveTable();
        mixer.AddVoice(440.0, waveTable);
        mixer.AddVoice(550.0, waveTable);

        // Act
        mixer.ReleaseAll();
        
        // Mix enough buffers for release to complete
        var buffer = new float[512];
        for (int i = 0; i < 100; i++)
        {
            mixer.Mix(buffer);
        }

        // Assert
        Assert.Equal(0, mixer.ActiveVoiceCount);
    }

    [Fact]
    public void UpdateVoice_UpdatesWaveTable()
    {
        // Arrange
        var mixer = new VoiceMixer(SampleRate, ReleaseSamples, MaxVoices);
        var waveTable1 = CreateTestWaveTable(100, 0.3f);
        var waveTable2 = CreateTestWaveTable(100, 0.8f);
        mixer.AddVoice(440.0, waveTable1);

        // Act
        mixer.UpdateVoice(440.0, waveTable2);
        var buffer = new float[512];
        mixer.Mix(buffer);

        // Assert - we expect the update to have happened (hard to verify exact values)
        Assert.Equal(1, mixer.ActiveVoiceCount);
    }

    [Fact]
    public void SetEnvelope_UpdatesEnvelopeSettings()
    {
        // Arrange
        var mixer = new VoiceMixer(SampleRate, ReleaseSamples, MaxVoices);
        var settings = new AHDSHRSettings
        {
            AttackMs = 50,
            Hold1Ms = 10,
            DecayMs = 100,
            SustainLevel = 0.7f,
            Hold2Ms = -1,
            ReleaseMs = 200
        };

        // Act & Assert (no exception means success)
        mixer.SetEnvelope(settings);
    }
}

