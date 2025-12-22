namespace MusicPad.Core.Layout;

/// <summary>
/// Fluent layout definition for Chorus effect controls.
/// Defines layout for OnOffButton, DepthKnob, and RateKnob elements.
/// </summary>
public class ChorusLayoutDefinition : LayoutDefinition
{
    // Element names
    public const string OnOffButton = "OnOffButton";
    public const string DepthKnob = "DepthKnob";
    public const string RateKnob = "RateKnob";

    // Singleton instance for reuse
    private static ChorusLayoutDefinition? _instance;
    public static ChorusLayoutDefinition Instance => _instance ??= new ChorusLayoutDefinition();

    public ChorusLayoutDefinition()
    {
        // === DEFAULT LAYOUT ===
        // Values traced from original ChorusDrawable to ensure identical layout
        Default()
            .Constants(c => c
                .Set("Padding", 8f)
                .Set("ButtonSize", 28f)
                .Set("KnobDiameter", 52f)         // Actual visual diameter (65 * 0.4 * 2)
                .Set("KnobHitPadding", 5f)        // Extra padding around knob for touch
                .Set("KnobVerticalMargin", 16f)   // Space reserved above/below knobs
                .Set("ButtonToKnobSpacing", 19f)  // Gap between button and first knob hit rect
                .Set("KnobToKnobSpacing", 14f))   // Gap between knob hit rects
            .Element(OnOffButton)
                .Left("Padding")
                .VCenter()
                .Size("ButtonSize")
            .Element(DepthKnob)
                .After(OnOffButton, "ButtonToKnobSpacing")
                .VCenter()
                .KnobSize("KnobDiameter", "KnobHitPadding", "KnobVerticalMargin")
            .Element(RateKnob)
                .After(DepthKnob, "KnobToKnobSpacing")
                .VCenter()
                .KnobSize("KnobDiameter", "KnobHitPadding", "KnobVerticalMargin")
            .Done();

        // === PIANO PADREA: Reserved for future customization ===
        // Currently matches default layout to ensure consistency with Calculator.
        // Can be customized later for piano-specific layouts.

        // === NARROW ASPECT RATIO (near-square screens) ===
        When(AspectRatio.LessThan(1.3f), Orientation.Landscape)
            .Constants(c => c
                .Set("Padding", 4f)
                .Set("KnobDiameter", 36f)         // Smaller for tight space
                .Set("ButtonToKnobSpacing", 8f)
                .Set("KnobToKnobSpacing", 16f))
            .Done();

        // === WIDE ASPECT RATIO (ultrawide screens) ===
        // Note: Effect areas typically have aspect ratios of 3-5, so threshold must be higher
        // to avoid triggering on normal layouts. Only truly extreme widths should use this.
        When(AspectRatio.GreaterThan(6.0f), Orientation.Landscape)
            .Constants(c => c
                .Set("Padding", 12f)
                .Set("KnobDiameter", 60f)         // Larger
                .Set("ButtonToKnobSpacing", 24f)
                .Set("KnobToKnobSpacing", 32f))
            .Done();

        // === PORTRAIT MODE: Vertical stacking ===
        When(Orientation.Portrait)
            .Element(OnOffButton)
                .Top("Padding")
                .HCenter()
                .Size("ButtonSize")
            .Element(DepthKnob)
                .Below(OnOffButton, "ButtonToKnobSpacing")
                .HCenter()
                .KnobSize("KnobDiameter", "KnobHitPadding", "KnobVerticalMargin")
            .Element(RateKnob)
                .Below(DepthKnob, "KnobToKnobSpacing")
                .HCenter()
                .KnobSize("KnobDiameter", "KnobHitPadding", "KnobVerticalMargin")
            .Done();
    }
}
