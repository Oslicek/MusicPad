using Microsoft.Maui.Graphics;
using MusicPad.Core.Layout;
using MusicPad.Core.Theme;
using CoreRectF = MusicPad.Core.Layout.RectF;
using MauiRectF = Microsoft.Maui.Graphics.RectF;
using MauiPointF = Microsoft.Maui.Graphics.PointF;

namespace MusicPad.Controls;

/// <summary>
/// Drawable for the recording area controls (Record, Stop, Play).
/// Uses RecAreaLayoutDefinition for layout calculations.
/// </summary>
public class RecAreaDrawable : IDrawable
{
    private MauiRectF _recordButtonRect;
    private MauiRectF _playButtonRect;
    private MauiRectF _statusRect;
    
    private bool _isRecording;
    private bool _isPlaying;
    private string _statusText = "";
    
    // Layout definition (singleton)
    private readonly RecAreaLayoutDefinition _layout = RecAreaLayoutDefinition.Instance;
    
    // Colors - dynamic palette colors
    private static Color BackgroundColor => Color.FromArgb(AppColors.BackgroundEffect);
    
    // Bronze accent for record button (warm copper/amber tone)
    private static Color RecordColor => Color.FromArgb(AppColors.Secondary); // Amber - bronze accent
    private static Color RecordActiveColor => Color.FromArgb(AppColors.Accent); // Orange - active bronze
    
    // Play button uses primary color (teal)
    private static Color PlayColor => Color.FromArgb(AppColors.Primary); // Teal
    private static Color PlayActiveColor => Color.FromArgb(AppColors.PrimaryLight); // Light teal
    
    // Stop uses secondary dark (darker amber)
    private static Color StopColor => Color.FromArgb(AppColors.SecondaryDark);
    
    private static Color ButtonBgColor => Color.FromArgb(AppColors.ButtonOff);
    private static Color TextColor => Color.FromArgb(AppColors.TextPrimary);
    private static Color TextSecondaryColor => Color.FromArgb(AppColors.TextSecondary);
    
    // Border color for the recording area - uses primary color
    private static Color BorderColor => Color.FromArgb(AppColors.Primary);
    
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
    
    public void Draw(ICanvas canvas, MauiRectF dirtyRect)
    {
        float cornerRadius = 6f;
        
        // Background
        canvas.FillColor = BackgroundColor;
        canvas.FillRoundedRectangle(dirtyRect, cornerRadius);
        
        // Border - uses primary color to match effect area style
        canvas.StrokeColor = BorderColor.WithAlpha(0.7f);
        canvas.StrokeSize = 2f;
        canvas.DrawRoundedRectangle(dirtyRect, cornerRadius);
        
        // Calculate layout using DSL definition
        var bounds = new CoreRectF(dirtyRect.X, dirtyRect.Y, dirtyRect.Width, dirtyRect.Height);
        var context = LayoutContext.Horizontal();
        var layoutResult = _layout.Calculate(bounds, context);
        
        // Get element rectangles from layout
        var recordRect = layoutResult[RecAreaLayoutDefinition.RecordButton];
        var playRect = layoutResult[RecAreaLayoutDefinition.PlayButton];
        var statusRect = layoutResult[RecAreaLayoutDefinition.StatusArea];
        
        // Convert to MAUI RectF and store for hit testing
        _recordButtonRect = new MauiRectF(recordRect.X, recordRect.Y, recordRect.Width, recordRect.Height);
        _playButtonRect = new MauiRectF(playRect.X, playRect.Y, playRect.Width, playRect.Height);
        _statusRect = new MauiRectF(statusRect.X, statusRect.Y, statusRect.Width, statusRect.Height);
        
        // Draw elements
        DrawRecordButton(canvas, _recordButtonRect);
        DrawPlayButton(canvas, _playButtonRect);
        DrawStatus(canvas, _statusRect);
    }
    
    private void DrawRecordButton(ICanvas canvas, MauiRectF rect)
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
            var stopRect = new MauiRectF(iconX - iconSize/2, iconY - iconSize/2, iconSize, iconSize);
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
    
    private void DrawPlayButton(ICanvas canvas, MauiRectF rect)
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
            var stopRect = new MauiRectF(iconX - iconSize/2, iconY - iconSize/2, iconSize, iconSize);
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
    
    private void DrawStatus(ICanvas canvas, MauiRectF rect)
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
    
    public void OnTouchStart(MauiPointF point)
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

