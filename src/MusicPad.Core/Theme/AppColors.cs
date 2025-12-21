namespace MusicPad.Core.Theme;

/// <summary>
/// Centralized color configuration for the entire application.
/// All colors are derived from the current palette's 7 core colors.
/// 
/// To change the palette at runtime, use:
/// PaletteService.Instance.SetPalette(Palette.Sunset);
/// 
/// Available palettes: Default, Sunset, Forest, Neon
/// </summary>
public static class AppColors
{
    /// <summary>
    /// Gets the current computed palette colors.
    /// </summary>
    private static ComputedPalette Colors => PaletteService.Instance.Colors;
    
    #region Core Palette (read-only access to current palette)
    
    /// <summary>Light sky blue - light accent, text, highlights</summary>
    public static string SkyBlue => ColorHelper.ToHex(PaletteService.Instance.CurrentPalette.SkyBlue);
    
    /// <summary>Teal - primary interactive, buttons, links</summary>
    public static string Teal => ColorHelper.ToHex(PaletteService.Instance.CurrentPalette.Teal);
    
    /// <summary>Dark navy - base background, dark surfaces</summary>
    public static string Navy => ColorHelper.ToHex(PaletteService.Instance.CurrentPalette.Navy);
    
    /// <summary>Amber - main accent, selected states, knobs</summary>
    public static string Amber => ColorHelper.ToHex(PaletteService.Instance.CurrentPalette.Amber);
    
    /// <summary>Orange - secondary accent, pressed states</summary>
    public static string Orange => ColorHelper.ToHex(PaletteService.Instance.CurrentPalette.Orange);
    
    #endregion
    
    #region Theme Colors - Primary Palette
    
    /// <summary>Teal - main brand color for interactive elements</summary>
    public static string Primary => Colors.Primary;
    
    /// <summary>Dark teal for depth</summary>
    public static string PrimaryDark => Colors.PrimaryDark;
    
    /// <summary>Light teal for highlights</summary>
    public static string PrimaryLight => Colors.PrimaryLight;
    
    /// <summary>Amber - secondary accent for buttons and highlights</summary>
    public static string Secondary => Colors.Secondary;
    
    /// <summary>Darker amber for pressed states</summary>
    public static string SecondaryDark => Colors.SecondaryDark;
    
    /// <summary>Lighter amber for hover states</summary>
    public static string SecondaryLight => Colors.SecondaryLight;
    
    /// <summary>Orange - attention-grabbing accent</summary>
    public static string Accent => Colors.Accent;
    
    /// <summary>Darker orange for pressed states</summary>
    public static string AccentDark => Colors.AccentDark;
    
    #endregion
    
    #region Background Colors
    
    /// <summary>Main page background - dark navy</summary>
    public static string BackgroundMain => Colors.BackgroundMain;
    
    /// <summary>Even darker background for pages</summary>
    public static string BackgroundPage => Colors.BackgroundPage;
    
    /// <summary>Effect area background - slightly lighter navy</summary>
    public static string BackgroundEffect => Colors.BackgroundEffect;
    
    /// <summary>Picker/dropdown background - navy lighter</summary>
    public static string BackgroundPicker => Colors.BackgroundPicker;
    
    #endregion
    
    #region Surface Colors - Cards and Containers
    
    /// <summary>Card/container background - navy light</summary>
    public static string Surface => Colors.Surface;
    
    /// <summary>Slightly lighter surface for layers</summary>
    public static string SurfaceLight => Colors.SurfaceLight;
    
    /// <summary>Card border/stroke color - teal dark</summary>
    public static string SurfaceBorder => Colors.SurfaceBorder;
    
    /// <summary>Lighter border color</summary>
    public static string SurfaceBorderLight => Colors.SurfaceBorderLight;
    
    #endregion
    
    #region Text Colors
    
    /// <summary>Primary text - light sky blue</summary>
    public static string TextPrimary => Colors.TextPrimary;
    
    /// <summary>Secondary/label text - teal light</summary>
    public static string TextSecondary => Colors.TextSecondary;
    
    /// <summary>Muted/disabled text - teal</summary>
    public static string TextMuted => Colors.TextMuted;
    
    /// <summary>Dim text for subtle elements</summary>
    public static string TextDim => Colors.TextDim;
    
    /// <summary>Commit label - orange accent</summary>
    public static string TextCommit => Colors.TextCommit;
    
    /// <summary>White text for high contrast</summary>
    public static string TextWhite => Colors.TextWhite;
    
    /// <summary>Dark text on light backgrounds</summary>
    public static string TextDark => Colors.TextDark;
    
    /// <summary>Link text color - teal for clickable URLs</summary>
    public static string LinkColor => Colors.LinkColor;
    
    #endregion
    
    #region Pad Colors - Full Range (Chromatic)
    
    /// <summary>Teal - chromatic pad normal state</summary>
    public static string PadChromaticNormal => Colors.PadChromaticNormal;
    
    /// <summary>Sky blue - chromatic pad pressed (static, no glow)</summary>
    public static string PadChromaticPressed => Colors.PadChromaticPressed;
    
    /// <summary>Navy - chromatic sharps/flats normal (no orange for idle)</summary>
    public static string PadChromaticAlt => Colors.PadChromaticAlt;
    
    /// <summary>Teal - chromatic sharps/flats pressed (static, no glow)</summary>
    public static string PadChromaticAltPressed => Colors.PadChromaticAltPressed;
    
    /// <summary>Border color for chromatic pads</summary>
    public static string PadChromaticBorder => Colors.PadChromaticBorder;
    
    #endregion
    
    #region Pad Colors - Pentatonic
    
    /// <summary>Teal - pentatonic pad normal (no orange for idle)</summary>
    public static string PadPentatonicNormal => Colors.PadPentatonicNormal;
    
    /// <summary>Sky blue - pentatonic pressed (static, no glow)</summary>
    public static string PadPentatonicPressed => Colors.PadPentatonicPressed;
    
    /// <summary>Navy - pentatonic alt normal (no orange for idle)</summary>
    public static string PadPentatonicAlt => Colors.PadPentatonicAlt;
    
    /// <summary>Teal - pentatonic alt pressed (static, no glow)</summary>
    public static string PadPentatonicAltPressed => Colors.PadPentatonicAltPressed;
    
    #endregion
    
    #region Pad Colors - Scales
    
    /// <summary>Sky blue - scale pad normal</summary>
    public static string PadScaleNormal => Colors.PadScaleNormal;
    
    /// <summary>White - scale pad pressed (static, no glow)</summary>
    public static string PadScalePressed => Colors.PadScalePressed;
    
    /// <summary>Teal - scale halftones normal</summary>
    public static string PadScaleAlt => Colors.PadScaleAlt;
    
    /// <summary>Sky blue - scale halftones pressed (static, no glow)</summary>
    public static string PadScaleAltPressed => Colors.PadScaleAltPressed;
    
    #endregion
    
    #region Pad Colors - Piano
    
    /// <summary>White piano keys</summary>
    public static string PianoWhiteKey => Colors.PianoWhiteKey;
    
    /// <summary>Amber - white key pressed</summary>
    public static string PianoWhiteKeyPressed => Colors.PianoWhiteKeyPressed;
    
    /// <summary>Navy - black piano keys</summary>
    public static string PianoBlackKey => Colors.PianoBlackKey;
    
    /// <summary>Orange - black key pressed</summary>
    public static string PianoBlackKeyPressed => Colors.PianoBlackKeyPressed;
    
    /// <summary>Black - actual black key color</summary>
    public static string PianoBlackKeyDark => Colors.PianoBlackKeyDark;
    
    /// <summary>Piano strip background</summary>
    public static string PianoStripBackground => Colors.PianoStripBackground;
    
    /// <summary>Piano strip highlight area - teal</summary>
    public static string PianoStripHighlight => Colors.PianoStripHighlight;
    
    /// <summary>Piano strip inactive area</summary>
    public static string PianoStripInactive => Colors.PianoStripInactive;
    
    #endregion
    
    #region Knob Colors - Amber Theme
    
    /// <summary>Amber base color for rotary knobs</summary>
    public static string KnobBase => Colors.KnobBase;
    
    /// <summary>Light amber highlight for 3D effect</summary>
    public static string KnobHighlight => Colors.KnobHighlight;
    
    /// <summary>Dark amber shadow for 3D effect</summary>
    public static string KnobShadow => Colors.KnobShadow;
    
    /// <summary>Dark navy indicator dot on knobs</summary>
    public static string KnobIndicator => Colors.KnobIndicator;
    
    #endregion
    
    #region Button Colors
    
    /// <summary>Teal - button/toggle ON state</summary>
    public static string ButtonOn => Colors.ButtonOn;
    
    /// <summary>Navy - button OFF state</summary>
    public static string ButtonOff => Colors.ButtonOff;
    
    /// <summary>Button border when off</summary>
    public static string ButtonBorder => Colors.ButtonBorder;
    
    /// <summary>Type button base (unselected)</summary>
    public static string TypeButtonBase => Colors.TypeButtonBase;
    
    /// <summary>Type button selected (amber)</summary>
    public static string TypeButtonSelected => Colors.TypeButtonSelected;
    
    /// <summary>Type button highlight</summary>
    public static string TypeButtonHighlight => Colors.TypeButtonHighlight;
    
    #endregion
    
    #region Slider/EQ Colors
    
    /// <summary>Slider track background</summary>
    public static string SliderTrack => Colors.SliderTrack;
    
    /// <summary>Slider fill color - teal</summary>
    public static string SliderFill => Colors.SliderFill;
    
    /// <summary>Slider thumb (amber)</summary>
    public static string SliderThumb => Colors.SliderThumb;
    
    /// <summary>Slider thumb highlight</summary>
    public static string SliderThumbHighlight => Colors.SliderThumbHighlight;
    
    /// <summary>Center line on EQ sliders</summary>
    public static string SliderCenterLine => Colors.SliderCenterLine;
    
    #endregion
    
    #region Effect Area Colors
    
    /// <summary>Effect button background</summary>
    public static string EffectButtonBackground => Colors.EffectButtonBackground;
    
    /// <summary>Effect button selected - teal</summary>
    public static string EffectButtonSelected => Colors.EffectButtonSelected;
    
    /// <summary>Effect icon normal color</summary>
    public static string EffectIconNormal => Colors.EffectIconNormal;
    
    /// <summary>Effect icon selected/active</summary>
    public static string EffectIconSelected => Colors.EffectIconSelected;
    
    #endregion
    
    #region Disabled/Muted States
    
    /// <summary>General disabled color</summary>
    public static string Disabled => Colors.Disabled;
    
    /// <summary>Disabled border</summary>
    public static string DisabledBorder => Colors.DisabledBorder;
    
    /// <summary>Very dark disabled background</summary>
    public static string DisabledDark => Colors.DisabledDark;
    
    /// <summary>Darker disabled for deep shadows</summary>
    public static string DisabledDarker => Colors.DisabledDarker;
    
    /// <summary>Disabled text on white backgrounds</summary>
    public static string DisabledTextLight => Colors.DisabledTextLight;
    
    #endregion
    
    #region Arrow Colors (Navigation)
    
    /// <summary>Amber arrow color - vibrant</summary>
    public static string ArrowNormal => Colors.ArrowNormal;
    
    /// <summary>Arrow glow color - amber</summary>
    public static string ArrowGlow => Colors.ArrowGlow;
    
    /// <summary>Arrow background - teal</summary>
    public static string ArrowBackground => Colors.ArrowBackground;
    
    #endregion
    
    #region Miscellaneous
    
    /// <summary>Pure white</summary>
    public static string White => Colors.White;
    
    /// <summary>Pure black</summary>
    public static string Black => Colors.Black;
    
    /// <summary>Text shadow - black</summary>
    public static string TextShadow => Colors.TextShadow;
    
    /// <summary>Border dark - navy</summary>
    public static string BorderDark => Colors.BorderDark;
    
    /// <summary>Border medium - navy</summary>
    public static string BorderMedium => Colors.BorderMedium;
    
    #endregion
}
