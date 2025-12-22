using Microsoft.Maui.Graphics;
using MusicPad.Core.Drawing;
using MusicPad.Core.Layout;
using MusicPad.Core.Models;
using MusicPad.Core.Theme;
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
    
    // Colors for type buttons (dynamic for palette switching)
    private static Color TypeButtonBaseColor => Color.FromArgb(AppColors.TypeButtonBase);
    private static Color TypeButtonSelectedColor => Color.FromArgb(AppColors.TypeButtonSelected);
    private static Color AccentColor => Color.FromArgb(AppColors.Accent);
    private static Color LabelColor => Color.FromArgb(AppColors.TextSecondary);
    private static Color DisabledColor => Color.FromArgb(AppColors.Disabled);

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
        DrawHarmonyControls(canvas);
        
        // Row 3: ARPEGGIO title
        DrawSectionTitle(canvas, "ARPEGGIO", dirtyRect.X, arpTitleY, dirtyRect.Width, TitleHeight);
        
        // Row 4: Arpeggio controls
        DrawArpControls(canvas);
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

    private void DrawHarmonyControls(ICanvas canvas)
    {
        bool isEnabled = _harmonySettings.IsEnabled;
        bool isAllowed = _harmonySettings.IsAllowed;
        bool effectiveEnabled = isEnabled && isAllowed;

        // On/Off toggle using shared renderer
        ToggleRenderer.Draw(canvas, _harmonyOnOffRect, isEnabled, isAllowed);

        // Type buttons
        for (int i = 0; i < 4; i++)
        {
            bool isSelected = HarmonyTypeMap[i] == (int)_harmonySettings.Type;
            DrawCircleButtonWithLabel(canvas, _harmonyTypeRects[i], HarmonyLabels[i], isSelected, effectiveEnabled);
        }
    }

    private void DrawArpControls(ICanvas canvas)
    {
        bool isEnabled = _arpSettings.IsEnabled;

        // On/Off toggle using shared renderer
        ToggleRenderer.Draw(canvas, _arpOnOffRect, isEnabled);

        // Pattern buttons
        for (int i = 0; i < 4; i++)
        {
            bool isSelected = (int)_arpSettings.Pattern == i;
            DrawCircleButtonWithLabel(canvas, _arpPatternRects[i], PatternLabels[i], isSelected, isEnabled);
        }

        // Rate knob using shared renderer
        KnobRenderer.Draw(canvas, _arpRateKnobRect.Center.X, _arpRateKnobRect.Center.Y, 
            _rateKnobRadius, _arpSettings.Rate, "RATE", isEnabled, DrawableConstants.FontSizeSmall);
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
        canvas.FontSize = DrawableConstants.FontSizeSmall;
        canvas.FontColor = isEnabled ? LabelColor : DisabledColor;
        canvas.DrawString(label, centerX - 20, centerY + radius + 2, 40, DrawableConstants.LabelHeight,
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
                _lastAngle = KnobRenderer.GetAngleFromCenter(_arpRateKnobRect, x, y);
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
                float currentAngle = KnobRenderer.GetAngleFromCenter(_arpRateKnobRect, x, y);
                _arpSettings.Rate = KnobRenderer.UpdateValueFromAngle(_lastAngle, currentAngle, _arpSettings.Rate);
                _lastAngle = currentAngle;
                return true;
            }
        }

        return false;
    }

    public void OnTouchEnd()
    {
        _isDraggingRate = false;
    }
}
