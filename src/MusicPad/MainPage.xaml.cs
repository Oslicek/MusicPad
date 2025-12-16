using MusicPad.Controls;
using MusicPad.Services;

namespace MusicPad;

public partial class MainPage : ContentPage
{
    private readonly ISfzService _sfzService;
    private readonly PadMatrixDrawable _padDrawable;
    private GraphicsView? _padGraphicsView;
    private bool _isLoading;

    public MainPage(ISfzService sfzService)
    {
        InitializeComponent();
        _sfzService = sfzService;
        _padDrawable = new PadMatrixDrawable();
        _padDrawable.NoteOn += OnNoteOn;
        _padDrawable.NoteOff += OnNoteOff;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadInstrumentsAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _sfzService.StopAll();
    }

    private async Task LoadInstrumentsAsync()
    {
        try
        {
            var instruments = _sfzService.AvailableInstruments;
            
            if (instruments.Count == 0)
            {
                StatusLabel.Text = "No instruments found";
                LoadingLabel.Text = "No instruments";
                return;
            }

            InstrumentPicker.ItemsSource = instruments.ToList();
            InstrumentPicker.SelectedIndex = 0;
            
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
            LoadingLabel.IsVisible = true;
            LoadingLabel.Text = "Loading...";
            
            await _sfzService.LoadInstrumentAsync(instrumentName);
            
            // Setup pad matrix
            SetupPadMatrix();
            
            StatusLabel.Text = $"{instrumentName}";
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Error: {ex.Message}";
            LoadingLabel.Text = "Error loading";
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void SetupPadMatrix()
    {
        LoadingLabel.IsVisible = false;

        var (minKey, maxKey) = _sfzService.CurrentKeyRange;
        
        if (maxKey <= minKey)
        {
            LoadingLabel.IsVisible = true;
            LoadingLabel.Text = "No notes";
            return;
        }

        _padDrawable.SetKeyRange(minKey, maxKey);

        // Create or reuse GraphicsView
        if (_padGraphicsView == null)
        {
            _padGraphicsView = new GraphicsView
            {
                Drawable = _padDrawable,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill
            };

            // Setup touch handlers
            _padGraphicsView.StartInteraction += OnStartInteraction;
            _padGraphicsView.DragInteraction += OnDragInteraction;
            _padGraphicsView.EndInteraction += OnEndInteraction;
            _padGraphicsView.CancelInteraction += OnCancelInteraction;

            PadContainer.Children.Add(_padGraphicsView);
        }

        _padGraphicsView.Invalidate();
    }

    private void OnStartInteraction(object? sender, TouchEventArgs e)
    {
        var touch = e.Touches.FirstOrDefault();
        if (touch != default)
        {
            _padDrawable.OnTouchStart((float)touch.X, (float)touch.Y);
            _padGraphicsView?.Invalidate();
        }
    }

    private void OnDragInteraction(object? sender, TouchEventArgs e)
    {
        var touch = e.Touches.FirstOrDefault();
        if (touch != default)
        {
            _padDrawable.OnTouchMove((float)touch.X, (float)touch.Y);
        }
    }

    private void OnEndInteraction(object? sender, TouchEventArgs e)
    {
        var touch = e.Touches.FirstOrDefault();
        if (touch != default)
        {
            _padDrawable.OnTouchEnd((float)touch.X, (float)touch.Y);
            _padGraphicsView?.Invalidate();
        }
    }

    private void OnCancelInteraction(object? sender, EventArgs e)
    {
        _padDrawable.OnAllTouchesEnd();
        _padGraphicsView?.Invalidate();
    }

    private void OnNoteOn(object? sender, int midiNote)
    {
        _sfzService.NoteOn(midiNote);
    }

    private void OnNoteOff(object? sender, int midiNote)
    {
        _sfzService.NoteOff(midiNote);
    }
}
