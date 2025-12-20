using MusicPad.Core.Models;
using MusicPad.Core.NoteProcessing;

namespace MusicPad.Tests.NoteProcessing;

/// <summary>
/// Tests for audio-thread arpeggiator with sample-accurate timing.
/// </summary>
public class AudioArpeggiatorTests
{
    private const int SampleRate = 44100;
    
    #region Basic Timing Tests
    
    [Fact]
    public void ProcessBuffer_NoNotes_ReturnsNoEvents()
    {
        var arp = new AudioArpeggiator(SampleRate);
        arp.IsEnabled = true;
        arp.SetIntervalMs(100);
        
        var events = arp.ProcessBuffer(512);
        
        Assert.Empty(events);
    }
    
    [Fact]
    public void ProcessBuffer_WithNotes_TriggersAtCorrectTime()
    {
        var arp = new AudioArpeggiator(SampleRate);
        arp.IsEnabled = true;
        arp.SetIntervalMs(100); // 100ms = 4410 samples at 44100Hz
        arp.AddNote(60);
        
        // First buffer should trigger immediately
        var events1 = arp.ProcessBuffer(512);
        Assert.Single(events1);
        Assert.Equal(60, events1[0].MidiNote);
        Assert.Equal(ArpEventType.NoteOn, events1[0].EventType);
        
        // Process 4000 samples (not yet 4410)
        var events2 = arp.ProcessBuffer(4000);
        Assert.DoesNotContain(events2, e => e.EventType == ArpEventType.NoteOn);
        
        // Process 500 more samples (now past 4410)
        var events3 = arp.ProcessBuffer(500);
        // Should have NoteOff for previous and NoteOn for next
        Assert.Contains(events3, e => e.EventType == ArpEventType.NoteOff && e.MidiNote == 60);
        Assert.Contains(events3, e => e.EventType == ArpEventType.NoteOn && e.MidiNote == 60);
    }
    
    [Fact]
    public void ProcessBuffer_MultipleNotes_CyclesThroughUp()
    {
        var arp = new AudioArpeggiator(SampleRate);
        arp.IsEnabled = true;
        arp.Pattern = ArpPattern.Up;
        arp.SetIntervalMs(100);
        arp.AddNote(60);
        arp.AddNote(64);
        arp.AddNote(67);
        
        // First note
        var events1 = arp.ProcessBuffer(512);
        Assert.Contains(events1, e => e.EventType == ArpEventType.NoteOn && e.MidiNote == 60);
        
        // Advance past interval
        arp.ProcessBuffer(4410);
        var events2 = arp.ProcessBuffer(512);
        Assert.Contains(events2, e => e.EventType == ArpEventType.NoteOn && e.MidiNote == 64);
        
        // Advance again
        arp.ProcessBuffer(4410);
        var events3 = arp.ProcessBuffer(512);
        Assert.Contains(events3, e => e.EventType == ArpEventType.NoteOn && e.MidiNote == 67);
        
        // Should wrap around
        arp.ProcessBuffer(4410);
        var events4 = arp.ProcessBuffer(512);
        Assert.Contains(events4, e => e.EventType == ArpEventType.NoteOn && e.MidiNote == 60);
    }
    
    [Fact]
    public void ProcessBuffer_DownPattern_PlaysHighToLow()
    {
        var arp = new AudioArpeggiator(SampleRate);
        arp.IsEnabled = true;
        arp.Pattern = ArpPattern.Down;
        arp.SetIntervalMs(100);
        arp.AddNote(60);
        arp.AddNote(64);
        arp.AddNote(67);
        
        // First note should be highest
        var events1 = arp.ProcessBuffer(512);
        Assert.Contains(events1, e => e.EventType == ArpEventType.NoteOn && e.MidiNote == 67);
        
        arp.ProcessBuffer(4410);
        var events2 = arp.ProcessBuffer(512);
        Assert.Contains(events2, e => e.EventType == ArpEventType.NoteOn && e.MidiNote == 64);
    }
    
    #endregion
    
    #region Enable/Disable Tests
    
    [Fact]
    public void Disabled_ReturnsNoEvents()
    {
        var arp = new AudioArpeggiator(SampleRate);
        arp.IsEnabled = false;
        arp.SetIntervalMs(100);
        arp.AddNote(60);
        
        var events = arp.ProcessBuffer(10000);
        
        Assert.Empty(events);
    }
    
    [Fact]
    public void Enable_StartsFromBeginning()
    {
        var arp = new AudioArpeggiator(SampleRate);
        arp.IsEnabled = false;
        arp.SetIntervalMs(100);
        arp.AddNote(60);
        
        // Process while disabled
        arp.ProcessBuffer(10000);
        
        // Enable
        arp.IsEnabled = true;
        
        // Should trigger immediately
        var events = arp.ProcessBuffer(512);
        Assert.Contains(events, e => e.EventType == ArpEventType.NoteOn);
    }
    
    [Fact]
    public void Disable_StopsCurrentNote()
    {
        var arp = new AudioArpeggiator(SampleRate);
        arp.IsEnabled = true;
        arp.SetIntervalMs(100);
        arp.AddNote(60);
        
        // Trigger a note
        arp.ProcessBuffer(512);
        
        // Disable - should return NoteOff
        arp.IsEnabled = false;
        var events = arp.ProcessBuffer(512);
        
        Assert.Contains(events, e => e.EventType == ArpEventType.NoteOff && e.MidiNote == 60);
    }
    
    #endregion
    
    #region Note Management Tests
    
    [Fact]
    public void RemoveAllNotes_StopsCurrentNote()
    {
        var arp = new AudioArpeggiator(SampleRate);
        arp.IsEnabled = true;
        arp.SetIntervalMs(100);
        arp.AddNote(60);
        
        // Trigger note
        arp.ProcessBuffer(512);
        
        // Remove note
        arp.RemoveNote(60);
        var events = arp.ProcessBuffer(512);
        
        Assert.Contains(events, e => e.EventType == ArpEventType.NoteOff && e.MidiNote == 60);
    }
    
    [Fact]
    public void IntervalChange_AffectsNextScheduledInterval()
    {
        var arp = new AudioArpeggiator(SampleRate);
        arp.IsEnabled = true;
        arp.SetIntervalMs(100); // 4410 samples
        arp.AddNote(60);
        
        // Trigger first note
        var events1 = arp.ProcessBuffer(512);
        Assert.Contains(events1, e => e.EventType == ArpEventType.NoteOn);
        
        // Now change interval to 200ms (8820 samples)
        // But the next trigger is already scheduled for ~4410 samples from trigger point
        arp.SetIntervalMs(200);
        
        // Process to get past the original 100ms interval
        // Note: trigger check happens at START of ProcessBuffer
        arp.ProcessBuffer(4500); // sampleCounter = 5012, passes threshold of 4922
        var events2 = arp.ProcessBuffer(512); // Now triggers
        Assert.Contains(events2, e => e.EventType == ArpEventType.NoteOn);
        
        // Now the NEXT interval should be 200ms (8820 samples from ~5012)
        // Next trigger at ~5012 + 8820 = ~13832
        
        // Process 100ms (4410 samples) - should not trigger yet
        arp.ProcessBuffer(4410); // sampleCounter = ~10000
        var events3 = arp.ProcessBuffer(512);
        Assert.DoesNotContain(events3, e => e.EventType == ArpEventType.NoteOn);
        
        // Process enough to pass 200ms total - should trigger
        arp.ProcessBuffer(4000); // sampleCounter = ~14500, past 13832
        var events4 = arp.ProcessBuffer(512);
        Assert.Contains(events4, e => e.EventType == ArpEventType.NoteOn);
    }
    
    #endregion
    
    #region Reset Tests
    
    [Fact]
    public void Reset_ClearsAllStateAndStopsNote()
    {
        var arp = new AudioArpeggiator(SampleRate);
        arp.IsEnabled = true;
        arp.SetIntervalMs(100);
        arp.AddNote(60);
        arp.AddNote(64);
        
        // Trigger and advance
        arp.ProcessBuffer(512);
        arp.ProcessBuffer(4410);
        arp.ProcessBuffer(512); // Now on note 64
        
        // Reset
        var events = arp.Reset();
        
        Assert.Contains(events, e => e.EventType == ArpEventType.NoteOff);
        Assert.Empty(arp.ActiveNotes);
    }
    
    #endregion
    
    #region Timing Consistency Tests
    
    [Fact]
    public void TimingConsistency_IntervalsArePerfectlyEven()
    {
        var arp = new AudioArpeggiator(SampleRate);
        arp.IsEnabled = true;
        arp.SetIntervalMs(100); // 4410 samples
        arp.AddNote(60);
        
        const int bufferSize = 256; // Small buffer for precise measurement
        const int expectedInterval = 4410;
        var triggerSamples = new List<long>();
        long totalSamples = 0;
        
        // Run for 10 note triggers
        while (triggerSamples.Count < 10)
        {
            var events = arp.ProcessBuffer(bufferSize);
            
            if (events.Any(e => e.EventType == ArpEventType.NoteOn))
            {
                triggerSamples.Add(totalSamples);
            }
            
            totalSamples += bufferSize;
        }
        
        // Calculate intervals between triggers
        var intervals = new List<long>();
        for (int i = 1; i < triggerSamples.Count; i++)
        {
            intervals.Add(triggerSamples[i] - triggerSamples[i - 1]);
        }
        
        // All intervals should be the same (within buffer size tolerance)
        // Since triggers happen at buffer boundaries, max jitter = buffer size
        foreach (var interval in intervals)
        {
            Assert.InRange(interval, expectedInterval - bufferSize, expectedInterval + bufferSize);
        }
        
        // More importantly: variance should be near zero
        // All intervals should be essentially identical
        double average = intervals.Average();
        double variance = intervals.Select(i => Math.Pow(i - average, 2)).Average();
        double stdDev = Math.Sqrt(variance);
        
        // Standard deviation should be 0 for perfect consistency
        // (small buffer boundary variance is acceptable)
        Assert.True(stdDev < bufferSize, $"Timing jitter too high: stdDev={stdDev}");
    }
    
    [Fact]
    public void TimingConsistency_FastTempo_StillConsistent()
    {
        var arp = new AudioArpeggiator(SampleRate);
        arp.IsEnabled = true;
        arp.SetIntervalMs(125); // Fastest tempo: 480 BPM, 5512 samples
        arp.AddNote(60);
        arp.AddNote(64);
        arp.AddNote(67);
        
        const int bufferSize = 128; // Very small buffer
        var triggerSamples = new List<long>();
        long totalSamples = 0;
        
        // Run for 20 note triggers at fast tempo
        while (triggerSamples.Count < 20)
        {
            var events = arp.ProcessBuffer(bufferSize);
            
            if (events.Any(e => e.EventType == ArpEventType.NoteOn))
            {
                triggerSamples.Add(totalSamples);
            }
            
            totalSamples += bufferSize;
        }
        
        // Calculate intervals
        var intervals = new List<long>();
        for (int i = 1; i < triggerSamples.Count; i++)
        {
            intervals.Add(triggerSamples[i] - triggerSamples[i - 1]);
        }
        
        // All intervals should be consistent
        double average = intervals.Average();
        double variance = intervals.Select(i => Math.Pow(i - average, 2)).Average();
        double stdDev = Math.Sqrt(variance);
        
        // At fast tempos, jitter would be very noticeable
        // With sample-accurate timing, stdDev should be essentially 0
        Assert.True(stdDev < bufferSize, $"Fast tempo jitter too high: stdDev={stdDev}");
    }
    
    [Fact]
    public void TimingConsistency_VaryingBufferSizes_StillConsistent()
    {
        var arp = new AudioArpeggiator(SampleRate);
        arp.IsEnabled = true;
        arp.SetIntervalMs(100);
        arp.AddNote(60);
        
        var random = new Random(42); // Fixed seed for reproducibility
        var triggerSamples = new List<long>();
        long totalSamples = 0;
        
        // Run with varying buffer sizes (simulating real-world conditions)
        while (triggerSamples.Count < 15)
        {
            // Buffer sizes between 64 and 1024 (typical audio range)
            int bufferSize = random.Next(64, 1024);
            var events = arp.ProcessBuffer(bufferSize);
            
            if (events.Any(e => e.EventType == ArpEventType.NoteOn))
            {
                triggerSamples.Add(totalSamples);
            }
            
            totalSamples += bufferSize;
        }
        
        // Calculate intervals
        var intervals = new List<long>();
        for (int i = 1; i < triggerSamples.Count; i++)
        {
            intervals.Add(triggerSamples[i] - triggerSamples[i - 1]);
        }
        
        // Even with varying buffer sizes, the underlying timing should be consistent
        // Each interval should be close to 4410 samples (100ms)
        const int expectedInterval = 4410;
        const int maxBufferSize = 1024;
        
        foreach (var interval in intervals)
        {
            Assert.InRange(interval, expectedInterval - maxBufferSize, expectedInterval + maxBufferSize);
        }
    }
    
    [Fact]
    public void TimingConsistency_NoJitterComparedToUITimer()
    {
        // This test demonstrates the improvement over UI timer
        // With sample-based timing, we get perfect consistency
        
        var arp = new AudioArpeggiator(SampleRate);
        arp.IsEnabled = true;
        arp.SetIntervalMs(150); // 6615 samples
        arp.AddNote(60);
        
        const int bufferSize = 512;
        var intervals = new List<long>();
        long lastTrigger = 0;
        long totalSamples = 0;
        bool firstTrigger = true;
        
        while (intervals.Count < 50)
        {
            var events = arp.ProcessBuffer(bufferSize);
            
            if (events.Any(e => e.EventType == ArpEventType.NoteOn))
            {
                if (!firstTrigger)
                {
                    intervals.Add(totalSamples - lastTrigger);
                }
                firstTrigger = false;
                lastTrigger = totalSamples;
            }
            
            totalSamples += bufferSize;
        }
        
        // Calculate timing statistics
        double average = intervals.Average();
        double min = intervals.Min();
        double max = intervals.Max();
        double range = max - min;
        
        // The range should be at most 2x buffer size (one buffer early or late)
        Assert.True(range <= bufferSize * 2, 
            $"Timing range too large: {range}. Expected <= {bufferSize * 2}");
        
        // In contrast, a UI timer would have:
        // - 10-50ms jitter from UI thread delays
        // - Additional variance from GC pauses
        // - Compounding errors at faster tempos
    }
    
    #endregion
}

