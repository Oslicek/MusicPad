using MusicPad.Core.Layout;

namespace MusicPad.Tests.Layout;

public class ChorusLayoutCalculatorTests
{
    private readonly ChorusLayoutCalculator _calculator = new();

    [Fact]
    public void Calculate_ReturnsAllElements()
    {
        var bounds = new RectF(0, 0, 400, 100);
        var context = LayoutContext.Horizontal();

        var result = _calculator.Calculate(bounds, context);

        Assert.True(result.HasElement(ChorusLayoutCalculator.OnOffButton));
        Assert.True(result.HasElement(ChorusLayoutCalculator.DepthKnob));
        Assert.True(result.HasElement(ChorusLayoutCalculator.RateKnob));
    }

    [Theory]
    [InlineData(400, 100)]  // Wide landscape
    [InlineData(300, 80)]   // Narrow landscape
    [InlineData(250, 70)]   // Compact landscape
    [InlineData(200, 60)]   // Very compact
    public void Calculate_AllElementsFitWithinBounds(float width, float height)
    {
        var bounds = new RectF(0, 0, width, height);
        var context = LayoutContext.Horizontal();

        var result = _calculator.Calculate(bounds, context);

        Assert.True(result.AllFitWithin(bounds),
            $"Elements should fit within bounds {bounds}");
    }

    [Theory]
    [InlineData(400, 100)]
    [InlineData(300, 80)]
    public void Calculate_NoElementsOverlap(float width, float height)
    {
        var bounds = new RectF(0, 0, width, height);
        var context = LayoutContext.Horizontal();

        var result = _calculator.Calculate(bounds, context);

        Assert.False(result.HasOverlaps(),
            "No elements should overlap");
    }

    [Fact]
    public void Calculate_KnobsAreHorizontallyAligned()
    {
        var bounds = new RectF(0, 0, 400, 100);
        var context = LayoutContext.Horizontal();

        var result = _calculator.Calculate(bounds, context);

        var depthKnob = result[ChorusLayoutCalculator.DepthKnob];
        var rateKnob = result[ChorusLayoutCalculator.RateKnob];

        // Knobs should be at same Y center (horizontally aligned)
        Assert.Equal(depthKnob.CenterY, rateKnob.CenterY, precision: 1);
    }

    [Fact]
    public void Calculate_RateKnobIsRightOfDepthKnob()
    {
        var bounds = new RectF(0, 0, 400, 100);
        var context = LayoutContext.Horizontal();

        var result = _calculator.Calculate(bounds, context);

        var depthKnob = result[ChorusLayoutCalculator.DepthKnob];
        var rateKnob = result[ChorusLayoutCalculator.RateKnob];

        Assert.True(rateKnob.Left > depthKnob.Right,
            "Rate knob should be to the right of Depth knob");
    }

    [Fact]
    public void Calculate_ButtonIsLeftOfKnobs()
    {
        var bounds = new RectF(0, 0, 400, 100);
        var context = LayoutContext.Horizontal();

        var result = _calculator.Calculate(bounds, context);

        var button = result[ChorusLayoutCalculator.OnOffButton];
        var depthKnob = result[ChorusLayoutCalculator.DepthKnob];

        Assert.True(button.Right < depthKnob.Left,
            "Button should be to the left of knobs");
    }

    [Fact]
    public void Calculate_ElementsAreVerticallyCentered()
    {
        var bounds = new RectF(0, 0, 400, 100);
        var context = LayoutContext.Horizontal();

        var result = _calculator.Calculate(bounds, context);

        var button = result[ChorusLayoutCalculator.OnOffButton];
        var depthKnob = result[ChorusLayoutCalculator.DepthKnob];
        var rateKnob = result[ChorusLayoutCalculator.RateKnob];

        float boundsCenter = bounds.CenterY;

        // All elements should be roughly centered vertically
        Assert.InRange(button.CenterY, boundsCenter - 5, boundsCenter + 5);
        Assert.InRange(depthKnob.CenterY, boundsCenter - 5, boundsCenter + 5);
        Assert.InRange(rateKnob.CenterY, boundsCenter - 5, boundsCenter + 5);
    }

    [Fact]
    public void Calculate_WithOffset_ElementsAreProperlyPositioned()
    {
        // Test that bounds with non-zero origin work correctly
        var bounds = new RectF(50, 100, 400, 100);
        var context = LayoutContext.Horizontal();

        var result = _calculator.Calculate(bounds, context);

        Assert.True(result.AllFitWithin(bounds),
            "Elements should fit within offset bounds");

        var button = result[ChorusLayoutCalculator.OnOffButton];
        Assert.True(button.Left >= bounds.Left,
            "Button should respect bounds X offset");
        Assert.True(button.Top >= bounds.Top,
            "Button should respect bounds Y offset");
    }

    [Theory]
    [InlineData(60)]   // Minimum practical height
    [InlineData(80)]   // Standard height
    [InlineData(100)]  // Generous height
    [InlineData(120)]  // Large height
    public void Calculate_KnobSizeScalesWithHeight(float height)
    {
        var bounds = new RectF(0, 0, 400, height);
        var context = LayoutContext.Horizontal();

        var result = _calculator.Calculate(bounds, context);

        var depthKnob = result[ChorusLayoutCalculator.DepthKnob];

        // Knob should not exceed bounds height
        Assert.True(depthKnob.Height <= height,
            $"Knob height {depthKnob.Height} should not exceed bounds height {height}");

        // Knob should be reasonably sized (at least 20% of height for usability)
        Assert.True(depthKnob.Height >= height * 0.2f,
            $"Knob should be at least 20% of bounds height for usability");
    }

    [Fact]
    public void Calculate_ProducesConsistentResults()
    {
        var bounds = new RectF(0, 0, 400, 100);
        var context = LayoutContext.Horizontal();

        var result1 = _calculator.Calculate(bounds, context);
        var result2 = _calculator.Calculate(bounds, context);

        Assert.Equal(result1[ChorusLayoutCalculator.OnOffButton].X,
                     result2[ChorusLayoutCalculator.OnOffButton].X);
        Assert.Equal(result1[ChorusLayoutCalculator.DepthKnob].X,
                     result2[ChorusLayoutCalculator.DepthKnob].X);
        Assert.Equal(result1[ChorusLayoutCalculator.RateKnob].X,
                     result2[ChorusLayoutCalculator.RateKnob].X);
    }
}

