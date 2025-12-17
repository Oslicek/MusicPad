using MusicPad.Controls;
using MusicPad.Core.Models;
using MusicPad.Services;

namespace MusicPad;

public partial class MainPage : ContentPage
{
    private readonly ISfzService _sfzService;
    private readonly IPadreaService _padreaService;
    private readonly PadMatrixDrawable _padDrawable;
    private readonly RotaryKnobDrawable _volumeKnobDrawable;
    private readonly List<ScaleOption> _scaleOptions = new();
    private readonly PianoKeyboardDrawable _pianoDrawable = new();
    private PianoRangeManager? _pianoRangeManager;
    private GraphicsView? _padGraphicsView;
    private bool _isLoading;
    private bool _isLandscape;
    private double _pageWidth;
    private double _pageHeight;

    public MainPage(ISfzService sfzService, IPadreaService padreaService)
    {
        InitializeComponent();
        _sfzService = sfzService;
        _padreaService = padreaService;
        _padDrawable = new PadMatrixDrawable();
        _padDrawable.NoteOn += OnNoteOn;
        _padDrawable.NoteOff += OnNoteOff;
        _padDrawable.NavigateUp += OnNavigateUp;
        _padDrawable.NavigateDown += OnNavigateDown;
        
        // Build scale options (common scales)
        BuildScaleOptions();

        // Piano events
        _pianoDrawable.NoteOn += OnNoteOn;
        _pianoDrawable.NoteOff += OnNoteOff;
        _pianoDrawable.ShiftRequested += OnPianoShiftRequested;
        _pianoDrawable.StripDragRequested += OnPianoStripDragRequested;
        
        // Setup volume knob
        _volumeKnobDrawable = new RotaryKnobDrawable { Label = "VOL", Value = 0.75f };
        _volumeKnobDrawable.ValueChanged += OnVolumeChanged;
        SetupVolumeKnob();
    }

    private void SetupVolumeKnob()
    {
        VolumeKnob.Drawable = _volumeKnobDrawable;
        
        VolumeKnob.StartInteraction += (s, e) =>
        {
            var touch = e.Touches.FirstOrDefault();
            if (touch != default)
            {
                _volumeKnobDrawable.OnTouch((float)touch.X, (float)touch.Y, isStart: true);
                VolumeKnob.Invalidate();
            }
        };
        
        VolumeKnob.DragInteraction += (s, e) =>
        {
            var touch = e.Touches.FirstOrDefault();
            if (touch != default)
            {
                _volumeKnobDrawable.OnTouch((float)touch.X, (float)touch.Y, isStart: false);
                VolumeKnob.Invalidate();
            }
        };
        
        VolumeKnob.EndInteraction += (s, e) =>
        {
            _volumeKnobDrawable.OnTouchEnd();
            VolumeKnob.Invalidate();
        };
        
        VolumeKnob.CancelInteraction += (s, e) =>
        {
            _volumeKnobDrawable.OnTouchEnd();
            VolumeKnob.Invalidate();
        };
        
        // Apply initial volume
        _sfzService.Volume = _volumeKnobDrawable.Value;
    }

    private void OnVolumeChanged(object? sender, float volume)
    {
        _sfzService.Volume = volume;
    }

    private void AdjustVolumeLayout()
    {
        var currentPadrea = _padreaService.CurrentPadrea;
        bool isPiano = currentPadrea?.Kind == PadreaKind.Piano;
        bool isSquarePadrea = currentPadrea?.Kind == PadreaKind.Grid;

        if (_isLandscape)
        {
            if (isPiano)
            {
                // Piano in landscape: volume knob at left edge, top
                VolumeKnob.HorizontalOptions = LayoutOptions.Start;
                VolumeKnob.VerticalOptions = LayoutOptions.Start;
                VolumeKnob.Margin = new Thickness(8, 8, 0, 0);
            }
            else if (isSquarePadrea)
            {
                // Square padrea in landscape: volume knob at top, centered between left edge and padrea
                // The padrea will be centered, so we position volume knob in the left space
                VolumeKnob.HorizontalOptions = LayoutOptions.Start;
                VolumeKnob.VerticalOptions = LayoutOptions.Start;
                // Calculate left margin to center it in the space left of the padrea
                double padreaSize = Math.Min(_pageWidth, _pageHeight - 60); // Approximate padrea size
                double leftSpace = (_pageWidth - padreaSize) / 2;
                double knobCenterX = (leftSpace - 120) / 2; // 120 is knob width
                VolumeKnob.Margin = new Thickness(Math.Max(8, knobCenterX), 8, 0, 0);
            }
        }
        else
        {
            // Portrait: volume knob at top center
            VolumeKnob.HorizontalOptions = LayoutOptions.Center;
            VolumeKnob.VerticalOptions = LayoutOptions.Start;
            VolumeKnob.Margin = new Thickness(0, 8, 0, 8);
        }
    }

    private void BuildScaleOptions()
    {
        _scaleOptions.Clear();
        // Major (12 roots)
        foreach (var (root, name) in RootNotes())
        {
            _scaleOptions.Add(new ScaleOption($"{name} Major", root, ScaleType.Major));
        }
        // Natural minor (12 roots)
        foreach (var (root, name) in RootNotes())
        {
            _scaleOptions.Add(new ScaleOption($"{name} Minor", root, ScaleType.NaturalMinor));
        }
        // Common modes (rooted on C by default)
        _scaleOptions.Add(new ScaleOption("C Dorian", 0, ScaleType.Dorian));
        _scaleOptions.Add(new ScaleOption("C Phrygian", 0, ScaleType.Phrygian));
        _scaleOptions.Add(new ScaleOption("C Lydian", 0, ScaleType.Lydian));
        _scaleOptions.Add(new ScaleOption("C Mixolydian", 0, ScaleType.Mixolydian));
        _scaleOptions.Add(new ScaleOption("C Locrian", 0, ScaleType.Locrian));
        _scaleOptions.Add(new ScaleOption("C Harmonic Minor", 0, ScaleType.HarmonicMinor));
        _scaleOptions.Add(new ScaleOption("C Melodic Minor", 0, ScaleType.MelodicMinor));
    }

    private static IEnumerable<(int root, string name)> RootNotes()
    {
        yield return (0, "C");
        yield return (1, "C#");
        yield return (2, "D");
        yield return (3, "D#");
        yield return (4, "E");
        yield return (5, "F");
        yield return (6, "F#");
        yield return (7, "G");
        yield return (8, "G#");
        yield return (9, "A");
        yield return (10, "A#");
        yield return (11, "B");
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        LoadPadreas();
        await LoadInstrumentsAsync();
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);
        
        if (width <= 0 || height <= 0) return;
        
        _pageWidth = width;
        _pageHeight = height;
        
        bool landscape = width > height;
        bool orientationChanged = landscape != _isLandscape;
        _isLandscape = landscape;
        
        if (orientationChanged)
        {
            if (_pianoRangeManager != null)
            {
                _pianoRangeManager.SetOrientation(_isLandscape, preserveStart: false);
            }
        }
        
        AdjustVolumeLayout();
        
        // Re-run layout for size-dependent padrea
        if (_sfzService.CurrentInstrumentName != null)
        {
            SetupPadMatrix();
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _sfzService.StopAll();
    }

    private void LoadPadreas()
    {
        var padreas = _padreaService.AvailablePadreas;
        PadreaPicker.ItemsSource = padreas.ToList();
        ScalePicker.ItemsSource = _scaleOptions.ToList();
        
        // Select current padrea
        var currentPadrea = _padreaService.CurrentPadrea;
        if (currentPadrea != null)
        {
            var index = padreas.ToList().FindIndex(p => p.Id == currentPadrea.Id);
            if (index >= 0)
                PadreaPicker.SelectedIndex = index;
        }
        else if (padreas.Count > 0)
        {
            PadreaPicker.SelectedIndex = 0;
        }
    }

    private void OnPadreaChanged(object? sender, EventArgs e)
    {
        if (PadreaPicker.SelectedItem is Padrea selectedPadrea)
        {
            _padreaService.CurrentPadrea = selectedPadrea;
            UpdateScalePickerForPadrea(selectedPadrea);
            AdjustVolumeLayout();
            UpdatePadMatrixForPadrea();
        }
    }

    private void OnScaleChanged(object? sender, EventArgs e)
    {
        if (ScalePicker.SelectedItem is ScaleOption option &&
            _padreaService.CurrentPadrea is Padrea padrea &&
            padrea.Id == "scales")
        {
            ApplyScaleOptionToPadrea(padrea, option);
            padrea.CurrentViewpage = 0; // reset paging when scale changes
            SetupPadMatrix();
        }
    }

    private async Task LoadInstrumentsAsync()
    {
        try
        {
            var instruments = _sfzService.AvailableInstruments;
            
            if (instruments.Count == 0)
            {
                LoadingLabel.Text = "No instruments found";
                return;
            }

            InstrumentPicker.ItemsSource = instruments.ToList();
            InstrumentPicker.SelectedIndex = 0;
            
            await LoadSelectedInstrumentAsync();
        }
        catch (Exception ex)
        {
            LoadingLabel.Text = $"Error: {ex.Message}";
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
            LoadingLabel.IsVisible = true;
            LoadingLabel.Text = $"Loading {instrumentName}...";
            
            await _sfzService.LoadInstrumentAsync(instrumentName);
            
            // Setup pad matrix
            SetupPadMatrix();
        }
        catch (Exception ex)
        {
            LoadingLabel.Text = $"Error: {ex.Message}";
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void SetupPadMatrix()
    {
        LoadingLabel.IsVisible = false;

        var (instrumentMinKey, instrumentMaxKey) = _sfzService.CurrentKeyRange;
        
        if (instrumentMaxKey <= instrumentMinKey)
        {
            LoadingLabel.IsVisible = true;
            LoadingLabel.Text = "No notes";
            return;
        }

        var padrea = _padreaService.CurrentPadrea;
        bool isPiano = padrea?.Kind == PadreaKind.Piano;

        if (isPiano)
        {
            SetupPianoPadrea(padrea!, instrumentMinKey, instrumentMaxKey);
        }
        else
        {
            SetupGridPadrea(padrea, instrumentMinKey, instrumentMaxKey);
        }
    }

    private void SetupGridPadrea(Padrea? padrea, int instrumentMinKey, int instrumentMaxKey)
    {
        if (padrea == null)
        {
            // Fallback to simple range
            _padDrawable.SetKeyRange(instrumentMinKey, instrumentMaxKey);
            _padDrawable.SetHalftoneDetector(null);
        }
        else
        {
            // Get notes for current viewpage based on padrea settings
            var notes = padrea.GetViewpageNotes(instrumentMinKey, instrumentMaxKey);
            
            if (notes.Count == 0)
            {
                LoadingLabel.IsVisible = true;
                LoadingLabel.Text = "No notes in range";
                return;
            }
            
            // Check if we need navigation arrows
            int totalViewpages = padrea.GetTotalViewpages(instrumentMinKey, instrumentMaxKey);
            bool hasUpArrow = padrea.CurrentViewpage < totalViewpages - 1;
            bool hasDownArrow = padrea.CurrentViewpage > 0;
            
            // Set notes and colors
            _padDrawable.SetNotes(notes, padrea.Columns, hasUpArrow, hasDownArrow);
            _padDrawable.SetColors(padrea.PadColor, padrea.PadPressedColor, 
                                   padrea.PadAltColor, padrea.PadAltPressedColor);
            _padDrawable.SetHalftoneDetector(padrea.IsHalftone);
        }

        // Calculate square size for grid padreas
        double availableHeight = _pageHeight - 60 - 140; // Header and volume knob
        double availableWidth = _pageWidth - 16; // Padding
        double padreaSize = Math.Min(availableWidth, availableHeight);

        EnsurePadGraphicsView(_padDrawable);
        
        // Square padreas: set size and center horizontally in landscape
        if (_padGraphicsView != null)
        {
            _padGraphicsView.WidthRequest = padreaSize;
            _padGraphicsView.HeightRequest = padreaSize;
            _padGraphicsView.HorizontalOptions = _isLandscape ? LayoutOptions.Center : LayoutOptions.Fill;
            _padGraphicsView.VerticalOptions = LayoutOptions.End;
        }
    }

    private void SetupPianoPadrea(Padrea padrea, int instrumentMinKey, int instrumentMaxKey)
    {
        if (_pianoRangeManager == null)
        {
            _pianoRangeManager = new PianoRangeManager(instrumentMinKey, instrumentMaxKey, _isLandscape);
        }
        else
        {
            _pianoRangeManager.UpdateInstrumentRange(instrumentMinKey, instrumentMaxKey);
            _pianoRangeManager.SetOrientation(_isLandscape, preserveStart: true);
        }
        var (start, end) = _pianoRangeManager.GetRange();

        _pianoDrawable.SetRange(start, end, instrumentMinKey, instrumentMaxKey, _isLandscape);
        
        EnsurePadGraphicsView(_pianoDrawable);
        
        // Piano padrea: full width, shorter height in portrait
        if (_padGraphicsView != null)
        {
            _padGraphicsView.WidthRequest = -1; // Auto width
            
            if (_isLandscape)
            {
                // Landscape: use available height minus header/volume space
                _padGraphicsView.HeightRequest = _pageHeight - 60 - 140;
            }
            else
            {
                // Portrait: make it shorter (about 35% of screen height)
                _padGraphicsView.HeightRequest = _pageHeight * 0.35;
            }
            
            _padGraphicsView.HorizontalOptions = LayoutOptions.Fill;
            _padGraphicsView.VerticalOptions = LayoutOptions.End;
        }
    }

    private void UpdatePadMatrixForPadrea()
    {
        // Only update if we have an instrument loaded
        if (_sfzService.CurrentInstrumentName != null)
        {
            SetupPadMatrix();
        }
    }

    private void OnStartInteraction(object? sender, TouchEventArgs e)
    {
        var touches = e.Touches.Select(t => new PointF((float)t.X, (float)t.Y)).ToList();
        if (IsCurrentPadreaPiano())
        {
            if (touches.Count > 0)
            {
                _pianoDrawable.OnTouchStart(touches[0].X, touches[0].Y);
            }
            _pianoDrawable.OnTouches(touches);
        }
        else
        {
            _padDrawable.OnTouches(touches);
        }
        _padGraphicsView?.Invalidate();
    }

    private void OnDragInteraction(object? sender, TouchEventArgs e)
    {
        var touches = e.Touches.Select(t => new PointF((float)t.X, (float)t.Y)).ToList();
        if (IsCurrentPadreaPiano())
        {
            _pianoDrawable.OnTouches(touches);
        }
        else
        {
            _padDrawable.OnTouches(touches);
        }
        _padGraphicsView?.Invalidate();
    }

    private void OnEndInteraction(object? sender, TouchEventArgs e)
    {
        var lastTouch = e.Touches.FirstOrDefault();
        if (IsCurrentPadreaPiano())
        {
            if (lastTouch != default)
            {
                _pianoDrawable.OnTouchEnd((float)lastTouch.X, (float)lastTouch.Y);
            }
        }
        else
        {
            if (lastTouch != default)
            {
                _padDrawable.OnTapEnd((float)lastTouch.X, (float)lastTouch.Y);
            }
            _padDrawable.OnAllTouchesEnd();
        }
        _padGraphicsView?.Invalidate();
    }

    private void OnCancelInteraction(object? sender, EventArgs e)
    {
        if (IsCurrentPadreaPiano())
        {
            _pianoDrawable.OnTouchEnd(0, 0);
        }
        else
        {
            _padDrawable.OnAllTouchesEnd();
        }
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

    private void OnNavigateUp(object? sender, EventArgs e)
    {
        var padrea = _padreaService.CurrentPadrea;
        if (padrea == null) return;
        
        var (minKey, maxKey) = _sfzService.CurrentKeyRange;
        if (padrea.NextViewpage(minKey, maxKey))
        {
            SetupPadMatrix();
        }
    }

    private void OnNavigateDown(object? sender, EventArgs e)
    {
        var padrea = _padreaService.CurrentPadrea;
        if (padrea == null) return;
        
        var (minKey, maxKey) = _sfzService.CurrentKeyRange;
        if (padrea.PreviousViewpage(minKey, maxKey))
        {
            SetupPadMatrix();
        }
    }

    private void UpdateScalePickerForPadrea(Padrea padrea)
    {
        bool isScalePadrea = padrea.Id == "scales" || padrea.NoteFilter == NoteFilterType.HeptatonicScale;
        ScalePicker.IsVisible = isScalePadrea;
        if (isScalePadrea)
        {
            // Ensure a selection (default to C Major)
            if (ScalePicker.SelectedIndex < 0)
            {
                var defaultIndex = _scaleOptions.FindIndex(s => s.Name == "C Major");
                ScalePicker.SelectedIndex = defaultIndex >= 0 ? defaultIndex : 0;
            }

            if (ScalePicker.SelectedItem is ScaleOption option)
            {
                ApplyScaleOptionToPadrea(padrea, option);
            }
        }
    }

    private void ApplyScaleOptionToPadrea(Padrea padrea, ScaleOption option)
    {
        padrea.NoteFilter = NoteFilterType.HeptatonicScale;
        padrea.ScaleType = option.ScaleType;
        padrea.RootNote = option.Root;
    }

    private record ScaleOption(string Name, int Root, ScaleType ScaleType)
    {
        public override string ToString() => Name;
    }

    private bool IsCurrentPadreaPiano() => _padreaService.CurrentPadrea?.Kind == PadreaKind.Piano;

    private void EnsurePadGraphicsView(IDrawable drawable)
    {
        if (_padGraphicsView == null)
        {
            _padGraphicsView = new GraphicsView();

            // Setup touch handlers
            _padGraphicsView.StartInteraction += OnStartInteraction;
            _padGraphicsView.DragInteraction += OnDragInteraction;
            _padGraphicsView.EndInteraction += OnEndInteraction;
            _padGraphicsView.CancelInteraction += OnCancelInteraction;

            PadContainer.Children.Add(_padGraphicsView);
        }

        _padGraphicsView.Drawable = drawable;
        _padGraphicsView.Invalidate();
    }

    private void OnPianoShiftRequested(object? sender, int semitoneDelta)
    {
        if (_pianoRangeManager == null) return;
        _pianoRangeManager.Move(semitoneDelta);
        SetupPadMatrix();
    }

    private void OnPianoStripDragRequested(object? sender, int newStart)
    {
        if (_pianoRangeManager == null) return;
        _pianoRangeManager.SetStartAbsolute(newStart);
        SetupPadMatrix();
    }
}
