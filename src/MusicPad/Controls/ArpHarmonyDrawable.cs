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

    private static readonly string[] HarmonyLabels = { "O", "5", "M", "m" }; // Oct, 5th, Maj, Min
    private static readonly string[] PatternLabels = { "▲", "▼", "↕", "?" }; // Up, Down, UpDown, Random

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
        float padding = 4f;
        float buttonSize = 20f;
        float rowHeight = (dirtyRect.Height - padding) / 2;
        
        // Row 1: Harmony
        float row1Y = dirtyRect.Y;
        DrawHarmonyRow(canvas, new RectF(dirtyRect.X, row1Y, dirtyRect.Width, rowHeight), padding, buttonSize);
        
        // Row 2: Arpeggiator
        float row2Y = dirtyRect.Y + rowHeight + padding;
        DrawArpRow(canvas, new RectF(dirtyRect.X, row2Y, dirtyRect.Width, rowHeight), padding, buttonSize);
    }

    private void DrawHarmonyRow(ICanvas canvas, RectF rowRect, float padding, float buttonSize)
    {
        bool isEnabled = _harmonySettings.IsEnabled;
        bool isAllowed = _harmonySettings.IsAllowed;
        float x = rowRect.X + padding;
        float centerY = rowRect.Y + rowRect.Height / 2;
        
        // Label - dimmed when not allowed
        canvas.FontSize = 9;
        canvas.FontColor = isAllowed ? LabelColor : DisabledColor;
        canvas.DrawString("HARM", x, centerY - 6, 30, 12, HorizontalAlignment.Left, VerticalAlignment.Center);
        x += 32;
        
        // On/Off button - disabled when not allowed
        _harmonyOnOffRect = new RectF(x, centerY - buttonSize / 2, buttonSize, buttonSize);
        DrawOnOffButton(canvas, _harmonyOnOffRect, isEnabled, isAllowed);
        x += buttonSize + padding * 2;
        
        // Type selector (4 buttons) - all disabled when not allowed
        float typeButtonWidth = 22f;
        float typeButtonHeight = 20f;
        
        // When not allowed, treat as if disabled
        bool effectiveEnabled = isEnabled && isAllowed;
        
        for (int i = 0; i < 4; i++)
        {
            float bx = x + i * (typeButtonWidth + 2);
            _harmonyTypeRects[i] = new RectF(bx, centerY - typeButtonHeight / 2, typeButtonWidth, typeButtonHeight);
            DrawTypeButton(canvas, _harmonyTypeRects[i], HarmonyLabels[i], (int)_harmonySettings.Type == i, effectiveEnabled, i == 0, i == 3);
        }
    }

    private void DrawArpRow(ICanvas canvas, RectF rowRect, float padding, float buttonSize)
    {
        bool isEnabled = _arpSettings.IsEnabled;
        float x = rowRect.X + padding;
        float centerY = rowRect.Y + rowRect.Height / 2;
        
        // Label
        canvas.FontSize = 9;
        canvas.FontColor = LabelColor;
        canvas.DrawString("ARP", x, centerY - 6, 30, 12, HorizontalAlignment.Left, VerticalAlignment.Center);
        x += 32;
        
        // On/Off button
        _arpOnOffRect = new RectF(x, centerY - buttonSize / 2, buttonSize, buttonSize);
        DrawOnOffButton(canvas, _arpOnOffRect, isEnabled);
        x += buttonSize + padding * 2;
        
        // Rate knob
        float knobSize = Math.Min(rowRect.Height - 8, 36f);
        _rateKnobRadius = knobSize * 0.4f;
        float knobX = x + _rateKnobRadius;
        
        _arpRateKnobRect = new RectF(knobX - _rateKnobRadius - 5, centerY - _rateKnobRadius - 5,
                                      _rateKnobRadius * 2 + 10, _rateKnobRadius * 2 + 10);
        DrawKnob(canvas, knobX, centerY, _rateKnobRadius, _arpSettings.Rate, "RATE", isEnabled);
        x += _rateKnobRadius * 2 + padding * 3;
        
        // Pattern selector (4 buttons)
        float patternButtonWidth = 22f;
        float patternButtonHeight = 20f;
        
        for (int i = 0; i < 4; i++)
        {
            float bx = x + i * (patternButtonWidth + 2);
            _arpPatternRects[i] = new RectF(bx, centerY - patternButtonHeight / 2, patternButtonWidth, patternButtonHeight);
            DrawTypeButton(canvas, _arpPatternRects[i], PatternLabels[i], (int)_arpSettings.Pattern == i, isEnabled, i == 0, i == 3);
        }
    }

    private void DrawOnOffButton(ICanvas canvas, RectF rect, bool isOn, bool isAllowed = true)
    {
        // When not allowed, show as disabled
        if (!isAllowed)
        {
            canvas.FillColor = DisabledColor.WithAlpha(0.3f);
            canvas.FillRoundedRectangle(rect, 4);
            
            canvas.StrokeColor = Color.FromArgb(AppColors.DisabledBorder);
            canvas.StrokeSize = 1;
            canvas.DrawRoundedRectangle(rect, 4);
            
            float iconSize = rect.Width * 0.5f;
            float centerX = rect.Center.X;
            float centerY = rect.Center.Y;
            
            canvas.StrokeColor = DisabledColor;
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
            return;
        }
        
        canvas.FillColor = isOn ? ButtonOnColor : ButtonOffColor;
        canvas.FillRoundedRectangle(rect, 4);
        
        canvas.StrokeColor = isOn ? ButtonOnColor.WithAlpha(0.8f) : Color.FromArgb(AppColors.ButtonBorder);
        canvas.StrokeSize = 1;
        canvas.DrawRoundedRectangle(rect, 4);
        
        float iconSz = rect.Width * 0.5f;
        float cx = rect.Center.X;
        float cy = rect.Center.Y;
        
        canvas.StrokeColor = Colors.White;
        canvas.StrokeSize = 2;
        
        var iconPath = new PathF();
        for (int a = 60; a <= 300; a += 10)
        {
            float rad = a * MathF.PI / 180f;
            float x = cx + MathF.Cos(rad) * iconSz * 0.4f;
            float y = cy + MathF.Sin(rad) * iconSz * 0.4f;
            if (a == 60)
                iconPath.MoveTo(x, y);
            else
                iconPath.LineTo(x, y);
        }
        canvas.DrawPath(iconPath);
        
        canvas.DrawLine(cx, cy - iconSz * 0.5f, cx, cy);
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

        canvas.FontSize = 7;
        canvas.FontColor = isEnabled ? LabelColor : DisabledColor;
        canvas.DrawString(label, centerX - radius, centerY + radius + 1, radius * 2, 10,
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
                        _harmonySettings.Type = (HarmonyType)i;
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

