namespace MusicPad.Core.Layout;

/// <summary>
/// Represents a calculated layout with named element rectangles.
/// </summary>
public class LayoutResult
{
    private readonly Dictionary<string, RectF> _elements = new();

    /// <summary>
    /// Gets or sets a rectangle for the specified element name.
    /// </summary>
    public RectF this[string name]
    {
        get => _elements.TryGetValue(name, out var rect) ? rect : RectF.Zero;
        set => _elements[name] = value;
    }

    /// <summary>
    /// Checks if an element exists in the layout.
    /// </summary>
    public bool HasElement(string name) => _elements.ContainsKey(name);

    /// <summary>
    /// Gets all element names in this layout.
    /// </summary>
    public IEnumerable<string> ElementNames => _elements.Keys;

    /// <summary>
    /// Gets all elements as key-value pairs.
    /// </summary>
    public IReadOnlyDictionary<string, RectF> Elements => _elements;

    /// <summary>
    /// Checks if all elements fit within the specified bounds.
    /// </summary>
    public bool AllFitWithin(RectF bounds)
    {
        foreach (var rect in _elements.Values)
        {
            if (rect.Left < bounds.Left || rect.Top < bounds.Top ||
                rect.Right > bounds.Right || rect.Bottom > bounds.Bottom)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Checks if any elements overlap with each other.
    /// </summary>
    public bool HasOverlaps()
    {
        var rects = _elements.Values.ToList();
        for (int i = 0; i < rects.Count; i++)
        {
            for (int j = i + 1; j < rects.Count; j++)
            {
                if (rects[i].IntersectsWith(rects[j]))
                {
                    return true;
                }
            }
        }
        return false;
    }
}

/// <summary>
/// Simple rectangle structure for layout calculations (platform-independent).
/// </summary>
public readonly struct RectF
{
    public float X { get; }
    public float Y { get; }
    public float Width { get; }
    public float Height { get; }

    public RectF(float x, float y, float width, float height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    public float Left => X;
    public float Top => Y;
    public float Right => X + Width;
    public float Bottom => Y + Height;
    public float CenterX => X + Width / 2;
    public float CenterY => Y + Height / 2;
    public PointF Center => new(CenterX, CenterY);

    public static RectF Zero => new(0, 0, 0, 0);

    public bool Contains(RectF other) =>
        other.Left >= Left && other.Top >= Top &&
        other.Right <= Right && other.Bottom <= Bottom;

    public bool Contains(PointF point) =>
        point.X >= Left && point.X <= Right &&
        point.Y >= Top && point.Y <= Bottom;

    public bool IntersectsWith(RectF other) =>
        Left < other.Right && Right > other.Left &&
        Top < other.Bottom && Bottom > other.Top;

    public override string ToString() => $"RectF(X={X}, Y={Y}, W={Width}, H={Height})";
}

/// <summary>
/// Simple point structure for layout calculations.
/// </summary>
public readonly struct PointF
{
    public float X { get; }
    public float Y { get; }

    public PointF(float x, float y)
    {
        X = x;
        Y = y;
    }

    public override string ToString() => $"PointF({X}, {Y})";
}

