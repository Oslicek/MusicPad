using Microsoft.Maui.Graphics;
using MusicPad.Core.Theme;

namespace MusicPad.Controls;

/// <summary>
/// Drawable for a continuous pitch-volume surface.
/// X axis = pitch (instrument's full chromatic range)
/// Y axis = volume (0-1, bottom = quiet, top = loud)
/// Touch points glow with aggressive accent color.
/// </summary>
public class PitchVolumeDrawable : IDrawable
{
    private static readonly string[] NoteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
    
    // Colors
    private static readonly Color SurfaceColor = Color.FromArgb(AppColors.Navy);
    private static readonly Color GridLineColor = Color.FromArgb(AppColors.SurfaceBorder);
    private static readonly Color OctaveLineColor = Color.FromArgb(AppColors.Teal);
    private static readonly Color TouchGlowColor = Color.FromArgb(AppColors.Accent); // Orange - most aggressive
    private static readonly Color TouchCoreColor = Color.FromArgb(AppColors.Amber);
    private static readonly Color LabelColor = Color.FromArgb(AppColors.TextSecondary);
    private static readonly Color GradientTopColor = Color.FromArgb(AppColors.SkyBlue).WithAlpha(0.15f);
    private static readonly Color GradientBottomColor = Color.FromArgb(AppColors.Navy);
    
    private int _minNote = 21;  // A0
    private int _maxNote = 108; // C8
    private float _surfaceWidth;
    private float _surfaceHeight;
    private float _offsetX;
    private float _offsetY;
    
    // Active touches: stores (midiNote, volume, x, y) for glow rendering
    private readonly List<TouchPoint> _activeTouches = new();
    private readonly object _touchLock = new();
    
    public event EventHandler<PitchVolumeEventArgs>? NoteOn;
    public event EventHandler<int>? NoteOff;
    public event EventHandler<PitchVolumeEventArgs>? VolumeChanged;
    
    private class TouchPoint
    {
        public long PointerId { get; set; }
        public int MidiNote { get; set; }
        public float Volume { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
    }
    
    /// <summary>
    /// Sets the instrument's note range.
    /// </summary>
    public void SetNoteRange(int minNote, int maxNote)
    {
        _minNote = minNote;
        _maxNote = maxNote;
    }
    
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        float margin = 40; // Space for labels
        _offsetX = margin;
        _offsetY = 20;
        _surfaceWidth = dirtyRect.Width - margin - 10;
        _surfaceHeight = dirtyRect.Height - 40;
        
        // Draw background gradient (darker at bottom, lighter at top for volume indication)
        DrawBackground(canvas);
        
        // Draw grid lines for reference
        DrawGrid(canvas);
        
        // Draw axis labels
        DrawLabels(canvas, dirtyRect);
        
        // Draw active touch points with glow
        DrawTouchPoints(canvas);
    }
    
    private void DrawBackground(ICanvas canvas)
    {
        // Fill with gradient effect (simulated with multiple rects)
        int steps = 20;
        float stepHeight = _surfaceHeight / steps;
        
        for (int i = 0; i < steps; i++)
        {
            float t = (float)i / steps;
            // Interpolate from bottom (dark) to top (lighter)
            Color color = InterpolateColor(GradientBottomColor, GradientTopColor, 1 - t);
            canvas.FillColor = color;
            canvas.FillRectangle(_offsetX, _offsetY + i * stepHeight, _surfaceWidth, stepHeight + 1);
        }
        
        // Border
        canvas.StrokeColor = GridLineColor;
        canvas.StrokeSize = 2;
        canvas.DrawRectangle(_offsetX, _offsetY, _surfaceWidth, _surfaceHeight);
    }
    
    private void DrawGrid(ICanvas canvas)
    {
        int totalNotes = _maxNote - _minNote + 1;
        float noteWidth = _surfaceWidth / totalNotes;
        
        // Draw octave lines (vertical)
        canvas.StrokeSize = 1;
        for (int note = _minNote; note <= _maxNote; note++)
        {
            if (note % 12 == 0) // C notes = octave boundaries
            {
                float x = _offsetX + (note - _minNote) * noteWidth;
                canvas.StrokeColor = OctaveLineColor;
                canvas.DrawLine(x, _offsetY, x, _offsetY + _surfaceHeight);
            }
        }
        
        // Draw horizontal volume lines (25%, 50%, 75%)
        canvas.StrokeColor = GridLineColor;
        canvas.StrokeDashPattern = new float[] { 4, 4 };
        for (int i = 1; i <= 3; i++)
        {
            float y = _offsetY + _surfaceHeight * (1 - i * 0.25f);
            canvas.DrawLine(_offsetX, y, _offsetX + _surfaceWidth, y);
        }
        canvas.StrokeDashPattern = null;
    }
    
    private void DrawLabels(ICanvas canvas, RectF dirtyRect)
    {
        canvas.FontSize = 10;
        canvas.FontColor = LabelColor;
        
        int totalNotes = _maxNote - _minNote + 1;
        float noteWidth = _surfaceWidth / totalNotes;
        
        // Draw note labels at octave boundaries (C notes)
        for (int note = _minNote; note <= _maxNote; note++)
        {
            if (note % 12 == 0) // C notes
            {
                int octave = (note / 12) - 1;
                string label = $"C{octave}";
                float x = _offsetX + (note - _minNote) * noteWidth;
                canvas.DrawString(label, x - 10, _offsetY + _surfaceHeight + 5, 20, 15, 
                    HorizontalAlignment.Center, VerticalAlignment.Top);
            }
        }
        
        // Draw volume labels on left side
        canvas.DrawString("Loud", 0, _offsetY, 35, 20, HorizontalAlignment.Right, VerticalAlignment.Top);
        canvas.DrawString("Quiet", 0, _offsetY + _surfaceHeight - 20, 35, 20, HorizontalAlignment.Right, VerticalAlignment.Bottom);
    }
    
    private void DrawTouchPoints(ICanvas canvas)
    {
        lock (_touchLock)
        {
            foreach (var touch in _activeTouches)
            {
                DrawGlowingTouchPoint(canvas, touch.X, touch.Y);
            }
        }
    }
    
    private void DrawGlowingTouchPoint(ICanvas canvas, float x, float y)
    {
        // Outer glow layers (largest to smallest for radial glow effect)
        float[] glowRadii = { 60, 45, 30, 20, 12 };
        float[] glowAlphas = { 0.1f, 0.2f, 0.35f, 0.6f, 1.0f };
        
        for (int i = 0; i < glowRadii.Length; i++)
        {
            Color glowColor = (i < glowRadii.Length - 1) 
                ? TouchGlowColor.WithAlpha(glowAlphas[i])
                : TouchCoreColor;
            canvas.FillColor = glowColor;
            canvas.FillCircle(x, y, glowRadii[i]);
        }
        
        // Bright white center
        canvas.FillColor = Colors.White;
        canvas.FillCircle(x, y, 6);
    }
    
    private static Color InterpolateColor(Color a, Color b, float t)
    {
        return new Color(
            a.Red + (b.Red - a.Red) * t,
            a.Green + (b.Green - a.Green) * t,
            a.Blue + (b.Blue - a.Blue) * t,
            a.Alpha + (b.Alpha - a.Alpha) * t
        );
    }
    
    /// <summary>
    /// Handle touch start - returns (midiNote, volume) for the touch position.
    /// </summary>
    public void OnTouchStart(long pointerId, float x, float y)
    {
        var result = GetNoteAndVolumeAtPosition(x, y);
        if (result == null) return;
        
        lock (_touchLock)
        {
            // Remove any existing touch with same pointer
            _activeTouches.RemoveAll(t => t.PointerId == pointerId);
            
            _activeTouches.Add(new TouchPoint
            {
                PointerId = pointerId,
                MidiNote = result.Value.note,
                Volume = result.Value.volume,
                X = x,
                Y = y
            });
        }
        
        NoteOn?.Invoke(this, new PitchVolumeEventArgs(result.Value.note, result.Value.volume));
    }
    
    /// <summary>
    /// Handle touch move - updates volume and potentially triggers note changes.
    /// </summary>
    public void OnTouchMove(long pointerId, float x, float y)
    {
        var result = GetNoteAndVolumeAtPosition(x, y);
        if (result == null) return;
        
        TouchPoint? existingTouch;
        lock (_touchLock)
        {
            existingTouch = _activeTouches.FirstOrDefault(t => t.PointerId == pointerId);
        }
        
        if (existingTouch == null)
        {
            // New touch
            OnTouchStart(pointerId, x, y);
            return;
        }
        
        bool noteChanged = existingTouch.MidiNote != result.Value.note;
        
        if (noteChanged)
        {
            // Release old note, start new note
            NoteOff?.Invoke(this, existingTouch.MidiNote);
            NoteOn?.Invoke(this, new PitchVolumeEventArgs(result.Value.note, result.Value.volume));
        }
        else if (Math.Abs(existingTouch.Volume - result.Value.volume) > 0.02f)
        {
            // Volume changed significantly
            VolumeChanged?.Invoke(this, new PitchVolumeEventArgs(result.Value.note, result.Value.volume));
        }
        
        lock (_touchLock)
        {
            existingTouch.MidiNote = result.Value.note;
            existingTouch.Volume = result.Value.volume;
            existingTouch.X = x;
            existingTouch.Y = y;
        }
    }
    
    /// <summary>
    /// Handle touch end.
    /// </summary>
    public void OnTouchEnd(long pointerId)
    {
        TouchPoint? touch;
        lock (_touchLock)
        {
            touch = _activeTouches.FirstOrDefault(t => t.PointerId == pointerId);
            if (touch != null)
            {
                _activeTouches.Remove(touch);
            }
        }
        
        if (touch != null)
        {
            NoteOff?.Invoke(this, touch.MidiNote);
        }
    }
    
    /// <summary>
    /// Release all active touches.
    /// </summary>
    public void OnAllTouchesEnd()
    {
        List<TouchPoint> touches;
        lock (_touchLock)
        {
            touches = _activeTouches.ToList();
            _activeTouches.Clear();
        }
        
        foreach (var touch in touches)
        {
            NoteOff?.Invoke(this, touch.MidiNote);
        }
    }
    
    private (int note, float volume)? GetNoteAndVolumeAtPosition(float x, float y)
    {
        // Check bounds
        if (x < _offsetX || x > _offsetX + _surfaceWidth ||
            y < _offsetY || y > _offsetY + _surfaceHeight)
        {
            return null;
        }
        
        // Calculate note from X position
        float relativeX = (x - _offsetX) / _surfaceWidth;
        int totalNotes = _maxNote - _minNote + 1;
        int noteOffset = (int)(relativeX * totalNotes);
        int midiNote = Math.Clamp(_minNote + noteOffset, _minNote, _maxNote);
        
        // Calculate volume from Y position (top = 1.0, bottom = 0.0)
        float relativeY = (y - _offsetY) / _surfaceHeight;
        float volume = Math.Clamp(1.0f - relativeY, 0.0f, 1.0f);
        
        return (midiNote, volume);
    }
    
    /// <summary>
    /// Gets current touch info for a pointer.
    /// </summary>
    public (int note, float volume)? GetTouchInfo(long pointerId)
    {
        lock (_touchLock)
        {
            var touch = _activeTouches.FirstOrDefault(t => t.PointerId == pointerId);
            if (touch != null)
            {
                return (touch.MidiNote, touch.Volume);
            }
        }
        return null;
    }
}

/// <summary>
/// Event args for pitch-volume surface events.
/// </summary>
public class PitchVolumeEventArgs : EventArgs
{
    public int MidiNote { get; }
    public float Volume { get; }
    
    public PitchVolumeEventArgs(int midiNote, float volume)
    {
        MidiNote = midiNote;
        Volume = volume;
    }
}

