#if !ANDROID

using MusicPad.Core.Models;
using MusicPad.Core.Sfz;

namespace MusicPad.Services;

/// <summary>
/// Stub implementation of ISfzService for non-Android platforms.
/// </summary>
public class SfzService : ISfzService
{
    public IReadOnlyList<string> AvailableInstruments => Array.Empty<string>();
    public string? CurrentInstrumentName => null;
    public (int minKey, int maxKey) CurrentKeyRange => (0, 127);
    public SfzInstrument? CurrentInstrument => null;
    public float Volume { get; set; } = 0.75f;
    public bool LpfEnabled { get; set; } = false;
    public float LpfCutoff { get; set; } = 1.0f;
    public float LpfResonance { get; set; } = 0.0f;

    public Task LoadInstrumentAsync(string instrumentName) => Task.CompletedTask;
    public void NoteOn(int midiNote) { }
    public void NoteOn(int midiNote, int velocity) { }
    public void NoteOff(int midiNote) { }
    public void StopAll() { }
    public float GetNoteEnvelopeLevel(int midiNote) => 0f;
    public void SetEqBandGain(int band, float normalizedGain) { }
    public bool ChorusEnabled { get; set; } = false;
    public float ChorusDepth { get; set; } = 0.5f;
    public float ChorusRate { get; set; } = 0.3f;
    public bool DelayEnabled { get; set; } = false;
    public float DelayTime { get; set; } = 0.4f;
    public float DelayFeedback { get; set; } = 0.4f;
    public float DelayLevel { get; set; } = 0.5f;
    public bool ReverbEnabled { get; set; } = false;
    public float ReverbLevel { get; set; } = 0.3f;
    public ReverbType ReverbType { get; set; } = ReverbType.Room;
}
#endif

