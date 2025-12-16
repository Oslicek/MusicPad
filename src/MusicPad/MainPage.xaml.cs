using MusicPad.Services;

namespace MusicPad;

public partial class MainPage : ContentPage
{
    private readonly ISfzService _sfzService;
    private bool _isLoading;

    public MainPage(ISfzService sfzService)
    {
        InitializeComponent();
        _sfzService = sfzService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadInstrumentsAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _sfzService.StopNote();
    }

    private async Task LoadInstrumentsAsync()
    {
        try
        {
            var instruments = _sfzService.AvailableInstruments;
            
            if (instruments.Count == 0)
            {
                StatusLabel.Text = "No instruments found";
                PadLabel.Text = "No instruments";
                return;
            }

            InstrumentPicker.ItemsSource = instruments.ToList();
            InstrumentPicker.SelectedIndex = 0;
            
            // Load first instrument
            await LoadSelectedInstrumentAsync();
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Error: {ex.Message}";
        }
    }

    private async void OnInstrumentChanged(object? sender, EventArgs e)
    {
        await LoadSelectedInstrumentAsync();
    }

    private async Task LoadSelectedInstrumentAsync()
    {
        if (_isLoading || InstrumentPicker.SelectedIndex < 0)
            return;

        _isLoading = true;
        var instrumentName = InstrumentPicker.SelectedItem?.ToString();
        
        if (string.IsNullOrEmpty(instrumentName))
            return;

        try
        {
            StatusLabel.Text = $"Loading {instrumentName}...";
            PadLabel.Text = "Loading...";
            
            await _sfzService.LoadInstrumentAsync(instrumentName);
            
            StatusLabel.Text = $"Ready: {instrumentName}";
            PadLabel.Text = "Tap to Play";
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Error: {ex.Message}";
            PadLabel.Text = "Error loading";
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void OnPadTapped(object? sender, TappedEventArgs e)
    {
        if (_sfzService.CurrentInstrumentName == null)
            return;

        // Play a short note
        _sfzService.PlayNote();
        
        // Visual feedback
        PadLabel.Text = "♪";
        
        // Stop note after a short delay
        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(500), () =>
        {
            _sfzService.StopNote();
            PadLabel.Text = "Tap to Play";
        });
    }
}

