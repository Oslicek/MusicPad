using MusicPad.Controls;
using MusicPad.Core.Models;
using MusicPad.Core.NoteProcessing;
using MusicPad.Core.Theme;
using MusicPad.Services;

namespace MusicPad;

public partial class MainPage : ContentPage
{
    private readonly ISfzService _sfzService;
    private readonly IPadreaService _padreaService;
    private readonly ISettingsService _settingsService;
    private readonly PadMatrixDrawable _padDrawable;
    private readonly RotaryKnobDrawable _volumeKnobDrawable;
    private readonly EffectAreaDrawable _effectAreaDrawable;
    private readonly List<ScaleOption> _scaleOptions = new();
    private readonly PianoKeyboardDrawable _pianoDrawable = new();
    private readonly PitchVolumeDrawable _pitchVolumeDrawable = new();
    private PianoRangeManager? _pianoRangeManager;
    private GraphicsView? _padGraphicsView;
    private bool _isLoading;
    private bool _isLandscape;
    private double _pageWidth;
    private double _pageHeight;
    
    // Harmony and Arpeggiator
    private readonly Harmony _harmony = new();
    private readonly Arpeggiator _arpeggiator = new();
    private IDispatcherTimer? _arpTimer;
    private int? _lastArpNote;
    
    // Envelope animation timer for pad glow effect
    private IDispatcherTimer? _envelopeAnimationTimer;

    public MainPage(ISfzService sfzService, IPadreaService padreaService, ISettingsService settingsService)
    {
        InitializeComponent();
        _sfzService = sfzService;
        _padreaService = padreaService;
        _settingsService = settingsService;
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
        
        // Pitch-Volume events
        _pitchVolumeDrawable.NoteOn += OnPitchVolumeNoteOn;
        _pitchVolumeDrawable.NoteOff += OnNoteOff;
        _pitchVolumeDrawable.VolumeChanged += OnPitchVolumeChanged;
        
        // Setup volume knob
        _volumeKnobDrawable = new RotaryKnobDrawable { Label = "VOL", Value = 0.75f };
        _volumeKnobDrawable.ValueChanged += OnVolumeChanged;
        SetupVolumeKnob();
        
        // Setup effect area
        _effectAreaDrawable = new EffectAreaDrawable();
        _effectAreaDrawable.EffectSelected += OnEffectSelected;
        SetupEffectArea();
        
        // Setup Harmony and Arpeggiator from effect area settings
        SetupHarmonyAndArpeggiator();
        
        // Subscribe to palette changes
        PaletteService.Instance.PaletteChanged += OnPaletteChanged;
    }
    
    private void OnPaletteChanged(object? sender, EventArgs e)
    {
        // Invalidate all drawables to pick up new colors
        MainThread.BeginInvokeOnMainThread(() =>
        {
            _padGraphicsView?.Invalidate();
            VolumeKnob?.Invalidate();
            EffectArea?.Invalidate();
            
            // Update background color dynamically
            BackgroundColor = Color.FromArgb(AppColors.BackgroundMain);
            
            // Update all pickers with palette colors
            var pickerBgColor = Color.FromArgb(AppColors.BackgroundPicker);
            var textColor = Color.FromArgb(AppColors.TextPrimary);
            
            if (InstrumentPicker != null)
            {
                InstrumentPicker.BackgroundColor = pickerBgColor;
                InstrumentPicker.TextColor = textColor;
            }
            if (PadreaPicker != null)
            {
                PadreaPicker.BackgroundColor = pickerBgColor;
                PadreaPicker.TextColor = textColor;
            }
            if (ScalePicker != null)
            {
                ScalePicker.BackgroundColor = pickerBgColor;
                ScalePicker.TextColor = textColor;
            }
            
            // Update hamburger button
            if (HamburgerButton != null)
            {
                HamburgerButton.BackgroundColor = pickerBgColor;
                HamburgerButton.Stroke = new SolidColorBrush(Color.FromArgb(AppColors.BorderDark));
            }
            if (HamburgerIcon != null)
            {
                HamburgerIcon.TextColor = textColor;
            }
            
            // Update commit hash label
            if (CommitHashLabel != null)
            {
                CommitHashLabel.TextColor = Color.FromArgb(AppColors.TextCommit);
            }
        });
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

    private void SetupEffectArea()
    {
        EffectArea.Drawable = _effectAreaDrawable;
        
        // Wire up LPF settings to audio service
        var lpfSettings = _effectAreaDrawable.LpfSettings;
        lpfSettings.EnabledChanged += (s, enabled) => _sfzService.LpfEnabled = enabled;
        lpfSettings.CutoffChanged += (s, cutoff) => _sfzService.LpfCutoff = cutoff;
        lpfSettings.ResonanceChanged += (s, resonance) => _sfzService.LpfResonance = resonance;
        
        // Wire up EQ settings to audio service
        var eqSettings = _effectAreaDrawable.EqSettings;
        eqSettings.BandChanged += (s, e) => _sfzService.SetEqBandGain(e.BandIndex, e.NewGain);
        
        // Wire up Chorus settings to audio service
        var chorusSettings = _effectAreaDrawable.ChorusSettings;
        chorusSettings.EnabledChanged += (s, enabled) => _sfzService.ChorusEnabled = enabled;
        chorusSettings.DepthChanged += (s, depth) => _sfzService.ChorusDepth = depth;
        chorusSettings.RateChanged += (s, rate) => _sfzService.ChorusRate = rate;
        
        // Wire up Delay settings to audio service
        var delaySettings = _effectAreaDrawable.DelaySettings;
        delaySettings.EnabledChanged += (s, enabled) => _sfzService.DelayEnabled = enabled;
        delaySettings.TimeChanged += (s, time) => _sfzService.DelayTime = time;
        delaySettings.FeedbackChanged += (s, feedback) => _sfzService.DelayFeedback = feedback;
        delaySettings.LevelChanged += (s, level) => _sfzService.DelayLevel = level;
        
        // Wire up Reverb settings to audio service
        var reverbSettings = _effectAreaDrawable.ReverbSettings;
        reverbSettings.EnabledChanged += (s, enabled) => _sfzService.ReverbEnabled = enabled;
        reverbSettings.LevelChanged += (s, level) => _sfzService.ReverbLevel = level;
        reverbSettings.TypeChanged += (s, type) => _sfzService.ReverbType = type;
        
        // Apply initial values
        _sfzService.LpfEnabled = lpfSettings.IsEnabled;
        _sfzService.LpfCutoff = lpfSettings.Cutoff;
        _sfzService.LpfResonance = lpfSettings.Resonance;
        for (int i = 0; i < 4; i++)
        {
            _sfzService.SetEqBandGain(i, eqSettings.GetGain(i));
        }
        _sfzService.ChorusEnabled = chorusSettings.IsEnabled;
        _sfzService.ChorusDepth = chorusSettings.Depth;
        _sfzService.ChorusRate = chorusSettings.Rate;
        _sfzService.DelayEnabled = delaySettings.IsEnabled;
        _sfzService.DelayTime = delaySettings.Time;
        _sfzService.DelayFeedback = delaySettings.Feedback;
        _sfzService.DelayLevel = delaySettings.Level;
        _sfzService.ReverbEnabled = reverbSettings.IsEnabled;
        _sfzService.ReverbLevel = reverbSettings.Level;
        _sfzService.ReverbType = reverbSettings.Type;
        
        // Handle invalidation requests from LPF/EQ controls
        _effectAreaDrawable.InvalidateRequested += (s, e) => EffectArea.Invalidate();
        
        EffectArea.StartInteraction += (s, e) =>
        {
            var touch = e.Touches.FirstOrDefault();
            if (touch != default)
            {
                if (_effectAreaDrawable.OnTouchStart((float)touch.X, (float)touch.Y))
                {
                    EffectArea.Invalidate();
                }
            }
        };
        
        EffectArea.DragInteraction += (s, e) =>
        {
            var touch = e.Touches.FirstOrDefault();
            if (touch != default)
            {
                if (_effectAreaDrawable.OnTouchMove((float)touch.X, (float)touch.Y))
                {
                    EffectArea.Invalidate();
                }
            }
        };
        
        EffectArea.EndInteraction += (s, e) =>
        {
            _effectAreaDrawable.OnTouchEnd();
            EffectArea.Invalidate();
        };
    }

    private void SetupHarmonyAndArpeggiator()
    {
        // Wire up Harmony settings
        var harmonySettings = _effectAreaDrawable.HarmonySettings;
        harmonySettings.EnabledChanged += (s, enabled) =>
        {
            _harmony.IsEnabled = enabled;
            if (!enabled)
            {
                _harmony.Reset();
            }
        };
        harmonySettings.TypeChanged += (s, type) => _harmony.Type = type;
        
        // Apply initial harmony settings
        _harmony.IsEnabled = harmonySettings.IsEnabled;
        _harmony.Type = harmonySettings.Type;
        
        // Wire up Arpeggiator settings
        var arpSettings = _effectAreaDrawable.ArpSettings;
        arpSettings.EnabledChanged += (s, enabled) =>
        {
            _arpeggiator.IsEnabled = enabled;
            if (enabled)
            {
                StartArpeggiator();
            }
            else
            {
                StopArpeggiator();
            }
        };
        arpSettings.RateChanged += (s, rate) =>
        {
            _arpeggiator.Rate = rate;
            UpdateArpTimerInterval();
        };
        arpSettings.PatternChanged += (s, pattern) => _arpeggiator.Pattern = pattern;
        
        // Apply initial arpeggiator settings
        _arpeggiator.IsEnabled = arpSettings.IsEnabled;
        _arpeggiator.Rate = arpSettings.Rate;
        _arpeggiator.Pattern = arpSettings.Pattern;
    }

    private void StartArpeggiator()
    {
        if (_arpTimer != null) return;
        
        _arpTimer = Dispatcher.CreateTimer();
        UpdateArpTimerInterval();
        _arpTimer.Tick += OnArpTimerTick;
        _arpTimer.Start();
    }

    private void StopArpeggiator()
    {
        if (_arpTimer == null) return;
        
        _arpTimer.Stop();
        _arpTimer.Tick -= OnArpTimerTick;
        _arpTimer = null;
        
        // Stop the last arpeggiated note
        if (_lastArpNote.HasValue)
        {
            _sfzService.NoteOff(_lastArpNote.Value);
            _lastArpNote = null;
        }
    }

    private void UpdateArpTimerInterval()
    {
        if (_arpTimer != null)
        {
            _arpTimer.Interval = TimeSpan.FromMilliseconds(_arpeggiator.GetIntervalMs());
        }
    }

    private void OnArpTimerTick(object? sender, EventArgs e)
    {
        if (!_arpeggiator.IsEnabled) return;
        
        // Stop the previous note
        if (_lastArpNote.HasValue)
        {
            _sfzService.NoteOff(_lastArpNote.Value);
        }
        
        // Get and play the next note
        var nextNote = _arpeggiator.GetNextNote();
        if (nextNote.HasValue)
        {
            _sfzService.NoteOn(nextNote.Value);
            _lastArpNote = nextNote;
        }
        else
        {
            _lastArpNote = null;
        }
    }

    private void OnEffectSelected(object? sender, EffectType effect)
    {
        // Effect selection changed - redraw to show new selection
        EffectArea.Invalidate();
    }

    private void OnVolumeChanged(object? sender, float volume)
    {
        _sfzService.Volume = volume;
    }
    
    private void EnsureEnvelopeAnimationTimer()
    {
        if (_envelopeAnimationTimer != null) return;
        
        _envelopeAnimationTimer = Dispatcher.CreateTimer();
        _envelopeAnimationTimer.Interval = TimeSpan.FromMilliseconds(33); // ~30 FPS
        _envelopeAnimationTimer.Tick += OnEnvelopeAnimationTick;
        _envelopeAnimationTimer.Start();
    }
    
    private void StopEnvelopeAnimationTimer()
    {
        if (_envelopeAnimationTimer == null) return;
        
        _envelopeAnimationTimer.Stop();
        _envelopeAnimationTimer.Tick -= OnEnvelopeAnimationTick;
        _envelopeAnimationTimer = null;
    }
    
    private void OnEnvelopeAnimationTick(object? sender, EventArgs e)
    {
        // Refresh for grid padreas and piano (both use envelope glow)
        // Skip only pitch-volume padrea which has its own touch glow
        if (_padGraphicsView != null && !IsCurrentPadreaPitchVolume())
        {
            _padGraphicsView.Invalidate();
        }
    }

    private void UpdateLayout()
    {
        var currentPadrea = _padreaService.CurrentPadrea;
        bool isPiano = currentPadrea?.Kind == PadreaKind.Piano;

        // Calculate available space
        double controlsWidth = 180 + 16; // picker width + margins
        double controlsHeight = GetControlsStackHeight();
        double volumeSize = 120;
        double padding = 8;

        if (_isLandscape)
        {
            if (isPiano)
            {
                // Landscape Piano: pickers top-left, volume next to pickers, efarea top-right, piano at bottom
                ControlsStack.HorizontalOptions = LayoutOptions.Start;
                ControlsStack.VerticalOptions = LayoutOptions.Start;
                ControlsStack.Margin = new Thickness(0);

                VolumeKnob.HorizontalOptions = LayoutOptions.Start;
                VolumeKnob.VerticalOptions = LayoutOptions.Start;
                VolumeKnob.Margin = new Thickness(controlsWidth + 8, 0, 0, 0);

                // Piano at bottom - compact height in landscape
                double pianoHeight = _pageHeight * 0.45;
                double topAreaHeight = _pageHeight - pianoHeight - padding;

                // Effect area: positioned in top-right, constrained to top area
                double efareaLeft = controlsWidth + volumeSize + 24;
                double efareaWidth = _pageWidth - efareaLeft - padding;
                
                _effectAreaDrawable.SetOrientation(false); // Vertical buttons
                _effectAreaDrawable.SetLandscapeSquare(false); // Not square padrea layout
                EffectArea.HorizontalOptions = LayoutOptions.Start;
                EffectArea.VerticalOptions = LayoutOptions.Start;
                EffectArea.WidthRequest = Math.Max(40, efareaWidth);
                EffectArea.HeightRequest = topAreaHeight;
                EffectArea.Margin = new Thickness(efareaLeft, 0, 0, 0);

                PadContainer.HorizontalOptions = LayoutOptions.Fill;
                PadContainer.VerticalOptions = LayoutOptions.End;
                PadContainer.HeightRequest = Math.Max(100, pianoHeight);
                PadContainer.WidthRequest = -1;
                PadContainer.Margin = new Thickness(0);
            }
            else
            {
                // Landscape Square Padrea: pickers top-left, volume below pickers, padrea center, efarea right
                ControlsStack.HorizontalOptions = LayoutOptions.Start;
                ControlsStack.VerticalOptions = LayoutOptions.Start;
                ControlsStack.Margin = new Thickness(0);

                VolumeKnob.HorizontalOptions = LayoutOptions.Start;
                VolumeKnob.VerticalOptions = LayoutOptions.Start;
                VolumeKnob.Margin = new Thickness(30, controlsHeight + 16, 0, 0);

                // Square padrea centered on page
                double availableHeight = _pageHeight - padding * 4;
                double padreaSize = availableHeight;

                PadContainer.HorizontalOptions = LayoutOptions.Center;
                PadContainer.VerticalOptions = LayoutOptions.Center;
                PadContainer.WidthRequest = padreaSize;
                PadContainer.HeightRequest = padreaSize;
                PadContainer.Margin = new Thickness(0);

                // Effect area on the right side - constrained width
                double padreaCenterX = _pageWidth / 2;
                double padreaRight = padreaCenterX + padreaSize / 2;
                double efareaWidth = _pageWidth - padreaRight - padding * 2;
                
                _effectAreaDrawable.SetOrientation(true); // Horizontal buttons at top
                _effectAreaDrawable.SetLandscapeSquare(true); // EQ under LPF layout
                EffectArea.HorizontalOptions = LayoutOptions.Start;
                EffectArea.VerticalOptions = LayoutOptions.Start;
                EffectArea.WidthRequest = Math.Max(40, efareaWidth);
                EffectArea.HeightRequest = _pageHeight - padding * 2;
                EffectArea.Margin = new Thickness(padreaRight + padding, 0, 0, 0);
            }
        }
        else
        {
            // Portrait: pickers top-left, volume to right, efarea below controls, padrea at bottom
            ControlsStack.HorizontalOptions = LayoutOptions.Start;
            ControlsStack.VerticalOptions = LayoutOptions.Start;
            ControlsStack.Margin = new Thickness(0);

            VolumeKnob.HorizontalOptions = LayoutOptions.Start;
            VolumeKnob.VerticalOptions = LayoutOptions.Start;
            VolumeKnob.Margin = new Thickness(controlsWidth + 8, 0, 0, 0);

            // Calculate sizes - efarea needs height for 4 effect buttons (25px each + spacing)
            double topAreaHeight = Math.Max(controlsHeight, volumeSize) + padding;
            double efareaHeight = 125; // Height for 4 buttons: 4*25 + 3*4 spacing + 2*4 margins = 120+
            
            _effectAreaDrawable.SetOrientation(false); // Vertical buttons on left
            _effectAreaDrawable.SetLandscapeSquare(false);
            EffectArea.HorizontalOptions = LayoutOptions.Fill;
            EffectArea.VerticalOptions = LayoutOptions.Start;
            EffectArea.WidthRequest = _pageWidth - padding * 2;
            EffectArea.HeightRequest = efareaHeight;
            EffectArea.Margin = new Thickness(0, topAreaHeight + padding, 0, 0);
            
            double padreaTop = topAreaHeight + efareaHeight + padding * 2;
            double availableForPadrea = _pageHeight - padreaTop - padding;
            
            if (isPiano)
            {
                // Piano padrea - full width
                PadContainer.HorizontalOptions = LayoutOptions.Fill;
                PadContainer.VerticalOptions = LayoutOptions.End;
                PadContainer.WidthRequest = _pageWidth - padding * 2;
                PadContainer.HeightRequest = Math.Min(_pageHeight * 0.42, availableForPadrea);
                PadContainer.Margin = new Thickness(0);
            }
            else
            {
                // Square padrea - centered
                double padreaSize = Math.Min(_pageWidth - padding * 2, availableForPadrea);

                PadContainer.HorizontalOptions = LayoutOptions.Center;
                PadContainer.VerticalOptions = LayoutOptions.End;
                PadContainer.WidthRequest = padreaSize;
                PadContainer.HeightRequest = padreaSize;
                PadContainer.Margin = new Thickness(0);
            }
        }
        
        EffectArea.Invalidate();
    }

    private double GetControlsStackHeight()
    {
        // Approximate height: 3 pickers at ~44 each + spacing
        int visiblePickers = ScalePicker.IsVisible ? 3 : 2;
        return visiblePickers * 44 + (visiblePickers - 1) * 4;
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
        
        // Update glow settings (in case user changed them in Settings page)
        _padDrawable.SetGlowEnabled(_settingsService.PadGlowEnabled);
        _pianoDrawable.SetGlowEnabled(_settingsService.PianoKeyGlowEnabled);
        
        // Refresh instruments list (in case order changed or instruments were added/removed)
        _sfzService.RefreshInstruments();
        
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
        
        UpdateLayout();
        
        // Re-run pad setup for size-dependent elements
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
            UpdateLayout();
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
            
            // Check if there's already a loaded instrument (e.g., from Instrument Detail page)
            var currentInstrument = _sfzService.CurrentInstrumentName;
            if (!string.IsNullOrEmpty(currentInstrument))
            {
                var index = instruments.ToList().IndexOf(currentInstrument);
                if (index >= 0)
                {
                    InstrumentPicker.SelectedIndex = index;
                    // Just update the UI, don't reload the instrument
                    SetupPadMatrix();
                    LoadingLabel.IsVisible = false;
                    return;
                }
            }
            
            // No instrument loaded yet, select first one
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
        
        // Update volume knob visibility based on padrea type
        UpdateVolumeKnobState(padrea);

        if (padrea?.Kind == PadreaKind.Piano)
        {
            SetupPianoPadrea(padrea!, instrumentMinKey, instrumentMaxKey);
        }
        else if (padrea?.Kind == PadreaKind.PitchVolume)
        {
            SetupPitchVolumePadrea(padrea!, instrumentMinKey, instrumentMaxKey);
        }
        else
        {
            SetupGridPadrea(padrea, instrumentMinKey, instrumentMaxKey);
        }
    }
    
    private void UpdateVolumeKnobState(Padrea? padrea)
    {
        // Disable volume knob when PitchVolume padrea is active (volume is controlled by Y position)
        bool isPitchVolume = padrea?.Kind == PadreaKind.PitchVolume;
        VolumeKnob.IsVisible = !isPitchVolume;
        VolumeKnob.IsEnabled = !isPitchVolume;
    }
    
    private void SetupPitchVolumePadrea(Padrea padrea, int instrumentMinKey, int instrumentMaxKey)
    {
        _pitchVolumeDrawable.SetNoteRange(instrumentMinKey, instrumentMaxKey);
        EnsurePadGraphicsView(_pitchVolumeDrawable);
    }

    private void SetupGridPadrea(Padrea? padrea, int instrumentMinKey, int instrumentMaxKey)
    {
        if (padrea == null)
        {
            // Fallback to simple range
            _padDrawable.SetKeyRange(instrumentMinKey, instrumentMaxKey);
            _padDrawable.SetHalftoneDetector(null);
            _padDrawable.SetEnvelopeLevelGetter(_sfzService.GetNoteEnvelopeLevel);
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
            _padDrawable.SetEnvelopeLevelGetter(_sfzService.GetNoteEnvelopeLevel);
            _padDrawable.SetGlowEnabled(_settingsService.PadGlowEnabled);
        }

        EnsurePadGraphicsView(_padDrawable);
        EnsureEnvelopeAnimationTimer();
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
        _pianoDrawable.SetEnvelopeLevelGetter(_sfzService.GetNoteEnvelopeLevel);
        _pianoDrawable.SetGlowEnabled(_settingsService.PianoKeyGlowEnabled);
        
        EnsurePadGraphicsView(_pianoDrawable);
        EnsureEnvelopeAnimationTimer();
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
        else if (IsCurrentPadreaPitchVolume())
        {
            // For pitch-volume, we use index 0 as pointer ID for simplicity
            if (touches.Count > 0)
            {
                _pitchVolumeDrawable.OnTouchStart(0, touches[0].X, touches[0].Y);
            }
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
        else if (IsCurrentPadreaPitchVolume())
        {
            if (touches.Count > 0)
            {
                _pitchVolumeDrawable.OnTouchMove(0, touches[0].X, touches[0].Y);
            }
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
        else if (IsCurrentPadreaPitchVolume())
        {
            _pitchVolumeDrawable.OnTouchEnd(0);
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
        else if (IsCurrentPadreaPitchVolume())
        {
            _pitchVolumeDrawable.OnAllTouchesEnd();
        }
        else
        {
            _padDrawable.OnAllTouchesEnd();
        }
        _padGraphicsView?.Invalidate();
    }

    private void OnNoteOn(object? sender, int midiNote)
    {
        // Apply harmony (generates chord from single note)
        var notes = _harmony.ProcessNoteOn(midiNote);
        
        if (_arpeggiator.IsEnabled)
        {
            // When arpeggiator is enabled, add notes to it (don't play directly)
            foreach (var note in notes)
            {
                _arpeggiator.AddNote(note);
            }
        }
        else
        {
            // Play notes directly
            foreach (var note in notes)
            {
                _sfzService.NoteOn(note);
            }
        }
    }

    private void OnNoteOff(object? sender, int midiNote)
    {
        // Get all notes that were generated for this root note
        var notes = _harmony.ProcessNoteOff(midiNote);
        
        if (_arpeggiator.IsEnabled)
        {
            // Remove notes from arpeggiator
            foreach (var note in notes)
            {
                _arpeggiator.RemoveNote(note);
            }
        }
        else
        {
            // Stop notes directly
            foreach (var note in notes)
            {
                _sfzService.NoteOff(note);
            }
        }
    }
    
    private void OnPitchVolumeNoteOn(object? sender, PitchVolumeEventArgs e)
    {
        // For pitch-volume surface, velocity is controlled by Y position
        int velocity = (int)(e.Volume * 127);
        velocity = Math.Clamp(velocity, 1, 127);
        
        // Apply harmony
        var notes = _harmony.ProcessNoteOn(e.MidiNote);
        
        if (_arpeggiator.IsEnabled)
        {
            foreach (var note in notes)
            {
                _arpeggiator.AddNote(note);
            }
        }
        else
        {
            foreach (var note in notes)
            {
                _sfzService.NoteOn(note, velocity);
            }
        }
    }
    
    private void OnPitchVolumeChanged(object? sender, PitchVolumeEventArgs e)
    {
        // Volume changes during a sustained note - for future implementation
        // Could be used for continuous controller messages or velocity aftertouch
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
        
        // Update layout after visibility change
        UpdateLayout();
        
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
    
    private bool IsCurrentPadreaPitchVolume() => _padreaService.CurrentPadrea?.Kind == PadreaKind.PitchVolume;

    private void EnsurePadGraphicsView(IDrawable drawable)
    {
        if (_padGraphicsView == null)
        {
            _padGraphicsView = new GraphicsView
            {
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

    private async void OnHamburgerMenuClicked(object? sender, EventArgs e)
    {
        // Show action sheet with menu options
        var action = await DisplayActionSheet("Menu", "Cancel", null, "Instruments", "Import Instrument", "Settings");
        
        switch (action)
        {
            case "Instruments":
                await Shell.Current.GoToAsync(nameof(Views.InstrumentsPage));
                break;
            case "Import Instrument":
                await Shell.Current.GoToAsync(nameof(Views.ImportInstrumentPage));
                break;
            case "Settings":
                await Shell.Current.GoToAsync(nameof(Views.SettingsPage));
                break;
        }
    }
}
