using MusicPad.Core.Sfz;

namespace MusicPad.Tests.Sfz;

public class SfzInstrumentTests
{
    [Fact]
    public void FindRegions_ReturnsMatchingRegions()
    {
        var instrument = new SfzInstrument();
        instrument.Regions.Add(new SfzRegion { LoKey = 60, HiKey = 72 });
        instrument.Regions.Add(new SfzRegion { LoKey = 48, HiKey = 59 });
        instrument.Regions.Add(new SfzRegion { LoKey = 60, HiKey = 72 }); // Overlapping (stereo pair)
        
        var matches = instrument.FindRegions(65).ToList();
        
        Assert.Equal(2, matches.Count);
    }

    [Fact]
    public void FindRegions_NoMatch_ReturnsEmpty()
    {
        var instrument = new SfzInstrument();
        instrument.Regions.Add(new SfzRegion { LoKey = 60, HiKey = 72 });
        
        var matches = instrument.FindRegions(48).ToList();
        
        Assert.Empty(matches);
    }

    [Fact]
    public void GetKeyRange_ReturnsMinAndMaxKeys()
    {
        var instrument = new SfzInstrument();
        instrument.Regions.Add(new SfzRegion { LoKey = 48, HiKey = 60 });
        instrument.Regions.Add(new SfzRegion { LoKey = 72, HiKey = 96 });
        
        var (min, max) = instrument.GetKeyRange();
        
        Assert.Equal(48, min);
        Assert.Equal(96, max);
    }

    [Fact]
    public void GetKeyRange_WithSingleKey_UsesKeyValue()
    {
        var instrument = new SfzInstrument();
        instrument.Regions.Add(new SfzRegion { Key = 45 });
        instrument.Regions.Add(new SfzRegion { Key = 60 });
        
        var (min, max) = instrument.GetKeyRange();
        
        Assert.Equal(45, min);
        Assert.Equal(60, max);
    }

    [Fact]
    public void GetMiddleKey_ReturnsMiddleOfRange()
    {
        var instrument = new SfzInstrument();
        instrument.Regions.Add(new SfzRegion { LoKey = 48, HiKey = 72 });
        
        var middle = instrument.GetMiddleKey();
        
        Assert.Equal(60, middle);
    }

    [Fact]
    public void GetSamplePath_CombinesBasePathAndSample()
    {
        var instrument = new SfzInstrument
        {
            BasePath = "/data/instruments/piano"
        };
        var region = new SfzRegion { Sample = "samples/c4.wav" };
        
        var path = instrument.GetSamplePath(region);
        
        Assert.Contains("piano", path);
        Assert.Contains("c4.wav", path);
    }

    [Fact]
    public void GetSamplePath_UsesDefaultSample_WhenRegionHasNone()
    {
        var instrument = new SfzInstrument
        {
            BasePath = "/data/instruments/synth",
            DefaultSample = "sf2_smpl.wav"
        };
        var region = new SfzRegion(); // No sample specified
        
        var path = instrument.GetSamplePath(region);
        
        Assert.Contains("sf2_smpl.wav", path);
    }
}

