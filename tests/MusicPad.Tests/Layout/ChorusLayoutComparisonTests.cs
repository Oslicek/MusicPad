using MusicPad.Core.Layout;

namespace MusicPad.Tests.Layout;

/// <summary>
/// Tests that verify ChorusLayoutDefinition produces the same results as ChorusLayoutCalculator.
/// This ensures the fluent DSL is a faithful replacement for the imperative calculator.
/// </summary>
public class ChorusLayoutComparisonTests
{
    private readonly ChorusLayoutCalculator _calculator = new();
    private readonly ChorusLayoutDefinition _definition = new();
    
    // Tolerance for floating point comparison
    private const float Tolerance = 0.5f;

    /// <summary>
    /// Tests that Definition matches Calculator for aspect ratios in the "normal" range (1.5-2.4)
    /// where no narrow/wide overrides apply in the Definition.
    /// Note: We explicitly set AspectRatio=2.0 to avoid triggering narrow (<1.3) or wide (>2.5) overrides.
    /// </summary>
    [Theory]
    [InlineData(400, 100)]
    [InlineData(300, 80)]
    [InlineData(250, 70)]
    [InlineData(200, 60)]
    [InlineData(500, 120)]
    public void Calculate_DefinitionMatchesCalculator_Landscape(float width, float height)
    {
        var bounds = new RectF(0, 0, width, height);
        // Use a fixed "normal" aspect ratio to ensure no narrow/wide overrides apply
        // The Definition has overrides for <1.3 and >2.5, so we use 2.0 (in the middle)
        var context = new LayoutContext
        {
            Orientation = Orientation.Landscape,
            AspectRatio = 2.0f,  // Fixed ratio in "normal" range - avoids narrow/wide overrides
            PadreaShape = PadreaShape.Square  // Use Square (default) which has no padding override
        };

        var calculatorResult = _calculator.Calculate(bounds, context);
        var definitionResult = _definition.Calculate(bounds, context);

        AssertLayoutsMatch(calculatorResult, definitionResult, bounds);
    }

    [Theory]
    [InlineData(0, 0, 400, 100)]
    [InlineData(50, 100, 300, 80)]
    [InlineData(100, 50, 250, 70)]
    public void Calculate_DefinitionMatchesCalculator_WithOffset(float x, float y, float width, float height)
    {
        var bounds = new RectF(x, y, width, height);
        // Use fixed aspect ratio in "normal" range to match Calculator behavior
        var context = new LayoutContext
        {
            Orientation = Orientation.Landscape,
            AspectRatio = 2.0f,  // Fixed ratio - avoids narrow/wide overrides
            PadreaShape = PadreaShape.Square
        };

        var calculatorResult = _calculator.Calculate(bounds, context);
        var definitionResult = _definition.Calculate(bounds, context);

        AssertLayoutsMatch(calculatorResult, definitionResult, bounds);
    }

    [Fact]
    public void Calculate_BothProduceSameElements()
    {
        var bounds = new RectF(0, 0, 400, 100);
        var context = LayoutContext.Horizontal(aspectRatio: 4.0f, PadreaShape.Piano);

        var calculatorResult = _calculator.Calculate(bounds, context);
        var definitionResult = _definition.Calculate(bounds, context);

        // Both should have the same element names
        Assert.Equal(
            calculatorResult.ElementNames.OrderBy(n => n),
            definitionResult.ElementNames.OrderBy(n => n));
    }

    [Fact]
    public void Calculate_KnobSizeIsCorrect_StandardHeight()
    {
        var bounds = new RectF(0, 0, 400, 100);
        // Use aspect ratio in "normal" range (1.3-2.5) to avoid narrow/wide overrides
        // Use Square shape to avoid Piano override
        var context = LayoutContext.Horizontal(aspectRatio: 2.0f, PadreaShape.Square);

        var definitionResult = _definition.Calculate(bounds, context);
        var knob = definitionResult[ChorusLayoutDefinition.DepthKnob];

        // Expected: diameter=52, hitPadding=5, so total = 52 + 10 = 62
        // Capped by (height - verticalMargin) = (100 - 16) = 84, so 52 fits
        float expectedSize = 52 + 5 * 2;  // diameter + 2*hitPadding
        Assert.Equal(expectedSize, knob.Width, Tolerance);
        Assert.Equal(expectedSize, knob.Height, Tolerance);
    }

    [Fact]
    public void Calculate_KnobSizeIsCapped_SmallHeight()
    {
        var bounds = new RectF(0, 0, 200, 50);
        // Use aspect ratio in "normal" range (1.3-2.5) to avoid narrow/wide overrides
        var context = LayoutContext.Horizontal(aspectRatio: 2.0f, PadreaShape.Square);

        var definitionResult = _definition.Calculate(bounds, context);
        var knob = definitionResult[ChorusLayoutDefinition.DepthKnob];

        // Height is 50, verticalMargin is 16, so max diameter = 50 - 16 = 34
        // Plus hitPadding*2 = 10, total = 44
        float maxDiameter = bounds.Height - 16;  // 34
        float expectedSize = maxDiameter + 5 * 2;  // 44
        Assert.Equal(expectedSize, knob.Width, Tolerance);
    }

    private void AssertLayoutsMatch(LayoutResult calculator, LayoutResult definition, RectF bounds)
    {
        // Check OnOffButton
        AssertRectMatch(
            calculator[ChorusLayoutCalculator.OnOffButton],
            definition[ChorusLayoutDefinition.OnOffButton],
            "OnOffButton");

        // Check DepthKnob
        AssertRectMatch(
            calculator[ChorusLayoutCalculator.DepthKnob],
            definition[ChorusLayoutDefinition.DepthKnob],
            "DepthKnob");

        // Check RateKnob
        AssertRectMatch(
            calculator[ChorusLayoutCalculator.RateKnob],
            definition[ChorusLayoutDefinition.RateKnob],
            "RateKnob");
    }

    private void AssertRectMatch(RectF expected, RectF actual, string elementName)
    {
        Assert.True(Math.Abs(expected.X - actual.X) <= Tolerance,
            $"{elementName} X mismatch: expected {expected.X}, got {actual.X}");
        Assert.True(Math.Abs(expected.Y - actual.Y) <= Tolerance,
            $"{elementName} Y mismatch: expected {expected.Y}, got {actual.Y}");
        Assert.True(Math.Abs(expected.Width - actual.Width) <= Tolerance,
            $"{elementName} Width mismatch: expected {expected.Width}, got {actual.Width}");
        Assert.True(Math.Abs(expected.Height - actual.Height) <= Tolerance,
            $"{elementName} Height mismatch: expected {expected.Height}, got {actual.Height}");
    }
}

