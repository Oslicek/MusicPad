using MusicPad.Core.Sfz;

namespace MusicPad.Tests.Sfz;

public class SfzRegionTests
{
    [Fact]
    public void Matches_SingleKey_ReturnsTrue_WhenNoteEquals()
    {
        var region = new SfzRegion { Key = 60 };
        
        Assert.True(region.Matches(60));
    }

    [Fact]
    public void Matches_SingleKey_ReturnsFalse_WhenNoteDiffers()
    {
        var region = new SfzRegion { Key = 60 };
        
        Assert.False(region.Matches(61));
    }

    [Fact]
    public void Matches_KeyRange_ReturnsTrue_WhenNoteInRange()
    {
        var region = new SfzRegion { LoKey = 48, HiKey = 72 };
        
        Assert.True(region.Matches(60));
        Assert.True(region.Matches(48));
        Assert.True(region.Matches(72));
    }

    [Fact]
    public void Matches_KeyRange_ReturnsFalse_WhenNoteOutOfRange()
    {
        var region = new SfzRegion { LoKey = 48, HiKey = 72 };
        
        Assert.False(region.Matches(47));
        Assert.False(region.Matches(73));
    }

    [Fact]
    public void Matches_VelocityRange_ReturnsTrue_WhenVelocityInRange()
    {
        var region = new SfzRegion { LoKey = 0, HiKey = 127, LoVel = 64, HiVel = 127 };
        
        Assert.True(region.Matches(60, 100));
        Assert.True(region.Matches(60, 64));
        Assert.True(region.Matches(60, 127));
    }

    [Fact]
    public void Matches_VelocityRange_ReturnsFalse_WhenVelocityOutOfRange()
    {
        var region = new SfzRegion { LoKey = 0, HiKey = 127, LoVel = 64, HiVel = 127 };
        
        Assert.False(region.Matches(60, 63));
        Assert.False(region.Matches(60, 0));
    }

    [Fact]
    public void GetPitchRatio_SameAsKeycenter_ReturnsOne()
    {
        var region = new SfzRegion { PitchKeycenter = 60 };
        
        var ratio = region.GetPitchRatio(60);
        
        Assert.Equal(1.0, ratio, precision: 5);
    }

    [Fact]
    public void GetPitchRatio_OctaveUp_ReturnsTwo()
    {
        var region = new SfzRegion { PitchKeycenter = 60 };
        
        var ratio = region.GetPitchRatio(72); // 12 semitones up
        
        Assert.Equal(2.0, ratio, precision: 5);
    }

    [Fact]
    public void GetPitchRatio_OctaveDown_ReturnsHalf()
    {
        var region = new SfzRegion { PitchKeycenter = 60 };
        
        var ratio = region.GetPitchRatio(48); // 12 semitones down
        
        Assert.Equal(0.5, ratio, precision: 5);
    }

    [Fact]
    public void GetPitchRatio_WithTuning_AdjustsRatio()
    {
        var region = new SfzRegion { PitchKeycenter = 60, Tune = 100 }; // +100 cents = +1 semitone
        
        var ratio = region.GetPitchRatio(60);
        
        // Should be same as playing one semitone higher
        var expectedRatio = Math.Pow(2.0, 1.0 / 12.0);
        Assert.Equal(expectedRatio, ratio, precision: 5);
    }

    [Fact]
    public void GetPitchRatio_WithTranspose_AdjustsRatio()
    {
        var region = new SfzRegion { PitchKeycenter = 60, Transpose = 12 }; // +12 semitones
        
        var ratio = region.GetPitchRatio(60);
        
        Assert.Equal(2.0, ratio, precision: 5);
    }
}

