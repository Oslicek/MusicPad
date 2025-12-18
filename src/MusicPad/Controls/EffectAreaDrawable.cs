using Microsoft.Maui.Graphics;
using MusicPad.Core.Models;

namespace MusicPad.Controls;

/// <summary>
/// Drawable for the effect area with selection buttons and effect controls.
/// </summary>
public class EffectAreaDrawable : IDrawable
{
    private readonly EffectSelector _selector = new();
    private readonly List<RectF> _buttonRects = new();
    private bool _isHorizontal;

    // Colors
    private static readonly Color ButtonBackgroundColor = Color.FromArgb("#2a2a4e");
    private static readonly Color ButtonSelectedColor = Color.FromArgb("#4a6a9e");
    private static readonly Color ButtonIconColor = Color.FromArgb("#AAAAAA");
    private static readonly Color ButtonIconSelectedColor = Color.FromArgb("#FFFFFF");
    private static readonly Color EffectAreaBackground = Color.FromArgb("#1e1e3a");

    public event EventHandler<EffectType>? EffectSelected;

    /// <summary>
    /// Gets the effect selector for external access.
    /// </summary>
    public EffectSelector Selector => _selector;

    /// <summary>
    /// Sets whether buttons should be arranged horizontally (landscape) or vertically (portrait).
    /// </summary>
    public void SetOrientation(bool isHorizontal)
    {
        _isHorizontal = isHorizontal;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        _buttonRects.Clear();

        // Draw effect area background
        canvas.FillColor = EffectAreaBackground;
        canvas.FillRectangle(dirtyRect);

        // Calculate button layout
        float buttonSize = 50f;
        float buttonSpacing = 8f;
        float buttonMargin = 8f;

        var effects = EffectSelector.AllEffects;

        if (_isHorizontal)
        {
            // Horizontal layout - buttons at top
            float totalWidth = effects.Count * buttonSize + (effects.Count - 1) * buttonSpacing;
            float startX = dirtyRect.X + buttonMargin;
            float startY = dirtyRect.Y + buttonMargin;

            for (int i = 0; i < effects.Count; i++)
            {
                var effect = effects[i];
                var rect = new RectF(startX + i * (buttonSize + buttonSpacing), startY, buttonSize, buttonSize);
                _buttonRects.Add(rect);
                DrawEffectButton(canvas, rect, effect);
            }
        }
        else
        {
            // Vertical layout - buttons on left
            float startX = dirtyRect.X + buttonMargin;
            float startY = dirtyRect.Y + buttonMargin;

            for (int i = 0; i < effects.Count; i++)
            {
                var effect = effects[i];
                var rect = new RectF(startX, startY + i * (buttonSize + buttonSpacing), buttonSize, buttonSize);
                _buttonRects.Add(rect);
                DrawEffectButton(canvas, rect, effect);
            }
        }

        // Draw placeholder text for effect controls area
        canvas.FontSize = 14;
        canvas.FontColor = Color.FromArgb("#666666");
        string selectedName = _selector.SelectedEffect.ToString();
        
        if (_isHorizontal)
        {
            float controlsY = dirtyRect.Y + buttonMargin + 50 + buttonSpacing;
            var controlsRect = new RectF(dirtyRect.X, controlsY, dirtyRect.Width, dirtyRect.Height - controlsY);
            canvas.DrawString($"{selectedName} controls", controlsRect, HorizontalAlignment.Center, VerticalAlignment.Center);
        }
        else
        {
            float controlsX = dirtyRect.X + buttonMargin + 50 + buttonSpacing;
            var controlsRect = new RectF(controlsX, dirtyRect.Y, dirtyRect.Width - controlsX, dirtyRect.Height);
            canvas.DrawString($"{selectedName} controls", controlsRect, HorizontalAlignment.Center, VerticalAlignment.Center);
        }
    }

    private void DrawEffectButton(ICanvas canvas, RectF rect, EffectType effect)
    {
        bool isSelected = _selector.IsSelected(effect);

        // Button background
        canvas.FillColor = isSelected ? ButtonSelectedColor : ButtonBackgroundColor;
        canvas.FillRoundedRectangle(rect, 8);

        // Button border
        canvas.StrokeColor = isSelected ? ButtonIconSelectedColor : Color.FromArgb("#444466");
        canvas.StrokeSize = isSelected ? 2 : 1;
        canvas.DrawRoundedRectangle(rect, 8);

        // Draw icon
        Color iconColor = isSelected ? ButtonIconSelectedColor : ButtonIconColor;
        float iconPadding = 12f;
        var iconRect = new RectF(
            rect.X + iconPadding,
            rect.Y + iconPadding,
            rect.Width - iconPadding * 2,
            rect.Height - iconPadding * 2);

        switch (effect)
        {
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

    private void DrawEQIcon(ICanvas canvas, RectF rect, Color color)
    {
        // 3 vertical bars at different heights (equalizer)
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

    private void DrawChorusIcon(ICanvas canvas, RectF rect, Color color)
    {
        // 2-3 overlapping waves
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

    private void DrawDelayIcon(ICanvas canvas, RectF rect, Color color)
    {
        // Repeating dots/circles getting smaller (echo effect)
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

    private void DrawReverbIcon(ICanvas canvas, RectF rect, Color color)
    {
        // Expanding arcs (sound waves radiating)
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

    /// <summary>
    /// Handles touch events to select effects.
    /// </summary>
    public bool OnTouch(float x, float y)
    {
        var point = new PointF(x, y);
        var effects = EffectSelector.AllEffects;

        for (int i = 0; i < _buttonRects.Count && i < effects.Count; i++)
        {
            if (_buttonRects[i].Contains(point))
            {
                _selector.SelectedEffect = effects[i];
                EffectSelected?.Invoke(this, effects[i]);
                return true;
            }
        }

        return false;
    }
}

