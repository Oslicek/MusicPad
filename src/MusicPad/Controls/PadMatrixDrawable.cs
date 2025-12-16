using Microsoft.Maui.Graphics;

namespace MusicPad.Controls;

/// <summary>
/// Drawable for the pad matrix that handles rendering and touch detection.
/// </summary>
public class PadMatrixDrawable : IDrawable
{
    private static readonly string[] NoteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
    private static readonly bool[] IsSharp = { false, true, false, true, false, false, true, false, true, false, true, false };

    // Colors
    private static readonly Color NaturalColor = Color.FromArgb("#4ECDC4");
    private static readonly Color NaturalPressedColor = Color.FromArgb("#7EEEE6");
    private static readonly Color SharpColor = Color.FromArgb("#E8A838");
    private static readonly Color SharpPressedColor = Color.FromArgb("#F5C868");
    private static readonly Color BorderColor = Color.FromArgb("#2A9D8F");
    private static readonly Color TextColor = Colors.White;
    private static readonly Color TextShadowColor = Color.FromArgb("#40000000");

    private int _minKey;
    private int _maxKey;
    private int _rows;
    private int _columns;
    private float _padSize;
    private float _spacing = 3;
    private float _offsetX;
    private float _offsetY;
    
    private readonly HashSet<int> _pressedNotes = new();

    public event EventHandler<int>? NoteOn;
    public event EventHandler<int>? NoteOff;

    public void SetKeyRange(int minKey, int maxKey)
    {
        _minKey = minKey;
        _maxKey = maxKey;
        
        int noteCount = maxKey - minKey + 1;
        _columns = (int)Math.Ceiling(Math.Sqrt(noteCount));
        _rows = (int)Math.Ceiling((double)noteCount / _columns);
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (_maxKey <= _minKey)
            return;

        float width = dirtyRect.Width;
        float height = dirtyRect.Height;

        // Calculate pad size for squares
        float padSizeByWidth = (width - (_columns - 1) * _spacing) / _columns;
        float padSizeByHeight = (height - (_rows - 1) * _spacing) / _rows;
        _padSize = Math.Min(padSizeByWidth, padSizeByHeight);

        // Center the grid
        float gridWidth = _padSize * _columns + (_columns - 1) * _spacing;
        float gridHeight = _padSize * _rows + (_rows - 1) * _spacing;
        _offsetX = (width - gridWidth) / 2;
        _offsetY = (height - gridHeight) / 2;

        // Draw pads (bottom-left = low notes, top-right = high notes)
        int noteIndex = 0;
        int noteCount = _maxKey - _minKey + 1;

        for (int row = _rows - 1; row >= 0 && noteIndex < noteCount; row--)
        {
            for (int col = 0; col < _columns && noteIndex < noteCount; col++)
            {
                int midiNote = _minKey + noteIndex;
                DrawPad(canvas, row, col, midiNote);
                noteIndex++;
            }
        }
    }

    private void DrawPad(ICanvas canvas, int row, int col, int midiNote)
    {
        float x = _offsetX + col * (_padSize + _spacing);
        float y = _offsetY + row * (_padSize + _spacing);

        bool isSharpNote = IsSharp[midiNote % 12];
        bool isPressed = _pressedNotes.Contains(midiNote);

        Color bgColor = isSharpNote
            ? (isPressed ? SharpPressedColor : SharpColor)
            : (isPressed ? NaturalPressedColor : NaturalColor);

        // Draw background
        canvas.FillColor = bgColor;
        canvas.FillRoundedRectangle(x, y, _padSize, _padSize, 8);

        // Draw border
        canvas.StrokeColor = BorderColor;
        canvas.StrokeSize = 2;
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

    public void OnTouchStart(float x, float y)
    {
        int? note = GetNoteAtPosition(x, y);
        if (note.HasValue && !_pressedNotes.Contains(note.Value))
        {
            _pressedNotes.Add(note.Value);
            NoteOn?.Invoke(this, note.Value);
        }
    }

    public void OnTouchMove(float x, float y)
    {
        // For now, just handle the current touch position
        // Could be extended for glissando later
    }

    public void OnTouchEnd(float x, float y)
    {
        int? note = GetNoteAtPosition(x, y);
        if (note.HasValue && _pressedNotes.Contains(note.Value))
        {
            _pressedNotes.Remove(note.Value);
            NoteOff?.Invoke(this, note.Value);
        }
    }

    public void OnAllTouchesEnd()
    {
        foreach (var note in _pressedNotes.ToList())
        {
            _pressedNotes.Remove(note);
            NoteOff?.Invoke(this, note);
        }
    }

    private int? GetNoteAtPosition(float x, float y)
    {
        if (_padSize <= 0)
            return null;

        // Adjust for offset
        x -= _offsetX;
        y -= _offsetY;

        if (x < 0 || y < 0)
            return null;

        int col = (int)(x / (_padSize + _spacing));
        int row = (int)(y / (_padSize + _spacing));

        if (col >= _columns || row >= _rows)
            return null;

        // Check if actually inside the pad (not in spacing)
        float padX = col * (_padSize + _spacing);
        float padY = row * (_padSize + _spacing);
        if (x > padX + _padSize || y > padY + _padSize)
            return null;

        // Convert row/col to note index (bottom-left = low notes)
        int invertedRow = _rows - 1 - row;
        int noteIndex = invertedRow * _columns + col;
        int noteCount = _maxKey - _minKey + 1;

        if (noteIndex >= noteCount)
            return null;

        return _minKey + noteIndex;
    }

    private static string GetNoteName(int midiNote)
    {
        int noteIndex = midiNote % 12;
        int octave = (midiNote / 12) - 1;
        return $"{NoteNames[noteIndex]}{octave}";
    }
}

