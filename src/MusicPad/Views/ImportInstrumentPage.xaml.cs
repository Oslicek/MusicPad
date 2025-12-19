using MusicPad.Core.Models;
using MusicPad.Core.Theme;
using MusicPad.Services;

namespace MusicPad.Views;

/// <summary>
/// Page for importing SFZ instrument files.
/// </summary>
public partial class ImportInstrumentPage : ContentPage
{
    private readonly IInstrumentConfigService _configService;
    private string? _selectedSfzPath;
    private string? _selectedWavPath;
    private List<SfzInstrumentInfo> _detectedInstruments = new();
    private readonly Dictionary<int, (CheckBox checkbox, Entry nameEntry)> _instrumentControls = new();

    public ImportInstrumentPage(IInstrumentConfigService configService)
    {
        InitializeComponent();
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
    }

    private async void OnBrowseSfzClicked(object? sender, EventArgs e)
    {
        try
        {
            var options = new PickOptions
            {
                PickerTitle = "Select SFZ Instrument File",
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.Android, new[] { "application/octet-stream", "*/*" } },
                    { DevicePlatform.iOS, new[] { "public.data" } },
                    { DevicePlatform.WinUI, new[] { ".sfz" } }
                })
            };

            var result = await FilePicker.Default.PickAsync(options);
            if (result != null)
            {
                _selectedSfzPath = result.FullPath;
                SelectedSfzFileLabel.Text = result.FileName;
                SelectedSfzFileLabel.TextColor = Color.FromArgb(AppColors.TextPrimary);
                
                // Analyze the SFZ file
                await AnalyzeSfzFileAsync(result);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to pick SFZ file: {ex.Message}", "OK");
        }
    }
    
    private async void OnBrowseWavClicked(object? sender, EventArgs e)
    {
        try
        {
            var options = new PickOptions
            {
                PickerTitle = "Select WAV Sample File",
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.Android, new[] { "audio/wav", "audio/x-wav", "*/*" } },
                    { DevicePlatform.iOS, new[] { "public.audio" } },
                    { DevicePlatform.WinUI, new[] { ".wav" } }
                })
            };

            var result = await FilePicker.Default.PickAsync(options);
            if (result != null)
            {
                _selectedWavPath = result.FullPath;
                SelectedWavFileLabel.Text = result.FileName;
                SelectedWavFileLabel.TextColor = Color.FromArgb(AppColors.TextPrimary);
                
                UpdateImportButtonState();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to pick WAV file: {ex.Message}", "OK");
        }
    }

    private async Task AnalyzeSfzFileAsync(FileResult fileResult)
    {
        try
        {
            using var stream = await fileResult.OpenReadAsync();
            _detectedInstruments = await _configService.AnalyzeSfzAsync(stream, fileResult.FileName);
            
            BuildInstrumentCheckboxList();
            
            InstrumentsFrame.IsVisible = true;
            SettingsFrame.IsVisible = true;
            UpdateImportButtonState();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to analyze SFZ file: {ex.Message}", "OK");
        }
    }

    private void BuildInstrumentCheckboxList()
    {
        InstrumentCheckboxList.Children.Clear();
        _instrumentControls.Clear();
        
        foreach (var instrument in _detectedInstruments)
        {
            var grid = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition(GridLength.Auto),
                    new ColumnDefinition(GridLength.Star)
                },
                Margin = new Thickness(0, 4)
            };
            
            var checkbox = new CheckBox
            {
                IsChecked = true,
                Color = Color.FromArgb(AppColors.Secondary)
            };
            checkbox.CheckedChanged += (s, e) => UpdateImportButtonState();
            grid.Add(checkbox, 0);
            
            var nameEntry = new Entry
            {
                Text = instrument.SuggestedName,
                FontSize = 14,
                TextColor = Color.FromArgb(AppColors.TextPrimary),
                BackgroundColor = Color.FromArgb(AppColors.BackgroundPicker),
                Placeholder = "Instrument name",
                PlaceholderColor = Color.FromArgb(AppColors.TextDim)
            };
            grid.Add(nameEntry, 1);
            
            InstrumentCheckboxList.Children.Add(grid);
            _instrumentControls[instrument.Index] = (checkbox, nameEntry);
        }
    }

    private void UpdateImportButtonState()
    {
        var anySelected = _instrumentControls.Values.Any(c => c.checkbox.IsChecked);
        var bothFilesSelected = !string.IsNullOrEmpty(_selectedSfzPath) && !string.IsNullOrEmpty(_selectedWavPath);
        ImportButton.IsEnabled = anySelected && bothFilesSelected;
    }

    private async void OnImportClicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_selectedSfzPath))
        {
            await DisplayAlert("Error", "No SFZ file selected", "OK");
            return;
        }
        
        if (string.IsNullOrEmpty(_selectedWavPath))
        {
            await DisplayAlert("Error", "No WAV file selected", "OK");
            return;
        }
        
        var voicing = PolyphonicRadio.IsChecked ? VoicingType.Polyphonic : VoicingType.Monophonic;
        var pitchType = PitchedRadio.IsChecked ? PitchType.Pitched : PitchType.Unpitched;
        
        var instrumentsToImport = new List<InstrumentImportInfo>();
        
        foreach (var instrument in _detectedInstruments)
        {
            if (_instrumentControls.TryGetValue(instrument.Index, out var controls))
            {
                if (controls.checkbox.IsChecked)
                {
                    var displayName = controls.nameEntry.Text?.Trim();
                    if (string.IsNullOrEmpty(displayName))
                    {
                        displayName = instrument.SuggestedName;
                    }
                    
                    instrumentsToImport.Add(new InstrumentImportInfo
                    {
                        DisplayName = displayName,
                        InstrumentIndex = instrument.Index,
                        Voicing = voicing,
                        PitchType = pitchType
                    });
                }
            }
        }
        
        if (instrumentsToImport.Count == 0)
        {
            await DisplayAlert("Error", "No instruments selected", "OK");
            return;
        }
        
        try
        {
            ImportButton.IsEnabled = false;
            ImportButton.Text = "Importing...";
            
            var createdConfigs = await _configService.ImportSfzAsync(_selectedSfzPath, _selectedWavPath, instrumentsToImport);
            
            await DisplayAlert("Success", 
                $"Successfully imported {createdConfigs.Count} instrument(s)!", 
                "OK");
            
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to import: {ex.Message}", "OK");
            ImportButton.IsEnabled = true;
            ImportButton.Text = "Import";
        }
    }

    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}

