using MusicPad.Core.Layout;

namespace MusicPad.Tests.Layout;

/// <summary>
/// Tests for Reverb layout - both Calculator and Definition.
/// Ensures the DSL produces identical results to the Calculator.
/// </summary>
public class ReverbLayoutTests
{
    private readonly ReverbLayoutCalculator _calculator = new();
    private readonly ReverbLayoutDefinition _definition = new();
    
    private const float Tolerance = 0.5f;

    #region Calculator Tests

    [Fact]
    public void Calculator_ProducesAllElements()
    {
        var bounds = new RectF(0, 0, 400, 100);
        var context = LayoutContext.Horizontal();

        var result = _calculator.Calculate(bounds, context);

        Assert.Contains(ReverbLayoutCalculator.OnOffButton, result.ElementNames);
        Assert.Contains(ReverbLayoutCalculator.LevelKnob, result.ElementNames);
        Assert.Contains(ReverbLayoutCalculator.TypeButton0, result.ElementNames);
        Assert.Contains(ReverbLayoutCalculator.TypeButton1, result.ElementNames);
        Assert.Contains(ReverbLayoutCalculator.TypeButton2, result.ElementNames);
        Assert.Contains(ReverbLayoutCalculator.TypeButton3, result.ElementNames);
    }

    [Fact]
    public void Calculator_TypeButtonsAreHorizontallyAligned()
    {
        var bounds = new RectF(0, 0, 400, 100);
        var context = LayoutContext.Horizontal();

        var result = _calculator.Calculate(bounds, context);

        var btn0 = result[ReverbLayoutCalculator.TypeButton0];
        var btn1 = result[ReverbLayoutCalculator.TypeButton1];
        var btn2 = result[ReverbLayoutCalculator.TypeButton2];
        var btn3 = result[ReverbLayoutCalculator.TypeButton3];

        Assert.Equal(btn0.Y, btn1.Y, Tolerance);
        Assert.Equal(btn1.Y, btn2.Y, Tolerance);
        Assert.Equal(btn2.Y, btn3.Y, Tolerance);
    }

    [Fact]
    public void Calculator_ElementsAreOrderedLeftToRight()
    {
        var bounds = new RectF(0, 0, 400, 100);
        var context = LayoutContext.Horizontal();

        var result = _calculator.Calculate(bounds, context);

        var button = result[ReverbLayoutCalculator.OnOffButton];
        var knob = result[ReverbLayoutCalculator.LevelKnob];
        var type0 = result[ReverbLayoutCalculator.TypeButton0];
        var type3 = result[ReverbLayoutCalculator.TypeButton3];

        Assert.True(button.Right < knob.X, "Button should be left of Level knob");
        Assert.True(knob.Right < type0.X, "Level knob should be left of type buttons");
        Assert.True(type0.X < type3.X, "Type buttons should be ordered left to right");
    }

    #endregion

    #region Definition vs Calculator Comparison Tests

    /// <summary>
    /// Tests that Definition matches Calculator for realistic aspect ratios.
    /// </summary>
    [Theory]
    [InlineData(400, 100, PadreaShape.Square)]
    [InlineData(350, 90, PadreaShape.Square)]
    [InlineData(450, 110, PadreaShape.Square)]
    [InlineData(400, 100, PadreaShape.Piano)]
    public void Definition_MatchesCalculator_RealisticAspectRatios(
        float width, float height, PadreaShape shape)
    {
        var bounds = new RectF(0, 0, width, height);
        var context = LayoutContext.FromBounds(bounds, shape);

        var calculatorResult = _calculator.Calculate(bounds, context);
        var definitionResult = _definition.Calculate(bounds, context);

        AssertLayoutsMatch(calculatorResult, definitionResult);
    }

    [Theory]
    [InlineData(0, 0, 400, 100)]
    [InlineData(50, 100, 350, 90)]
    public void Definition_MatchesCalculator_WithOffset(float x, float y, float width, float height)
    {
        var bounds = new RectF(x, y, width, height);
        var context = LayoutContext.FromBounds(bounds, PadreaShape.Square);

        var calculatorResult = _calculator.Calculate(bounds, context);
        var definitionResult = _definition.Calculate(bounds, context);

        AssertLayoutsMatch(calculatorResult, definitionResult);
    }

    #endregion

    #region Definition-specific Tests

    [Fact]
    public void Definition_AllElementsFitWithinBounds()
    {
        var bounds = new RectF(0, 0, 400, 100);
        var context = LayoutContext.Horizontal();

        var result = _definition.Calculate(bounds, context);

        Assert.True(result.AllFitWithin(bounds), "All elements should fit within bounds");
    }

    [Fact]
    public void Definition_NoElementsOverlap()
    {
        var bounds = new RectF(0, 0, 400, 100);
        var context = LayoutContext.Horizontal();

        var result = _definition.Calculate(bounds, context);

        Assert.False(result.HasOverlaps(), "No elements should overlap");
    }

    #endregion

    private void AssertLayoutsMatch(LayoutResult calculator, LayoutResult definition)
    {
        AssertRectMatch(
            calculator[ReverbLayoutCalculator.OnOffButton],
            definition[ReverbLayoutDefinition.OnOffButton],
            "OnOffButton");

        AssertRectMatch(
            calculator[ReverbLayoutCalculator.LevelKnob],
            definition[ReverbLayoutDefinition.LevelKnob],
            "LevelKnob");

        for (int i = 0; i < 4; i++)
        {
            AssertRectMatch(
                calculator[$"TypeButton{i}"],
                definition[$"TypeButton{i}"],
                $"TypeButton{i}");
        }
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

