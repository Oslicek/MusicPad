namespace MusicPad.Core.Models;

/// <summary>
/// Settings for auto harmony.
/// </summary>
public class HarmonySettings
{
    private bool _isEnabled = false;
    private HarmonyType _type = HarmonyType.Major;

    /// <summary>
    /// Whether auto harmony is enabled.
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
    /// The harmony type to generate.
    /// </summary>
    public HarmonyType Type
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
    public event EventHandler<HarmonyType>? TypeChanged;
}

