namespace MusicPad.Core.Layout;

/// <summary>
/// Calculates layout positions for Low Pass Filter effect controls.
/// Elements: OnOffButton, CutoffKnob, ResonanceKnob
/// 
/// This is the reference implementation - LpfLayoutDefinition should produce identical results.
/// Layout constants traced from original LpfDrawable to ensure identical layout.
/// 
/// Note: LPF has smaller knobs than Chorus/Delay/Reverb (49 max vs 65) due to limited space.
/// </summary>
public class LpfLayoutCalculator : ILayoutCalculator
{
    // Element names
    public const string OnOffButton = "OnOffButton";
    public const string CutoffKnob = "CutoffKnob";
    public const string ResonanceKnob = "ResonanceKnob";

    // Layout constants - traced from original LpfDrawable (horizontal mode)
    public const float Padding = 8f;
    public const float ButtonSize = 28f;
    public const float KnobDiameter = 41f;           // 49 * 0.42 * 2 ≈ 41 (smaller than Chorus)
    public const float KnobHitPadding = 5f;          // Extra padding for touch
    public const float KnobVerticalMargin = 12f;     // Space reserved above/below
    public const float ButtonToKnobSpacing = 19f;    // Same as Chorus
    public const float KnobToKnobSpacing = 14f;      // Same as Chorus

    public LayoutResult Calculate(RectF bounds, LayoutContext context)
    {
        return context.Orientation == Orientation.Portrait
            ? CalculateVertical(bounds)
            : CalculateHorizontal(bounds);
    }

    private LayoutResult CalculateHorizontal(RectF bounds)
    {
        var result = new LayoutResult();

        // Calculate actual knob diameter (capped by available height)
        float actualDiameter = Math.Min(bounds.Height - KnobVerticalMargin, KnobDiameter);
        float knobHitSize = actualDiameter + KnobHitPadding * 2;

        // On/Off button on the left, vertically centered
        float buttonX = bounds.X + Padding;
        float buttonY = bounds.Y + (bounds.Height - ButtonSize) / 2;
        result[OnOffButton] = new RectF(buttonX, buttonY, ButtonSize, ButtonSize);

        // Cutoff knob after button
        float cutoffKnobX = buttonX + ButtonSize + ButtonToKnobSpacing;
        float knobY = bounds.Y + (bounds.Height - knobHitSize) / 2;
        result[CutoffKnob] = new RectF(cutoffKnobX, knobY, knobHitSize, knobHitSize);

        // Resonance knob after Cutoff
        float resonanceKnobX = cutoffKnobX + knobHitSize + KnobToKnobSpacing;
        result[ResonanceKnob] = new RectF(resonanceKnobX, knobY, knobHitSize, knobHitSize);

        return result;
    }

    private LayoutResult CalculateVertical(RectF bounds)
    {
        var result = new LayoutResult();

        // Calculate actual knob diameter (capped by available width)
        float actualDiameter = Math.Min(bounds.Width - KnobVerticalMargin, KnobDiameter);
        float knobHitSize = actualDiameter + KnobHitPadding * 2;

        // On/Off button at top, horizontally centered
        float buttonX = bounds.X + (bounds.Width - ButtonSize) / 2;
        float buttonY = bounds.Y + Padding;
        result[OnOffButton] = new RectF(buttonX, buttonY, ButtonSize, ButtonSize);

        // Cutoff knob below button, horizontally centered
        float knobX = bounds.X + (bounds.Width - knobHitSize) / 2;
        float cutoffKnobY = buttonY + ButtonSize + ButtonToKnobSpacing;
        result[CutoffKnob] = new RectF(knobX, cutoffKnobY, knobHitSize, knobHitSize);

        // Resonance knob below Cutoff
        float resonanceKnobY = cutoffKnobY + knobHitSize + KnobToKnobSpacing;
        result[ResonanceKnob] = new RectF(knobX, resonanceKnobY, knobHitSize, knobHitSize);

        return result;
    }

    /// <summary>
    /// Gets the knob radius for a given bounds height (useful for drawing).
    /// </summary>
    public static float GetKnobRadius(float boundsHeight)
    {
        float actualDiameter = Math.Min(boundsHeight - KnobVerticalMargin, KnobDiameter);
        return actualDiameter / 2;
    }
}


