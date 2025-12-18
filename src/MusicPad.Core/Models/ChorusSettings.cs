namespace MusicPad.Core.Models;

/// <summary>
/// Settings for a chorus effect.
/// </summary>
public class ChorusSettings
{
    private float _depth = 0.5f;
    private float _rate = 0.3f;
    private bool _isEnabled = false;

    /// <summary>
    /// Whether the chorus effect is enabled.
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
    /// Chorus depth (0.0 to 1.0). Controls the intensity of the modulation.
    /// </summary>
    public float Depth
    {
        get => _depth;
        set
        {
            var clamped = Math.Clamp(value, 0f, 1f);
            if (Math.Abs(_depth - clamped) > float.Epsilon)
            {
                _depth = clamped;
                DepthChanged?.Invoke(this, clamped);
            }
        }
    }

    /// <summary>
    /// Chorus rate (0.0 to 1.0). Controls the speed of the modulation.
    /// </summary>
    public float Rate
    {
        get => _rate;
        set
        {
            var clamped = Math.Clamp(value, 0f, 1f);
            if (Math.Abs(_rate - clamped) > float.Epsilon)
            {
                _rate = clamped;
                RateChanged?.Invoke(this, clamped);
            }
        }
    }

    /// <summary>
    /// Event raised when enabled state changes.
    /// </summary>
    public event EventHandler<bool>? EnabledChanged;

    /// <summary>
    /// Event raised when depth changes.
    /// </summary>
    public event EventHandler<float>? DepthChanged;

    /// <summary>
    /// Event raised when rate changes.
    /// </summary>
    public event EventHandler<float>? RateChanged;

    /// <summary>
    /// Resets to default values.
    /// </summary>
    public void Reset()
    {
        Depth = 0.5f;
        Rate = 0.3f;
        IsEnabled = false;
    }
}

