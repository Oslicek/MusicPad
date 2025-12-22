namespace MusicPad.Core.Layout;

/// <summary>
/// Represents a condition that must match for a layout variant to apply.
/// </summary>
public abstract class LayoutCondition
{
    /// <summary>
    /// Checks if this condition matches the given context.
    /// </summary>
    public abstract bool Matches(LayoutContext context);

    /// <summary>
    /// Returns the specificity of this condition (higher = more specific).
    /// </summary>
    public abstract int Specificity { get; }
}

/// <summary>
/// Condition that matches a specific orientation.
/// </summary>
public class OrientationCondition : LayoutCondition
{
    private readonly Orientation _orientation;

    public OrientationCondition(Orientation orientation)
    {
        _orientation = orientation;
    }

    public override bool Matches(LayoutContext context) => context.Orientation == _orientation;
    public override int Specificity => 1;

    public override string ToString() => $"Orientation={_orientation}";
}

/// <summary>
/// Condition that matches a specific padrea shape.
/// </summary>
public class PadreaShapeCondition : LayoutCondition
{
    private readonly PadreaShape _shape;

    public PadreaShapeCondition(PadreaShape shape)
    {
        _shape = shape;
    }

    public override bool Matches(LayoutContext context) => context.PadreaShape == _shape;
    public override int Specificity => 1;

    public override string ToString() => $"PadreaShape={_shape}";
}

/// <summary>
/// Condition that matches an aspect ratio range.
/// </summary>
public class AspectRatioCondition : LayoutCondition
{
    private readonly Func<float, bool> _predicate;
    private readonly string _description;

    public AspectRatioCondition(Func<float, bool> predicate, string description)
    {
        _predicate = predicate;
        _description = description;
    }

    public override bool Matches(LayoutContext context) => _predicate(context.AspectRatio);
    public override int Specificity => 1;

    public override string ToString() => _description;
}

/// <summary>
/// Composite condition that combines multiple conditions (all must match).
/// </summary>
public class CompositeCondition : LayoutCondition
{
    private readonly List<LayoutCondition> _conditions;

    public CompositeCondition(IEnumerable<LayoutCondition> conditions)
    {
        _conditions = conditions.ToList();
    }

    public override bool Matches(LayoutContext context) => _conditions.All(c => c.Matches(context));
    public override int Specificity => _conditions.Sum(c => c.Specificity);

    public override string ToString() => string.Join(" && ", _conditions);
}

/// <summary>
/// Helper class for creating aspect ratio conditions.
/// </summary>
public static class AspectRatio
{
    public static LayoutCondition LessThan(float ratio) =>
        new AspectRatioCondition(r => r < ratio, $"AspectRatio<{ratio}");

    public static LayoutCondition GreaterThan(float ratio) =>
        new AspectRatioCondition(r => r > ratio, $"AspectRatio>{ratio}");

    public static LayoutCondition Between(float min, float max) =>
        new AspectRatioCondition(r => r >= min && r <= max, $"AspectRatio[{min},{max}]");
}

