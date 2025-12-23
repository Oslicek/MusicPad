using Microsoft.Maui.Graphics;
using MusicPad.Core.Drawing;
using MusicPad.Core.Theme;

namespace MusicPad.Controls;

/// <summary>
/// Shared rendering logic for on/off toggle buttons.
/// Used by Chorus, Delay, LPF, Reverb, and ArpHarmony drawables.
/// </summary>
public static class ToggleRenderer
{
    /// <summary>
    /// Draws an on/off toggle button.
    /// </summary>
    public static void Draw(ICanvas canvas, RectF rect, bool isOn, bool isAllowed = true)
    {
        float cx = rect.Center.X;
        float cy = rect.Center.Y;
        float toggleWidth = rect.Width * DrawableConstants.ToggleWidthRatio;
        float toggleHeight = rect.Height * DrawableConstants.ToggleHeightRatio;
        float knobRadius = toggleHeight * DrawableConstants.ToggleKnobRatio;
        
        var shadowColor = Color.FromArgb(AppColors.KnobShadow);
        
        if (!isAllowed)
        {
            DrawDisabled(canvas, cx, cy, toggleWidth, toggleHeight, knobRadius);
            return;
        }
        
        var trackColor = isOn ? Color.FromArgb(AppColors.ButtonOn) : Color.FromArgb(AppColors.ButtonOff);
        var borderColor = isOn ? trackColor.WithAlpha(0.8f) : Color.FromArgb(AppColors.ButtonBorder);
        
        // Toggle track
        canvas.FillColor = trackColor;
        canvas.FillRoundedRectangle(cx - toggleWidth / 2, cy - toggleHeight / 2, toggleWidth, toggleHeight, toggleHeight / 2);
        
        canvas.StrokeColor = borderColor;
        canvas.StrokeSize = 1;
        canvas.DrawRoundedRectangle(cx - toggleWidth / 2, cy - toggleHeight / 2, toggleWidth, toggleHeight, toggleHeight / 2);
        
        // Knob position: left = off, right = on
        float knobOffset = toggleWidth / 2 - toggleHeight / 2;
        float knobX = isOn ? cx + knobOffset : cx - knobOffset;
        
        // Knob shadow
        canvas.FillColor = shadowColor.WithAlpha(0.5f);
        canvas.FillCircle(knobX + 1, cy + 1, knobRadius);
        
        // Knob
        canvas.FillColor = Colors.White;
        canvas.FillCircle(knobX, cy, knobRadius);
    }

    /// <summary>
    /// Draws a disabled toggle button.
    /// </summary>
    private static void DrawDisabled(ICanvas canvas, float cx, float cy, 
        float toggleWidth, float toggleHeight, float knobRadius)
    {
        var disabledColor = Color.FromArgb(AppColors.Disabled);
        
        // Disabled track
        canvas.FillColor = disabledColor.WithAlpha(0.3f);
        canvas.FillRoundedRectangle(cx - toggleWidth / 2, cy - toggleHeight / 2, toggleWidth, toggleHeight, toggleHeight / 2);
        
        canvas.StrokeColor = Color.FromArgb(AppColors.DisabledBorder);
        canvas.StrokeSize = 1;
        canvas.DrawRoundedRectangle(cx - toggleWidth / 2, cy - toggleHeight / 2, toggleWidth, toggleHeight, toggleHeight / 2);
        
        // Knob at left (off) position
        float knobX = cx - toggleWidth / 2 + toggleHeight / 2;
        canvas.FillColor = disabledColor;
        canvas.FillCircle(knobX, cy, knobRadius);
    }
}


