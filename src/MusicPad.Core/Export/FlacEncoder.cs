namespace MusicPad.Core.Export;

/// <summary>
/// Encodes audio samples to FLAC format.
/// FLAC (Free Lossless Audio Codec) provides lossless compression.
/// </summary>
public class FlacEncoder
{
    private const int DefaultBlockSize = 4096;
    
    public int SampleRate { get; }
    public int Channels { get; }
    public int BitsPerSample { get; }
    
    public FlacEncoder(int sampleRate, int channels, int bitsPerSample)
    {
        if (sampleRate <= 0)
            throw new ArgumentException("Sample rate must be positive", nameof(sampleRate));
        if (channels < 1 || channels > 2)
            throw new ArgumentException("Channels must be 1 or 2", nameof(channels));
        if (bitsPerSample != 8 && bitsPerSample != 16 && bitsPerSample != 24)
            throw new ArgumentException("Bits per sample must be 8, 16, or 24", nameof(bitsPerSample));
        
        SampleRate = sampleRate;
        Channels = channels;
        BitsPerSample = bitsPerSample;
    }
    
    /// <summary>
    /// Encodes float samples (-1.0 to 1.0) to FLAC format.
    /// </summary>
    /// <param name="samples">Interleaved float samples (L, R, L, R, ...)</param>
    /// <param name="output">Stream to write FLAC data to</param>
    public void Encode(float[] samples, Stream output)
    {
        using var writer = new BinaryWriter(output, System.Text.Encoding.UTF8, leaveOpen: true);
        
        // Calculate total samples per channel (use long for proper 36-bit shift in STREAMINFO)
        long totalSamplesPerChannel = samples.Length / Channels;
        
        // Write FLAC stream marker
        writer.Write((byte)'f');
        writer.Write((byte)'L');
        writer.Write((byte)'a');
        writer.Write((byte)'C');
        
        // Write STREAMINFO metadata block (mandatory, must be first)
        WriteStreamInfoBlock(writer, totalSamplesPerChannel);
        
        // Convert float samples to integer samples
        var intSamples = ConvertToIntSamples(samples);
        
        // Write audio frames
        if (intSamples.Length > 0)
        {
            WriteAudioFrames(writer, intSamples, (int)totalSamplesPerChannel);
        }
    }
    
    private void WriteStreamInfoBlock(BinaryWriter writer, long totalSamplesPerChannel)
    {
        // Metadata block header
        // Bit 0: Last metadata block flag (1 = last)
        // Bits 1-7: Block type (0 = STREAMINFO)
        byte header = 0x80; // Last block, type 0 (STREAMINFO)
        writer.Write(header);
        
        // Block length (34 bytes for STREAMINFO)
        writer.Write((byte)0);
        writer.Write((byte)0);
        writer.Write((byte)34);
        
        // STREAMINFO content (34 bytes)
        // Minimum block size (16 bits)
        WriteBigEndian16(writer, (ushort)DefaultBlockSize);
        
        // Maximum block size (16 bits)
        WriteBigEndian16(writer, (ushort)DefaultBlockSize);
        
        // Minimum frame size (24 bits) - 0 means unknown
        writer.Write((byte)0);
        writer.Write((byte)0);
        writer.Write((byte)0);
        
        // Maximum frame size (24 bits) - 0 means unknown
        writer.Write((byte)0);
        writer.Write((byte)0);
        writer.Write((byte)0);
        
        // Sample rate (20 bits), channels-1 (3 bits), bits per sample-1 (5 bits), total samples (36 bits)
        // This is packed into 8 bytes
        
        // Byte 0: sample rate bits 19-12
        writer.Write((byte)(SampleRate >> 12));
        
        // Byte 1: sample rate bits 11-4
        writer.Write((byte)((SampleRate >> 4) & 0xFF));
        
        // Byte 2: sample rate bits 3-0, channels-1 (3 bits), bps-1 bit 4
        byte byte2 = (byte)(((SampleRate & 0x0F) << 4) | ((Channels - 1) << 1) | ((BitsPerSample - 1) >> 4));
        writer.Write(byte2);
        
        // Byte 3: bps-1 bits 3-0, total samples bits 35-32
        // NOTE: Must use long for the shift - shifting int by 32 in C# shifts by 0!
        byte byte3 = (byte)((((BitsPerSample - 1) & 0x0F) << 4) | ((totalSamplesPerChannel >> 32) & 0x0F));
        writer.Write(byte3);
        
        // Bytes 4-7: total samples bits 31-0
        WriteBigEndian32(writer, (uint)(totalSamplesPerChannel & 0xFFFFFFFF));
        
        // MD5 signature (16 bytes) - all zeros (not computed)
        for (int i = 0; i < 16; i++)
            writer.Write((byte)0);
    }
    
    private int[] ConvertToIntSamples(float[] floatSamples)
    {
        int maxValue = (1 << (BitsPerSample - 1)) - 1;
        int minValue = -(1 << (BitsPerSample - 1));
        
        var intSamples = new int[floatSamples.Length];
        for (int i = 0; i < floatSamples.Length; i++)
        {
            // Clamp to -1.0 to 1.0
            float clamped = Math.Clamp(floatSamples[i], -1f, 1f);
            // Convert to integer
            int intValue = (int)(clamped * maxValue);
            intSamples[i] = Math.Clamp(intValue, minValue, maxValue);
        }
        return intSamples;
    }
    
    private void WriteAudioFrames(BinaryWriter writer, int[] samples, int totalSamplesPerChannel)
    {
        int samplesProcessed = 0;
        int frameNumber = 0;
        
        while (samplesProcessed < totalSamplesPerChannel)
        {
            int blockSize = Math.Min(DefaultBlockSize, totalSamplesPerChannel - samplesProcessed);
            
            // Extract block samples
            var blockSamples = new int[blockSize * Channels];
            int srcOffset = samplesProcessed * Channels;
            int copyLength = Math.Min(blockSamples.Length, samples.Length - srcOffset);
            if (copyLength > 0)
            {
                Array.Copy(samples, srcOffset, blockSamples, 0, copyLength);
            }
            
            WriteFrame(writer, blockSamples, blockSize, frameNumber);
            
            samplesProcessed += blockSize;
            frameNumber++;
        }
    }
    
    private void WriteFrame(BinaryWriter writer, int[] samples, int blockSize, int frameNumber)
    {
        using var frameData = new MemoryStream();
        using var frameWriter = new BinaryWriter(frameData);
        
        // Frame header
        // Sync code (14 bits) = 0x3FFE, reserved (1 bit) = 0, blocking strategy (1 bit) = 0 (fixed)
        frameWriter.Write((byte)0xFF); // 11111111
        frameWriter.Write((byte)0xF8); // 11111000 (sync + reserved + fixed blocking)
        
        // Block size code (4 bits) + sample rate code (4 bits)
        byte blockSizeCode = GetBlockSizeCode(blockSize);
        byte sampleRateCode = GetSampleRateCode(SampleRate);
        frameWriter.Write((byte)((blockSizeCode << 4) | sampleRateCode));
        
        // Channel assignment (4 bits) + sample size code (3 bits) + reserved (1 bit)
        byte channelCode = (byte)(Channels - 1); // 0 = mono, 1 = stereo
        byte sampleSizeCode = GetSampleSizeCode(BitsPerSample);
        frameWriter.Write((byte)((channelCode << 4) | (sampleSizeCode << 1)));
        
        // Frame number (UTF-8 coded, variable length) - using simple 1-byte for small frame numbers
        if (frameNumber < 128)
        {
            frameWriter.Write((byte)frameNumber);
        }
        else
        {
            // Write as UTF-8 multi-byte
            WriteUtf8Number(frameWriter, frameNumber);
        }
        
        // Block size (if code indicates it follows)
        if (blockSizeCode == 0x06)
        {
            frameWriter.Write((byte)(blockSize - 1)); // 8-bit block size - 1
        }
        else if (blockSizeCode == 0x07)
        {
            WriteBigEndian16(frameWriter, (ushort)(blockSize - 1)); // 16-bit block size - 1
        }
        
        // Sample rate (if code indicates it follows) - we use predefined codes, so skip
        
        // Frame header CRC-8
        var headerBytes = frameData.ToArray();
        byte crc8 = CalculateCrc8(headerBytes);
        frameWriter.Write(crc8);
        
        // Subframes (one per channel)
        for (int ch = 0; ch < Channels; ch++)
        {
            WriteSubframe(frameWriter, samples, ch, blockSize);
        }
        
        // Byte-align
        frameWriter.Flush();
        
        // Frame footer CRC-16
        var allFrameBytes = frameData.ToArray();
        ushort crc16 = CalculateCrc16(allFrameBytes);
        WriteBigEndian16(frameWriter, crc16);
        
        // Write complete frame to output
        writer.Write(frameData.ToArray());
    }
    
    private void WriteSubframe(BinaryWriter writer, int[] samples, int channel, int blockSize)
    {
        // Subframe header: 1 zero bit + 6 bits subframe type + 1 wasted bits flag
        // Type 0 = CONSTANT, Type 1 = VERBATIM, Type 8-12 = FIXED predictor
        // We'll use VERBATIM (type 1) for simplicity
        byte subframeHeader = 0x02; // 0 + 000001 (verbatim) + 0
        writer.Write(subframeHeader);
        
        // Write unencoded samples
        for (int i = 0; i < blockSize; i++)
        {
            int sampleIndex = i * Channels + channel;
            int sample = sampleIndex < samples.Length ? samples[sampleIndex] : 0;
            
            // Write sample as big-endian
            switch (BitsPerSample)
            {
                case 8:
                    writer.Write((byte)(sample & 0xFF));
                    break;
                case 16:
                    WriteBigEndian16(writer, (ushort)sample);
                    break;
                case 24:
                    writer.Write((byte)((sample >> 16) & 0xFF));
                    writer.Write((byte)((sample >> 8) & 0xFF));
                    writer.Write((byte)(sample & 0xFF));
                    break;
            }
        }
    }
    
    private static byte GetBlockSizeCode(int blockSize)
    {
        return blockSize switch
        {
            192 => 0x01,
            576 => 0x02,
            1152 => 0x03,
            2304 => 0x04,
            4608 => 0x05,
            256 => 0x08,
            512 => 0x09,
            1024 => 0x0A,
            2048 => 0x0B,
            4096 => 0x0C,
            8192 => 0x0D,
            16384 => 0x0E,
            32768 => 0x0F,
            _ when blockSize <= 256 => 0x06, // 8-bit follows
            _ => 0x07 // 16-bit follows
        };
    }
    
    private static byte GetSampleRateCode(int sampleRate)
    {
        return sampleRate switch
        {
            88200 => 0x01,
            176400 => 0x02,
            192000 => 0x03,
            8000 => 0x04,
            16000 => 0x05,
            22050 => 0x06,
            24000 => 0x07,
            32000 => 0x08,
            44100 => 0x09,
            48000 => 0x0A,
            96000 => 0x0B,
            _ => 0x00 // Unknown or custom
        };
    }
    
    private static byte GetSampleSizeCode(int bitsPerSample)
    {
        return bitsPerSample switch
        {
            8 => 0x01,
            12 => 0x02,
            16 => 0x04,
            20 => 0x05,
            24 => 0x06,
            _ => 0x00
        };
    }
    
    private static void WriteUtf8Number(BinaryWriter writer, int value)
    {
        if (value < 0x80)
        {
            writer.Write((byte)value);
        }
        else if (value < 0x800)
        {
            writer.Write((byte)(0xC0 | (value >> 6)));
            writer.Write((byte)(0x80 | (value & 0x3F)));
        }
        else if (value < 0x10000)
        {
            writer.Write((byte)(0xE0 | (value >> 12)));
            writer.Write((byte)(0x80 | ((value >> 6) & 0x3F)));
            writer.Write((byte)(0x80 | (value & 0x3F)));
        }
        else
        {
            writer.Write((byte)(0xF0 | (value >> 18)));
            writer.Write((byte)(0x80 | ((value >> 12) & 0x3F)));
            writer.Write((byte)(0x80 | ((value >> 6) & 0x3F)));
            writer.Write((byte)(0x80 | (value & 0x3F)));
        }
    }
    
    private static void WriteBigEndian16(BinaryWriter writer, ushort value)
    {
        writer.Write((byte)(value >> 8));
        writer.Write((byte)(value & 0xFF));
    }
    
    private static void WriteBigEndian32(BinaryWriter writer, uint value)
    {
        writer.Write((byte)((value >> 24) & 0xFF));
        writer.Write((byte)((value >> 16) & 0xFF));
        writer.Write((byte)((value >> 8) & 0xFF));
        writer.Write((byte)(value & 0xFF));
    }
    
    private static byte CalculateCrc8(byte[] data)
    {
        // CRC-8 polynomial: x^8 + x^2 + x^1 + x^0 (0x07)
        byte crc = 0;
        foreach (byte b in data)
        {
            crc ^= b;
            for (int i = 0; i < 8; i++)
            {
                if ((crc & 0x80) != 0)
                    crc = (byte)((crc << 1) ^ 0x07);
                else
                    crc <<= 1;
            }
        }
        return crc;
    }
    
    private static ushort CalculateCrc16(byte[] data)
    {
        // CRC-16 polynomial: x^16 + x^15 + x^2 + x^0 (0x8005), bit-reversed to 0xA001
        ushort crc = 0;
        foreach (byte b in data)
        {
            crc ^= (ushort)(b << 8);
            for (int i = 0; i < 8; i++)
            {
                if ((crc & 0x8000) != 0)
                    crc = (ushort)((crc << 1) ^ 0x8005);
                else
                    crc <<= 1;
            }
        }
        return crc;
    }
}



