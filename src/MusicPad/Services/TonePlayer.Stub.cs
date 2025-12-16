#if !ANDROID
using MusicPad.Core.Audio;

namespace MusicPad.Services;

/// <summary>
/// Stub implementation of ITonePlayer for platforms without audio support.
/// Platform-specific implementations should be in Platforms/{Platform}/Services/
/// </summary>
public partial class TonePlayer : ITonePlayer
{
    public void StartTone(double frequency) { }
    public void StartTone(double frequency, float[] waveTable) { }
    public void UpdateWaveTable(double frequency, float[] waveTable) { }
    public void UpdateEnvelope(AHDSHRSettings settings) { }
    public void StopTone(double frequency) { }
    public void StopAll() { }
}
#endif