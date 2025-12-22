using Microsoft.Maui.Graphics;
using MusicPad.Core.Layout;
using MusicPad.Core.Models;
using MusicPad.Core.Theme;
using MauiRectF = Microsoft.Maui.Graphics.RectF;
using MauiPointF = Microsoft.Maui.Graphics.PointF;
using LayoutRectF = MusicPad.Core.Layout.RectF;

namespace MusicPad.Controls;

/// <summary>
/// Drawable for Reverb controls with on/off button, level knob, and 4-button type selector.
/// Uses fluent Layout DSL for responsive positioning.
/// </summary>
public class ReverbDrawable
{
    // Drawing constants
    private const float KnobHitPadding = 5f;
    private const float KnobMinAngle = 225f;
    private const float KnobMaxAngle = -45f;
    private const float ToggleWidthRatio = 0.85f;
    private const float ToggleHeightRatio = 0.5f;
    private const float ToggleKnobRatio = 0.4f;
    
    private readonly ReverbSettings _settings;
    private readonly ReverbLayoutDefinition _layoutDefinition = ReverbLayoutDefinition.Instance;
    
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
    
    // Type selector colors (dynamic for palette switching)
    private static Color TypeButtonBaseColor => Color.FromArgb(AppColors.TypeButtonBase);
    private static Color TypeButtonSelectedColor => Color.FromArgb(AppColors.TypeButtonSelected);
    private static Color TypeButtonTextColor => Color.FromArgb(AppColors.TextSecondary);

    private MauiRectF _onOffButtonRect;
    private MauiRectF _levelKnobRect;
    private readonly MauiRectF[] _typeButtonRects = new MauiRectF[4];
    private float _knobRadius;
    private bool _isDraggingLevel;
    private float _lastAngle;
    
    private PadreaShape _padreaShape = PadreaShape.Square;

    private static readonly string[] TypeLabels = { "ROOM", "HALL", "PLATE", "CATH" };

    public event EventHandler? InvalidateRequested;

    public ReverbDrawable(ReverbSettings settings)
    {
        _settings = settings;
        _settings.LevelChanged += (s, e) => InvalidateRequested?.Invoke(this, EventArgs.Empty);
        _settings.TypeChanged += (s, e) => InvalidateRequested?.Invoke(this, EventArgs.Empty);
        _settings.EnabledChanged += (s, e) => InvalidateRequested?.Invoke(this, EventArgs.Empty);
    }

    public ReverbSettings Settings => _settings;
    
    public void SetPadreaShape(PadreaShape shape)
    {
        _padreaShape = shape;
    }

    /// <summary>
    /// Draws the Reverb controls using the Layout DSL.
    /// </summary>
    public void Draw(ICanvas canvas, MauiRectF dirtyRect)
    {
        // Create layout context from bounds and padrea shape
        var bounds = new LayoutRectF(dirtyRect.X, dirtyRect.Y, dirtyRect.Width, dirtyRect.Height);
        var context = LayoutContext.FromBounds(bounds, _padreaShape);
        
        // Calculate layout using the fluent DSL
        var layout = _layoutDefinition.Calculate(bounds, context);
        
        // Convert layout results to MAUI RectF for hit testing
        var onOffRect = layout[ReverbLayoutDefinition.OnOffButton];
        var levelRect = layout[ReverbLayoutDefinition.LevelKnob];
        
        _onOffButtonRect = new MauiRectF(onOffRect.X, onOffRect.Y, onOffRect.Width, onOffRect.Height);
        _levelKnobRect = new MauiRectF(levelRect.X, levelRect.Y, levelRect.Width, levelRect.Height);
        
        for (int i = 0; i < 4; i++)
        {
            var typeRect = layout[$"TypeButton{i}"];
            _typeButtonRects[i] = new MauiRectF(typeRect.X, typeRect.Y, typeRect.Width, typeRect.Height);
        }
        
        // Calculate knob radius from hit rect
        _knobRadius = (levelRect.Width - KnobHitPadding * 2) / 2;
        
        bool isEnabled = _settings.IsEnabled;
        
        // Draw controls at calculated positions
        DrawOnOffButton(canvas, _onOffButtonRect);
        DrawKnob(canvas, levelRect.CenterX, levelRect.CenterY, _knobRadius, _settings.Level, "LVL", isEnabled);
        
        for (int i = 0; i < 4; i++)
        {
            DrawCircleTypeButton(canvas, _typeButtonRects[i], TypeLabels[i], (int)_settings.Type == i, isEnabled);
        }
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

        canvas.FontSize = 9;
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

    private void DrawCircleTypeButton(ICanvas canvas, MauiRectF rect, string label, bool isSelected, bool isEnabled)
    {
        float centerX = rect.Center.X;
        float centerY = rect.Center.Y;
        float radius = Math.Min(rect.Width, rect.Height) / 2f;
        
        if (isSelected && isEnabled)
        {
            canvas.FillColor = TypeButtonSelectedColor;
        }
        else
        {
            canvas.FillColor = isEnabled ? TypeButtonBaseColor : DisabledColor.WithAlpha(0.3f);
        }
        canvas.FillCircle(centerX, centerY, radius);
        
        canvas.StrokeColor = isSelected && isEnabled ? AccentColor : (isEnabled ? Color.FromArgb(AppColors.ButtonBorder) : DisabledColor);
        canvas.StrokeSize = isSelected ? 2 : 1;
        canvas.DrawCircle(centerX, centerY, radius);
        
        canvas.FontSize = 7;
        canvas.FontColor = isEnabled ? LabelColor : DisabledColor;
        canvas.DrawString(label, centerX - 20, centerY + radius + 2, 40, 12,
            HorizontalAlignment.Center, VerticalAlignment.Top);
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
            
            for (int i = 0; i < 4; i++)
            {
                if (_typeButtonRects[i].Contains(point))
                {
                    _settings.Type = (ReverbType)i;
                    return true;
                }
            }
            
            if (_levelKnobRect.Contains(point))
            {
                _isDraggingLevel = true;
                _lastAngle = GetAngleFromKnobCenter(_levelKnobRect, x, y);
                return true;
            }
        }
        else
        {
            if (_isDraggingLevel)
            {
                UpdateKnobValue(_levelKnobRect, x, y, v => _settings.Level = v, _settings.Level);
                return true;
            }
        }

        return false;
    }

    private void UpdateKnobValue(MauiRectF knobRect, float x, float y, Action<float> setValue, float currentValue)
    {
        float currentAngle = GetAngleFromKnobCenter(knobRect, x, y);
        float angleDelta = currentAngle - _lastAngle;
        
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
        _isDraggingLevel = false;
    }
}
