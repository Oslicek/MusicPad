using MusicPad.Core.Models;
using MusicPad.Core.Theme;

namespace MusicPad.Services;

/// <summary>
/// Service for managing application settings with MAUI Preferences persistence.
/// </summary>
public class SettingsService : ISettingsService
{
    private const string PianoKeyGlowKey = "PianoKeyGlowEnabled";
    private const string PadGlowKey = "PadGlowEnabled";
    private const string SelectedPaletteKey = "SelectedPalette";
    
    private readonly AppSettings _settings = new();
    private string _selectedPalette = "Default";
    
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
    
    public string SelectedPalette
    {
        get => _selectedPalette;
        set
        {
            if (_selectedPalette != value)
            {
                _selectedPalette = value;
                // Apply the palette change immediately
                PaletteService.Instance.SetPaletteByName(value);
                Save();
            }
        }
    }
    
    public void Save()
    {
        Preferences.Set(PianoKeyGlowKey, _settings.PianoKeyGlowEnabled);
        Preferences.Set(PadGlowKey, _settings.PadGlowEnabled);
        Preferences.Set(SelectedPaletteKey, _selectedPalette);
    }
    
    public void Load()
    {
        _settings.PianoKeyGlowEnabled = Preferences.Get(PianoKeyGlowKey, true);
        _settings.PadGlowEnabled = Preferences.Get(PadGlowKey, true);
        _selectedPalette = Preferences.Get(SelectedPaletteKey, "Default");
        
        // Apply the loaded palette
        PaletteService.Instance.SetPaletteByName(_selectedPalette);
    }
}

