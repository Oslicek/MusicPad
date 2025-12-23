namespace MusicPad.Core.Layout;

/// <summary>
/// Fluent builder for defining layout variants.
/// </summary>
public class LayoutBuilder
{
    private readonly LayoutDefinition _definition;
    private readonly LayoutCondition? _condition;
    private readonly LayoutVariant? _variant;

    internal LayoutBuilder(LayoutDefinition definition, LayoutCondition? condition)
    {
        _definition = definition;
        _condition = condition;

        if (condition != null)
        {
            _variant = new LayoutVariant { Condition = condition };
            definition.AddVariant(_variant);
        }
    }

    /// <summary>
    /// Defines constants for this layout variant.
    /// </summary>
    public LayoutBuilder Constants(Action<ConstantsBuilder> configure)
    {
        var builder = new ConstantsBuilder(_definition, _variant);
        configure(builder);
        return this;
    }

    /// <summary>
    /// Starts defining an element's position and size.
    /// </summary>
    public ElementBuilder Element(string name) => new(_definition, _variant, name, this);

    /// <summary>
    /// Finalizes this layout variant (for variants with only constants, no elements).
    /// </summary>
    public LayoutBuilder Done() => this;
}

/// <summary>
/// Fluent builder for defining constants.
/// </summary>
public class ConstantsBuilder
{
    private readonly LayoutDefinition _definition;
    private readonly LayoutVariant? _variant;

    internal ConstantsBuilder(LayoutDefinition definition, LayoutVariant? variant)
    {
        _definition = definition;
        _variant = variant;
    }

    /// <summary>
    /// Sets a constant value.
    /// </summary>
    public ConstantsBuilder Set(string name, float value)
    {
        if (_variant != null)
            _variant.Constants[name] = value;
        else
            _definition.SetDefaultConstant(name, value);
        return this;
    }
}

/// <summary>
/// Fluent builder for defining element position and size.
/// </summary>
public class ElementBuilder
{
    private readonly LayoutDefinition _definition;
    private readonly LayoutVariant? _variant;
    private readonly string _name;
    private readonly LayoutBuilder _parent;
    private readonly ElementSpec _spec = new();

    internal ElementBuilder(LayoutDefinition definition, LayoutVariant? variant, string name, LayoutBuilder parent)
    {
        _definition = definition;
        _variant = variant;
        _name = name;
        _parent = parent;
    }

    // === X Position ===

    /// <summary>
    /// Positions element at left edge with offset.
    /// </summary>
    public ElementBuilder Left(float offset)
    {
        _spec.XPosition = new PositionSpec { Type = PositionType.Left, Value = offset };
        return this;
    }

    /// <summary>
    /// Positions element at left edge with constant offset.
    /// </summary>
    public ElementBuilder Left(string constantName)
    {
        _spec.XPosition = new PositionSpec { Type = PositionType.Left, ConstantName = constantName };
        return this;
    }

    /// <summary>
    /// Positions element at right edge with offset.
    /// </summary>
    public ElementBuilder Right(float offset)
    {
        _spec.XPosition = new PositionSpec { Type = PositionType.Right, Value = offset };
        return this;
    }

    /// <summary>
    /// Centers element horizontally.
    /// </summary>
    public ElementBuilder HCenter()
    {
        _spec.XPosition = new PositionSpec { Type = PositionType.Center };
        return this;
    }

    /// <summary>
    /// Positions element after another element (to the right).
    /// </summary>
    public ElementBuilder After(string elementName, float spacing = 0)
    {
        _spec.XPosition = new PositionSpec
        {
            Type = PositionType.After,
            RelativeTo = elementName,
            Value = spacing
        };
        return this;
    }

    /// <summary>
    /// Positions element after another element with constant spacing.
    /// </summary>
    public ElementBuilder After(string elementName, string spacingConstant)
    {
        _spec.XPosition = new PositionSpec
        {
            Type = PositionType.After,
            RelativeTo = elementName,
            ConstantName = spacingConstant
        };
        return this;
    }

    // === Y Position ===

    /// <summary>
    /// Positions element at top edge with offset.
    /// </summary>
    public ElementBuilder Top(float offset)
    {
        _spec.YPosition = new PositionSpec { Type = PositionType.Top, Value = offset };
        return this;
    }

    /// <summary>
    /// Positions element at top edge with constant offset.
    /// </summary>
    public ElementBuilder Top(string constantName)
    {
        _spec.YPosition = new PositionSpec { Type = PositionType.Top, ConstantName = constantName };
        return this;
    }

    /// <summary>
    /// Positions element at bottom edge with offset.
    /// </summary>
    public ElementBuilder Bottom(float offset)
    {
        _spec.YPosition = new PositionSpec { Type = PositionType.Bottom, Value = offset };
        return this;
    }

    /// <summary>
    /// Centers element vertically.
    /// </summary>
    public ElementBuilder VCenter()
    {
        _spec.YPosition = new PositionSpec { Type = PositionType.Center };
        return this;
    }

    /// <summary>
    /// Centers element vertically with an offset (positive = down, negative = up).
    /// </summary>
    public ElementBuilder VCenter(float offset)
    {
        _spec.YPosition = new PositionSpec { Type = PositionType.Center, Value = offset };
        return this;
    }

    /// <summary>
    /// Aligns element's top edge with another element's top edge.
    /// </summary>
    public ElementBuilder AlignTop(string elementName)
    {
        _spec.YPosition = new PositionSpec
        {
            Type = PositionType.AlignTop,
            RelativeTo = elementName
        };
        return this;
    }

    /// <summary>
    /// Positions element below another element.
    /// </summary>
    public ElementBuilder Below(string elementName, float spacing = 0)
    {
        _spec.YPosition = new PositionSpec
        {
            Type = PositionType.After,
            RelativeTo = elementName,
            Value = spacing
        };
        return this;
    }

    /// <summary>
    /// Positions element below another element with constant spacing.
    /// </summary>
    public ElementBuilder Below(string elementName, string spacingConstant)
    {
        _spec.YPosition = new PositionSpec
        {
            Type = PositionType.After,
            RelativeTo = elementName,
            ConstantName = spacingConstant
        };
        return this;
    }

    // === Size ===

    /// <summary>
    /// Sets fixed size (width = height = size).
    /// </summary>
    public ElementBuilder Size(float size)
    {
        _spec.Width = new SizeSpec { Type = SizeType.Fixed, Value = size };
        _spec.Height = new SizeSpec { Type = SizeType.Fixed, Value = size };
        return this;
    }

    /// <summary>
    /// Sets fixed size from constant.
    /// </summary>
    public ElementBuilder Size(string constantName)
    {
        _spec.Width = new SizeSpec { Type = SizeType.Constant, ConstantName = constantName };
        _spec.Height = new SizeSpec { Type = SizeType.Constant, ConstantName = constantName };
        return this;
    }

    /// <summary>
    /// Sets fixed width and height.
    /// </summary>
    public ElementBuilder Size(float width, float height)
    {
        _spec.Width = new SizeSpec { Type = SizeType.Fixed, Value = width };
        _spec.Height = new SizeSpec { Type = SizeType.Fixed, Value = height };
        return this;
    }

    /// <summary>
    /// Sets size for a knob element using constants for diameter, hit padding, and vertical margin.
    /// The actual size is: min(boundsHeight - verticalMargin, diameter) + hitPadding * 2
    /// </summary>
    public ElementBuilder KnobSize(string diameterConstant, string hitPaddingConstant, string verticalMarginConstant)
    {
        _spec.Width = new SizeSpec
        {
            Type = SizeType.KnobSize,
            ConstantName = diameterConstant,
            KnobPaddingConstant = hitPaddingConstant,
            KnobVerticalMarginConstant = verticalMarginConstant
        };
        _spec.Height = new SizeSpec
        {
            Type = SizeType.KnobSize,
            ConstantName = diameterConstant,
            KnobPaddingConstant = hitPaddingConstant,
            KnobVerticalMarginConstant = verticalMarginConstant
        };
        return this;
    }

    /// <summary>
    /// Sets size for a knob element with fixed diameter value.
    /// </summary>
    public ElementBuilder KnobSize(float diameter, float hitPadding = 5f, float verticalMargin = 16f)
    {
        _spec.Width = new SizeSpec
        {
            Type = SizeType.KnobSize,
            Value = diameter,
            KnobPadding = hitPadding,
            KnobVerticalMargin = verticalMargin
        };
        _spec.Height = new SizeSpec
        {
            Type = SizeType.KnobSize,
            Value = diameter,
            KnobPadding = hitPadding,
            KnobVerticalMargin = verticalMargin
        };
        return this;
    }

    /// <summary>
    /// Sets size for a button that fits within height: Min(height - margin*2, maxSize).
    /// Used for buttons that should be square but scale down in short containers.
    /// </summary>
    public ElementBuilder ButtonSize(string maxSizeConstant, string marginConstant)
    {
        _spec.Width = new SizeSpec
        {
            Type = SizeType.ButtonSize,
            ConstantName = maxSizeConstant,
            ButtonMarginConstant = marginConstant
        };
        _spec.Height = new SizeSpec
        {
            Type = SizeType.ButtonSize,
            ConstantName = maxSizeConstant,
            ButtonMarginConstant = marginConstant
        };
        return this;
    }

    /// <summary>
    /// Sets size for a button with fixed values.
    /// </summary>
    public ElementBuilder ButtonSize(float maxSize, float margin = 8f)
    {
        _spec.Width = new SizeSpec
        {
            Type = SizeType.ButtonSize,
            Value = maxSize,
            ButtonMargin = margin
        };
        _spec.Height = new SizeSpec
        {
            Type = SizeType.ButtonSize,
            Value = maxSize,
            ButtonMargin = margin
        };
        return this;
    }

    /// <summary>
    /// Sets height to fill the container.
    /// </summary>
    public ElementBuilder FillHeight()
    {
        _spec.Height = new SizeSpec { Type = SizeType.FillHeight };
        return this;
    }

    /// <summary>
    /// Sets width to a fixed value.
    /// </summary>
    public ElementBuilder Width(float width)
    {
        _spec.Width = new SizeSpec { Type = SizeType.Fixed, Value = width };
        return this;
    }

    /// <summary>
    /// Sets width from a constant.
    /// </summary>
    public ElementBuilder Width(string constantName)
    {
        _spec.Width = new SizeSpec { Type = SizeType.Constant, ConstantName = constantName };
        return this;
    }

    /// <summary>
    /// Sets height to a fixed value.
    /// </summary>
    public ElementBuilder Height(float height)
    {
        _spec.Height = new SizeSpec { Type = SizeType.Fixed, Value = height };
        return this;
    }

    /// <summary>
    /// Sets height from a constant.
    /// </summary>
    public ElementBuilder Height(string constantName)
    {
        _spec.Height = new SizeSpec { Type = SizeType.Constant, ConstantName = constantName };
        return this;
    }

    /// <summary>
    /// Positions element to the left of another element (before it).
    /// </summary>
    public ElementBuilder Before(string elementName, float spacing = 0)
    {
        _spec.XPosition = new PositionSpec
        {
            Type = PositionType.Before,
            RelativeTo = elementName,
            Value = spacing
        };
        return this;
    }

    /// <summary>
    /// Positions element to the left of another element with constant spacing.
    /// </summary>
    public ElementBuilder Before(string elementName, string spacingConstant)
    {
        _spec.XPosition = new PositionSpec
        {
            Type = PositionType.Before,
            RelativeTo = elementName,
            ConstantName = spacingConstant
        };
        return this;
    }

    /// <summary>
    /// Positions element at right edge with constant offset.
    /// </summary>
    public ElementBuilder Right(string constantName)
    {
        _spec.XPosition = new PositionSpec { Type = PositionType.Right, ConstantName = constantName };
        return this;
    }

    /// <summary>
    /// Sets width to fill the space between two elements with padding.
    /// </summary>
    public ElementBuilder FillWidthBetween(string afterElement, string beforeElement, string paddingConstant)
    {
        _spec.Width = new SizeSpec
        {
            Type = SizeType.FillBetween,
            FillAfterElement = afterElement,
            FillBeforeElement = beforeElement,
            FillPaddingConstant = paddingConstant
        };
        return this;
    }

    /// <summary>
    /// Sets width to fill the space between two elements with fixed padding.
    /// </summary>
    public ElementBuilder FillWidthBetween(string afterElement, string beforeElement, float padding = 8f)
    {
        _spec.Width = new SizeSpec
        {
            Type = SizeType.FillBetween,
            FillAfterElement = afterElement,
            FillBeforeElement = beforeElement,
            FillPadding = padding
        };
        return this;
    }

    // === Chaining ===

    /// <summary>
    /// Finishes this element and starts defining another.
    /// </summary>
    public ElementBuilder Element(string name)
    {
        Commit();
        return new ElementBuilder(_definition, _variant, name, _parent);
    }

    /// <summary>
    /// Defines constants for the parent layout.
    /// </summary>
    public LayoutBuilder Constants(Action<ConstantsBuilder> configure)
    {
        Commit();
        return _parent.Constants(configure);
    }

    private void Commit()
    {
        if (_variant != null)
            _variant.Elements[_name] = _spec;
        else
            _definition.SetDefaultElement(_name, _spec);
    }

    // Implicit commit when the builder goes out of scope
    ~ElementBuilder()
    {
        // Note: This won't work reliably, so we need explicit commits
    }

    /// <summary>
    /// Commits the element definition. Call at the end of element chain.
    /// </summary>
    public LayoutBuilder Done()
    {
        Commit();
        return _parent;
    }
}

