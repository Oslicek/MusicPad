namespace MusicPad.Core.Theme;

/// <summary>
/// Centralized color configuration for the entire application.
/// 
/// Color Palette:
/// - #8ECAE6 - Light Sky Blue (light accent, text, highlights)
/// - #219EBC - Teal (primary interactive, buttons, links)
/// - #023047 - Dark Navy (base background, dark surfaces)
/// - #FFB703 - Amber (main accent, selected states, knobs)
/// - #FB8500 - Orange (secondary accent, pressed states)
/// </summary>
public static class AppColors
{
    #region Core Palette
    
    /// <summary>Light sky blue - light accent, text, highlights</summary>
    public const string SkyBlue = "#8ECAE6";
    
    /// <summary>Teal - primary interactive, buttons, links</summary>
    public const string Teal = "#219EBC";
    
    /// <summary>Dark navy - base background, dark surfaces</summary>
    public const string Navy = "#023047";
    
    /// <summary>Amber - main accent, selected states, knobs</summary>
    public const string Amber = "#FFB703";
    
    /// <summary>Orange - secondary accent, pressed states</summary>
    public const string Orange = "#FB8500";
    
    #endregion
    
    #region Theme Colors - Primary Palette
    
    /// <summary>Teal - main brand color for interactive elements</summary>
    public const string Primary = "#219EBC";
    
    /// <summary>Dark teal for depth</summary>
    public const string PrimaryDark = "#1A7E96";
    
    /// <summary>Light teal for highlights</summary>
    public const string PrimaryLight = "#4CB8D4";
    
    /// <summary>Amber - secondary accent for buttons and highlights</summary>
    public const string Secondary = "#FFB703";
    
    /// <summary>Darker amber for pressed states</summary>
    public const string SecondaryDark = "#CC9200";
    
    /// <summary>Lighter amber for hover states</summary>
    public const string SecondaryLight = "#FFCC44";
    
    /// <summary>Orange - attention-grabbing accent</summary>
    public const string Accent = "#FB8500";
    
    /// <summary>Darker orange for pressed states</summary>
    public const string AccentDark = "#C86A00";
    
    #endregion
    
    #region Background Colors
    
    /// <summary>Main page background - dark navy</summary>
    public const string BackgroundMain = "#023047";
    
    /// <summary>Even darker background for pages</summary>
    public const string BackgroundPage = "#011627";
    
    /// <summary>Effect area background - slightly lighter navy</summary>
    public const string BackgroundEffect = "#0A4060";
    
    /// <summary>Picker/dropdown background - navy lighter</summary>
    public const string BackgroundPicker = "#145070";
    
    #endregion
    
    #region Surface Colors - Cards and Containers
    
    /// <summary>Card/container background - navy light</summary>
    public const string Surface = "#0A4060";
    
    /// <summary>Slightly lighter surface for layers</summary>
    public const string SurfaceLight = "#145070";
    
    /// <summary>Card border/stroke color - teal dark</summary>
    public const string SurfaceBorder = "#1A7E96";
    
    /// <summary>Lighter border color</summary>
    public const string SurfaceBorderLight = "#219EBC";
    
    #endregion
    
    #region Text Colors
    
    /// <summary>Primary text - light sky blue</summary>
    public const string TextPrimary = "#8ECAE6";
    
    /// <summary>Secondary/label text - teal light</summary>
    public const string TextSecondary = "#4CB8D4";
    
    /// <summary>Muted/disabled text - teal</summary>
    public const string TextMuted = "#219EBC";
    
    /// <summary>Dim text for subtle elements</summary>
    public const string TextDim = "#1A7E96";
    
    /// <summary>Commit label - orange accent</summary>
    public const string TextCommit = "#FB8500";
    
    /// <summary>White text for high contrast</summary>
    public const string TextWhite = "#FFFFFF";
    
    /// <summary>Dark text on light backgrounds</summary>
    public const string TextDark = "#023047";
    
    #endregion
    
    #region Pad Colors - Full Range (Chromatic)
    
    /// <summary>Teal - chromatic pad normal state</summary>
    public const string PadChromaticNormal = "#219EBC";
    
    /// <summary>Sky blue - chromatic pad pressed (static, no glow)</summary>
    public const string PadChromaticPressed = "#8ECAE6";
    
    /// <summary>Navy light - chromatic sharps/flats normal (no orange for idle)</summary>
    public const string PadChromaticAlt = "#145070";
    
    /// <summary>Teal - chromatic sharps/flats pressed (static, no glow)</summary>
    public const string PadChromaticAltPressed = "#4CB8D4";
    
    /// <summary>Border color for chromatic pads</summary>
    public const string PadChromaticBorder = "#1A7E96";
    
    #endregion
    
    #region Pad Colors - Pentatonic
    
    /// <summary>Teal - pentatonic pad normal (no orange for idle)</summary>
    public const string PadPentatonicNormal = "#219EBC";
    
    /// <summary>Sky blue - pentatonic pressed (static, no glow)</summary>
    public const string PadPentatonicPressed = "#8ECAE6";
    
    /// <summary>Navy light - pentatonic alt normal (no orange for idle)</summary>
    public const string PadPentatonicAlt = "#145070";
    
    /// <summary>Teal light - pentatonic alt pressed (static, no glow)</summary>
    public const string PadPentatonicAltPressed = "#4CB8D4";
    
    #endregion
    
    #region Pad Colors - Scales
    
    /// <summary>Sky blue - scale pad normal</summary>
    public const string PadScaleNormal = "#8ECAE6";
    
    /// <summary>White - scale pad pressed (static, no glow)</summary>
    public const string PadScalePressed = "#FFFFFF";
    
    /// <summary>Teal - scale halftones normal</summary>
    public const string PadScaleAlt = "#219EBC";
    
    /// <summary>Sky blue light - scale halftones pressed (static, no glow)</summary>
    public const string PadScaleAltPressed = "#B8DBE8";
    
    #endregion
    
    #region Pad Colors - Piano
    
    /// <summary>White piano keys</summary>
    public const string PianoWhiteKey = "#FFFFFF";
    
    /// <summary>Amber - white key pressed</summary>
    public const string PianoWhiteKeyPressed = "#FFB703";
    
    /// <summary>Navy - black piano keys</summary>
    public const string PianoBlackKey = "#023047";
    
    /// <summary>Orange - black key pressed</summary>
    public const string PianoBlackKeyPressed = "#FB8500";
    
    /// <summary>Very dark navy - actual black key color</summary>
    public const string PianoBlackKeyDark = "#011627";
    
    /// <summary>Piano strip background</summary>
    public const string PianoStripBackground = "#0A4060";
    
    /// <summary>Piano strip highlight area - teal transparent</summary>
    public const string PianoStripHighlight = "#40219EBC";
    
    /// <summary>Piano strip inactive area</summary>
    public const string PianoStripInactive = "#145070";
    
    #endregion
    
    #region Knob Colors - Amber Theme
    
    /// <summary>Amber base color for rotary knobs</summary>
    public const string KnobBase = "#FFB703";
    
    /// <summary>Light amber highlight for 3D effect</summary>
    public const string KnobHighlight = "#FFCC44";
    
    /// <summary>Dark amber shadow for 3D effect</summary>
    public const string KnobShadow = "#CC9200";
    
    /// <summary>Dark navy indicator dot on knobs</summary>
    public const string KnobIndicator = "#023047";
    
    #endregion
    
    #region Button Colors
    
    /// <summary>Teal - button/toggle ON state</summary>
    public const string ButtonOn = "#219EBC";
    
    /// <summary>Navy light - button OFF state</summary>
    public const string ButtonOff = "#0A4060";
    
    /// <summary>Button border when off</summary>
    public const string ButtonBorder = "#1A7E96";
    
    /// <summary>Type button base (unselected)</summary>
    public const string TypeButtonBase = "#145070";
    
    /// <summary>Type button selected (amber)</summary>
    public const string TypeButtonSelected = "#FFB703";
    
    /// <summary>Type button highlight</summary>
    public const string TypeButtonHighlight = "#FFCC44";
    
    #endregion
    
    #region Slider/EQ Colors
    
    /// <summary>Slider track background</summary>
    public const string SliderTrack = "#0A4060";
    
    /// <summary>Slider fill color - teal</summary>
    public const string SliderFill = "#219EBC";
    
    /// <summary>Slider thumb (amber)</summary>
    public const string SliderThumb = "#FFB703";
    
    /// <summary>Slider thumb highlight</summary>
    public const string SliderThumbHighlight = "#FFCC44";
    
    /// <summary>Center line on EQ sliders</summary>
    public const string SliderCenterLine = "#1A7E96";
    
    #endregion
    
    #region Effect Area Colors
    
    /// <summary>Effect button background</summary>
    public const string EffectButtonBackground = "#0A4060";
    
    /// <summary>Effect button selected - teal</summary>
    public const string EffectButtonSelected = "#219EBC";
    
    /// <summary>Effect icon normal color</summary>
    public const string EffectIconNormal = "#4CB8D4";
    
    /// <summary>Effect icon selected/active</summary>
    public const string EffectIconSelected = "#FFFFFF";
    
    #endregion
    
    #region Disabled/Muted States
    
    /// <summary>General disabled color</summary>
    public const string Disabled = "#1A7E96";
    
    /// <summary>Disabled border</summary>
    public const string DisabledBorder = "#145070";
    
    /// <summary>Very dark disabled background</summary>
    public const string DisabledDark = "#0A4060";
    
    /// <summary>Darker disabled for deep shadows</summary>
    public const string DisabledDarker = "#023047";
    
    /// <summary>Disabled text on white backgrounds</summary>
    public const string DisabledTextLight = "#4CB8D4";
    
    #endregion
    
    #region Arrow Colors (Navigation)
    
    /// <summary>Amber arrow color - vibrant</summary>
    public const string ArrowNormal = "#FFB703";
    
    /// <summary>Arrow glow color - light amber</summary>
    public const string ArrowGlow = "#FFCC44";
    
    /// <summary>Arrow background (semi-transparent teal)</summary>
    public const string ArrowBackground = "#40219EBC";
    
    #endregion
    
    #region Miscellaneous
    
    /// <summary>Pure white</summary>
    public const string White = "#FFFFFF";
    
    /// <summary>Pure black</summary>
    public const string Black = "#000000";
    
    /// <summary>Text shadow (semi-transparent black)</summary>
    public const string TextShadow = "#40000000";
    
    /// <summary>Border dark - navy</summary>
    public const string BorderDark = "#023047";
    
    /// <summary>Border medium - navy light</summary>
    public const string BorderMedium = "#0A4060";
    
    #endregion
}
