namespace MusicPad.Core.Layout;

/// <summary>
/// Fluent layout definition for EQ slider controls.
/// 
/// Note: EQ has a unique layout pattern - 4 evenly-spaced sliders centered horizontally.
/// This pattern doesn't fit the standard linear DSL, so we implement the layout
/// imperatively while reusing the EqLayoutCalculator constants.
/// </summary>
public class EqLayoutDefinition : ILayoutCalculator
{
    // Element names - same as calculator
    public const string Slider0 = EqLayoutCalculator.Slider0;
    public const string Slider1 = EqLayoutCalculator.Slider1;
    public const string Slider2 = EqLayoutCalculator.Slider2;
    public const string Slider3 = EqLayoutCalculator.Slider3;

    // Singleton instance for reuse
    private static EqLayoutDefinition? _instance;
    public static EqLayoutDefinition Instance => _instance ??= new EqLayoutDefinition();

    // Layout constants - use same values as calculator
    private const float Padding = EqLayoutCalculator.Padding;
    private const float LabelHeight = EqLayoutCalculator.LabelHeight;
    private const float MaxSliderWidth = EqLayoutCalculator.MaxSliderWidth;
    private const float SliderHitPadding = EqLayoutCalculator.SliderHitPadding;

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
        float trackHeight = EqLayoutCalculator.GetTrackHeight(bounds.Height);

        // Total slider height including label
        float totalSliderHeight = trackHeight + LabelHeight;

        // Center vertically
        float verticalOffset = (bounds.Height - totalSliderHeight) / 2;
        float trackTop = bounds.Y + verticalOffset;

        // Slider hit rect includes padding above/below track
        float sliderY = trackTop - SliderHitPadding;
        float sliderHeight = trackHeight + SliderHitPadding * 2;

        // Create slider rects
        result[Slider0] = new RectF(startX, sliderY, sliderWidth, sliderHeight);
        result[Slider1] = new RectF(startX + (sliderWidth + Padding), sliderY, sliderWidth, sliderHeight);
        result[Slider2] = new RectF(startX + 2 * (sliderWidth + Padding), sliderY, sliderWidth, sliderHeight);
        result[Slider3] = new RectF(startX + 3 * (sliderWidth + Padding), sliderY, sliderWidth, sliderHeight);

        return result;
    }

    /// <summary>
    /// Gets the track height for a given bounds height (delegates to calculator).
    /// </summary>
    public static float GetTrackHeight(float boundsHeight) => 
        EqLayoutCalculator.GetTrackHeight(boundsHeight);

    /// <summary>
    /// Gets the track top/bottom positions for a given bounds (delegates to calculator).
    /// </summary>
    public static (float trackTop, float trackBottom) GetTrackBounds(RectF bounds) => 
        EqLayoutCalculator.GetTrackBounds(bounds);
}




