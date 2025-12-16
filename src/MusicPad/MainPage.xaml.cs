using Microsoft.Maui.Controls.Shapes;
using MusicPad.Services;

namespace MusicPad;

public partial class MainPage : ContentPage
{
    private static readonly string[] NoteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
    private static readonly bool[] IsSharp = { false, true, false, true, false, false, true, false, true, false, true, false };
    
    // Natural notes - teal
    private static readonly Color PadColor = Color.FromArgb("#4ECDC4");
    private static readonly Color PadPressedColor = Color.FromArgb("#7EEEE6");
    private static readonly Color PadBorderColor = Color.FromArgb("#2A9D8F");
    
    // Sharp/flat notes - orange/amber
    private static readonly Color SharpPadColor = Color.FromArgb("#E8A838");
    private static readonly Color SharpPadPressedColor = Color.FromArgb("#F5C868");
    private static readonly Color SharpPadBorderColor = Color.FromArgb("#C48820");
    
    private readonly ISfzService _sfzService;
    private readonly Dictionary<int, Border> _padBorders = new();
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
            
            // Build pad matrix
            BuildPadMatrix();
            
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

    private Grid? _padGrid;
    private int _gridRows;
    private int _gridColumns;

    private void BuildPadMatrix()
    {
        _padBorders.Clear();
        PadContainer.Children.Clear();
        LoadingLabel.IsVisible = false;

        var (minKey, maxKey) = _sfzService.CurrentKeyRange;
        int noteCount = maxKey - minKey + 1;

        if (noteCount <= 0)
        {
            LoadingLabel.IsVisible = true;
            LoadingLabel.Text = "No notes";
            return;
        }

        // Calculate grid dimensions - aim for a roughly square grid
        _gridColumns = (int)Math.Ceiling(Math.Sqrt(noteCount));
        _gridRows = (int)Math.Ceiling((double)noteCount / _gridColumns);

        // Create the grid - will be centered and sized to fill space with square pads
        _padGrid = new Grid
        {
            ColumnSpacing = 3,
            RowSpacing = 3,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };

        // Add row and column definitions
        for (int i = 0; i < _gridRows; i++)
            _padGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
        for (int i = 0; i < _gridColumns; i++)
            _padGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

        // Create pads - arrange from bottom-left (low notes) to top-right (high notes)
        int noteIndex = 0;
        for (int row = _gridRows - 1; row >= 0 && noteIndex < noteCount; row--)
        {
            for (int col = 0; col < _gridColumns && noteIndex < noteCount; col++)
            {
                int midiNote = minKey + noteIndex;
                var pad = CreatePad(midiNote);
                
                Grid.SetRow(pad, row);
                Grid.SetColumn(pad, col);
                _padGrid.Children.Add(pad);
                
                _padBorders[midiNote] = pad;
                noteIndex++;
            }
        }

        PadContainer.Children.Add(_padGrid);
        
        // Update sizing when container size changes
        PadContainer.SizeChanged -= OnPadContainerSizeChanged;
        PadContainer.SizeChanged += OnPadContainerSizeChanged;
        
        // Trigger initial sizing
        UpdateGridSize();
    }

    private void OnPadContainerSizeChanged(object? sender, EventArgs e)
    {
        UpdateGridSize();
    }

    private void UpdateGridSize()
    {
        if (_padGrid == null || PadContainer.Width <= 0 || PadContainer.Height <= 0)
            return;

        double availableWidth = PadContainer.Width;
        double availableHeight = PadContainer.Height;
        double spacing = 3;

        // Calculate pad size to make squares that fill the smaller dimension
        double padSizeByWidth = (availableWidth - (_gridColumns - 1) * spacing) / _gridColumns;
        double padSizeByHeight = (availableHeight - (_gridRows - 1) * spacing) / _gridRows;
        
        // Use the smaller size to ensure squares fit
        double padSize = Math.Min(padSizeByWidth, padSizeByHeight);
        
        // Calculate total grid size
        double gridWidth = padSize * _gridColumns + (_gridColumns - 1) * spacing;
        double gridHeight = padSize * _gridRows + (_gridRows - 1) * spacing;

        _padGrid.WidthRequest = gridWidth;
        _padGrid.HeightRequest = gridHeight;
        
        // Update font size based on pad size
        double fontSize = Math.Max(10, padSize / 4);
        foreach (var child in _padGrid.Children)
        {
            if (child is Border border && border.Content is Label label)
            {
                label.FontSize = fontSize;
            }
        }
    }

    private Border CreatePad(int midiNote)
    {
        string noteName = GetNoteName(midiNote);
        bool isSharpNote = IsSharp[midiNote % 12];
        
        Color padColor = isSharpNote ? SharpPadColor : PadColor;
        Color borderColor = isSharpNote ? SharpPadBorderColor : PadBorderColor;
        
        var label = new Label
        {
            Text = noteName,
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Shadow = new Shadow
            {
                Brush = new SolidColorBrush(Colors.Black),
                Offset = new Point(1, 1),
                Radius = 2,
                Opacity = 0.5f
            }
        };

        var border = new Border
        {
            BackgroundColor = padColor,
            Stroke = borderColor,
            StrokeThickness = 2,
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            Padding = 4,
            Content = label
        };

        // Store the normal color for this pad
        border.BindingContext = (isSharpNote, padColor);

        // Add touch handling
        var tapRecognizer = new TapGestureRecognizer();
        tapRecognizer.Tapped += (s, e) => OnPadPressed(midiNote, border);
        border.GestureRecognizers.Add(tapRecognizer);

        // Use PointerGestureRecognizer for press/release on supported platforms
        var pointerRecognizer = new PointerGestureRecognizer();
        pointerRecognizer.PointerPressed += (s, e) => OnPadTouchDown(midiNote, border);
        pointerRecognizer.PointerReleased += (s, e) => OnPadTouchUp(midiNote, border);
        pointerRecognizer.PointerExited += (s, e) => OnPadTouchUp(midiNote, border);
        border.GestureRecognizers.Add(pointerRecognizer);

        return border;
    }

    private void OnPadPressed(int midiNote, Border border)
    {
        bool isSharp = IsSharp[midiNote % 12];
        Color normalColor = isSharp ? SharpPadColor : PadColor;
        Color pressedColor = isSharp ? SharpPadPressedColor : PadPressedColor;
        
        // Fallback for tap - play note briefly
        _sfzService.NoteOn(midiNote);
        border.BackgroundColor = pressedColor;
        
        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(300), () =>
        {
            _sfzService.NoteOff(midiNote);
            border.BackgroundColor = normalColor;
        });
    }

    private void OnPadTouchDown(int midiNote, Border border)
    {
        bool isSharp = IsSharp[midiNote % 12];
        Color pressedColor = isSharp ? SharpPadPressedColor : PadPressedColor;
        
        _sfzService.NoteOn(midiNote);
        border.BackgroundColor = pressedColor;
    }

    private void OnPadTouchUp(int midiNote, Border border)
    {
        bool isSharp = IsSharp[midiNote % 12];
        Color normalColor = isSharp ? SharpPadColor : PadColor;
        
        _sfzService.NoteOff(midiNote);
        border.BackgroundColor = normalColor;
    }

    private static string GetNoteName(int midiNote)
    {
        int noteIndex = midiNote % 12;
        int octave = (midiNote / 12) - 1;
        return $"{NoteNames[noteIndex]}{octave}";
    }
}

