using MusicPad.Core.Recording;

namespace MusicPad.Tests.Recording;

public class SongTests
{
    [Fact]
    public void GenerateName_ShortDuration_FormatsCorrectly()
    {
        var dateTime = new DateTime(2025, 12, 20, 14, 30, 0);
        var instrumentName = "Piano";
        var durationMs = 45_000; // 45 seconds
        
        var name = Song.GenerateName(dateTime, instrumentName, durationMs);
        
        Assert.Equal("2025-12-20_1430_Piano_45s", name);
    }
    
    [Fact]
    public void GenerateName_LongDuration_FormatsWithMinutes()
    {
        var dateTime = new DateTime(2025, 12, 20, 14, 30, 0);
        var instrumentName = "Guitar";
        var durationMs = 150_000; // 2m30s
        
        var name = Song.GenerateName(dateTime, instrumentName, durationMs);
        
        Assert.Equal("2025-12-20_1430_Guitar_2m30s", name);
    }
    
    [Fact]
    public void GenerateName_LongInstrumentName_Truncates()
    {
        var dateTime = new DateTime(2025, 12, 20, 14, 30, 0);
        var instrumentName = "Very Long Instrument Name That Should Be Truncated";
        var durationMs = 60_000; // 1m00s
        
        var name = Song.GenerateName(dateTime, instrumentName, durationMs);
        
        Assert.Contains("2025-12-20_1430_", name);
        Assert.True(name.Length <= 50); // Reasonable length
    }
    
    [Fact]
    public void GenerateName_InstrumentWithSpaces_ReplacesWithUnderscores()
    {
        var dateTime = new DateTime(2025, 12, 20, 14, 30, 0);
        var instrumentName = "Grand Piano";
        var durationMs = 30_000;
        
        var name = Song.GenerateName(dateTime, instrumentName, durationMs);
        
        Assert.Contains("Grand_Piano", name);
    }
    
    [Fact]
    public void NewSong_HasUniqueId()
    {
        var song1 = new Song();
        var song2 = new Song();
        
        Assert.NotEqual(song1.Id, song2.Id);
    }
    
    [Fact]
    public void NewSong_HasCreatedAtSet()
    {
        var before = DateTime.UtcNow;
        var song = new Song();
        var after = DateTime.UtcNow;
        
        Assert.True(song.CreatedAt >= before);
        Assert.True(song.CreatedAt <= after);
    }
}


