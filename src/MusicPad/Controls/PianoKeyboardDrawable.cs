using Microsoft.Maui.Graphics;
using MusicPad.Core.Theme;

namespace MusicPad.Controls;

/// <summary>
/// Drawable for a piano-style keyboard with a mini 88-key strip and draggable range.
/// </summary>
public class PianoKeyboardDrawable : IDrawable
{
    private const int GlobalMin = 21;  // A0
    private const int GlobalMax = 108; // C8
    
    // Envelope glow color - follows the sound's amplitude envelope (dynamic for palette switching)
    private static Color EnvelopeGlowColor => Color.FromArgb(AppColors.Accent);

    private int _rangeStart;
    private int _rangeEnd;
    private int _instrumentMin;
    private int _instrumentMax;
    private bool _isLandscape;
    private Func<int, float>? _envelopeLevelGetter; // Gets envelope level for a note (0-1)
    private bool _glowEnabled = true; // Whether envelope glow effect is enabled

    private readonly List<KeyRect> _keyRects = new();
    private readonly HashSet<int> _activeNotes = new();

    private RectF _leftArrowRect;
    private RectF _rightArrowRect;
    private RectF _stripRect;
    private RectF _stripHighlight;
    private bool _draggingStrip;
    private float _dragOffset;

    public event EventHandler<int>? NoteOn;
    public event EventHandler<int>? NoteOff;
    public event EventHandler<int>? ShiftRequested;      // semitone delta (arrows)
    public event EventHandler<int>? StripDragRequested;  // new start note when dragging highlight

    public void SetRange(int start, int end, int instrumentMin, int instrumentMax, bool isLandscape)
    {
        _rangeStart = Math.Clamp(start, GlobalMin, GlobalMax);
        _rangeEnd = Math.Clamp(end, GlobalMin, GlobalMax);
        _instrumentMin = Math.Clamp(instrumentMin, GlobalMin, GlobalMax);
        _instrumentMax = Math.Clamp(instrumentMax, GlobalMin, GlobalMax);
        _isLandscape = isLandscape;
    }
    
    /// <summary>
    /// Sets the envelope level getter for visual feedback that follows the sound's amplitude.
    /// The function should return 0-1 based on the current envelope level of the note.
    /// </summary>
    public void SetEnvelopeLevelGetter(Func<int, float>? getter)
    {
        _envelopeLevelGetter = getter;
    }
    
    /// <summary>
    /// Enables or disables the envelope glow effect.
    /// </summary>
    public void SetGlowEnabled(bool enabled)
    {
        _glowEnabled = enabled;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (_rangeEnd <= _rangeStart)
            return;

        _keyRects.Clear();

        float stripHeight = 26f;
        float arrowHeight = stripHeight;
        float arrowWidth = 28f;
        float arrowMargin = 6f;

        // Layout regions (left/right arrows)
        _leftArrowRect = new RectF(dirtyRect.X + arrowMargin,
                                   dirtyRect.Y + arrowMargin,
                                   arrowWidth,
                                   arrowHeight);

        _rightArrowRect = new RectF(dirtyRect.Right - arrowMargin - arrowWidth,
                                    dirtyRect.Y + arrowMargin,
                                    arrowWidth,
                                    arrowHeight);

        _stripRect = new RectF(_leftArrowRect.Right + 4,
                               dirtyRect.Y + arrowMargin,
                               dirtyRect.Width - (arrowWidth * 2) - (arrowMargin * 2) - 8,
                               stripHeight);

        float keyboardTop = _stripRect.Bottom + 6;
        float keyboardHeight = dirtyRect.Bottom - keyboardTop;
        if (!_isLandscape)
        {
            keyboardHeight *= 0.6f; // make it shorter in portrait so it's rectangular
        }

        // Draw strip (88 keys)
        DrawStrip(canvas);

        // Draw main keyboard
        var keyboardRect = new RectF(dirtyRect.X, keyboardTop, dirtyRect.Width, keyboardHeight);
        DrawKeyboard(canvas, keyboardRect);
    }

    private void DrawStrip(ICanvas canvas)
    {
        int totalKeys = GlobalMax - GlobalMin + 1; // 88
        float keyWidth = _stripRect.Width / totalKeys;

        // Highlight for current range
        float highlightX = _stripRect.X + (_rangeStart - GlobalMin) * keyWidth;
        float highlightWidth = (_rangeEnd - _rangeStart + 1) * keyWidth;
        _stripHighlight = new RectF(highlightX, _stripRect.Y, highlightWidth, _stripRect.Height);

        // Draw white keys
        for (int i = 0; i < totalKeys; i++)
        {
            int midi = GlobalMin + i;
            float x = _stripRect.X + i * keyWidth;
            var keyRect = new RectF(x, _stripRect.Y, keyWidth, _stripRect.Height);
            bool isBlack = IsBlack(midi);

            if (!isBlack)
            {
                canvas.FillColor = Colors.White;
                canvas.FillRectangle(keyRect);
            }
        }

        // Draw instrument range highlight underlay
        int instStart = Math.Max(_instrumentMin, GlobalMin);
        int instEnd = Math.Min(_instrumentMax, GlobalMax);
        float instX = _stripRect.X + (instStart - GlobalMin) * keyWidth;
        float instW = (instEnd - instStart + 1) * keyWidth;
        var instRect = new RectF(instX, _stripRect.Y, instW, _stripRect.Height);
        canvas.FillColor = Color.FromArgb(AppColors.PianoStripHighlight);
        canvas.FillRectangle(instRect);

        // Draw current window highlight
        canvas.FillColor = Color.FromArgb(AppColors.PianoStripInactive);
        canvas.FillRectangle(_stripHighlight);

        // Draw black keys on top - much shorter outside selection, taller inside
        for (int i = 0; i < totalKeys; i++)
        {
            int midi = GlobalMin + i;
            if (!IsBlack(midi)) continue;
            float x = _stripRect.X + i * keyWidth;
            
            // Check if this key is inside the selection rectangle
            bool insideSelection = midi >= _rangeStart && midi <= _rangeEnd;
            // Make difference very dramatic: 65% inside, 18% outside
            float blackKeyHeight = insideSelection ? _stripRect.Height * 0.65f : _stripRect.Height * 0.18f;
            
            var keyRect = new RectF(x, _stripRect.Y, keyWidth, blackKeyHeight);
            canvas.FillColor = Color.FromArgb(AppColors.PianoBlackKeyDark);
            canvas.FillRectangle(keyRect);
        }

        // Outlines
        canvas.StrokeColor = Color.FromArgb(AppColors.Disabled);
        canvas.StrokeSize = 1;
        canvas.DrawRectangle(_stripRect);

        canvas.StrokeColor = Colors.White.WithAlpha(0.7f);
        canvas.DrawRectangle(_stripHighlight);
    }

    private void DrawKeyboard(ICanvas canvas, RectF rect)
    {
        var visibleNotes = Enumerable.Range(_rangeStart, _rangeEnd - _rangeStart + 1).ToList();
        int whiteCount = visibleNotes.Count(n => !IsBlack(n));
        if (whiteCount == 0) return;

        float whiteWidth = rect.Width / whiteCount;
        float whiteHeight = rect.Height;
        float blackWidth = whiteWidth * 0.6f;
        float blackHeight = rect.Height * 0.6f;

        float currentX = rect.X;

        // First pass: draw white keys
        foreach (var note in visibleNotes)
        {
            if (IsBlack(note))
                continue;

            var keyRect = new RectF(currentX, rect.Y, whiteWidth, whiteHeight);
            bool disabled = note < _instrumentMin || note > _instrumentMax;
            bool isTouched = _activeNotes.Contains(note);
            
            // Get envelope level for this note
            float envelopeLevel = _envelopeLevelGetter?.Invoke(note) ?? 0f;
            bool isPlaying = envelopeLevel > 0.001f;
            
            // Determine key color
            Color keyColor;
            if (disabled)
            {
                keyColor = Color.FromArgb(AppColors.DisabledDark);
            }
            else if (_glowEnabled && isPlaying)
            {
                // Glow enabled: animate color based on envelope
                float glowIntensity = (float)Math.Pow(envelopeLevel, 0.7);
                keyColor = InterpolateColor(Colors.White, EnvelopeGlowColor, glowIntensity * 0.7f);
            }
            else if (isTouched || isPlaying)
            {
                // Glow disabled or touch feedback: static pressed color (sky blue tint)
                keyColor = Color.FromArgb(AppColors.PadChromaticPressed);
            }
            else
            {
                keyColor = Colors.White;
            }

            canvas.FillColor = keyColor;
            canvas.FillRectangle(keyRect);

            // Draw border - only changes with glow enabled
            if (_glowEnabled && isPlaying && !disabled)
            {
                float glowIntensity = (float)Math.Pow(envelopeLevel, 0.7);
                canvas.StrokeColor = EnvelopeGlowColor.WithAlpha(glowIntensity * 0.9f);
                canvas.StrokeSize = 2 + envelopeLevel * 2;
                canvas.DrawRectangle(keyRect);
                
                // Inner highlight
                canvas.StrokeColor = EnvelopeGlowColor.WithAlpha(0.3f + glowIntensity * 0.4f);
                canvas.StrokeSize = 1;
                canvas.DrawRectangle(keyRect.X + 1, keyRect.Y + 1, keyRect.Width - 2, keyRect.Height - 2);
            }
            else
            {
                // No outline color change when glow is off (per user request)
                canvas.StrokeColor = Color.FromArgb(AppColors.BorderDark);
                canvas.StrokeSize = 1;
                canvas.DrawRectangle(keyRect);
            }

            // Label
            canvas.FontSize = 12;
            canvas.FontColor = disabled ? Color.FromArgb(AppColors.DisabledTextLight) : Color.FromArgb(AppColors.TextDark);
            canvas.DrawString(GetNoteName(note), keyRect, HorizontalAlignment.Center, VerticalAlignment.Bottom);

            _keyRects.Add(new KeyRect(note, keyRect, false, disabled));
            currentX += whiteWidth;
        }

        // Second pass: draw black keys
        currentX = rect.X;
        foreach (var note in visibleNotes)
        {
            if (IsBlack(note))
            {
                // Position black key between adjacent whites
                int noteIndex = visibleNotes.IndexOf(note);
                int whitesBefore = visibleNotes.Take(noteIndex).Count(n => !IsBlack(n));
                float bx = rect.X + whitesBefore * whiteWidth + whiteWidth - blackWidth / 2;
                var keyRect = new RectF(bx, rect.Y, blackWidth, blackHeight);
                bool disabled = note < _instrumentMin || note > _instrumentMax;
                bool isTouched = _activeNotes.Contains(note);
                
                // Get envelope level for this note
                float envelopeLevel = _envelopeLevelGetter?.Invoke(note) ?? 0f;
                bool isPlaying = envelopeLevel > 0.001f;
                
                // Determine key color
                Color keyColor;
                if (disabled)
                {
                    keyColor = Color.FromArgb(AppColors.DisabledDarker);
                }
                else if (_glowEnabled && isPlaying)
                {
                    // Glow enabled: animate color based on envelope
                    float glowIntensity = (float)Math.Pow(envelopeLevel, 0.7);
                    keyColor = InterpolateColor(Color.FromArgb(AppColors.PianoBlackKeyDark), EnvelopeGlowColor, glowIntensity * 0.6f);
                }
                else if (isTouched || isPlaying)
                {
                    // Glow disabled or touch feedback: static pressed color (teal tint)
                    keyColor = Color.FromArgb(AppColors.PrimaryDark);
                }
                else
                {
                    keyColor = Color.FromArgb(AppColors.PianoBlackKeyDark);
                }

                canvas.FillColor = keyColor;
                canvas.FillRectangle(keyRect);

                // Draw border - only changes with glow enabled
                if (_glowEnabled && isPlaying && !disabled)
                {
                    float glowIntensity = (float)Math.Pow(envelopeLevel, 0.7);
                    canvas.StrokeColor = EnvelopeGlowColor.WithAlpha(glowIntensity * 0.9f);
                    canvas.StrokeSize = 2 + envelopeLevel * 2;
                    canvas.DrawRectangle(keyRect);
                }
                else
                {
                    // No outline color change when glow is off (per user request)
                    canvas.StrokeColor = Color.FromArgb(AppColors.BorderMedium);
                    canvas.StrokeSize = 1;
                    canvas.DrawRectangle(keyRect);
                }

                canvas.FontSize = 10;
                canvas.FontColor = disabled ? Color.FromArgb(AppColors.TextDim) : Colors.White;
                canvas.DrawString(GetNoteName(note), keyRect, HorizontalAlignment.Center, VerticalAlignment.Bottom);

                _keyRects.Add(new KeyRect(note, keyRect, true, disabled));
            }
        }

        // Arrows
        DrawArrow(canvas, _leftArrowRect, left: true);
        DrawArrow(canvas, _rightArrowRect, left: false);
    }
    
    private static Color InterpolateColor(Color a, Color b, float t)
    {
        t = Math.Clamp(t, 0f, 1f);
        return new Color(
            a.Red + (b.Red - a.Red) * t,
            a.Green + (b.Green - a.Green) * t,
            a.Blue + (b.Blue - a.Blue) * t,
            a.Alpha + (b.Alpha - a.Alpha) * t
        );
    }

    private void DrawArrow(ICanvas canvas, RectF rect, bool left)
    {
        canvas.FillColor = Color.FromArgb(AppColors.PianoStripBackground);
        canvas.FillRoundedRectangle(rect, 6);
        canvas.StrokeColor = Colors.White.WithAlpha(0.7f);
        canvas.StrokeSize = 1;
        canvas.DrawRoundedRectangle(rect, 6);

        float cx = rect.Center.X;
        float cy = rect.Center.Y;
        float size = rect.Height * 0.3f;

        var path = new PathF();
        if (left)
        {
            path.MoveTo(cx - size, cy);
            path.LineTo(cx + size, cy - size);
            path.LineTo(cx + size, cy + size);
        }
        else
        {
            path.MoveTo(cx + size, cy);
            path.LineTo(cx - size, cy - size);
            path.LineTo(cx - size, cy + size);
        }
        path.Close();

        canvas.FillColor = Colors.White;
        canvas.FillPath(path);
    }

    public void OnTouches(IEnumerable<PointF> touches)
    {
        var touchList = touches.ToList();

        // Handle strip dragging (only first touch)
        if (_draggingStrip && touchList.Count > 0)
        {
            var p = touchList[0];
            UpdateStripDrag(p.X);
        }

        // Handle key presses
        var newNotes = new HashSet<int>();
        foreach (var touch in touchList)
        {
            int? note = HitTestKey(touch);
            if (note.HasValue)
            {
                newNotes.Add(note.Value);
            }
        }

        // Note on for newly pressed
        foreach (var n in newNotes)
        {
            if (!_activeNotes.Contains(n) && n >= _instrumentMin && n <= _instrumentMax)
            {
                _activeNotes.Add(n);
                NoteOn?.Invoke(this, n);
            }
        }

        // Note off for released
        var toRelease = _activeNotes.Where(n => !newNotes.Contains(n)).ToList();
        foreach (var n in toRelease)
        {
            _activeNotes.Remove(n);
            NoteOff?.Invoke(this, n);
        }
    }

    public void OnTouchStart(float x, float y)
    {
        var p = new PointF(x, y);
        if (_stripHighlight.Contains(p))
        {
            _draggingStrip = true;
            _dragOffset = x - _stripHighlight.Left;
        }
    }

    public void OnTouchEnd(float x, float y)
    {
        var p = new PointF(x, y);
        if (_leftArrowRect.Contains(p))
        {
            ShiftRequested?.Invoke(this, -12);
        }
        else if (_rightArrowRect.Contains(p))
        {
            ShiftRequested?.Invoke(this, +12);
        }
        _draggingStrip = false;

        // Release all active notes
        foreach (var n in _activeNotes.ToList())
        {
            _activeNotes.Remove(n);
            NoteOff?.Invoke(this, n);
        }
    }

    private void UpdateStripDrag(float x)
    {
        float keyWidth = _stripRect.Width / (GlobalMax - GlobalMin + 1);
        float highlightWidth = _stripHighlight.Width;
        float left = x - _dragOffset;
        left = Math.Clamp(left, _stripRect.Left, _stripRect.Right - highlightWidth);
        int newStart = (int)Math.Round((left - _stripRect.Left) / keyWidth) + GlobalMin;
        StripDragRequested?.Invoke(this, newStart);
    }

    private int? HitTestKey(PointF p)
    {
        // Prefer black keys on hit-test (drawn later)
        foreach (var key in _keyRects.Where(k => k.IsBlack))
        {
            if (key.Rect.Contains(p))
                return key.Note;
        }
        foreach (var key in _keyRects.Where(k => !k.IsBlack))
        {
            if (key.Rect.Contains(p))
                return key.Note;
        }
        return null;
    }

    private static bool IsBlack(int midiNote)
    {
        int n = midiNote % 12;
        return n is 1 or 3 or 6 or 8 or 10;
    }

    private static string GetNoteName(int midiNote)
    {
        string[] names = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
        int note = midiNote % 12;
        int octave = (midiNote / 12) - 1;
        return $"{names[note]}{octave}";
    }

    private record KeyRect(int Note, RectF Rect, bool IsBlack, bool Disabled);
}

