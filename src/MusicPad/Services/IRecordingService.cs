using MusicPad.Core.Recording;

namespace MusicPad.Services;

/// <summary>
/// Service for recording and playing back performances.
/// </summary>
public interface IRecordingService
{
    /// <summary>
    /// Whether recording is currently active.
    /// </summary>
    bool IsRecording { get; }
    
    /// <summary>
    /// The currently loaded song (for playback).
    /// </summary>
    Song? CurrentSong { get; }
    
    /// <summary>
    /// Starts recording.
    /// </summary>
    /// <param name="initialInstrumentId">Current instrument when recording starts.</param>
    /// <param name="initialSettingsJson">Current effect settings as JSON.</param>
    void StartRecording(string? initialInstrumentId, string? initialSettingsJson);
    
    /// <summary>
    /// Stops recording and saves the song.
    /// </summary>
    /// <returns>The saved song, or null if recording failed.</returns>
    Task<Song?> StopRecordingAsync();
    
    /// <summary>
    /// Records a note on event (pad touched).
    /// </summary>
    void RecordNoteOn(int midiNote, int velocity = 100);
    
    /// <summary>
    /// Records a note off event (pad released).
    /// </summary>
    void RecordNoteOff(int midiNote);
    
    /// <summary>
    /// Records an instrument change.
    /// </summary>
    void RecordInstrumentChange(string instrumentId);
    
    /// <summary>
    /// Loads a song for playback. Returns the events for audio-thread playback.
    /// </summary>
    Task<IReadOnlyList<RecordedEvent>?> LoadSongAsync(string songId);
    
    /// <summary>
    /// Gets the initial instrument ID from the currently loaded song.
    /// </summary>
    string? LoadedSongInitialInstrumentId { get; }
    
    /// <summary>
    /// Gets all saved songs.
    /// </summary>
    Task<IReadOnlyList<Song>> GetSongsAsync();
    
    /// <summary>
    /// Deletes a song.
    /// </summary>
    Task<bool> DeleteSongAsync(string songId);
    
    /// <summary>
    /// Event raised when recording state changes.
    /// </summary>
    event EventHandler<bool>? RecordingStateChanged;
}


