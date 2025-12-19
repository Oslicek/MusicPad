using System.Text.Json;
using System.Text.RegularExpressions;
using MusicPad.Core.Models;

namespace MusicPad.Services;

/// <summary>
/// Service for managing instrument configurations with support for bundled and user-imported instruments.
/// </summary>
public class InstrumentConfigService : IInstrumentConfigService
{
    private const string BundledConfigsFolder = "instruments";
    private const string UserInstrumentsFolder = "instruments";
    private const string OrderFileName = "instrument-order.json";
    private const string UserOrderFileName = "user-instrument-order.json";
    private const string BundledSettingsOverrideFolder = "bundled-settings-overrides";
    
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };
    
    private readonly string _userInstrumentsPath;
    
    public InstrumentConfigService()
    {
        _userInstrumentsPath = Path.Combine(FileSystem.AppDataDirectory, UserInstrumentsFolder);
        EnsureUserDirectoryExists();
    }
    
    private void EnsureUserDirectoryExists()
    {
        if (!Directory.Exists(_userInstrumentsPath))
        {
            Directory.CreateDirectory(_userInstrumentsPath);
        }
    }
    
    public async Task<List<InstrumentConfig>> GetAllInstrumentsAsync()
    {
        // Load all available instruments
        var userInstruments = await GetUserInstrumentsAsync();
        var bundledInstruments = await GetBundledInstrumentsAsync();
        
        // Create a lookup by filename
        var allByFileName = new Dictionary<string, InstrumentConfig>();
        foreach (var inst in userInstruments)
            allByFileName[inst.FileName] = inst;
        foreach (var inst in bundledInstruments)
            allByFileName[inst.FileName] = inst;
        
        // Check for saved unified order
        var unifiedOrderPath = Path.Combine(_userInstrumentsPath, "unified-order.json");
        if (File.Exists(unifiedOrderPath))
        {
            try
            {
                var json = await File.ReadAllTextAsync(unifiedOrderPath);
                var data = JsonSerializer.Deserialize<OrderFile>(json, JsonOptions);
                if (data?.Order != null && data.Order.Count > 0)
                {
                    var result = new List<InstrumentConfig>();
                    var remaining = new HashSet<string>(allByFileName.Keys);
                    
                    // Add in saved order
                    foreach (var fileName in data.Order)
                    {
                        if (allByFileName.TryGetValue(fileName, out var config))
                        {
                            result.Add(config);
                            remaining.Remove(fileName);
                        }
                    }
                    
                    // Add any new instruments at the end
                    foreach (var fileName in remaining.OrderBy(f => f))
                    {
                        result.Add(allByFileName[fileName]);
                    }
                    
                    return result;
                }
            }
            catch { }
        }
        
        // Default: user instruments first, then bundled
        var defaultResult = new List<InstrumentConfig>();
        defaultResult.AddRange(userInstruments);
        defaultResult.AddRange(bundledInstruments);
        return defaultResult;
    }
    
    public async Task<List<InstrumentConfig>> GetUserInstrumentsAsync()
    {
        var result = new List<InstrumentConfig>();
        var order = await GetUserOrderAsync();
        
        // List of files to exclude (system files, not instrument configs)
        var excludedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            UserOrderFileName,
            "bundled-order-override.json",
            "unified-order.json"
        };
        
        // Get all user config files (only instrument configs)
        var configFiles = Directory.GetFiles(_userInstrumentsPath, "*.json")
            .Select(Path.GetFileName)
            .Where(f => f != null && !excludedFiles.Contains(f))
            .Cast<string>()
            .ToList();
        
        // Sort by order file, then alphabetically for any not in order
        var ordered = new List<string>();
        foreach (var fileName in order)
        {
            if (configFiles.Contains(fileName))
            {
                ordered.Add(fileName);
                configFiles.Remove(fileName);
            }
        }
        ordered.AddRange(configFiles.OrderBy(f => f));
        
        // Load each config
        foreach (var fileName in ordered)
        {
            var config = await LoadUserConfigAsync(fileName);
            if (config != null)
            {
                config.IsBundled = false;
                config.FileName = fileName; // Set actual filename from disk
                result.Add(config);
            }
        }
        
        return result;
    }
    
    public async Task<List<InstrumentConfig>> GetBundledInstrumentsAsync()
    {
        var result = new List<InstrumentConfig>();
        var order = await GetBundledOrderAsync();
        
        foreach (var fileName in order)
        {
            var config = await LoadBundledConfigAsync(fileName);
            if (config != null)
            {
                config.IsBundled = true;
                config.FileName = fileName; // Set actual filename
                result.Add(config);
            }
        }
        
        return result;
    }
    
    public async Task<InstrumentConfig?> GetInstrumentAsync(string configFileName)
    {
        // Try user instruments first
        var userConfig = await LoadUserConfigAsync(configFileName);
        if (userConfig != null)
        {
            userConfig.IsBundled = false;
            return userConfig;
        }
        
        // Then try bundled
        var bundledConfig = await LoadBundledConfigAsync(configFileName);
        if (bundledConfig != null)
        {
            bundledConfig.IsBundled = true;
            return bundledConfig;
        }
        
        return null;
    }
    
    public async Task SaveInstrumentAsync(InstrumentConfig config)
    {
        if (config.IsBundled)
        {
            throw new InvalidOperationException("Cannot modify bundled instruments.");
        }
        
        // Ensure filename is set
        config.EnsureFileName();
        
        var filePath = Path.Combine(_userInstrumentsPath, config.FileName);
        var json = JsonSerializer.Serialize(config, JsonOptions);
        await File.WriteAllTextAsync(filePath, json);
    }
    
    public async Task<bool> DeleteInstrumentAsync(string configFileName)
    {
        var filePath = Path.Combine(_userInstrumentsPath, configFileName);
        if (!File.Exists(filePath))
        {
            return false;
        }
        
        // Load config to get SFZ path
        var config = await LoadUserConfigAsync(configFileName);
        if (config == null)
        {
            return false;
        }
        
        // Delete the config file
        File.Delete(filePath);
        
        // Delete the SFZ folder if it exists in user storage
        var sfzFolder = Path.GetDirectoryName(config.SfzPath);
        if (!string.IsNullOrEmpty(sfzFolder))
        {
            var userSfzPath = Path.Combine(_userInstrumentsPath, sfzFolder);
            if (Directory.Exists(userSfzPath))
            {
                Directory.Delete(userSfzPath, recursive: true);
            }
        }
        
        // Remove from order
        await RemoveFromUserOrderAsync(configFileName);
        
        return true;
    }
    
    public async Task<string?> RenameInstrumentAsync(string configFileName, string newDisplayName)
    {
        var config = await LoadUserConfigAsync(configFileName);
        if (config == null)
        {
            return null;
        }
        
        // Store old filename (which is the actual file on disk)
        var oldFileName = configFileName;
        config.FileName = configFileName; // Ensure it's set
        
        // Update display name and compute new filename
        config.DisplayName = newDisplayName;
        var newFileName = config.ComputeFileName();
        config.FileName = newFileName; // Set new filename before saving
        
        // Save with new name
        await SaveInstrumentAsync(config);
        
        // If filename changed, delete old file and update order
        if (!oldFileName.Equals(newFileName, StringComparison.OrdinalIgnoreCase))
        {
            var oldPath = Path.Combine(_userInstrumentsPath, oldFileName);
            if (File.Exists(oldPath))
            {
                File.Delete(oldPath);
            }
            
            await UpdateUserOrderFileNameAsync(oldFileName, newFileName);
            return newFileName;
        }
        
        return null;
    }
    
    public async Task SaveOrderAsync(List<string> orderedFileNames)
    {
        // Save unified order (all instruments in one list)
        var unifiedOrderPath = Path.Combine(_userInstrumentsPath, "unified-order.json");
        var unifiedOrderData = new { version = 1, order = orderedFileNames };
        await File.WriteAllTextAsync(unifiedOrderPath, JsonSerializer.Serialize(unifiedOrderData, JsonOptions));
    }
    
    public async Task<List<string>> ImportSfzAsync(string sfzSourcePath, string wavSourcePath, List<InstrumentImportInfo> instruments)
    {
        var createdConfigs = new List<string>();
        
        foreach (var importInfo in instruments)
        {
            // Generate unique filename
            var baseFileName = importInfo.DisplayName;
            var fileName = await GetUniqueFileNameAsync(baseFileName);
            
            // Create folder for the instrument
            var sfzFolderName = Path.GetFileNameWithoutExtension(fileName);
            var sfzFolder = Path.Combine(_userInstrumentsPath, sfzFolderName);
            Directory.CreateDirectory(sfzFolder);
            
            // Copy SFZ file
            var sfzFileName = Path.GetFileName(sfzSourcePath);
            var destSfzPath = Path.Combine(sfzFolder, sfzFileName);
            File.Copy(sfzSourcePath, destSfzPath, overwrite: true);
            
            // Copy the WAV file
            var wavFileName = Path.GetFileName(wavSourcePath);
            var destWavPath = Path.Combine(sfzFolder, wavFileName);
            File.Copy(wavSourcePath, destWavPath, overwrite: true);
            
            // Create config with the unique filename
            var config = new InstrumentConfig
            {
                DisplayName = importInfo.DisplayName,
                SfzPath = $"{sfzFolderName}/{sfzFileName}",
                Voicing = importInfo.Voicing,
                PitchType = importInfo.PitchType,
                IsBundled = false,
                FileName = fileName // Use the unique filename we generated
            };
            
            await SaveInstrumentAsync(config);
            
            // Add to user order
            await AddToUserOrderAsync(config.FileName);
            
            createdConfigs.Add(config.FileName);
        }
        
        return createdConfigs;
    }
    
    public async Task SaveBundledSettingsOverrideAsync(string displayName, VoicingType voicing, PitchType pitchType)
    {
        var overridesFolder = Path.Combine(_userInstrumentsPath, BundledSettingsOverrideFolder);
        if (!Directory.Exists(overridesFolder))
        {
            Directory.CreateDirectory(overridesFolder);
        }
        
        var safeName = Regex.Replace(displayName, @"[<>:""/\\|?*]", "_");
        var filePath = Path.Combine(overridesFolder, $"{safeName}.json");
        
        var overrideData = new
        {
            displayName,
            voicing = voicing.ToString(),
            pitchType = pitchType.ToString()
        };
        
        var json = JsonSerializer.Serialize(overrideData, JsonOptions);
        await File.WriteAllTextAsync(filePath, json);
    }
    
    public async Task<(VoicingType voicing, PitchType pitchType)?> GetBundledSettingsOverrideAsync(string displayName)
    {
        var overridesFolder = Path.Combine(_userInstrumentsPath, BundledSettingsOverrideFolder);
        if (!Directory.Exists(overridesFolder))
        {
            return null;
        }
        
        var safeName = Regex.Replace(displayName, @"[<>:""/\\|?*]", "_");
        var filePath = Path.Combine(overridesFolder, $"{safeName}.json");
        
        if (!File.Exists(filePath))
        {
            return null;
        }
        
        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            
            var voicingStr = root.GetProperty("voicing").GetString() ?? "Polyphonic";
            var pitchTypeStr = root.GetProperty("pitchType").GetString() ?? "Pitched";
            
            var voicing = Enum.TryParse<VoicingType>(voicingStr, out var v) ? v : VoicingType.Polyphonic;
            var pitchType = Enum.TryParse<PitchType>(pitchTypeStr, out var p) ? p : PitchType.Pitched;
            
            return (voicing, pitchType);
        }
        catch
        {
            return null;
        }
    }
    
    public Task<List<SfzInstrumentInfo>> AnalyzeSfzAsync(Stream sfzStream, string fileName)
    {
        // For now, we assume one instrument per SFZ file
        // In the future, we could parse the SFZ and detect multiple instruments
        var result = new List<SfzInstrumentInfo>
        {
            new SfzInstrumentInfo
            {
                SuggestedName = Path.GetFileNameWithoutExtension(fileName),
                Index = 0,
                RegionCount = 0, // Would need actual parsing
                NoteRange = "Unknown"
            }
        };
        
        return Task.FromResult(result);
    }
    
    #region Private Methods
    
    private async Task<InstrumentConfig?> LoadUserConfigAsync(string fileName)
    {
        var filePath = Path.Combine(_userInstrumentsPath, fileName);
        if (!File.Exists(filePath))
        {
            return null;
        }
        
        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<InstrumentConfig>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }
    
    private async Task<InstrumentConfig?> LoadBundledConfigAsync(string fileName)
    {
        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync($"{BundledConfigsFolder}/{fileName}");
            return await JsonSerializer.DeserializeAsync<InstrumentConfig>(stream, JsonOptions);
        }
        catch
        {
            return null;
        }
    }
    
    private async Task<List<string>> GetBundledOrderAsync()
    {
        // Check for user override first
        var overridePath = Path.Combine(_userInstrumentsPath, "bundled-order-override.json");
        if (File.Exists(overridePath))
        {
            try
            {
                var json = await File.ReadAllTextAsync(overridePath);
                var data = JsonSerializer.Deserialize<OrderFile>(json, JsonOptions);
                if (data?.Order != null)
                {
                    return data.Order;
                }
            }
            catch { }
        }
        
        // Fall back to bundled order
        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync($"{BundledConfigsFolder}/{OrderFileName}");
            var data = await JsonSerializer.DeserializeAsync<OrderFile>(stream, JsonOptions);
            return data?.Order ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }
    
    private async Task<List<string>> GetUserOrderAsync()
    {
        var orderPath = Path.Combine(_userInstrumentsPath, UserOrderFileName);
        if (!File.Exists(orderPath))
        {
            return new List<string>();
        }
        
        try
        {
            var json = await File.ReadAllTextAsync(orderPath);
            var data = JsonSerializer.Deserialize<OrderFile>(json, JsonOptions);
            return data?.Order ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }
    
    private async Task AddToUserOrderAsync(string fileName)
    {
        var order = await GetUserOrderAsync();
        if (!order.Contains(fileName))
        {
            order.Insert(0, fileName); // Add at beginning
            var orderPath = Path.Combine(_userInstrumentsPath, UserOrderFileName);
            var data = new { version = 1, order };
            await File.WriteAllTextAsync(orderPath, JsonSerializer.Serialize(data, JsonOptions));
        }
    }
    
    private async Task RemoveFromUserOrderAsync(string fileName)
    {
        var order = await GetUserOrderAsync();
        if (order.Remove(fileName))
        {
            var orderPath = Path.Combine(_userInstrumentsPath, UserOrderFileName);
            var data = new { version = 1, order };
            await File.WriteAllTextAsync(orderPath, JsonSerializer.Serialize(data, JsonOptions));
        }
    }
    
    private async Task UpdateUserOrderFileNameAsync(string oldFileName, string newFileName)
    {
        var order = await GetUserOrderAsync();
        var index = order.IndexOf(oldFileName);
        if (index >= 0)
        {
            order[index] = newFileName;
            var orderPath = Path.Combine(_userInstrumentsPath, UserOrderFileName);
            var data = new { version = 1, order };
            await File.WriteAllTextAsync(orderPath, JsonSerializer.Serialize(data, JsonOptions));
        }
    }
    
    private async Task<string> GetUniqueFileNameAsync(string baseName)
    {
        var safeName = Regex.Replace(baseName, @"[<>:""/\\|?*]", "_");
        var fileName = $"{safeName}.json";
        var filePath = Path.Combine(_userInstrumentsPath, fileName);
        
        if (!File.Exists(filePath))
        {
            return fileName;
        }
        
        // Add number suffix
        var counter = 2;
        while (true)
        {
            fileName = $"{safeName} ({counter}).json";
            filePath = Path.Combine(_userInstrumentsPath, fileName);
            if (!File.Exists(filePath))
            {
                return fileName;
            }
            counter++;
        }
    }
    
    private class OrderFile
    {
        public int Version { get; set; }
        public List<string>? Order { get; set; }
    }
    
    #endregion
}

