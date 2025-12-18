namespace MusicPad.Core.Models;

/// <summary>
/// Settings for a delay effect.
/// </summary>
public class DelaySettings
{
    private float _time = 0.4f;      // Default ~300ms
    private float _feedback = 0.4f;  // Default 40% feedback
    private float _level = 0.5f;     // Default 50% wet level
    private bool _isEnabled = false;

    /// <summary>
    /// Whether the delay effect is enabled.
    /// </summary>
    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            if (_isEnabled != value)
            {
                _isEnabled = value;
                EnabledChanged?.Invoke(this, value);
            }
        }
    }

    /// <summary>
    /// Delay time (0.0 to 1.0). Maps to ~50ms to ~1000ms.
    /// </summary>
    public float Time
    {
        get => _time;
        set
        {
            var clamped = Math.Clamp(value, 0f, 1f);
            if (Math.Abs(_time - clamped) > float.Epsilon)
            {
                _time = clamped;
                TimeChanged?.Invoke(this, clamped);
            }
        }
    }

    /// <summary>
    /// Feedback amount (0.0 to 1.0). Controls how much delayed signal feeds back.
    /// </summary>
    public float Feedback
    {
        get => _feedback;
        set
        {
            var clamped = Math.Clamp(value, 0f, 1f);
            if (Math.Abs(_feedback - clamped) > float.Epsilon)
            {
                _feedback = clamped;
                FeedbackChanged?.Invoke(this, clamped);
            }
        }
    }

    /// <summary>
    /// Wet level (0.0 to 1.0). Controls the volume of the delayed signal.
    /// </summary>
    public float Level
    {
        get => _level;
        set
        {
            var clamped = Math.Clamp(value, 0f, 1f);
            if (Math.Abs(_level - clamped) > float.Epsilon)
            {
                _level = clamped;
                LevelChanged?.Invoke(this, clamped);
            }
        }
    }

    public event EventHandler<bool>? EnabledChanged;
    public event EventHandler<float>? TimeChanged;
    public event EventHandler<float>? FeedbackChanged;
    public event EventHandler<float>? LevelChanged;

    /// <summary>
    /// Resets to default values.
    /// </summary>
    public void Reset()
    {
        Time = 0.4f;
        Feedback = 0.4f;
        Level = 0.5f;
        IsEnabled = false;
    }
}

