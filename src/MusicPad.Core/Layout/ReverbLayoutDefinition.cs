namespace MusicPad.Core.Layout;

/// <summary>
/// Fluent layout definition for Reverb effect controls.
/// Defines layout for OnOffButton, LevelKnob, and TypeButton0-3 elements.
/// </summary>
public class ReverbLayoutDefinition : LayoutDefinition
{
    // Element names
    public const string OnOffButton = "OnOffButton";
    public const string LevelKnob = "LevelKnob";
    public const string TypeButton0 = "TypeButton0";
    public const string TypeButton1 = "TypeButton1";
    public const string TypeButton2 = "TypeButton2";
    public const string TypeButton3 = "TypeButton3";

    // Singleton instance for reuse
    private static ReverbLayoutDefinition? _instance;
    public static ReverbLayoutDefinition Instance => _instance ??= new ReverbLayoutDefinition();

    public ReverbLayoutDefinition()
    {
        // === DEFAULT LAYOUT ===
        // Values traced from original ReverbDrawable to ensure identical layout
        Default()
            .Constants(c => c
                .Set("Padding", 8f)
                .Set("ButtonSize", 28f)
                .Set("KnobDiameter", 52f)
                .Set("KnobHitPadding", 5f)
                .Set("KnobVerticalMargin", 16f)
                .Set("ButtonToKnobSpacing", 19f)
                .Set("TypeButtonSize", 24f)
                .Set("KnobToTypeSpacing", 32f)    // padding * 4
                .Set("TypeButtonSpacing", 42f))   // circleButtonSize + padding + 10
            .Element(OnOffButton)
                .Left("Padding")
                .VCenter()
                .Size("ButtonSize")
            .Element(LevelKnob)
                .After(OnOffButton, "ButtonToKnobSpacing")
                .VCenter()
                .KnobSize("KnobDiameter", "KnobHitPadding", "KnobVerticalMargin")
            .Element(TypeButton0)
                .After(LevelKnob, 27f)  // KnobToTypeSpacing(32) - KnobHitPadding(5) = 27 (align from visual knob edge)
                .VCenter(-5)  // Shift up for labels
                .Size("TypeButtonSize")
            .Element(TypeButton1)
                .After(TypeButton0, 18f)  // TypeButtonSpacing - TypeButtonSize = 42 - 24 = 18
                .VCenter(-5)
                .Size("TypeButtonSize")
            .Element(TypeButton2)
                .After(TypeButton1, 18f)
                .VCenter(-5)
                .Size("TypeButtonSize")
            .Element(TypeButton3)
                .After(TypeButton2, 18f)
                .VCenter(-5)
                .Size("TypeButtonSize")
            .Done();

        // === NARROW ASPECT RATIO ===
        When(AspectRatio.LessThan(1.3f), Orientation.Landscape)
            .Constants(c => c
                .Set("Padding", 4f)
                .Set("KnobDiameter", 36f)
                .Set("TypeButtonSize", 20f)
                .Set("ButtonToKnobSpacing", 10f)
                .Set("KnobToTypeSpacing", 16f)
                .Set("TypeButtonSpacing", 32f))
            .Done();

        // === WIDE ASPECT RATIO ===
        When(AspectRatio.GreaterThan(6.0f), Orientation.Landscape)
            .Constants(c => c
                .Set("Padding", 12f)
                .Set("KnobDiameter", 60f)
                .Set("TypeButtonSize", 28f)
                .Set("ButtonToKnobSpacing", 24f)
                .Set("KnobToTypeSpacing", 40f)
                .Set("TypeButtonSpacing", 48f))
            .Done();

        // === PORTRAIT MODE ===
        When(Orientation.Portrait)
            .Element(OnOffButton)
                .Top("Padding")
                .HCenter()
                .Size("ButtonSize")
            .Element(LevelKnob)
                .Below(OnOffButton, "ButtonToKnobSpacing")
                .HCenter()
                .KnobSize("KnobDiameter", "KnobHitPadding", "KnobVerticalMargin")
            // Type buttons arranged horizontally below knob
            .Element(TypeButton0)
                .Below(LevelKnob, "KnobToTypeSpacing")
                .Left("Padding")
                .Size("TypeButtonSize")
            .Element(TypeButton1)
                .After(TypeButton0, 18f)
                .AlignTop(TypeButton0)
                .Size("TypeButtonSize")
            .Element(TypeButton2)
                .After(TypeButton1, 18f)
                .AlignTop(TypeButton0)
                .Size("TypeButtonSize")
            .Element(TypeButton3)
                .After(TypeButton2, 18f)
                .AlignTop(TypeButton0)
                .Size("TypeButtonSize")
            .Done();
    }
}

