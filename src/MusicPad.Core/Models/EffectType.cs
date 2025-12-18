namespace MusicPad.Core.Models;

/// <summary>
/// Types of audio effects available in the effect area.
/// Order: ArpHarmony (note processing) -> EQ -> Chorus -> Delay -> Reverb (audio effects)
/// </summary>
public enum EffectType
{
    ArpHarmony = 0,
    EQ = 1,
    Chorus = 2,
    Delay = 3,
    Reverb = 4
}

/// <summary>
/// Manages effect selection state. Only one effect's controls can be visible at a time.
/// </summary>
public class EffectSelector
{
    private EffectType _selectedEffect = EffectType.ArpHarmony;

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
        EffectType.ArpHarmony,
        EffectType.EQ,
        EffectType.Chorus,
        EffectType.Delay,
        EffectType.Reverb
    };
}

