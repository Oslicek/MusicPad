using Microsoft.Maui.Graphics;
using MusicPad.Core.Layout;
using MusicPad.Core.Models;
using MusicPad.Core.Theme;
using LayoutRectF = MusicPad.Core.Layout.RectF;
using MauiRectF = Microsoft.Maui.Graphics.RectF;
using MauiPointF = Microsoft.Maui.Graphics.PointF;

namespace MusicPad.Controls;

/// <summary>
/// Drawable for the effect area with selection buttons and effect controls.
/// </summary>
public class EffectAreaDrawable : IDrawable
{
    private readonly EffectSelector _selector = new();
    private readonly List<MauiRectF> _buttonRects = new();
    private readonly EffectSelectorLayoutDefinition _layoutDefinition = EffectSelectorLayoutDefinition.Instance;
    private bool _isHorizontal;
    private bool _isLandscapeSquare; // Special layout for landscape with square padrea

    // ArpHarmony, LPF, EQ, Chorus, Delay, and Reverb controls
    private readonly ArpHarmonyDrawable _arpHarmonyDrawable;
    private readonly LpfDrawable _lpfDrawable;
    private readonly EqDrawable _eqDrawable;
    private readonly ChorusDrawable _chorusDrawable;
    private readonly DelayDrawable _delayDrawable;
    private readonly ReverbDrawable _reverbDrawable;
    private MauiRectF _arpHarmonyRect;
    private MauiRectF _lpfRect;
    private MauiRectF _eqRect;
    private MauiRectF _chorusRect;
    private MauiRectF _delayRect;
    private MauiRectF _reverbRect;
    private MauiRectF _controlsRect;

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
    
    private void DrawEffectTitle(ICanvas canvas, MauiRectF controlsRect, EffectType effect)
    {
        string title = EffectTitles.GetValueOrDefault(effect, "");
        if (string.IsNullOrEmpty(title)) return;
        
        // Add more space between title and top edge
        float topPadding = 4f;
        canvas.FontSize = 11;
        canvas.FontColor = Color.FromArgb(AppColors.TextSecondary);
        canvas.DrawString(title, controlsRect.X, controlsRect.Y + topPadding, controlsRect.Width, 16,
            HorizontalAlignment.Center, VerticalAlignment.Top);
    }

    private const float CornerRadius = 8f;  // Match pad corner radius
    private const float ButtonCornerRadius = 6f;  // Uniform corner radius for pad-like buttons
    private const float IconPadding = 5f;  // Icon padding within buttons
    
    public void Draw(ICanvas canvas, MauiRectF dirtyRect)
    {
        _buttonRects.Clear();

        // Draw effect area background with rounded corners
        canvas.FillColor = EffectAreaBackground;
        canvas.FillRoundedRectangle(dirtyRect, CornerRadius);
        
        // Draw visible outline - distinctive teal for effect area
        canvas.StrokeColor = Color.FromArgb(AppColors.Teal).WithAlpha(0.7f);
        canvas.StrokeSize = 2f;
        canvas.DrawRoundedRectangle(dirtyRect, CornerRadius);

        // Calculate layout using the DSL
        var bounds = new LayoutRectF(dirtyRect.X, dirtyRect.Y, dirtyRect.Width, dirtyRect.Height);
        var context = _isHorizontal ? LayoutContext.Horizontal() : LayoutContext.Vertical();
        var layout = _layoutDefinition.Calculate(bounds, context);

        // Draw effect buttons
        var effects = EffectSelector.AllEffects;
        for (int i = 0; i < EffectSelectorLayoutDefinition.ButtonCount && i < effects.Count; i++)
        {
            var layoutRect = layout[$"Button{i}"];
            var rect = new MauiRectF(layoutRect.X, layoutRect.Y, layoutRect.Width, layoutRect.Height);
            _buttonRects.Add(rect);
            DrawEffectButton(canvas, rect, effects[i]);
        }

        // Get controls area from layout
        var controlsLayoutRect = layout[EffectSelectorLayoutDefinition.ControlsArea];
        _controlsRect = new MauiRectF(controlsLayoutRect.X, controlsLayoutRect.Y, 
                                       controlsLayoutRect.Width, controlsLayoutRect.Height);

        // Draw controls based on selected effect
        switch (_selector.SelectedEffect)
        {
            case EffectType.ArpHarmony:
                DrawArpHarmonyControls(canvas);
                break;
            case EffectType.EQ:
                DrawEQControls(canvas);
                break;
            case EffectType.Chorus:
                DrawChorusControls(canvas);
                break;
            case EffectType.Delay:
                DrawDelayControls(canvas);
                break;
            case EffectType.Reverb:
                DrawReverbControls(canvas);
                break;
            default:
                // Draw placeholder text for unimplemented effects
                canvas.FontSize = 12;
                canvas.FontColor = Color.FromArgb(AppColors.TextDim);
                string selectedName = _selector.SelectedEffect.ToString();
                canvas.DrawString($"{selectedName} controls", _controlsRect, 
                    HorizontalAlignment.Center, VerticalAlignment.Center);
                break;
        }
    }

    private void DrawArpHarmonyControls(ICanvas canvas)
    {
        _arpHarmonyRect = _controlsRect;
        _arpHarmonyDrawable.Draw(canvas, _arpHarmonyRect);
    }

    private void DrawChorusControls(ICanvas canvas)
    {
        _chorusRect = _controlsRect;
        DrawEffectTitle(canvas, _chorusRect, EffectType.Chorus);
        _chorusDrawable.Draw(canvas, _chorusRect);
    }

    private void DrawDelayControls(ICanvas canvas)
    {
        _delayRect = _controlsRect;
        DrawEffectTitle(canvas, _delayRect, EffectType.Delay);
        _delayDrawable.Draw(canvas, _delayRect);
    }

    private void DrawReverbControls(ICanvas canvas)
    {
        _reverbRect = _controlsRect;
        DrawEffectTitle(canvas, _reverbRect, EffectType.Reverb);
        _reverbDrawable.Draw(canvas, _reverbRect);
    }

    private void DrawEQControls(ICanvas canvas)
    {
        float controlsX = _controlsRect.X;
        float controlsY = _controlsRect.Y;
        float controlsWidth = _controlsRect.Width;
        float controlsHeight = _controlsRect.Height;
        float buttonSpacing = EffectSelectorLayoutDefinition.ButtonSpacing;

        // Draw title at top
        DrawEffectTitle(canvas, _controlsRect, EffectType.EQ);

        if (_isLandscapeSquare)
        {
            // Landscape square: Vertical stack - LPF knobs on top, EQ sliders at bottom
            float lpfHeight = controlsHeight * 0.55f;
            float eqHeight = controlsHeight - lpfHeight - buttonSpacing;
            
            _lpfRect = new MauiRectF(controlsX, controlsY, controlsWidth, lpfHeight);
            _lpfDrawable.Draw(canvas, _lpfRect, true); // Vertical layout for knobs
            
            _eqRect = new MauiRectF(controlsX, controlsY + lpfHeight + buttonSpacing, controlsWidth, eqHeight);
            _eqDrawable.Draw(canvas, _eqRect);
        }
        else
        {
            // Other layouts: LPF and EQ side by side
            float lpfWidth = Math.Min(controlsWidth * 0.45f, 150f);
            float eqWidth = controlsWidth - lpfWidth - buttonSpacing;
            
            _lpfRect = new MauiRectF(controlsX, controlsY, lpfWidth, controlsHeight);
            _lpfDrawable.Draw(canvas, _lpfRect, false); // Horizontal layout for knobs
            
            _eqRect = new MauiRectF(controlsX + lpfWidth + buttonSpacing, controlsY, eqWidth, controlsHeight);
            _eqDrawable.Draw(canvas, _eqRect);
        }
    }

    private void DrawEffectButton(ICanvas canvas, MauiRectF rect, EffectType effect)
    {
        bool isSelected = _selector.IsSelected(effect);

        // Button background - uniform rounded square
        canvas.FillColor = isSelected ? ButtonSelectedColor : ButtonBackgroundColor;
        canvas.FillRoundedRectangle(rect, ButtonCornerRadius);

        // Button border - thin outline
        canvas.StrokeColor = isSelected ? ButtonIconSelectedColor : Color.FromArgb(AppColors.ButtonOff);
        canvas.StrokeSize = 1;
        canvas.DrawRoundedRectangle(rect, ButtonCornerRadius);

        // Draw icon using shared renderer
        Color iconColor = isSelected ? ButtonIconSelectedColor : ButtonIconColor;
        var iconRect = new MauiRectF(
            rect.X + IconPadding,
            rect.Y + IconPadding,
            rect.Width - IconPadding * 2,
            rect.Height - IconPadding * 2);

        EffectIconRenderer.Draw(canvas, iconRect, effect, iconColor);
    }

    /// <summary>
    /// Handles touch start events.
    /// </summary>
    public bool OnTouchStart(float x, float y)
    {
        var point = new MauiPointF(x, y);
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

