using Microsoft.Maui.Graphics;
using MusicPad.Core.Layout;
using MusicPad.Core.Models;
using MusicPad.Core.Theme;

// Type aliases to resolve ambiguity with MusicPad.Core.Layout types
using MauiRectF = Microsoft.Maui.Graphics.RectF;
using MauiPointF = Microsoft.Maui.Graphics.PointF;
using LayoutRectF = MusicPad.Core.Layout.RectF;

namespace MusicPad.Controls;

/// <summary>
/// Drawable for Arpeggiator and Harmony controls in two rows.
/// Row 1: HARM [⏻] [MAJ|MIN|OCT|5TH]
/// Row 2: ARP  [⏻] [UP|DOWN|U+D|RAND] (○)RATE
/// </summary>
public class ArpHarmonyDrawable
{
    private readonly HarmonySettings _harmonySettings;
    private readonly ArpeggiatorSettings _arpSettings;
    private readonly ArpHarmonyLayoutDefinition _layoutDefinition = ArpHarmonyLayoutDefinition.Instance;
    
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

    // Layout constants
    private const float TitleHeight = 16f;
    private const float Padding = 8f;

    // Hit test rects
    private MauiRectF _harmonyOnOffRect;
    private readonly MauiRectF[] _harmonyTypeRects = new MauiRectF[4];
    private MauiRectF _arpOnOffRect;
    private MauiRectF _arpRateKnobRect;
    private readonly MauiRectF[] _arpPatternRects = new MauiRectF[4];
    
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

    public void Draw(ICanvas canvas, MauiRectF dirtyRect)
    {
        // Calculate layout using the definition
        var bounds = new LayoutRectF(dirtyRect.X, dirtyRect.Y, dirtyRect.Width, dirtyRect.Height);
        var context = LayoutContext.FromBounds(bounds);
        var layout = _layoutDefinition.Calculate(bounds, context);

        // Get knob radius for drawing
        _rateKnobRadius = ArpHarmonyLayoutDefinition.GetKnobRadius();

        // Store hit test rects from layout
        StoreHitRects(layout);

        // Calculate row positions for titles
        float controlsHeight = (dirtyRect.Height - TitleHeight * 2 - Padding * 2) / 2;
        float harmonyRowY = dirtyRect.Y + TitleHeight;
        float arpTitleY = harmonyRowY + controlsHeight + Padding;
        
        // Row 1: HARMONY title
        DrawSectionTitle(canvas, "HARMONY", dirtyRect.X, dirtyRect.Y, dirtyRect.Width, TitleHeight);
        
        // Row 2: Harmony controls
        DrawHarmonyControls(canvas, layout);
        
        // Row 3: ARPEGGIO title
        DrawSectionTitle(canvas, "ARPEGGIO", dirtyRect.X, arpTitleY, dirtyRect.Width, TitleHeight);
        
        // Row 4: Arpeggio controls
        DrawArpControls(canvas, layout);
    }

    private void StoreHitRects(LayoutResult layout)
    {
        _harmonyOnOffRect = ToMauiRect(layout[ArpHarmonyLayoutDefinition.HarmonyOnOff]);
        _harmonyTypeRects[0] = ToMauiRect(layout[ArpHarmonyLayoutDefinition.HarmonyType0]);
        _harmonyTypeRects[1] = ToMauiRect(layout[ArpHarmonyLayoutDefinition.HarmonyType1]);
        _harmonyTypeRects[2] = ToMauiRect(layout[ArpHarmonyLayoutDefinition.HarmonyType2]);
        _harmonyTypeRects[3] = ToMauiRect(layout[ArpHarmonyLayoutDefinition.HarmonyType3]);
        
        _arpOnOffRect = ToMauiRect(layout[ArpHarmonyLayoutDefinition.ArpOnOff]);
        _arpPatternRects[0] = ToMauiRect(layout[ArpHarmonyLayoutDefinition.ArpPattern0]);
        _arpPatternRects[1] = ToMauiRect(layout[ArpHarmonyLayoutDefinition.ArpPattern1]);
        _arpPatternRects[2] = ToMauiRect(layout[ArpHarmonyLayoutDefinition.ArpPattern2]);
        _arpPatternRects[3] = ToMauiRect(layout[ArpHarmonyLayoutDefinition.ArpPattern3]);
        _arpRateKnobRect = ToMauiRect(layout[ArpHarmonyLayoutDefinition.ArpRateKnob]);
    }

    private static MauiRectF ToMauiRect(LayoutRectF r) => new(r.X, r.Y, r.Width, r.Height);

    private void DrawSectionTitle(ICanvas canvas, string title, float x, float y, float width, float height)
    {
        canvas.FontSize = 10;
        canvas.FontColor = Color.FromArgb(AppColors.TextSecondary);
        canvas.DrawString(title, x, y, width, height, HorizontalAlignment.Center, VerticalAlignment.Center);
    }

    private void DrawHarmonyControls(ICanvas canvas, LayoutResult layout)
    {
        bool isEnabled = _harmonySettings.IsEnabled;
        bool isAllowed = _harmonySettings.IsAllowed;
        bool effectiveEnabled = isEnabled && isAllowed;

        // On/Off toggle
        DrawOnOffButton(canvas, _harmonyOnOffRect, isEnabled, isAllowed);

        // Type buttons
        for (int i = 0; i < 4; i++)
        {
            bool isSelected = HarmonyTypeMap[i] == (int)_harmonySettings.Type;
            DrawCircleButtonWithLabel(canvas, _harmonyTypeRects[i], HarmonyLabels[i], isSelected, effectiveEnabled);
        }
    }

    private void DrawArpControls(ICanvas canvas, LayoutResult layout)
    {
        bool isEnabled = _arpSettings.IsEnabled;

        // On/Off toggle
        DrawOnOffButton(canvas, _arpOnOffRect, isEnabled);

        // Pattern buttons
        for (int i = 0; i < 4; i++)
        {
            bool isSelected = (int)_arpSettings.Pattern == i;
            DrawCircleButtonWithLabel(canvas, _arpPatternRects[i], PatternLabels[i], isSelected, isEnabled);
        }

        // Rate knob
        var knobRect = _arpRateKnobRect;
        float knobCenterX = knobRect.Center.X;
        float knobCenterY = knobRect.Center.Y;
        DrawKnob(canvas, knobCenterX, knobCenterY, _rateKnobRadius, _arpSettings.Rate, "RATE", isEnabled);
    }
    
    private void DrawCircleButtonWithLabel(ICanvas canvas, MauiRectF rect, string label, bool isSelected, bool isEnabled)
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

    private void DrawOnOffButton(ICanvas canvas, MauiRectF rect, bool isOn, bool isAllowed = true)
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
        var point = new MauiPointF(x, y);

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
        _isDraggingRate = false;
    }
}
