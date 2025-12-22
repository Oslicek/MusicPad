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
        // All values are explicit - no magic numbers or derived calculations
        Default()
            .Constants(c => c
                .Set("Padding", 8f)
                .Set("ButtonSize", 28f)
                .Set("KnobDiameter", 52f)         // Actual visual knob diameter (was: 65 * 0.4 * 2 = 52)
                .Set("KnobHitPadding", 5f)        // Extra padding around knob for touch
                .Set("KnobVerticalMargin", 16f)   // Space reserved above/below knobs
                .Set("ButtonToKnobSpacing", 16f)  // Padding * 2
                .Set("KnobToKnobSpacing", 24f))   // Padding * 3
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

        // === PIANO PADREA: More horizontal space available ===
        When(Orientation.Landscape, PadreaShape.Piano)
            .Constants(c => c
                .Set("KnobDiameter", 56f)         // Slightly larger
                .Set("KnobToKnobSpacing", 28f))   // More spacing
            .Done();

        // === NARROW ASPECT RATIO (near-square screens) ===
        When(AspectRatio.LessThan(1.3f), Orientation.Landscape)
            .Constants(c => c
                .Set("Padding", 4f)
                .Set("KnobDiameter", 36f)         // Smaller for tight space
                .Set("ButtonToKnobSpacing", 8f)
                .Set("KnobToKnobSpacing", 16f))
            .Done();

        // === WIDE ASPECT RATIO (ultrawide screens) ===
        When(AspectRatio.GreaterThan(2.5f), Orientation.Landscape)
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
