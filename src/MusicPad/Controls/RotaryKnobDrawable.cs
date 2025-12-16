using Microsoft.Maui.Graphics;

namespace MusicPad.Controls;

/// <summary>
/// A rotating knob control for volume and other continuous parameters.
/// </summary>
public class RotaryKnobDrawable : IDrawable
{
    // Neon color scheme
    private static readonly Color KnobOuterColor = Color.FromArgb("#2A2A4E");
    private static readonly Color KnobInnerColor = Color.FromArgb("#1A1A2E");
    private static readonly Color KnobHighlightColor = Color.FromArgb("#00FF88");
    private static readonly Color KnobIndicatorColor = Color.FromArgb("#FF00FF");
    private static readonly Color KnobTrackColor = Color.FromArgb("#404060");
    private static readonly Color TextColor = Colors.White;

    private float _value = 0.75f; // 0-1 range, default 75%
    private float _minAngle = 225f; // Start angle (degrees, 0 = right, counter-clockwise)
    private float _maxAngle = -45f; // End angle
    private string _label = "VOL";
    
    private float _knobCenterX;
    private float _knobCenterY;
    private float _knobRadius;
    private bool _isDragging;
    private float _lastAngle;

    public event EventHandler<float>? ValueChanged;

    /// <summary>
    /// Gets or sets the current value (0-1 range).
    /// </summary>
    public float Value
    {
        get => _value;
        set
        {
            _value = Math.Clamp(value, 0f, 1f);
            ValueChanged?.Invoke(this, _value);
        }
    }

    /// <summary>
    /// Gets or sets the label displayed below the knob.
    /// </summary>
    public string Label
    {
        get => _label;
        set => _label = value;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        float size = Math.Min(dirtyRect.Width, dirtyRect.Height);
        _knobRadius = size * 0.35f;
        _knobCenterX = dirtyRect.Width / 2;
        _knobCenterY = dirtyRect.Height / 2 - 8; // Offset up to make room for label

        // Draw outer ring (track)
        DrawTrack(canvas);
        
        // Draw value arc
        DrawValueArc(canvas);
        
        // Draw knob body
        DrawKnobBody(canvas);
        
        // Draw indicator line
        DrawIndicator(canvas);
        
        // Draw label
        DrawLabel(canvas, dirtyRect);
        
        // Draw value percentage
        DrawValueText(canvas);
    }

    private void DrawTrack(ICanvas canvas)
    {
        canvas.StrokeColor = KnobTrackColor;
        canvas.StrokeSize = 6;
        canvas.StrokeLineCap = LineCap.Round;
        
        // Draw arc from min to max angle
        float sweepAngle = _maxAngle - _minAngle;
        if (sweepAngle > 0) sweepAngle -= 360;
        
        var rect = new RectF(
            _knobCenterX - _knobRadius - 8,
            _knobCenterY - _knobRadius - 8,
            (_knobRadius + 8) * 2,
            (_knobRadius + 8) * 2);
        
        canvas.DrawArc(rect, _minAngle, sweepAngle, false, false);
    }

    private void DrawValueArc(ICanvas canvas)
    {
        canvas.StrokeColor = KnobHighlightColor;
        canvas.StrokeSize = 6;
        canvas.StrokeLineCap = LineCap.Round;
        
        float totalSweep = _maxAngle - _minAngle;
        if (totalSweep > 0) totalSweep -= 360;
        float valueSweep = totalSweep * _value;
        
        var rect = new RectF(
            _knobCenterX - _knobRadius - 8,
            _knobCenterY - _knobRadius - 8,
            (_knobRadius + 8) * 2,
            (_knobRadius + 8) * 2);
        
        canvas.DrawArc(rect, _minAngle, valueSweep, false, false);
    }

    private void DrawKnobBody(ICanvas canvas)
    {
        // Outer circle (3D effect)
        canvas.FillColor = KnobOuterColor;
        canvas.FillCircle(_knobCenterX, _knobCenterY, _knobRadius);
        
        // Inner circle (slightly smaller, darker)
        canvas.FillColor = KnobInnerColor;
        canvas.FillCircle(_knobCenterX, _knobCenterY, _knobRadius * 0.85f);
        
        // Highlight rim
        canvas.StrokeColor = KnobHighlightColor.WithAlpha(0.3f);
        canvas.StrokeSize = 2;
        canvas.DrawCircle(_knobCenterX, _knobCenterY, _knobRadius);
    }

    private void DrawIndicator(ICanvas canvas)
    {
        // Calculate indicator angle based on value
        float totalAngle = _maxAngle - _minAngle;
        if (totalAngle > 0) totalAngle -= 360;
        float currentAngle = _minAngle + totalAngle * _value;
        float radians = currentAngle * MathF.PI / 180f;
        
        // Draw line from center towards edge
        float innerRadius = _knobRadius * 0.3f;
        float outerRadius = _knobRadius * 0.75f;
        
        float x1 = _knobCenterX + innerRadius * MathF.Cos(radians);
        float y1 = _knobCenterY - innerRadius * MathF.Sin(radians);
        float x2 = _knobCenterX + outerRadius * MathF.Cos(radians);
        float y2 = _knobCenterY - outerRadius * MathF.Sin(radians);
        
        canvas.StrokeColor = KnobIndicatorColor;
        canvas.StrokeSize = 4;
        canvas.StrokeLineCap = LineCap.Round;
        canvas.DrawLine(x1, y1, x2, y2);
        
        // Dot at the end
        canvas.FillColor = KnobIndicatorColor;
        canvas.FillCircle(x2, y2, 4);
    }

    private void DrawLabel(ICanvas canvas, RectF dirtyRect)
    {
        canvas.FontSize = 10;
        canvas.FontColor = TextColor.WithAlpha(0.7f);
        canvas.DrawString(_label, 0, dirtyRect.Height - 16, dirtyRect.Width, 16, 
            HorizontalAlignment.Center, VerticalAlignment.Center);
    }

    private void DrawValueText(ICanvas canvas)
    {
        int percent = (int)(_value * 100);
        canvas.FontSize = 12;
        canvas.FontColor = KnobHighlightColor;
        canvas.DrawString($"{percent}%", 
            _knobCenterX - 20, _knobCenterY - 6, 40, 12, 
            HorizontalAlignment.Center, VerticalAlignment.Center);
    }

    /// <summary>
    /// Handle touch/drag to rotate the knob.
    /// </summary>
    public void OnTouch(float x, float y, bool isStart)
    {
        if (isStart)
        {
            // Check if touch is on the knob
            float dx = x - _knobCenterX;
            float dy = y - _knobCenterY;
            float distance = MathF.Sqrt(dx * dx + dy * dy);
            
            if (distance <= _knobRadius + 20) // Some tolerance
            {
                _isDragging = true;
                _lastAngle = GetAngleFromPoint(x, y);
            }
        }
        else if (_isDragging)
        {
            float currentAngle = GetAngleFromPoint(x, y);
            float angleDelta = currentAngle - _lastAngle;
            
            // Handle wrap-around
            if (angleDelta > 180) angleDelta -= 360;
            if (angleDelta < -180) angleDelta += 360;
            
            // Convert angle delta to value delta
            float totalAngle = _maxAngle - _minAngle;
            if (totalAngle > 0) totalAngle -= 360;
            
            float valueDelta = angleDelta / totalAngle;
            Value = Math.Clamp(_value + valueDelta, 0f, 1f);
            
            _lastAngle = currentAngle;
        }
    }

    public void OnTouchEnd()
    {
        _isDragging = false;
    }

    private float GetAngleFromPoint(float x, float y)
    {
        float dx = x - _knobCenterX;
        float dy = _knobCenterY - y; // Invert Y for standard math coordinates
        return MathF.Atan2(dy, dx) * 180f / MathF.PI;
    }
}

