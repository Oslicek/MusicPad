using Microsoft.Maui.Graphics;
using MusicPad.Core.Models;
using MusicPad.Core.Theme;

namespace MusicPad.Controls;

/// <summary>
/// Drawable for Delay controls with on/off button, Time, Feedback, and Level knobs.
/// </summary>
public class DelayDrawable
{
    private readonly DelaySettings _settings;
    
    // Knob colors (dynamic for palette switching)
    private static Color KnobBaseColor => Color.FromArgb(AppColors.KnobBase);
    private static Color KnobHighlightColor => Color.FromArgb(AppColors.KnobHighlight);
    private static Color KnobShadowColor => Color.FromArgb(AppColors.KnobShadow);
    private static Color IndicatorColor => Color.FromArgb(AppColors.KnobIndicator);
    private static Color LabelColor => Color.FromArgb(AppColors.TextSecondary);
    private static Color DisabledColor => Color.FromArgb(AppColors.Disabled);
    
    // Button colors (dynamic for palette switching)
    private static Color ButtonOnColor => Color.FromArgb(AppColors.ButtonOn);
    private static Color ButtonOffColor => Color.FromArgb(AppColors.ButtonOff);

    private RectF _onOffButtonRect;
    private RectF _timeKnobRect;
    private RectF _feedbackKnobRect;
    private RectF _levelKnobRect;
    private float _knobRadius;
    private int _draggingKnob = -1; // 0=time, 1=feedback, 2=level
    private float _lastAngle;

    public event EventHandler? InvalidateRequested;

    public DelayDrawable(DelaySettings settings)
    {
        _settings = settings;
        _settings.TimeChanged += (s, e) => InvalidateRequested?.Invoke(this, EventArgs.Empty);
        _settings.FeedbackChanged += (s, e) => InvalidateRequested?.Invoke(this, EventArgs.Empty);
        _settings.LevelChanged += (s, e) => InvalidateRequested?.Invoke(this, EventArgs.Empty);
        _settings.EnabledChanged += (s, e) => InvalidateRequested?.Invoke(this, EventArgs.Empty);
    }

    public DelaySettings Settings => _settings;

    /// <summary>
    /// Draws the Delay controls.
    /// </summary>
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        float padding = 4f;
        float buttonSize = 20f;
        
        // Calculate knob size based on available space (3 knobs)
        float availableWidth = dirtyRect.Width - buttonSize - padding * 5;
        float knobSize = Math.Min(dirtyRect.Height - 16, availableWidth / 3 - padding);
        knobSize = Math.Max(knobSize, 25f);
        _knobRadius = knobSize * 0.35f;
        
        bool isEnabled = _settings.IsEnabled;
        
        // On/Off button on the left
        float buttonX = dirtyRect.X + padding;
        float buttonY = dirtyRect.Y + (dirtyRect.Height - buttonSize) / 2;
        _onOffButtonRect = new RectF(buttonX, buttonY, buttonSize, buttonSize);
        DrawOnOffButton(canvas, _onOffButtonRect);
        
        // Three knobs side by side after the button
        float knobsStartX = buttonX + buttonSize + padding * 2;
        float knobY = dirtyRect.Y + dirtyRect.Height / 2;
        float knobSpacing = _knobRadius * 2 + padding * 2;
        
        float timeX = knobsStartX + _knobRadius + padding;
        float feedbackX = timeX + knobSpacing;
        float levelX = feedbackX + knobSpacing;
        
        _timeKnobRect = new RectF(timeX - _knobRadius - 5, knobY - _knobRadius - 5,
                                   _knobRadius * 2 + 10, _knobRadius * 2 + 10);
        _feedbackKnobRect = new RectF(feedbackX - _knobRadius - 5, knobY - _knobRadius - 5,
                                       _knobRadius * 2 + 10, _knobRadius * 2 + 10);
        _levelKnobRect = new RectF(levelX - _knobRadius - 5, knobY - _knobRadius - 5,
                                    _knobRadius * 2 + 10, _knobRadius * 2 + 10);

        DrawKnob(canvas, timeX, knobY, _knobRadius, _settings.Time, "TIME", isEnabled);
        DrawKnob(canvas, feedbackX, knobY, _knobRadius, _settings.Feedback, "FDBK", isEnabled);
        DrawKnob(canvas, levelX, knobY, _knobRadius, _settings.Level, "LVL", isEnabled);
    }

    private void DrawOnOffButton(ICanvas canvas, RectF rect)
    {
        bool isOn = _settings.IsEnabled;
        
        canvas.FillColor = isOn ? ButtonOnColor : ButtonOffColor;
        canvas.FillRoundedRectangle(rect, 4);
        
        canvas.StrokeColor = isOn ? ButtonOnColor.WithAlpha(0.8f) : Color.FromArgb(AppColors.ButtonBorder);
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
        Color shadowColor = isEnabled ? KnobShadowColor : Color.FromArgb(AppColors.DisabledDark);
        
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
        canvas.FillColor = isEnabled ? IndicatorColor : Color.FromArgb(AppColors.DisabledDarker);
        canvas.FillCircle(notchX, notchY, notchRadius);

        canvas.FontSize = 8;
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
            
            if (_timeKnobRect.Contains(point))
            {
                _draggingKnob = 0;
                _lastAngle = GetAngleFromKnobCenter(_timeKnobRect, x, y);
                return true;
            }
            else if (_feedbackKnobRect.Contains(point))
            {
                _draggingKnob = 1;
                _lastAngle = GetAngleFromKnobCenter(_feedbackKnobRect, x, y);
                return true;
            }
            else if (_levelKnobRect.Contains(point))
            {
                _draggingKnob = 2;
                _lastAngle = GetAngleFromKnobCenter(_levelKnobRect, x, y);
                return true;
            }
        }
        else
        {
            if (_draggingKnob >= 0)
            {
                UpdateKnob(x, y);
                return true;
            }
        }

        return false;
    }

    private void UpdateKnob(float x, float y)
    {
        RectF knobRect = _draggingKnob switch
        {
            0 => _timeKnobRect,
            1 => _feedbackKnobRect,
            2 => _levelKnobRect,
            _ => default
        };
        
        if (knobRect == default) return;
        
        float currentAngle = GetAngleFromKnobCenter(knobRect, x, y);
        float angleDelta = currentAngle - _lastAngle;
        
        if (angleDelta > 180) angleDelta -= 360;
        if (angleDelta < -180) angleDelta += 360;
        
        float totalAngle = -45f - 225f;
        if (totalAngle > 0) totalAngle -= 360;
        
        float valueDelta = angleDelta / totalAngle;
        
        switch (_draggingKnob)
        {
            case 0:
                _settings.Time = Math.Clamp(_settings.Time + valueDelta, 0f, 1f);
                break;
            case 1:
                _settings.Feedback = Math.Clamp(_settings.Feedback + valueDelta, 0f, 1f);
                break;
            case 2:
                _settings.Level = Math.Clamp(_settings.Level + valueDelta, 0f, 1f);
                break;
        }
        
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
        _draggingKnob = -1;
    }
}

