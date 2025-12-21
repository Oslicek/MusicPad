using CommunityToolkit.Maui.Storage;
using Microsoft.Maui.Controls.Shapes;
using MusicPad.Core.Recording;
using MusicPad.Core.Theme;
using MusicPad.Services;

namespace MusicPad.Views;

public partial class SongsPage : ContentPage
{
    private readonly IRecordingService _recordingService;
    private readonly ISfzService _sfzService;
    private List<Song> _songs = new();
    
    public SongsPage(IRecordingService recordingService, ISfzService sfzService)
    {
        InitializeComponent();
        _recordingService = recordingService;
        _sfzService = sfzService;
        ApplyTheme();
    }
    
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadSongsAsync();
    }
    
    private void ApplyTheme()
    {
        HeaderBar.BackgroundColor = Color.FromArgb(AppColors.Surface);
        BackArrow.TextColor = Color.FromArgb(AppColors.Accent);
        HeaderLabel.TextColor = Color.FromArgb(AppColors.TextPrimary);
    }
    
    private async Task LoadSongsAsync()
    {
        _songs = (await _recordingService.GetSongsAsync()).ToList();
        
        SongsList.Children.Clear();
        
        if (_songs.Count == 0)
        {
            SongsList.Children.Add(EmptyLabel);
            EmptyLabel.IsVisible = true;
            return;
        }
        
        EmptyLabel.IsVisible = false;
        
        foreach (var song in _songs)
        {
            var card = CreateSongCard(song);
            SongsList.Children.Add(card);
        }
    }
    
    private Border CreateSongCard(Song song)
    {
        var duration = TimeSpan.FromMilliseconds(song.DurationMs);
        var durationStr = duration.TotalMinutes >= 1 
            ? $"{(int)duration.TotalMinutes}:{duration.Seconds:D2}"
            : $"0:{duration.Seconds:D2}";
        
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            },
            RowDefinitions = new RowDefinitionCollection
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto)
            },
            Padding = new Thickness(12, 10)
        };
        
        // Song name
        var nameLabel = new Label
        {
            Text = song.Name,
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb(AppColors.TextPrimary),
            LineBreakMode = LineBreakMode.TailTruncation
        };
        Grid.SetRow(nameLabel, 0);
        Grid.SetColumn(nameLabel, 0);
        
        // Duration
        var durationLabel = new Label
        {
            Text = durationStr,
            FontSize = 14,
            TextColor = Color.FromArgb(AppColors.Accent),
            VerticalOptions = LayoutOptions.Center
        };
        Grid.SetRow(durationLabel, 0);
        Grid.SetColumn(durationLabel, 1);
        
        // Metadata row
        var dateStr = song.CreatedAt.ToLocalTime().ToString("MMM d, yyyy HH:mm");
        var instrumentCount = song.Instruments?.Count ?? 0;
        var metaText = instrumentCount > 0 
            ? $"{dateStr} • {instrumentCount} instrument{(instrumentCount > 1 ? "s" : "")}"
            : dateStr;
        
        var metaLabel = new Label
        {
            Text = metaText,
            FontSize = 12,
            TextColor = Color.FromArgb(AppColors.TextMuted),
            Margin = new Thickness(0, 4, 0, 0)
        };
        Grid.SetRow(metaLabel, 1);
        Grid.SetColumn(metaLabel, 0);
        Grid.SetColumnSpan(metaLabel, 2);
        
        grid.Children.Add(nameLabel);
        grid.Children.Add(durationLabel);
        grid.Children.Add(metaLabel);
        
        var border = new Border
        {
            Content = grid,
            BackgroundColor = Color.FromArgb(AppColors.Surface),
            Stroke = Color.FromArgb(AppColors.BorderDark),
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = 8 },
            Margin = new Thickness(0, 0, 0, 4)
        };
        
        // Tap to show actions
        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += async (s, e) => await ShowSongActionsAsync(song);
        border.GestureRecognizers.Add(tapGesture);
        
        return border;
    }
    
    private async Task ShowSongActionsAsync(Song song)
    {
        var action = await DisplayActionSheet(
            song.Name, 
            "Cancel", 
            null,
            "▶ Play",
            "✎ Rename",
            "📤 Export & Share",
            "💾 Export & Save",
            "🗑 Delete");
        
        switch (action)
        {
            case "▶ Play":
                await PlaySongAsync(song);
                break;
            case "✎ Rename":
                await RenameSongAsync(song);
                break;
            case "📤 Export & Share":
                await ShowExportOptionsAsync(song, shareAfterExport: true);
                break;
            case "💾 Export & Save":
                await ShowExportOptionsAsync(song, shareAfterExport: false);
                break;
            case "🗑 Delete":
                await DeleteSongAsync(song);
                break;
        }
    }
    
    private async Task PlaySongAsync(Song song)
    {
        var events = await _recordingService.LoadSongAsync(song.Id);
        if (events != null && events.Count > 0)
        {
            // Load initial instrument if available
            if (!string.IsNullOrEmpty(song.InitialInstrumentId))
            {
                await _sfzService.LoadInstrumentAsync(song.InitialInstrumentId);
            }
            
            // Start playback via audio thread
            _sfzService.LoadPlaybackEvents(events);
            _sfzService.StartPlayback();
            
            await DisplayAlert("Playing", $"Playing: {song.Name}", "OK");
        }
        else
        {
            await DisplayAlert("Error", "Could not load song events.", "OK");
        }
    }
    
    private async Task RenameSongAsync(Song song)
    {
        var newName = await DisplayPromptAsync(
            "Rename Song",
            "Enter a new name:",
            initialValue: song.Name,
            maxLength: 100,
            keyboard: Keyboard.Text);
        
        if (!string.IsNullOrWhiteSpace(newName) && newName != song.Name)
        {
            var success = await _recordingService.RenameSongAsync(song.Id, newName.Trim());
            if (success)
            {
                await LoadSongsAsync();
            }
            else
            {
                await DisplayAlert("Error", "Could not rename song.", "OK");
            }
        }
    }
    
    private async Task ShowExportOptionsAsync(Song song, bool shareAfterExport)
    {
        var format = await DisplayActionSheet(
            "Export Format",
            "Cancel",
            null,
            "MIDI (Notes only)",
            "MIDI (With effects)",
            "MIDI (Complete - baked)",
            "WAV (Audio)",
            "MP3 (Compressed audio)",
            "FLAC (Lossless audio)");
        
        if (format == "Cancel" || string.IsNullOrEmpty(format))
            return;
        
        ExportFormat exportFormat = format switch
        {
            "MIDI (Notes only)" => ExportFormat.MidiNaked,
            "MIDI (With effects)" => ExportFormat.MidiEnhanced,
            "MIDI (Complete - baked)" => ExportFormat.MidiComplete,
            "WAV (Audio)" => ExportFormat.Wav,
            "MP3 (Compressed audio)" => ExportFormat.Mp3,
            "FLAC (Lossless audio)" => ExportFormat.Flac,
            _ => ExportFormat.MidiNaked
        };
        
        if (shareAfterExport)
        {
            await ExportAndShareAsync(song, exportFormat);
        }
        else
        {
            await ExportAndSaveAsync(song, exportFormat);
        }
    }
    
    private async Task ExportAndShareAsync(Song song, ExportFormat format)
    {
        try
        {
            var events = await _recordingService.LoadSongAsync(song.Id);
            if (events == null || events.Count == 0)
            {
                await DisplayAlert("Error", "Could not load song for export.", "OK");
                return;
            }
            
            var exportService = new ExportService(_sfzService);
            var (filePath, mimeType) = await exportService.ExportAsync(song, events, format);
            
            if (filePath != null)
            {
                // Share the file
                await Share.Default.RequestAsync(new ShareFileRequest
                {
                    Title = $"Export {song.Name}",
                    File = new ShareFile(filePath, mimeType)
                });
            }
            else
            {
                await DisplayAlert("Error", "Export failed.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Export failed: {ex.Message}", "OK");
        }
    }
    
    private async Task ExportAndSaveAsync(Song song, ExportFormat format)
    {
        try
        {
            var events = await _recordingService.LoadSongAsync(song.Id);
            if (events == null || events.Count == 0)
            {
                await DisplayAlert("Error", "Could not load song for export.", "OK");
                return;
            }
            
            var exportService = new ExportService(_sfzService);
            var (filePath, mimeType) = await exportService.ExportAsync(song, events, format);
            
            if (filePath == null)
            {
                await DisplayAlert("Error", "Export failed.", "OK");
                return;
            }
            
            // Get file extension from the exported file
            var extension = System.IO.Path.GetExtension(filePath);
            var suggestedName = $"{SanitizeFileName(song.Name)}{extension}";
            
            // Use FileSaver to save to user-chosen location
            var fileBytes = await File.ReadAllBytesAsync(filePath);
            using var stream = new MemoryStream(fileBytes);
            
            var result = await FileSaver.Default.SaveAsync(suggestedName, stream, CancellationToken.None);
            
            if (result.IsSuccessful)
            {
                await DisplayAlert("Success", $"Saved to:\n{result.FilePath}", "OK");
            }
            else if (!string.IsNullOrEmpty(result.Exception?.Message))
            {
                await DisplayAlert("Error", $"Could not save file: {result.Exception.Message}", "OK");
            }
            // If user cancelled, no message needed
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Export failed: {ex.Message}", "OK");
        }
    }
    
    private static string SanitizeFileName(string name)
    {
        var invalid = System.IO.Path.GetInvalidFileNameChars();
        return new string(name.Where(c => !invalid.Contains(c)).ToArray()).Replace(' ', '_');
    }
    
    private async Task DeleteSongAsync(Song song)
    {
        var confirm = await DisplayAlert(
            "Delete Song",
            $"Are you sure you want to delete '{song.Name}'?",
            "Delete",
            "Cancel");
        
        if (confirm)
        {
            var success = await _recordingService.DeleteSongAsync(song.Id);
            if (success)
            {
                await LoadSongsAsync();
            }
            else
            {
                await DisplayAlert("Error", "Could not delete song.", "OK");
            }
        }
    }
    
    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}

public enum ExportFormat
{
    MidiNaked,      // Just notes and timing
    MidiEnhanced,   // Notes + instrument changes, effects as MIDI CCs
    MidiComplete,   // Harmony/arpeggio baked in + instruments + effects
    Wav,            // Rendered audio (WAV)
    Mp3,            // Compressed audio (MP3) - requires FFmpeg
    Flac            // Lossless audio (FLAC)
}

