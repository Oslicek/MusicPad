using Microsoft.Maui.Graphics;

namespace MusicPad.Controls;

/// <summary>
/// Drawable for a piano-style keyboard with a mini 88-key strip and draggable range.
/// </summary>
public class PianoKeyboardDrawable : IDrawable
{
    private const int GlobalMin = 21;  // A0
    private const int GlobalMax = 108; // C8

    private int _rangeStart;
    private int _rangeEnd;
    private int _instrumentMin;
    private int _instrumentMax;
    private bool _isLandscape;

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
        canvas.FillColor = Color.FromArgb("#2044AAFF");
        canvas.FillRectangle(instRect);

        // Draw current window highlight
        canvas.FillColor = Color.FromArgb("#404040");
        canvas.FillRectangle(_stripHighlight);

        // Draw black keys on top
        for (int i = 0; i < totalKeys; i++)
        {
            int midi = GlobalMin + i;
            if (!IsBlack(midi)) continue;
            float x = _stripRect.X + i * keyWidth;
            var keyRect = new RectF(x, _stripRect.Y, keyWidth, _stripRect.Height * 0.6f);
            canvas.FillColor = Color.FromArgb("#111111");
            canvas.FillRectangle(keyRect);
        }

        // Outlines
        canvas.StrokeColor = Color.FromArgb("#555555");
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
            bool pressed = _activeNotes.Contains(note);

            canvas.FillColor = disabled ? Color.FromArgb("#333333")
                                        : pressed ? Color.FromArgb("#FFDD88") : Colors.White;
            canvas.FillRectangle(keyRect);

            canvas.StrokeColor = Color.FromArgb("#444444");
            canvas.StrokeSize = 1;
            canvas.DrawRectangle(keyRect);

            // Label
            canvas.FontSize = 12;
            canvas.FontColor = disabled ? Color.FromArgb("#777777") : Color.FromArgb("#222222");
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
                bool pressed = _activeNotes.Contains(note);

                canvas.FillColor = disabled ? Color.FromArgb("#222222")
                                            : pressed ? Color.FromArgb("#FFAA55") : Color.FromArgb("#111111");
                canvas.FillRectangle(keyRect);

                canvas.StrokeColor = Color.FromArgb("#333333");
                canvas.StrokeSize = 1;
                canvas.DrawRectangle(keyRect);

                canvas.FontSize = 10;
                canvas.FontColor = disabled ? Color.FromArgb("#666666") : Colors.White;
                canvas.DrawString(GetNoteName(note), keyRect, HorizontalAlignment.Center, VerticalAlignment.Bottom);

                _keyRects.Add(new KeyRect(note, keyRect, true, disabled));
            }
        }

        // Arrows
        DrawArrow(canvas, _leftArrowRect, left: true);
        DrawArrow(canvas, _rightArrowRect, left: false);
    }

    private void DrawArrow(ICanvas canvas, RectF rect, bool left)
    {
        canvas.FillColor = Color.FromArgb("#303030");
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

