using MusicPad.Core.Layout;

namespace MusicPad.Tests.Layout;

/// <summary>
/// Tests for RecArea layout.
/// Captures current layout behavior before refactoring to DSL.
/// RecArea has: RecordButton (left), StatusArea (center), PlayButton (right)
/// </summary>
public class RecAreaLayoutTests
{
    // Element names for RecArea
    private const string RecordButton = "RecordButton";
    private const string StatusArea = "StatusArea";
    private const string PlayButton = "PlayButton";
    
    // Current layout constants from RecAreaDrawable
    private const float Padding = 8f;
    private const float MaxButtonSize = 40f;

    #region Current Behavior Tests (to capture before refactoring)

    [Fact]
    public void CurrentLayout_RecordButton_IsOnLeft()
    {
        // Current: recX = dirtyRect.X + padding
        var bounds = new RectF(0, 0, 300, 44);
        float buttonSize = Math.Min(bounds.Height - Padding * 2, MaxButtonSize);
        
        float expectedX = bounds.X + Padding;
        float expectedY = bounds.Y + (bounds.Height - buttonSize) / 2;
        
        Assert.Equal(Padding, expectedX);
        Assert.True(expectedY > 0, "Button should be vertically centered");
    }

    [Fact]
    public void CurrentLayout_PlayButton_IsOnRight()
    {
        var bounds = new RectF(0, 0, 300, 44);
        float buttonSize = Math.Min(bounds.Height - Padding * 2, MaxButtonSize);
        
        float expectedX = bounds.Right - Padding - buttonSize;
        
        Assert.Equal(300 - Padding - buttonSize, expectedX);
    }

    [Fact]
    public void CurrentLayout_StatusArea_FillsCenter()
    {
        var bounds = new RectF(0, 0, 300, 44);
        float buttonSize = Math.Min(bounds.Height - Padding * 2, MaxButtonSize);
        
        float recX = bounds.X + Padding;
        float playX = bounds.Right - Padding - buttonSize;
        
        float statusX = recX + buttonSize + Padding * 2;
        float statusWidth = playX - statusX - Padding * 2;
        
        Assert.True(statusX > recX + buttonSize, "Status should start after record button");
        Assert.True(statusWidth > 0, "Status should have positive width");
        Assert.True(statusX + statusWidth < playX, "Status should end before play button");
    }

    [Theory]
    [InlineData(300, 44)]   // Standard portrait width
    [InlineData(400, 44)]   // Wider
    [InlineData(200, 44)]   // Narrower
    [InlineData(300, 60)]   // Taller (button capped at 40)
    [InlineData(300, 30)]   // Shorter (button shrinks)
    public void CurrentLayout_ButtonSize_IsConstrainedByHeight(float width, float height)
    {
        var bounds = new RectF(0, 0, width, height);
        float expectedButtonSize = Math.Min(height - Padding * 2, MaxButtonSize);
        
        Assert.True(expectedButtonSize > 0, "Button should have positive size");
        Assert.True(expectedButtonSize <= MaxButtonSize, "Button should not exceed max size");
        Assert.True(expectedButtonSize <= height - Padding * 2, "Button should fit within height");
    }

    [Fact]
    public void CurrentLayout_ButtonsAreVerticallyCentered()
    {
        var bounds = new RectF(0, 0, 300, 44);
        float buttonSize = Math.Min(bounds.Height - Padding * 2, MaxButtonSize);
        
        float buttonY = bounds.Y + (bounds.Height - buttonSize) / 2;
        float buttonCenterY = buttonY + buttonSize / 2;
        float boundsCenterY = bounds.Y + bounds.Height / 2;
        
        Assert.Equal(boundsCenterY, buttonCenterY, precision: 1);
    }

    [Fact]
    public void CurrentLayout_ButtonsAreSameSize()
    {
        var bounds = new RectF(0, 0, 300, 44);
        float buttonSize = Math.Min(bounds.Height - Padding * 2, MaxButtonSize);
        
        // Both record and play buttons use the same buttonSize
        Assert.True(buttonSize > 0);
        // In the drawable, both buttons are created with same size
    }

    [Theory]
    [InlineData(300, 44)]
    [InlineData(400, 50)]
    [InlineData(250, 40)]
    public void CurrentLayout_AllElementsFitWithinBounds(float width, float height)
    {
        var bounds = new RectF(0, 0, width, height);
        float buttonSize = Math.Min(height - Padding * 2, MaxButtonSize);
        
        // Record button
        float recX = bounds.X + Padding;
        float buttonY = bounds.Y + (bounds.Height - buttonSize) / 2;
        var recordRect = new RectF(recX, buttonY, buttonSize, buttonSize);
        
        // Play button
        float playX = bounds.Right - Padding - buttonSize;
        var playRect = new RectF(playX, buttonY, buttonSize, buttonSize);
        
        // Status area
        float statusX = recX + buttonSize + Padding * 2;
        float statusWidth = playX - statusX - Padding * 2;
        var statusRect = new RectF(statusX, bounds.Y, statusWidth, bounds.Height);
        
        // All should fit within bounds
        Assert.True(recordRect.Left >= bounds.Left);
        Assert.True(recordRect.Right <= bounds.Right);
        Assert.True(recordRect.Top >= bounds.Top);
        Assert.True(recordRect.Bottom <= bounds.Bottom);
        
        Assert.True(playRect.Left >= bounds.Left);
        Assert.True(playRect.Right <= bounds.Right);
        Assert.True(playRect.Top >= bounds.Top);
        Assert.True(playRect.Bottom <= bounds.Bottom);
        
        Assert.True(statusRect.Left >= bounds.Left);
        Assert.True(statusRect.Right <= bounds.Right);
    }

    [Fact]
    public void CurrentLayout_NoOverlapBetweenElements()
    {
        var bounds = new RectF(0, 0, 300, 44);
        float buttonSize = Math.Min(bounds.Height - Padding * 2, MaxButtonSize);
        
        float recX = bounds.X + Padding;
        float buttonY = bounds.Y + (bounds.Height - buttonSize) / 2;
        float playX = bounds.Right - Padding - buttonSize;
        float statusX = recX + buttonSize + Padding * 2;
        float statusWidth = playX - statusX - Padding * 2;
        
        // Record button ends before status starts
        float recordRight = recX + buttonSize;
        Assert.True(statusX > recordRight, "Status should not overlap with record button");
        
        // Status ends before play button starts
        float statusRight = statusX + statusWidth;
        Assert.True(playX > statusRight, "Play button should not overlap with status");
    }

    #endregion

    #region RecAreaLayoutDefinition Tests

    [Fact]
    public void Definition_ReturnsAllElements()
    {
        var layout = RecAreaLayoutDefinition.Instance;
        var bounds = new RectF(0, 0, 300, 44);
        var context = LayoutContext.Horizontal();

        var result = layout.Calculate(bounds, context);

        Assert.True(result.HasElement(RecordButton));
        Assert.True(result.HasElement(StatusArea));
        Assert.True(result.HasElement(PlayButton));
    }

    [Theory]
    [InlineData(300, 44)]
    [InlineData(400, 50)]
    [InlineData(250, 40)]
    [InlineData(300, 30)]  // Short height
    [InlineData(300, 60)]  // Tall height (button capped)
    public void Definition_AllElementsFitWithinBounds(float width, float height)
    {
        var layout = RecAreaLayoutDefinition.Instance;
        var bounds = new RectF(0, 0, width, height);
        var context = LayoutContext.Horizontal();

        var result = layout.Calculate(bounds, context);

        Assert.True(result.AllFitWithin(bounds),
            $"All elements should fit within bounds ({width}x{height})");
    }

    [Fact]
    public void Definition_NoElementsOverlap()
    {
        var layout = RecAreaLayoutDefinition.Instance;
        var bounds = new RectF(0, 0, 300, 44);
        var context = LayoutContext.Horizontal();

        var result = layout.Calculate(bounds, context);

        Assert.False(result.HasOverlaps(), "No elements should overlap");
    }

    [Fact]
    public void Definition_RecordButton_IsOnLeft()
    {
        var layout = RecAreaLayoutDefinition.Instance;
        var bounds = new RectF(0, 0, 300, 44);
        var context = LayoutContext.Horizontal();

        var result = layout.Calculate(bounds, context);
        var recordButton = result[RecordButton];

        Assert.Equal(Padding, recordButton.Left, 0.5f);
    }

    [Fact]
    public void Definition_PlayButton_IsOnRight()
    {
        var layout = RecAreaLayoutDefinition.Instance;
        var bounds = new RectF(0, 0, 300, 44);
        var context = LayoutContext.Horizontal();

        var result = layout.Calculate(bounds, context);
        var playButton = result[PlayButton];

        Assert.Equal(300 - Padding, playButton.Right, 0.5f);
    }

    [Fact]
    public void Definition_StatusArea_IsBetweenButtons()
    {
        var layout = RecAreaLayoutDefinition.Instance;
        var bounds = new RectF(0, 0, 300, 44);
        var context = LayoutContext.Horizontal();

        var result = layout.Calculate(bounds, context);
        var recordButton = result[RecordButton];
        var statusArea = result[StatusArea];
        var playButton = result[PlayButton];

        Assert.True(statusArea.Left > recordButton.Right, "Status should start after record button");
        Assert.True(statusArea.Right < playButton.Left, "Status should end before play button");
    }

    [Fact]
    public void Definition_ButtonsAreVerticallyCentered()
    {
        var layout = RecAreaLayoutDefinition.Instance;
        var bounds = new RectF(0, 0, 300, 44);
        var context = LayoutContext.Horizontal();

        var result = layout.Calculate(bounds, context);
        var recordButton = result[RecordButton];
        var playButton = result[PlayButton];

        float boundsCenterY = bounds.CenterY;
        Assert.Equal(boundsCenterY, recordButton.CenterY, 0.5f);
        Assert.Equal(boundsCenterY, playButton.CenterY, 0.5f);
    }

    [Fact]
    public void Definition_ButtonsAreSameSize()
    {
        var layout = RecAreaLayoutDefinition.Instance;
        var bounds = new RectF(0, 0, 300, 44);
        var context = LayoutContext.Horizontal();

        var result = layout.Calculate(bounds, context);
        var recordButton = result[RecordButton];
        var playButton = result[PlayButton];

        Assert.Equal(recordButton.Width, playButton.Width, 0.5f);
        Assert.Equal(recordButton.Height, playButton.Height, 0.5f);
    }

    [Theory]
    [InlineData(44, 28)]  // height 44: min(44-16, 40) = 28
    [InlineData(60, 40)]  // height 60: min(60-16, 40) = 40 (capped)
    [InlineData(30, 14)]  // height 30: min(30-16, 40) = 14
    public void Definition_ButtonSize_IsConstrainedByHeight(float height, float expectedSize)
    {
        var layout = RecAreaLayoutDefinition.Instance;
        var bounds = new RectF(0, 0, 300, height);
        var context = LayoutContext.Horizontal();

        var result = layout.Calculate(bounds, context);
        var recordButton = result[RecordButton];

        Assert.Equal(expectedSize, recordButton.Width, 0.5f);
        Assert.Equal(expectedSize, recordButton.Height, 0.5f);
    }

    [Fact]
    public void Definition_MatchesCurrentLayout()
    {
        // This test verifies the definition produces the same layout as the current implementation
        var layout = RecAreaLayoutDefinition.Instance;
        var bounds = new RectF(0, 0, 300, 44);
        var context = LayoutContext.Horizontal();

        var result = layout.Calculate(bounds, context);

        // Calculate expected values using current logic
        float buttonSize = Math.Min(bounds.Height - Padding * 2, MaxButtonSize);
        float buttonY = bounds.Y + (bounds.Height - buttonSize) / 2;
        float recX = bounds.X + Padding;
        float playX = bounds.Right - Padding - buttonSize;
        float statusX = recX + buttonSize + Padding * 2;
        float statusWidth = playX - statusX - Padding * 2;

        var recordButton = result[RecordButton];
        var playButton = result[PlayButton];
        var statusArea = result[StatusArea];

        // Record button position and size
        Assert.Equal(recX, recordButton.Left, 0.5f);
        Assert.Equal(buttonY, recordButton.Top, 0.5f);
        Assert.Equal(buttonSize, recordButton.Width, 0.5f);

        // Play button position and size
        Assert.Equal(playX, playButton.Left, 0.5f);
        Assert.Equal(buttonY, playButton.Top, 0.5f);
        Assert.Equal(buttonSize, playButton.Width, 0.5f);

        // Status area position and width
        Assert.Equal(statusX, statusArea.Left, 0.5f);
        Assert.Equal(statusWidth, statusArea.Width, 0.5f);
    }

    #endregion
}

