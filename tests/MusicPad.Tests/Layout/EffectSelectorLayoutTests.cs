using MusicPad.Core.Layout;

namespace MusicPad.Tests.Layout;

/// <summary>
/// Tests for effect selector button layout.
/// Ensures buttons are properly positioned in horizontal and vertical orientations.
/// </summary>
public class EffectSelectorLayoutTests
{
    // Element names for 5 effect buttons
    private const string Button0 = "Button0";  // ArpHarmony
    private const string Button1 = "Button1";  // EQ
    private const string Button2 = "Button2";  // Chorus
    private const string Button3 = "Button3";  // Delay
    private const string Button4 = "Button4";  // Reverb

    #region Horizontal Layout Tests

    [Fact]
    public void Calculator_Horizontal_AllButtonsCreated()
    {
        var calculator = new EffectSelectorLayoutCalculator();
        var bounds = new RectF(0, 0, 400, 150);
        var context = LayoutContext.Horizontal();

        var result = calculator.Calculate(bounds, context);

        Assert.True(result.HasElement(Button0));
        Assert.True(result.HasElement(Button1));
        Assert.True(result.HasElement(Button2));
        Assert.True(result.HasElement(Button3));
        Assert.True(result.HasElement(Button4));
    }

    [Fact]
    public void Calculator_Horizontal_ButtonsAtTop()
    {
        var calculator = new EffectSelectorLayoutCalculator();
        var bounds = new RectF(0, 0, 400, 150);
        var context = LayoutContext.Horizontal();

        var result = calculator.Calculate(bounds, context);

        var button0 = result[Button0];
        
        // Button should be near the top
        Assert.True(button0.Y < 10, $"Button Y={button0.Y} should be near top");
    }

    [Fact]
    public void Calculator_Horizontal_ButtonsArrangedLeftToRight()
    {
        var calculator = new EffectSelectorLayoutCalculator();
        var bounds = new RectF(0, 0, 400, 150);
        var context = LayoutContext.Horizontal();

        var result = calculator.Calculate(bounds, context);

        var btn0 = result[Button0];
        var btn1 = result[Button1];
        var btn2 = result[Button2];
        var btn3 = result[Button3];
        var btn4 = result[Button4];

        Assert.True(btn0.Right < btn1.X, "Button0 should be left of Button1");
        Assert.True(btn1.Right < btn2.X, "Button1 should be left of Button2");
        Assert.True(btn2.Right < btn3.X, "Button2 should be left of Button3");
        Assert.True(btn3.Right < btn4.X, "Button3 should be left of Button4");
    }

    [Fact]
    public void Calculator_Horizontal_ButtonsSameY()
    {
        var calculator = new EffectSelectorLayoutCalculator();
        var bounds = new RectF(0, 0, 400, 150);
        var context = LayoutContext.Horizontal();

        var result = calculator.Calculate(bounds, context);

        var btn0 = result[Button0];
        var btn1 = result[Button1];
        var btn2 = result[Button2];
        var btn3 = result[Button3];
        var btn4 = result[Button4];

        Assert.Equal(btn0.Y, btn1.Y, precision: 1);
        Assert.Equal(btn1.Y, btn2.Y, precision: 1);
        Assert.Equal(btn2.Y, btn3.Y, precision: 1);
        Assert.Equal(btn3.Y, btn4.Y, precision: 1);
    }

    [Fact]
    public void Calculator_Horizontal_ButtonsSameSize()
    {
        var calculator = new EffectSelectorLayoutCalculator();
        var bounds = new RectF(0, 0, 400, 150);
        var context = LayoutContext.Horizontal();

        var result = calculator.Calculate(bounds, context);

        var btn0 = result[Button0];
        var btn1 = result[Button1];
        
        Assert.Equal(btn0.Width, btn1.Width);
        Assert.Equal(btn0.Height, btn1.Height);
        Assert.Equal(btn0.Width, btn0.Height); // Square buttons
    }

    [Fact]
    public void Calculator_Horizontal_ControlsAreaReturned()
    {
        var calculator = new EffectSelectorLayoutCalculator();
        var bounds = new RectF(0, 0, 400, 150);
        var context = LayoutContext.Horizontal();

        var result = calculator.Calculate(bounds, context);

        Assert.True(result.HasElement(EffectSelectorLayoutCalculator.ControlsArea));
        
        var controls = result[EffectSelectorLayoutCalculator.ControlsArea];
        var btn0 = result[Button0];
        
        // Controls area should be below buttons
        Assert.True(controls.Y > btn0.Bottom, 
            $"Controls Y={controls.Y} should be > Button bottom={btn0.Bottom}");
    }

    #endregion

    #region Vertical Layout Tests

    [Fact]
    public void Calculator_Vertical_AllButtonsCreated()
    {
        var calculator = new EffectSelectorLayoutCalculator();
        var bounds = new RectF(0, 0, 150, 400);
        var context = LayoutContext.Vertical();

        var result = calculator.Calculate(bounds, context);

        Assert.True(result.HasElement(Button0));
        Assert.True(result.HasElement(Button1));
        Assert.True(result.HasElement(Button2));
        Assert.True(result.HasElement(Button3));
        Assert.True(result.HasElement(Button4));
    }

    [Fact]
    public void Calculator_Vertical_ButtonsOnLeft()
    {
        var calculator = new EffectSelectorLayoutCalculator();
        var bounds = new RectF(0, 0, 150, 400);
        var context = LayoutContext.Vertical();

        var result = calculator.Calculate(bounds, context);

        var button0 = result[Button0];
        
        // Button should be near the left
        Assert.True(button0.X < 10, $"Button X={button0.X} should be near left");
    }

    [Fact]
    public void Calculator_Vertical_ButtonsArrangedTopToBottom()
    {
        var calculator = new EffectSelectorLayoutCalculator();
        var bounds = new RectF(0, 0, 150, 400);
        var context = LayoutContext.Vertical();

        var result = calculator.Calculate(bounds, context);

        var btn0 = result[Button0];
        var btn1 = result[Button1];
        var btn2 = result[Button2];
        var btn3 = result[Button3];
        var btn4 = result[Button4];

        Assert.True(btn0.Bottom < btn1.Y, "Button0 should be above Button1");
        Assert.True(btn1.Bottom < btn2.Y, "Button1 should be above Button2");
        Assert.True(btn2.Bottom < btn3.Y, "Button2 should be above Button3");
        Assert.True(btn3.Bottom < btn4.Y, "Button3 should be above Button4");
    }

    [Fact]
    public void Calculator_Vertical_ButtonsSameX()
    {
        var calculator = new EffectSelectorLayoutCalculator();
        var bounds = new RectF(0, 0, 150, 400);
        var context = LayoutContext.Vertical();

        var result = calculator.Calculate(bounds, context);

        var btn0 = result[Button0];
        var btn1 = result[Button1];
        var btn2 = result[Button2];
        var btn3 = result[Button3];
        var btn4 = result[Button4];

        Assert.Equal(btn0.X, btn1.X, precision: 1);
        Assert.Equal(btn1.X, btn2.X, precision: 1);
        Assert.Equal(btn2.X, btn3.X, precision: 1);
        Assert.Equal(btn3.X, btn4.X, precision: 1);
    }

    [Fact]
    public void Calculator_Vertical_ControlsAreaReturned()
    {
        var calculator = new EffectSelectorLayoutCalculator();
        var bounds = new RectF(0, 0, 150, 400);
        var context = LayoutContext.Vertical();

        var result = calculator.Calculate(bounds, context);

        Assert.True(result.HasElement(EffectSelectorLayoutCalculator.ControlsArea));
        
        var controls = result[EffectSelectorLayoutCalculator.ControlsArea];
        var btn0 = result[Button0];
        
        // Controls area should be to the right of buttons
        Assert.True(controls.X > btn0.Right, 
            $"Controls X={controls.X} should be > Button right={btn0.Right}");
    }

    #endregion

    #region Common Tests

    [Fact]
    public void Calculator_ButtonsDoNotOverlap()
    {
        var calculator = new EffectSelectorLayoutCalculator();
        var bounds = new RectF(0, 0, 400, 150);
        var context = LayoutContext.Horizontal();

        var result = calculator.Calculate(bounds, context);

        // Check only button elements, not controls area
        var buttonNames = new[] { Button0, Button1, Button2, Button3, Button4 };
        for (int i = 0; i < buttonNames.Length - 1; i++)
        {
            var rect1 = result[buttonNames[i]];
            var rect2 = result[buttonNames[i + 1]];
            
            // In horizontal mode, buttons shouldn't overlap horizontally
            Assert.True(rect1.Right <= rect2.X, 
                $"{buttonNames[i]} overlaps {buttonNames[i + 1]}");
        }
    }

    [Fact]
    public void Calculator_ButtonsFitWithinBounds()
    {
        var calculator = new EffectSelectorLayoutCalculator();
        var bounds = new RectF(10, 20, 400, 150);
        var context = LayoutContext.Horizontal();

        var result = calculator.Calculate(bounds, context);

        foreach (var name in new[] { Button0, Button1, Button2, Button3, Button4, 
                                      EffectSelectorLayoutCalculator.ControlsArea })
        {
            var rect = result[name];
            Assert.True(rect.X >= bounds.X, $"{name} X out of bounds");
            Assert.True(rect.Y >= bounds.Y, $"{name} Y out of bounds");
            Assert.True(rect.Right <= bounds.Right, $"{name} Right out of bounds");
            Assert.True(rect.Bottom <= bounds.Bottom, $"{name} Bottom out of bounds");
        }
    }

    [Fact]
    public void Calculator_WithOffset_PositionsRelativeToBounds()
    {
        var calculator = new EffectSelectorLayoutCalculator();
        var bounds = new RectF(50, 100, 400, 150);
        var context = LayoutContext.Horizontal();

        var result = calculator.Calculate(bounds, context);

        var button0 = result[Button0];
        
        // Button should be relative to bounds, not (0,0)
        Assert.True(button0.X >= bounds.X, $"Button X={button0.X} should be >= bounds X={bounds.X}");
        Assert.True(button0.Y >= bounds.Y, $"Button Y={button0.Y} should be >= bounds Y={bounds.Y}");
    }

    #endregion

    #region Definition vs Calculator Tests

    [Fact]
    public void Definition_MatchesCalculator_Horizontal()
    {
        var calculator = new EffectSelectorLayoutCalculator();
        var definition = EffectSelectorLayoutDefinition.Instance;
        var bounds = new RectF(0, 0, 400, 150);
        var context = LayoutContext.Horizontal();

        var calcResult = calculator.Calculate(bounds, context);
        var defResult = definition.Calculate(bounds, context);

        foreach (var name in EffectSelectorLayoutDefinition.ButtonNames)
        {
            var calcRect = calcResult[name];
            var defRect = defResult[name];
            
            Assert.Equal(calcRect.X, defRect.X, precision: 1);
            Assert.Equal(calcRect.Y, defRect.Y, precision: 1);
            Assert.Equal(calcRect.Width, defRect.Width, precision: 1);
            Assert.Equal(calcRect.Height, defRect.Height, precision: 1);
        }

        // Check controls area
        var calcControls = calcResult[EffectSelectorLayoutCalculator.ControlsArea];
        var defControls = defResult[EffectSelectorLayoutDefinition.ControlsArea];
        Assert.Equal(calcControls.X, defControls.X, precision: 1);
        Assert.Equal(calcControls.Y, defControls.Y, precision: 1);
    }

    [Fact]
    public void Definition_MatchesCalculator_Vertical()
    {
        var calculator = new EffectSelectorLayoutCalculator();
        var definition = EffectSelectorLayoutDefinition.Instance;
        var bounds = new RectF(0, 0, 150, 400);
        var context = LayoutContext.Vertical();

        var calcResult = calculator.Calculate(bounds, context);
        var defResult = definition.Calculate(bounds, context);

        foreach (var name in EffectSelectorLayoutDefinition.ButtonNames)
        {
            var calcRect = calcResult[name];
            var defRect = defResult[name];
            
            Assert.Equal(calcRect.X, defRect.X, precision: 1);
            Assert.Equal(calcRect.Y, defRect.Y, precision: 1);
        }
    }

    #endregion
}

