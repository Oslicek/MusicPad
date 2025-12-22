using Microsoft.Maui.Graphics;
using MusicPad.Core.Drawing;
using MusicPad.Core.Layout;
using MusicPad.Core.Models;
using MauiRectF = Microsoft.Maui.Graphics.RectF;
using MauiPointF = Microsoft.Maui.Graphics.PointF;
using LayoutRectF = MusicPad.Core.Layout.RectF;

namespace MusicPad.Controls;

/// <summary>
/// Drawable for Low Pass Filter controls with on/off button, Cutoff and Resonance knobs.
/// Uses fluent Layout DSL for responsive positioning.
/// </summary>
public class LpfDrawable
{
    private readonly LowPassFilterSettings _settings;
    private readonly LpfLayoutDefinition _layoutDefinition = LpfLayoutDefinition.Instance;
    
    private MauiRectF _onOffButtonRect;
    private MauiRectF _cutoffKnobRect;
    private MauiRectF _resonanceKnobRect;
    private float _knobRadius;
    private bool _isDraggingCutoff;
    private bool _isDraggingResonance;
    private float _lastAngle;
    
    private PadreaShape _padreaShape = PadreaShape.Square;

    public event EventHandler? InvalidateRequested;

    public LpfDrawable(LowPassFilterSettings settings)
    {
        _settings = settings;
        _settings.CutoffChanged += (s, e) => InvalidateRequested?.Invoke(this, EventArgs.Empty);
        _settings.ResonanceChanged += (s, e) => InvalidateRequested?.Invoke(this, EventArgs.Empty);
        _settings.EnabledChanged += (s, e) => InvalidateRequested?.Invoke(this, EventArgs.Empty);
    }

    public LowPassFilterSettings Settings => _settings;
    
    public void SetPadreaShape(PadreaShape shape)
    {
        _padreaShape = shape;
    }

    /// <summary>
    /// Draws the LPF controls using the Layout DSL.
    /// </summary>
    /// <param name="isVertical">Explicitly sets orientation - overrides bounds-based detection.</param>
    public void Draw(ICanvas canvas, MauiRectF dirtyRect, bool isVertical = false)
    {
        // Create layout context from bounds and padrea shape
        var bounds = new LayoutRectF(dirtyRect.X, dirtyRect.Y, dirtyRect.Width, dirtyRect.Height);
        // Explicitly set orientation with standard aspect ratio to avoid narrow/wide overrides
        // (bounds may be tall even in landscape mode, which would trigger narrow override incorrectly)
        var context = isVertical 
            ? LayoutContext.Vertical(2.0f, _padreaShape)   // Standard aspect ratio
            : LayoutContext.Horizontal(2.0f, _padreaShape); // Standard aspect ratio
        
        // Calculate layout using the fluent DSL
        var layout = _layoutDefinition.Calculate(bounds, context);
        
        // Convert layout results to MAUI RectF for hit testing
        var onOffRect = layout[LpfLayoutDefinition.OnOffButton];
        var cutoffRect = layout[LpfLayoutDefinition.CutoffKnob];
        var resonanceRect = layout[LpfLayoutDefinition.ResonanceKnob];
        
        _onOffButtonRect = new MauiRectF(onOffRect.X, onOffRect.Y, onOffRect.Width, onOffRect.Height);
        _cutoffKnobRect = new MauiRectF(cutoffRect.X, cutoffRect.Y, cutoffRect.Width, cutoffRect.Height);
        _resonanceKnobRect = new MauiRectF(resonanceRect.X, resonanceRect.Y, resonanceRect.Width, resonanceRect.Height);
        
        // Calculate knob radius from hit rect (hit rect = diameter + 2*padding)
        _knobRadius = (cutoffRect.Width - DrawableConstants.KnobHitPadding * 2) / 2;
        
        bool isEnabled = _settings.IsEnabled;
        
        // Draw controls using shared renderers
        ToggleRenderer.Draw(canvas, _onOffButtonRect, _settings.IsEnabled);
        KnobRenderer.Draw(canvas, cutoffRect.CenterX, cutoffRect.CenterY, _knobRadius, _settings.Cutoff, "CUT", isEnabled);
        KnobRenderer.Draw(canvas, resonanceRect.CenterX, resonanceRect.CenterY, _knobRadius, _settings.Resonance, "RES", isEnabled);
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
            
            if (_cutoffKnobRect.Contains(point))
            {
                _isDraggingCutoff = true;
                _lastAngle = KnobRenderer.GetAngleFromCenter(_cutoffKnobRect, x, y);
                return true;
            }
            else if (_resonanceKnobRect.Contains(point))
            {
                _isDraggingResonance = true;
                _lastAngle = KnobRenderer.GetAngleFromCenter(_resonanceKnobRect, x, y);
                return true;
            }
        }
        else
        {
            if (_isDraggingCutoff)
            {
                float currentAngle = KnobRenderer.GetAngleFromCenter(_cutoffKnobRect, x, y);
                _settings.Cutoff = KnobRenderer.UpdateValueFromAngle(_lastAngle, currentAngle, _settings.Cutoff);
                _lastAngle = currentAngle;
                return true;
            }
            else if (_isDraggingResonance)
            {
                float currentAngle = KnobRenderer.GetAngleFromCenter(_resonanceKnobRect, x, y);
                _settings.Resonance = KnobRenderer.UpdateValueFromAngle(_lastAngle, currentAngle, _settings.Resonance);
                _lastAngle = currentAngle;
                return true;
            }
        }

        return false;
    }

    public void OnTouchEnd()
    {
        _isDraggingCutoff = false;
        _isDraggingResonance = false;
    }
}
