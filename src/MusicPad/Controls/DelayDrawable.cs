using Microsoft.Maui.Graphics;
using MusicPad.Core.Layout;
using MusicPad.Core.Models;
using MusicPad.Core.Theme;
using MauiRectF = Microsoft.Maui.Graphics.RectF;
using MauiPointF = Microsoft.Maui.Graphics.PointF;
using LayoutRectF = MusicPad.Core.Layout.RectF;

namespace MusicPad.Controls;

/// <summary>
/// Drawable for Delay controls with on/off button, Time, Feedback, and Level knobs.
/// Uses fluent Layout DSL for responsive positioning.
/// </summary>
public class DelayDrawable
{
    // Drawing constants
    private const float KnobHitPadding = 5f;      // Must match layout constant
    private const float KnobMinAngle = 225f;      // Start angle for knob rotation
    private const float KnobMaxAngle = -45f;      // End angle for knob rotation
    private const float ToggleWidthRatio = 0.85f; // Toggle width relative to button
    private const float ToggleHeightRatio = 0.5f; // Toggle height relative to button
    private const float ToggleKnobRatio = 0.4f;   // Toggle knob size relative to height
    
    private readonly DelaySettings _settings;
    private readonly DelayLayoutDefinition _layoutDefinition = DelayLayoutDefinition.Instance;
    
    // Knob colors (dynamic for palette switching)
    private static Color KnobBaseColor => Color.FromArgb(AppColors.KnobBase);
    private static Color KnobHighlightColor => Color.FromArgb(AppColors.KnobHighlight);
    private static Color KnobShadowColor => Color.FromArgb(AppColors.KnobShadow);
    private static Color IndicatorColor => Color.FromArgb(AppColors.KnobIndicator);
    private static Color AccentColor => Color.FromArgb(AppColors.Accent);
    private static Color LabelColor => Color.FromArgb(AppColors.TextSecondary);
    private static Color DisabledColor => Color.FromArgb(AppColors.Disabled);
    
    // Button colors (dynamic for palette switching)
    private static Color ButtonOnColor => Color.FromArgb(AppColors.ButtonOn);
    private static Color ButtonOffColor => Color.FromArgb(AppColors.ButtonOff);

    private MauiRectF _onOffButtonRect;
    private MauiRectF _timeKnobRect;
    private MauiRectF _feedbackKnobRect;
    private MauiRectF _levelKnobRect;
    private float _knobRadius;
    private int _draggingKnob = -1; // 0=time, 1=feedback, 2=level
    private float _lastAngle;
    
    private PadreaShape _padreaShape = PadreaShape.Square;

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
    
    public void SetPadreaShape(PadreaShape shape)
    {
        _padreaShape = shape;
    }

    /// <summary>
    /// Draws the Delay controls using the Layout DSL.
    /// </summary>
    public void Draw(ICanvas canvas, MauiRectF dirtyRect)
    {
        // Create layout context from bounds and padrea shape
        var bounds = new LayoutRectF(dirtyRect.X, dirtyRect.Y, dirtyRect.Width, dirtyRect.Height);
        var context = LayoutContext.FromBounds(bounds, _padreaShape);
        
        // Calculate layout using the fluent DSL
        var layout = _layoutDefinition.Calculate(bounds, context);
        
        // Convert layout results to MAUI RectF for hit testing
        var onOffRect = layout[DelayLayoutDefinition.OnOffButton];
        var timeRect = layout[DelayLayoutDefinition.TimeKnob];
        var feedbackRect = layout[DelayLayoutDefinition.FeedbackKnob];
        var levelRect = layout[DelayLayoutDefinition.LevelKnob];
        
        _onOffButtonRect = new MauiRectF(onOffRect.X, onOffRect.Y, onOffRect.Width, onOffRect.Height);
        _timeKnobRect = new MauiRectF(timeRect.X, timeRect.Y, timeRect.Width, timeRect.Height);
        _feedbackKnobRect = new MauiRectF(feedbackRect.X, feedbackRect.Y, feedbackRect.Width, feedbackRect.Height);
        _levelKnobRect = new MauiRectF(levelRect.X, levelRect.Y, levelRect.Width, levelRect.Height);
        
        // Calculate knob radius from hit rect (hit rect = diameter + 2*padding)
        _knobRadius = (timeRect.Width - KnobHitPadding * 2) / 2;
        
        bool isEnabled = _settings.IsEnabled;
        
        // Draw controls at calculated positions
        DrawOnOffButton(canvas, _onOffButtonRect);
        DrawKnob(canvas, timeRect.CenterX, timeRect.CenterY, _knobRadius, _settings.Time, "TIME", isEnabled);
        DrawKnob(canvas, feedbackRect.CenterX, feedbackRect.CenterY, _knobRadius, _settings.Feedback, "FDBK", isEnabled);
        DrawKnob(canvas, levelRect.CenterX, levelRect.CenterY, _knobRadius, _settings.Level, "LVL", isEnabled);
    }

    private void DrawOnOffButton(ICanvas canvas, MauiRectF rect)
    {
        bool isOn = _settings.IsEnabled;
        float cx = rect.Center.X;
        float cy = rect.Center.Y;
        float toggleWidth = rect.Width * ToggleWidthRatio;
        float toggleHeight = rect.Height * ToggleHeightRatio;
        float knobRadius = toggleHeight * ToggleKnobRatio;
        
        canvas.FillColor = isOn ? ButtonOnColor : ButtonOffColor;
        canvas.FillRoundedRectangle(cx - toggleWidth / 2, cy - toggleHeight / 2, toggleWidth, toggleHeight, toggleHeight / 2);
        
        canvas.StrokeColor = isOn ? ButtonOnColor.WithAlpha(0.8f) : Color.FromArgb(AppColors.ButtonBorder);
        canvas.StrokeSize = 1;
        canvas.DrawRoundedRectangle(cx - toggleWidth / 2, cy - toggleHeight / 2, toggleWidth, toggleHeight, toggleHeight / 2);
        
        float knobOffset = toggleWidth / 2 - toggleHeight / 2;
        float knobX = isOn ? cx + knobOffset : cx - knobOffset;
        
        canvas.FillColor = KnobShadowColor.WithAlpha(0.5f);
        canvas.FillCircle(knobX + 1, cy + 1, knobRadius);
        
        canvas.FillColor = Colors.White;
        canvas.FillCircle(knobX, cy, knobRadius);
    }

    private void DrawKnob(ICanvas canvas, float centerX, float centerY, float radius, float value, string label, bool isEnabled)
    {
        Color baseColor = isEnabled ? KnobBaseColor : DisabledColor;
        Color highlightColor = isEnabled ? KnobHighlightColor : DisabledColor.WithAlpha(0.6f);
        Color shadowColor = isEnabled ? KnobShadowColor : Color.FromArgb(AppColors.DisabledDark);
        
        float totalAngle = GetTotalKnobAngle();
        
        // Draw radial markers
        DrawKnobMarkers(canvas, centerX, centerY, radius, isEnabled, totalAngle);
        
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

        float currentAngle = KnobMinAngle + totalAngle * value;
        float radians = currentAngle * MathF.PI / 180f;
        
        float notchDistance = radius * 0.7f;
        float notchX = centerX + notchDistance * MathF.Cos(radians);
        float notchY = centerY - notchDistance * MathF.Sin(radians);
        
        float notchRadius = radius * 0.12f;
        canvas.FillColor = isEnabled ? IndicatorColor : Color.FromArgb(AppColors.DisabledDarker);
        canvas.FillCircle(notchX, notchY, notchRadius);

        canvas.FontSize = 8;
        canvas.FontColor = isEnabled ? LabelColor : DisabledColor;
        canvas.DrawString(label, centerX - radius, centerY + radius + 4, radius * 2, 12,
            HorizontalAlignment.Center, VerticalAlignment.Top);
    }

    private void DrawKnobMarkers(ICanvas canvas, float centerX, float centerY, float radius, bool isEnabled, float totalAngle)
    {
        canvas.StrokeColor = isEnabled ? AccentColor : DisabledColor;
        canvas.StrokeSize = 1.5f;
        canvas.StrokeLineCap = LineCap.Round;
        float outerRadius = radius + 5;
        float innerRadius = radius + 2;
        for (int i = 0; i <= 6; i++)
        {
            float t = i / 6f;
            float angle = KnobMinAngle + totalAngle * t;
            float rads = angle * MathF.PI / 180f;
            canvas.DrawLine(centerX + innerRadius * MathF.Cos(rads), centerY - innerRadius * MathF.Sin(rads),
                           centerX + outerRadius * MathF.Cos(rads), centerY - outerRadius * MathF.Sin(rads));
        }
    }

    private static float GetTotalKnobAngle()
    {
        float totalAngle = KnobMaxAngle - KnobMinAngle;
        if (totalAngle > 0) totalAngle -= 360;
        return totalAngle;
    }

    public bool OnTouch(float x, float y, bool isStart)
    {
        var point = new MauiPointF(x, y);

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
                UpdateKnobValue();
                return true;
            }
        }

        return false;
    }

    private void UpdateKnobValue()
    {
        // Knob update is handled in the drag continuation
    }

    private float GetAngleFromKnobCenter(MauiRectF knobRect, float x, float y)
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
    
    public void OnTouchMove(float x, float y)
    {
        if (_draggingKnob < 0) return;
        
        MauiRectF knobRect = _draggingKnob switch
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
        
        float totalAngle = GetTotalKnobAngle();
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
}
