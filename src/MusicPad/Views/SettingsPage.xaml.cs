using MusicPad.Services;

namespace MusicPad.Views;

public partial class SettingsPage : ContentPage
{
    private readonly ISettingsService _settingsService;
    private bool _isInitializing = true;
    
    public SettingsPage(ISettingsService settingsService)
    {
        InitializeComponent();
        _settingsService = settingsService;
        
        // Set initial toggle states
        PianoGlowSwitch.IsToggled = _settingsService.PianoKeyGlowEnabled;
        PadGlowSwitch.IsToggled = _settingsService.PadGlowEnabled;
        
        _isInitializing = false;
    }
    
    private void OnPianoGlowToggled(object? sender, ToggledEventArgs e)
    {
        if (_isInitializing) return;
        _settingsService.PianoKeyGlowEnabled = e.Value;
    }
    
    private void OnPadGlowToggled(object? sender, ToggledEventArgs e)
    {
        if (_isInitializing) return;
        _settingsService.PadGlowEnabled = e.Value;
    }
    
    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}

