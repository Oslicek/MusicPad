namespace MusicPad.Core.Layout;

/// <summary>
/// Calculates layout positions for EQ slider controls.
/// Elements: 4 vertical sliders (Slider0, Slider1, Slider2, Slider3)
/// 
/// This is the reference implementation - EqLayoutDefinition should produce identical results.
/// Layout constants traced from original EqDrawable to ensure identical layout.
/// </summary>
public class EqLayoutCalculator : ILayoutCalculator
{
    // Element names
    public const string Slider0 = "Slider0";
    public const string Slider1 = "Slider1";
    public const string Slider2 = "Slider2";
    public const string Slider3 = "Slider3";

    // Layout constants - traced from original EqDrawable
    public const float Padding = 4f;
    public const float LabelHeight = 14f;
    public const float MaxSliderWidth = 28f;
    public const float TrackWidthVisual = 5f;
    public const float MinTrackHeight = 30f;
    public const float MaxTrackHeight = 80f;
    public const float TrackHeightRatio = 0.65f;
    public const float SliderHitPadding = 5f;  // Extra padding above track for hit testing

    public LayoutResult Calculate(RectF bounds, LayoutContext context)
    {
        var result = new LayoutResult();

        // Calculate slider width: (width - padding * 5) / 4, capped at MaxSliderWidth
        float sliderWidth = Math.Min((bounds.Width - Padding * 5) / 4, MaxSliderWidth);
        
        // Calculate total width of all sliders and gaps
        float totalWidth = sliderWidth * 4 + Padding * 3;
        
        // Center horizontally
        float startX = bounds.X + (bounds.Width - totalWidth) / 2;

        // Calculate track height (clamped)
        float trackHeight = GetTrackHeight(bounds.Height);

        // Total slider height including label
        float totalSliderHeight = trackHeight + LabelHeight;

        // Center vertically
        float verticalOffset = (bounds.Height - totalSliderHeight) / 2;
        float trackTop = bounds.Y + verticalOffset;

        // Slider hit rect includes padding above track
        float sliderY = trackTop - SliderHitPadding;
        float sliderHeight = trackHeight + SliderHitPadding * 2;

        // Create slider rects
        for (int i = 0; i < 4; i++)
        {
            float x = startX + i * (sliderWidth + Padding);
            string name = i switch
            {
                0 => Slider0,
                1 => Slider1,
                2 => Slider2,
                3 => Slider3,
                _ => throw new InvalidOperationException()
            };
            result[name] = new RectF(x, sliderY, sliderWidth, sliderHeight);
        }

        return result;
    }

    /// <summary>
    /// Gets the track height for a given bounds height.
    /// </summary>
    public static float GetTrackHeight(float boundsHeight)
    {
        float trackHeight = boundsHeight * TrackHeightRatio;
        return Math.Clamp(trackHeight, MinTrackHeight, MaxTrackHeight);
    }

    /// <summary>
    /// Gets the track top/bottom positions for a given bounds.
    /// </summary>
    public static (float trackTop, float trackBottom) GetTrackBounds(RectF bounds)
    {
        float trackHeight = GetTrackHeight(bounds.Height);
        float totalSliderHeight = trackHeight + LabelHeight;
        float verticalOffset = (bounds.Height - totalSliderHeight) / 2;
        float trackTop = bounds.Y + verticalOffset;
        float trackBottom = trackTop + trackHeight;
        return (trackTop, trackBottom);
    }
}




