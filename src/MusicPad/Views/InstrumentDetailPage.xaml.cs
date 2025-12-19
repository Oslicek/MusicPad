using MusicPad.Core.Models;
using MusicPad.Core.Sfz;
using MusicPad.Core.Theme;
using MusicPad.Services;

namespace MusicPad.Views;

/// <summary>
/// Page displaying detailed information about an instrument.
/// </summary>
[QueryProperty(nameof(InstrumentName), "name")]
public partial class InstrumentDetailPage : ContentPage
{
    private readonly ISfzService _sfzService;
    private readonly IInstrumentConfigService _configService;
    private string _instrumentName = "";
    private SfzInstrument? _instrument;
    private InstrumentConfig? _config;

    /// <summary>
    /// The instrument name, set via query parameter.
    /// </summary>
    public string InstrumentName
    {
        get => _instrumentName;
        set
        {
            _instrumentName = Uri.UnescapeDataString(value ?? "");
            InstrumentNameLabel.Text = _instrumentName;
            _ = LoadInstrumentAsync();
        }
    }

    /// <summary>
    /// Event fired when user selects this instrument.
    /// </summary>
    public event EventHandler<string>? InstrumentSelected;

    public InstrumentDetailPage(ISfzService sfzService, IInstrumentConfigService configService)
    {
        InitializeComponent();
        _sfzService = sfzService;
        _configService = configService;
        RefreshPageColors();
    }
    
    protected override void OnAppearing()
    {
        base.OnAppearing();
        RefreshPageColors();
    }
    
    private void RefreshPageColors()
    {
        // Get colors from current palette
        var bgPage = Color.FromArgb(AppColors.BackgroundPage);
        var surface = Color.FromArgb(AppColors.Surface);
        var textPrimary = Color.FromArgb(AppColors.TextPrimary);
        
        // Page background
        BackgroundColor = bgPage;
        
        // Header bar
        HeaderBar.BackgroundColor = surface;
        BackArrow.TextColor = textPrimary;
        HeaderLabel.TextColor = textPrimary;
        InstrumentNameLabel.TextColor = textPrimary;
    }
    
    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private async Task LoadInstrumentAsync()
    {
        try
        {
            // Load the instrument to get its metadata
            await _sfzService.LoadInstrumentAsync(_instrumentName);
            _instrument = _sfzService.CurrentInstrument;
            
            if (_instrument != null)
            {
                DisplayInstrumentInfo(_instrument);
            }
            
            // Load config to get settings
            await LoadConfigAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load instrument: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
            ContentContainer.IsVisible = true;
        }
    }
    
    private async Task LoadConfigAsync()
    {
        // Find the config file for this instrument by display name
        var allConfigs = await _configService.GetAllInstrumentsAsync();
        _config = allConfigs.FirstOrDefault(c => c.DisplayName == _instrumentName);
        
        // For bundled instruments, check for settings override
        if (_config != null && _config.IsBundled)
        {
            var overrideSettings = await _configService.GetBundledSettingsOverrideAsync(_instrumentName);
            if (overrideSettings.HasValue)
            {
                _config.Voicing = overrideSettings.Value.voicing;
                _config.PitchType = overrideSettings.Value.pitchType;
            }
        }
        
        UpdateSettingsUI();
    }
    
    private void UpdateSettingsUI()
    {
        var selectedColor = Color.FromArgb(AppColors.Secondary);
        var unselectedColor = Color.FromArgb(AppColors.Surface);
        var selectedTextColor = Color.FromArgb(AppColors.TextDark);
        var unselectedTextColor = Color.FromArgb(AppColors.TextPrimary);
        
        var isPolyphonic = _config?.Voicing == VoicingType.Polyphonic;
        PolyphonicBtn.BackgroundColor = isPolyphonic ? selectedColor : unselectedColor;
        PolyphonicBtn.TextColor = isPolyphonic ? selectedTextColor : unselectedTextColor;
        MonophonicBtn.BackgroundColor = !isPolyphonic ? selectedColor : unselectedColor;
        MonophonicBtn.TextColor = !isPolyphonic ? selectedTextColor : unselectedTextColor;
        
        var isPitched = _config?.PitchType == PitchType.Pitched;
        PitchedBtn.BackgroundColor = isPitched ? selectedColor : unselectedColor;
        PitchedBtn.TextColor = isPitched ? selectedTextColor : unselectedTextColor;
        UnpitchedBtn.BackgroundColor = !isPitched ? selectedColor : unselectedColor;
        UnpitchedBtn.TextColor = !isPitched ? selectedTextColor : unselectedTextColor;
    }
    
    private async void OnPolyphonicClicked(object? sender, EventArgs e)
    {
        if (_config == null) return;
        _config.Voicing = VoicingType.Polyphonic;
        await SaveSettingsAsync();
        UpdateSettingsUI();
    }
    
    private async void OnMonophonicClicked(object? sender, EventArgs e)
    {
        if (_config == null) return;
        _config.Voicing = VoicingType.Monophonic;
        await SaveSettingsAsync();
        UpdateSettingsUI();
    }
    
    private async void OnPitchedClicked(object? sender, EventArgs e)
    {
        if (_config == null) return;
        _config.PitchType = PitchType.Pitched;
        await SaveSettingsAsync();
        UpdateSettingsUI();
    }
    
    private async void OnUnpitchedClicked(object? sender, EventArgs e)
    {
        if (_config == null) return;
        _config.PitchType = PitchType.Unpitched;
        await SaveSettingsAsync();
        UpdateSettingsUI();
    }
    
    private async Task SaveSettingsAsync()
    {
        if (_config == null) return;
        
        if (_config.IsBundled)
        {
            // Save as override for bundled instruments
            await _configService.SaveBundledSettingsOverrideAsync(
                _instrumentName, _config.Voicing, _config.PitchType);
        }
        else
        {
            // Save directly for user instruments
            await _configService.SaveInstrumentAsync(_config);
        }
    }

    private void DisplayInstrumentInfo(SfzInstrument instrument)
    {
        var metadata = instrument.Metadata;
        
        // Credits section
        SoundEngineerLabel.Text = metadata.SoundEngineer ?? "Unknown";
        CreationDateLabel.Text = metadata.CreationDate ?? "Unknown";
        EditorLabel.Text = metadata.EditorUsed ?? "Unknown";
        
        // Technical info
        InternalNameLabel.Text = metadata.InternalName ?? instrument.Name;
        SourceFileLabel.Text = GetFileName(metadata.ParentFile) ?? "Unknown";
        SoundfontVersionLabel.Text = metadata.SoundfontVersion ?? "Unknown";
        
        var (minKey, maxKey) = instrument.GetKeyRange();
        KeyRangeLabel.Text = $"{GetNoteName(minKey)} ({minKey}) - {GetNoteName(maxKey)} ({maxKey})";
        RegionCountLabel.Text = instrument.Regions.Count.ToString();
        
        // Conversion info
        ConverterLabel.Text = metadata.Converter ?? "Unknown";
        ConversionDateLabel.Text = metadata.ConversionDate ?? "Unknown";
        CopyrightLabel.Text = metadata.ConverterCopyright ?? "Unknown";
    }

    private static string? GetFileName(string? path)
    {
        if (string.IsNullOrEmpty(path))
            return null;
        
        return Path.GetFileName(path);
    }

    private static string GetNoteName(int midiNote)
    {
        var noteNames = new[] { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
        var octave = (midiNote / 12) - 1;
        var note = noteNames[midiNote % 12];
        return $"{note}{octave}";
    }

    private async void OnSelectClicked(object? sender, EventArgs e)
    {
        // Fire event for parent to handle
        InstrumentSelected?.Invoke(this, _instrumentName);
        
        // Navigate back to main page
        await Shell.Current.GoToAsync("//MainPage");
    }
}

