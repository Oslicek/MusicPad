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
    /// Metadata extracted from SFZ file comments (credits, author, etc.).
    /// </summary>
    public SfzMetadata Metadata { get; set; } = new();
    
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
    /// Gets a list of unique MIDI notes that have regions assigned, sorted ascending.
    /// This is useful for unpitched instruments where each pad corresponds to a distinct note.
    /// </summary>
    public List<int> GetUniqueMidiNotes()
    {
        var notes = new HashSet<int>();
        
        foreach (var region in Regions)
        {
            if (region.Key.HasValue)
            {
                notes.Add(region.Key.Value);
            }
            else
            {
                // For regions with ranges, add all notes in the range
                for (int note = region.LoKey; note <= region.HiKey; note++)
                {
                    notes.Add(note);
                }
            }
        }
        
        return notes.OrderBy(n => n).ToList();
    }

    /// <summary>
    /// Gets a human-readable label for a MIDI note based on the region that covers it.
    /// Returns (in priority order): RegionLabel, sample filename without extension, or note name.
    /// </summary>
    public string GetRegionLabel(int midiNote)
    {
        // Find the first region that matches this note
        var region = Regions.FirstOrDefault(r => r.Matches(midiNote, 100));
        
        if (region != null)
        {
            // Priority 1: Use explicit region label
            if (!string.IsNullOrEmpty(region.RegionLabel))
            {
                return region.RegionLabel;
            }
            
            // Priority 2: Use sample filename without extension
            if (!string.IsNullOrEmpty(region.Sample))
            {
                var fileName = Path.GetFileNameWithoutExtension(region.Sample);
                if (!string.IsNullOrEmpty(fileName))
                {
                    return fileName;
                }
            }
        }
        
        // Priority 3: Fall back to note name (C4, D#3, etc.)
        return MidiNoteToName(midiNote);
    }

    /// <summary>
    /// Converts a MIDI note number to a note name (e.g., 60 -> "C4").
    /// </summary>
    private static string MidiNoteToName(int midiNote)
    {
        var noteNames = new[] { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
        int octave = (midiNote / 12) - 1;
        int noteIndex = midiNote % 12;
        return $"{noteNames[noteIndex]}{octave}";
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

