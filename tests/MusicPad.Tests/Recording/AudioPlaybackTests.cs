using MusicPad.Core.Recording;
using Xunit;

namespace MusicPad.Tests.Recording;

public class AudioPlaybackTests
{
    private const int SampleRate = 44100;
    
    [Fact]
    public void IsPlaying_Initially_IsFalse()
    {
        var playback = new AudioPlayback(SampleRate);
        
        Assert.False(playback.IsPlaying);
    }
    
    [Fact]
    public void Start_WithNoEvents_DoesNotStartPlaying()
    {
        var playback = new AudioPlayback(SampleRate);
        
        playback.Start();
        
        Assert.False(playback.IsPlaying);
    }
    
    [Fact]
    public void Start_WithEvents_StartsPlaying()
    {
        var playback = new AudioPlayback(SampleRate);
        var events = new List<RecordedEvent>
        {
            new() { TimestampMs = 0, EventType = RecordedEventType.NoteOn, MidiNote = 60 }
        };
        
        playback.LoadEvents(events);
        playback.Start();
        
        Assert.True(playback.IsPlaying);
    }
    
    [Fact]
    public void Stop_StopsPlaying()
    {
        var playback = new AudioPlayback(SampleRate);
        var events = new List<RecordedEvent>
        {
            new() { TimestampMs = 0, EventType = RecordedEventType.NoteOn, MidiNote = 60 }
        };
        
        playback.LoadEvents(events);
        playback.Start();
        playback.Stop();
        
        Assert.False(playback.IsPlaying);
    }
    
    [Fact]
    public void ProcessBuffer_ReturnsNoteOnEvent_AtCorrectTime()
    {
        var playback = new AudioPlayback(SampleRate);
        var events = new List<RecordedEvent>
        {
            new() { TimestampMs = 0, EventType = RecordedEventType.NoteOn, MidiNote = 60, Velocity = 100 }
        };
        
        playback.LoadEvents(events);
        playback.Start();
        
        // Process first buffer - should contain the note
        var result = playback.ProcessBuffer(1024);
        
        Assert.Single(result);
        Assert.Equal(60, result[0].MidiNote);
        Assert.True(result[0].IsNoteOn);
        Assert.Equal(100, result[0].Velocity);
    }
    
    [Fact]
    public void ProcessBuffer_ReturnsNoteOffEvent()
    {
        var playback = new AudioPlayback(SampleRate);
        var events = new List<RecordedEvent>
        {
            new() { TimestampMs = 0, EventType = RecordedEventType.NoteOff, MidiNote = 60 }
        };
        
        playback.LoadEvents(events);
        playback.Start();
        
        var result = playback.ProcessBuffer(1024);
        
        Assert.Single(result);
        Assert.Equal(60, result[0].MidiNote);
        Assert.False(result[0].IsNoteOn);
    }
    
    [Fact]
    public void ProcessBuffer_EventAt100ms_NotReturnedInFirstBuffer()
    {
        var playback = new AudioPlayback(SampleRate);
        // Event at 100ms = 4410 samples at 44100Hz
        var events = new List<RecordedEvent>
        {
            new() { TimestampMs = 100, EventType = RecordedEventType.NoteOn, MidiNote = 60 }
        };
        
        playback.LoadEvents(events);
        playback.Start();
        
        // First buffer: 1024 samples = ~23ms
        var result = playback.ProcessBuffer(1024);
        
        Assert.Empty(result);
        Assert.True(playback.IsPlaying); // Still playing, event not yet reached
    }
    
    [Fact]
    public void ProcessBuffer_EventAt100ms_ReturnedAfterEnoughBuffers()
    {
        var playback = new AudioPlayback(SampleRate);
        // Event at 100ms = 4410 samples at 44100Hz
        var events = new List<RecordedEvent>
        {
            new() { TimestampMs = 100, EventType = RecordedEventType.NoteOn, MidiNote = 60 }
        };
        
        playback.LoadEvents(events);
        playback.Start();
        
        // Process 5 buffers of 1024 samples each = 5120 samples = ~116ms
        var allResults = new List<PlaybackNoteEvent>();
        for (int i = 0; i < 5; i++)
        {
            allResults.AddRange(playback.ProcessBuffer(1024));
        }
        
        Assert.Single(allResults);
        Assert.Equal(60, allResults[0].MidiNote);
    }
    
    [Fact]
    public void ProcessBuffer_MultipleEventsInSameBuffer_AllReturned()
    {
        var playback = new AudioPlayback(SampleRate);
        var events = new List<RecordedEvent>
        {
            new() { TimestampMs = 0, EventType = RecordedEventType.NoteOn, MidiNote = 60 },
            new() { TimestampMs = 10, EventType = RecordedEventType.NoteOn, MidiNote = 62 },
            new() { TimestampMs = 20, EventType = RecordedEventType.NoteOn, MidiNote = 64 }
        };
        
        playback.LoadEvents(events);
        playback.Start();
        
        // 1024 samples = ~23ms, should contain all 3 events
        var result = playback.ProcessBuffer(1024);
        
        Assert.Equal(3, result.Count);
    }
    
    [Fact]
    public void ProcessBuffer_WhenAllEventsProcessed_StopsPlaying()
    {
        var playback = new AudioPlayback(SampleRate);
        var events = new List<RecordedEvent>
        {
            new() { TimestampMs = 0, EventType = RecordedEventType.NoteOn, MidiNote = 60 }
        };
        
        playback.LoadEvents(events);
        playback.Start();
        
        playback.ProcessBuffer(1024);
        
        Assert.False(playback.IsPlaying); // All events processed
    }
    
    [Fact]
    public void ProcessBuffer_InstrumentChangeEvent_AddedToPendingUiEvents()
    {
        var playback = new AudioPlayback(SampleRate);
        var events = new List<RecordedEvent>
        {
            new() { TimestampMs = 0, EventType = RecordedEventType.InstrumentChange, InstrumentId = "Piano" }
        };
        
        playback.LoadEvents(events);
        playback.Start();
        
        var noteEvents = playback.ProcessBuffer(1024);
        var uiEvents = playback.GetPendingUiEvents();
        
        Assert.Empty(noteEvents); // No note events
        Assert.Single(uiEvents);
        Assert.Equal(RecordedEventType.InstrumentChange, uiEvents[0].EventType);
        Assert.Equal("Piano", uiEvents[0].InstrumentId);
    }
    
    [Fact]
    public void GetPendingUiEvents_ClearsQueue()
    {
        var playback = new AudioPlayback(SampleRate);
        var events = new List<RecordedEvent>
        {
            new() { TimestampMs = 0, EventType = RecordedEventType.InstrumentChange, InstrumentId = "Piano" }
        };
        
        playback.LoadEvents(events);
        playback.Start();
        playback.ProcessBuffer(1024);
        
        var firstCall = playback.GetPendingUiEvents();
        var secondCall = playback.GetPendingUiEvents();
        
        Assert.Single(firstCall);
        Assert.Empty(secondCall);
    }
    
    [Fact]
    public void ProcessBuffer_WhenNotPlaying_ReturnsEmpty()
    {
        var playback = new AudioPlayback(SampleRate);
        var events = new List<RecordedEvent>
        {
            new() { TimestampMs = 0, EventType = RecordedEventType.NoteOn, MidiNote = 60 }
        };
        
        playback.LoadEvents(events);
        // Don't start playback
        
        var result = playback.ProcessBuffer(1024);
        
        Assert.Empty(result);
    }
    
    [Fact]
    public void Start_ResetsToBeginning()
    {
        var playback = new AudioPlayback(SampleRate);
        var events = new List<RecordedEvent>
        {
            new() { TimestampMs = 0, EventType = RecordedEventType.NoteOn, MidiNote = 60 }
        };
        
        playback.LoadEvents(events);
        playback.Start();
        playback.ProcessBuffer(1024); // Process all events
        Assert.False(playback.IsPlaying);
        
        // Start again - should replay from beginning
        playback.Start();
        Assert.True(playback.IsPlaying);
        
        var result = playback.ProcessBuffer(1024);
        Assert.Single(result);
        Assert.Equal(60, result[0].MidiNote);
    }
}




