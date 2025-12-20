using Microsoft.Maui.Graphics;
using MusicPad.Core.Models;
using MusicPad.Core.Theme;

namespace MusicPad.Controls;

/// <summary>
/// Drawable for Arpeggiator and Harmony controls in two rows.
/// Row 1: HARM [⏻] [O|5|M|m]
/// Row 2: ARP  [⏻] (○)RATE [▲|▼|↕|?]
/// </summary>
public class ArpHarmonyDrawable
{
    private readonly HarmonySettings _harmonySettings;
    private readonly ArpeggiatorSettings _arpSettings;
    
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
    private static Color TypeButtonTextSelectedColor => Color.FromArgb(AppColors.TextWhite);

    // Hit test rects
    private RectF _harmonyOnOffRect;
    private readonly RectF[] _harmonyTypeRects = new RectF[4];
    private RectF _arpOnOffRect;
    private RectF _arpRateKnobRect;
    private readonly RectF[] _arpPatternRects = new RectF[4];
    
    private float _rateKnobRadius;
    private bool _isDraggingRate;
    private float _lastAngle;

    // New order: MAJ, MIN, OCT, 5TH (as requested)
    private static readonly string[] HarmonyLabels = { "MAJ", "MIN", "OCT", "5TH" };
    private static readonly int[] HarmonyTypeMap = { 2, 3, 0, 1 }; // Maps button index to HarmonyType enum
    private static readonly string[] PatternLabels = { "UP", "DOWN", "U+D", "RAND" };

    public event EventHandler? InvalidateRequested;

    public ArpHarmonyDrawable(HarmonySettings harmonySettings, ArpeggiatorSettings arpSettings)
    {
        _harmonySettings = harmonySettings;
        _arpSettings = arpSettings;
        
        _harmonySettings.EnabledChanged += (s, e) => InvalidateRequested?.Invoke(this, EventArgs.Empty);
        _harmonySettings.TypeChanged += (s, e) => InvalidateRequested?.Invoke(this, EventArgs.Empty);
        _harmonySettings.AllowedChanged += (s, e) => InvalidateRequested?.Invoke(this, EventArgs.Empty);
        _arpSettings.EnabledChanged += (s, e) => InvalidateRequested?.Invoke(this, EventArgs.Empty);
        _arpSettings.RateChanged += (s, e) => InvalidateRequested?.Invoke(this, EventArgs.Empty);
        _arpSettings.PatternChanged += (s, e) => InvalidateRequested?.Invoke(this, EventArgs.Empty);
    }

    public HarmonySettings HarmonySettings => _harmonySettings;
    public ArpeggiatorSettings ArpSettings => _arpSettings;

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        float padding = 8f;  // Uniform spacing
        float buttonSize = 28f;
        float titleHeight = 16f;
        float controlsHeight = (dirtyRect.Height - titleHeight * 2 - padding * 2) / 2;
        
        // 4-row layout:
        // Row 1: "HARMONY" title
        // Row 2: Harmony controls
        // Row 3: "ARPEGGIO" title
        // Row 4: Arpeggio controls
        
        float y = dirtyRect.Y;
        
        // Row 1: HARMONY title
        DrawSectionTitle(canvas, "HARMONY", dirtyRect.X, y, dirtyRect.Width, titleHeight);
        y += titleHeight;
        
        // Row 2: Harmony controls
        DrawHarmonyRow(canvas, new RectF(dirtyRect.X, y, dirtyRect.Width, controlsHeight), padding, buttonSize);
        y += controlsHeight + padding;
        
        // Row 3: ARPEGGIO title
        DrawSectionTitle(canvas, "ARPEGGIO", dirtyRect.X, y, dirtyRect.Width, titleHeight);
        y += titleHeight;
        
        // Row 4: Arpeggio controls
        DrawArpRow(canvas, new RectF(dirtyRect.X, y, dirtyRect.Width, controlsHeight), padding, buttonSize);
    }
    
    private void DrawSectionTitle(ICanvas canvas, string title, float x, float y, float width, float height)
    {
        canvas.FontSize = 10;
        canvas.FontColor = Color.FromArgb(AppColors.TextSecondary);
        canvas.DrawString(title, x, y, width, height, HorizontalAlignment.Center, VerticalAlignment.Center);
    }

    private void DrawHarmonyRow(ICanvas canvas, RectF rowRect, float padding, float buttonSize)
    {
        bool isEnabled = _harmonySettings.IsEnabled;
        bool isAllowed = _harmonySettings.IsAllowed;
        bool effectiveEnabled = isEnabled && isAllowed;
        
        float centerY = rowRect.Y + rowRect.Height / 2 - 5; // Shift up to make room for labels
        
        // Calculate layout - toggle + 4 circular buttons, left-aligned like other effects
        float circleButtonSize = 24f;
        float startX = rowRect.X + padding;
        float x = startX;
        
        // On/Off toggle
        _harmonyOnOffRect = new RectF(x, centerY - buttonSize / 2, buttonSize, buttonSize);
        DrawOnOffButton(canvas, _harmonyOnOffRect, isEnabled, isAllowed);
        x += buttonSize + padding * 3;
        
        // Type buttons (circular with labels below) - MAJ, MIN, OCT, 5TH
        for (int i = 0; i < 4; i++)
        {
            float bx = x + i * (circleButtonSize + padding + 8);
            _harmonyTypeRects[i] = new RectF(bx, centerY - circleButtonSize / 2, circleButtonSize, circleButtonSize);
            
            bool isSelected = HarmonyTypeMap[i] == (int)_harmonySettings.Type;
            DrawCircleButtonWithLabel(canvas, _harmonyTypeRects[i], HarmonyLabels[i], isSelected, effectiveEnabled);
        }
    }

    private void DrawArpRow(ICanvas canvas, RectF rowRect, float padding, float buttonSize)
    {
        bool isEnabled = _arpSettings.IsEnabled;
        float centerY = rowRect.Y + rowRect.Height / 2 - 5; // Shift up for labels
        
        float circleButtonSize = 24f;
        float knobSize = 49f;  // Small knob size - same as LPF (+15% bigger)
        _rateKnobRadius = knobSize * 0.42f;
        
        // Calculate layout - toggle + knob + 4 circular buttons, left-aligned like other effects
        float startX = rowRect.X + padding;
        float x = startX;
        
        // On/Off toggle
        _arpOnOffRect = new RectF(x, centerY - buttonSize / 2, buttonSize, buttonSize);
        DrawOnOffButton(canvas, _arpOnOffRect, isEnabled);
        x += buttonSize + padding * 3;
        
        // Rate knob
        float knobCenterX = x + _rateKnobRadius + 5;
        _arpRateKnobRect = new RectF(knobCenterX - _rateKnobRadius - 5, centerY - _rateKnobRadius - 5,
                                      _rateKnobRadius * 2 + 10, _rateKnobRadius * 2 + 10);
        DrawKnob(canvas, knobCenterX, centerY, _rateKnobRadius, _arpSettings.Rate, "RATE", isEnabled);
        x += knobSize + padding * 4;
        
        // Pattern buttons (circular with labels below) - UP, DOWN, U+D, RAND
        for (int i = 0; i < 4; i++)
        {
            float bx = x + i * (circleButtonSize + padding + 8);
            _arpPatternRects[i] = new RectF(bx, centerY - circleButtonSize / 2, circleButtonSize, circleButtonSize);
            
            bool isSelected = (int)_arpSettings.Pattern == i;
            DrawCircleButtonWithLabel(canvas, _arpPatternRects[i], PatternLabels[i], isSelected, isEnabled);
        }
    }
    
    private void DrawCircleButtonWithLabel(ICanvas canvas, RectF rect, string label, bool isSelected, bool isEnabled)
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

    private void DrawOnOffButton(ICanvas canvas, RectF rect, bool isOn, bool isAllowed = true)
    {
        float cx = rect.Center.X;
        float cy = rect.Center.Y;
        float toggleWidth = rect.Width * 0.85f;
        float toggleHeight = rect.Height * 0.5f;
        float knobRadius = toggleHeight * 0.4f;
        
        // When not allowed, show as disabled
        if (!isAllowed)
        {
            // Toggle track (disabled)
            canvas.FillColor = DisabledColor.WithAlpha(0.3f);
            canvas.FillRoundedRectangle(cx - toggleWidth / 2, cy - toggleHeight / 2, toggleWidth, toggleHeight, toggleHeight / 2);
            
            canvas.StrokeColor = Color.FromArgb(AppColors.DisabledBorder);
            canvas.StrokeSize = 1;
            canvas.DrawRoundedRectangle(cx - toggleWidth / 2, cy - toggleHeight / 2, toggleWidth, toggleHeight, toggleHeight / 2);
            
            // Toggle knob (left position = off, disabled)
            float knobX = cx - toggleWidth / 2 + toggleHeight / 2;
            canvas.FillColor = DisabledColor;
            canvas.FillCircle(knobX, cy, knobRadius);
            return;
        }
        
        // Toggle track
        canvas.FillColor = isOn ? ButtonOnColor : ButtonOffColor;
        canvas.FillRoundedRectangle(cx - toggleWidth / 2, cy - toggleHeight / 2, toggleWidth, toggleHeight, toggleHeight / 2);
        
        canvas.StrokeColor = isOn ? ButtonOnColor.WithAlpha(0.8f) : Color.FromArgb(AppColors.ButtonBorder);
        canvas.StrokeSize = 1;
        canvas.DrawRoundedRectangle(cx - toggleWidth / 2, cy - toggleHeight / 2, toggleWidth, toggleHeight, toggleHeight / 2);
        
        // Toggle knob position: left = off, right = on
        float knobOffset = toggleWidth / 2 - toggleHeight / 2;
        float knobX2 = isOn ? cx + knobOffset : cx - knobOffset;
        
        // Knob shadow
        canvas.FillColor = KnobShadowColor.WithAlpha(0.5f);
        canvas.FillCircle(knobX2 + 1, cy + 1, knobRadius);
        
        // Knob
        canvas.FillColor = Colors.White;
        canvas.FillCircle(knobX2, cy, knobRadius);
    }

    private void DrawTypeButton(ICanvas canvas, RectF rect, string label, bool isSelected, bool isEnabled, bool isFirst, bool isLast)
    {
        float leftRadius = isFirst ? 6 : 0;
        float rightRadius = isLast ? 6 : 0;
        
        if (isSelected && isEnabled)
        {
            canvas.FillColor = KnobShadowColor;
            canvas.FillRoundedRectangle(new RectF(rect.X + 1, rect.Y + 1, rect.Width, rect.Height),
                leftRadius, rightRadius, rightRadius, leftRadius);
            
            canvas.FillColor = TypeButtonSelectedColor;
            canvas.FillRoundedRectangle(rect, leftRadius, rightRadius, rightRadius, leftRadius);
            
            canvas.FillColor = KnobHighlightColor.WithAlpha(0.3f);
            var highlightRect = new RectF(rect.X + 2, rect.Y + 2, rect.Width - 4, rect.Height * 0.4f);
            canvas.FillRoundedRectangle(highlightRect, Math.Max(0, leftRadius - 2), Math.Max(0, rightRadius - 2), 0, 0);
        }
        else
        {
            canvas.FillColor = isEnabled ? TypeButtonBaseColor : DisabledColor.WithAlpha(0.5f);
            canvas.FillRoundedRectangle(rect, leftRadius, rightRadius, rightRadius, leftRadius);
        }
        
        canvas.StrokeColor = isSelected && isEnabled ? KnobShadowColor : Color.FromArgb(AppColors.DisabledBorder);
        canvas.StrokeSize = 1;
        canvas.DrawRoundedRectangle(rect, leftRadius, rightRadius, rightRadius, leftRadius);
        
        canvas.FontSize = 11;
        canvas.FontColor = isSelected && isEnabled ? TypeButtonTextSelectedColor :
                          (isEnabled ? TypeButtonTextColor : DisabledColor);
        canvas.DrawString(label, rect, HorizontalAlignment.Center, VerticalAlignment.Center);
    }

    private void DrawKnob(ICanvas canvas, float centerX, float centerY, float radius, float value, string label, bool isEnabled)
    {
        Color baseColor = isEnabled ? KnobBaseColor : DisabledColor;
        Color highlightColor = isEnabled ? KnobHighlightColor : DisabledColor.WithAlpha(0.6f);
        Color shadowColor = isEnabled ? KnobShadowColor : Color.FromArgb(AppColors.DisabledDark);
        
        // Draw radial marker lines around the knob
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

        canvas.FontSize = 7;
        canvas.FontColor = isEnabled ? LabelColor : DisabledColor;
        canvas.DrawString(label, centerX - radius, centerY + radius + 4, radius * 2, 10,
            HorizontalAlignment.Center, VerticalAlignment.Top);
    }

    public bool OnTouch(float x, float y, bool isStart)
    {
        var point = new PointF(x, y);

        if (isStart)
        {
            // Harmony controls - only respond if allowed (not monophonic instrument)
            if (_harmonySettings.IsAllowed)
            {
                if (_harmonyOnOffRect.Contains(point))
                {
                    _harmonySettings.IsEnabled = !_harmonySettings.IsEnabled;
                    return true;
                }
                
                for (int i = 0; i < 4; i++)
                {
                    if (_harmonyTypeRects[i].Contains(point) && _harmonySettings.IsEnabled)
                    {
                        _harmonySettings.Type = (HarmonyType)HarmonyTypeMap[i];
                        return true;
                    }
                }
            }
            
            // Arpeggiator controls
            if (_arpOnOffRect.Contains(point))
            {
                _arpSettings.IsEnabled = !_arpSettings.IsEnabled;
                return true;
            }
            
            if (_arpRateKnobRect.Contains(point) && _arpSettings.IsEnabled)
            {
                _isDraggingRate = true;
                _lastAngle = GetAngleFromKnobCenter(_arpRateKnobRect, x, y);
                return true;
            }
            
            for (int i = 0; i < 4; i++)
            {
                if (_arpPatternRects[i].Contains(point) && _arpSettings.IsEnabled)
                {
                    _arpSettings.Pattern = (ArpPattern)i;
                    return true;
                }
            }
        }
        else
        {
            if (_isDraggingRate)
            {
                UpdateRateKnob(x, y);
                return true;
            }
        }

        return false;
    }

    private void UpdateRateKnob(float x, float y)
    {
        float currentAngle = GetAngleFromKnobCenter(_arpRateKnobRect, x, y);
        float angleDelta = currentAngle - _lastAngle;
        
        if (angleDelta > 180) angleDelta -= 360;
        if (angleDelta < -180) angleDelta += 360;
        
        float totalAngle = -45f - 225f;
        if (totalAngle > 0) totalAngle -= 360;
        
        float valueDelta = angleDelta / totalAngle;
        _arpSettings.Rate = Math.Clamp(_arpSettings.Rate + valueDelta, 0f, 1f);
        
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
        _isDraggingRate = false;
    }
}

