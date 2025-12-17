using MusicPad.Core.Models;

namespace MusicPad.Tests.Models;

public class PianoRangeManagerTests
{
    [Fact]
    public void Portrait_DefaultRange_IsC3ToC4()
    {
        var mgr = new PianoRangeManager(21, 108, isLandscape: false);
        var (start, end) = mgr.GetRange();
        Assert.Equal(48, start); // C3
        Assert.Equal(60, end);   // C4
    }

    [Fact]
    public void Landscape_DefaultRange_IsC2ToC4()
    {
        var mgr = new PianoRangeManager(21, 108, isLandscape: true);
        var (start, end) = mgr.GetRange();
        Assert.Equal(36, start); // C2
        Assert.Equal(60, end);   // C4
    }

    [Fact]
    public void Clamps_ToInstrumentMax()
    {
        var mgr = new PianoRangeManager(40, 50, isLandscape: false); // narrow instrument range
        var (start, end) = mgr.GetRange();
        Assert.Equal(40, start);
        Assert.Equal(50, end); // truncated span
    }

    [Fact]
    public void Move_UpAndDown_IsClamped()
    {
        var mgr = new PianoRangeManager(36, 72, isLandscape: false); // span 13
        mgr.Move(12); // up an octave
        var (start, end) = mgr.GetRange();
        Assert.Equal(60, start); // C5 candidate
        Assert.Equal(72, end);   // clamped to instrument max

        mgr.Move(-24); // down 2 octaves, should clamp to min (36)
        (start, end) = mgr.GetRange();
        Assert.Equal(36, start);
        Assert.Equal(48, end);
    }

    [Fact]
    public void SetOrientation_PreservesOrResetsStart()
    {
        var mgr = new PianoRangeManager(21, 108, isLandscape: false);
        mgr.Move(12); // move up to start at 60
        var (start, _) = mgr.GetRange();
        Assert.Equal(60, start);

        // Preserve start when switching orientation
        mgr.SetOrientation(isLandscape: true, preserveStart: true);
        (start, _) = mgr.GetRange();
        Assert.True(start >= 60 - 12); // clamped but not reset to default

        // Reset when requested
        mgr.SetOrientation(isLandscape: false, preserveStart: false);
        (start, _) = mgr.GetRange();
        Assert.Equal(48, start); // default portrait start C3
    }
}

