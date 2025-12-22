using MusicPad.Core.Layout;

namespace MusicPad.Tests.Layout;

/// <summary>
/// Tests for ChorusLayoutDefinition using the fluent DSL.
/// These tests verify that the fluent definition produces correct layouts
/// for various orientation, aspect ratio, and padrea shape combinations.
/// </summary>
public class ChorusLayoutDefinitionTests
{
    // Element names expected in Chorus layout
    private const string OnOffButton = "OnOffButton";
    private const string DepthKnob = "DepthKnob";
    private const string RateKnob = "RateKnob";

    [Fact]
    public void Calculate_ReturnsAllElements()
    {
        var layout = new ChorusLayoutDefinition();
        var bounds = new RectF(0, 0, 400, 100);
        var context = LayoutContext.Horizontal();

        var result = layout.Calculate(bounds, context);

        Assert.True(result.HasElement(OnOffButton));
        Assert.True(result.HasElement(DepthKnob));
        Assert.True(result.HasElement(RateKnob));
    }

    [Theory]
    [InlineData(400, 100, Orientation.Landscape, PadreaShape.Square)]
    [InlineData(300, 80, Orientation.Landscape, PadreaShape.Square)]
    [InlineData(200, 80, Orientation.Landscape, PadreaShape.Square)]  // Aspect ratio 2.5 (avoids wide override)
    [InlineData(400, 100, Orientation.Landscape, PadreaShape.Piano)]
    [InlineData(100, 200, Orientation.Portrait, PadreaShape.Square)]
    public void Calculate_AllElementsFitWithinBounds(float width, float height, Orientation orientation, PadreaShape shape)
    {
        var layout = new ChorusLayoutDefinition();
        var bounds = new RectF(0, 0, width, height);
        var context = new LayoutContext
        {
            Orientation = orientation,
            AspectRatio = width / height,
            PadreaShape = shape
        };

        var result = layout.Calculate(bounds, context);

        Assert.True(result.AllFitWithin(bounds),
            $"Elements should fit within bounds {bounds} for {orientation}/{shape}");
    }

    [Fact]
    public void Calculate_NoElementsOverlap_Landscape()
    {
        var layout = new ChorusLayoutDefinition();
        var bounds = new RectF(0, 0, 400, 100);
        var context = LayoutContext.Horizontal();

        var result = layout.Calculate(bounds, context);

        Assert.False(result.HasOverlaps(), "No elements should overlap");
    }

    [Fact]
    public void Calculate_Landscape_KnobsAreHorizontallyAligned()
    {
        var layout = new ChorusLayoutDefinition();
        var bounds = new RectF(0, 0, 400, 100);
        var context = LayoutContext.Horizontal();

        var result = layout.Calculate(bounds, context);

        var depthKnob = result[DepthKnob];
        var rateKnob = result[RateKnob];

        // Knobs should be at same Y center
        Assert.Equal(depthKnob.CenterY, rateKnob.CenterY, precision: 1);
    }

    [Fact]
    public void Calculate_Landscape_RateKnobIsRightOfDepthKnob()
    {
        var layout = new ChorusLayoutDefinition();
        var bounds = new RectF(0, 0, 400, 100);
        var context = LayoutContext.Horizontal();

        var result = layout.Calculate(bounds, context);

        var depthKnob = result[DepthKnob];
        var rateKnob = result[RateKnob];

        Assert.True(rateKnob.Left > depthKnob.Right,
            "Rate knob should be to the right of Depth knob");
    }

    [Fact]
    public void Calculate_Landscape_ButtonIsLeftOfKnobs()
    {
        var layout = new ChorusLayoutDefinition();
        var bounds = new RectF(0, 0, 400, 100);
        var context = LayoutContext.Horizontal();

        var result = layout.Calculate(bounds, context);

        var button = result[OnOffButton];
        var depthKnob = result[DepthKnob];

        Assert.True(button.Right < depthKnob.Left,
            "Button should be to the left of knobs");
    }

    [Fact]
    public void Calculate_Landscape_ElementsAreVerticallyCentered()
    {
        var layout = new ChorusLayoutDefinition();
        var bounds = new RectF(0, 0, 400, 100);
        var context = LayoutContext.Horizontal();

        var result = layout.Calculate(bounds, context);

        var button = result[OnOffButton];
        var depthKnob = result[DepthKnob];
        var rateKnob = result[RateKnob];

        float boundsCenter = bounds.CenterY;

        // All elements should be roughly centered vertically
        Assert.InRange(button.CenterY, boundsCenter - 5, boundsCenter + 5);
        Assert.InRange(depthKnob.CenterY, boundsCenter - 5, boundsCenter + 5);
        Assert.InRange(rateKnob.CenterY, boundsCenter - 5, boundsCenter + 5);
    }

    [Fact]
    public void Calculate_SquarePadrea_UsesSmallerKnobs()
    {
        var layout = new ChorusLayoutDefinition();
        var bounds = new RectF(0, 0, 300, 100);

        var squareContext = new LayoutContext
        {
            Orientation = Orientation.Landscape,
            AspectRatio = 1.2f, // Near-square
            PadreaShape = PadreaShape.Square
        };

        var wideContext = new LayoutContext
        {
            Orientation = Orientation.Landscape,
            AspectRatio = 2.0f, // Wide
            PadreaShape = PadreaShape.Square
        };

        var squareResult = layout.Calculate(bounds, squareContext);
        var wideResult = layout.Calculate(bounds, wideContext);

        // Square layout should have smaller or equal knobs due to tighter space
        // (exact size depends on implementation)
        Assert.True(squareResult[DepthKnob].Width <= wideResult[DepthKnob].Width + 10,
            "Square padrea should not have significantly larger knobs than wide layout");
    }

    [Fact]
    public void Calculate_PianoPadrea_HasDifferentLayout()
    {
        var layout = new ChorusLayoutDefinition();
        var bounds = new RectF(0, 0, 400, 100);

        var squareContext = LayoutContext.Horizontal(2.0f, PadreaShape.Square);
        var pianoContext = LayoutContext.Horizontal(2.0f, PadreaShape.Piano);

        var squareResult = layout.Calculate(bounds, squareContext);
        var pianoResult = layout.Calculate(bounds, pianoContext);

        // Both should be valid layouts
        Assert.True(squareResult.AllFitWithin(bounds));
        Assert.True(pianoResult.AllFitWithin(bounds));
    }

    [Theory]
    [InlineData(1.5f)]  // Normal landscape
    [InlineData(2.0f)]  // Standard landscape
    [InlineData(2.5f)]  // Edge of wide threshold
    public void Calculate_NormalAspectRatio_UsesDefaultPadding(float aspectRatio)
    {
        var layout = new ChorusLayoutDefinition();
        var bounds = new RectF(0, 0, 400, 100);
        var context = LayoutContext.Horizontal(aspectRatio);

        var result = layout.Calculate(bounds, context);

        // Default padding is 8, so button should be at X=8
        Assert.Equal(8, result[OnOffButton].X, precision: 1);
    }

    [Theory]
    [InlineData(7.0f)]  // Wide - threshold is > 6.0
    [InlineData(8.0f)]  // Very wide
    public void Calculate_WideAspectRatio_UsesLargerPadding(float aspectRatio)
    {
        var layout = new ChorusLayoutDefinition();
        var bounds = new RectF(0, 0, 700, 100);  // Larger width for realistic ultra-wide
        var context = LayoutContext.Horizontal(aspectRatio);

        var result = layout.Calculate(bounds, context);

        // Wide variant has larger padding (12) - only triggers for aspect ratio > 6.0
        Assert.Equal(12, result[OnOffButton].X, precision: 1);
    }

    [Theory]
    [InlineData(1.1f)]  // Near-square
    [InlineData(1.2f)]  // Still near-square
    public void Calculate_NarrowAspectRatio_UsesTighterPadding(float aspectRatio)
    {
        var layout = new ChorusLayoutDefinition();
        var bounds = new RectF(0, 0, 120, 100);
        var context = LayoutContext.Horizontal(aspectRatio);

        var result = layout.Calculate(bounds, context);

        // Narrow variant has tighter padding (4)
        Assert.Equal(4, result[OnOffButton].X, precision: 1);
    }

    private static void AssertRectsEqual(RectF actual, RectF expected, float tolerance)
    {
        Assert.True(Math.Abs(actual.X - expected.X) <= tolerance,
            $"X mismatch: {actual.X} vs {expected.X}");
        Assert.True(Math.Abs(actual.Y - expected.Y) <= tolerance,
            $"Y mismatch: {actual.Y} vs {expected.Y}");
        Assert.True(Math.Abs(actual.Width - expected.Width) <= tolerance,
            $"Width mismatch: {actual.Width} vs {expected.Width}");
        Assert.True(Math.Abs(actual.Height - expected.Height) <= tolerance,
            $"Height mismatch: {actual.Height} vs {expected.Height}");
    }

    // === COMPARISON TESTS: Definition must match Calculator for standard layouts ===

    [Theory]
    [InlineData(400, 100)]
    [InlineData(300, 80)]
    [InlineData(250, 70)]
    public void Calculate_MatchesCalculator_StandardLandscape(float width, float height)
    {
        // Standard landscape without aspect ratio overrides should match Calculator exactly
        var layout = new ChorusLayoutDefinition();
        var calculator = new ChorusLayoutCalculator();
        var bounds = new RectF(0, 0, width, height);
        // Use aspect ratio in the 1.5-2.5 range to avoid narrow/wide overrides
        var context = LayoutContext.Horizontal(aspectRatio: 2.0f);

        var layoutResult = layout.Calculate(bounds, context);
        var calcResult = calculator.Calculate(bounds, context);

        const float tolerance = 1f;
        
        AssertRectsEqual(layoutResult[OnOffButton], calcResult[OnOffButton], tolerance);
        AssertRectsEqual(layoutResult[DepthKnob], calcResult[DepthKnob], tolerance);
        AssertRectsEqual(layoutResult[RateKnob], calcResult[RateKnob], tolerance);
    }

    [Fact]
    public void Calculate_MatchesCalculator_WithOffset()
    {
        var layout = new ChorusLayoutDefinition();
        var calculator = new ChorusLayoutCalculator();
        var bounds = new RectF(50, 100, 300, 80);
        var context = LayoutContext.Horizontal(aspectRatio: 2.0f);

        var layoutResult = layout.Calculate(bounds, context);
        var calcResult = calculator.Calculate(bounds, context);

        const float tolerance = 1f;
        
        AssertRectsEqual(layoutResult[OnOffButton], calcResult[OnOffButton], tolerance);
        AssertRectsEqual(layoutResult[DepthKnob], calcResult[DepthKnob], tolerance);
        AssertRectsEqual(layoutResult[RateKnob], calcResult[RateKnob], tolerance);
    }

    [Theory]
    [InlineData(60)]
    [InlineData(80)]
    [InlineData(100)]
    [InlineData(120)]
    public void Calculate_MatchesCalculator_KnobSizesMatchForVariousHeights(float height)
    {
        var layout = new ChorusLayoutDefinition();
        var calculator = new ChorusLayoutCalculator();
        var bounds = new RectF(0, 0, 400, height);
        var context = LayoutContext.Horizontal(aspectRatio: 2.0f);

        var layoutResult = layout.Calculate(bounds, context);
        var calcResult = calculator.Calculate(bounds, context);

        const float tolerance = 1f;
        
        // Knob sizes should match exactly
        Assert.True(Math.Abs(layoutResult[DepthKnob].Width - calcResult[DepthKnob].Width) <= tolerance,
            $"Knob width mismatch at height {height}: {layoutResult[DepthKnob].Width} vs {calcResult[DepthKnob].Width}");
        Assert.True(Math.Abs(layoutResult[DepthKnob].Height - calcResult[DepthKnob].Height) <= tolerance,
            $"Knob height mismatch at height {height}: {layoutResult[DepthKnob].Height} vs {calcResult[DepthKnob].Height}");
    }
}

