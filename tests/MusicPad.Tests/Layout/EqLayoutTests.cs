using MusicPad.Core.Layout;

namespace MusicPad.Tests.Layout;

/// <summary>
/// Tests for EQ layout calculator and definition.
/// </summary>
public class EqLayoutTests
{
    // Element names
    private const string Slider0 = "Slider0";
    private const string Slider1 = "Slider1";
    private const string Slider2 = "Slider2";
    private const string Slider3 = "Slider3";

    [Fact]
    public void Calculator_AllFourSlidersAreCreated()
    {
        var calculator = new EqLayoutCalculator();
        var bounds = new RectF(0, 0, 200, 100);
        var context = LayoutContext.Horizontal(2.0f, PadreaShape.Square);

        var result = calculator.Calculate(bounds, context);

        Assert.True(result.HasElement(Slider0));
        Assert.True(result.HasElement(Slider1));
        Assert.True(result.HasElement(Slider2));
        Assert.True(result.HasElement(Slider3));
    }

    [Fact]
    public void Calculator_SlidersAreEvenlySpaced()
    {
        var calculator = new EqLayoutCalculator();
        var bounds = new RectF(0, 0, 200, 100);
        var context = LayoutContext.Horizontal(2.0f, PadreaShape.Square);

        var result = calculator.Calculate(bounds, context);

        var slider0 = result[Slider0];
        var slider1 = result[Slider1];
        var slider2 = result[Slider2];
        var slider3 = result[Slider3];

        // All sliders should have the same width
        Assert.Equal(slider0.Width, slider1.Width);
        Assert.Equal(slider1.Width, slider2.Width);
        Assert.Equal(slider2.Width, slider3.Width);

        // All sliders should have the same Y position
        Assert.Equal(slider0.Y, slider1.Y);
        Assert.Equal(slider1.Y, slider2.Y);
        Assert.Equal(slider2.Y, slider3.Y);

        // Spacing between sliders should be equal
        float spacing01 = slider1.X - slider0.Right;
        float spacing12 = slider2.X - slider1.Right;
        float spacing23 = slider3.X - slider2.Right;
        Assert.Equal(spacing01, spacing12, precision: 1);
        Assert.Equal(spacing12, spacing23, precision: 1);
    }

    [Fact]
    public void Calculator_SlidersCenteredHorizontally()
    {
        var calculator = new EqLayoutCalculator();
        var bounds = new RectF(0, 0, 200, 100);
        var context = LayoutContext.Horizontal(2.0f, PadreaShape.Square);

        var result = calculator.Calculate(bounds, context);

        var slider0 = result[Slider0];
        var slider3 = result[Slider3];

        // Left and right margins should be equal (centered)
        float leftMargin = slider0.X;
        float rightMargin = bounds.Right - slider3.Right;
        Assert.Equal(leftMargin, rightMargin, precision: 1);
    }

    [Fact]
    public void Calculator_SlidersCenteredVertically()
    {
        var calculator = new EqLayoutCalculator();
        var bounds = new RectF(10, 20, 200, 100);
        var context = LayoutContext.Horizontal(2.0f, PadreaShape.Square);

        var result = calculator.Calculate(bounds, context);

        var slider0 = result[Slider0];

        // Slider should be centered vertically in bounds
        float topMargin = slider0.Y - bounds.Y;
        float bottomMargin = bounds.Bottom - slider0.Bottom;
        // Allow some tolerance for label height offset
        Assert.True(Math.Abs(topMargin - bottomMargin) < 20, 
            $"Slider not centered: top={topMargin}, bottom={bottomMargin}");
    }

    [Fact]
    public void Calculator_SlidersAllFitWithinBounds()
    {
        var calculator = new EqLayoutCalculator();
        var bounds = new RectF(0, 0, 200, 100);
        var context = LayoutContext.Horizontal(2.0f, PadreaShape.Square);

        var result = calculator.Calculate(bounds, context);

        Assert.True(result.AllFitWithin(bounds));
    }

    [Fact]
    public void Calculator_SlidersDoNotOverlap()
    {
        var calculator = new EqLayoutCalculator();
        var bounds = new RectF(0, 0, 200, 100);
        var context = LayoutContext.Horizontal(2.0f, PadreaShape.Square);

        var result = calculator.Calculate(bounds, context);

        Assert.False(result.HasOverlaps());
    }

    [Fact]
    public void Calculator_SliderWidthCappedAtMaximum()
    {
        var calculator = new EqLayoutCalculator();
        // Very wide bounds - slider width should be capped
        var bounds = new RectF(0, 0, 500, 100);
        var context = LayoutContext.Horizontal(5.0f, PadreaShape.Square);

        var result = calculator.Calculate(bounds, context);

        var slider0 = result[Slider0];
        Assert.True(slider0.Width <= EqLayoutCalculator.MaxSliderWidth,
            $"Slider width {slider0.Width} exceeds max {EqLayoutCalculator.MaxSliderWidth}");
    }

    [Fact]
    public void Calculator_TrackHeightClampedToRange()
    {
        var calculator = new EqLayoutCalculator();
        var bounds = new RectF(0, 0, 200, 100);
        var context = LayoutContext.Horizontal(2.0f, PadreaShape.Square);

        float trackHeight = EqLayoutCalculator.GetTrackHeight(bounds.Height);

        Assert.True(trackHeight >= EqLayoutCalculator.MinTrackHeight);
        Assert.True(trackHeight <= EqLayoutCalculator.MaxTrackHeight);
    }

    [Fact]
    public void Definition_MatchesCalculator_StandardBounds()
    {
        var calculator = new EqLayoutCalculator();
        var definition = EqLayoutDefinition.Instance;
        var bounds = new RectF(0, 0, 200, 100);
        var context = LayoutContext.Horizontal(2.0f, PadreaShape.Square);

        var calcResult = calculator.Calculate(bounds, context);
        var defResult = definition.Calculate(bounds, context);

        // All elements should match
        foreach (var name in new[] { Slider0, Slider1, Slider2, Slider3 })
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
        var calculator = new EqLayoutCalculator();
        var definition = EqLayoutDefinition.Instance;
        var bounds = new RectF(50, 30, 180, 90);
        var context = LayoutContext.Horizontal(2.0f, PadreaShape.Square);

        var calcResult = calculator.Calculate(bounds, context);
        var defResult = definition.Calculate(bounds, context);

        foreach (var name in new[] { Slider0, Slider1, Slider2, Slider3 })
        {
            var calcRect = calcResult[name];
            var defRect = defResult[name];
            
            Assert.Equal(calcRect.X, defRect.X, precision: 1);
            Assert.Equal(calcRect.Y, defRect.Y, precision: 1);
        }
    }
}

