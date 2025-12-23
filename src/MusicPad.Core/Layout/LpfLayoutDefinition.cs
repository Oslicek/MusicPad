namespace MusicPad.Core.Layout;

/// <summary>
/// Fluent layout definition for Low Pass Filter effect controls.
/// Defines layout for OnOffButton, CutoffKnob, and ResonanceKnob elements.
/// 
/// Note: LPF has smaller knobs than Chorus/Delay/Reverb due to limited space.
/// </summary>
public class LpfLayoutDefinition : LayoutDefinition
{
    // Element names
    public const string OnOffButton = "OnOffButton";
    public const string CutoffKnob = "CutoffKnob";
    public const string ResonanceKnob = "ResonanceKnob";

    // Singleton instance for reuse
    private static LpfLayoutDefinition? _instance;
    public static LpfLayoutDefinition Instance => _instance ??= new LpfLayoutDefinition();

    public LpfLayoutDefinition()
    {
        // === DEFAULT LAYOUT ===
        // Values traced from original LpfDrawable (horizontal mode) to ensure identical layout
        // LPF uses smaller knobs (41 diameter vs 52 for Chorus) due to limited space
        Default()
            .Constants(c => c
                .Set("Padding", 8f)
                .Set("ButtonSize", 28f)
                .Set("KnobDiameter", 41f)         // 49 * 0.42 * 2 ≈ 41 (smaller than Chorus)
                .Set("KnobHitPadding", 5f)        // Extra padding around knob for touch
                .Set("KnobVerticalMargin", 12f)   // Space reserved above/below knobs
                .Set("ButtonToKnobSpacing", 19f)  // Gap between button and first knob
                .Set("KnobToKnobSpacing", 14f))   // Gap between knob hit rects
            .Element(OnOffButton)
                .Left("Padding")
                .VCenter()
                .Size("ButtonSize")
            .Element(CutoffKnob)
                .After(OnOffButton, "ButtonToKnobSpacing")
                .VCenter()
                .KnobSize("KnobDiameter", "KnobHitPadding", "KnobVerticalMargin")
            .Element(ResonanceKnob)
                .After(CutoffKnob, "KnobToKnobSpacing")
                .VCenter()
                .KnobSize("KnobDiameter", "KnobHitPadding", "KnobVerticalMargin")
            .Done();

        // === NARROW ASPECT RATIO (near-square screens) ===
        When(AspectRatio.LessThan(1.3f), Orientation.Landscape)
            .Constants(c => c
                .Set("Padding", 4f)
                .Set("KnobDiameter", 30f)
                .Set("ButtonToKnobSpacing", 10f)
                .Set("KnobToKnobSpacing", 8f))
            .Done();

        // === WIDE ASPECT RATIO (ultrawide screens) ===
        When(AspectRatio.GreaterThan(6.0f), Orientation.Landscape)
            .Constants(c => c
                .Set("Padding", 12f)
                .Set("KnobDiameter", 48f)
                .Set("ButtonToKnobSpacing", 24f)
                .Set("KnobToKnobSpacing", 18f))
            .Done();

        // === PORTRAIT MODE: Vertical stacking ===
        When(Orientation.Portrait)
            .Element(OnOffButton)
                .Top("Padding")
                .HCenter()
                .Size("ButtonSize")
            .Element(CutoffKnob)
                .Below(OnOffButton, "ButtonToKnobSpacing")
                .HCenter()
                .KnobSize("KnobDiameter", "KnobHitPadding", "KnobVerticalMargin")
            .Element(ResonanceKnob)
                .Below(CutoffKnob, "KnobToKnobSpacing")
                .HCenter()
                .KnobSize("KnobDiameter", "KnobHitPadding", "KnobVerticalMargin")
            .Done();
    }
}




