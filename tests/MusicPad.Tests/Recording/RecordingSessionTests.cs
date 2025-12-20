using MusicPad.Core.Recording;

namespace MusicPad.Tests.Recording;

public class RecordingSessionTests
{
    [Fact]
    public void Start_SetsIsRecordingToTrue()
    {
        var session = new RecordingSession();
        
        session.Start();
        
        Assert.True(session.IsRecording);
    }
    
    [Fact]
    public void Stop_SetsIsRecordingToFalse()
    {
        var session = new RecordingSession();
        session.Start();
        
        session.Stop();
        
        Assert.False(session.IsRecording);
    }
    
    [Fact]
    public void RecordNoteOn_WhenRecording_AddsEvent()
    {
        var session = new RecordingSession();
        session.Start();
        
        session.RecordNoteOn(60, 100);
        
        Assert.Equal(1, session.EventCount);
    }
    
    [Fact]
    public void RecordNoteOn_WhenNotRecording_DoesNotAddEvent()
    {
        var session = new RecordingSession();
        // Not started
        
        session.RecordNoteOn(60, 100);
        
        Assert.Equal(0, session.EventCount);
    }
    
    [Fact]
    public void RecordNoteOff_WhenRecording_AddsEvent()
    {
        var session = new RecordingSession();
        session.Start();
        
        session.RecordNoteOff(60);
        
        Assert.Equal(1, session.EventCount);
    }
    
    [Fact]
    public void Stop_ReturnsRecordedEvents()
    {
        var session = new RecordingSession();
        session.Start();
        session.RecordNoteOn(60, 100);
        session.RecordNoteOn(64, 80);
        session.RecordNoteOff(60);
        
        var (events, duration) = session.Stop();
        
        Assert.Equal(3, events.Count);
        Assert.Equal(RecordedEventType.NoteOn, events[0].EventType);
        Assert.Equal(60, events[0].MidiNote);
        Assert.Equal(100, events[0].Velocity);
        Assert.Equal(RecordedEventType.NoteOn, events[1].EventType);
        Assert.Equal(64, events[1].MidiNote);
        Assert.Equal(RecordedEventType.NoteOff, events[2].EventType);
    }
    
    [Fact]
    public void Stop_ReturnsDuration()
    {
        var session = new RecordingSession();
        session.Start();
        Thread.Sleep(50); // Wait a bit
        
        var (_, duration) = session.Stop();
        
        Assert.True(duration >= 40); // At least some time passed
    }
    
    [Fact]
    public void RecordInstrumentChange_AddsEventWithInstrumentId()
    {
        var session = new RecordingSession();
        session.Start("piano");
        
        session.RecordInstrumentChange("guitar");
        
        var (events, _) = session.Stop();
        Assert.Single(events);
        Assert.Equal(RecordedEventType.InstrumentChange, events[0].EventType);
        Assert.Equal("guitar", events[0].InstrumentId);
    }
    
    [Fact]
    public void RecordInstrumentChange_SameInstrument_DoesNotAddEvent()
    {
        var session = new RecordingSession();
        session.Start("piano");
        
        session.RecordInstrumentChange("piano"); // Same as initial
        
        Assert.Equal(0, session.EventCount);
    }
    
    [Fact]
    public void Start_ClearsPreviousEvents()
    {
        var session = new RecordingSession();
        session.Start();
        session.RecordNoteOn(60, 100);
        session.Stop();
        
        session.Start(); // Start new recording
        
        Assert.Equal(0, session.EventCount);
    }
    
    [Fact]
    public void Events_HaveIncreasingTimestamps()
    {
        var session = new RecordingSession();
        session.Start();
        
        session.RecordNoteOn(60, 100);
        Thread.Sleep(20);
        session.RecordNoteOn(64, 100);
        Thread.Sleep(20);
        session.RecordNoteOff(60);
        
        var (events, _) = session.Stop();
        
        Assert.True(events[1].TimestampMs >= events[0].TimestampMs);
        Assert.True(events[2].TimestampMs >= events[1].TimestampMs);
    }
}



