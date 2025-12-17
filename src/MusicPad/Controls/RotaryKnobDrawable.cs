using Microsoft.Maui.Graphics;

namespace MusicPad.Controls;

/// <summary>
/// A simple hardware-style rotating knob control for volume.
/// </summary>
public class RotaryKnobDrawable : IDrawable
{
    // Warm copper/bronze color scheme like hardware knobs
    private static readonly Color KnobBaseColor = Color.FromArgb("#CD8B5A");
    private static readonly Color KnobHighlightColor = Color.FromArgb("#E8A878");
    private static readonly Color KnobShadowColor = Color.FromArgb("#8B5A3A");
    private static readonly Color IndicatorColor = Color.FromArgb("#4A3020");
    private static readonly Color LabelColor = Color.FromArgb("#888888");

    private float _value = 0.75f; // 0-1 range, default 75%
    private float _minAngle = 225f; // Start angle (7 o'clock position)
    private float _maxAngle = -45f; // End angle (5 o'clock position)
    private string _label = "VOL";
    
    private float _knobCenterX;
    private float _knobCenterY;
    private float _knobRadius;
    private bool _isDragging;
    private float _lastAngle;

    public event EventHandler<float>? ValueChanged;

    public float Value
    {
        get => _value;
        set
        {
            _value = Math.Clamp(value, 0f, 1f);
            ValueChanged?.Invoke(this, _value);
        }
    }

    public string Label
    {
        get => _label;
        set => _label = value;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        float size = Math.Min(dirtyRect.Width, dirtyRect.Height - 20); // Leave room for label
        _knobRadius = size * 0.45f;
        _knobCenterX = dirtyRect.Width / 2;
        _knobCenterY = dirtyRect.Height / 2 - 10;

        // Draw simple knob body with subtle 3D effect
        DrawKnobBody(canvas);
        
        // Draw indicator notch
        DrawIndicator(canvas);
        
        // Draw label
        DrawLabel(canvas, dirtyRect);
    }

    private void DrawKnobBody(ICanvas canvas)
    {
        // Shadow/depth effect
        canvas.FillColor = KnobShadowColor;
        canvas.FillCircle(_knobCenterX + 2, _knobCenterY + 2, _knobRadius);
        
        // Main knob body - gradient-like effect with concentric circles
        canvas.FillColor = KnobBaseColor;
        canvas.FillCircle(_knobCenterX, _knobCenterY, _knobRadius);
        
        // Subtle highlight on top-left
        canvas.FillColor = KnobHighlightColor.WithAlpha(0.4f);
        canvas.FillCircle(_knobCenterX - _knobRadius * 0.15f, 
                         _knobCenterY - _knobRadius * 0.15f, 
                         _knobRadius * 0.7f);
        
        // Inner area slightly darker
        canvas.FillColor = KnobBaseColor;
        canvas.FillCircle(_knobCenterX, _knobCenterY, _knobRadius * 0.85f);
        
        // Subtle edge ring
        canvas.StrokeColor = KnobShadowColor.WithAlpha(0.5f);
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
        
        // Simple notch/indent on the edge of the knob
        float notchDistance = _knobRadius * 0.75f;
        float notchX = _knobCenterX + notchDistance * MathF.Cos(radians);
        float notchY = _knobCenterY - notchDistance * MathF.Sin(radians);
        
        // Draw circular indent
        float notchRadius = _knobRadius * 0.12f;
        
        // Shadow for depth
        canvas.FillColor = IndicatorColor;
        canvas.FillCircle(notchX, notchY, notchRadius);
        
        // Highlight edge
        canvas.FillColor = KnobShadowColor;
        canvas.FillCircle(notchX - 1, notchY - 1, notchRadius * 0.6f);
    }

    private void DrawLabel(ICanvas canvas, RectF dirtyRect)
    {
        canvas.FontSize = 14;
        canvas.FontColor = LabelColor;
        canvas.DrawString(_label, 0, dirtyRect.Height - 20, dirtyRect.Width, 20, 
            HorizontalAlignment.Center, VerticalAlignment.Center);
    }

    public void OnTouch(float x, float y, bool isStart)
    {
        if (isStart)
        {
            float dx = x - _knobCenterX;
            float dy = y - _knobCenterY;
            float distance = MathF.Sqrt(dx * dx + dy * dy);
            
            if (distance <= _knobRadius + 30)
            {
                _isDragging = true;
                _lastAngle = GetAngleFromPoint(x, y);
            }
        }
        else if (_isDragging)
        {
            float currentAngle = GetAngleFromPoint(x, y);
            float angleDelta = currentAngle - _lastAngle;
            
            if (angleDelta > 180) angleDelta -= 360;
            if (angleDelta < -180) angleDelta += 360;
            
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
        float dy = _knobCenterY - y;
        return MathF.Atan2(dy, dx) * 180f / MathF.PI;
    }
}

