namespace MusicPad.Core.Layout;

/// <summary>
/// Fluent layout definition for Delay effect controls.
/// Defines layout for OnOffButton, TimeKnob, FeedbackKnob, and LevelKnob elements.
/// </summary>
public class DelayLayoutDefinition : LayoutDefinition
{
    // Element names
    public const string OnOffButton = "OnOffButton";
    public const string TimeKnob = "TimeKnob";
    public const string FeedbackKnob = "FeedbackKnob";
    public const string LevelKnob = "LevelKnob";

    // Singleton instance for reuse
    private static DelayLayoutDefinition? _instance;
    public static DelayLayoutDefinition Instance => _instance ??= new DelayLayoutDefinition();

    public DelayLayoutDefinition()
    {
        // === DEFAULT LAYOUT ===
        // Values traced from original DelayDrawable to ensure identical layout
        Default()
            .Constants(c => c
                .Set("Padding", 8f)
                .Set("ButtonSize", 28f)
                .Set("KnobDiameter", 52f)         // 65 * 0.4 * 2
                .Set("KnobHitPadding", 5f)        // Extra padding around knob for touch
                .Set("KnobVerticalMargin", 16f)   // Space reserved above/below knobs
                .Set("ButtonToKnobSpacing", 19f)  // Gap between button and first knob
                .Set("KnobToKnobSpacing", 14f))   // Gap between knob hit rects
            .Element(OnOffButton)
                .Left("Padding")
                .VCenter()
                .Size("ButtonSize")
            .Element(TimeKnob)
                .After(OnOffButton, "ButtonToKnobSpacing")
                .VCenter()
                .KnobSize("KnobDiameter", "KnobHitPadding", "KnobVerticalMargin")
            .Element(FeedbackKnob)
                .After(TimeKnob, "KnobToKnobSpacing")
                .VCenter()
                .KnobSize("KnobDiameter", "KnobHitPadding", "KnobVerticalMargin")
            .Element(LevelKnob)
                .After(FeedbackKnob, "KnobToKnobSpacing")
                .VCenter()
                .KnobSize("KnobDiameter", "KnobHitPadding", "KnobVerticalMargin")
            .Done();

        // === NARROW ASPECT RATIO (near-square screens) ===
        When(AspectRatio.LessThan(1.3f), Orientation.Landscape)
            .Constants(c => c
                .Set("Padding", 4f)
                .Set("KnobDiameter", 36f)
                .Set("ButtonToKnobSpacing", 10f)
                .Set("KnobToKnobSpacing", 8f))
            .Done();

        // === WIDE ASPECT RATIO (ultrawide screens) ===
        When(AspectRatio.GreaterThan(6.0f), Orientation.Landscape)
            .Constants(c => c
                .Set("Padding", 12f)
                .Set("KnobDiameter", 60f)
                .Set("ButtonToKnobSpacing", 24f)
                .Set("KnobToKnobSpacing", 20f))
            .Done();

        // === PORTRAIT MODE: Vertical stacking ===
        When(Orientation.Portrait)
            .Element(OnOffButton)
                .Top("Padding")
                .HCenter()
                .Size("ButtonSize")
            .Element(TimeKnob)
                .Below(OnOffButton, "ButtonToKnobSpacing")
                .HCenter()
                .KnobSize("KnobDiameter", "KnobHitPadding", "KnobVerticalMargin")
            .Element(FeedbackKnob)
                .Below(TimeKnob, "KnobToKnobSpacing")
                .HCenter()
                .KnobSize("KnobDiameter", "KnobHitPadding", "KnobVerticalMargin")
            .Element(LevelKnob)
                .Below(FeedbackKnob, "KnobToKnobSpacing")
                .HCenter()
                .KnobSize("KnobDiameter", "KnobHitPadding", "KnobVerticalMargin")
            .Done();
    }
}



