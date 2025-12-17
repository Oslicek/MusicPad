namespace MusicPad.Core.Models;

/// <summary>
/// Calculates the visible piano range based on orientation and instrument limits.
/// </summary>
public class PianoRangeManager
{
    // Standard 88-key MIDI limits
    private const int GlobalMin = 21;  // A0
    private const int GlobalMax = 108; // C8

    private readonly int _instrumentMin;
    private readonly int _instrumentMax;

    private int _desiredSpan; // number of notes in window
    private int _start;       // inclusive start of visible window

    public PianoRangeManager(int instrumentMin, int instrumentMax, bool isLandscape)
    {
        _instrumentMin = Math.Clamp(instrumentMin, GlobalMin, GlobalMax);
        _instrumentMax = Math.Clamp(instrumentMax, GlobalMin, GlobalMax);
        SetOrientation(isLandscape);
    }

    /// <summary>
    /// Sets orientation; portrait = 13-note window (C3-C4), landscape = 25-note window (C2-C4).
    /// </summary>
    public void SetOrientation(bool isLandscape)
    {
        _desiredSpan = isLandscape ? 25 : 13;
        int suggestedStart = isLandscape ? 36 : 48; // C2 or C3
        SetStart(suggestedStart);
    }

    /// <summary>
    /// Gets the current visible range (inclusive).
    /// </summary>
    public (int start, int end) GetRange()
    {
        int end = Math.Min(_start + _desiredSpan - 1, _instrumentMax);
        return (_start, end);
    }

    /// <summary>
    /// Sets the start position directly (clamped).
    /// </summary>
    public void SetStartAbsolute(int start)
    {
        SetStart(start);
    }

    /// <summary>
    /// Move the window up by semitones (positive to higher pitches).
    /// </summary>
    public void Move(int semitoneDelta)
    {
        SetStart(_start + semitoneDelta);
    }

    private void SetStart(int proposedStart)
    {
        int minStart = _instrumentMin;
        int maxStart = Math.Max(_instrumentMin, _instrumentMax - _desiredSpan + 1);

        _start = Math.Clamp(proposedStart, minStart, maxStart);
    }
}

