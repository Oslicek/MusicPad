using Microsoft.Maui.Graphics;
using MusicPad.Core.Models;
using MusicPad.Core.Theme;

namespace MusicPad.Controls;

/// <summary>
/// Drawable for the effect area with selection buttons and effect controls.
/// </summary>
public class EffectAreaDrawable : IDrawable
{
    private readonly EffectSelector _selector = new();
    private readonly List<RectF> _buttonRects = new();
    private bool _isHorizontal;
    private bool _isLandscapeSquare; // Special layout for landscape with square padrea

    // ArpHarmony, LPF, EQ, Chorus, Delay, and Reverb controls
    private readonly ArpHarmonyDrawable _arpHarmonyDrawable;
    private readonly LpfDrawable _lpfDrawable;
    private readonly EqDrawable _eqDrawable;
    private readonly ChorusDrawable _chorusDrawable;
    private readonly DelayDrawable _delayDrawable;
    private readonly ReverbDrawable _reverbDrawable;
    private RectF _arpHarmonyRect;
    private RectF _lpfRect;
    private RectF _eqRect;
    private RectF _chorusRect;
    private RectF _delayRect;
    private RectF _reverbRect;

    // Colors (dynamic for palette switching)
    private static Color ButtonBackgroundColor => Color.FromArgb(AppColors.EffectButtonBackground);
    private static Color ButtonSelectedColor => Color.FromArgb(AppColors.EffectButtonSelected);
    private static Color ButtonIconColor => Color.FromArgb(AppColors.EffectIconNormal);
    private static Color ButtonIconSelectedColor => Color.FromArgb(AppColors.EffectIconSelected);
    private static Color EffectAreaBackground => Color.FromArgb(AppColors.BackgroundEffect);

    public event EventHandler<EffectType>? EffectSelected;
    public event EventHandler? InvalidateRequested;

    public EffectAreaDrawable()
    {
        _arpHarmonyDrawable = new ArpHarmonyDrawable(new HarmonySettings(), new ArpeggiatorSettings());
        _lpfDrawable = new LpfDrawable(new LowPassFilterSettings());
        _eqDrawable = new EqDrawable(new EqualizerSettings());
        _chorusDrawable = new ChorusDrawable(new ChorusSettings());
        _delayDrawable = new DelayDrawable(new DelaySettings());
        _reverbDrawable = new ReverbDrawable(new ReverbSettings());
        
        _arpHarmonyDrawable.InvalidateRequested += (s, e) => InvalidateRequested?.Invoke(this, EventArgs.Empty);
        _lpfDrawable.InvalidateRequested += (s, e) => InvalidateRequested?.Invoke(this, EventArgs.Empty);
        _eqDrawable.InvalidateRequested += (s, e) => InvalidateRequested?.Invoke(this, EventArgs.Empty);
        _chorusDrawable.InvalidateRequested += (s, e) => InvalidateRequested?.Invoke(this, EventArgs.Empty);
        _delayDrawable.InvalidateRequested += (s, e) => InvalidateRequested?.Invoke(this, EventArgs.Empty);
        _reverbDrawable.InvalidateRequested += (s, e) => InvalidateRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Gets the effect selector for external access.
    /// </summary>
    public EffectSelector Selector => _selector;

    /// <summary>
    /// Gets the Harmony settings for external access.
    /// </summary>
    public HarmonySettings HarmonySettings => _arpHarmonyDrawable.HarmonySettings;

    /// <summary>
    /// Gets the Arpeggiator settings for external access.
    /// </summary>
    public ArpeggiatorSettings ArpSettings => _arpHarmonyDrawable.ArpSettings;

    /// <summary>
    /// Gets the LPF settings for external access.
    /// </summary>
    public LowPassFilterSettings LpfSettings => _lpfDrawable.Settings;

    /// <summary>
    /// Gets the EQ settings for external access.
    /// </summary>
    public EqualizerSettings EqSettings => _eqDrawable.Settings;

    /// <summary>
    /// Gets the Chorus settings for external access.
    /// </summary>
    public ChorusSettings ChorusSettings => _chorusDrawable.Settings;

    /// <summary>
    /// Gets the Delay settings for external access.
    /// </summary>
    public DelaySettings DelaySettings => _delayDrawable.Settings;

    /// <summary>
    /// Gets the Reverb settings for external access.
    /// </summary>
    public ReverbSettings ReverbSettings => _reverbDrawable.Settings;

    /// <summary>
    /// Sets whether buttons should be arranged horizontally (landscape) or vertically (portrait).
    /// </summary>
    public void SetOrientation(bool isHorizontal)
    {
        _isHorizontal = isHorizontal;
    }

    /// <summary>
    /// Sets whether we're in landscape with square padrea (special layout: EQ under LPF).
    /// </summary>
    public void SetLandscapeSquare(bool isLandscapeSquare)
    {
        _isLandscapeSquare = isLandscapeSquare;
    }
    
    private static readonly Dictionary<EffectType, string> EffectTitles = new()
    {
        { EffectType.ArpHarmony, "" }, // Has internal titles for HARMONY and ARPEGGIO
        { EffectType.EQ, "EQUALIZER" },
        { EffectType.Chorus, "CHORUS" },
        { EffectType.Delay, "DELAY" },
        { EffectType.Reverb, "REVERB" }
    };
    
    private void DrawEffectTitle(ICanvas canvas, RectF controlsRect, EffectType effect)
    {
        string title = EffectTitles.GetValueOrDefault(effect, "");
        if (string.IsNullOrEmpty(title)) return;
        
        canvas.FontSize = 11;
        canvas.FontColor = Color.FromArgb(AppColors.TextSecondary);
        canvas.DrawString(title, controlsRect.X, controlsRect.Y, controlsRect.Width, 16,
            HorizontalAlignment.Center, VerticalAlignment.Top);
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        _buttonRects.Clear();

        // Draw effect area background
        canvas.FillColor = EffectAreaBackground;
        canvas.FillRectangle(dirtyRect);

        // Calculate button layout - larger circular buttons
        float buttonSize = 30f;
        float buttonSpacing = 3f;
        float buttonMargin = 4f;

        var effects = EffectSelector.AllEffects;

        if (_isHorizontal)
        {
            // Horizontal layout - buttons at top
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

        // Draw controls based on selected effect
        switch (_selector.SelectedEffect)
        {
            case EffectType.ArpHarmony:
                DrawArpHarmonyControls(canvas, dirtyRect, buttonSize, buttonSpacing, buttonMargin);
                break;
            case EffectType.EQ:
                DrawEQControls(canvas, dirtyRect, buttonSize, buttonSpacing, buttonMargin);
                break;
            case EffectType.Chorus:
                DrawChorusControls(canvas, dirtyRect, buttonSize, buttonSpacing, buttonMargin);
                break;
            case EffectType.Delay:
                DrawDelayControls(canvas, dirtyRect, buttonSize, buttonSpacing, buttonMargin);
                break;
            case EffectType.Reverb:
                DrawReverbControls(canvas, dirtyRect, buttonSize, buttonSpacing, buttonMargin);
                break;
            default:
                // Draw placeholder text for unimplemented effects
                canvas.FontSize = 12;
                canvas.FontColor = Color.FromArgb(AppColors.TextDim);
                string selectedName = _selector.SelectedEffect.ToString();
                
                if (_isHorizontal)
                {
                    float controlsY = dirtyRect.Y + buttonMargin + buttonSize + buttonSpacing;
                    var controlsRect = new RectF(dirtyRect.X, controlsY, dirtyRect.Width, dirtyRect.Height - controlsY);
                    canvas.DrawString($"{selectedName} controls", controlsRect, HorizontalAlignment.Center, VerticalAlignment.Center);
                }
                else
                {
                    float controlsX = dirtyRect.X + buttonMargin + buttonSize + buttonSpacing;
                    var controlsRect = new RectF(controlsX, dirtyRect.Y, dirtyRect.Width - controlsX, dirtyRect.Height);
                    canvas.DrawString($"{selectedName} controls", controlsRect, HorizontalAlignment.Center, VerticalAlignment.Center);
                }
                break;
        }
    }

    private void DrawArpHarmonyControls(ICanvas canvas, RectF dirtyRect, float buttonSize, float buttonSpacing, float buttonMargin)
    {
        float controlsX, controlsY, controlsWidth, controlsHeight;
        
        if (_isHorizontal)
        {
            controlsX = dirtyRect.X;
            controlsY = dirtyRect.Y + buttonMargin + buttonSize + buttonSpacing;
            controlsWidth = dirtyRect.Width;
            controlsHeight = dirtyRect.Height - (controlsY - dirtyRect.Y);
        }
        else
        {
            controlsX = dirtyRect.X + buttonMargin + buttonSize + buttonSpacing;
            controlsY = dirtyRect.Y;
            controlsWidth = dirtyRect.Width - (controlsX - dirtyRect.X);
            controlsHeight = dirtyRect.Height;
        }

        _arpHarmonyRect = new RectF(controlsX, controlsY, controlsWidth, controlsHeight);
        _arpHarmonyDrawable.Draw(canvas, _arpHarmonyRect);
    }

    private void DrawChorusControls(ICanvas canvas, RectF dirtyRect, float buttonSize, float buttonSpacing, float buttonMargin)
    {
        float controlsX, controlsY, controlsWidth, controlsHeight;
        
        if (_isHorizontal)
        {
            controlsX = dirtyRect.X;
            controlsY = dirtyRect.Y + buttonMargin + buttonSize + buttonSpacing;
            controlsWidth = dirtyRect.Width;
            controlsHeight = dirtyRect.Height - (controlsY - dirtyRect.Y);
        }
        else
        {
            controlsX = dirtyRect.X + buttonMargin + buttonSize + buttonSpacing;
            controlsY = dirtyRect.Y;
            controlsWidth = dirtyRect.Width - (controlsX - dirtyRect.X);
            controlsHeight = dirtyRect.Height;
        }

        _chorusRect = new RectF(controlsX, controlsY, controlsWidth, controlsHeight);
        DrawEffectTitle(canvas, _chorusRect, EffectType.Chorus);
        _chorusDrawable.Draw(canvas, _chorusRect);
    }

    private void DrawDelayControls(ICanvas canvas, RectF dirtyRect, float buttonSize, float buttonSpacing, float buttonMargin)
    {
        float controlsX, controlsY, controlsWidth, controlsHeight;
        
        if (_isHorizontal)
        {
            controlsX = dirtyRect.X;
            controlsY = dirtyRect.Y + buttonMargin + buttonSize + buttonSpacing;
            controlsWidth = dirtyRect.Width;
            controlsHeight = dirtyRect.Height - (controlsY - dirtyRect.Y);
        }
        else
        {
            controlsX = dirtyRect.X + buttonMargin + buttonSize + buttonSpacing;
            controlsY = dirtyRect.Y;
            controlsWidth = dirtyRect.Width - (controlsX - dirtyRect.X);
            controlsHeight = dirtyRect.Height;
        }

        _delayRect = new RectF(controlsX, controlsY, controlsWidth, controlsHeight);
        DrawEffectTitle(canvas, _delayRect, EffectType.Delay);
        _delayDrawable.Draw(canvas, _delayRect);
    }

    private void DrawReverbControls(ICanvas canvas, RectF dirtyRect, float buttonSize, float buttonSpacing, float buttonMargin)
    {
        float controlsX, controlsY, controlsWidth, controlsHeight;
        
        if (_isHorizontal)
        {
            controlsX = dirtyRect.X;
            controlsY = dirtyRect.Y + buttonMargin + buttonSize + buttonSpacing;
            controlsWidth = dirtyRect.Width;
            controlsHeight = dirtyRect.Height - (controlsY - dirtyRect.Y);
        }
        else
        {
            controlsX = dirtyRect.X + buttonMargin + buttonSize + buttonSpacing;
            controlsY = dirtyRect.Y;
            controlsWidth = dirtyRect.Width - (controlsX - dirtyRect.X);
            controlsHeight = dirtyRect.Height;
        }

        _reverbRect = new RectF(controlsX, controlsY, controlsWidth, controlsHeight);
        DrawEffectTitle(canvas, _reverbRect, EffectType.Reverb);
        _reverbDrawable.Draw(canvas, _reverbRect);
    }

    private void DrawEQControls(ICanvas canvas, RectF dirtyRect, float buttonSize, float buttonSpacing, float buttonMargin)
    {
        // Controls area starts after buttons
        float controlsX, controlsY, controlsWidth, controlsHeight;
        
        if (_isHorizontal)
        {
            // Horizontal layout - controls below buttons
            controlsX = dirtyRect.X;
            controlsY = dirtyRect.Y + buttonMargin + buttonSize + buttonSpacing;
            controlsWidth = dirtyRect.Width;
            controlsHeight = dirtyRect.Height - (controlsY - dirtyRect.Y);
        }
        else
        {
            // Vertical layout - controls to the right of buttons
            controlsX = dirtyRect.X + buttonMargin + buttonSize + buttonSpacing;
            controlsY = dirtyRect.Y;
            controlsWidth = dirtyRect.Width - (controlsX - dirtyRect.X);
            controlsHeight = dirtyRect.Height;
        }

        // Draw title at top
        DrawEffectTitle(canvas, new RectF(controlsX, controlsY, controlsWidth, controlsHeight), EffectType.EQ);

        if (_isLandscapeSquare)
        {
            // Landscape square: Vertical stack - LPF knobs on top, EQ sliders at bottom
            float lpfHeight = controlsHeight * 0.55f;
            float eqHeight = controlsHeight - lpfHeight - buttonSpacing;
            
            _lpfRect = new RectF(controlsX, controlsY, controlsWidth, lpfHeight);
            _lpfDrawable.Draw(canvas, _lpfRect, true); // Vertical layout for knobs
            
            _eqRect = new RectF(controlsX, controlsY + lpfHeight + buttonSpacing, controlsWidth, eqHeight);
            _eqDrawable.Draw(canvas, _eqRect);
        }
        else
        {
            // Other layouts: LPF and EQ side by side
            float lpfWidth = Math.Min(controlsWidth * 0.45f, 150f);
            float eqWidth = controlsWidth - lpfWidth - buttonSpacing;
            
            _lpfRect = new RectF(controlsX, controlsY, lpfWidth, controlsHeight);
            _lpfDrawable.Draw(canvas, _lpfRect, false); // Horizontal layout for knobs
            
            _eqRect = new RectF(controlsX + lpfWidth + buttonSpacing, controlsY, eqWidth, controlsHeight);
            _eqDrawable.Draw(canvas, _eqRect);
        }
    }

    private void DrawEffectButton(ICanvas canvas, RectF rect, EffectType effect)
    {
        bool isSelected = _selector.IsSelected(effect);
        float cornerRadius = 6f;

        // Button background - rounded square
        canvas.FillColor = isSelected ? ButtonSelectedColor : ButtonBackgroundColor;
        canvas.FillRoundedRectangle(rect, cornerRadius);

        // Button border - thin outline
        canvas.StrokeColor = isSelected ? ButtonIconSelectedColor : Color.FromArgb(AppColors.ButtonOff);
        canvas.StrokeSize = 1;
        canvas.DrawRoundedRectangle(rect, cornerRadius);

        // Draw icon - minimal padding for small buttons
        Color iconColor = isSelected ? ButtonIconSelectedColor : ButtonIconColor;
        float iconPadding = 5f;
        var iconRect = new RectF(
            rect.X + iconPadding,
            rect.Y + iconPadding,
            rect.Width - iconPadding * 2,
            rect.Height - iconPadding * 2);

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

    private void DrawArpHarmonyIcon(ICanvas canvas, RectF rect, Color color)
    {
        // Stacked notes with ascending arrow - represents chord + arpeggio
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
    /// Handles touch start events.
    /// </summary>
    public bool OnTouchStart(float x, float y)
    {
        var point = new PointF(x, y);
        var effects = EffectSelector.AllEffects;

        // Check effect selection buttons
        for (int i = 0; i < _buttonRects.Count && i < effects.Count; i++)
        {
            if (_buttonRects[i].Contains(point))
            {
                _selector.SelectedEffect = effects[i];
                EffectSelected?.Invoke(this, effects[i]);
                return true;
            }
        }

        // Handle control touches based on selected effect
        switch (_selector.SelectedEffect)
        {
            case EffectType.ArpHarmony:
                if (_arpHarmonyRect.Contains(point) && _arpHarmonyDrawable.OnTouch(x, y, true))
                    return true;
                break;
            case EffectType.EQ:
                if (_lpfRect.Contains(point) && _lpfDrawable.OnTouch(x, y, true))
                    return true;
                if (_eqRect.Contains(point) && _eqDrawable.OnTouch(x, y, true))
                    return true;
                break;
            case EffectType.Chorus:
                if (_chorusRect.Contains(point) && _chorusDrawable.OnTouch(x, y, true))
                    return true;
                break;
            case EffectType.Delay:
                if (_delayRect.Contains(point) && _delayDrawable.OnTouch(x, y, true))
                    return true;
                break;
            case EffectType.Reverb:
                if (_reverbRect.Contains(point) && _reverbDrawable.OnTouch(x, y, true))
                    return true;
                break;
        }

        return false;
    }

    /// <summary>
    /// Handles touch move events.
    /// </summary>
    public bool OnTouchMove(float x, float y)
    {
        switch (_selector.SelectedEffect)
        {
            case EffectType.ArpHarmony:
                if (_arpHarmonyDrawable.OnTouch(x, y, false))
                    return true;
                break;
            case EffectType.EQ:
                if (_lpfDrawable.OnTouch(x, y, false))
                    return true;
                if (_eqDrawable.OnTouch(x, y, false))
                    return true;
                break;
            case EffectType.Chorus:
                if (_chorusDrawable.OnTouch(x, y, false))
                    return true;
                break;
            case EffectType.Delay:
                if (_delayDrawable.OnTouch(x, y, false))
                    return true;
                break;
            case EffectType.Reverb:
                if (_reverbDrawable.OnTouch(x, y, false))
                    return true;
                break;
        }
        return false;
    }

    /// <summary>
    /// Handles touch end events.
    /// </summary>
    public void OnTouchEnd()
    {
        _arpHarmonyDrawable.OnTouchEnd();
        _lpfDrawable.OnTouchEnd();
        _eqDrawable.OnTouchEnd();
        _chorusDrawable.OnTouchEnd();
        _delayDrawable.OnTouchEnd();
        _reverbDrawable.OnTouchEnd();
    }

    /// <summary>
    /// Legacy touch handler for compatibility.
    /// </summary>
    public bool OnTouch(float x, float y)
    {
        return OnTouchStart(x, y);
    }
}

