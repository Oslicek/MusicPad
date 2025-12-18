namespace MusicPad.Core.Models;

/// <summary>
/// Settings for the arpeggiator.
/// </summary>
public class ArpeggiatorSettings
{
    private bool _isEnabled = false;
    private float _rate = 0.5f;
    private ArpPattern _pattern = ArpPattern.Up;

    /// <summary>
    /// Whether the arpeggiator is enabled.
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
    /// Arpeggiator rate (0.0 to 1.0). 0 = slow, 1 = fast.
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
    /// Arpeggiator pattern.
    /// </summary>
    public ArpPattern Pattern
    {
        get => _pattern;
        set
        {
            if (_pattern != value)
            {
                _pattern = value;
                PatternChanged?.Invoke(this, value);
            }
        }
    }

    public event EventHandler<bool>? EnabledChanged;
    public event EventHandler<float>? RateChanged;
    public event EventHandler<ArpPattern>? PatternChanged;
}

