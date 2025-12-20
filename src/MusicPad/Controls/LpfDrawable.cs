using Microsoft.Maui.Graphics;
using MusicPad.Core.Models;
using MusicPad.Core.Theme;

namespace MusicPad.Controls;

/// <summary>
/// Drawable for Low Pass Filter controls with on/off button, Cutoff and Resonance knobs.
/// </summary>
public class LpfDrawable
{
    private readonly LowPassFilterSettings _settings;
    
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

    private RectF _onOffButtonRect;
    private RectF _cutoffKnobRect;
    private RectF _resonanceKnobRect;
    private float _knobRadius;
    private bool _isDraggingCutoff;
    private bool _isDraggingResonance;
    private float _lastAngle;

    public event EventHandler? InvalidateRequested;

    public LpfDrawable(LowPassFilterSettings settings)
    {
        _settings = settings;
        _settings.CutoffChanged += (s, e) => InvalidateRequested?.Invoke(this, EventArgs.Empty);
        _settings.ResonanceChanged += (s, e) => InvalidateRequested?.Invoke(this, EventArgs.Empty);
        _settings.EnabledChanged += (s, e) => InvalidateRequested?.Invoke(this, EventArgs.Empty);
    }

    public LowPassFilterSettings Settings => _settings;

    /// <summary>
    /// Draws the LPF controls. Layout depends on isVertical flag.
    /// </summary>
    public void Draw(ICanvas canvas, RectF dirtyRect, bool isVertical = false)
    {
        float padding = 4f;
        float buttonSize = 28f;
        bool isEnabled = _settings.IsEnabled;
        
        if (isVertical)
        {
            // Vertical layout: button at top-left, CUT below, RES below CUT
            float availableHeight = dirtyRect.Height - buttonSize - padding * 3;
            float knobSize = Math.Min(dirtyRect.Width - padding * 2, availableHeight / 2 - 12);
            knobSize = Math.Max(knobSize, 25f);
            _knobRadius = knobSize * 0.4f;
            
            // On/Off button at top-left
            float buttonX = dirtyRect.X + padding;
            float buttonY = dirtyRect.Y + padding;
            _onOffButtonRect = new RectF(buttonX, buttonY, buttonSize, buttonSize);
            DrawOnOffButton(canvas, _onOffButtonRect);
            
            // CUT knob below button
            float knobX = dirtyRect.X + dirtyRect.Width / 2;
            float cutoffY = dirtyRect.Y + buttonSize + padding * 2 + _knobRadius + 4;
            
            _cutoffKnobRect = new RectF(knobX - _knobRadius - 5, cutoffY - _knobRadius - 5,
                                         _knobRadius * 2 + 10, _knobRadius * 2 + 10);
            DrawKnob(canvas, knobX, cutoffY, _knobRadius, _settings.Cutoff, "CUT", isEnabled);
            
            // RES knob below CUT
            float resonanceY = cutoffY + _knobRadius * 2 + 20;
            _resonanceKnobRect = new RectF(knobX - _knobRadius - 5, resonanceY - _knobRadius - 5,
                                            _knobRadius * 2 + 10, _knobRadius * 2 + 10);
            DrawKnob(canvas, knobX, resonanceY, _knobRadius, _settings.Resonance, "RES", isEnabled);
        }
        else
        {
            // Horizontal layout: button on left, knobs side by side - small knob size
            float knobSize = Math.Min(dirtyRect.Height - 12, 36f);
            _knobRadius = knobSize * 0.42f;
            
            // On/Off button on the left
            float buttonX = dirtyRect.X + padding;
            float buttonY = dirtyRect.Y + (dirtyRect.Height - buttonSize) / 2;
            _onOffButtonRect = new RectF(buttonX, buttonY, buttonSize, buttonSize);
            DrawOnOffButton(canvas, _onOffButtonRect);
            
            // Two knobs with even spacing after the button
            float knobsStartX = buttonX + buttonSize + padding * 3;
            float knobY = dirtyRect.Y + dirtyRect.Height / 2;
            float knobSpacing = _knobRadius * 2 + padding * 6; // More spacing between knobs
            
            float cutoffX = knobsStartX + _knobRadius + padding;
            float resonanceX = cutoffX + knobSpacing;
            
            _cutoffKnobRect = new RectF(cutoffX - _knobRadius - 5, knobY - _knobRadius - 5,
                                         _knobRadius * 2 + 10, _knobRadius * 2 + 10);
            _resonanceKnobRect = new RectF(resonanceX - _knobRadius - 5, knobY - _knobRadius - 5,
                                            _knobRadius * 2 + 10, _knobRadius * 2 + 10);

            DrawKnob(canvas, cutoffX, knobY, _knobRadius, _settings.Cutoff, "CUT", isEnabled);
            DrawKnob(canvas, resonanceX, knobY, _knobRadius, _settings.Resonance, "RES", isEnabled);
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
        
        // Toggle track
        canvas.FillColor = isOn ? ButtonOnColor : ButtonOffColor;
        canvas.FillRoundedRectangle(cx - toggleWidth / 2, cy - toggleHeight / 2, toggleWidth, toggleHeight, toggleHeight / 2);
        
        canvas.StrokeColor = isOn ? ButtonOnColor.WithAlpha(0.8f) : Color.FromArgb(AppColors.ButtonBorder);
        canvas.StrokeSize = 1;
        canvas.DrawRoundedRectangle(cx - toggleWidth / 2, cy - toggleHeight / 2, toggleWidth, toggleHeight, toggleHeight / 2);
        
        // Toggle knob position
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
        
        // Draw radial marker lines
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
        
        int markerCount = 6;
        for (int i = 0; i <= markerCount; i++)
        {
            float t = i / (float)markerCount;
            float angle = minAngle + totalAngle * t;
            float rads = angle * MathF.PI / 180f;
            
            float innerX = centerX + innerRadius * MathF.Cos(rads);
            float innerY = centerY - innerRadius * MathF.Sin(rads);
            float outerX = centerX + outerRadius * MathF.Cos(rads);
            float outerY = centerY - outerRadius * MathF.Sin(rads);
            
            canvas.DrawLine(innerX, innerY, outerX, outerY);
        }
        
        // Shadow/depth effect
        canvas.FillColor = shadowColor;
        canvas.FillCircle(centerX + 1, centerY + 1, radius);
        
        // Main knob body
        canvas.FillColor = baseColor;
        canvas.FillCircle(centerX, centerY, radius);
        
        // Subtle highlight
        canvas.FillColor = highlightColor.WithAlpha(0.4f);
        canvas.FillCircle(centerX - radius * 0.15f, centerY - radius * 0.15f, radius * 0.6f);
        
        // Inner area
        canvas.FillColor = baseColor;
        canvas.FillCircle(centerX, centerY, radius * 0.85f);
        
        // Edge ring
        canvas.StrokeColor = shadowColor.WithAlpha(0.5f);
        canvas.StrokeSize = 1;
        canvas.DrawCircle(centerX, centerY, radius);

        // Draw indicator
        float currentAngle = minAngle + totalAngle * value;
        float radians = currentAngle * MathF.PI / 180f;
        
        float notchDistance = radius * 0.7f;
        float notchX = centerX + notchDistance * MathF.Cos(radians);
        float notchY = centerY - notchDistance * MathF.Sin(radians);
        
        float notchRadius = radius * 0.12f;
        canvas.FillColor = isEnabled ? IndicatorColor : Color.FromArgb(AppColors.DisabledDarker);
        canvas.FillCircle(notchX, notchY, notchRadius);

        // Label below
        canvas.FontSize = 9;
        canvas.FontColor = isEnabled ? LabelColor : DisabledColor;
        canvas.DrawString(label, centerX - radius, centerY + radius + 4, radius * 2, 12,
            HorizontalAlignment.Center, VerticalAlignment.Top);
    }

    public bool OnTouch(float x, float y, bool isStart)
    {
        var point = new PointF(x, y);

        if (isStart)
        {
            // Check on/off button
            if (_onOffButtonRect.Contains(point))
            {
                _settings.IsEnabled = !_settings.IsEnabled;
                return true;
            }
            
            // Only allow knob interaction if enabled
            if (!_settings.IsEnabled)
                return false;
            
            if (_cutoffKnobRect.Contains(point))
            {
                _isDraggingCutoff = true;
                _lastAngle = GetAngleFromKnobCenter(_cutoffKnobRect, x, y);
                return true;
            }
            else if (_resonanceKnobRect.Contains(point))
            {
                _isDraggingResonance = true;
                _lastAngle = GetAngleFromKnobCenter(_resonanceKnobRect, x, y);
                return true;
            }
        }
        else
        {
            if (_isDraggingCutoff)
            {
                UpdateCutoffKnob(x, y);
                return true;
            }
            else if (_isDraggingResonance)
            {
                UpdateResonanceKnob(x, y);
                return true;
            }
        }

        return false;
    }

    private void UpdateCutoffKnob(float x, float y)
    {
        float currentAngle = GetAngleFromKnobCenter(_cutoffKnobRect, x, y);
        float angleDelta = currentAngle - _lastAngle;
        
        if (angleDelta > 180) angleDelta -= 360;
        if (angleDelta < -180) angleDelta += 360;
        
        float totalAngle = -45f - 225f;
        if (totalAngle > 0) totalAngle -= 360;
        
        float valueDelta = angleDelta / totalAngle;
        _settings.Cutoff = Math.Clamp(_settings.Cutoff + valueDelta, 0f, 1f);
        
        _lastAngle = currentAngle;
    }

    private void UpdateResonanceKnob(float x, float y)
    {
        float currentAngle = GetAngleFromKnobCenter(_resonanceKnobRect, x, y);
        float angleDelta = currentAngle - _lastAngle;
        
        if (angleDelta > 180) angleDelta -= 360;
        if (angleDelta < -180) angleDelta += 360;
        
        float totalAngle = -45f - 225f;
        if (totalAngle > 0) totalAngle -= 360;
        
        float valueDelta = angleDelta / totalAngle;
        _settings.Resonance = Math.Clamp(_settings.Resonance + valueDelta, 0f, 1f);
        
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
        _isDraggingCutoff = false;
        _isDraggingResonance = false;
    }
}
