namespace MusicPad.Core.Layout;

/// <summary>
/// Fluent layout definition for effect selector buttons.
/// 
/// Note: The effect selector has a simple linear layout pattern.
/// This definition produces identical results to EffectSelectorLayoutCalculator.
/// </summary>
public class EffectSelectorLayoutDefinition : ILayoutCalculator
{
    // Element names - same as calculator
    public const string Button0 = EffectSelectorLayoutCalculator.Button0;
    public const string Button1 = EffectSelectorLayoutCalculator.Button1;
    public const string Button2 = EffectSelectorLayoutCalculator.Button2;
    public const string Button3 = EffectSelectorLayoutCalculator.Button3;
    public const string Button4 = EffectSelectorLayoutCalculator.Button4;
    public const string ControlsArea = EffectSelectorLayoutCalculator.ControlsArea;

    // Layout constants - same as calculator
    public const float ButtonSize = EffectSelectorLayoutCalculator.ButtonSize;
    public const float ButtonSpacing = EffectSelectorLayoutCalculator.ButtonSpacing;
    public const float ButtonMargin = EffectSelectorLayoutCalculator.ButtonMargin;
    public const int ButtonCount = EffectSelectorLayoutCalculator.ButtonCount;

    // Singleton instance for reuse
    private static EffectSelectorLayoutDefinition? _instance;
    public static EffectSelectorLayoutDefinition Instance => _instance ??= new EffectSelectorLayoutDefinition();

    // Delegate to calculator since the pattern is simple and identical
    private readonly EffectSelectorLayoutCalculator _calculator = new();

    public LayoutResult Calculate(RectF bounds, LayoutContext context)
    {
        return _calculator.Calculate(bounds, context);
    }

    /// <summary>
    /// Gets button names as an array for iteration.
    /// </summary>
    public static string[] ButtonNames => new[] { Button0, Button1, Button2, Button3, Button4 };
}

