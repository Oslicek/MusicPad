using System.Text.Json;
using MusicPad.Core.Recording;

namespace MusicPad.Services;

/// <summary>
/// Implementation of recording service with directory-based storage.
/// </summary>
public class RecordingService : IRecordingService
{
    private readonly RecordingSession _session = new();
    private readonly string _songsDirectory;
    private Song? _currentSong;
    private List<RecordedEvent>? _loadedEvents;
    private string? _initialInstrumentId;
    private string? _initialSettingsJson;
    
    public bool IsRecording => _session.IsRecording;
    public Song? CurrentSong => _currentSong;
    public string? LoadedSongInitialInstrumentId => _currentSong?.InitialInstrumentId;
    
    public event EventHandler<bool>? RecordingStateChanged;
    
    public RecordingService()
    {
        // Use app data directory for songs
        var appData = FileSystem.AppDataDirectory;
        _songsDirectory = Path.Combine(appData, "Songs");
        Directory.CreateDirectory(_songsDirectory);
    }
    
    public void StartRecording(string? initialInstrumentId, string? initialSettingsJson)
    {
        _initialInstrumentId = initialInstrumentId;
        _initialSettingsJson = initialSettingsJson;
        _session.Start(initialInstrumentId);
        RecordingStateChanged?.Invoke(this, true);
    }
    
    public async Task<Song?> StopRecordingAsync()
    {
        var (events, durationMs) = _session.Stop();
        RecordingStateChanged?.Invoke(this, false);
        
        if (events.Count == 0 || durationMs < 500)
        {
            // Don't save empty or very short recordings
            return null;
        }
        
        // Create song metadata
        var instrumentName = _initialInstrumentId ?? "Unknown";
        var song = new Song
        {
            Name = Song.GenerateName(DateTime.Now, instrumentName, durationMs),
            DurationMs = durationMs,
            InitialInstrumentId = _initialInstrumentId,
            InitialSettings = _initialSettingsJson
        };
        
        // Collect unique instruments
        var instruments = new HashSet<string>();
        if (_initialInstrumentId != null)
            instruments.Add(_initialInstrumentId);
        foreach (var evt in events.Where(e => e.EventType == RecordedEventType.InstrumentChange))
        {
            if (evt.InstrumentId != null)
                instruments.Add(evt.InstrumentId);
        }
        song.Instruments = instruments.ToList();
        
        // Save to directory
        try
        {
            await SaveSongAsync(song, events);
            return song;
        }
        catch
        {
            return null;
        }
    }
    
    public void RecordNoteOn(int midiNote, int velocity = 100)
    {
        _session.RecordNoteOn(midiNote, velocity);
    }
    
    public void RecordNoteOff(int midiNote)
    {
        _session.RecordNoteOff(midiNote);
    }
    
    public void RecordInstrumentChange(string instrumentId)
    {
        _session.RecordInstrumentChange(instrumentId);
    }
    
    public async Task<IReadOnlyList<RecordedEvent>?> LoadSongAsync(string songId)
    {
        try
        {
            var songDir = Path.Combine(_songsDirectory, songId);
            if (!Directory.Exists(songDir))
                return null;
            
            var metadataPath = Path.Combine(songDir, "metadata.json");
            var eventsPath = Path.Combine(songDir, "events.json");
            
            if (!File.Exists(metadataPath) || !File.Exists(eventsPath))
                return null;
            
            var metadataJson = await File.ReadAllTextAsync(metadataPath);
            var eventsJson = await File.ReadAllTextAsync(eventsPath);
            
            _currentSong = JsonSerializer.Deserialize<Song>(metadataJson);
            _loadedEvents = JsonSerializer.Deserialize<List<RecordedEvent>>(eventsJson);
            
            return _loadedEvents;
        }
        catch
        {
            return null;
        }
    }
    
    public async Task<IReadOnlyList<Song>> GetSongsAsync()
    {
        var songs = new List<Song>();
        
        if (!Directory.Exists(_songsDirectory))
            return songs;
        
        foreach (var dir in Directory.GetDirectories(_songsDirectory))
        {
            var metadataPath = Path.Combine(dir, "metadata.json");
            if (File.Exists(metadataPath))
            {
                try
                {
                    var json = await File.ReadAllTextAsync(metadataPath);
                    var song = JsonSerializer.Deserialize<Song>(json);
                    if (song != null)
                        songs.Add(song);
                }
                catch
                {
                    // Skip corrupted songs
                }
            }
        }
        
        return songs.OrderByDescending(s => s.CreatedAt).ToList();
    }
    
    public async Task<bool> DeleteSongAsync(string songId)
    {
        try
        {
            var songDir = Path.Combine(_songsDirectory, songId);
            if (Directory.Exists(songDir))
            {
                Directory.Delete(songDir, recursive: true);
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }
    
    private async Task SaveSongAsync(Song song, List<RecordedEvent> events)
    {
        var songDir = Path.Combine(_songsDirectory, song.Id);
        Directory.CreateDirectory(songDir);
        
        var metadataPath = Path.Combine(songDir, "metadata.json");
        var eventsPath = Path.Combine(songDir, "events.json");
        
        var metadataJson = JsonSerializer.Serialize(song, new JsonSerializerOptions { WriteIndented = true });
        var eventsJson = JsonSerializer.Serialize(events, new JsonSerializerOptions { WriteIndented = true });
        
        await File.WriteAllTextAsync(metadataPath, metadataJson);
        await File.WriteAllTextAsync(eventsPath, eventsJson);
    }
}


