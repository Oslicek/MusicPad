using MusicPad.Core.Layout;

namespace MusicPad.Tests.Layout;

/// <summary>
/// Tests for LPF layout - both Calculator and Definition.
/// Ensures the DSL produces identical results to the Calculator.
/// </summary>
public class LpfLayoutTests
{
    private readonly LpfLayoutCalculator _calculator = new();
    private readonly LpfLayoutDefinition _definition = new();
    
    private const float Tolerance = 0.5f;

    #region Calculator Tests

    [Fact]
    public void Calculator_ProducesAllElements()
    {
        var bounds = new RectF(0, 0, 200, 80);
        var context = LayoutContext.Horizontal();

        var result = _calculator.Calculate(bounds, context);

        Assert.Contains(LpfLayoutCalculator.OnOffButton, result.ElementNames);
        Assert.Contains(LpfLayoutCalculator.CutoffKnob, result.ElementNames);
        Assert.Contains(LpfLayoutCalculator.ResonanceKnob, result.ElementNames);
    }

    [Fact]
    public void Calculator_KnobsAreHorizontallyAligned()
    {
        var bounds = new RectF(0, 0, 200, 80);
        var context = LayoutContext.Horizontal();

        var result = _calculator.Calculate(bounds, context);

        var cutoff = result[LpfLayoutCalculator.CutoffKnob];
        var resonance = result[LpfLayoutCalculator.ResonanceKnob];

        Assert.Equal(cutoff.Y, resonance.Y, Tolerance);
    }

    [Fact]
    public void Calculator_KnobsAreOrderedLeftToRight()
    {
        var bounds = new RectF(0, 0, 200, 80);
        var context = LayoutContext.Horizontal();

        var result = _calculator.Calculate(bounds, context);

        var button = result[LpfLayoutCalculator.OnOffButton];
        var cutoff = result[LpfLayoutCalculator.CutoffKnob];
        var resonance = result[LpfLayoutCalculator.ResonanceKnob];

        Assert.True(button.Right < cutoff.X, "Button should be left of Cutoff knob");
        Assert.True(cutoff.Right < resonance.X, "Cutoff knob should be left of Resonance knob");
    }

    [Fact]
    public void Calculator_KnobsAreSmallerThanChorus()
    {
        // LPF uses smaller knobs (41 diameter) vs Chorus (52 diameter)
        Assert.True(LpfLayoutCalculator.KnobDiameter < ChorusLayoutCalculator.KnobDiameter,
            "LPF knobs should be smaller than Chorus knobs");
    }

    #endregion

    #region Definition vs Calculator Comparison Tests

    /// <summary>
    /// Tests that Definition matches Calculator for realistic aspect ratios.
    /// </summary>
    [Theory]
    [InlineData(200, 80, PadreaShape.Square)]
    [InlineData(180, 70, PadreaShape.Square)]
    [InlineData(250, 90, PadreaShape.Square)]
    [InlineData(200, 80, PadreaShape.Piano)]
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
    [InlineData(0, 0, 200, 80)]
    [InlineData(50, 100, 180, 70)]
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
        var bounds = new RectF(0, 0, 200, 80);
        var context = LayoutContext.Horizontal();

        var result = _definition.Calculate(bounds, context);

        Assert.True(result.AllFitWithin(bounds), "All elements should fit within bounds");
    }

    [Fact]
    public void Definition_NoElementsOverlap()
    {
        var bounds = new RectF(0, 0, 200, 80);
        var context = LayoutContext.Horizontal();

        var result = _definition.Calculate(bounds, context);

        Assert.False(result.HasOverlaps(), "No elements should overlap");
    }

    #endregion

    private void AssertLayoutsMatch(LayoutResult calculator, LayoutResult definition)
    {
        AssertRectMatch(
            calculator[LpfLayoutCalculator.OnOffButton],
            definition[LpfLayoutDefinition.OnOffButton],
            "OnOffButton");

        AssertRectMatch(
            calculator[LpfLayoutCalculator.CutoffKnob],
            definition[LpfLayoutDefinition.CutoffKnob],
            "CutoffKnob");

        AssertRectMatch(
            calculator[LpfLayoutCalculator.ResonanceKnob],
            definition[LpfLayoutDefinition.ResonanceKnob],
            "ResonanceKnob");
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

