namespace MusicPad.Core.Layout;

/// <summary>
/// Interface for layout calculators that compute element positions.
/// </summary>
public interface ILayoutCalculator
{
    /// <summary>
    /// Calculates the layout for the given bounds and orientation.
    /// </summary>
    /// <param name="bounds">The available bounds for the layout.</param>
    /// <param name="context">Layout context with orientation and aspect ratio info.</param>
    /// <returns>A LayoutResult containing all element positions.</returns>
    LayoutResult Calculate(RectF bounds, LayoutContext context);
}

/// <summary>
/// Page orientation.
/// </summary>
public enum Orientation
{
    Portrait,
    Landscape
}

/// <summary>
/// Padrea shape type.
/// </summary>
public enum PadreaShape
{
    Square,
    Piano
}

/// <summary>
/// Context information for layout calculations.
/// </summary>
public readonly struct LayoutContext
{
    /// <summary>
    /// Page orientation (portrait or landscape).
    /// </summary>
    public Orientation Orientation { get; init; }

    /// <summary>
    /// The aspect ratio (width / height) of the page/bounds.
    /// </summary>
    public float AspectRatio { get; init; }

    /// <summary>
    /// The shape of the current padrea.
    /// </summary>
    public PadreaShape PadreaShape { get; init; }

    // Convenience properties
    public bool IsLandscape => Orientation == Orientation.Landscape;
    public bool IsPortrait => Orientation == Orientation.Portrait;
    public bool IsSquarePadrea => PadreaShape == PadreaShape.Square;
    public bool IsPianoPadrea => PadreaShape == PadreaShape.Piano;

    /// <summary>
    /// Whether the layout is horizontal (landscape) - for backwards compatibility.
    /// </summary>
    public bool IsHorizontal => IsLandscape;

    /// <summary>
    /// Whether this is a landscape layout with near-square aspect ratio.
    /// </summary>
    public bool IsLandscapeSquare => IsLandscape && AspectRatio < 1.3f;

    /// <summary>
    /// Creates a context from bounds dimensions.
    /// </summary>
    public static LayoutContext FromBounds(RectF bounds, PadreaShape padreaShape = PadreaShape.Square)
    {
        float aspectRatio = bounds.Height > 0 ? bounds.Width / bounds.Height : 1f;
        var orientation = aspectRatio > 1 ? Orientation.Landscape : Orientation.Portrait;

        return new LayoutContext
        {
            Orientation = orientation,
            AspectRatio = aspectRatio,
            PadreaShape = padreaShape
        };
    }

    /// <summary>
    /// Creates a horizontal (landscape) context.
    /// </summary>
    public static LayoutContext Horizontal(float aspectRatio = 2.0f, PadreaShape padreaShape = PadreaShape.Square) => new()
    {
        Orientation = Orientation.Landscape,
        AspectRatio = aspectRatio,
        PadreaShape = padreaShape
    };

    /// <summary>
    /// Creates a vertical (portrait) context.
    /// </summary>
    public static LayoutContext Vertical(float aspectRatio = 0.5f, PadreaShape padreaShape = PadreaShape.Square) => new()
    {
        Orientation = Orientation.Portrait,
        AspectRatio = aspectRatio,
        PadreaShape = padreaShape
    };
}

