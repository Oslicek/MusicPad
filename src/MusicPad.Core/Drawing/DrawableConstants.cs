namespace MusicPad.Core.Drawing;

/// <summary>
/// Shared constants for all effect drawable controls.
/// Centralizes magic numbers to ensure consistency across drawables.
/// </summary>
public static class DrawableConstants
{
    // Knob rotation angles (in degrees)
    // Knob starts at 7 o'clock (225°) and ends at 5 o'clock (-45°)
    public const float KnobMinAngle = 225f;
    public const float KnobMaxAngle = -45f;
    
    // Knob proportions (relative to radius)
    public const float KnobHighlightOffset = 0.15f;
    public const float KnobHighlightRadius = 0.6f;
    public const float KnobInnerRadius = 0.85f;
    public const float KnobIndicatorDistance = 0.7f;
    public const float KnobIndicatorRadius = 0.12f;
    
    // Knob markers (tick marks around the knob)
    public const int KnobMarkerCount = 6;
    public const float KnobMarkerStrokeWidth = 1.5f;
    public const float KnobMarkerOuterOffset = 5f;
    public const float KnobMarkerInnerOffset = 2f;
    
    // Hit rect padding - must match layout constants
    public const float KnobHitPadding = 5f;
    
    // Toggle button proportions (relative to button rect)
    public const float ToggleWidthRatio = 0.85f;
    public const float ToggleHeightRatio = 0.5f;
    public const float ToggleKnobRatio = 0.4f;
    
    // Typography
    public const float LabelHeight = 12f;
    public const float LabelOffsetY = 4f;
    public const float FontSizeSmall = 7f;
    public const float FontSizeMedium = 8f;
    public const float FontSizeLarge = 9f;
    
    /// <summary>
    /// Calculates total rotation angle for knob (always negative for clockwise).
    /// </summary>
    public static float GetTotalKnobAngle()
    {
        float totalAngle = KnobMaxAngle - KnobMinAngle;
        if (totalAngle > 0) totalAngle -= 360;
        return totalAngle;
    }
}


