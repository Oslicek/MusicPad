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
    
    // Colors (dynamic for palette switching)
    private static Color SliderTrackColor => Color.FromArgb(AppColors.SliderTrack);
    private static Color SliderFillColor => Color.FromArgb(AppColors.SliderFill);
    private static Color AccentColor => Color.FromArgb(AppColors.Accent);
    private static Color LabelColor => Color.FromArgb(AppColors.TextSecondary);
    private static Color CenterLineColor => Color.FromArgb(AppColors.SliderCenterLine);
    private static Color GrooveColor => Color.FromArgb(AppColors.KnobShadow);

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

            // Draw track groove (inset look for skeuomorphic style)
            canvas.FillColor = GrooveColor.WithAlpha(0.4f);
            canvas.FillRoundedRectangle(new RectF(centerX - trackWidth / 2 - 1, trackTop - 1, trackWidth + 2, trackHeight + 2), trackWidth / 2 + 1);
            
            // Draw track background
            canvas.FillColor = SliderTrackColor;
            var trackRect = new RectF(centerX - trackWidth / 2, trackTop, trackWidth, trackHeight);
            canvas.FillRoundedRectangle(trackRect, trackWidth / 2);

            // Draw tick marks along the track (hardware EQ style)
            canvas.StrokeColor = CenterLineColor.WithAlpha(0.4f);
            canvas.StrokeSize = 1;
            float tickSpacing = trackHeight / 6;
            for (int t = 0; t <= 6; t++)
            {
                float tickY = trackTop + t * tickSpacing;
                float tickWidth = (t == 3) ? sliderWidth / 2.5f : sliderWidth / 4; // Center tick is longer
                canvas.DrawLine(centerX - tickWidth, tickY, centerX + tickWidth, tickY);
            }
            
            // Draw center line (0 position) - emphasized
            canvas.StrokeColor = CenterLineColor;
            canvas.StrokeSize = 1.5f;
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

            // Draw thumb - bigger, more skeuomorphic with orange accent
            float thumbWidth = sliderWidth * 0.85f;  // Bigger width
            float thumbHeight = 12f;  // Taller
            var thumbRect = new RectF(centerX - thumbWidth / 2, thumbY - thumbHeight / 2, thumbWidth, thumbHeight);
            
            // Shadow for depth
            canvas.FillColor = GrooveColor.WithAlpha(0.6f);
            canvas.FillRoundedRectangle(new RectF(thumbRect.X + 1, thumbRect.Y + 2, thumbRect.Width, thumbRect.Height), 3);
            
            // Thumb body - orange accent color
            canvas.FillColor = AccentColor;
            canvas.FillRoundedRectangle(thumbRect, 3);
            
            // Top highlight for 3D effect
            canvas.FillColor = Colors.White.WithAlpha(0.3f);
            canvas.FillRoundedRectangle(new RectF(thumbRect.X + 1, thumbRect.Y + 1, thumbRect.Width - 2, 3), 2);
            
            // Center groove line on thumb (hardware EQ style)
            canvas.StrokeColor = GrooveColor.WithAlpha(0.5f);
            canvas.StrokeSize = 1;
            canvas.DrawLine(centerX - thumbWidth / 4, thumbY, centerX + thumbWidth / 4, thumbY);

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
