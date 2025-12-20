using Microsoft.Maui.Graphics;
using MusicPad.Core.Theme;

namespace MusicPad.Controls;

/// <summary>
/// Drawable for the navigation bar above pads: [MUTE] [▲] o●o [▼]
/// </summary>
public class NavigationBarDrawable : IDrawable
{
    private static Color AccentColor => Color.FromArgb(AppColors.Accent);
    private static Color TextColor => Color.FromArgb(AppColors.TextSecondary);
    private static Color BackgroundColor => Color.FromArgb(AppColors.BackgroundPicker);
    private static Color BorderColor => Color.FromArgb(AppColors.BorderDark);
    
    private int _currentPage = 0;
    private int _totalPages = 1;
    private bool _isMuted = false;
    
    private RectF _muteRect;
    private RectF _upArrowRect;
    private RectF _downArrowRect;
    
    public event EventHandler? MuteClicked;
    public event EventHandler? NavigateUp;
    public event EventHandler? NavigateDown;
    public event EventHandler? InvalidateRequested;
    
    public int CurrentPage
    {
        get => _currentPage;
        set
        {
            if (_currentPage != value)
            {
                _currentPage = value;
                InvalidateRequested?.Invoke(this, EventArgs.Empty);
            }
        }
    }
    
    public int TotalPages
    {
        get => _totalPages;
        set
        {
            if (_totalPages != value)
            {
                _totalPages = Math.Max(1, value);
                InvalidateRequested?.Invoke(this, EventArgs.Empty);
            }
        }
    }
    
    public bool IsMuted
    {
        get => _isMuted;
        set
        {
            if (_isMuted != value)
            {
                _isMuted = value;
                InvalidateRequested?.Invoke(this, EventArgs.Empty);
            }
        }
    }
    
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        float padding = 8f;
        float buttonHeight = dirtyRect.Height - padding * 2;
        float muteWidth = 70f;
        float arrowSize = 36f;
        float dotRadius = 5f;
        float dotSpacing = 12f;
        
        float totalDotsWidth = _totalPages * dotRadius * 2 + (_totalPages - 1) * (dotSpacing - dotRadius * 2);
        
        // Calculate total width of navigation elements
        float elementsWidth = muteWidth + padding + arrowSize + padding + totalDotsWidth + padding + arrowSize;
        float startX = (dirtyRect.Width - elementsWidth) / 2;
        
        // Draw MUTE button
        _muteRect = new RectF(startX, padding, muteWidth, buttonHeight);
        DrawMuteButton(canvas, _muteRect);
        
        // Draw Up arrow
        float upX = _muteRect.Right + padding;
        _upArrowRect = new RectF(upX, padding, arrowSize, buttonHeight);
        DrawArrow(canvas, _upArrowRect, isUp: true, enabled: _currentPage < _totalPages - 1);
        
        // Draw page indicator dots
        float dotsX = _upArrowRect.Right + padding;
        DrawPageDots(canvas, dotsX, dirtyRect.Height / 2, dotRadius, dotSpacing);
        
        // Draw Down arrow
        float downX = dotsX + totalDotsWidth + padding;
        _downArrowRect = new RectF(downX, padding, arrowSize, buttonHeight);
        DrawArrow(canvas, _downArrowRect, isUp: false, enabled: _currentPage > 0);
    }
    
    private void DrawMuteButton(ICanvas canvas, RectF rect)
    {
        // Background
        canvas.FillColor = BackgroundColor;
        canvas.FillRoundedRectangle(rect, 8);
        
        // Border
        canvas.StrokeColor = BorderColor;
        canvas.StrokeSize = 2;
        canvas.DrawRoundedRectangle(rect, 8);
        
        // Text
        canvas.FontSize = 13;
        canvas.FontColor = AccentColor;
        canvas.DrawString("MUTE", rect, HorizontalAlignment.Center, VerticalAlignment.Center);
    }
    
    private void DrawArrow(ICanvas canvas, RectF rect, bool isUp, bool enabled)
    {
        float arrowSize = Math.Min(rect.Width, rect.Height) * 0.5f;
        float centerX = rect.Center.X;
        float centerY = rect.Center.Y;
        
        Color arrowColor = enabled ? AccentColor : TextColor.WithAlpha(0.4f);
        
        canvas.StrokeColor = arrowColor;
        canvas.FillColor = arrowColor;
        canvas.StrokeSize = 2.5f;
        canvas.StrokeLineCap = LineCap.Round;
        canvas.StrokeLineJoin = LineJoin.Round;
        
        var path = new PathF();
        if (isUp)
        {
            // Triangle pointing up
            path.MoveTo(centerX, centerY - arrowSize * 0.5f);
            path.LineTo(centerX - arrowSize * 0.6f, centerY + arrowSize * 0.4f);
            path.LineTo(centerX + arrowSize * 0.6f, centerY + arrowSize * 0.4f);
            path.Close();
        }
        else
        {
            // Triangle pointing down
            path.MoveTo(centerX, centerY + arrowSize * 0.5f);
            path.LineTo(centerX - arrowSize * 0.6f, centerY - arrowSize * 0.4f);
            path.LineTo(centerX + arrowSize * 0.6f, centerY - arrowSize * 0.4f);
            path.Close();
        }
        
        canvas.FillPath(path);
    }
    
    private void DrawPageDots(ICanvas canvas, float startX, float centerY, float radius, float spacing)
    {
        for (int i = 0; i < _totalPages; i++)
        {
            float dotX = startX + i * spacing + radius;
            bool isActive = i == _currentPage;
            
            if (isActive)
            {
                canvas.FillColor = AccentColor;
                canvas.FillCircle(dotX, centerY, radius);
            }
            else
            {
                canvas.StrokeColor = TextColor;
                canvas.StrokeSize = 1.5f;
                canvas.DrawCircle(dotX, centerY, radius);
            }
        }
    }
    
    public bool OnTouchStart(float x, float y)
    {
        var point = new PointF(x, y);
        
        if (_muteRect.Contains(point))
        {
            MuteClicked?.Invoke(this, EventArgs.Empty);
            return true;
        }
        
        if (_upArrowRect.Contains(point) && _currentPage < _totalPages - 1)
        {
            NavigateUp?.Invoke(this, EventArgs.Empty);
            return true;
        }
        
        if (_downArrowRect.Contains(point) && _currentPage > 0)
        {
            NavigateDown?.Invoke(this, EventArgs.Empty);
            return true;
        }
        
        return false;
    }
}

