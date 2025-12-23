namespace MusicPad.Core.Layout;

/// <summary>
/// Calculates layout positions for effect selector buttons.
/// Supports horizontal (landscape) and vertical (portrait) orientations.
/// 
/// Elements: Button0-4 (effect buttons), ControlsArea (remaining space for controls)
/// </summary>
public class EffectSelectorLayoutCalculator : ILayoutCalculator
{
    // Element names
    public const string Button0 = "Button0";  // ArpHarmony
    public const string Button1 = "Button1";  // EQ
    public const string Button2 = "Button2";  // Chorus
    public const string Button3 = "Button3";  // Delay
    public const string Button4 = "Button4";  // Reverb
    public const string ControlsArea = "ControlsArea";

    // Layout constants - traced from original EffectAreaDrawable
    public const float ButtonSize = 30f;
    public const float ButtonSpacing = 2f;
    public const float ButtonMargin = 4f;
    public const int ButtonCount = 5;

    public LayoutResult Calculate(RectF bounds, LayoutContext context)
    {
        return context.Orientation == Orientation.Portrait
            ? CalculateVertical(bounds)
            : CalculateHorizontal(bounds);
    }

    private LayoutResult CalculateHorizontal(RectF bounds)
    {
        var result = new LayoutResult();

        float startX = bounds.X + ButtonMargin;
        float startY = bounds.Y + ButtonMargin;

        // Create 5 buttons arranged horizontally
        for (int i = 0; i < ButtonCount; i++)
        {
            float x = startX + i * (ButtonSize + ButtonSpacing);
            result[$"Button{i}"] = new RectF(x, startY, ButtonSize, ButtonSize);
        }

        // Controls area is below buttons
        float controlsY = startY + ButtonSize + ButtonSpacing;
        result[ControlsArea] = new RectF(
            bounds.X,
            controlsY,
            bounds.Width,
            bounds.Height - (controlsY - bounds.Y));

        return result;
    }

    private LayoutResult CalculateVertical(RectF bounds)
    {
        var result = new LayoutResult();

        float startX = bounds.X + ButtonMargin;
        float startY = bounds.Y + ButtonMargin;

        // Create 5 buttons arranged vertically
        for (int i = 0; i < ButtonCount; i++)
        {
            float y = startY + i * (ButtonSize + ButtonSpacing);
            result[$"Button{i}"] = new RectF(startX, y, ButtonSize, ButtonSize);
        }

        // Controls area is to the right of buttons
        float controlsX = startX + ButtonSize + ButtonSpacing;
        result[ControlsArea] = new RectF(
            controlsX,
            bounds.Y,
            bounds.Width - (controlsX - bounds.X),
            bounds.Height);

        return result;
    }
}



