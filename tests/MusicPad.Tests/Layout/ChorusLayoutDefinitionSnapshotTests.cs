using MusicPad.Core.Layout;

namespace MusicPad.Tests.Layout;

/// <summary>
/// Snapshot tests for ChorusLayoutDefinition.
/// These tests capture the exact layout coordinates and fail if they change unexpectedly.
/// If layout changes are intentional, delete the .verified.txt files and re-run to update.
/// </summary>
public class ChorusLayoutDefinitionSnapshotTests
{
    private readonly ChorusLayoutDefinition _definition = new();

    [Fact]
    public Task Definition_Landscape_Default()
    {
        var bounds = new RectF(0, 0, 400, 100);
        var context = new LayoutContext
        {
            Orientation = Orientation.Landscape,
            AspectRatio = 2.0f,  // Normal aspect ratio (not narrow, not wide)
            PadreaShape = PadreaShape.Piano  // Use Piano to get default constants
        };
        
        var result = _definition.Calculate(bounds, context);
        
        return Verifier.Verify(LayoutToVerifiable(result, bounds, context, "Default"));
    }

    [Fact]
    public Task Definition_Landscape_Piano()
    {
        var bounds = new RectF(0, 0, 400, 100);
        var context = LayoutContext.Horizontal(aspectRatio: 2.0f, PadreaShape.Piano);
        
        var result = _definition.Calculate(bounds, context);
        
        return Verifier.Verify(LayoutToVerifiable(result, bounds, context, "Piano"));
    }

    [Fact]
    public Task Definition_Landscape_NarrowAspect()
    {
        var bounds = new RectF(0, 0, 120, 100);
        var context = LayoutContext.Horizontal(aspectRatio: 1.2f);  // Near-square
        
        var result = _definition.Calculate(bounds, context);
        
        return Verifier.Verify(LayoutToVerifiable(result, bounds, context, "NarrowAspect"));
    }

    [Fact]
    public Task Definition_Landscape_WideAspect()
    {
        var bounds = new RectF(0, 0, 500, 100);
        var context = LayoutContext.Horizontal(aspectRatio: 3.0f);  // Wide
        
        var result = _definition.Calculate(bounds, context);
        
        return Verifier.Verify(LayoutToVerifiable(result, bounds, context, "WideAspect"));
    }

    [Fact]
    public Task Definition_Portrait()
    {
        var bounds = new RectF(0, 0, 100, 300);
        var context = LayoutContext.Vertical(aspectRatio: 0.33f);
        
        var result = _definition.Calculate(bounds, context);
        
        return Verifier.Verify(LayoutToVerifiable(result, bounds, context, "Portrait"));
    }

    [Fact]
    public Task Definition_SmallHeight_KnobsCapped()
    {
        var bounds = new RectF(0, 0, 300, 50);
        var context = LayoutContext.Horizontal(aspectRatio: 6.0f);
        
        var result = _definition.Calculate(bounds, context);
        
        return Verifier.Verify(LayoutToVerifiable(result, bounds, context, "SmallHeight"));
    }

    /// <summary>
    /// Converts layout result to a verifiable object with metadata.
    /// </summary>
    private static object LayoutToVerifiable(LayoutResult result, RectF bounds, LayoutContext context, string variant)
    {
        return new
        {
            Variant = variant,
            Bounds = new { bounds.X, bounds.Y, bounds.Width, bounds.Height },
            Context = new
            {
                context.Orientation,
                context.AspectRatio,
                context.PadreaShape,
                context.IsLandscape,
                context.IsPortrait
            },
            Elements = result.Elements.ToDictionary(
                kvp => kvp.Key,
                kvp => new
                {
                    X = Round(kvp.Value.X),
                    Y = Round(kvp.Value.Y),
                    Width = Round(kvp.Value.Width),
                    Height = Round(kvp.Value.Height),
                    CenterX = Round(kvp.Value.CenterX),
                    CenterY = Round(kvp.Value.CenterY)
                }),
            Validation = new
            {
                AllFitWithinBounds = result.AllFitWithin(bounds),
                HasOverlaps = result.HasOverlaps()
            }
        };
    }

    private static float Round(float value) => MathF.Round(value, 2);
}

