namespace MusicPad.Core.Models;

/// <summary>
/// Settings for a reverb effect.
/// </summary>
public class ReverbSettings
{
    private float _level = 0.3f;
    private ReverbType _type = ReverbType.Room;
    private bool _isEnabled = false;

    /// <summary>
    /// Whether the reverb effect is enabled.
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
    /// Reverb wet/dry mix level (0.0 to 1.0).
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

    /// <summary>
    /// The reverb algorithm type. Only one type can be selected at a time.
    /// </summary>
    public ReverbType Type
    {
        get => _type;
        set
        {
            if (_type != value)
            {
                _type = value;
                TypeChanged?.Invoke(this, value);
            }
        }
    }

    public event EventHandler<bool>? EnabledChanged;
    public event EventHandler<float>? LevelChanged;
    public event EventHandler<ReverbType>? TypeChanged;
}

