using Microsoft.Maui.Graphics;
using MusicPad.Core.Models;

namespace MusicPad.Controls;

/// <summary>
/// Drawable for the pad matrix that handles rendering and multi-touch detection.
/// Supports filtered notes, custom colors, and viewpage navigation.
/// </summary>
public class PadMatrixDrawable : IDrawable
{
    private static readonly string[] NoteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
    private static readonly bool[] IsSharp = { false, true, false, true, false, false, true, false, true, false, true, false };

    // Default colors (teal/cyan theme)
    private Color _padColor = Color.FromArgb("#4ECDC4");
    private Color _padPressedColor = Color.FromArgb("#7EEEE6");
    private Color _padAltColor = Color.FromArgb("#E8A838");
    private Color _padAltPressedColor = Color.FromArgb("#F5C868");
    private static readonly Color BorderColor = Color.FromArgb("#2A9D8F");
    private static readonly Color TextColor = Colors.White;
    private static readonly Color TextShadowColor = Color.FromArgb("#40000000");
    
    // Arrow colors - captivating neon
    private static readonly Color ArrowColor = Color.FromArgb("#00FF88");
    private static readonly Color ArrowGlowColor = Color.FromArgb("#00FFAA");
    private static readonly Color ArrowBgColor = Color.FromArgb("#20FFFFFF");

    private List<int> _notes = new();
    private int _columns;
    private int _rows;
    private float _padSize;
    private float _spacing = 4;
    private float _offsetX;
    private float _offsetY;
    private const float ArrowSize = 24;
    
    // Navigation state
    private bool _hasUpArrow;
    private bool _hasDownArrow;
    private RectF _upArrowRect;
    private RectF _downArrowRect;
    
    private readonly HashSet<int> _activeNotes = new();
    private Func<int, bool>? _isHalftone;
    private int _lastTouchCount;

    public event EventHandler<int>? NoteOn;
    public event EventHandler<int>? NoteOff;
    public event EventHandler? NavigateUp;
    public event EventHandler? NavigateDown;

    /// <summary>
    /// Sets the notes to display and grid configuration.
    /// </summary>
    public void SetNotes(List<int> notes, int? columns = null, bool hasUpArrow = false, bool hasDownArrow = false)
    {
        _notes = notes;
        _hasUpArrow = hasUpArrow;
        _hasDownArrow = hasDownArrow;
        
        if (notes.Count == 0)
        {
            _columns = 0;
            _rows = 0;
            return;
        }
        
        if (columns.HasValue && columns.Value > 0)
        {
            _columns = columns.Value;
            _rows = (int)Math.Ceiling((double)notes.Count / _columns);
        }
        else
        {
            // Auto-calculate for square-ish grid
            _columns = (int)Math.Ceiling(Math.Sqrt(notes.Count));
            _rows = (int)Math.Ceiling((double)notes.Count / _columns);
        }
    }

    /// <summary>
    /// Sets custom colors for the pads.
    /// </summary>
    public void SetColors(string? padColor, string? padPressedColor, string? padAltColor, string? padAltPressedColor)
    {
        if (!string.IsNullOrEmpty(padColor))
            _padColor = Color.FromArgb(padColor);
        if (!string.IsNullOrEmpty(padPressedColor))
            _padPressedColor = Color.FromArgb(padPressedColor);
        if (!string.IsNullOrEmpty(padAltColor))
            _padAltColor = Color.FromArgb(padAltColor);
        if (!string.IsNullOrEmpty(padAltPressedColor))
            _padAltPressedColor = Color.FromArgb(padAltPressedColor);
    }

    /// <summary>
    /// Sets a custom halftone detector (e.g., scale-based sharps/flats).
    /// </summary>
    public void SetHalftoneDetector(Func<int, bool>? detector)
    {
        _isHalftone = detector;
    }

    /// <summary>
    /// Legacy method for backward compatibility.
    /// </summary>
    public void SetKeyRange(int minKey, int maxKey)
    {
        var notes = new List<int>();
        for (int note = minKey; note <= maxKey; note++)
        {
            notes.Add(note);
        }
        SetNotes(notes);
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (_notes.Count == 0 || _columns == 0)
            return;

        float width = dirtyRect.Width;
        float height = dirtyRect.Height;

        // Calculate pad size for squares (no reserved arrow space - arrows are inline)
        float padSizeByWidth = (width - (_columns - 1) * _spacing) / _columns;
        float padSizeByHeight = (height - (_rows - 1) * _spacing) / _rows;
        _padSize = Math.Min(padSizeByWidth, padSizeByHeight);

        // Center the grid
        float gridWidth = _padSize * _columns + (_columns - 1) * _spacing;
        float gridHeight = _padSize * _rows + (_rows - 1) * _spacing;
        _offsetX = (width - gridWidth) / 2;
        _offsetY = (height - gridHeight) / 2;

        // Draw pads (bottom-left = low notes, top-right = high notes)
        for (int i = 0; i < _notes.Count; i++)
        {
            int row = i / _columns;
            int col = i % _columns;
            int visualRow = _rows - 1 - row; // Invert rows so low notes are at bottom
            
            DrawPad(canvas, visualRow, col, _notes[i]);
        }

        // Draw navigation arrows next to the pad grid
        if (_hasUpArrow)
        {
            DrawUpArrow(canvas, gridWidth, gridHeight);
        }
        if (_hasDownArrow)
        {
            DrawDownArrow(canvas, gridWidth, gridHeight);
        }
    }

    private void DrawUpArrow(ICanvas canvas, float gridWidth, float gridHeight)
    {
        // Position arrow just above the top row, centered
        float arrowX = _offsetX + gridWidth / 2 - ArrowSize;
        float arrowY = _offsetY - ArrowSize - 8;
        float arrowWidth = ArrowSize * 2;
        float arrowHeight = ArrowSize;
        
        _upArrowRect = new RectF(arrowX, Math.Max(0, arrowY), arrowWidth, arrowHeight);
        
        // Draw glowing background pill
        canvas.FillColor = ArrowBgColor;
        canvas.FillRoundedRectangle(_upArrowRect, arrowHeight / 2);
        
        // Draw arrow triangle pointing up
        float centerX = arrowX + arrowWidth / 2;
        float centerY = arrowY + arrowHeight / 2;
        float triSize = ArrowSize * 0.4f;
        
        var path = new PathF();
        path.MoveTo(centerX, centerY - triSize);
        path.LineTo(centerX + triSize, centerY + triSize * 0.5f);
        path.LineTo(centerX - triSize, centerY + triSize * 0.5f);
        path.Close();
        
        // Glow effect
        canvas.FillColor = ArrowGlowColor;
        canvas.FillPath(path);
        
        // Border glow
        canvas.StrokeColor = ArrowColor;
        canvas.StrokeSize = 2;
        canvas.DrawRoundedRectangle(_upArrowRect, arrowHeight / 2);
    }

    private void DrawDownArrow(ICanvas canvas, float gridWidth, float gridHeight)
    {
        // Position arrow just below the bottom row, centered
        float arrowX = _offsetX + gridWidth / 2 - ArrowSize;
        float arrowY = _offsetY + gridHeight + 8;
        float arrowWidth = ArrowSize * 2;
        float arrowHeight = ArrowSize;
        
        _downArrowRect = new RectF(arrowX, arrowY, arrowWidth, arrowHeight);
        
        // Draw glowing background pill
        canvas.FillColor = ArrowBgColor;
        canvas.FillRoundedRectangle(_downArrowRect, arrowHeight / 2);
        
        // Draw arrow triangle pointing down
        float centerX = arrowX + arrowWidth / 2;
        float centerY = arrowY + arrowHeight / 2;
        float triSize = ArrowSize * 0.4f;
        
        var path = new PathF();
        path.MoveTo(centerX, centerY + triSize);
        path.LineTo(centerX + triSize, centerY - triSize * 0.5f);
        path.LineTo(centerX - triSize, centerY - triSize * 0.5f);
        path.Close();
        
        // Glow effect
        canvas.FillColor = ArrowGlowColor;
        canvas.FillPath(path);
        
        // Border glow
        canvas.StrokeColor = ArrowColor;
        canvas.StrokeSize = 2;
        canvas.DrawRoundedRectangle(_downArrowRect, arrowHeight / 2);
    }

    private void DrawPad(ICanvas canvas, int visualRow, int col, int midiNote)
    {
        float x = _offsetX + col * (_padSize + _spacing);
        float y = _offsetY + visualRow * (_padSize + _spacing);

        bool isAltNote = _isHalftone?.Invoke(midiNote) ?? IsSharp[midiNote % 12];
        bool isPressed = _activeNotes.Contains(midiNote);

        Color bgColor = isAltNote
            ? (isPressed ? _padAltPressedColor : _padAltColor)
            : (isPressed ? _padPressedColor : _padColor);

        // Draw background with glow effect for neon look
        canvas.FillColor = bgColor;
        canvas.FillRoundedRectangle(x, y, _padSize, _padSize, 8);

        // Draw brighter border for neon effect
        canvas.StrokeColor = isPressed ? Colors.White : bgColor.WithAlpha(0.7f);
        canvas.StrokeSize = isPressed ? 3 : 2;
        canvas.DrawRoundedRectangle(x, y, _padSize, _padSize, 8);

        // Draw note name
        string noteName = GetNoteName(midiNote);
        float fontSize = Math.Max(10, _padSize / 4);
        
        canvas.FontSize = fontSize;
        canvas.FontColor = TextShadowColor;
        canvas.DrawString(noteName, x + 1, y + 1, _padSize, _padSize, 
            HorizontalAlignment.Center, VerticalAlignment.Center);
        
        canvas.FontColor = TextColor;
        canvas.DrawString(noteName, x, y, _padSize, _padSize, 
            HorizontalAlignment.Center, VerticalAlignment.Center);
    }

    public void OnTouches(IEnumerable<PointF> touches)
    {
        var touchList = touches.ToList();
        var currentTouchCount = touchList.Count;
        
        // Check for arrow taps first
        foreach (var touch in touchList)
        {
            if (_hasUpArrow && _upArrowRect.Contains(new PointF(touch.X, touch.Y)))
            {
                // Don't process as pad touch, wait for release to trigger navigation
            }
            else if (_hasDownArrow && _downArrowRect.Contains(new PointF(touch.X, touch.Y)))
            {
                // Don't process as pad touch
            }
        }
        
        // Handle note touches
        if (currentTouchCount < _lastTouchCount)
        {
            var notesToKeep = new HashSet<int>();
            foreach (var touch in touchList)
            {
                int? note = GetNoteAtPosition(touch.X, touch.Y);
                if (note.HasValue && _activeNotes.Contains(note.Value))
                {
                    notesToKeep.Add(note.Value);
                }
            }
            
            var toRelease = _activeNotes.Except(notesToKeep).ToList();
            foreach (var note in toRelease)
            {
                _activeNotes.Remove(note);
                NoteOff?.Invoke(this, note);
            }
        }
        
        foreach (var touch in touchList)
        {
            // Skip arrow areas
            if (_hasUpArrow && _upArrowRect.Contains(new PointF(touch.X, touch.Y)))
                continue;
            if (_hasDownArrow && _downArrowRect.Contains(new PointF(touch.X, touch.Y)))
                continue;
                
            int? note = GetNoteAtPosition(touch.X, touch.Y);
            if (note.HasValue && !_activeNotes.Contains(note.Value))
            {
                _activeNotes.Add(note.Value);
                NoteOn?.Invoke(this, note.Value);
            }
        }
        
        _lastTouchCount = currentTouchCount;
    }

    public void OnAllTouchesEnd()
    {
        foreach (var note in _activeNotes.ToList())
        {
            _activeNotes.Remove(note);
            NoteOff?.Invoke(this, note);
        }
        _lastTouchCount = 0;
    }
    
    /// <summary>
    /// Call this when a tap ends to check for arrow navigation.
    /// </summary>
    public void OnTapEnd(float x, float y)
    {
        if (_hasUpArrow && _upArrowRect.Contains(new PointF(x, y)))
        {
            NavigateUp?.Invoke(this, EventArgs.Empty);
        }
        else if (_hasDownArrow && _downArrowRect.Contains(new PointF(x, y)))
        {
            NavigateDown?.Invoke(this, EventArgs.Empty);
        }
    }

    private int? GetNoteAtPosition(float x, float y)
    {
        if (_padSize <= 0 || _notes.Count == 0)
            return null;

        // Adjust for offset
        x -= _offsetX;
        y -= _offsetY;

        if (x < 0 || y < 0)
            return null;

        int col = (int)(x / (_padSize + _spacing));
        int visualRow = (int)(y / (_padSize + _spacing));

        if (col >= _columns || visualRow >= _rows)
            return null;

        // Check if actually inside the pad (not in spacing)
        float padX = col * (_padSize + _spacing);
        float padY = visualRow * (_padSize + _spacing);
        if (x > padX + _padSize || y > padY + _padSize)
            return null;

        // Convert visual row/col to note index (visual row 0 = top = highest notes in view)
        int row = _rows - 1 - visualRow;
        int noteIndex = row * _columns + col;

        if (noteIndex >= _notes.Count)
            return null;

        return _notes[noteIndex];
    }

    private static string GetNoteName(int midiNote)
    {
        int noteIndex = midiNote % 12;
        int octave = (midiNote / 12) - 1;
        return $"{NoteNames[noteIndex]}{octave}";
    }
}
