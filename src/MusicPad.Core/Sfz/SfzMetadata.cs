using System.Text.RegularExpressions;

namespace MusicPad.Core.Sfz;

/// <summary>
/// Metadata extracted from SFZ file comments.
/// </summary>
public class SfzMetadata
{
    /// <summary>Internal name of the instrument.</summary>
    public string? InternalName { get; set; }
    
    /// <summary>Sound engineer / author credits.</summary>
    public string? SoundEngineer { get; set; }
    
    /// <summary>Original creation date.</summary>
    public string? CreationDate { get; set; }
    
    /// <summary>Source file path (e.g., original SF2).</summary>
    public string? ParentFile { get; set; }
    
    /// <summary>Soundfont version.</summary>
    public string? SoundfontVersion { get; set; }
    
    /// <summary>Editor used to create/edit the soundfont.</summary>
    public string? EditorUsed { get; set; }
    
    /// <summary>Converter tool name.</summary>
    public string? Converter { get; set; }
    
    /// <summary>Converter copyright info.</summary>
    public string? ConverterCopyright { get; set; }
    
    /// <summary>Date of conversion to SFZ.</summary>
    public string? ConversionDate { get; set; }
    
    /// <summary>Hardware/software optimized for.</summary>
    public string? OptimisedFor { get; set; }
    
    /// <summary>Intended hardware/software.</summary>
    public string? IntendedFor { get; set; }

    /// <summary>
    /// Gets a display name for the instrument, using internal name or fallback.
    /// </summary>
    public string GetDisplayName(string fallbackName)
    {
        return !string.IsNullOrWhiteSpace(InternalName) ? InternalName : fallbackName;
    }

    /// <summary>
    /// Gets a formatted credits string combining available metadata.
    /// </summary>
    public string GetCredits()
    {
        var parts = new List<string>();
        
        if (!string.IsNullOrWhiteSpace(SoundEngineer))
            parts.Add($"Sound Engineer: {SoundEngineer}");
        
        if (!string.IsNullOrWhiteSpace(CreationDate))
            parts.Add($"Created: {CreationDate}");
        
        if (!string.IsNullOrWhiteSpace(EditorUsed))
            parts.Add($"Editor: {EditorUsed}");
        
        if (!string.IsNullOrWhiteSpace(Converter))
            parts.Add($"Converter: {Converter}");
        
        if (!string.IsNullOrWhiteSpace(ConverterCopyright))
            parts.Add($"© {ConverterCopyright}");
        
        return string.Join("\n", parts);
    }

    /// <summary>
    /// Parses SFZ content and extracts metadata from comment block.
    /// </summary>
    public static SfzMetadata Parse(string sfzContent)
    {
        var metadata = new SfzMetadata();
        
        // Look for comment block /* ... */
        var commentMatch = Regex.Match(sfzContent, @"/\*(.*?)\*/", RegexOptions.Singleline);
        if (!commentMatch.Success)
            return metadata;
        
        var commentBlock = commentMatch.Groups[1].Value;
        var lines = commentBlock.Split('\n');
        
        foreach (var line in lines)
        {
            var trimmed = line.Trim().TrimStart('/').Trim();
            
            // Parse key: value patterns
            if (TryExtractValue(trimmed, "Internal Name", out var internalName))
                metadata.InternalName = internalName;
            else if (TryExtractValue(trimmed, "Sound Engineer", out var engineer))
                metadata.SoundEngineer = engineer;
            else if (TryExtractValue(trimmed, "Creation Date", out var creationDate))
                metadata.CreationDate = creationDate;
            else if (TryExtractValue(trimmed, "Parent file", out var parentFile))
                metadata.ParentFile = parentFile;
            else if (TryExtractValue(trimmed, "Soundfont", out var soundfont))
                metadata.SoundfontVersion = soundfont;
            else if (TryExtractValue(trimmed, "Editor Used", out var editor))
                metadata.EditorUsed = editor;
            else if (TryExtractValue(trimmed, "Conversion Date", out var convDate))
                metadata.ConversionDate = convDate;
            else if (TryExtractValue(trimmed, "Optimised for", out var optimised))
                metadata.OptimisedFor = optimised;
            else if (TryExtractValue(trimmed, "Intendend for", out var intended)) // Note: typo in original
                metadata.IntendedFor = intended;
            else if (trimmed.StartsWith("Converted with "))
                metadata.Converter = trimmed.Substring("Converted with ".Length).Trim();
            else if (trimmed.StartsWith("Copyright "))
                metadata.ConverterCopyright = trimmed.Substring("Copyright ".Length).Trim();
        }
        
        return metadata;
    }

    private static bool TryExtractValue(string line, string key, out string? value)
    {
        value = null;
        
        // Handle both "Key:" and "Key  :" patterns
        var pattern = $@"^{Regex.Escape(key)}\s*:\s*(.+)$";
        var match = Regex.Match(line, pattern, RegexOptions.IgnoreCase);
        
        if (match.Success)
        {
            value = match.Groups[1].Value.Trim();
            return true;
        }
        
        return false;
    }
}

