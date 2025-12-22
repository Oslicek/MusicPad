using Microsoft.Maui.Graphics;
using MusicPad.Core.Layout;
using MusicPad.Core.Models;
using MusicPad.Core.Theme;
using MauiRectF = Microsoft.Maui.Graphics.RectF;
using LayoutRectF = MusicPad.Core.Layout.RectF;

namespace MusicPad.Controls;

/// <summary>
/// Drawable for Chorus controls with on/off button, Depth and Rate knobs.
/// Uses fluent Layout DSL for responsive positioning.
/// </summary>
public class ChorusDrawable
{
    // Drawing constants
    private const float KnobHitPadding = 5f;      // Must match layout constant
    private const float KnobMinAngle = 225f;      // Start angle for knob rotation
    private const float KnobMaxAngle = -45f;      // End angle for knob rotation
    private const float ToggleWidthRatio = 0.85f; // Toggle width relative to button
    private const float ToggleHeightRatio = 0.5f; // Toggle height relative to button
    private const float ToggleKnobRatio = 0.4f;   // Toggle knob size relative to height
    
    private readonly ChorusSettings _settings;
    private readonly ChorusLayoutDefinition _layoutDefinition = ChorusLayoutDefinition.Instance;
    
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
    private MauiRectF _depthKnobRect;
    private MauiRectF _rateKnobRect;
    private float _knobRadius;
    private bool _isDraggingDepth;
    private bool _isDraggingRate;
    private float _lastAngle;
    
    // Layout context (can be set externally for padrea shape awareness)
    private PadreaShape _padreaShape = PadreaShape.Square;

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
    /// Sets the padrea shape for layout calculations.
    /// </summary>
    public void SetPadreaShape(PadreaShape shape)
    {
        _padreaShape = shape;
    }

    /// <summary>
    /// Draws the Chorus controls using the Layout DSL.
    /// </summary>
    public void Draw(ICanvas canvas, MauiRectF dirtyRect)
    {
        // Create layout context from bounds and padrea shape
        var bounds = new LayoutRectF(dirtyRect.X, dirtyRect.Y, dirtyRect.Width, dirtyRect.Height);
        var context = LayoutContext.FromBounds(bounds, _padreaShape);
        
        // Calculate layout using the fluent DSL
        var layout = _layoutDefinition.Calculate(bounds, context);
        
        // Convert layout results to MAUI RectF for hit testing
        var onOffRect = layout[ChorusLayoutDefinition.OnOffButton];
        var depthRect = layout[ChorusLayoutDefinition.DepthKnob];
        var rateRect = layout[ChorusLayoutDefinition.RateKnob];
        
        _onOffButtonRect = new MauiRectF(onOffRect.X, onOffRect.Y, onOffRect.Width, onOffRect.Height);
        _depthKnobRect = new MauiRectF(depthRect.X, depthRect.Y, depthRect.Width, depthRect.Height);
        _rateKnobRect = new MauiRectF(rateRect.X, rateRect.Y, rateRect.Width, rateRect.Height);
        
        // Calculate knob radius from hit rect (hit rect = diameter + 2*padding)
        _knobRadius = (depthRect.Width - KnobHitPadding * 2) / 2;
        
        bool isEnabled = _settings.IsEnabled;
        
        // Draw controls at calculated positions
        DrawOnOffButton(canvas, _onOffButtonRect);
        DrawKnob(canvas, depthRect.CenterX, depthRect.CenterY, _knobRadius, _settings.Depth, "DEPTH", isEnabled);
        DrawKnob(canvas, rateRect.CenterX, rateRect.CenterY, _knobRadius, _settings.Rate, "RATE", isEnabled);
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
        
        // Draw knob body
        canvas.FillColor = shadowColor;
        canvas.FillCircle(centerX + 1, centerY + 1, radius);
        
        canvas.FillColor = baseColor;
        canvas.FillCircle(centerX, centerY, radius);
        
        // Draw highlight
        canvas.FillColor = highlightColor.WithAlpha(0.4f);
        canvas.FillCircle(centerX - radius * 0.15f, centerY - radius * 0.15f, radius * 0.6f);
        
        canvas.FillColor = baseColor;
        canvas.FillCircle(centerX, centerY, radius * 0.85f);
        
        // Draw outline
        canvas.StrokeColor = shadowColor.WithAlpha(0.5f);
        canvas.StrokeSize = 1;
        canvas.DrawCircle(centerX, centerY, radius);

        // Draw indicator notch
        float currentAngle = KnobMinAngle + totalAngle * value;
        float radians = currentAngle * MathF.PI / 180f;
        float notchDistance = radius * 0.7f;
        float notchX = centerX + notchDistance * MathF.Cos(radians);
        float notchY = centerY - notchDistance * MathF.Sin(radians);
        float notchRadius = radius * 0.12f;
        
        canvas.FillColor = isEnabled ? IndicatorColor : Color.FromArgb(AppColors.DisabledDarker);
        canvas.FillCircle(notchX, notchY, notchRadius);

        // Draw label
        canvas.FontSize = 9;
        canvas.FontColor = isEnabled ? LabelColor : DisabledColor;
        canvas.DrawString(label, centerX - radius, centerY + radius + 4, radius * 2, 12,
            HorizontalAlignment.Center, VerticalAlignment.Top);
    }

    private void DrawKnobMarkers(ICanvas canvas, float centerX, float centerY, float radius, bool isEnabled, float totalAngle)
    {
        const int markerCount = 6;
        const float markerStrokeWidth = 1.5f;
        const float outerOffset = 5f;
        const float innerOffset = 2f;
        
        canvas.StrokeColor = isEnabled ? AccentColor : DisabledColor;
        canvas.StrokeSize = markerStrokeWidth;
        canvas.StrokeLineCap = LineCap.Round;
        
        float outerRadius = radius + outerOffset;
        float innerRadius = radius + innerOffset;
        
        for (int i = 0; i <= markerCount; i++)
        {
            float t = i / (float)markerCount;
            float angle = KnobMinAngle + totalAngle * t;
            float rads = angle * MathF.PI / 180f;
            canvas.DrawLine(
                centerX + innerRadius * MathF.Cos(rads), 
                centerY - innerRadius * MathF.Sin(rads),
                centerX + outerRadius * MathF.Cos(rads), 
                centerY - outerRadius * MathF.Sin(rads));
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
        var point = new Microsoft.Maui.Graphics.PointF(x, y);

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
                UpdateKnobValue(_depthKnobRect, x, y, v => _settings.Depth = v, _settings.Depth);
                return true;
            }
            else if (_isDraggingRate)
            {
                UpdateKnobValue(_rateKnobRect, x, y, v => _settings.Rate = v, _settings.Rate);
                return true;
            }
        }

        return false;
    }

    private void UpdateKnobValue(MauiRectF knobRect, float x, float y, Action<float> setValue, float currentValue)
    {
        float currentAngle = GetAngleFromKnobCenter(knobRect, x, y);
        float angleDelta = currentAngle - _lastAngle;
        
        // Normalize angle delta to handle wrap-around
        if (angleDelta > 180) angleDelta -= 360;
        if (angleDelta < -180) angleDelta += 360;
        
        float totalAngle = GetTotalKnobAngle();
        float valueDelta = angleDelta / totalAngle;
        setValue(Math.Clamp(currentValue + valueDelta, 0f, 1f));
        
        _lastAngle = currentAngle;
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
        _isDraggingDepth = false;
        _isDraggingRate = false;
    }
}
