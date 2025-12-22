namespace MusicPad.Core.Layout;

/// <summary>
/// Calculates layout positions for Arpeggiator and Harmony controls.
/// 
/// Layout structure (4 rows):
/// - Row 1: "HARMONY" title
/// - Row 2: Harmony controls (OnOff + 4 type buttons)
/// - Row 3: "ARPEGGIO" title
/// - Row 4: Arp controls (OnOff + 4 pattern buttons + Rate knob)
/// 
/// This is the reference implementation - ArpHarmonyLayoutDefinition should produce identical results.
/// Layout constants traced from original ArpHarmonyDrawable.
/// </summary>
public class ArpHarmonyLayoutCalculator : ILayoutCalculator
{
    // Element names - Harmony row
    public const string HarmonyOnOff = "HarmonyOnOff";
    public const string HarmonyType0 = "HarmonyType0";
    public const string HarmonyType1 = "HarmonyType1";
    public const string HarmonyType2 = "HarmonyType2";
    public const string HarmonyType3 = "HarmonyType3";

    // Element names - Arp row
    public const string ArpOnOff = "ArpOnOff";
    public const string ArpPattern0 = "ArpPattern0";
    public const string ArpPattern1 = "ArpPattern1";
    public const string ArpPattern2 = "ArpPattern2";
    public const string ArpPattern3 = "ArpPattern3";
    public const string ArpRateKnob = "ArpRateKnob";

    // Layout constants - traced from original ArpHarmonyDrawable
    public const float Padding = 8f;
    public const float ButtonSize = 28f;
    public const float TitleHeight = 16f;
    public const float CircleButtonSize = 24f;
    public const float KnobSize = 49f;           // Same as LPF
    public const float KnobRatio = 0.42f;        // For calculating visual radius
    public const float KnobHitPadding = 5f;
    public const float ButtonSpacing = 8f;       // Extra spacing between circle buttons
    public const float CenterYOffset = 5f;       // Shift up to make room for labels

    public LayoutResult Calculate(RectF bounds, LayoutContext context)
    {
        var result = new LayoutResult();

        // Calculate row heights
        float controlsHeight = (bounds.Height - TitleHeight * 2 - Padding * 2) / 2;

        // Row positions (titles are not returned as elements, just used for positioning)
        float harmonyTitleY = bounds.Y;
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

        // Type buttons (circular with labels below)
        float typeButtonSpacing = CircleButtonSize + Padding + ButtonSpacing;
        for (int i = 0; i < 4; i++)
        {
            float bx = x + i * typeButtonSpacing;
            string name = i switch
            {
                0 => HarmonyType0,
                1 => HarmonyType1,
                2 => HarmonyType2,
                3 => HarmonyType3,
                _ => throw new InvalidOperationException()
            };
            result[name] = new RectF(bx, centerY - CircleButtonSize / 2, CircleButtonSize, CircleButtonSize);
        }
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
        for (int i = 0; i < 4; i++)
        {
            float bx = x + i * patternButtonSpacing;
            string name = i switch
            {
                0 => ArpPattern0,
                1 => ArpPattern1,
                2 => ArpPattern2,
                3 => ArpPattern3,
                _ => throw new InvalidOperationException()
            };
            result[name] = new RectF(bx, centerY - CircleButtonSize / 2, CircleButtonSize, CircleButtonSize);
        }
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
    /// Gets the rate knob radius (useful for drawing).
    /// </summary>
    public static float GetKnobRadius() => KnobSize * KnobRatio;
}

