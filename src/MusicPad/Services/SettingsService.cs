using MusicPad.Core.Models;

namespace MusicPad.Services;

/// <summary>
/// Service for managing application settings with MAUI Preferences persistence.
/// </summary>
public class SettingsService : ISettingsService
{
    private const string PianoKeyGlowKey = "PianoKeyGlowEnabled";
    private const string PadGlowKey = "PadGlowEnabled";
    
    private readonly AppSettings _settings = new();
    
    public SettingsService()
    {
        Load();
    }
    
    public AppSettings Settings => _settings;
    
    public bool PianoKeyGlowEnabled
    {
        get => _settings.PianoKeyGlowEnabled;
        set
        {
            _settings.PianoKeyGlowEnabled = value;
            Save();
        }
    }
    
    public bool PadGlowEnabled
    {
        get => _settings.PadGlowEnabled;
        set
        {
            _settings.PadGlowEnabled = value;
            Save();
        }
    }
    
    public void Save()
    {
        Preferences.Set(PianoKeyGlowKey, _settings.PianoKeyGlowEnabled);
        Preferences.Set(PadGlowKey, _settings.PadGlowEnabled);
    }
    
    public void Load()
    {
        _settings.PianoKeyGlowEnabled = Preferences.Get(PianoKeyGlowKey, true);
        _settings.PadGlowEnabled = Preferences.Get(PadGlowKey, true);
    }
}

