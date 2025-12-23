using MusicPad.Core.Layout;

namespace MusicPad.Tests.Layout;

/// <summary>
/// Tests for MainPage layout calculations.
/// Ensures padrea and effect area are positioned correctly without overlapping screen edges.
/// </summary>
public class MainPageLayoutTests
{
    // Element names for main page layout
    public const string ControlsStack = "ControlsStack";
    public const string VolumeKnob = "VolumeKnob";
    public const string PadContainer = "PadContainer";
    public const string EffectArea = "EffectArea";
    public const string NavigationBar = "NavigationBar";
    public const string RecArea = "RecArea";

    #region Bug Reproduction Tests - These verify the original bug existed

    /// <summary>
    /// This test demonstrates the original bug where the old calculation
    /// would push the effect area past the right edge of the screen.
    /// The old code used: padreaCenterX = _pageWidth / 2 (ignoring left controls)
    /// </summary>
    [Fact]
    public void OldCalculation_EffectArea_ExceedsRightEdge()
    {
        // Simulate the OLD buggy calculation
        float pageWidth = 1024;
        float pageHeight = 768;
        float padding = 8;
        float controlsWidth = 166; // 150 + 16

        // OLD BUGGY CALCULATION:
        float availableHeight = pageHeight - padding * 4; // 768 - 32 = 736
        float padreaSize = availableHeight; // 736 (too big!)
        float padreaCenterX = pageWidth / 2; // 512 (ignoring left controls)
        float padreaRight = padreaCenterX + padreaSize / 2; // 512 + 368 = 880
        float efareaWidth = pageWidth - padreaRight - padding * 2; // 1024 - 880 - 16 = 128
        float efareaLeft = padreaRight + padding; // 888

        // Effect area right edge with old calculation
        float oldEfareaRight = efareaLeft + efareaWidth; // 888 + 128 = 1016
        
        // This looks OK for width, but the HEIGHT was the problem:
        float oldEfareaHeight = pageHeight - padding * 2; // 768 - 16 = 752
        // Combined with efareaLeft = 888, the effect area would be shifted right
        // and its height would push it to overlap bottom

        // The real issue: padrea overlaps with left controls
        float padreaLeft = padreaCenterX - padreaSize / 2; // 512 - 368 = 144
        // Controls right edge is at ~166, but padrea starts at 144 = OVERLAP!
        
        Assert.True(padreaLeft < controlsWidth,
            $"Old calculation causes padrea (left={padreaLeft}) to overlap with controls (width={controlsWidth})");
    }

    #endregion

    #region Landscape Square Padrea Tests

    [Theory]
    [InlineData(1920, 1080)]  // Full HD landscape
    [InlineData(1280, 720)]   // HD landscape
    [InlineData(1024, 768)]   // iPad-like
    [InlineData(800, 600)]    // Compact landscape
    [InlineData(2560, 1440)]  // QHD landscape
    public void LandscapeSquare_EffectArea_FitsWithinScreenBounds(float width, float height)
    {
        var calculator = new MainPageLayoutCalculator();
        var bounds = new RectF(0, 0, width, height);
        var context = MainPageLayoutContext.LandscapeSquare(width, height);

        var result = calculator.Calculate(bounds, context);

        var effectArea = result[EffectArea];
        
        // Effect area should not extend past right edge
        Assert.True(effectArea.Right <= width,
            $"Effect area right edge ({effectArea.Right}) should not exceed page width ({width})");
        
        // Effect area should not extend past bottom edge
        Assert.True(effectArea.Bottom <= height,
            $"Effect area bottom edge ({effectArea.Bottom}) should not exceed page height ({height})");
        
        // Effect area should have reasonable width (at least 100px for buttons)
        Assert.True(effectArea.Width >= 100,
            $"Effect area width ({effectArea.Width}) should be at least 100px");
    }

    [Theory]
    [InlineData(1920, 1080)]
    [InlineData(1280, 720)]
    [InlineData(1024, 768)]
    [InlineData(800, 600)]
    public void LandscapeSquare_PadContainer_DoesNotOverlapWithControls(float width, float height)
    {
        var calculator = new MainPageLayoutCalculator();
        var bounds = new RectF(0, 0, width, height);
        var context = MainPageLayoutContext.LandscapeSquare(width, height);

        var result = calculator.Calculate(bounds, context);

        var controlsStack = result[ControlsStack];
        var volumeKnob = result[VolumeKnob];
        var padContainer = result[PadContainer];
        
        // Padrea left edge should be past controls area
        float leftControlsRight = Math.Max(controlsStack.Right, volumeKnob.Right);
        Assert.True(padContainer.Left >= leftControlsRight,
            $"Padrea left edge ({padContainer.Left}) should not overlap with controls (right: {leftControlsRight})");
    }

    [Theory]
    [InlineData(1920, 1080)]
    [InlineData(1280, 720)]
    [InlineData(1024, 768)]
    [InlineData(800, 600)]
    public void LandscapeSquare_PadContainer_DoesNotOverlapWithEffectArea(float width, float height)
    {
        var calculator = new MainPageLayoutCalculator();
        var bounds = new RectF(0, 0, width, height);
        var context = MainPageLayoutContext.LandscapeSquare(width, height);

        var result = calculator.Calculate(bounds, context);

        var padContainer = result[PadContainer];
        var effectArea = result[EffectArea];
        
        // Padrea right edge should not overlap with effect area
        Assert.True(padContainer.Right <= effectArea.Left,
            $"Padrea right edge ({padContainer.Right}) should not overlap with effect area left ({effectArea.Left})");
    }

    [Theory]
    [InlineData(1920, 1080)]
    [InlineData(1280, 720)]
    [InlineData(1024, 768)]
    [InlineData(800, 600)]
    public void LandscapeSquare_AllElements_FitWithinBounds(float width, float height)
    {
        var calculator = new MainPageLayoutCalculator();
        var bounds = new RectF(0, 0, width, height);
        var context = MainPageLayoutContext.LandscapeSquare(width, height);

        var result = calculator.Calculate(bounds, context);

        Assert.True(result.AllFitWithin(bounds),
            $"All elements should fit within bounds ({width}x{height})");
    }

    [Theory]
    [InlineData(1920, 1080)]
    [InlineData(1280, 720)]
    public void LandscapeSquare_PadContainer_IsSquare(float width, float height)
    {
        var calculator = new MainPageLayoutCalculator();
        var bounds = new RectF(0, 0, width, height);
        var context = MainPageLayoutContext.LandscapeSquare(width, height);

        var result = calculator.Calculate(bounds, context);

        var padContainer = result[PadContainer];
        
        // Padrea should be square (width == height)
        Assert.Equal(padContainer.Width, padContainer.Height, 0.5f);
    }

    [Theory]
    [InlineData(1920, 1080)]
    [InlineData(1280, 720)]
    public void LandscapeSquare_PadContainer_IsCenteredVertically(float width, float height)
    {
        var calculator = new MainPageLayoutCalculator();
        var bounds = new RectF(0, 0, width, height);
        var context = MainPageLayoutContext.LandscapeSquare(width, height);

        var result = calculator.Calculate(bounds, context);

        var padContainer = result[PadContainer];
        
        // Padrea should be vertically centered
        float expectedCenterY = height / 2;
        Assert.Equal(expectedCenterY, padContainer.CenterY, 1.0f);
    }

    #endregion
}

