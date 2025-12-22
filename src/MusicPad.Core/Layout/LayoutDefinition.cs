namespace MusicPad.Core.Layout;

/// <summary>
/// Base class for fluent layout definitions with cascading condition-based overrides.
/// </summary>
public abstract class LayoutDefinition : ILayoutCalculator
{
    private readonly Dictionary<string, float> _defaultConstants = new();
    private readonly Dictionary<string, ElementSpec> _defaultElements = new();
    private readonly List<LayoutVariant> _variants = new();

    /// <summary>
    /// Defines the default layout that applies when no conditions match.
    /// </summary>
    protected LayoutBuilder Default() => new(this, null);

    /// <summary>
    /// Defines a layout variant for a specific orientation.
    /// </summary>
    protected LayoutBuilder When(Orientation orientation) =>
        new(this, new OrientationCondition(orientation));

    /// <summary>
    /// Defines a layout variant for a specific padrea shape.
    /// </summary>
    protected LayoutBuilder When(PadreaShape shape) =>
        new(this, new PadreaShapeCondition(shape));

    /// <summary>
    /// Defines a layout variant for an aspect ratio condition.
    /// </summary>
    protected LayoutBuilder When(LayoutCondition condition) =>
        new(this, condition);

    /// <summary>
    /// Defines a layout variant for multiple conditions (all must match).
    /// </summary>
    protected LayoutBuilder When(Orientation orientation, PadreaShape shape) =>
        new(this, new CompositeCondition(new LayoutCondition[]
        {
            new OrientationCondition(orientation),
            new PadreaShapeCondition(shape)
        }));

    /// <summary>
    /// Defines a layout variant for multiple conditions (all must match).
    /// </summary>
    protected LayoutBuilder When(LayoutCondition condition, Orientation orientation) =>
        new(this, new CompositeCondition(new LayoutCondition[]
        {
            condition,
            new OrientationCondition(orientation)
        }));

    /// <summary>
    /// Defines a layout variant for multiple conditions (all must match).
    /// </summary>
    protected LayoutBuilder When(Orientation orientation, PadreaShape shape, LayoutCondition condition) =>
        new(this, new CompositeCondition(new LayoutCondition[]
        {
            new OrientationCondition(orientation),
            new PadreaShapeCondition(shape),
            condition
        }));

    internal void SetDefaultConstant(string name, float value) => _defaultConstants[name] = value;
    internal void SetDefaultElement(string name, ElementSpec spec) => _defaultElements[name] = spec;
    internal void AddVariant(LayoutVariant variant) => _variants.Add(variant);

    /// <summary>
    /// Calculates the layout for the given bounds and context.
    /// </summary>
    public LayoutResult Calculate(RectF bounds, LayoutContext context)
    {
        // Start with default constants
        var constants = new Dictionary<string, float>(_defaultConstants);

        // Start with default element specs
        var elements = new Dictionary<string, ElementSpec>();
        foreach (var (name, spec) in _defaultElements)
        {
            elements[name] = spec.Clone();
        }

        // Apply matching variants in order of specificity (least specific first)
        var matchingVariants = _variants
            .Where(v => v.Condition?.Matches(context) ?? false)
            .OrderBy(v => v.Condition?.Specificity ?? 0)
            .ToList();

        foreach (var variant in matchingVariants)
        {
            // Merge constants
            foreach (var (name, value) in variant.Constants)
            {
                constants[name] = value;
            }

            // Merge element specs
            foreach (var (name, spec) in variant.Elements)
            {
                if (elements.TryGetValue(name, out var existing))
                {
                    elements[name] = existing.MergeWith(spec);
                }
                else
                {
                    elements[name] = spec.Clone();
                }
            }
        }

        // Resolve element positions
        return ResolveLayout(bounds, context, constants, elements);
    }

    private LayoutResult ResolveLayout(
        RectF bounds,
        LayoutContext context,
        Dictionary<string, float> constants,
        Dictionary<string, ElementSpec> elements)
    {
        var result = new LayoutResult();
        var resolved = new Dictionary<string, RectF>();

        // Resolve each element
        foreach (var (name, spec) in elements)
        {
            var rect = ResolveElement(bounds, context, constants, resolved, spec);
            result[name] = rect;
            resolved[name] = rect;
        }

        return result;
    }

    private RectF ResolveElement(
        RectF bounds,
        LayoutContext context,
        Dictionary<string, float> constants,
        Dictionary<string, RectF> resolved,
        ElementSpec spec)
    {
        // Resolve size first
        float width = ResolveSize(spec.Width, bounds, context, constants, isWidth: true);
        float height = ResolveSize(spec.Height, bounds, context, constants, isWidth: false);

        // Resolve position
        float x = ResolveX(spec, bounds, constants, resolved, width);
        float y = ResolveY(spec, bounds, constants, resolved, height);

        return new RectF(x, y, width, height);
    }

    private float ResolveSize(SizeSpec? spec, RectF bounds, LayoutContext context,
        Dictionary<string, float> constants, bool isWidth)
    {
        if (spec == null)
            return isWidth ? bounds.Width : bounds.Height;

        return spec.Type switch
        {
            SizeType.Fixed => spec.Value,
            SizeType.Constant => constants.GetValueOrDefault(spec.ConstantName!, spec.Value),
            SizeType.Percentage => (isWidth ? bounds.Width : bounds.Height) * spec.Value / 100f,
            SizeType.KnobSize => ResolveKnobSize(bounds, constants, spec),
            _ => spec.Value
        };
    }

    private float ResolveKnobSize(RectF bounds, Dictionary<string, float> constants, SizeSpec spec)
    {
        // Get diameter from constant or value
        float diameter = spec.ConstantName != null 
            ? constants.GetValueOrDefault(spec.ConstantName, 52f)
            : spec.Value;
        
        // Get hit padding from constant or value
        float hitPadding = spec.KnobPaddingConstant != null
            ? constants.GetValueOrDefault(spec.KnobPaddingConstant, 5f)
            : spec.KnobPadding ?? 5f;
        
        // Get vertical margin from constant or value
        float verticalMargin = spec.KnobVerticalMarginConstant != null
            ? constants.GetValueOrDefault(spec.KnobVerticalMarginConstant, 16f)
            : spec.KnobVerticalMargin ?? 16f;

        // Cap diameter to available height minus margin
        float actualDiameter = Math.Min(bounds.Height - verticalMargin, diameter);
        
        // Return total hit rect size (diameter + padding on each side)
        return actualDiameter + hitPadding * 2;
    }

    private float ResolveX(ElementSpec spec, RectF bounds, Dictionary<string, float> constants,
        Dictionary<string, RectF> resolved, float width)
    {
        return spec.XPosition?.Type switch
        {
            PositionType.Left => bounds.X + ResolveValue(spec.XPosition.Value, spec.XPosition.ConstantName, constants),
            PositionType.Right => bounds.Right - width - ResolveValue(spec.XPosition.Value, spec.XPosition.ConstantName, constants),
            PositionType.Center => bounds.X + (bounds.Width - width) / 2,
            PositionType.After => ResolveAfter(spec.XPosition, resolved, constants, isX: true),
            PositionType.Before => ResolveBefore(spec.XPosition, resolved, constants, width, isX: true),
            _ => bounds.X
        };
    }

    private float ResolveY(ElementSpec spec, RectF bounds, Dictionary<string, float> constants,
        Dictionary<string, RectF> resolved, float height)
    {
        return spec.YPosition?.Type switch
        {
            PositionType.Top => bounds.Y + ResolveValue(spec.YPosition.Value, spec.YPosition.ConstantName, constants),
            PositionType.Bottom => bounds.Bottom - height - ResolveValue(spec.YPosition.Value, spec.YPosition.ConstantName, constants),
            PositionType.Center => bounds.Y + (bounds.Height - height) / 2,
            PositionType.After => ResolveAfter(spec.YPosition, resolved, constants, isX: false),
            PositionType.Before => ResolveBefore(spec.YPosition, resolved, constants, height, isX: false),
            _ => bounds.Y
        };
    }

    private float ResolveAfter(PositionSpec pos, Dictionary<string, RectF> resolved,
        Dictionary<string, float> constants, bool isX)
    {
        if (pos.RelativeTo == null || !resolved.TryGetValue(pos.RelativeTo, out var ref_rect))
            return 0;

        float spacing = ResolveValue(pos.Value, pos.ConstantName, constants);
        return isX ? ref_rect.Right + spacing : ref_rect.Bottom + spacing;
    }

    private float ResolveBefore(PositionSpec pos, Dictionary<string, RectF> resolved,
        Dictionary<string, float> constants, float size, bool isX)
    {
        if (pos.RelativeTo == null || !resolved.TryGetValue(pos.RelativeTo, out var ref_rect))
            return 0;

        float spacing = ResolveValue(pos.Value, pos.ConstantName, constants);
        return isX ? ref_rect.Left - spacing - size : ref_rect.Top - spacing - size;
    }

    private float ResolveValue(float value, string? constantName, Dictionary<string, float> constants)
    {
        if (constantName != null && constants.TryGetValue(constantName, out var constValue))
            return constValue;
        return value;
    }
}

/// <summary>
/// A layout variant with a condition and overrides.
/// </summary>
internal class LayoutVariant
{
    public LayoutCondition? Condition { get; set; }
    public Dictionary<string, float> Constants { get; } = new();
    public Dictionary<string, ElementSpec> Elements { get; } = new();
}

/// <summary>
/// Specification for an element's position and size.
/// </summary>
internal class ElementSpec
{
    public PositionSpec? XPosition { get; set; }
    public PositionSpec? YPosition { get; set; }
    public SizeSpec? Width { get; set; }
    public SizeSpec? Height { get; set; }

    public ElementSpec Clone() => new()
    {
        XPosition = XPosition?.Clone(),
        YPosition = YPosition?.Clone(),
        Width = Width?.Clone(),
        Height = Height?.Clone()
    };

    public ElementSpec MergeWith(ElementSpec other)
    {
        return new ElementSpec
        {
            XPosition = other.XPosition ?? XPosition,
            YPosition = other.YPosition ?? YPosition,
            Width = other.Width ?? Width,
            Height = other.Height ?? Height
        };
    }
}

/// <summary>
/// Position specification.
/// </summary>
internal class PositionSpec
{
    public PositionType Type { get; set; }
    public float Value { get; set; }
    public string? ConstantName { get; set; }
    public string? RelativeTo { get; set; }

    public PositionSpec Clone() => new()
    {
        Type = Type,
        Value = Value,
        ConstantName = ConstantName,
        RelativeTo = RelativeTo
    };
}

internal enum PositionType
{
    Left,
    Right,
    Top,
    Bottom,
    Center,
    After,
    Before
}

/// <summary>
/// Size specification.
/// </summary>
internal class SizeSpec
{
    public SizeType Type { get; set; }
    public float Value { get; set; }
    public string? ConstantName { get; set; }
    
    // Knob sizing - simplified: diameter + hit padding (no ratio)
    public float? KnobPadding { get; set; }
    public float? KnobVerticalMargin { get; set; }
    public string? KnobPaddingConstant { get; set; }
    public string? KnobVerticalMarginConstant { get; set; }

    public SizeSpec Clone() => new()
    {
        Type = Type,
        Value = Value,
        ConstantName = ConstantName,
        KnobPadding = KnobPadding,
        KnobVerticalMargin = KnobVerticalMargin,
        KnobPaddingConstant = KnobPaddingConstant,
        KnobVerticalMarginConstant = KnobVerticalMarginConstant
    };
}

internal enum SizeType
{
    Fixed,
    Constant,
    Percentage,
    KnobSize
}

