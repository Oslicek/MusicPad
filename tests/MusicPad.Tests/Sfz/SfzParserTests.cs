using MusicPad.Core.Sfz;

namespace MusicPad.Tests.Sfz;

public class SfzParserTests
{
    [Fact]
    public void Parse_EmptyString_ReturnsEmptyInstrument()
    {
        var instrument = SfzParser.Parse("");
        
        Assert.Empty(instrument.Regions);
    }

    [Fact]
    public void Parse_GlobalSample_SetsDefaultSample()
    {
        var sfz = @"
<global>
sample=sf2_smpl.wav
";
        var instrument = SfzParser.Parse(sfz);
        
        Assert.Equal("sf2_smpl.wav", instrument.DefaultSample);
    }

    [Fact]
    public void Parse_SingleRegion_CreatesOneRegion()
    {
        var sfz = @"
<region>
lokey=60 hikey=72
sample=piano.wav
";
        var instrument = SfzParser.Parse(sfz);
        
        Assert.Single(instrument.Regions);
        Assert.Equal(60, instrument.Regions[0].LoKey);
        Assert.Equal(72, instrument.Regions[0].HiKey);
        Assert.Equal("piano.wav", instrument.Regions[0].Sample);
    }

    [Fact]
    public void Parse_RegionWithKey_SetsBothLoKeyAndHiKey()
    {
        var sfz = @"
<region>
key=45
";
        var instrument = SfzParser.Parse(sfz);
        
        Assert.Single(instrument.Regions);
        Assert.Equal(45, instrument.Regions[0].Key);
        Assert.True(instrument.Regions[0].Matches(45));
        Assert.False(instrument.Regions[0].Matches(46));
    }

    [Fact]
    public void Parse_RegionWithOffsetAndEnd_ParsesSampleSlice()
    {
        var sfz = @"
<region>
offset=1000 end=5000
";
        var instrument = SfzParser.Parse(sfz);
        
        Assert.Equal(1000, instrument.Regions[0].Offset);
        Assert.Equal(5000, instrument.Regions[0].End);
    }

    [Fact]
    public void Parse_RegionWithPitchKeycenter_ParsesRootKey()
    {
        var sfz = @"
<region>
pitch_keycenter=72
";
        var instrument = SfzParser.Parse(sfz);
        
        Assert.Equal(72, instrument.Regions[0].PitchKeycenter);
    }

    [Fact]
    public void Parse_RegionWithLoopMode_ParsesLoopSettings()
    {
        var sfz = @"
<region>
loop_mode=loop_continuous
loop_start=100
loop_end=2000
";
        var instrument = SfzParser.Parse(sfz);
        
        Assert.Equal(LoopMode.LoopContinuous, instrument.Regions[0].LoopMode);
        Assert.Equal(100, instrument.Regions[0].LoopStart);
        Assert.Equal(2000, instrument.Regions[0].LoopEnd);
    }

    [Fact]
    public void Parse_RegionWithVolume_ParsesVolumeDb()
    {
        var sfz = @"
<region>
volume=-6
";
        var instrument = SfzParser.Parse(sfz);
        
        Assert.Equal(-6f, instrument.Regions[0].Volume);
    }

    [Fact]
    public void Parse_RegionWithPan_ParsesPanning()
    {
        var sfz = @"
<region>
pan=-100
";
        var instrument = SfzParser.Parse(sfz);
        
        Assert.Equal(-100f, instrument.Regions[0].Pan);
    }

    [Fact]
    public void Parse_RegionWithTune_ParsesFineTuning()
    {
        var sfz = @"
<region>
tune=-788
";
        var instrument = SfzParser.Parse(sfz);
        
        Assert.Equal(-788, instrument.Regions[0].Tune);
    }

    [Fact]
    public void Parse_RegionWithEnvelope_ParsesAmpegSettings()
    {
        var sfz = @"
<region>
ampeg_attack=0.01
ampeg_decay=0.5
ampeg_sustain=80
ampeg_release=0.3
";
        var instrument = SfzParser.Parse(sfz);
        
        Assert.Equal(0.01f, instrument.Regions[0].AmpegAttack, precision: 5);
        Assert.Equal(0.5f, instrument.Regions[0].AmpegDecay, precision: 5);
        Assert.Equal(80f, instrument.Regions[0].AmpegSustain, precision: 5);
        Assert.Equal(0.3f, instrument.Regions[0].AmpegRelease, precision: 5);
    }

    [Fact]
    public void Parse_MultipleRegions_CreatesAllRegions()
    {
        var sfz = @"
<region>
key=45
<region>
key=46
<region>
key=47
";
        var instrument = SfzParser.Parse(sfz);
        
        Assert.Equal(3, instrument.Regions.Count);
        Assert.Equal(45, instrument.Regions[0].Key);
        Assert.Equal(46, instrument.Regions[1].Key);
        Assert.Equal(47, instrument.Regions[2].Key);
    }

    [Fact]
    public void Parse_GlobalInheritsToRegions_AppliesGlobalSample()
    {
        var sfz = @"
<global>
sample=common.wav

<region>
key=60
<region>
key=72
";
        var instrument = SfzParser.Parse(sfz);
        
        Assert.Equal("common.wav", instrument.DefaultSample);
        // Regions without explicit sample should use default
        Assert.Equal(string.Empty, instrument.Regions[0].Sample);
        Assert.Equal(string.Empty, instrument.Regions[1].Sample);
    }

    [Fact]
    public void Parse_GroupInheritsToRegions_AppliesGroupSettings()
    {
        var sfz = @"
<group>
ampeg_release=1.5

<region>
key=60
<region>
key=72
";
        var instrument = SfzParser.Parse(sfz);
        
        Assert.Equal(1.5f, instrument.Regions[0].AmpegRelease, precision: 5);
        Assert.Equal(1.5f, instrument.Regions[1].AmpegRelease, precision: 5);
    }

    [Fact]
    public void Parse_Comments_AreIgnored()
    {
        var sfz = @"
// This is a comment
<region>
key=60 // inline comment
";
        var instrument = SfzParser.Parse(sfz);
        
        Assert.Single(instrument.Regions);
        Assert.Equal(60, instrument.Regions[0].Key);
    }

    [Fact]
    public void Parse_BlockComments_AreIgnored()
    {
        var sfz = @"
/*
Multi-line comment
*/
<region>
key=60
";
        var instrument = SfzParser.Parse(sfz);
        
        Assert.Single(instrument.Regions);
    }

    [Fact]
    public void Parse_RealGlockenspielSfz_ParsesCorrectly()
    {
        var sfz = @"
<global>
 sample=sf2_smpl.wav
<group>
 ampeg_decay=29.9953
 ampeg_sustain=0.001
 ampeg_release=3.00008
<region>
 lokey=72 hikey=80
 loop_mode=loop_continuous
 pitch_keycenter=79
 pan=-100
 region_label=EGlock G2(L)
 tune=0 offset=2903444 end=3087010
 loop_start=3081577 loop_end=3086996
";
        var instrument = SfzParser.Parse(sfz);
        
        Assert.Single(instrument.Regions);
        Assert.Equal("sf2_smpl.wav", instrument.DefaultSample);
        
        var region = instrument.Regions[0];
        Assert.Equal(72, region.LoKey);
        Assert.Equal(80, region.HiKey);
        Assert.Equal(79, region.PitchKeycenter);
        Assert.Equal(-100f, region.Pan);
        Assert.Equal(2903444, region.Offset);
        Assert.Equal(3087010, region.End);
        Assert.Equal(LoopMode.LoopContinuous, region.LoopMode);
        Assert.Equal(3.00008f, region.AmpegRelease, precision: 3);
    }
}


