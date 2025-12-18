using MusicPad.Core.Sfz;
using MusicPad.Services;

namespace MusicPad.Views;

/// <summary>
/// Page displaying detailed information about an instrument.
/// </summary>
[QueryProperty(nameof(InstrumentName), "name")]
public partial class InstrumentDetailPage : ContentPage
{
    private readonly ISfzService _sfzService;
    private string _instrumentName = "";
    private SfzInstrument? _instrument;

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

    public InstrumentDetailPage(ISfzService sfzService)
    {
        InitializeComponent();
        _sfzService = sfzService;
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
        
        // Update button text if already selected
        if (_sfzService.CurrentInstrumentName == _instrumentName)
        {
            SelectButton.Text = "✓ Currently Selected";
            SelectButton.BackgroundColor = Color.FromArgb("#2A6A3A");
        }
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

