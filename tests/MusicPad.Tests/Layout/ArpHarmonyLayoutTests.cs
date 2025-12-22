using MusicPad.Core.Layout;

namespace MusicPad.Tests.Layout;

/// <summary>
/// Tests for ArpHarmony layout calculator and definition.
/// </summary>
public class ArpHarmonyLayoutTests
{
    // Element names - Harmony row
    private const string HarmonyOnOff = "HarmonyOnOff";
    private const string HarmonyType0 = "HarmonyType0";
    private const string HarmonyType1 = "HarmonyType1";
    private const string HarmonyType2 = "HarmonyType2";
    private const string HarmonyType3 = "HarmonyType3";

    // Element names - Arp row
    private const string ArpOnOff = "ArpOnOff";
    private const string ArpPattern0 = "ArpPattern0";
    private const string ArpPattern1 = "ArpPattern1";
    private const string ArpPattern2 = "ArpPattern2";
    private const string ArpPattern3 = "ArpPattern3";
    private const string ArpRateKnob = "ArpRateKnob";

    [Fact]
    public void Calculator_AllElementsAreCreated()
    {
        var calculator = new ArpHarmonyLayoutCalculator();
        var bounds = new RectF(0, 0, 300, 150);
        var context = LayoutContext.Horizontal(2.0f, PadreaShape.Square);

        var result = calculator.Calculate(bounds, context);

        // Harmony controls
        Assert.True(result.HasElement(HarmonyOnOff));
        Assert.True(result.HasElement(HarmonyType0));
        Assert.True(result.HasElement(HarmonyType1));
        Assert.True(result.HasElement(HarmonyType2));
        Assert.True(result.HasElement(HarmonyType3));

        // Arp controls
        Assert.True(result.HasElement(ArpOnOff));
        Assert.True(result.HasElement(ArpPattern0));
        Assert.True(result.HasElement(ArpPattern1));
        Assert.True(result.HasElement(ArpPattern2));
        Assert.True(result.HasElement(ArpPattern3));
        Assert.True(result.HasElement(ArpRateKnob));
    }

    [Fact]
    public void Calculator_HarmonyRowAboveArpRow()
    {
        var calculator = new ArpHarmonyLayoutCalculator();
        var bounds = new RectF(0, 0, 300, 150);
        var context = LayoutContext.Horizontal(2.0f, PadreaShape.Square);

        var result = calculator.Calculate(bounds, context);

        var harmonyOnOff = result[HarmonyOnOff];
        var arpOnOff = result[ArpOnOff];

        // Harmony row should be above arp row
        Assert.True(harmonyOnOff.Y < arpOnOff.Y,
            $"Harmony Y={harmonyOnOff.Y} should be < Arp Y={arpOnOff.Y}");
    }

    [Fact]
    public void Calculator_TypeButtonsAfterOnOff()
    {
        var calculator = new ArpHarmonyLayoutCalculator();
        var bounds = new RectF(0, 0, 300, 150);
        var context = LayoutContext.Horizontal(2.0f, PadreaShape.Square);

        var result = calculator.Calculate(bounds, context);

        var harmonyOnOff = result[HarmonyOnOff];
        var harmonyType0 = result[HarmonyType0];

        // Type buttons should be after OnOff button
        Assert.True(harmonyType0.X > harmonyOnOff.Right,
            $"Type0 X={harmonyType0.X} should be > OnOff.Right={harmonyOnOff.Right}");
    }

    [Fact]
    public void Calculator_TypeButtonsEvenlySpaced()
    {
        var calculator = new ArpHarmonyLayoutCalculator();
        var bounds = new RectF(0, 0, 300, 150);
        var context = LayoutContext.Horizontal(2.0f, PadreaShape.Square);

        var result = calculator.Calculate(bounds, context);

        var type0 = result[HarmonyType0];
        var type1 = result[HarmonyType1];
        var type2 = result[HarmonyType2];
        var type3 = result[HarmonyType3];

        // All type buttons should have the same size
        Assert.Equal(type0.Width, type1.Width);
        Assert.Equal(type1.Width, type2.Width);
        Assert.Equal(type2.Width, type3.Width);

        // All should be at same Y
        Assert.Equal(type0.Y, type1.Y);
        Assert.Equal(type1.Y, type2.Y);
        Assert.Equal(type2.Y, type3.Y);

        // Even spacing
        float spacing01 = type1.X - type0.Right;
        float spacing12 = type2.X - type1.Right;
        float spacing23 = type3.X - type2.Right;
        Assert.Equal(spacing01, spacing12, precision: 1);
        Assert.Equal(spacing12, spacing23, precision: 1);
    }

    [Fact]
    public void Calculator_ArpRateKnobAfterPatternButtons()
    {
        var calculator = new ArpHarmonyLayoutCalculator();
        var bounds = new RectF(0, 0, 300, 150);
        var context = LayoutContext.Horizontal(2.0f, PadreaShape.Square);

        var result = calculator.Calculate(bounds, context);

        var pattern3 = result[ArpPattern3];
        var rateKnob = result[ArpRateKnob];

        // Rate knob should be after the last pattern button
        Assert.True(rateKnob.X > pattern3.Right,
            $"RateKnob X={rateKnob.X} should be > Pattern3.Right={pattern3.Right}");
    }

    [Fact]
    public void Calculator_ElementsDoNotOverlap()
    {
        var calculator = new ArpHarmonyLayoutCalculator();
        var bounds = new RectF(0, 0, 300, 150);
        var context = LayoutContext.Horizontal(2.0f, PadreaShape.Square);

        var result = calculator.Calculate(bounds, context);

        Assert.False(result.HasOverlaps());
    }

    [Fact]
    public void Calculator_AllElementsFitWithinBounds()
    {
        var calculator = new ArpHarmonyLayoutCalculator();
        var bounds = new RectF(0, 0, 300, 150);
        var context = LayoutContext.Horizontal(2.0f, PadreaShape.Square);

        var result = calculator.Calculate(bounds, context);

        Assert.True(result.AllFitWithin(bounds));
    }

    [Fact]
    public void Definition_MatchesCalculator_StandardBounds()
    {
        var calculator = new ArpHarmonyLayoutCalculator();
        var definition = ArpHarmonyLayoutDefinition.Instance;
        var bounds = new RectF(0, 0, 300, 150);
        var context = LayoutContext.Horizontal(2.0f, PadreaShape.Square);

        var calcResult = calculator.Calculate(bounds, context);
        var defResult = definition.Calculate(bounds, context);

        foreach (var name in calcResult.ElementNames)
        {
            var calcRect = calcResult[name];
            var defRect = defResult[name];
            
            Assert.Equal(calcRect.X, defRect.X, precision: 1);
            Assert.Equal(calcRect.Y, defRect.Y, precision: 1);
            Assert.Equal(calcRect.Width, defRect.Width, precision: 1);
            Assert.Equal(calcRect.Height, defRect.Height, precision: 1);
        }
    }

    [Fact]
    public void Definition_MatchesCalculator_WithOffset()
    {
        var calculator = new ArpHarmonyLayoutCalculator();
        var definition = ArpHarmonyLayoutDefinition.Instance;
        var bounds = new RectF(20, 30, 280, 140);
        var context = LayoutContext.Horizontal(2.0f, PadreaShape.Square);

        var calcResult = calculator.Calculate(bounds, context);
        var defResult = definition.Calculate(bounds, context);

        foreach (var name in calcResult.ElementNames)
        {
            var calcRect = calcResult[name];
            var defRect = defResult[name];
            
            Assert.Equal(calcRect.X, defRect.X, precision: 1);
            Assert.Equal(calcRect.Y, defRect.Y, precision: 1);
        }
    }
}

