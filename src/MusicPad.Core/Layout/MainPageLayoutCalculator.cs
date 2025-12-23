namespace MusicPad.Core.Layout;

/// <summary>
/// Context information for main page layout calculations.
/// </summary>
public readonly struct MainPageLayoutContext
{
    public float PageWidth { get; init; }
    public float PageHeight { get; init; }
    public bool IsLandscape { get; init; }
    public bool IsPiano { get; init; }
    
    // Layout constants
    public float ControlsWidth { get; init; }
    public float ControlsHeight { get; init; }
    public float VolumeSize { get; init; }
    public float Padding { get; init; }
    public float NavBarHeight { get; init; }
    public float RecAreaHeight { get; init; }

    public static MainPageLayoutContext LandscapeSquare(float width, float height)
    {
        return new MainPageLayoutContext
        {
            PageWidth = width,
            PageHeight = height,
            IsLandscape = true,
            IsPiano = false,
            ControlsWidth = 150 + 16, // picker width + margins
            ControlsHeight = 150, // approximate controls stack height
            VolumeSize = 120,
            Padding = 8,
            NavBarHeight = 50,
            RecAreaHeight = 44
        };
    }

    public static MainPageLayoutContext LandscapePiano(float width, float height)
    {
        return new MainPageLayoutContext
        {
            PageWidth = width,
            PageHeight = height,
            IsLandscape = true,
            IsPiano = true,
            ControlsWidth = 150 + 16,
            ControlsHeight = 150,
            VolumeSize = 120,
            Padding = 8,
            NavBarHeight = 50,
            RecAreaHeight = 44
        };
    }

    public static MainPageLayoutContext Portrait(float width, float height, bool isPiano = false)
    {
        return new MainPageLayoutContext
        {
            PageWidth = width,
            PageHeight = height,
            IsLandscape = false,
            IsPiano = isPiano,
            ControlsWidth = 150 + 16,
            ControlsHeight = 150,
            VolumeSize = 120,
            Padding = 8,
            NavBarHeight = 50,
            RecAreaHeight = 44
        };
    }
}

/// <summary>
/// Calculator for main page layout.
/// Extracts layout logic from MainPage.xaml.cs for testability.
/// </summary>
public class MainPageLayoutCalculator
{
    // Element names
    public const string ControlsStack = "ControlsStack";
    public const string VolumeKnob = "VolumeKnob";
    public const string PadContainer = "PadContainer";
    public const string EffectArea = "EffectArea";
    public const string NavigationBar = "NavigationBar";
    public const string RecArea = "RecArea";

    public LayoutResult Calculate(RectF bounds, MainPageLayoutContext context)
    {
        var result = new LayoutResult();

        if (context.IsLandscape)
        {
            if (context.IsPiano)
            {
                CalculateLandscapePiano(result, bounds, context);
            }
            else
            {
                CalculateLandscapeSquare(result, bounds, context);
            }
        }
        else
        {
            CalculatePortrait(result, bounds, context);
        }

        return result;
    }

    private void CalculateLandscapeSquare(LayoutResult result, RectF bounds, MainPageLayoutContext ctx)
    {
        float padding = ctx.Padding;
        float controlsWidth = ctx.ControlsWidth;
        float controlsHeight = ctx.ControlsHeight;
        float volumeSize = ctx.VolumeSize;
        float navBarHeight = ctx.NavBarHeight;
        float recAreaHeight = ctx.RecAreaHeight;
        float pageWidth = ctx.PageWidth;
        float pageHeight = ctx.PageHeight;

        // Controls stack - top left
        result[ControlsStack] = new RectF(0, 0, controlsWidth, controlsHeight);

        // Volume knob - below controls
        result[VolumeKnob] = new RectF(30, controlsHeight + 16, volumeSize, volumeSize);

        // Account for Grid's Padding=8 on all sides
        // Content area is (pageWidth - 16) x (pageHeight - 16)
        float contentWidth = pageWidth - padding * 2;
        float contentHeight = pageHeight - padding * 2;

        // Layout zones
        float leftControlsWidth = Math.Max(controlsWidth, 30 + volumeSize);
        float hamburgerHeight = 50;
        float minEfareaWidth = 160;
        float gapBetweenElements = padding * 2;

        // Padrea: positioned after left controls
        float padreaLeft = leftControlsWidth + padding;
        
        // Calculate max padrea size
        float efareaMinLeft = contentWidth - minEfareaWidth;
        float padreaMaxWidth = efareaMinLeft - gapBetweenElements - padreaLeft;
        float padreaMaxHeight = contentHeight - padding * 2;
        float padreaSize = Math.Min(padreaMaxWidth, padreaMaxHeight);
        
        float padreaTop = (pageHeight - padreaSize) / 2;
        float padreaRight = padreaLeft + padreaSize;

        // Effect area: fills remaining space to the right
        float efareaLeft = padreaRight + gapBetweenElements;
        float efareaTop = hamburgerHeight;
        float efareaWidth = contentWidth - efareaLeft;   // Ends at content right edge
        float efareaHeight = contentHeight - efareaTop;  // Ends at content bottom edge

        // PadContainer - positioned after controls, vertically centered
        result[PadContainer] = new RectF(padreaLeft, padreaTop, padreaSize, padreaSize);

        // Navigation bar - above padrea
        result[NavigationBar] = new RectF(
            padreaLeft,
            controlsHeight + 16 + recAreaHeight + padding,
            padreaSize,
            navBarHeight);

        // Recording area - above navigation bar
        result[RecArea] = new RectF(
            padreaLeft,
            controlsHeight + 16,
            padreaSize,
            recAreaHeight);

        // Effect area: strict bounds within content area
        result[EffectArea] = new RectF(
            efareaLeft,
            efareaTop,
            efareaWidth,
            efareaHeight);
    }

    private void CalculateLandscapePiano(LayoutResult result, RectF bounds, MainPageLayoutContext ctx)
    {
        float padding = ctx.Padding;
        float controlsWidth = ctx.ControlsWidth;
        float volumeSize = ctx.VolumeSize;
        float navBarHeight = ctx.NavBarHeight;
        float recAreaHeight = ctx.RecAreaHeight;
        float pageWidth = ctx.PageWidth;
        float pageHeight = ctx.PageHeight;

        // Controls stack - top left
        result[ControlsStack] = new RectF(0, 0, controlsWidth, 150);

        // Volume knob - to the right of controls
        result[VolumeKnob] = new RectF(controlsWidth + 8, 0, volumeSize, volumeSize);

        // Piano at bottom
        float pianoHeight = pageHeight * 0.45f;
        float topAreaHeight = pageHeight - pianoHeight - padding;

        // PadContainer - piano at bottom
        result[PadContainer] = new RectF(0, pageHeight - pianoHeight, pageWidth, pianoHeight);

        // Navigation bar - above piano
        result[NavigationBar] = new RectF(
            padding,
            pageHeight - pianoHeight - navBarHeight - padding,
            pageWidth - padding * 2,
            navBarHeight);

        // Recording area - above navigation bar
        result[RecArea] = new RectF(
            padding,
            pageHeight - pianoHeight - navBarHeight - padding * 2 - recAreaHeight,
            pageWidth - padding * 2,
            recAreaHeight);

        // Effect area - top right, constrained to top area
        float efareaLeft = controlsWidth + volumeSize + 24;
        float efareaWidth = pageWidth - efareaLeft - padding;
        result[EffectArea] = new RectF(efareaLeft, 0, Math.Max(40, efareaWidth), topAreaHeight);
    }

    private void CalculatePortrait(LayoutResult result, RectF bounds, MainPageLayoutContext ctx)
    {
        float padding = ctx.Padding;
        float controlsWidth = ctx.ControlsWidth;
        float controlsHeight = ctx.ControlsHeight;
        float volumeSize = ctx.VolumeSize;
        float navBarHeight = ctx.NavBarHeight;
        float recAreaHeight = ctx.RecAreaHeight;
        float pageWidth = ctx.PageWidth;
        float pageHeight = ctx.PageHeight;

        // Controls stack - top left
        result[ControlsStack] = new RectF(0, 0, controlsWidth, controlsHeight);

        // Volume knob - to the right of controls
        result[VolumeKnob] = new RectF(controlsWidth + 8, 0, volumeSize, volumeSize);

        // Effect area - below controls
        float topAreaHeight = Math.Max(controlsHeight, volumeSize) + padding;
        float efareaHeight = 165;
        result[EffectArea] = new RectF(0, topAreaHeight + padding, pageWidth - padding * 2, efareaHeight);

        // Calculate bottom section
        float bottomControlsHeight = recAreaHeight + navBarHeight + padding * 2;
        float availableForPadrea = pageHeight - topAreaHeight - efareaHeight - bottomControlsHeight - padding * 3;

        if (ctx.IsPiano)
        {
            float pianoHeight = Math.Min(pageHeight * 0.35f, availableForPadrea);
            result[PadContainer] = new RectF(0, pageHeight - pianoHeight, pageWidth - padding * 2, pianoHeight);
            result[NavigationBar] = new RectF(padding, pageHeight - pianoHeight - navBarHeight, pageWidth - padding * 2, navBarHeight);
            result[RecArea] = new RectF(padding, pageHeight - pianoHeight - navBarHeight - recAreaHeight, pageWidth - padding * 2, recAreaHeight);
        }
        else
        {
            float maxPadreaHeight = Math.Min(pageHeight * 0.35f, availableForPadrea);
            float padreaSize = Math.Min(pageWidth - padding * 2, maxPadreaHeight);

            float padreaLeft = (pageWidth - padreaSize) / 2;
            result[PadContainer] = new RectF(padreaLeft, pageHeight - padreaSize, padreaSize, padreaSize);
            result[NavigationBar] = new RectF(padding, pageHeight - padreaSize - navBarHeight, pageWidth - padding * 2, navBarHeight);
            result[RecArea] = new RectF(padding, pageHeight - padreaSize - navBarHeight - recAreaHeight, pageWidth - padding * 2, recAreaHeight);
        }
    }
}

