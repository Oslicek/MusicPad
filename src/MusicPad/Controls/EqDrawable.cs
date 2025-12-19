using Microsoft.Maui.Graphics;
using MusicPad.Core.Models;
using MusicPad.Core.Theme;

namespace MusicPad.Controls;

/// <summary>
/// Drawable for 4-band EQ with vertical sliders.
/// </summary>
public class EqDrawable
{
    private readonly EqualizerSettings _settings;
    
    // Colors
    private static readonly Color SliderTrackColor = Color.FromArgb(AppColors.SliderTrack);
    private static readonly Color SliderFillColor = Color.FromArgb(AppColors.SliderFill);
    private static readonly Color SliderThumbColor = Color.FromArgb(AppColors.SliderThumb);
    private static readonly Color SliderThumbHighlight = Color.FromArgb(AppColors.SliderThumbHighlight);
    private static readonly Color LabelColor = Color.FromArgb(AppColors.TextSecondary);
    private static readonly Color CenterLineColor = Color.FromArgb(AppColors.SliderCenterLine);

    private readonly RectF[] _sliderRects = new RectF[4];
    private readonly float[] _sliderTrackTops = new float[4];
    private readonly float[] _sliderTrackBottoms = new float[4];
    private int _draggingSlider = -1;

    public event EventHandler? InvalidateRequested;

    public EqDrawable(EqualizerSettings settings)
    {
        _settings = settings;
        _settings.BandChanged += (s, e) => InvalidateRequested?.Invoke(this, EventArgs.Empty);
    }

    public EqualizerSettings Settings => _settings;

    /// <summary>
    /// Draws the EQ sliders in a compact layout.
    /// </summary>
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        float padding = 4;
        float labelHeight = 10;
        float sliderWidth = Math.Min((dirtyRect.Width - padding * 5) / 4, 25f);
        float totalWidth = sliderWidth * 4 + padding * 3;
        float startX = dirtyRect.X + (dirtyRect.Width - totalWidth) / 2;
        
        float trackHeight = dirtyRect.Height - labelHeight - padding * 2;
        trackHeight = Math.Max(trackHeight, 30f);
        float trackWidth = 4f;

        for (int i = 0; i < 4; i++)
        {
            float x = startX + i * (sliderWidth + padding);
            float centerX = x + sliderWidth / 2;
            float trackTop = dirtyRect.Y + padding;
            float trackBottom = trackTop + trackHeight;
            float trackCenterY = (trackTop + trackBottom) / 2;

            // Store for hit testing
            _sliderRects[i] = new RectF(x, trackTop - 5, sliderWidth, trackHeight + 10);
            _sliderTrackTops[i] = trackTop;
            _sliderTrackBottoms[i] = trackBottom;

            // Draw track background
            canvas.FillColor = SliderTrackColor;
            var trackRect = new RectF(centerX - trackWidth / 2, trackTop, trackWidth, trackHeight);
            canvas.FillRoundedRectangle(trackRect, trackWidth / 2);

            // Draw center line (0 position)
            canvas.StrokeColor = CenterLineColor;
            canvas.StrokeSize = 1;
            canvas.DrawLine(centerX - sliderWidth / 3, trackCenterY, centerX + sliderWidth / 3, trackCenterY);

            // Calculate thumb position from gain (-1 to 1 maps to bottom to top)
            float gain = _settings.GetGain(i);
            float thumbY = trackCenterY - gain * (trackHeight / 2 - 6);
            thumbY = Math.Clamp(thumbY, trackTop + 6, trackBottom - 6);

            // Draw fill from center to thumb
            if (Math.Abs(gain) > 0.01f)
            {
                canvas.FillColor = SliderFillColor;
                if (gain > 0)
                {
                    canvas.FillRoundedRectangle(
                        new RectF(centerX - trackWidth / 2, thumbY, trackWidth, trackCenterY - thumbY),
                        trackWidth / 2);
                }
                else
                {
                    canvas.FillRoundedRectangle(
                        new RectF(centerX - trackWidth / 2, trackCenterY, trackWidth, thumbY - trackCenterY),
                        trackWidth / 2);
                }
            }

            // Draw thumb
            float thumbWidth = sliderWidth * 0.6f;
            float thumbHeight = 8f;
            var thumbRect = new RectF(centerX - thumbWidth / 2, thumbY - thumbHeight / 2, thumbWidth, thumbHeight);
            
            // Shadow
            canvas.FillColor = Color.FromArgb(AppColors.KnobIndicator);
            canvas.FillRoundedRectangle(new RectF(thumbRect.X + 1, thumbRect.Y + 1, thumbRect.Width, thumbRect.Height), 2);
            
            // Thumb body
            canvas.FillColor = SliderThumbColor;
            canvas.FillRoundedRectangle(thumbRect, 2);
            
            // Highlight
            canvas.FillColor = SliderThumbHighlight.WithAlpha(0.4f);
            canvas.FillRoundedRectangle(new RectF(thumbRect.X + 1, thumbRect.Y + 1, thumbRect.Width - 2, thumbRect.Height / 2 - 1), 1);

            // Draw label
            string label = GetShortLabel(i);
            canvas.FontSize = 8;
            canvas.FontColor = LabelColor;
            canvas.DrawString(label, x, trackBottom + 1, sliderWidth, labelHeight,
                HorizontalAlignment.Center, VerticalAlignment.Top);
        }
    }

    private string GetShortLabel(int band)
    {
        return band switch
        {
            0 => "L",
            1 => "LM",
            2 => "HM",
            3 => "H",
            _ => ""
        };
    }

    public bool OnTouch(float x, float y, bool isStart)
    {
        var point = new PointF(x, y);

        if (isStart)
        {
            for (int i = 0; i < 4; i++)
            {
                if (_sliderRects[i].Contains(point))
                {
                    _draggingSlider = i;
                    UpdateSliderValue(i, y);
                    return true;
                }
            }
        }
        else if (_draggingSlider >= 0)
        {
            UpdateSliderValue(_draggingSlider, y);
            return true;
        }

        return false;
    }

    private void UpdateSliderValue(int sliderIndex, float y)
    {
        float trackTop = _sliderTrackTops[sliderIndex];
        float trackBottom = _sliderTrackBottoms[sliderIndex];
        float trackCenterY = (trackTop + trackBottom) / 2;
        float halfRange = (trackBottom - trackTop) / 2 - 6;

        // Clamp y to track bounds
        y = Math.Clamp(y, trackTop + 6, trackBottom - 6);

        // Convert y position to gain (-1 to 1)
        // Top = +1, Center = 0, Bottom = -1
        float gain = -(y - trackCenterY) / halfRange;
        gain = Math.Clamp(gain, -1f, 1f);

        _settings.SetGain(sliderIndex, gain);
    }

    public void OnTouchEnd()
    {
        _draggingSlider = -1;
    }
}
