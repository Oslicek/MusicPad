using MusicPad.Controls;
using MusicPad.Core.Models;
using MusicPad.Core.NoteProcessing;
using MusicPad.Core.Recording;
using MusicPad.Core.Theme;
using MusicPad.Services;

namespace MusicPad;

public partial class MainPage : ContentPage
{
    private readonly ISfzService _sfzService;
    private readonly IPadreaService _padreaService;
    private readonly ISettingsService _settingsService;
    private readonly IInstrumentConfigService _instrumentConfigService;
    private readonly IRecordingService _recordingService;
    private readonly PadMatrixDrawable _padDrawable;
    private readonly RotaryKnobDrawable _volumeKnobDrawable;
    private readonly EffectAreaDrawable _effectAreaDrawable;
    private readonly NavigationBarDrawable _navigationBarDrawable;
    private readonly RecAreaDrawable _recAreaDrawable;
    private readonly List<ScaleOption> _scaleOptions = new();
    private readonly PianoKeyboardDrawable _pianoDrawable = new();
    private readonly PitchVolumeDrawable _pitchVolumeDrawable = new();
    private PianoRangeManager? _pianoRangeManager;
    private GraphicsView? _padGraphicsView;
    private bool _isLoading;
    private bool _isLandscape;
    private double _pageWidth;
    private double _pageHeight;
    
    // Harmony (chord effect) - may be disabled for monophonic instruments
    private readonly Harmony _harmony = new();
    private bool _harmonyAllowed = true; // False for monophonic instruments
    
    // Envelope animation timer for pad glow effect
    private IDispatcherTimer? _envelopeAnimationTimer;

    public MainPage(ISfzService sfzService, IPadreaService padreaService, ISettingsService settingsService, IInstrumentConfigService instrumentConfigService, IRecordingService recordingService)
    {
        InitializeComponent();
        _sfzService = sfzService;
        _padreaService = padreaService;
        _settingsService = settingsService;
        _instrumentConfigService = instrumentConfigService;
        _recordingService = recordingService;
        _padDrawable = new PadMatrixDrawable();
        _padDrawable.NoteOn += OnNoteOn;
        _padDrawable.NoteOff += OnNoteOff;
        
        // Setup navigation bar (mute + arrows + page dots)
        _navigationBarDrawable = new NavigationBarDrawable();
        _navigationBarDrawable.MuteClicked += OnMuteButtonClicked;
        _navigationBarDrawable.NavigateUp += OnNavigateUp;
        _navigationBarDrawable.NavigateDown += OnNavigateDown;
        _navigationBarDrawable.InvalidateRequested += (s, e) => NavigationBar?.Invalidate();
        SetupNavigationBar();
        
        // Setup recording area
        _recAreaDrawable = new RecAreaDrawable();
        _recAreaDrawable.RecordClicked += OnRecordClicked;
        _recAreaDrawable.StopClicked += OnStopClicked;
        _recAreaDrawable.PlayClicked += OnPlayClicked;
        _recAreaDrawable.InvalidateRequested += (s, e) => RecArea?.Invalidate();
        SetupRecArea();
        
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
            
            // Update navigation bar
            NavigationBar?.Invalidate();
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
    
    private void SetupNavigationBar()
    {
        NavigationBar.Drawable = _navigationBarDrawable;
        
        NavigationBar.StartInteraction += (s, e) =>
        {
            var touch = e.Touches.FirstOrDefault();
            if (touch != default)
            {
                if (_navigationBarDrawable.OnTouchStart((float)touch.X, (float)touch.Y))
                {
                    NavigationBar.Invalidate();
                }
            }
        };
    }
    
    private void SetupRecArea()
    {
        RecArea.Drawable = _recAreaDrawable;
        
        RecArea.StartInteraction += (s, e) =>
        {
            var touch = e.Touches.FirstOrDefault();
            if (touch != default)
            {
                _recAreaDrawable.OnTouchStart(new PointF((float)touch.X, (float)touch.Y));
                RecArea.Invalidate();
            }
        };
        
        // Wire up playback events
        _recordingService.PlaybackNoteEvent += OnPlaybackNoteEvent;
        _recordingService.PlaybackStateChanged += (s, isPlaying) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                _recAreaDrawable.IsPlaying = isPlaying;
                if (!isPlaying)
                {
                    _recAreaDrawable.StatusText = "Ready";
                }
                RecArea.Invalidate();
            });
        };
        _recordingService.RecordingStateChanged += (s, isRecording) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                _recAreaDrawable.IsRecording = isRecording;
                RecArea.Invalidate();
            });
        };
    }
    
    private void OnRecordClicked(object? sender, EventArgs e)
    {
        var instrumentId = InstrumentPicker.SelectedItem?.ToString();
        _recordingService.StartRecording(instrumentId, null);
        _recAreaDrawable.StatusText = "● REC";
        RecArea.Invalidate();
    }
    
    private async void OnStopClicked(object? sender, EventArgs e)
    {
        if (_recordingService.IsRecording)
        {
            var song = await _recordingService.StopRecordingAsync();
            if (song != null)
            {
                _recAreaDrawable.StatusText = $"Saved: {song.Name}";
            }
            else
            {
                _recAreaDrawable.StatusText = "Ready";
            }
        }
        else if (_recordingService.IsPlaying)
        {
            _recordingService.StopPlayback();
            _sfzService.StopAll();
        }
        RecArea.Invalidate();
    }
    
    private async void OnPlayClicked(object? sender, EventArgs e)
    {
        // Get the most recent song and play it
        var songs = await _recordingService.GetSongsAsync();
        if (songs.Count > 0)
        {
            var latestSong = songs[0]; // Already sorted by CreatedAt descending
            if (await _recordingService.LoadSongAsync(latestSong.Id))
            {
                _recAreaDrawable.StatusText = $"▶ {latestSong.Name}";
                RecArea.Invalidate();
                _recordingService.StartPlayback(liveMode: true); // Use current instrument
            }
        }
        else
        {
            _recAreaDrawable.StatusText = "No recordings";
            RecArea.Invalidate();
        }
    }
    
    private void OnPlaybackNoteEvent(object? sender, RecordedEvent evt)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (evt.EventType == RecordedEventType.NoteOn)
            {
                OnNoteOn(this, evt.MidiNote);
            }
            else if (evt.EventType == RecordedEventType.NoteOff)
            {
                OnNoteOff(this, evt.MidiNote);
            }
        });
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
        harmonySettings.TypeChanged += (s, type) => OnHarmonyTypeChanged(type);
        
        // Apply initial harmony settings
        _harmony.IsEnabled = harmonySettings.IsEnabled;
        _harmony.Type = harmonySettings.Type;
        
        // Wire up Arpeggiator settings - uses audio-thread arpeggiator for sample-accurate timing
        var arpSettings = _effectAreaDrawable.ArpSettings;
        var audioArp = _sfzService.Arpeggiator;
        
        arpSettings.EnabledChanged += (s, enabled) =>
        {
            audioArp.IsEnabled = enabled;
        };
        arpSettings.RateChanged += (s, rate) =>
        {
            audioArp.SetRate(rate);
        };
        arpSettings.PatternChanged += (s, pattern) => audioArp.Pattern = pattern;
        
        // Apply initial arpeggiator settings
        audioArp.IsEnabled = arpSettings.IsEnabled;
        audioArp.SetRate(arpSettings.Rate);
        audioArp.Pattern = arpSettings.Pattern;
    }
    
    private void OnHarmonyTypeChanged(HarmonyType newType)
    {
        var audioArp = _sfzService.Arpeggiator;
        
        // If arpeggiator is active with notes, we need to update the notes live
        if (audioArp.IsEnabled && audioArp.ActiveNotes.Count > 0)
        {
            // Get notes to add/remove based on new harmony type
            var (notesToRemove, notesToAdd) = _harmony.ReharmonizeActiveNotes(newType);
            
            // Update the arpeggiator
            foreach (var note in notesToRemove)
            {
                audioArp.RemoveNote(note);
            }
            foreach (var note in notesToAdd)
            {
                audioArp.AddNote(note);
            }
        }
        else
        {
            // Just update the type for future notes
            _harmony.Type = newType;
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
        double controlsWidth = 150 + 16; // picker width + margins
        double controlsHeight = GetControlsStackHeight();
        double volumeSize = 120;
        double padding = 8;

        // Navigation and recording area heights
        double navBarHeight = 50;
        double recAreaHeight = 44;

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
                
                // Recording area above navigation bar
                RecArea.HorizontalOptions = LayoutOptions.Center;
                RecArea.VerticalOptions = LayoutOptions.End;
                RecArea.WidthRequest = _pageWidth - padding * 2;
                RecArea.Margin = new Thickness(0, 0, 0, pianoHeight + navBarHeight + padding * 2);

                // Navigation bar above the piano
                NavigationBar.HorizontalOptions = LayoutOptions.Center;
                NavigationBar.VerticalOptions = LayoutOptions.End;
                NavigationBar.WidthRequest = _pageWidth - padding * 2;
                NavigationBar.Margin = new Thickness(0, 0, 0, pianoHeight + padding);

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
                double padreaCenterX = _pageWidth / 2;
                double padreaLeft = padreaCenterX - padreaSize / 2;

                // Recording area in landscape square mode
                RecArea.HorizontalOptions = LayoutOptions.Center;
                RecArea.VerticalOptions = LayoutOptions.Start;
                RecArea.WidthRequest = padreaSize;
                RecArea.Margin = new Thickness(0, controlsHeight + 16, 0, 0);
                
                // Navigation bar above the padrea (landscape square mode)
                NavigationBar.HorizontalOptions = LayoutOptions.Center;
                NavigationBar.VerticalOptions = LayoutOptions.Start;
                NavigationBar.WidthRequest = padreaSize;
                NavigationBar.Margin = new Thickness(0, controlsHeight + 16 + recAreaHeight + padding, 0, 0);

                PadContainer.HorizontalOptions = LayoutOptions.Center;
                PadContainer.VerticalOptions = LayoutOptions.Center;
                PadContainer.WidthRequest = padreaSize;
                PadContainer.HeightRequest = padreaSize;
                PadContainer.Margin = new Thickness(0);

                // Effect area on the right side - constrained width
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

            // Calculate sizes - efarea needs height for 5 effect buttons (30px each + spacing)
            double topAreaHeight = Math.Max(controlsHeight, volumeSize) + padding;
            double efareaHeight = 165; // Height for 5 buttons: 5*30 + 4*4 spacing + margins
            
            _effectAreaDrawable.SetOrientation(false); // Vertical buttons on left
            _effectAreaDrawable.SetLandscapeSquare(false);
            EffectArea.HorizontalOptions = LayoutOptions.Fill;
            EffectArea.VerticalOptions = LayoutOptions.Start;
            EffectArea.WidthRequest = _pageWidth - padding * 2;
            EffectArea.HeightRequest = efareaHeight;
            EffectArea.Margin = new Thickness(0, topAreaHeight + padding, 0, 0);
            
            double padreaTop = topAreaHeight + efareaHeight + padding * 2;
            // Available space for padrea + navigation bar + recarea (bottom padding is in Grid)
            double availableForPadrea = _pageHeight - padreaTop - navBarHeight - recAreaHeight - padding * 2;
            
            if (isPiano)
            {
                // Piano padrea - full width
                double pianoHeight = Math.Min(_pageHeight * 0.42, availableForPadrea);
                
                // Recording area above the navigation bar  
                RecArea.HorizontalOptions = LayoutOptions.Center;
                RecArea.VerticalOptions = LayoutOptions.End;
                RecArea.WidthRequest = _pageWidth - padding * 2;
                RecArea.Margin = new Thickness(0, 0, 0, pianoHeight + navBarHeight + padding);
                
                // Navigation bar above the piano
                NavigationBar.HorizontalOptions = LayoutOptions.Center;
                NavigationBar.VerticalOptions = LayoutOptions.End;
                NavigationBar.WidthRequest = _pageWidth - padding * 2;
                NavigationBar.Margin = new Thickness(0, 0, 0, pianoHeight + padding);

                PadContainer.HorizontalOptions = LayoutOptions.Fill;
                PadContainer.VerticalOptions = LayoutOptions.End;
                PadContainer.WidthRequest = _pageWidth - padding * 2;
                PadContainer.HeightRequest = pianoHeight;
                PadContainer.Margin = new Thickness(0);
            }
            else
            {
                // Square padrea - centered, with mute button above the padrea
                double padreaSize = Math.Min(_pageWidth - padding * 2, availableForPadrea);

                // Recording area above the navigation bar
                RecArea.HorizontalOptions = LayoutOptions.Center;
                RecArea.VerticalOptions = LayoutOptions.End;
                RecArea.WidthRequest = padreaSize;
                RecArea.Margin = new Thickness(0, 0, 0, padreaSize + navBarHeight + padding);
                
                // Navigation bar above the padrea
                NavigationBar.HorizontalOptions = LayoutOptions.Center;
                NavigationBar.VerticalOptions = LayoutOptions.End;
                NavigationBar.WidthRequest = padreaSize;
                NavigationBar.Margin = new Thickness(0, 0, 0, padreaSize + padding);

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

    private void LoadPadreas(Core.Models.PitchType pitchType = Core.Models.PitchType.Pitched)
    {
        // Filter padreas based on instrument pitch type
        var padreas = _padreaService.GetPadreasForPitchType(pitchType);
        PadreaPicker.ItemsSource = padreas.ToList();
        ScalePicker.ItemsSource = _scaleOptions.ToList();
        
        // Select current padrea if it's in the filtered list
        var currentPadrea = _padreaService.CurrentPadrea;
        if (currentPadrea != null)
        {
            var index = padreas.ToList().FindIndex(p => p.Id == currentPadrea.Id);
            if (index >= 0)
            {
                PadreaPicker.SelectedIndex = index;
            }
            else if (padreas.Count > 0)
            {
                // Current padrea not in filtered list, select first available
                PadreaPicker.SelectedIndex = 0;
            }
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
            
            // Apply voicing mode from instrument config
            await ApplyInstrumentVoicingModeAsync(instrumentName);
            
            // Center the viewpage to show middle of instrument range
            CenterPadreaViewpage();
            
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
    
    private async Task ApplyInstrumentVoicingModeAsync(string instrumentName)
    {
        try
        {
            // Get all instruments to find the matching config
            var allInstruments = await _instrumentConfigService.GetAllInstrumentsAsync();
            var config = allInstruments.FirstOrDefault(c => c.DisplayName == instrumentName);
            
            if (config != null)
            {
                _sfzService.VoicingMode = config.Voicing;
                
                // Disable harmony for monophonic instruments (chords don't make sense)
                bool isMonophonic = config.Voicing == VoicingType.Monophonic;
                _harmonyAllowed = !isMonophonic;
                
                // Update the harmony settings to disable the UI
                var harmonySettings = _effectAreaDrawable.HarmonySettings;
                harmonySettings.IsAllowed = !isMonophonic;
                
                // If switching to monophonic and harmony was on, turn it off
                if (isMonophonic && harmonySettings.IsEnabled)
                {
                    harmonySettings.IsEnabled = false;
                    _harmony.IsEnabled = false;
                    _harmony.Reset();
                }
                
                // Redraw effect area to show disabled state
                EffectArea.Invalidate();
                
                // Reload padreas filtered by pitch type
                // This automatically restricts the picker to only show appropriate padreas
                LoadPadreas(config.PitchType);
            }
            else
            {
                // Default to polyphonic if config not found
                _sfzService.VoicingMode = Core.Models.VoicingType.Polyphonic;
            }
        }
        catch
        {
            // Default to polyphonic on error
            _sfzService.VoicingMode = Core.Models.VoicingType.Polyphonic;
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
            _padDrawable.SetLabelGetter(null);
        }
        else if (padrea.Kind == PadreaKind.Unpitched)
        {
            // Unpitched padrea: use unique MIDI notes from the instrument
            var uniqueNotes = _sfzService.CurrentUniqueMidiNotes.ToList();
            
            if (uniqueNotes.Count == 0)
            {
                // Fallback to instrument range if no unique notes
                uniqueNotes = Enumerable.Range(instrumentMinKey, instrumentMaxKey - instrumentMinKey + 1).ToList();
            }
            
            // Calculate viewpage for unpitched
            int columns = padrea.Columns ?? 4;
            int rowsPerViewpage = padrea.RowsPerViewpage ?? 4;
            int notesPerViewpage = columns * rowsPerViewpage;
            int startIndex = padrea.CurrentViewpage * notesPerViewpage;
            
            if (startIndex >= uniqueNotes.Count)
            {
                startIndex = 0;
                padrea.CurrentViewpage = 0;
            }
            
            var pageNotes = uniqueNotes.Skip(startIndex).Take(notesPerViewpage).ToList();
            
            if (pageNotes.Count == 0)
            {
                LoadingLabel.IsVisible = true;
                LoadingLabel.Text = "No sounds";
                return;
            }
            
            // Navigation is now in NavigationBar, no arrows in pad matrix
            _padDrawable.SetNotes(pageNotes, columns, false, false);
            _padDrawable.SetColors(padrea.PadColor, padrea.PadPressedColor, 
                                   padrea.PadAltColor, padrea.PadAltPressedColor);
            _padDrawable.SetHalftoneDetector(null); // No halftone distinction for unpitched
            _padDrawable.SetEnvelopeLevelGetter(_sfzService.GetNoteEnvelopeLevel);
            _padDrawable.SetLabelGetter(_sfzService.GetNoteLabel);
            _padDrawable.SetGlowEnabled(_settingsService.PadGlowEnabled);
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
            
            // Navigation is now in NavigationBar, no arrows in pad matrix
            _padDrawable.SetNotes(notes, padrea.Columns, false, false);
            _padDrawable.SetColors(padrea.PadColor, padrea.PadPressedColor, 
                                   padrea.PadAltColor, padrea.PadAltPressedColor);
            _padDrawable.SetHalftoneDetector(padrea.IsHalftone);
            _padDrawable.SetEnvelopeLevelGetter(_sfzService.GetNoteEnvelopeLevel);
            _padDrawable.SetLabelGetter(null); // No labels for pitched instruments
            _padDrawable.SetGlowEnabled(_settingsService.PadGlowEnabled);
        }

        EnsurePadGraphicsView(_padDrawable);
        EnsureEnvelopeAnimationTimer();
        UpdateNavigationBarPageInfo();
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
        UpdateNavigationBarPageInfo();
    }

    private void UpdatePadMatrixForPadrea()
    {
        // Only update if we have an instrument loaded
        if (_sfzService.CurrentInstrumentName != null)
        {
            SetupPadMatrix();
        }
    }
    
    /// <summary>
    /// Centers the padrea viewpage to show the middle of the instrument range.
    /// </summary>
    private void CenterPadreaViewpage()
    {
        var padrea = _padreaService.CurrentPadrea;
        if (padrea == null) return;
        
        // Only center for grid-type padreas (not piano or pitch-volume)
        if (padrea.Kind == PadreaKind.Piano || padrea.Kind == PadreaKind.PitchVolume)
            return;
        
        var (minKey, maxKey) = _sfzService.CurrentKeyRange;
        padrea.CenterViewpage(minKey, maxKey);
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
        // Record raw pad touch (before harmony/arpeggiator processing)
        _recordingService.RecordNoteOn(midiNote);
        
        // Apply harmony (generates chord from single note) if allowed
        var notes = _harmonyAllowed ? _harmony.ProcessNoteOn(midiNote) : new[] { midiNote };
        var audioArp = _sfzService.Arpeggiator;
        
        if (audioArp.IsEnabled)
        {
            // When arpeggiator is enabled, add notes to audio-thread arpeggiator
            foreach (var note in notes)
            {
                audioArp.AddNote(note);
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
        // Record raw pad release (before harmony/arpeggiator processing)
        _recordingService.RecordNoteOff(midiNote);
        
        // Get all notes that were generated for this root note
        var notes = _harmonyAllowed ? _harmony.ProcessNoteOff(midiNote) : new[] { midiNote };
        var audioArp = _sfzService.Arpeggiator;
        
        if (audioArp.IsEnabled)
        {
            // Remove notes from audio-thread arpeggiator
            foreach (var note in notes)
            {
                audioArp.RemoveNote(note);
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
        
        // Apply harmony if allowed
        var notes = _harmonyAllowed ? _harmony.ProcessNoteOn(e.MidiNote) : new[] { e.MidiNote };
        var audioArp = _sfzService.Arpeggiator;
        
        if (audioArp.IsEnabled)
        {
            foreach (var note in notes)
            {
                audioArp.AddNote(note);
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
            UpdateNavigationBarPageInfo();
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
            UpdateNavigationBarPageInfo();
        }
    }
    
    private void UpdateNavigationBarPageInfo()
    {
        var padrea = _padreaService.CurrentPadrea;
        if (padrea == null)
        {
            _navigationBarDrawable.TotalPages = 1;
            _navigationBarDrawable.CurrentPage = 0;
            return;
        }
        
        var (minKey, maxKey) = _sfzService.CurrentKeyRange;
        int totalPages = padrea.GetTotalViewpages(minKey, maxKey);
        _navigationBarDrawable.TotalPages = totalPages;
        _navigationBarDrawable.CurrentPage = padrea.CurrentViewpage;
        NavigationBar?.Invalidate();
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

    private void OnMuteButtonClicked(object? sender, EventArgs e)
    {
        _sfzService.Mute();
        
        // Brief visual feedback via NavigationBar
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            _navigationBarDrawable.IsMuted = true;
            NavigationBar?.Invalidate();
            await Task.Delay(100);
            _navigationBarDrawable.IsMuted = false;
            NavigationBar?.Invalidate();
        });
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
