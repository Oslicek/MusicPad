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
    /// Whether playback is currently active.
    /// </summary>
    bool IsPlaying { get; }
    
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
    /// Loads a song for playback.
    /// </summary>
    Task<bool> LoadSongAsync(string songId);
    
    /// <summary>
    /// Starts playback of the loaded song.
    /// </summary>
    /// <param name="liveMode">If true, uses current instruments/effects instead of recorded ones.</param>
    void StartPlayback(bool liveMode = false);
    
    /// <summary>
    /// Stops playback.
    /// </summary>
    void StopPlayback();
    
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
    
    /// <summary>
    /// Event raised when playback state changes.
    /// </summary>
    event EventHandler<bool>? PlaybackStateChanged;
    
    /// <summary>
    /// Event raised during playback when a note should be played.
    /// </summary>
    event EventHandler<RecordedEvent>? PlaybackNoteEvent;
    
    /// <summary>
    /// Event raised during playback when instrument should change (original mode only).
    /// </summary>
    event EventHandler<string>? PlaybackInstrumentChange;
}


