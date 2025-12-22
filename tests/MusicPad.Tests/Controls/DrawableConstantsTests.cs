using MusicPad.Core.Drawing;

namespace MusicPad.Tests.Controls;

/// <summary>
/// Tests for shared drawable constants.
/// </summary>
public class DrawableConstantsTests
{
    [Fact]
    public void KnobAngles_AreValid()
    {
        // Knob starts at 7 o'clock (225°) and ends at 5 o'clock (-45° or 315°)
        Assert.Equal(225f, DrawableConstants.KnobMinAngle);
        Assert.Equal(-45f, DrawableConstants.KnobMaxAngle);
    }

    [Fact]
    public void GetTotalKnobAngle_ReturnsNegativeValue()
    {
        // Total rotation should be negative (clockwise)
        float total = DrawableConstants.GetTotalKnobAngle();
        Assert.True(total < 0, $"Total angle should be negative, got {total}");
    }

    [Fact]
    public void GetTotalKnobAngle_ReturnsCorrectRange()
    {
        // From 225° to -45° is 270° of rotation
        float total = DrawableConstants.GetTotalKnobAngle();
        Assert.Equal(-270f, total);
    }

    [Fact]
    public void ToggleRatios_AreWithinValidRange()
    {
        Assert.True(DrawableConstants.ToggleWidthRatio > 0 && DrawableConstants.ToggleWidthRatio <= 1);
        Assert.True(DrawableConstants.ToggleHeightRatio > 0 && DrawableConstants.ToggleHeightRatio <= 1);
        Assert.True(DrawableConstants.ToggleKnobRatio > 0 && DrawableConstants.ToggleKnobRatio <= 1);
    }

    [Fact]
    public void KnobProportions_AreWithinValidRange()
    {
        Assert.True(DrawableConstants.KnobHighlightOffset > 0 && DrawableConstants.KnobHighlightOffset < 1);
        Assert.True(DrawableConstants.KnobHighlightRadius > 0 && DrawableConstants.KnobHighlightRadius < 1);
        Assert.True(DrawableConstants.KnobInnerRadius > 0 && DrawableConstants.KnobInnerRadius < 1);
        Assert.True(DrawableConstants.KnobIndicatorDistance > 0 && DrawableConstants.KnobIndicatorDistance < 1);
        Assert.True(DrawableConstants.KnobIndicatorRadius > 0 && DrawableConstants.KnobIndicatorRadius < 1);
    }

    [Fact]
    public void KnobHitPadding_MatchesLayoutConstants()
    {
        // This constant must match the layout calculators
        Assert.Equal(5f, DrawableConstants.KnobHitPadding);
    }

    [Fact]
    public void MarkerConstants_ArePositive()
    {
        Assert.True(DrawableConstants.KnobMarkerCount > 0);
        Assert.True(DrawableConstants.KnobMarkerStrokeWidth > 0);
        Assert.True(DrawableConstants.KnobMarkerOuterOffset > 0);
        Assert.True(DrawableConstants.KnobMarkerInnerOffset >= 0);
    }
}

