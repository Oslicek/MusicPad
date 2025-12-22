namespace MusicPad.Core.Layout;

/// <summary>
/// Calculates layout positions for Delay effect controls.
/// Elements: OnOffButton, TimeKnob, FeedbackKnob, LevelKnob
/// 
/// This is the reference implementation - DelayLayoutDefinition should produce identical results.
/// Layout constants traced from original DelayDrawable to ensure identical layout.
/// </summary>
public class DelayLayoutCalculator : ILayoutCalculator
{
    // Element names
    public const string OnOffButton = "OnOffButton";
    public const string TimeKnob = "TimeKnob";
    public const string FeedbackKnob = "FeedbackKnob";
    public const string LevelKnob = "LevelKnob";

    // Layout constants - traced from original DelayDrawable
    public const float Padding = 8f;
    public const float ButtonSize = 28f;
    public const float KnobDiameter = 52f;           // 65 * 0.4 * 2
    public const float KnobHitPadding = 5f;          // Extra padding for touch
    public const float KnobVerticalMargin = 16f;     // Space reserved above/below
    public const float ButtonToKnobSpacing = 19f;    // Same as Chorus
    public const float KnobToKnobSpacing = 14f;      // Same as Chorus

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

        // Time knob after button
        float timeKnobX = buttonX + ButtonSize + ButtonToKnobSpacing;
        float knobY = bounds.Y + (bounds.Height - knobHitSize) / 2;
        result[TimeKnob] = new RectF(timeKnobX, knobY, knobHitSize, knobHitSize);

        // Feedback knob after Time
        float feedbackKnobX = timeKnobX + knobHitSize + KnobToKnobSpacing;
        result[FeedbackKnob] = new RectF(feedbackKnobX, knobY, knobHitSize, knobHitSize);

        // Level knob after Feedback
        float levelKnobX = feedbackKnobX + knobHitSize + KnobToKnobSpacing;
        result[LevelKnob] = new RectF(levelKnobX, knobY, knobHitSize, knobHitSize);

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


