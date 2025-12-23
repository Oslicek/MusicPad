using MusicPad.Core.Layout;

namespace MusicPad.Tests.Layout;

/// <summary>
/// Tests for Delay layout - both Calculator and Definition.
/// Ensures the DSL produces identical results to the Calculator.
/// </summary>
public class DelayLayoutTests
{
    private readonly DelayLayoutCalculator _calculator = new();
    private readonly DelayLayoutDefinition _definition = new();
    
    private const float Tolerance = 0.5f;

    #region Calculator Tests

    [Fact]
    public void Calculator_ProducesAllElements()
    {
        var bounds = new RectF(0, 0, 400, 100);
        var context = LayoutContext.Horizontal();

        var result = _calculator.Calculate(bounds, context);

        Assert.Contains(DelayLayoutCalculator.OnOffButton, result.ElementNames);
        Assert.Contains(DelayLayoutCalculator.TimeKnob, result.ElementNames);
        Assert.Contains(DelayLayoutCalculator.FeedbackKnob, result.ElementNames);
        Assert.Contains(DelayLayoutCalculator.LevelKnob, result.ElementNames);
    }

    [Fact]
    public void Calculator_KnobsAreHorizontallyAligned()
    {
        var bounds = new RectF(0, 0, 400, 100);
        var context = LayoutContext.Horizontal();

        var result = _calculator.Calculate(bounds, context);

        var time = result[DelayLayoutCalculator.TimeKnob];
        var feedback = result[DelayLayoutCalculator.FeedbackKnob];
        var level = result[DelayLayoutCalculator.LevelKnob];

        Assert.Equal(time.Y, feedback.Y, Tolerance);
        Assert.Equal(feedback.Y, level.Y, Tolerance);
    }

    [Fact]
    public void Calculator_KnobsAreOrderedLeftToRight()
    {
        var bounds = new RectF(0, 0, 400, 100);
        var context = LayoutContext.Horizontal();

        var result = _calculator.Calculate(bounds, context);

        var button = result[DelayLayoutCalculator.OnOffButton];
        var time = result[DelayLayoutCalculator.TimeKnob];
        var feedback = result[DelayLayoutCalculator.FeedbackKnob];
        var level = result[DelayLayoutCalculator.LevelKnob];

        Assert.True(button.Right < time.X, "Button should be left of Time knob");
        Assert.True(time.Right < feedback.X, "Time knob should be left of Feedback knob");
        Assert.True(feedback.Right < level.X, "Feedback knob should be left of Level knob");
    }

    #endregion

    #region Definition vs Calculator Comparison Tests

    /// <summary>
    /// Tests that Definition matches Calculator for realistic aspect ratios.
    /// </summary>
    [Theory]
    [InlineData(400, 100, PadreaShape.Square)]
    [InlineData(300, 80, PadreaShape.Square)]
    [InlineData(500, 120, PadreaShape.Square)]
    [InlineData(400, 100, PadreaShape.Piano)]
    [InlineData(350, 90, PadreaShape.Piano)]
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
    [InlineData(50, 100, 300, 80)]
    [InlineData(100, 50, 250, 70)]
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
            calculator[DelayLayoutCalculator.OnOffButton],
            definition[DelayLayoutDefinition.OnOffButton],
            "OnOffButton");

        AssertRectMatch(
            calculator[DelayLayoutCalculator.TimeKnob],
            definition[DelayLayoutDefinition.TimeKnob],
            "TimeKnob");

        AssertRectMatch(
            calculator[DelayLayoutCalculator.FeedbackKnob],
            definition[DelayLayoutDefinition.FeedbackKnob],
            "FeedbackKnob");

        AssertRectMatch(
            calculator[DelayLayoutCalculator.LevelKnob],
            definition[DelayLayoutDefinition.LevelKnob],
            "LevelKnob");
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




