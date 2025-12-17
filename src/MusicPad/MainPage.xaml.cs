using MusicPad.Controls;
using MusicPad.Core.Models;
using MusicPad.Services;
using Microsoft.Maui.Devices;

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
        bool useLeft = _isLandscape && currentPadrea != null && currentPadrea.Kind == PadreaKind.Grid;

        // Move volume knob to left host if needed
        if (useLeft)
        {
            if (VolumeLeftHost.Content != VolumeKnob)
            {
                RemoveFromParent(VolumeKnob);
                VolumeLeftHost.Content = VolumeKnob;
            }
        }
        else
        {
            if (VolumeLeftHost.Content == VolumeKnob)
            {
                VolumeLeftHost.Content = null;
                AddVolumeTop();
            }
            else if (VolumeKnob.Parent == null)
            {
                AddVolumeTop();
            }
        }
    }

    private void AddVolumeTop()
    {
        if (Content is Grid grid && !grid.Children.Contains(VolumeKnob))
        {
            grid.Children.Add(VolumeKnob);
            Grid.SetRow(VolumeKnob, 1);
            Grid.SetColumn(VolumeKnob, 0);
        }
    }

    private static void RemoveFromParent(View view)
    {
        if (view.Parent is Layout layout)
        {
            layout.Children.Remove(view);
        }
        else if (view.Parent is ContentView cv)
        {
            cv.Content = null;
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
        bool landscape = width > height;
        if (landscape != _isLandscape)
        {
            _isLandscape = landscape;
            AdjustVolumeLayout();
            if (_pianoRangeManager != null)
            {
                _pianoRangeManager.SetOrientation(_isLandscape, preserveStart: false);
            }
            // Re-run layout if piano or for orientation-dependent logic
            if (_sfzService.CurrentInstrumentName != null)
            {
                SetupPadMatrix();
            }
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

            EnsurePadGraphicsView(_padDrawable);
        }

        // Update status with viewpage info
        if (padrea != null && padrea.RowsPerViewpage.HasValue && padrea.Kind != PadreaKind.Piano)
        {
            int total = padrea.GetTotalViewpages(instrumentMinKey, instrumentMaxKey);
            StatusLabel.Text = $"{_sfzService.CurrentInstrumentName} - Page {padrea.CurrentViewpage + 1}/{total}";
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

    private void EnsurePadGraphicsView(IDrawable drawable, bool alignBottom = false)
    {
        if (_padGraphicsView == null)
        {
            _padGraphicsView = new GraphicsView
            {
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = alignBottom ? LayoutOptions.End : LayoutOptions.Fill
            };

            // Setup touch handlers
            _padGraphicsView.StartInteraction += OnStartInteraction;
            _padGraphicsView.DragInteraction += OnDragInteraction;
            _padGraphicsView.EndInteraction += OnEndInteraction;
            _padGraphicsView.CancelInteraction += OnCancelInteraction;

            PadContainer.Children.Add(_padGraphicsView);
        }
        else
        {
            // Update vertical alignment if needed
            _padGraphicsView.VerticalOptions = alignBottom ? LayoutOptions.End : LayoutOptions.Fill;
        }

        _padGraphicsView.Drawable = drawable;
        _padGraphicsView.Invalidate();
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
        EnsurePadGraphicsView(_pianoDrawable, alignBottom: true); // Piano always at bottom

        StatusLabel.Text = $"{_sfzService.CurrentInstrumentName} - Piano {GetNoteNameShort(start)}..{GetNoteNameShort(end)}";
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

    private static string GetNoteNameShort(int midiNote)
    {
        string[] names = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
        int note = midiNote % 12;
        int octave = (midiNote / 12) - 1;
        return $"{names[note]}{octave}";
    }
}
