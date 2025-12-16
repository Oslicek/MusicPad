#if !ANDROID
using MusicPad.Core.Sfz;

namespace MusicPad.Services;

/// <summary>
/// Stub implementation of ISfzService for non-Android platforms.
/// </summary>
public class SfzService : ISfzService
{
    public IReadOnlyList<string> AvailableInstruments => Array.Empty<string>();
    public string? CurrentInstrumentName => null;

    public Task LoadInstrumentAsync(string instrumentName)
    {
        return Task.CompletedTask;
    }

    public void PlayNote() { }
    public void StopNote() { }
}
#endif

