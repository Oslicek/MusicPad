using Microsoft.Maui.Graphics;
using MusicPad.Core.Models;

namespace MusicPad.Controls;

/// <summary>
/// Drawable for Chorus controls with on/off button, Depth and Rate knobs.
/// </summary>
public class ChorusDrawable
{
    private readonly ChorusSettings _settings;
    
    // Knob colors
    private static readonly Color KnobBaseColor = Color.FromArgb("#CD8B5A");
    private static readonly Color KnobHighlightColor = Color.FromArgb("#E8A878");
    private static readonly Color KnobShadowColor = Color.FromArgb("#8B5A3A");
    private static readonly Color IndicatorColor = Color.FromArgb("#4A3020");
    private static readonly Color LabelColor = Color.FromArgb("#888888");
    private static readonly Color DisabledColor = Color.FromArgb("#555555");
    
    // Button colors
    private static readonly Color ButtonOnColor = Color.FromArgb("#4CAF50");
    private static readonly Color ButtonOffColor = Color.FromArgb("#444466");

    private RectF _onOffButtonRect;
    private RectF _depthKnobRect;
    private RectF _rateKnobRect;
    private float _knobRadius;
    private bool _isDraggingDepth;
    private bool _isDraggingRate;
    private float _lastAngle;

    public event EventHandler? InvalidateRequested;

    public ChorusDrawable(ChorusSettings settings)
    {
        _settings = settings;
        _settings.DepthChanged += (s, e) => InvalidateRequested?.Invoke(this, EventArgs.Empty);
        _settings.RateChanged += (s, e) => InvalidateRequested?.Invoke(this, EventArgs.Empty);
        _settings.EnabledChanged += (s, e) => InvalidateRequested?.Invoke(this, EventArgs.Empty);
    }

    public ChorusSettings Settings => _settings;

    /// <summary>
    /// Draws the Chorus controls.
    /// </summary>
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        float padding = 4f;
        float buttonSize = 20f;
        float knobSize = Math.Min(dirtyRect.Height - 16, 50f);
        _knobRadius = knobSize * 0.4f;
        bool isEnabled = _settings.IsEnabled;
        
        // On/Off button on the left
        float buttonX = dirtyRect.X + padding;
        float buttonY = dirtyRect.Y + (dirtyRect.Height - buttonSize) / 2;
        _onOffButtonRect = new RectF(buttonX, buttonY, buttonSize, buttonSize);
        DrawOnOffButton(canvas, _onOffButtonRect);
        
        // Two knobs side by side after the button
        float knobsStartX = buttonX + buttonSize + padding * 2;
        float knobY = dirtyRect.Y + dirtyRect.Height / 2;
        
        float depthX = knobsStartX + _knobRadius + padding;
        float rateX = depthX + _knobRadius * 2 + padding * 3;
        
        _depthKnobRect = new RectF(depthX - _knobRadius - 5, knobY - _knobRadius - 5,
                                    _knobRadius * 2 + 10, _knobRadius * 2 + 10);
        _rateKnobRect = new RectF(rateX - _knobRadius - 5, knobY - _knobRadius - 5,
                                   _knobRadius * 2 + 10, _knobRadius * 2 + 10);

        DrawKnob(canvas, depthX, knobY, _knobRadius, _settings.Depth, "DEPTH", isEnabled);
        DrawKnob(canvas, rateX, knobY, _knobRadius, _settings.Rate, "RATE", isEnabled);
    }

    private void DrawOnOffButton(ICanvas canvas, RectF rect)
    {
        bool isOn = _settings.IsEnabled;
        
        canvas.FillColor = isOn ? ButtonOnColor : ButtonOffColor;
        canvas.FillRoundedRectangle(rect, 4);
        
        canvas.StrokeColor = isOn ? ButtonOnColor.WithAlpha(0.8f) : Color.FromArgb("#666688");
        canvas.StrokeSize = 1;
        canvas.DrawRoundedRectangle(rect, 4);
        
        float iconSize = rect.Width * 0.5f;
        float centerX = rect.Center.X;
        float centerY = rect.Center.Y;
        
        canvas.StrokeColor = Colors.White;
        canvas.StrokeSize = 2;
        
        var path = new PathF();
        for (int a = 60; a <= 300; a += 10)
        {
            float rad = a * MathF.PI / 180f;
            float x = centerX + MathF.Cos(rad) * iconSize * 0.4f;
            float y = centerY + MathF.Sin(rad) * iconSize * 0.4f;
            if (a == 60)
                path.MoveTo(x, y);
            else
                path.LineTo(x, y);
        }
        canvas.DrawPath(path);
        
        canvas.DrawLine(centerX, centerY - iconSize * 0.5f, centerX, centerY);
    }

    private void DrawKnob(ICanvas canvas, float centerX, float centerY, float radius, float value, string label, bool isEnabled)
    {
        Color baseColor = isEnabled ? KnobBaseColor : DisabledColor;
        Color highlightColor = isEnabled ? KnobHighlightColor : DisabledColor.WithAlpha(0.6f);
        Color shadowColor = isEnabled ? KnobShadowColor : Color.FromArgb("#333333");
        
        canvas.FillColor = shadowColor;
        canvas.FillCircle(centerX + 1, centerY + 1, radius);
        
        canvas.FillColor = baseColor;
        canvas.FillCircle(centerX, centerY, radius);
        
        canvas.FillColor = highlightColor.WithAlpha(0.4f);
        canvas.FillCircle(centerX - radius * 0.15f, centerY - radius * 0.15f, radius * 0.6f);
        
        canvas.FillColor = baseColor;
        canvas.FillCircle(centerX, centerY, radius * 0.85f);
        
        canvas.StrokeColor = shadowColor.WithAlpha(0.5f);
        canvas.StrokeSize = 1;
        canvas.DrawCircle(centerX, centerY, radius);

        float minAngle = 225f;
        float maxAngle = -45f;
        float totalAngle = maxAngle - minAngle;
        if (totalAngle > 0) totalAngle -= 360;
        float currentAngle = minAngle + totalAngle * value;
        float radians = currentAngle * MathF.PI / 180f;
        
        float notchDistance = radius * 0.7f;
        float notchX = centerX + notchDistance * MathF.Cos(radians);
        float notchY = centerY - notchDistance * MathF.Sin(radians);
        
        float notchRadius = radius * 0.12f;
        canvas.FillColor = isEnabled ? IndicatorColor : Color.FromArgb("#222222");
        canvas.FillCircle(notchX, notchY, notchRadius);

        canvas.FontSize = 9;
        canvas.FontColor = isEnabled ? LabelColor : DisabledColor;
        canvas.DrawString(label, centerX - radius, centerY + radius + 2, radius * 2, 12,
            HorizontalAlignment.Center, VerticalAlignment.Top);
    }

    public bool OnTouch(float x, float y, bool isStart)
    {
        var point = new PointF(x, y);

        if (isStart)
        {
            if (_onOffButtonRect.Contains(point))
            {
                _settings.IsEnabled = !_settings.IsEnabled;
                return true;
            }
            
            if (!_settings.IsEnabled)
                return false;
            
            if (_depthKnobRect.Contains(point))
            {
                _isDraggingDepth = true;
                _lastAngle = GetAngleFromKnobCenter(_depthKnobRect, x, y);
                return true;
            }
            else if (_rateKnobRect.Contains(point))
            {
                _isDraggingRate = true;
                _lastAngle = GetAngleFromKnobCenter(_rateKnobRect, x, y);
                return true;
            }
        }
        else
        {
            if (_isDraggingDepth)
            {
                UpdateDepthKnob(x, y);
                return true;
            }
            else if (_isDraggingRate)
            {
                UpdateRateKnob(x, y);
                return true;
            }
        }

        return false;
    }

    private void UpdateDepthKnob(float x, float y)
    {
        float currentAngle = GetAngleFromKnobCenter(_depthKnobRect, x, y);
        float angleDelta = currentAngle - _lastAngle;
        
        if (angleDelta > 180) angleDelta -= 360;
        if (angleDelta < -180) angleDelta += 360;
        
        float totalAngle = -45f - 225f;
        if (totalAngle > 0) totalAngle -= 360;
        
        float valueDelta = angleDelta / totalAngle;
        _settings.Depth = Math.Clamp(_settings.Depth + valueDelta, 0f, 1f);
        
        _lastAngle = currentAngle;
    }

    private void UpdateRateKnob(float x, float y)
    {
        float currentAngle = GetAngleFromKnobCenter(_rateKnobRect, x, y);
        float angleDelta = currentAngle - _lastAngle;
        
        if (angleDelta > 180) angleDelta -= 360;
        if (angleDelta < -180) angleDelta += 360;
        
        float totalAngle = -45f - 225f;
        if (totalAngle > 0) totalAngle -= 360;
        
        float valueDelta = angleDelta / totalAngle;
        _settings.Rate = Math.Clamp(_settings.Rate + valueDelta, 0f, 1f);
        
        _lastAngle = currentAngle;
    }

    private float GetAngleFromKnobCenter(RectF knobRect, float x, float y)
    {
        float centerX = knobRect.Center.X;
        float centerY = knobRect.Center.Y;
        float dx = x - centerX;
        float dy = centerY - y;
        return MathF.Atan2(dy, dx) * 180f / MathF.PI;
    }

    public void OnTouchEnd()
    {
        _isDraggingDepth = false;
        _isDraggingRate = false;
    }
}

