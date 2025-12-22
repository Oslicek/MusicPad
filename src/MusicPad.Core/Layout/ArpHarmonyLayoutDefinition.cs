namespace MusicPad.Core.Layout;

/// <summary>
/// Fluent layout definition for Arpeggiator and Harmony controls.
/// 
/// Note: ArpHarmony has a complex 4-row layout with title rows and control rows.
/// Like EQ, this pattern doesn't fit the standard linear DSL, so we implement
/// the layout imperatively while reusing the ArpHarmonyLayoutCalculator constants.
/// </summary>
public class ArpHarmonyLayoutDefinition : ILayoutCalculator
{
    // Element names - same as calculator
    public const string HarmonyOnOff = ArpHarmonyLayoutCalculator.HarmonyOnOff;
    public const string HarmonyType0 = ArpHarmonyLayoutCalculator.HarmonyType0;
    public const string HarmonyType1 = ArpHarmonyLayoutCalculator.HarmonyType1;
    public const string HarmonyType2 = ArpHarmonyLayoutCalculator.HarmonyType2;
    public const string HarmonyType3 = ArpHarmonyLayoutCalculator.HarmonyType3;
    public const string ArpOnOff = ArpHarmonyLayoutCalculator.ArpOnOff;
    public const string ArpPattern0 = ArpHarmonyLayoutCalculator.ArpPattern0;
    public const string ArpPattern1 = ArpHarmonyLayoutCalculator.ArpPattern1;
    public const string ArpPattern2 = ArpHarmonyLayoutCalculator.ArpPattern2;
    public const string ArpPattern3 = ArpHarmonyLayoutCalculator.ArpPattern3;
    public const string ArpRateKnob = ArpHarmonyLayoutCalculator.ArpRateKnob;

    // Singleton instance for reuse
    private static ArpHarmonyLayoutDefinition? _instance;
    public static ArpHarmonyLayoutDefinition Instance => _instance ??= new ArpHarmonyLayoutDefinition();

    // Layout constants - use same values as calculator
    private const float Padding = ArpHarmonyLayoutCalculator.Padding;
    private const float ButtonSize = ArpHarmonyLayoutCalculator.ButtonSize;
    private const float TitleHeight = ArpHarmonyLayoutCalculator.TitleHeight;
    private const float CircleButtonSize = ArpHarmonyLayoutCalculator.CircleButtonSize;
    private const float KnobSize = ArpHarmonyLayoutCalculator.KnobSize;
    private const float KnobRatio = ArpHarmonyLayoutCalculator.KnobRatio;
    private const float KnobHitPadding = ArpHarmonyLayoutCalculator.KnobHitPadding;
    private const float ButtonSpacing = ArpHarmonyLayoutCalculator.ButtonSpacing;
    private const float CenterYOffset = ArpHarmonyLayoutCalculator.CenterYOffset;

    public LayoutResult Calculate(RectF bounds, LayoutContext context)
    {
        var result = new LayoutResult();

        // Calculate row heights
        float controlsHeight = (bounds.Height - TitleHeight * 2 - Padding * 2) / 2;

        // Row positions
        float harmonyRowY = bounds.Y + TitleHeight;
        float arpTitleY = harmonyRowY + controlsHeight + Padding;
        float arpRowY = arpTitleY + TitleHeight;

        // Harmony row
        var harmonyRowBounds = new RectF(bounds.X, harmonyRowY, bounds.Width, controlsHeight);
        CalculateHarmonyRow(harmonyRowBounds, result);

        // Arp row
        var arpRowBounds = new RectF(bounds.X, arpRowY, bounds.Width, controlsHeight);
        CalculateArpRow(arpRowBounds, result);

        return result;
    }

    private void CalculateHarmonyRow(RectF rowRect, LayoutResult result)
    {
        float centerY = rowRect.Y + rowRect.Height / 2 - CenterYOffset;
        float startX = rowRect.X + Padding;
        float x = startX;

        // On/Off toggle
        result[HarmonyOnOff] = new RectF(x, centerY - ButtonSize / 2, ButtonSize, ButtonSize);
        x += ButtonSize + Padding * 3;

        // Type buttons
        float typeButtonSpacing = CircleButtonSize + Padding + ButtonSpacing;
        result[HarmonyType0] = new RectF(x, centerY - CircleButtonSize / 2, CircleButtonSize, CircleButtonSize);
        result[HarmonyType1] = new RectF(x + typeButtonSpacing, centerY - CircleButtonSize / 2, CircleButtonSize, CircleButtonSize);
        result[HarmonyType2] = new RectF(x + 2 * typeButtonSpacing, centerY - CircleButtonSize / 2, CircleButtonSize, CircleButtonSize);
        result[HarmonyType3] = new RectF(x + 3 * typeButtonSpacing, centerY - CircleButtonSize / 2, CircleButtonSize, CircleButtonSize);
    }

    private void CalculateArpRow(RectF rowRect, LayoutResult result)
    {
        float centerY = rowRect.Y + rowRect.Height / 2 - CenterYOffset;
        float startX = rowRect.X + Padding;
        float x = startX;

        // On/Off toggle
        result[ArpOnOff] = new RectF(x, centerY - ButtonSize / 2, ButtonSize, ButtonSize);
        x += ButtonSize + Padding * 3;

        // Pattern buttons
        float patternButtonSpacing = CircleButtonSize + Padding + ButtonSpacing;
        result[ArpPattern0] = new RectF(x, centerY - CircleButtonSize / 2, CircleButtonSize, CircleButtonSize);
        result[ArpPattern1] = new RectF(x + patternButtonSpacing, centerY - CircleButtonSize / 2, CircleButtonSize, CircleButtonSize);
        result[ArpPattern2] = new RectF(x + 2 * patternButtonSpacing, centerY - CircleButtonSize / 2, CircleButtonSize, CircleButtonSize);
        result[ArpPattern3] = new RectF(x + 3 * patternButtonSpacing, centerY - CircleButtonSize / 2, CircleButtonSize, CircleButtonSize);
        x += 4 * patternButtonSpacing + Padding;

        // Rate knob
        float knobRadius = KnobSize * KnobRatio;
        float knobCenterX = x + knobRadius + KnobHitPadding;
        float knobHitSize = knobRadius * 2 + KnobHitPadding * 2;
        result[ArpRateKnob] = new RectF(
            knobCenterX - knobRadius - KnobHitPadding, 
            centerY - knobRadius - KnobHitPadding, 
            knobHitSize, 
            knobHitSize);
    }

    /// <summary>
    /// Gets the rate knob radius (delegates to calculator).
    /// </summary>
    public static float GetKnobRadius() => ArpHarmonyLayoutCalculator.GetKnobRadius();
}

