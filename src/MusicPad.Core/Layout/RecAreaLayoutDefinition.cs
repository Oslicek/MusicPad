namespace MusicPad.Core.Layout;

/// <summary>
/// Fluent layout definition for Recording Area controls.
/// Defines layout for RecordButton, StatusArea, and PlayButton elements.
/// Layout: [RecordButton] [StatusArea...] [PlayButton]
/// </summary>
public class RecAreaLayoutDefinition : LayoutDefinition
{
    // Element names
    public const string RecordButton = "RecordButton";
    public const string StatusArea = "StatusArea";
    public const string PlayButton = "PlayButton";

    // Singleton instance for reuse
    private static RecAreaLayoutDefinition? _instance;
    public static RecAreaLayoutDefinition Instance => _instance ??= new RecAreaLayoutDefinition();

    public RecAreaLayoutDefinition()
    {
        // === DEFAULT LAYOUT ===
        // Values traced from original RecAreaDrawable to ensure identical layout
        // Layout: [REC/STOP] [Status...] [PLAY/STOP]
        Default()
            .Constants(c => c
                .Set("Padding", 8f)
                .Set("MaxButtonSize", 40f)
                .Set("StatusPadding", 16f))  // Padding * 2 between button and status
            .Element(RecordButton)
                .Left("Padding")
                .VCenter()
                .ButtonSize("MaxButtonSize", "Padding")
            .Element(PlayButton)
                .Right("Padding")
                .VCenter()
                .ButtonSize("MaxButtonSize", "Padding")
            .Element(StatusArea)
                .After(RecordButton, "StatusPadding")
                .Top(0)
                .FillWidthBetween(RecordButton, PlayButton, "StatusPadding")
                .FillHeight()
            .Done();

        // Portrait mode uses same layout (horizontal bar)
        // No special variants needed for now
    }
}

