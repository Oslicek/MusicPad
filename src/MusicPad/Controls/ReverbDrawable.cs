using Microsoft.Maui.Graphics;
using MusicPad.Core.Models;
using MusicPad.Core.Theme;

namespace MusicPad.Controls;

/// <summary>
/// Drawable for Reverb controls with on/off button, level knob, and 4-button type selector.
/// </summary>
public class ReverbDrawable
{
    private readonly ReverbSettings _settings;
    
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
    private static Color TypeButtonHighlightColor => Color.FromArgb(AppColors.TypeButtonHighlight);
    private static Color TypeButtonTextColor => Color.FromArgb(AppColors.TextSecondary);
    private static Color TypeButtonTextSelectedColor => Color.FromArgb(AppColors.TextWhite);

    private RectF _onOffButtonRect;
    private RectF _levelKnobRect;
    private readonly RectF[] _typeButtonRects = new RectF[4];
    private float _knobRadius;
    private bool _isDraggingLevel;
    private float _lastAngle;

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

    /// <summary>
    /// Draws the Reverb controls.
    /// </summary>
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        float padding = 4f;
        float buttonSize = 28f;
        float knobSize = Math.Min(dirtyRect.Height - 16, 50f);
        _knobRadius = knobSize * 0.4f;
        bool isEnabled = _settings.IsEnabled;
        
        // On/Off button on the left
        float buttonX = dirtyRect.X + padding;
        float buttonY = dirtyRect.Y + (dirtyRect.Height - buttonSize) / 2;
        _onOffButtonRect = new RectF(buttonX, buttonY, buttonSize, buttonSize);
        DrawOnOffButton(canvas, _onOffButtonRect);
        
        // Level knob after the button
        float knobX = buttonX + buttonSize + padding * 2 + _knobRadius + padding;
        float knobY = dirtyRect.Y + dirtyRect.Height / 2;
        
        _levelKnobRect = new RectF(knobX - _knobRadius - 5, knobY - _knobRadius - 5,
                                    _knobRadius * 2 + 10, _knobRadius * 2 + 10);
        DrawKnob(canvas, knobX, knobY, _knobRadius, _settings.Level, "LVL", isEnabled);
        
        // Type selector buttons (circular with labels below) after the knob
        float selectorStartX = knobX + _knobRadius + padding * 4;
        float circleButtonSize = 24f;
        float centerY = dirtyRect.Y + dirtyRect.Height / 2 - 5; // Shift up for labels
        
        for (int i = 0; i < 4; i++)
        {
            float x = selectorStartX + i * (circleButtonSize + padding + 10);
            _typeButtonRects[i] = new RectF(x, centerY - circleButtonSize / 2, circleButtonSize, circleButtonSize);
            DrawCircleTypeButton(canvas, _typeButtonRects[i], TypeLabels[i], (int)_settings.Type == i, isEnabled);
        }
    }

    private void DrawOnOffButton(ICanvas canvas, RectF rect)
    {
        bool isOn = _settings.IsEnabled;
        float cx = rect.Center.X;
        float cy = rect.Center.Y;
        float toggleWidth = rect.Width * 0.85f;
        float toggleHeight = rect.Height * 0.5f;
        float knobRadius = toggleHeight * 0.4f;
        
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
        
        float minAngle = 225f;
        float maxAngle = -45f;
        float totalAngle = maxAngle - minAngle;
        if (totalAngle > 0) totalAngle -= 360;
        
        // Draw radial markers - use accent color when enabled
        canvas.StrokeColor = isEnabled ? AccentColor : DisabledColor;
        canvas.StrokeSize = 1.5f;
        canvas.StrokeLineCap = LineCap.Round;
        float outerRadius = radius + 5;
        float innerRadius = radius + 2;
        for (int i = 0; i <= 6; i++)
        {
            float t = i / 6f;
            float angle = minAngle + totalAngle * t;
            float rads = angle * MathF.PI / 180f;
            canvas.DrawLine(centerX + innerRadius * MathF.Cos(rads), centerY - innerRadius * MathF.Sin(rads),
                           centerX + outerRadius * MathF.Cos(rads), centerY - outerRadius * MathF.Sin(rads));
        }
        
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

        float currentAngle = minAngle + totalAngle * value;
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

    private void DrawTypeButton(ICanvas canvas, RectF rect, ReverbType type, bool isEnabled)
    {
        bool isSelected = _settings.Type == type;
        int index = (int)type;
        
        // Determine corner radii for seamless appearance
        float leftRadius = index == 0 ? 6 : 0;
        float rightRadius = index == 3 ? 6 : 0;
        
        // Background with knob-style aesthetic for selected button
        if (isSelected && isEnabled)
        {
            // Draw shadow for depth
            canvas.FillColor = KnobShadowColor;
            canvas.FillRoundedRectangle(new RectF(rect.X + 1, rect.Y + 1, rect.Width, rect.Height), 
                leftRadius, rightRadius, rightRadius, leftRadius);
            
            // Main button with copper color
            canvas.FillColor = TypeButtonSelectedColor;
            canvas.FillRoundedRectangle(rect, leftRadius, rightRadius, rightRadius, leftRadius);
            
            // Subtle highlight
            canvas.FillColor = TypeButtonHighlightColor.WithAlpha(0.3f);
            var highlightRect = new RectF(rect.X + 2, rect.Y + 2, rect.Width - 4, rect.Height * 0.4f);
            canvas.FillRoundedRectangle(highlightRect, Math.Max(0, leftRadius - 2), Math.Max(0, rightRadius - 2), 0, 0);
        }
        else
        {
            // Unselected or disabled
            canvas.FillColor = isEnabled ? TypeButtonBaseColor : DisabledColor.WithAlpha(0.5f);
            canvas.FillRoundedRectangle(rect, leftRadius, rightRadius, rightRadius, leftRadius);
        }
        
        // Border
        canvas.StrokeColor = isSelected && isEnabled ? KnobShadowColor : Color.FromArgb(AppColors.DisabledBorder);
        canvas.StrokeSize = 1;
        canvas.DrawRoundedRectangle(rect, leftRadius, rightRadius, rightRadius, leftRadius);
        
        // Label
        canvas.FontSize = 10;
        canvas.FontColor = isSelected && isEnabled ? TypeButtonTextSelectedColor : 
                          (isEnabled ? TypeButtonTextColor : DisabledColor);
        canvas.DrawString(TypeLabels[index], rect, HorizontalAlignment.Center, VerticalAlignment.Center);
    }
    
    private void DrawCircleTypeButton(ICanvas canvas, RectF rect, string label, bool isSelected, bool isEnabled)
    {
        float centerX = rect.Center.X;
        float centerY = rect.Center.Y;
        float radius = Math.Min(rect.Width, rect.Height) / 2f;
        
        // Circle background
        if (isSelected && isEnabled)
        {
            canvas.FillColor = TypeButtonSelectedColor;
        }
        else
        {
            canvas.FillColor = isEnabled ? TypeButtonBaseColor : DisabledColor.WithAlpha(0.3f);
        }
        canvas.FillCircle(centerX, centerY, radius);
        
        // Circle border
        canvas.StrokeColor = isSelected && isEnabled ? AccentColor : (isEnabled ? Color.FromArgb(AppColors.ButtonBorder) : DisabledColor);
        canvas.StrokeSize = isSelected ? 2 : 1;
        canvas.DrawCircle(centerX, centerY, radius);
        
        // Label below the button
        canvas.FontSize = 7;
        canvas.FontColor = isEnabled ? LabelColor : DisabledColor;
        canvas.DrawString(label, centerX - 20, centerY + radius + 2, 40, 12,
            HorizontalAlignment.Center, VerticalAlignment.Top);
    }

    public bool OnTouch(float x, float y, bool isStart)
    {
        var point = new PointF(x, y);

        if (isStart)
        {
            // Check power button
            if (_onOffButtonRect.Contains(point))
            {
                _settings.IsEnabled = !_settings.IsEnabled;
                return true;
            }
            
            if (!_settings.IsEnabled)
                return false;
            
            // Check type selector buttons
            for (int i = 0; i < 4; i++)
            {
                if (_typeButtonRects[i].Contains(point))
                {
                    _settings.Type = (ReverbType)i;
                    return true;
                }
            }
            
            // Check level knob
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
                UpdateLevelKnob(x, y);
                return true;
            }
        }

        return false;
    }

    private void UpdateLevelKnob(float x, float y)
    {
        float currentAngle = GetAngleFromKnobCenter(_levelKnobRect, x, y);
        float angleDelta = currentAngle - _lastAngle;
        
        if (angleDelta > 180) angleDelta -= 360;
        if (angleDelta < -180) angleDelta += 360;
        
        float totalAngle = -45f - 225f;
        if (totalAngle > 0) totalAngle -= 360;
        
        float valueDelta = angleDelta / totalAngle;
        _settings.Level = Math.Clamp(_settings.Level + valueDelta, 0f, 1f);
        
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
        _isDraggingLevel = false;
    }
}

