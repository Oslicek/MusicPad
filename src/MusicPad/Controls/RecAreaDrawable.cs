using Microsoft.Maui.Graphics;
using MusicPad.Core.Theme;

namespace MusicPad.Controls;

/// <summary>
/// Drawable for the recording area controls (Record, Stop, Play).
/// </summary>
public class RecAreaDrawable : IDrawable
{
    private RectF _recordButtonRect;
    private RectF _playButtonRect;
    private RectF _statusRect;
    
    private bool _isRecording;
    private bool _isPlaying;
    private string _statusText = "";
    
    // Colors
    private static Color BackgroundColor => Color.FromArgb(AppColors.BackgroundEffect);
    private static Color RecordColor => Color.FromArgb("#E53935"); // Red
    private static Color RecordActiveColor => Color.FromArgb("#FF1744"); // Bright red
    private static Color PlayColor => Color.FromArgb("#43A047"); // Green
    private static Color PlayActiveColor => Color.FromArgb("#00E676"); // Bright green
    private static Color StopColor => Color.FromArgb("#FFA000"); // Amber
    private static Color ButtonBgColor => Color.FromArgb(AppColors.ButtonOff);
    private static Color TextColor => Color.FromArgb(AppColors.TextPrimary);
    private static Color TextSecondaryColor => Color.FromArgb(AppColors.TextSecondary);
    
    public event EventHandler? RecordClicked;
    public event EventHandler? StopClicked;
    public event EventHandler? PlayClicked;
    public event EventHandler? InvalidateRequested;
    
    public bool IsRecording
    {
        get => _isRecording;
        set
        {
            if (_isRecording != value)
            {
                _isRecording = value;
                InvalidateRequested?.Invoke(this, EventArgs.Empty);
            }
        }
    }
    
    public bool IsPlaying
    {
        get => _isPlaying;
        set
        {
            if (_isPlaying != value)
            {
                _isPlaying = value;
                InvalidateRequested?.Invoke(this, EventArgs.Empty);
            }
        }
    }
    
    public string StatusText
    {
        get => _statusText;
        set
        {
            if (_statusText != value)
            {
                _statusText = value;
                InvalidateRequested?.Invoke(this, EventArgs.Empty);
            }
        }
    }
    
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        float padding = 8f;
        float buttonSize = Math.Min(dirtyRect.Height - padding * 2, 40f);
        float cornerRadius = 6f;
        
        // Background
        canvas.FillColor = BackgroundColor;
        canvas.FillRoundedRectangle(dirtyRect, cornerRadius);
        
        // Border
        canvas.StrokeColor = Color.FromArgb(AppColors.BorderMedium);
        canvas.StrokeSize = 1f;
        canvas.DrawRoundedRectangle(dirtyRect, cornerRadius);
        
        // Layout: [REC/STOP] [Status...] [PLAY/STOP]
        float buttonY = dirtyRect.Y + (dirtyRect.Height - buttonSize) / 2;
        
        // Record button (left side)
        float recX = dirtyRect.X + padding;
        _recordButtonRect = new RectF(recX, buttonY, buttonSize, buttonSize);
        DrawRecordButton(canvas, _recordButtonRect);
        
        // Play button (right side)
        float playX = dirtyRect.Right - padding - buttonSize;
        _playButtonRect = new RectF(playX, buttonY, buttonSize, buttonSize);
        DrawPlayButton(canvas, _playButtonRect);
        
        // Status text (center)
        float statusX = recX + buttonSize + padding * 2;
        float statusWidth = playX - statusX - padding * 2;
        _statusRect = new RectF(statusX, dirtyRect.Y, statusWidth, dirtyRect.Height);
        DrawStatus(canvas, _statusRect);
    }
    
    private void DrawRecordButton(ICanvas canvas, RectF rect)
    {
        // Background
        canvas.FillColor = ButtonBgColor;
        canvas.FillRoundedRectangle(rect, rect.Height / 4);
        
        // Icon - circle for record, square for stop
        float iconSize = rect.Width * 0.4f;
        float iconX = rect.Center.X;
        float iconY = rect.Center.Y;
        
        if (_isRecording)
        {
            // Stop icon (square)
            canvas.FillColor = StopColor;
            var stopRect = new RectF(iconX - iconSize/2, iconY - iconSize/2, iconSize, iconSize);
            canvas.FillRoundedRectangle(stopRect, 2);
            
            // Pulsing border
            canvas.StrokeColor = RecordActiveColor;
            canvas.StrokeSize = 2;
            canvas.DrawRoundedRectangle(rect, rect.Height / 4);
        }
        else
        {
            // Record icon (circle)
            canvas.FillColor = RecordColor;
            canvas.FillCircle(iconX, iconY, iconSize);
        }
    }
    
    private void DrawPlayButton(ICanvas canvas, RectF rect)
    {
        // Background
        canvas.FillColor = ButtonBgColor;
        canvas.FillRoundedRectangle(rect, rect.Height / 4);
        
        float iconSize = rect.Width * 0.4f;
        float iconX = rect.Center.X;
        float iconY = rect.Center.Y;
        
        if (_isPlaying)
        {
            // Stop icon (square)
            canvas.FillColor = StopColor;
            var stopRect = new RectF(iconX - iconSize/2, iconY - iconSize/2, iconSize, iconSize);
            canvas.FillRoundedRectangle(stopRect, 2);
            
            // Active border
            canvas.StrokeColor = PlayActiveColor;
            canvas.StrokeSize = 2;
            canvas.DrawRoundedRectangle(rect, rect.Height / 4);
        }
        else
        {
            // Play icon (triangle)
            canvas.FillColor = PlayColor;
            var path = new PathF();
            path.MoveTo(iconX - iconSize * 0.4f, iconY - iconSize * 0.5f);
            path.LineTo(iconX + iconSize * 0.6f, iconY);
            path.LineTo(iconX - iconSize * 0.4f, iconY + iconSize * 0.5f);
            path.Close();
            canvas.FillPath(path);
        }
    }
    
    private void DrawStatus(ICanvas canvas, RectF rect)
    {
        if (string.IsNullOrEmpty(_statusText))
        {
            // Draw placeholder
            canvas.FontSize = 12;
            canvas.FontColor = TextSecondaryColor;
            canvas.DrawString("Ready", rect.X, rect.Y, rect.Width, rect.Height,
                HorizontalAlignment.Center, VerticalAlignment.Center);
        }
        else
        {
            canvas.FontSize = 12;
            canvas.FontColor = _isRecording ? RecordActiveColor : (_isPlaying ? PlayActiveColor : TextColor);
            canvas.DrawString(_statusText, rect.X, rect.Y, rect.Width, rect.Height,
                HorizontalAlignment.Center, VerticalAlignment.Center);
        }
    }
    
    public void OnTouchStart(PointF point)
    {
        if (_recordButtonRect.Contains(point))
        {
            if (_isRecording)
            {
                StopClicked?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                RecordClicked?.Invoke(this, EventArgs.Empty);
            }
        }
        else if (_playButtonRect.Contains(point))
        {
            if (_isPlaying)
            {
                StopClicked?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                PlayClicked?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}

