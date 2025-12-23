using Microsoft.Maui.Graphics;
using MusicPad.Core.Drawing;
using MusicPad.Core.Theme;

namespace MusicPad.Controls;

/// <summary>
/// Shared rendering logic for rotary knobs.
/// Used by Chorus, Delay, LPF, Reverb, and ArpHarmony drawables.
/// </summary>
public static class KnobRenderer
{
    /// <summary>
    /// Draws a rotary knob with markers, indicator, and label.
    /// </summary>
    public static void Draw(ICanvas canvas, float centerX, float centerY, float radius, 
        float value, string label, bool isEnabled, float fontSize = DrawableConstants.FontSizeLarge)
    {
        var baseColor = isEnabled ? Color.FromArgb(AppColors.KnobBase) : Color.FromArgb(AppColors.Disabled);
        var highlightColor = isEnabled ? Color.FromArgb(AppColors.KnobHighlight) : Color.FromArgb(AppColors.Disabled).WithAlpha(0.6f);
        var shadowColor = isEnabled ? Color.FromArgb(AppColors.KnobShadow) : Color.FromArgb(AppColors.DisabledDark);
        var indicatorColor = isEnabled ? Color.FromArgb(AppColors.KnobIndicator) : Color.FromArgb(AppColors.DisabledDarker);
        var labelColor = isEnabled ? Color.FromArgb(AppColors.TextSecondary) : Color.FromArgb(AppColors.Disabled);
        
        float totalAngle = DrawableConstants.GetTotalKnobAngle();
        
        // Draw markers around the knob
        DrawMarkers(canvas, centerX, centerY, radius, isEnabled, totalAngle);
        
        // Shadow for depth
        canvas.FillColor = shadowColor;
        canvas.FillCircle(centerX + 1, centerY + 1, radius);
        
        // Main knob body
        canvas.FillColor = baseColor;
        canvas.FillCircle(centerX, centerY, radius);
        
        // Highlight
        canvas.FillColor = highlightColor.WithAlpha(0.4f);
        canvas.FillCircle(
            centerX - radius * DrawableConstants.KnobHighlightOffset, 
            centerY - radius * DrawableConstants.KnobHighlightOffset, 
            radius * DrawableConstants.KnobHighlightRadius);
        
        // Inner area
        canvas.FillColor = baseColor;
        canvas.FillCircle(centerX, centerY, radius * DrawableConstants.KnobInnerRadius);
        
        // Edge ring
        canvas.StrokeColor = shadowColor.WithAlpha(0.5f);
        canvas.StrokeSize = 1;
        canvas.DrawCircle(centerX, centerY, radius);
        
        // Indicator notch
        float currentAngle = DrawableConstants.KnobMinAngle + totalAngle * value;
        float radians = currentAngle * MathF.PI / 180f;
        float notchDistance = radius * DrawableConstants.KnobIndicatorDistance;
        float notchX = centerX + notchDistance * MathF.Cos(radians);
        float notchY = centerY - notchDistance * MathF.Sin(radians);
        float notchRadius = radius * DrawableConstants.KnobIndicatorRadius;
        
        canvas.FillColor = indicatorColor;
        canvas.FillCircle(notchX, notchY, notchRadius);
        
        // Label below
        canvas.FontSize = fontSize;
        canvas.FontColor = labelColor;
        canvas.DrawString(label, centerX - radius, centerY + radius + DrawableConstants.LabelOffsetY, 
            radius * 2, DrawableConstants.LabelHeight, HorizontalAlignment.Center, VerticalAlignment.Top);
    }

    /// <summary>
    /// Draws radial marker lines around the knob.
    /// </summary>
    private static void DrawMarkers(ICanvas canvas, float centerX, float centerY, 
        float radius, bool isEnabled, float totalAngle)
    {
        var accentColor = isEnabled ? Color.FromArgb(AppColors.Accent) : Color.FromArgb(AppColors.Disabled);
        
        canvas.StrokeColor = accentColor;
        canvas.StrokeSize = DrawableConstants.KnobMarkerStrokeWidth;
        canvas.StrokeLineCap = LineCap.Round;
        
        float outerRadius = radius + DrawableConstants.KnobMarkerOuterOffset;
        float innerRadius = radius + DrawableConstants.KnobMarkerInnerOffset;
        
        for (int i = 0; i <= DrawableConstants.KnobMarkerCount; i++)
        {
            float t = i / (float)DrawableConstants.KnobMarkerCount;
            float angle = DrawableConstants.KnobMinAngle + totalAngle * t;
            float rads = angle * MathF.PI / 180f;
            
            canvas.DrawLine(
                centerX + innerRadius * MathF.Cos(rads), 
                centerY - innerRadius * MathF.Sin(rads),
                centerX + outerRadius * MathF.Cos(rads), 
                centerY - outerRadius * MathF.Sin(rads));
        }
    }

    /// <summary>
    /// Calculates angle from knob center to touch point (for rotation tracking).
    /// </summary>
    public static float GetAngleFromCenter(RectF knobRect, float x, float y)
    {
        float centerX = knobRect.Center.X;
        float centerY = knobRect.Center.Y;
        float dx = x - centerX;
        float dy = centerY - y;
        return MathF.Atan2(dy, dx) * 180f / MathF.PI;
    }

    /// <summary>
    /// Updates knob value based on rotation delta.
    /// </summary>
    public static float UpdateValueFromAngle(float lastAngle, float currentAngle, float currentValue)
    {
        float angleDelta = currentAngle - lastAngle;
        
        // Normalize angle delta to handle wrap-around
        if (angleDelta > 180) angleDelta -= 360;
        if (angleDelta < -180) angleDelta += 360;
        
        float totalAngle = DrawableConstants.GetTotalKnobAngle();
        float valueDelta = angleDelta / totalAngle;
        
        return Math.Clamp(currentValue + valueDelta, 0f, 1f);
    }
}


