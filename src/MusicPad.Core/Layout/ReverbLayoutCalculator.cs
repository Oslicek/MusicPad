namespace MusicPad.Core.Layout;

/// <summary>
/// Calculates layout positions for Reverb effect controls.
/// Elements: OnOffButton, LevelKnob, TypeButton0-3
/// 
/// This is the reference implementation - ReverbLayoutDefinition should produce identical results.
/// Layout constants traced from original ReverbDrawable to ensure identical layout.
/// </summary>
public class ReverbLayoutCalculator : ILayoutCalculator
{
    // Element names
    public const string OnOffButton = "OnOffButton";
    public const string LevelKnob = "LevelKnob";
    public const string TypeButton0 = "TypeButton0";
    public const string TypeButton1 = "TypeButton1";
    public const string TypeButton2 = "TypeButton2";
    public const string TypeButton3 = "TypeButton3";

    // Layout constants - traced from original ReverbDrawable
    public const float Padding = 8f;
    public const float ButtonSize = 28f;
    public const float KnobDiameter = 52f;           // 65 * 0.4 * 2
    public const float KnobHitPadding = 5f;
    public const float KnobVerticalMargin = 16f;
    public const float ButtonToKnobSpacing = 19f;
    public const float TypeButtonSize = 24f;
    public const float KnobToTypeSpacing = 32f;      // padding * 4
    public const float TypeButtonSpacing = 42f;      // circleButtonSize + padding + 10

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

        // Level knob after button
        float levelKnobX = buttonX + ButtonSize + ButtonToKnobSpacing;
        float knobY = bounds.Y + (bounds.Height - knobHitSize) / 2;
        result[LevelKnob] = new RectF(levelKnobX, knobY, knobHitSize, knobHitSize);

        // Type buttons after knob
        float knobCenterX = levelKnobX + knobHitSize / 2;
        float knobRadius = actualDiameter / 2;
        float selectorStartX = knobCenterX + knobRadius + KnobToTypeSpacing;
        float typeCenterY = bounds.Y + bounds.Height / 2 - 5; // Shift up for labels

        for (int i = 0; i < 4; i++)
        {
            float x = selectorStartX + i * TypeButtonSpacing;
            float y = typeCenterY - TypeButtonSize / 2;
            result[$"TypeButton{i}"] = new RectF(x, y, TypeButtonSize, TypeButtonSize);
        }

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




