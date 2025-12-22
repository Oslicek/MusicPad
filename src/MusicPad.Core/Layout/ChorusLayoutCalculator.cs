namespace MusicPad.Core.Layout;

/// <summary>
/// Calculates layout positions for Chorus effect controls.
/// Elements: OnOffButton, DepthKnob, RateKnob
/// 
/// This is the reference implementation - ChorusLayoutDefinition should produce identical results.
/// </summary>
public class ChorusLayoutCalculator : ILayoutCalculator
{
    // Element names
    public const string OnOffButton = "OnOffButton";
    public const string DepthKnob = "DepthKnob";
    public const string RateKnob = "RateKnob";

    // Layout constants - all explicitly named, no derived values
    // Values traced from original ChorusDrawable to ensure identical layout
    public const float Padding = 8f;
    public const float ButtonSize = 28f;
    public const float KnobDiameter = 52f;           // Actual visual diameter (65 * 0.4 * 2)
    public const float KnobHitPadding = 5f;          // Extra padding for touch
    public const float KnobVerticalMargin = 16f;     // Space reserved above/below
    public const float ButtonToKnobSpacing = 19f;    // Original: padding*2 + radius + padding - radius - hitPad
    public const float KnobToKnobSpacing = 14f;      // Original: center-to-center(76) - hitSize(62)

    public LayoutResult Calculate(RectF bounds, LayoutContext context)
    {
        var result = new LayoutResult();

        // Calculate actual knob diameter (capped by available height)
        float actualDiameter = Math.Min(bounds.Height - KnobVerticalMargin, KnobDiameter);
        float knobHitSize = actualDiameter + KnobHitPadding * 2;

        // On/Off button on the left, vertically centered
        float buttonX = bounds.X + Padding;
        float buttonY = bounds.Y + (bounds.Height - ButtonSize) / 2;
        result[OnOffButton] = new RectF(buttonX, buttonY, ButtonSize, ButtonSize);

        // Depth knob after button
        float depthKnobX = buttonX + ButtonSize + ButtonToKnobSpacing;
        float knobY = bounds.Y + (bounds.Height - knobHitSize) / 2;
        result[DepthKnob] = new RectF(depthKnobX, knobY, knobHitSize, knobHitSize);

        // Rate knob after Depth
        float rateKnobX = depthKnobX + knobHitSize + KnobToKnobSpacing;
        result[RateKnob] = new RectF(rateKnobX, knobY, knobHitSize, knobHitSize);

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
