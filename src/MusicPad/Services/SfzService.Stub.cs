#if !ANDROID

namespace MusicPad.Services;

/// <summary>
/// Stub implementation of ISfzService for non-Android platforms.
/// </summary>
public class SfzService : ISfzService
{
    public IReadOnlyList<string> AvailableInstruments => Array.Empty<string>();
    public string? CurrentInstrumentName => null;
    public (int minKey, int maxKey) CurrentKeyRange => (0, 127);

    public Task LoadInstrumentAsync(string instrumentName) => Task.CompletedTask;
    public void NoteOn(int midiNote) { }
    public void NoteOff(int midiNote) { }
    public void StopAll() { }
}
#endif

