using Microsoft.Maui.Graphics;
using MusicPad.Core.Models;

namespace MusicPad.Controls;

/// <summary>
/// Shared renderer for effect type icons.
/// Draws distinctive icons for each effect type in selector buttons.
/// </summary>
public static class EffectIconRenderer
{
    /// <summary>
    /// Draws the icon for the specified effect type.
    /// </summary>
    public static void Draw(ICanvas canvas, RectF iconRect, EffectType effect, Color iconColor)
    {
        switch (effect)
        {
            case EffectType.ArpHarmony:
                DrawArpHarmonyIcon(canvas, iconRect, iconColor);
                break;
            case EffectType.EQ:
                DrawEQIcon(canvas, iconRect, iconColor);
                break;
            case EffectType.Chorus:
                DrawChorusIcon(canvas, iconRect, iconColor);
                break;
            case EffectType.Delay:
                DrawDelayIcon(canvas, iconRect, iconColor);
                break;
            case EffectType.Reverb:
                DrawReverbIcon(canvas, iconRect, iconColor);
                break;
        }
    }

    /// <summary>
    /// Stacked notes with ascending arrow - represents chord + arpeggio.
    /// </summary>
    private static void DrawArpHarmonyIcon(ICanvas canvas, RectF rect, Color color)
    {
        canvas.StrokeColor = color;
        canvas.FillColor = color;
        canvas.StrokeSize = 2;
        canvas.StrokeLineCap = LineCap.Round;

        float noteSize = rect.Height * 0.2f;
        float spacing = rect.Height * 0.25f;
        
        // Draw 3 stacked note heads (ellipses)
        for (int i = 0; i < 3; i++)
        {
            float y = rect.Bottom - (i + 0.5f) * spacing;
            float x = rect.X + rect.Width * 0.3f;
            canvas.FillEllipse(x - noteSize * 0.6f, y - noteSize * 0.4f, noteSize * 1.2f, noteSize * 0.8f);
        }
        
        // Draw ascending arrow on the right
        float arrowX = rect.X + rect.Width * 0.7f;
        float arrowBottom = rect.Bottom - spacing * 0.3f;
        float arrowTop = rect.Top + spacing * 0.3f;
        
        canvas.DrawLine(arrowX, arrowBottom, arrowX, arrowTop);
        canvas.DrawLine(arrowX - noteSize * 0.5f, arrowTop + noteSize, arrowX, arrowTop);
        canvas.DrawLine(arrowX + noteSize * 0.5f, arrowTop + noteSize, arrowX, arrowTop);
    }

    /// <summary>
    /// 3 vertical bars at different heights (equalizer).
    /// </summary>
    private static void DrawEQIcon(ICanvas canvas, RectF rect, Color color)
    {
        canvas.StrokeColor = color;
        canvas.FillColor = color;
        canvas.StrokeSize = 3;
        canvas.StrokeLineCap = LineCap.Round;

        float barWidth = rect.Width / 5;
        float spacing = barWidth;
        
        // Low bar (short)
        float x1 = rect.X + spacing * 0.5f;
        canvas.DrawLine(x1, rect.Bottom, x1, rect.Bottom - rect.Height * 0.4f);
        
        // Mid bar (tall)
        float x2 = rect.Center.X;
        canvas.DrawLine(x2, rect.Bottom, x2, rect.Top);
        
        // High bar (medium)
        float x3 = rect.Right - spacing * 0.5f;
        canvas.DrawLine(x3, rect.Bottom, x3, rect.Bottom - rect.Height * 0.65f);
    }

    /// <summary>
    /// 2-3 overlapping waves (chorus effect).
    /// </summary>
    private static void DrawChorusIcon(ICanvas canvas, RectF rect, Color color)
    {
        canvas.StrokeColor = color;
        canvas.StrokeSize = 2;
        canvas.StrokeLineCap = LineCap.Round;

        float waveHeight = rect.Height * 0.3f;
        
        // Draw 3 overlapping sine-like waves
        for (int w = 0; w < 3; w++)
        {
            float yOffset = rect.Y + rect.Height * 0.25f + w * (rect.Height * 0.2f);
            var path = new PathF();
            
            for (int i = 0; i <= 20; i++)
            {
                float t = i / 20f;
                float x = rect.X + t * rect.Width;
                float y = yOffset + (float)Math.Sin(t * Math.PI * 2) * waveHeight * (1 - w * 0.2f);
                
                if (i == 0)
                    path.MoveTo(x, y);
                else
                    path.LineTo(x, y);
            }
            
            canvas.DrawPath(path);
        }
    }

    /// <summary>
    /// Repeating dots/circles getting smaller (echo effect).
    /// </summary>
    private static void DrawDelayIcon(ICanvas canvas, RectF rect, Color color)
    {
        canvas.FillColor = color;

        float maxRadius = rect.Height * 0.2f;
        int dots = 4;
        
        for (int i = 0; i < dots; i++)
        {
            float t = i / (float)(dots - 1);
            float x = rect.X + t * rect.Width * 0.8f + rect.Width * 0.1f;
            float y = rect.Center.Y;
            float radius = maxRadius * (1 - t * 0.6f);
            float alpha = 1 - t * 0.5f;
            
            canvas.FillColor = color.WithAlpha(alpha);
            canvas.FillCircle(x, y, radius);
        }
    }

    /// <summary>
    /// Expanding arcs (sound waves radiating).
    /// </summary>
    private static void DrawReverbIcon(ICanvas canvas, RectF rect, Color color)
    {
        canvas.StrokeColor = color;
        canvas.StrokeSize = 2;
        canvas.StrokeLineCap = LineCap.Round;

        float centerX = rect.X + rect.Width * 0.2f;
        float centerY = rect.Center.Y;

        // Draw 3 arcs expanding to the right
        for (int i = 0; i < 3; i++)
        {
            float radius = rect.Width * 0.25f + i * (rect.Width * 0.2f);
            float alpha = 1 - i * 0.25f;
            
            canvas.StrokeColor = color.WithAlpha(alpha);
            
            // Draw arc (partial circle)
            var path = new PathF();
            float startAngle = -45;
            float endAngle = 45;
            
            for (int a = (int)startAngle; a <= endAngle; a += 5)
            {
                float rad = a * (float)Math.PI / 180f;
                float x = centerX + (float)Math.Cos(rad) * radius;
                float y = centerY + (float)Math.Sin(rad) * radius;
                
                if (a == (int)startAngle)
                    path.MoveTo(x, y);
                else
                    path.LineTo(x, y);
            }
            
            canvas.DrawPath(path);
        }
    }
}

