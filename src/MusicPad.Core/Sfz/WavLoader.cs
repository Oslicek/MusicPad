namespace MusicPad.Core.Sfz;

/// <summary>
/// Result of loading WAV samples.
/// </summary>
public readonly record struct WavData(float[] Samples, int SampleRate, int Channels);

/// <summary>
/// Loader for WAV audio files.
/// Supports 16-bit and 24-bit PCM WAV files.
/// </summary>
public static class WavLoader
{
    /// <summary>
    /// Loads all samples from a WAV file.
    /// </summary>
    public static WavData LoadSamples(byte[] wavData)
    {
        return LoadSamplesSlice(wavData, offset: 0, end: -1);
    }

    /// <summary>
    /// Loads all samples from a WAV file.
    /// </summary>
    public static WavData LoadSamples(Stream stream)
    {
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return LoadSamples(ms.ToArray());
    }

    /// <summary>
    /// Loads a slice of samples from a WAV file.
    /// Offset and end are in sample frames (not bytes).
    /// </summary>
    public static WavData LoadSamplesSlice(byte[] wavData, int offset, int end)
    {
        using var ms = new MemoryStream(wavData);
        using var reader = new BinaryReader(ms);

        // Read RIFF header
        var riff = reader.ReadBytes(4);
        if (!riff.SequenceEqual("RIFF"u8.ToArray()))
            throw new InvalidDataException("Not a valid WAV file: missing RIFF header");

        reader.ReadInt32(); // File size
        
        var wave = reader.ReadBytes(4);
        if (!wave.SequenceEqual("WAVE"u8.ToArray()))
            throw new InvalidDataException("Not a valid WAV file: missing WAVE format");

        int sampleRate = 0;
        int channels = 0;
        int bitsPerSample = 0;
        byte[]? dataBytes = null;

        // Read chunks
        while (ms.Position < ms.Length)
        {
            var chunkId = reader.ReadBytes(4);
            var chunkSize = reader.ReadInt32();

            if (chunkId.SequenceEqual("fmt "u8.ToArray()))
            {
                var audioFormat = reader.ReadInt16();
                if (audioFormat != 1)
                    throw new NotSupportedException($"Only PCM format is supported, got format {audioFormat}");

                channels = reader.ReadInt16();
                sampleRate = reader.ReadInt32();
                reader.ReadInt32(); // Byte rate
                reader.ReadInt16(); // Block align
                bitsPerSample = reader.ReadInt16();

                // Skip any extra format bytes
                var remaining = chunkSize - 16;
                if (remaining > 0)
                    reader.ReadBytes(remaining);
            }
            else if (chunkId.SequenceEqual("data"u8.ToArray()))
            {
                dataBytes = reader.ReadBytes(chunkSize);
            }
            else
            {
                // Skip unknown chunk
                reader.ReadBytes(chunkSize);
            }
        }

        if (dataBytes == null)
            throw new InvalidDataException("No data chunk found in WAV file");

        // Convert bytes to float samples
        var samples = ConvertToFloat(dataBytes, bitsPerSample, channels, offset, end);

        return new WavData(samples, sampleRate, channels);
    }

    private static float[] ConvertToFloat(byte[] dataBytes, int bitsPerSample, int channels, int frameOffset, int frameEnd)
    {
        int bytesPerSample = bitsPerSample / 8;
        int bytesPerFrame = bytesPerSample * channels;
        int totalFrames = dataBytes.Length / bytesPerFrame;

        // Calculate actual range
        int startFrame = Math.Max(0, frameOffset);
        int endFrame = frameEnd < 0 ? totalFrames - 1 : Math.Min(frameEnd, totalFrames - 1);
        int frameCount = endFrame - startFrame + 1;

        if (frameCount <= 0)
            return Array.Empty<float>();

        int sampleCount = frameCount * channels;
        var samples = new float[sampleCount];

        int startByte = startFrame * bytesPerFrame;
        int sampleIndex = 0;

        for (int frame = 0; frame < frameCount; frame++)
        {
            for (int channel = 0; channel < channels; channel++)
            {
                int bytePos = startByte + (frame * bytesPerFrame) + (channel * bytesPerSample);
                
                float sample = bitsPerSample switch
                {
                    16 => BitConverter.ToInt16(dataBytes, bytePos) / 32768f,
                    24 => Read24BitSample(dataBytes, bytePos) / 8388608f,
                    32 => BitConverter.ToInt32(dataBytes, bytePos) / 2147483648f,
                    _ => throw new NotSupportedException($"Unsupported bit depth: {bitsPerSample}")
                };

                samples[sampleIndex++] = sample;
            }
        }

        return samples;
    }

    private static int Read24BitSample(byte[] data, int offset)
    {
        // Read 3 bytes as signed 24-bit integer
        int value = data[offset] | (data[offset + 1] << 8) | (data[offset + 2] << 16);
        
        // Sign extend if negative
        if ((value & 0x800000) != 0)
            value |= unchecked((int)0xFF000000);
        
        return value;
    }
}


