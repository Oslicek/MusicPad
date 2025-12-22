using MusicPad.Core.Layout;

namespace MusicPad.Tests.Layout;

/// <summary>
/// Snapshot tests for Chorus layout.
/// These tests capture the exact layout coordinates and fail if they change unexpectedly.
/// Run 'dotnet test' to verify layouts match expectations.
/// If layout changes are intentional, delete the .verified.txt files and re-run to update.
/// 
/// IMPORTANT: Tests cover BOTH Calculator (reference) and Definition (DSL).
/// The Definition is what ChorusDrawable uses, so it must be tested!
/// </summary>
public class ChorusLayoutSnapshotTests
{
    private readonly ChorusLayoutCalculator _calculator = new();
    private readonly ChorusLayoutDefinition _definition = new();

    [Fact]
    public Task Chorus_Landscape_Wide()
    {
        var bounds = new RectF(0, 0, 400, 100);
        var context = LayoutContext.Horizontal(aspectRatio: 4.0f);
        
        var result = _calculator.Calculate(bounds, context);
        
        return Verifier.Verify(LayoutToVerifiable(result, bounds, context));
    }

    [Fact]
    public Task Chorus_Landscape_Standard()
    {
        var bounds = new RectF(0, 0, 300, 80);
        var context = LayoutContext.Horizontal(aspectRatio: 3.75f);
        
        var result = _calculator.Calculate(bounds, context);
        
        return Verifier.Verify(LayoutToVerifiable(result, bounds, context));
    }

    [Fact]
    public Task Chorus_Landscape_Compact()
    {
        var bounds = new RectF(0, 0, 200, 60);
        var context = LayoutContext.Horizontal(aspectRatio: 3.33f);
        
        var result = _calculator.Calculate(bounds, context);
        
        return Verifier.Verify(LayoutToVerifiable(result, bounds, context));
    }

    [Fact]
    public Task Chorus_WithOffset()
    {
        // Test that non-zero origin is respected
        var bounds = new RectF(50, 100, 300, 80);
        var context = LayoutContext.Horizontal(aspectRatio: 3.75f);
        
        var result = _calculator.Calculate(bounds, context);
        
        return Verifier.Verify(LayoutToVerifiable(result, bounds, context));
    }

    // === DEFINITION SNAPSHOTS (what ChorusDrawable actually uses) ===

    [Fact]
    public Task Definition_Landscape_Standard()
    {
        var bounds = new RectF(0, 0, 300, 80);
        var context = LayoutContext.Horizontal(aspectRatio: 2.0f);
        
        var result = _definition.Calculate(bounds, context);
        
        return Verifier.Verify(LayoutToVerifiable(result, bounds, context, "Definition"));
    }

    [Fact]
    public Task Definition_Landscape_Narrow()
    {
        // Tests narrow aspect ratio override
        var bounds = new RectF(0, 0, 150, 120);
        var context = LayoutContext.Horizontal(aspectRatio: 1.25f);
        
        var result = _definition.Calculate(bounds, context);
        
        return Verifier.Verify(LayoutToVerifiable(result, bounds, context, "Definition_Narrow"));
    }

    [Fact]
    public Task Definition_Landscape_Wide()
    {
        // Tests wide aspect ratio override
        var bounds = new RectF(0, 0, 500, 100);
        var context = LayoutContext.Horizontal(aspectRatio: 3.0f);
        
        var result = _definition.Calculate(bounds, context);
        
        return Verifier.Verify(LayoutToVerifiable(result, bounds, context, "Definition_Wide"));
    }

    [Fact]
    public Task Definition_Piano()
    {
        // Tests Piano padrea override
        var bounds = new RectF(0, 0, 400, 100);
        var context = LayoutContext.Horizontal(aspectRatio: 2.0f, padreaShape: PadreaShape.Piano);
        
        var result = _definition.Calculate(bounds, context);
        
        return Verifier.Verify(LayoutToVerifiable(result, bounds, context, "Definition_Piano"));
    }

    /// <summary>
    /// Converts layout result to a verifiable object with additional metadata.
    /// </summary>
    private static object LayoutToVerifiable(LayoutResult result, RectF bounds, LayoutContext context, string? source = null)
    {
        return new
        {
            Source = source ?? "Calculator",
            Bounds = new { bounds.X, bounds.Y, bounds.Width, bounds.Height },
            Context = new { context.IsHorizontal, context.AspectRatio, context.PadreaShape, context.IsLandscapeSquare },
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

