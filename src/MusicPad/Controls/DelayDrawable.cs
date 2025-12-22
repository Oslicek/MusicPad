using Microsoft.Maui.Graphics;
using MusicPad.Core.Drawing;
using MusicPad.Core.Layout;
using MusicPad.Core.Models;
using MauiRectF = Microsoft.Maui.Graphics.RectF;
using MauiPointF = Microsoft.Maui.Graphics.PointF;
using LayoutRectF = MusicPad.Core.Layout.RectF;

namespace MusicPad.Controls;

/// <summary>
/// Drawable for Delay controls with on/off button, Time, Feedback, and Level knobs.
/// Uses fluent Layout DSL for responsive positioning.
/// </summary>
public class DelayDrawable
{
    private readonly DelaySettings _settings;
    private readonly DelayLayoutDefinition _layoutDefinition = DelayLayoutDefinition.Instance;
    
    private MauiRectF _onOffButtonRect;
    private MauiRectF _timeKnobRect;
    private MauiRectF _feedbackKnobRect;
    private MauiRectF _levelKnobRect;
    private float _knobRadius;
    private int _draggingKnob = -1; // 0=time, 1=feedback, 2=level
    private float _lastAngle;
    
    private PadreaShape _padreaShape = PadreaShape.Square;

    public event EventHandler? InvalidateRequested;

    public DelayDrawable(DelaySettings settings)
    {
        _settings = settings;
        _settings.TimeChanged += (s, e) => InvalidateRequested?.Invoke(this, EventArgs.Empty);
        _settings.FeedbackChanged += (s, e) => InvalidateRequested?.Invoke(this, EventArgs.Empty);
        _settings.LevelChanged += (s, e) => InvalidateRequested?.Invoke(this, EventArgs.Empty);
        _settings.EnabledChanged += (s, e) => InvalidateRequested?.Invoke(this, EventArgs.Empty);
    }

    public DelaySettings Settings => _settings;
    
    public void SetPadreaShape(PadreaShape shape)
    {
        _padreaShape = shape;
    }

    /// <summary>
    /// Draws the Delay controls using the Layout DSL.
    /// </summary>
    public void Draw(ICanvas canvas, MauiRectF dirtyRect)
    {
        // Create layout context from bounds and padrea shape
        var bounds = new LayoutRectF(dirtyRect.X, dirtyRect.Y, dirtyRect.Width, dirtyRect.Height);
        var context = LayoutContext.FromBounds(bounds, _padreaShape);
        
        // Calculate layout using the fluent DSL
        var layout = _layoutDefinition.Calculate(bounds, context);
        
        // Convert layout results to MAUI RectF for hit testing
        var onOffRect = layout[DelayLayoutDefinition.OnOffButton];
        var timeRect = layout[DelayLayoutDefinition.TimeKnob];
        var feedbackRect = layout[DelayLayoutDefinition.FeedbackKnob];
        var levelRect = layout[DelayLayoutDefinition.LevelKnob];
        
        _onOffButtonRect = new MauiRectF(onOffRect.X, onOffRect.Y, onOffRect.Width, onOffRect.Height);
        _timeKnobRect = new MauiRectF(timeRect.X, timeRect.Y, timeRect.Width, timeRect.Height);
        _feedbackKnobRect = new MauiRectF(feedbackRect.X, feedbackRect.Y, feedbackRect.Width, feedbackRect.Height);
        _levelKnobRect = new MauiRectF(levelRect.X, levelRect.Y, levelRect.Width, levelRect.Height);
        
        // Calculate knob radius from hit rect (hit rect = diameter + 2*padding)
        _knobRadius = (timeRect.Width - DrawableConstants.KnobHitPadding * 2) / 2;
        
        bool isEnabled = _settings.IsEnabled;
        
        // Draw controls using shared renderers
        ToggleRenderer.Draw(canvas, _onOffButtonRect, _settings.IsEnabled);
        KnobRenderer.Draw(canvas, timeRect.CenterX, timeRect.CenterY, _knobRadius, _settings.Time, "TIME", isEnabled);
        KnobRenderer.Draw(canvas, feedbackRect.CenterX, feedbackRect.CenterY, _knobRadius, _settings.Feedback, "FDBK", isEnabled);
        KnobRenderer.Draw(canvas, levelRect.CenterX, levelRect.CenterY, _knobRadius, _settings.Level, "LVL", isEnabled);
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
            
            if (_timeKnobRect.Contains(point))
            {
                _draggingKnob = 0;
                _lastAngle = KnobRenderer.GetAngleFromCenter(_timeKnobRect, x, y);
                return true;
            }
            else if (_feedbackKnobRect.Contains(point))
            {
                _draggingKnob = 1;
                _lastAngle = KnobRenderer.GetAngleFromCenter(_feedbackKnobRect, x, y);
                return true;
            }
            else if (_levelKnobRect.Contains(point))
            {
                _draggingKnob = 2;
                _lastAngle = KnobRenderer.GetAngleFromCenter(_levelKnobRect, x, y);
                return true;
            }
        }
        else if (_draggingKnob >= 0)
        {
            MauiRectF knobRect = _draggingKnob switch
            {
                0 => _timeKnobRect,
                1 => _feedbackKnobRect,
                2 => _levelKnobRect,
                _ => default
            };
            
            if (knobRect != default)
            {
                float currentAngle = KnobRenderer.GetAngleFromCenter(knobRect, x, y);
                float newValue = KnobRenderer.UpdateValueFromAngle(_lastAngle, currentAngle, GetCurrentValue());
                SetCurrentValue(newValue);
                _lastAngle = currentAngle;
            }
            return true;
        }

        return false;
    }

    private float GetCurrentValue() => _draggingKnob switch
    {
        0 => _settings.Time,
        1 => _settings.Feedback,
        2 => _settings.Level,
        _ => 0
    };

    private void SetCurrentValue(float value)
    {
        switch (_draggingKnob)
        {
            case 0: _settings.Time = value; break;
            case 1: _settings.Feedback = value; break;
            case 2: _settings.Level = value; break;
        }
    }

    public void OnTouchEnd()
    {
        _draggingKnob = -1;
    }
    
    public void OnTouchMove(float x, float y)
    {
        if (_draggingKnob < 0) return;
        
        MauiRectF knobRect = _draggingKnob switch
        {
            0 => _timeKnobRect,
            1 => _feedbackKnobRect,
            2 => _levelKnobRect,
            _ => default
        };
        
        if (knobRect == default) return;
        
        float currentAngle = KnobRenderer.GetAngleFromCenter(knobRect, x, y);
        float newValue = KnobRenderer.UpdateValueFromAngle(_lastAngle, currentAngle, GetCurrentValue());
        SetCurrentValue(newValue);
        _lastAngle = currentAngle;
    }
}
