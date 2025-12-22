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
/// Drawable for Reverb controls with on/off button, level knob, and 4-button type selector.
/// Uses fluent Layout DSL for responsive positioning.
/// </summary>
public class ReverbDrawable
{
    private readonly ReverbSettings _settings;
    private readonly ReverbLayoutDefinition _layoutDefinition = ReverbLayoutDefinition.Instance;
    
    // Colors for type buttons (dynamic for palette switching)
    private static Color TypeButtonBaseColor => Color.FromArgb(AppColors.TypeButtonBase);
    private static Color TypeButtonSelectedColor => Color.FromArgb(AppColors.TypeButtonSelected);
    private static Color AccentColor => Color.FromArgb(AppColors.Accent);
    private static Color LabelColor => Color.FromArgb(AppColors.TextSecondary);
    private static Color DisabledColor => Color.FromArgb(AppColors.Disabled);

    private MauiRectF _onOffButtonRect;
    private MauiRectF _levelKnobRect;
    private readonly MauiRectF[] _typeButtonRects = new MauiRectF[4];
    private float _knobRadius;
    private bool _isDraggingLevel;
    private float _lastAngle;
    
    private PadreaShape _padreaShape = PadreaShape.Square;

    private static readonly string[] TypeLabels = { "ROOM", "HALL", "PLATE", "CATH" };

    public event EventHandler? InvalidateRequested;

    public ReverbDrawable(ReverbSettings settings)
    {
        _settings = settings;
        _settings.LevelChanged += (s, e) => InvalidateRequested?.Invoke(this, EventArgs.Empty);
        _settings.TypeChanged += (s, e) => InvalidateRequested?.Invoke(this, EventArgs.Empty);
        _settings.EnabledChanged += (s, e) => InvalidateRequested?.Invoke(this, EventArgs.Empty);
    }

    public ReverbSettings Settings => _settings;
    
    public void SetPadreaShape(PadreaShape shape)
    {
        _padreaShape = shape;
    }

    /// <summary>
    /// Draws the Reverb controls using the Layout DSL.
    /// </summary>
    public void Draw(ICanvas canvas, MauiRectF dirtyRect)
    {
        // Create layout context from bounds and padrea shape
        var bounds = new LayoutRectF(dirtyRect.X, dirtyRect.Y, dirtyRect.Width, dirtyRect.Height);
        var context = LayoutContext.FromBounds(bounds, _padreaShape);
        
        // Calculate layout using the fluent DSL
        var layout = _layoutDefinition.Calculate(bounds, context);
        
        // Convert layout results to MAUI RectF for hit testing
        var onOffRect = layout[ReverbLayoutDefinition.OnOffButton];
        var levelRect = layout[ReverbLayoutDefinition.LevelKnob];
        
        _onOffButtonRect = new MauiRectF(onOffRect.X, onOffRect.Y, onOffRect.Width, onOffRect.Height);
        _levelKnobRect = new MauiRectF(levelRect.X, levelRect.Y, levelRect.Width, levelRect.Height);
        
        for (int i = 0; i < 4; i++)
        {
            var typeRect = layout[$"TypeButton{i}"];
            _typeButtonRects[i] = new MauiRectF(typeRect.X, typeRect.Y, typeRect.Width, typeRect.Height);
        }
        
        // Calculate knob radius from hit rect
        _knobRadius = (levelRect.Width - DrawableConstants.KnobHitPadding * 2) / 2;
        
        bool isEnabled = _settings.IsEnabled;
        
        // Draw controls using shared renderers
        ToggleRenderer.Draw(canvas, _onOffButtonRect, _settings.IsEnabled);
        KnobRenderer.Draw(canvas, levelRect.CenterX, levelRect.CenterY, _knobRadius, _settings.Level, "LVL", isEnabled);
        
        for (int i = 0; i < 4; i++)
        {
            DrawCircleTypeButton(canvas, _typeButtonRects[i], TypeLabels[i], (int)_settings.Type == i, isEnabled);
        }
    }

    private void DrawCircleTypeButton(ICanvas canvas, MauiRectF rect, string label, bool isSelected, bool isEnabled)
    {
        float centerX = rect.Center.X;
        float centerY = rect.Center.Y;
        float radius = Math.Min(rect.Width, rect.Height) / 2f;
        
        if (isSelected && isEnabled)
        {
            canvas.FillColor = TypeButtonSelectedColor;
        }
        else
        {
            canvas.FillColor = isEnabled ? TypeButtonBaseColor : DisabledColor.WithAlpha(0.3f);
        }
        canvas.FillCircle(centerX, centerY, radius);
        
        canvas.StrokeColor = isSelected && isEnabled ? AccentColor : (isEnabled ? Color.FromArgb(AppColors.ButtonBorder) : DisabledColor);
        canvas.StrokeSize = isSelected ? 2 : 1;
        canvas.DrawCircle(centerX, centerY, radius);
        
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
            if (_onOffButtonRect.Contains(point))
            {
                _settings.IsEnabled = !_settings.IsEnabled;
                return true;
            }
            
            if (!_settings.IsEnabled)
                return false;
            
            for (int i = 0; i < 4; i++)
            {
                if (_typeButtonRects[i].Contains(point))
                {
                    _settings.Type = (ReverbType)i;
                    return true;
                }
            }
            
            if (_levelKnobRect.Contains(point))
            {
                _isDraggingLevel = true;
                _lastAngle = KnobRenderer.GetAngleFromCenter(_levelKnobRect, x, y);
                return true;
            }
        }
        else
        {
            if (_isDraggingLevel)
            {
                float currentAngle = KnobRenderer.GetAngleFromCenter(_levelKnobRect, x, y);
                _settings.Level = KnobRenderer.UpdateValueFromAngle(_lastAngle, currentAngle, _settings.Level);
                _lastAngle = currentAngle;
                return true;
            }
        }

        return false;
    }

    public void OnTouchEnd()
    {
        _isDraggingLevel = false;
    }
}
