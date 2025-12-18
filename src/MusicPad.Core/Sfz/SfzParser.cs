using System.Globalization;
using System.Text.RegularExpressions;

namespace MusicPad.Core.Sfz;

/// <summary>
/// Parser for SFZ instrument definition files.
/// See https://sfzformat.com/ for format specification.
/// </summary>
public static partial class SfzParser
{
    private static readonly Regex HeaderRegex = MyHeaderRegex();
    private static readonly Regex OpcodeRegex = MyOpcodeRegex();

    public static SfzInstrument Parse(string sfzContent, string? name = null, string? basePath = null)
    {
        // Extract metadata from comments BEFORE removing them
        var metadata = SfzMetadata.Parse(sfzContent);
        
        var instrument = new SfzInstrument
        {
            Name = name ?? "Untitled",
            BasePath = basePath ?? string.Empty,
            Metadata = metadata
        };

        // Remove comments
        var content = RemoveComments(sfzContent);

        // State for inheritance
        var globalSettings = new Dictionary<string, string>();
        var groupSettings = new Dictionary<string, string>();
        SfzRegion? currentRegion = null;
        string currentHeader = "";

        // Split content into lines for processing
        var lines = content.Split('\n');
        
        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (string.IsNullOrEmpty(line))
                continue;

            // Process line character by character to handle headers and opcodes on same line
            var remaining = line;
            while (!string.IsNullOrEmpty(remaining))
            {
                remaining = remaining.TrimStart();
                if (string.IsNullOrEmpty(remaining))
                    break;

                // Check for header
                var headerMatch = HeaderRegex.Match(remaining);
                if (headerMatch.Success && headerMatch.Index == 0)
                {
                    var header = headerMatch.Groups[1].Value.ToLowerInvariant();
                    
                    // Save current region if any
                    if (currentRegion != null && currentHeader == "region")
                    {
                        instrument.Regions.Add(currentRegion);
                    }

                    currentHeader = header;
                    
                    switch (header)
                    {
                        case "global":
                            globalSettings.Clear();
                            break;
                        case "group":
                        case "master":
                            groupSettings.Clear();
                            break;
                        case "region":
                            currentRegion = CreateRegion(globalSettings, groupSettings);
                            break;
                    }

                    remaining = remaining.Substring(headerMatch.Length);
                    continue;
                }

                // Check for opcode
                var opcodeMatch = OpcodeRegex.Match(remaining);
                if (opcodeMatch.Success && opcodeMatch.Index == 0)
                {
                    var opcode = opcodeMatch.Groups[1].Value.ToLowerInvariant();
                    var value = opcodeMatch.Groups[2].Value;

                    switch (currentHeader)
                    {
                        case "global":
                            globalSettings[opcode] = value;
                            if (opcode == "sample")
                                instrument.DefaultSample = value;
                            break;
                        case "group":
                        case "master":
                            groupSettings[opcode] = value;
                            break;
                        case "region":
                            if (currentRegion != null)
                                ApplyOpcode(currentRegion, opcode, value);
                            break;
                    }

                    remaining = remaining.Substring(opcodeMatch.Length);
                    continue;
                }

                // Skip unknown content
                var nextSpace = remaining.IndexOf(' ');
                if (nextSpace > 0)
                    remaining = remaining.Substring(nextSpace);
                else
                    break;
            }
        }

        // Don't forget the last region
        if (currentRegion != null && currentHeader == "region")
        {
            instrument.Regions.Add(currentRegion);
        }

        return instrument;
    }

    public static SfzInstrument ParseFile(string filePath)
    {
        var content = File.ReadAllText(filePath);
        var name = Path.GetFileNameWithoutExtension(filePath);
        var basePath = Path.GetDirectoryName(filePath) ?? string.Empty;
        return Parse(content, name, basePath);
    }

    private static string RemoveComments(string content)
    {
        // Remove block comments /* ... */
        content = Regex.Replace(content, @"/\*.*?\*/", "", RegexOptions.Singleline);
        
        // Remove line comments // ...
        content = Regex.Replace(content, @"//.*$", "", RegexOptions.Multiline);
        
        return content;
    }

    private static SfzRegion CreateRegion(Dictionary<string, string> globalSettings, Dictionary<string, string> groupSettings)
    {
        var region = new SfzRegion();
        
        // Apply global settings first (except sample - that goes to instrument.DefaultSample)
        foreach (var (opcode, value) in globalSettings)
        {
            if (opcode != "sample")
                ApplyOpcode(region, opcode, value);
        }
        
        // Apply group settings (overrides global, except sample)
        foreach (var (opcode, value) in groupSettings)
        {
            if (opcode != "sample")
                ApplyOpcode(region, opcode, value);
        }
        
        return region;
    }

    private static void ApplyOpcode(SfzRegion region, string opcode, string value)
    {
        switch (opcode)
        {
            // Sample
            case "sample":
                region.Sample = value;
                break;
            case "offset":
                region.Offset = ParseInt(value);
                break;
            case "end":
                region.End = ParseInt(value);
                break;
                
            // Key range
            case "lokey":
                region.LoKey = ParseInt(value);
                break;
            case "hikey":
                region.HiKey = ParseInt(value);
                break;
            case "key":
                region.Key = ParseInt(value);
                break;
            case "pitch_keycenter":
                region.PitchKeycenter = ParseInt(value);
                break;
                
            // Velocity range
            case "lovel":
                region.LoVel = ParseInt(value);
                break;
            case "hivel":
                region.HiVel = ParseInt(value);
                break;
                
            // Loop
            case "loop_mode":
                region.LoopMode = ParseLoopMode(value);
                break;
            case "loop_start":
                region.LoopStart = ParseInt(value);
                break;
            case "loop_end":
                region.LoopEnd = ParseInt(value);
                break;
                
            // Tuning
            case "tune":
                region.Tune = ParseInt(value);
                break;
            case "transpose":
                region.Transpose = ParseInt(value);
                break;
                
            // Volume and pan
            case "volume":
                region.Volume = ParseFloat(value);
                break;
            case "pan":
                region.Pan = ParseFloat(value);
                break;
                
            // Envelope
            case "ampeg_attack":
                region.AmpegAttack = ParseFloat(value);
                break;
            case "ampeg_hold":
                region.AmpegHold = ParseFloat(value);
                break;
            case "ampeg_decay":
                region.AmpegDecay = ParseFloat(value);
                break;
            case "ampeg_sustain":
                region.AmpegSustain = ParseFloat(value);
                break;
            case "ampeg_release":
                region.AmpegRelease = ParseFloat(value);
                break;
                
            // Label
            case "region_label":
                region.RegionLabel = value;
                break;
        }
    }

    private static int ParseInt(string value)
    {
        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
            return result;
        
        // Try parsing as float and convert
        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var floatResult))
            return (int)floatResult;
            
        return 0;
    }

    private static float ParseFloat(string value)
    {
        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
            return result;
        return 0f;
    }

    private static LoopMode ParseLoopMode(string value)
    {
        return value.ToLowerInvariant() switch
        {
            "loop_continuous" => LoopMode.LoopContinuous,
            "loop_sustain" => LoopMode.LoopSustain,
            "one_shot" => LoopMode.OneShot,
            "no_loop" => LoopMode.NoLoop,
            _ => LoopMode.NoLoop
        };
    }

    [GeneratedRegex(@"<(\w+)>")]
    private static partial Regex MyHeaderRegex();
    
    [GeneratedRegex(@"(\w+)=([^\s<]+)")]
    private static partial Regex MyOpcodeRegex();
}

