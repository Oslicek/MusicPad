using MusicMap.Core.Audio;

namespace MusicMap.Tests;

public class WaveTableGeneratorTests
{
    [Fact]
    public void GenerateSineWave_WithValidFrequency_ReturnsWaveTable()
    {
        // Arrange
        var generator = new WaveTableGenerator(44100);

        // Act
        var waveTable = generator.GenerateSineWave(440.0);

        // Assert
        Assert.NotNull(waveTable);
        Assert.NotEmpty(waveTable);
    }

    [Fact]
    public void GenerateSineWave_WaveTableLength_IsCorrect()
    {
        // Arrange
        var sampleRate = 44100;
        var frequency = 440.0;
        var generator = new WaveTableGenerator(sampleRate);

        // Act
        var waveTable = generator.GenerateSineWave(frequency);

        // Assert
        var expectedLength = (int)Math.Round(sampleRate / frequency);
        Assert.Equal(expectedLength, waveTable.Length);
    }

    [Fact]
    public void GenerateSineWave_ValuesWithinAmplitude()
    {
        // Arrange
        var generator = new WaveTableGenerator(44100);
        var amplitude = 0.5f;

        // Act
        var waveTable = generator.GenerateSineWave(440.0, amplitude);

        // Assert
        foreach (var sample in waveTable)
        {
            Assert.InRange(sample, -amplitude, amplitude);
        }
    }

    [Fact]
    public void GenerateSineWave_StartsAtZero()
    {
        // Arrange
        var generator = new WaveTableGenerator(44100);

        // Act
        var waveTable = generator.GenerateSineWave(440.0);

        // Assert
        Assert.Equal(0f, waveTable[0], 5);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void Constructor_InvalidSampleRate_Throws(int sampleRate)
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new WaveTableGenerator(sampleRate));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-440)]
    public void GenerateSineWave_InvalidFrequency_Throws(double frequency)
    {
        // Arrange
        var generator = new WaveTableGenerator(44100);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => generator.GenerateSineWave(frequency));
    }

    [Theory]
    [InlineData(-0.1f)]
    [InlineData(1.1f)]
    public void GenerateSineWave_InvalidAmplitude_Throws(float amplitude)
    {
        // Arrange
        var generator = new WaveTableGenerator(44100);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => generator.GenerateSineWave(440.0, amplitude));
    }
}

