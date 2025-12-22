using Microsoft.Maui.Graphics;
using MusicPad.Core.Drawing;
using MusicPad.Core.Layout;
using MusicPad.Core.Models;
using MauiRectF = Microsoft.Maui.Graphics.RectF;
using LayoutRectF = MusicPad.Core.Layout.RectF;

namespace MusicPad.Controls;

/// <summary>
/// Drawable for Chorus controls with on/off button, Depth and Rate knobs.
/// Uses fluent Layout DSL for responsive positioning.
/// </summary>
public class ChorusDrawable
{
    private readonly ChorusSettings _settings;
    private readonly ChorusLayoutDefinition _layoutDefinition = ChorusLayoutDefinition.Instance;
    
    private MauiRectF _onOffButtonRect;
    private MauiRectF _depthKnobRect;
    private MauiRectF _rateKnobRect;
    private float _knobRadius;
    private bool _isDraggingDepth;
    private bool _isDraggingRate;
    private float _lastAngle;
    
    // Layout context (can be set externally for padrea shape awareness)
    private PadreaShape _padreaShape = PadreaShape.Square;

    public event EventHandler? InvalidateRequested;

    public ChorusDrawable(ChorusSettings settings)
    {
        _settings = settings;
        _settings.DepthChanged += (s, e) => InvalidateRequested?.Invoke(this, EventArgs.Empty);
        _settings.RateChanged += (s, e) => InvalidateRequested?.Invoke(this, EventArgs.Empty);
        _settings.EnabledChanged += (s, e) => InvalidateRequested?.Invoke(this, EventArgs.Empty);
    }

    public ChorusSettings Settings => _settings;
    
    /// <summary>
    /// Sets the padrea shape for layout calculations.
    /// </summary>
    public void SetPadreaShape(PadreaShape shape)
    {
        _padreaShape = shape;
    }

    /// <summary>
    /// Draws the Chorus controls using the Layout DSL.
    /// </summary>
    public void Draw(ICanvas canvas, MauiRectF dirtyRect)
    {
        // Create layout context from bounds and padrea shape
        var bounds = new LayoutRectF(dirtyRect.X, dirtyRect.Y, dirtyRect.Width, dirtyRect.Height);
        var context = LayoutContext.FromBounds(bounds, _padreaShape);
        
        // Calculate layout using the fluent DSL
        var layout = _layoutDefinition.Calculate(bounds, context);
        
        // Convert layout results to MAUI RectF for hit testing
        var onOffRect = layout[ChorusLayoutDefinition.OnOffButton];
        var depthRect = layout[ChorusLayoutDefinition.DepthKnob];
        var rateRect = layout[ChorusLayoutDefinition.RateKnob];
        
        _onOffButtonRect = new MauiRectF(onOffRect.X, onOffRect.Y, onOffRect.Width, onOffRect.Height);
        _depthKnobRect = new MauiRectF(depthRect.X, depthRect.Y, depthRect.Width, depthRect.Height);
        _rateKnobRect = new MauiRectF(rateRect.X, rateRect.Y, rateRect.Width, rateRect.Height);
        
        // Calculate knob radius from hit rect (hit rect = diameter + 2*padding)
        _knobRadius = (depthRect.Width - DrawableConstants.KnobHitPadding * 2) / 2;
        
        bool isEnabled = _settings.IsEnabled;
        
        // Draw controls using shared renderers
        ToggleRenderer.Draw(canvas, _onOffButtonRect, _settings.IsEnabled);
        KnobRenderer.Draw(canvas, depthRect.CenterX, depthRect.CenterY, _knobRadius, _settings.Depth, "DEPTH", isEnabled);
        KnobRenderer.Draw(canvas, rateRect.CenterX, rateRect.CenterY, _knobRadius, _settings.Rate, "RATE", isEnabled);
    }

    public bool OnTouch(float x, float y, bool isStart)
    {
        var point = new Microsoft.Maui.Graphics.PointF(x, y);

        if (isStart)
        {
            if (_onOffButtonRect.Contains(point))
            {
                _settings.IsEnabled = !_settings.IsEnabled;
                return true;
            }
            
            if (!_settings.IsEnabled)
                return false;
            
            if (_depthKnobRect.Contains(point))
            {
                _isDraggingDepth = true;
                _lastAngle = KnobRenderer.GetAngleFromCenter(_depthKnobRect, x, y);
                return true;
            }
            else if (_rateKnobRect.Contains(point))
            {
                _isDraggingRate = true;
                _lastAngle = KnobRenderer.GetAngleFromCenter(_rateKnobRect, x, y);
                return true;
            }
        }
        else
        {
            if (_isDraggingDepth)
            {
                float currentAngle = KnobRenderer.GetAngleFromCenter(_depthKnobRect, x, y);
                _settings.Depth = KnobRenderer.UpdateValueFromAngle(_lastAngle, currentAngle, _settings.Depth);
                _lastAngle = currentAngle;
                return true;
            }
            else if (_isDraggingRate)
            {
                float currentAngle = KnobRenderer.GetAngleFromCenter(_rateKnobRect, x, y);
                _settings.Rate = KnobRenderer.UpdateValueFromAngle(_lastAngle, currentAngle, _settings.Rate);
                _lastAngle = currentAngle;
                return true;
            }
        }

        return false;
    }

    public void OnTouchEnd()
    {
        _isDraggingDepth = false;
        _isDraggingRate = false;
    }
}
