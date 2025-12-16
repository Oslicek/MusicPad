namespace MusicPad.Core.Sfz;

/// <summary>
/// Represents a complete SFZ instrument with all its regions.
/// </summary>
public class SfzInstrument
{
    public string Name { get; set; } = string.Empty;
    public string BasePath { get; set; } = string.Empty;
    public string DefaultSample { get; set; } = string.Empty;
    public List<SfzRegion> Regions { get; } = new();
    
    /// <summary>
    /// Pre-loaded sample data (for instruments loaded into memory).
    /// Key is the sample file path, value is the audio data.
    /// </summary>
    public Dictionary<string, WavData> LoadedSamples { get; } = new();
    
    /// <summary>
    /// Test samples for unit testing (bypasses file loading).
    /// </summary>
    public float[]? TestSamples { get; set; }

    /// <summary>
    /// Finds all regions that match a given MIDI note and velocity.
    /// </summary>
    public IEnumerable<SfzRegion> FindRegions(int midiNote, int velocity = 100)
    {
        return Regions.Where(r => r.Matches(midiNote, velocity));
    }

    /// <summary>
    /// Gets the key range of the instrument.
    /// </summary>
    public (int minKey, int maxKey) GetKeyRange()
    {
        if (Regions.Count == 0)
            return (0, 127);

        int min = Regions.Min(r => r.Key ?? r.LoKey);
        int max = Regions.Max(r => r.Key ?? r.HiKey);
        return (min, max);
    }

    /// <summary>
    /// Gets the middle key of the instrument's range.
    /// </summary>
    public int GetMiddleKey()
    {
        var (min, max) = GetKeyRange();
        return (min + max) / 2;
    }

    /// <summary>
    /// Gets the full path to a sample file.
    /// </summary>
    public string GetSamplePath(SfzRegion region)
    {
        var sample = string.IsNullOrEmpty(region.Sample) ? DefaultSample : region.Sample;
        if (string.IsNullOrEmpty(sample))
            return string.Empty;
        
        return Path.Combine(BasePath, sample);
    }
}

