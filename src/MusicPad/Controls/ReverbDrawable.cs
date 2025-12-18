using Microsoft.Maui.Graphics;
using MusicPad.Core.Models;

namespace MusicPad.Controls;

/// <summary>
/// Drawable for Reverb controls with on/off button, level knob, and 4-button type selector.
/// </summary>
public class ReverbDrawable
{
    private readonly ReverbSettings _settings;
    
    // Knob colors (matching other effects)
    private static readonly Color KnobBaseColor = Color.FromArgb("#CD8B5A");
    private static readonly Color KnobHighlightColor = Color.FromArgb("#E8A878");
    private static readonly Color KnobShadowColor = Color.FromArgb("#8B5A3A");
    private static readonly Color IndicatorColor = Color.FromArgb("#4A3020");
    private static readonly Color LabelColor = Color.FromArgb("#888888");
    private static readonly Color DisabledColor = Color.FromArgb("#555555");
    
    // Button colors
    private static readonly Color ButtonOnColor = Color.FromArgb("#4CAF50");
    private static readonly Color ButtonOffColor = Color.FromArgb("#444466");
    
    // Type selector colors (knob-style aesthetic)
    private static readonly Color TypeButtonBaseColor = Color.FromArgb("#3a3a5e");
    private static readonly Color TypeButtonSelectedColor = Color.FromArgb("#CD8B5A"); // Copper like knobs
    private static readonly Color TypeButtonHighlightColor = Color.FromArgb("#E8A878");
    private static readonly Color TypeButtonTextColor = Color.FromArgb("#888888");
    private static readonly Color TypeButtonTextSelectedColor = Color.FromArgb("#FFFFFF");

    private RectF _onOffButtonRect;
    private RectF _levelKnobRect;
    private readonly RectF[] _typeButtonRects = new RectF[4];
    private float _knobRadius;
    private bool _isDraggingLevel;
    private float _lastAngle;

    private static readonly string[] TypeLabels = { "R", "H", "P", "C" };
    private static readonly string[] TypeFullNames = { "ROOM", "HALL", "PLATE", "CHUR" };

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
        float buttonSize = 20f;
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
        
        // Type selector buttons after the knob
        float selectorStartX = knobX + _knobRadius + padding * 3;
        float availableWidth = dirtyRect.Right - selectorStartX - padding;
        float typeButtonWidth = Math.Min(availableWidth / 4 - 2, 28f);
        float typeButtonHeight = Math.Min(dirtyRect.Height - padding * 4, 24f);
        float selectorY = dirtyRect.Y + (dirtyRect.Height - typeButtonHeight) / 2;
        
        for (int i = 0; i < 4; i++)
        {
            float x = selectorStartX + i * (typeButtonWidth + 2);
            _typeButtonRects[i] = new RectF(x, selectorY, typeButtonWidth, typeButtonHeight);
            DrawTypeButton(canvas, _typeButtonRects[i], (ReverbType)i, isEnabled);
        }
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
        canvas.StrokeColor = isSelected && isEnabled ? KnobShadowColor : Color.FromArgb("#555577");
        canvas.StrokeSize = 1;
        canvas.DrawRoundedRectangle(rect, leftRadius, rightRadius, rightRadius, leftRadius);
        
        // Label
        canvas.FontSize = 10;
        canvas.FontColor = isSelected && isEnabled ? TypeButtonTextSelectedColor : 
                          (isEnabled ? TypeButtonTextColor : DisabledColor);
        canvas.DrawString(TypeLabels[index], rect, HorizontalAlignment.Center, VerticalAlignment.Center);
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

