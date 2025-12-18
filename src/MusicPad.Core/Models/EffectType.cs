namespace MusicPad.Core.Models;

/// <summary>
/// Types of audio effects available in the effect area.
/// Order matches the signal chain: EQ -> Chorus -> Delay -> Reverb
/// </summary>
public enum EffectType
{
    EQ = 0,
    Chorus = 1,
    Delay = 2,
    Reverb = 3
}

/// <summary>
/// Manages effect selection state. Only one effect's controls can be visible at a time.
/// </summary>
public class EffectSelector
{
    private EffectType _selectedEffect = EffectType.EQ;

    /// <summary>
    /// Gets or sets the currently selected effect.
    /// </summary>
    public EffectType SelectedEffect
    {
        get => _selectedEffect;
        set
        {
            if (_selectedEffect != value)
            {
                _selectedEffect = value;
                SelectionChanged?.Invoke(this, value);
            }
        }
    }

    /// <summary>
    /// Event raised when the selected effect changes.
    /// </summary>
    public event EventHandler<EffectType>? SelectionChanged;

    /// <summary>
    /// Checks if the specified effect is currently selected.
    /// </summary>
    public bool IsSelected(EffectType effect) => _selectedEffect == effect;

    /// <summary>
    /// Gets all available effect types in signal chain order.
    /// </summary>
    public static IReadOnlyList<EffectType> AllEffects { get; } = new[]
    {
        EffectType.EQ,
        EffectType.Chorus,
        EffectType.Delay,
        EffectType.Reverb
    };
}

