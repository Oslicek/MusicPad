using MusicPad.Core.Theme;
using MusicPad.Services;

namespace MusicPad.Views;

public partial class SettingsPage : ContentPage
{
    private readonly ISettingsService _settingsService;
    private bool _isInitializing = true;
    private readonly List<string> _paletteNames;
    
    public SettingsPage(ISettingsService settingsService)
    {
        InitializeComponent();
        _settingsService = settingsService;
        
        // Populate palette picker
        _paletteNames = PaletteService.AvailablePalettes.Select(p => p.Name).ToList();
        foreach (var name in _paletteNames)
        {
            PalettePicker.Items.Add(name);
        }
        
        // Set initial toggle states
        PianoGlowSwitch.IsToggled = _settingsService.PianoKeyGlowEnabled;
        PadGlowSwitch.IsToggled = _settingsService.PadGlowEnabled;
        
        // Set initial palette selection
        var currentPaletteIndex = _paletteNames.IndexOf(_settingsService.SelectedPalette);
        if (currentPaletteIndex >= 0)
        {
            PalettePicker.SelectedIndex = currentPaletteIndex;
        }
        else
        {
            PalettePicker.SelectedIndex = 0; // Default
        }
        
        // Apply current palette colors
        RefreshPageColors();
        
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
    
    private void OnPaletteChanged(object? sender, EventArgs e)
    {
        if (_isInitializing) return;
        
        if (PalettePicker.SelectedIndex >= 0 && PalettePicker.SelectedIndex < _paletteNames.Count)
        {
            var selectedPalette = _paletteNames[PalettePicker.SelectedIndex];
            _settingsService.SelectedPalette = selectedPalette;
            
            // Refresh page colors immediately
            RefreshPageColors();
        }
    }
    
    private void RefreshPageColors()
    {
        // Get all colors from current palette
        var bgPage = Color.FromArgb(AppColors.BackgroundPage);
        var surface = Color.FromArgb(AppColors.Surface);
        var surfaceBorder = Color.FromArgb(AppColors.SurfaceBorder);
        var textPrimary = Color.FromArgb(AppColors.TextPrimary);
        var textSecondary = Color.FromArgb(AppColors.TextSecondary);
        var textMuted = Color.FromArgb(AppColors.TextMuted);
        var textWhite = Color.FromArgb(AppColors.TextWhite);
        var primary = Color.FromArgb(AppColors.Primary);
        var secondary = Color.FromArgb(AppColors.Secondary);
        var bgPicker = Color.FromArgb(AppColors.BackgroundPicker);
        
        // Page background
        BackgroundColor = bgPage;
        
        // Header
        HeaderLabel.TextColor = textPrimary;
        
        // Visual Effects Frame
        VisualEffectsFrame.BackgroundColor = surface;
        VisualEffectsFrame.BorderColor = surfaceBorder;
        VisualEffectsTitle.TextColor = textSecondary;
        PianoGlowLabel.TextColor = textPrimary;
        PianoGlowDescription.TextColor = textMuted;
        PianoGlowSwitch.OnColor = primary;
        PianoGlowSwitch.ThumbColor = secondary;
        Divider1.Color = surfaceBorder;
        PadGlowLabel.TextColor = textPrimary;
        PadGlowDescription.TextColor = textMuted;
        PadGlowSwitch.OnColor = primary;
        PadGlowSwitch.ThumbColor = secondary;
        
        // Appearance Frame
        AppearanceFrame.BackgroundColor = surface;
        AppearanceFrame.BorderColor = surfaceBorder;
        AppearanceTitle.TextColor = textSecondary;
        PaletteLabel.TextColor = textPrimary;
        PaletteDescription.TextColor = textMuted;
        PalettePicker.BackgroundColor = bgPicker;
        PalettePicker.TextColor = textPrimary;
        
        // Header bar
        HeaderBar.BackgroundColor = surface;
        BackArrow.TextColor = textPrimary;
        HeaderLabel.TextColor = textPrimary;
    }
    
    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}

