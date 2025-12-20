namespace MusicPad.Core.Theme;

/// <summary>
/// Contains all derived colors computed from a base Palette.
/// All colors are computed at construction time.
/// </summary>
public class ComputedPalette
{
    private readonly Palette _palette;
    
    public ComputedPalette(Palette palette)
    {
        _palette = palette;
        ComputeAllColors();
    }
    
    private void ComputeAllColors()
    {
        // Theme Colors - Primary Palette
        Primary = ToHex(_palette.Teal);
        PrimaryDark = ToHex(Darker(_palette.Teal, 0.2f));
        PrimaryLight = ToHex(Lighter(_palette.Teal, 0.3f));
        Secondary = ToHex(_palette.Amber);
        SecondaryDark = ToHex(Darker(_palette.Amber, 0.2f));
        SecondaryLight = ToHex(Lighter(_palette.Amber, 0.2f));
        Accent = ToHex(_palette.Orange);
        AccentDark = ToHex(Darker(_palette.Orange, 0.2f));
        
        // Background Colors
        BackgroundMain = ToHex(_palette.Navy);
        BackgroundPage = ToHex(Darker(_palette.Navy, 0.3f));
        BackgroundEffect = ToHex(Lighter(_palette.Navy, 0.15f));
        BackgroundPicker = ToHex(Lighter(_palette.Navy, 0.25f));
        
        // Surface Colors
        Surface = ToHex(Lighter(_palette.Navy, 0.15f));
        SurfaceLight = ToHex(Lighter(_palette.Navy, 0.25f));
        SurfaceBorder = ToHex(Darker(_palette.Teal, 0.2f));
        SurfaceBorderLight = ToHex(_palette.Teal);
        
        // Text Colors
        TextPrimary = ToHex(_palette.SkyBlue);
        TextSecondary = ToHex(Lighter(_palette.Teal, 0.3f));
        TextMuted = ToHex(_palette.Teal);
        TextDim = ToHex(Darker(_palette.Teal, 0.2f));
        TextCommit = ToHex(_palette.Orange);
        TextWhite = ToHex(_palette.White);
        TextDark = ToHex(_palette.Navy);
        
        // Pad Colors - Chromatic
        PadChromaticNormal = ToHex(_palette.Teal);
        PadChromaticPressed = ToHex(_palette.SkyBlue);
        PadChromaticAlt = ToHex(Lighter(_palette.Navy, 0.25f));
        PadChromaticAltPressed = ToHex(Lighter(_palette.Teal, 0.3f));
        PadChromaticBorder = ToHex(Darker(_palette.Teal, 0.2f));
        
        // Pad Colors - Pentatonic
        PadPentatonicNormal = ToHex(_palette.Teal);
        PadPentatonicPressed = ToHex(_palette.SkyBlue);
        PadPentatonicAlt = ToHex(Lighter(_palette.Navy, 0.25f));
        PadPentatonicAltPressed = ToHex(Lighter(_palette.Teal, 0.3f));
        
        // Pad Colors - Scales
        PadScaleNormal = ToHex(_palette.Teal); // Darker blue for in-scale notes
        PadScalePressed = ToHex(Lighter(_palette.Teal, 0.3f));
        PadScaleAlt = ToHex(_palette.SkyBlue);
        PadScaleAltPressed = ToHex(Lighter(_palette.SkyBlue, 0.3f));
        
        // Pad Colors - Piano
        PianoWhiteKey = ToHex(_palette.White);
        PianoWhiteKeyPressed = ToHex(_palette.Amber);
        PianoBlackKey = ToHex(_palette.Navy);
        PianoBlackKeyPressed = ToHex(_palette.Orange);
        PianoBlackKeyDark = ToHex(Darker(_palette.Navy, 0.5f));
        PianoStripBackground = ToHex(Lighter(_palette.Navy, 0.15f));
        PianoStripHighlight = WithAlpha(_palette.Teal, 0x40);
        PianoStripInactive = ToHex(Lighter(_palette.Navy, 0.25f));
        
        // Knob Colors
        KnobBase = ToHex(_palette.Amber);
        KnobHighlight = ToHex(Lighter(_palette.Amber, 0.2f));
        KnobShadow = ToHex(Darker(_palette.Amber, 0.2f));
        KnobIndicator = ToHex(_palette.Navy);
        
        // Button Colors
        ButtonOn = ToHex(_palette.Teal);
        ButtonOff = ToHex(Lighter(_palette.Navy, 0.15f));
        ButtonBorder = ToHex(Darker(_palette.Teal, 0.2f));
        TypeButtonBase = ToHex(Lighter(_palette.Navy, 0.25f));
        TypeButtonSelected = ToHex(_palette.Amber);
        TypeButtonHighlight = ToHex(Lighter(_palette.Amber, 0.2f));
        
        // Slider/EQ Colors
        SliderTrack = ToHex(Lighter(_palette.Navy, 0.15f));
        SliderFill = ToHex(_palette.Teal);
        SliderThumb = ToHex(_palette.Amber);
        SliderThumbHighlight = ToHex(Lighter(_palette.Amber, 0.2f));
        SliderCenterLine = ToHex(Darker(_palette.Teal, 0.2f));
        
        // Effect Area Colors
        EffectButtonBackground = ToHex(Lighter(_palette.Navy, 0.15f));
        EffectButtonSelected = ToHex(_palette.Teal);
        EffectIconNormal = ToHex(Lighter(_palette.Teal, 0.3f));
        EffectIconSelected = ToHex(_palette.White);
        
        // Disabled/Muted States
        Disabled = ToHex(Darker(_palette.Teal, 0.2f));
        DisabledBorder = ToHex(Lighter(_palette.Navy, 0.25f));
        DisabledDark = ToHex(Lighter(_palette.Navy, 0.15f));
        DisabledDarker = ToHex(_palette.Navy);
        DisabledTextLight = ToHex(Lighter(_palette.Teal, 0.3f));
        
        // Arrow Colors
        ArrowNormal = ToHex(_palette.Amber);
        ArrowGlow = ToHex(Lighter(_palette.Amber, 0.2f));
        ArrowBackground = WithAlpha(_palette.Teal, 0x40);
        
        // Miscellaneous
        White = ToHex(_palette.White);
        Black = ToHex(_palette.Black);
        TextShadow = WithAlpha(_palette.Black, 0x40);
        BorderDark = ToHex(_palette.Navy);
        BorderMedium = ToHex(Lighter(_palette.Navy, 0.15f));
    }
    
    // Helper methods for brevity
    private static uint Lighter(uint color, float amount) => ColorHelper.Lighter(color, amount);
    private static uint Darker(uint color, float amount) => ColorHelper.Darker(color, amount);
    private static string ToHex(uint color) => ColorHelper.ToHex(color);
    private static string WithAlpha(uint color, byte alpha) => ColorHelper.WithAlpha(color, alpha);
    
    #region Theme Colors - Primary Palette
    
    public string Primary { get; private set; } = "";
    public string PrimaryDark { get; private set; } = "";
    public string PrimaryLight { get; private set; } = "";
    public string Secondary { get; private set; } = "";
    public string SecondaryDark { get; private set; } = "";
    public string SecondaryLight { get; private set; } = "";
    public string Accent { get; private set; } = "";
    public string AccentDark { get; private set; } = "";
    
    #endregion
    
    #region Background Colors
    
    public string BackgroundMain { get; private set; } = "";
    public string BackgroundPage { get; private set; } = "";
    public string BackgroundEffect { get; private set; } = "";
    public string BackgroundPicker { get; private set; } = "";
    
    #endregion
    
    #region Surface Colors
    
    public string Surface { get; private set; } = "";
    public string SurfaceLight { get; private set; } = "";
    public string SurfaceBorder { get; private set; } = "";
    public string SurfaceBorderLight { get; private set; } = "";
    
    #endregion
    
    #region Text Colors
    
    public string TextPrimary { get; private set; } = "";
    public string TextSecondary { get; private set; } = "";
    public string TextMuted { get; private set; } = "";
    public string TextDim { get; private set; } = "";
    public string TextCommit { get; private set; } = "";
    public string TextWhite { get; private set; } = "";
    public string TextDark { get; private set; } = "";
    
    #endregion
    
    #region Pad Colors - Chromatic
    
    public string PadChromaticNormal { get; private set; } = "";
    public string PadChromaticPressed { get; private set; } = "";
    public string PadChromaticAlt { get; private set; } = "";
    public string PadChromaticAltPressed { get; private set; } = "";
    public string PadChromaticBorder { get; private set; } = "";
    
    #endregion
    
    #region Pad Colors - Pentatonic
    
    public string PadPentatonicNormal { get; private set; } = "";
    public string PadPentatonicPressed { get; private set; } = "";
    public string PadPentatonicAlt { get; private set; } = "";
    public string PadPentatonicAltPressed { get; private set; } = "";
    
    #endregion
    
    #region Pad Colors - Scales
    
    public string PadScaleNormal { get; private set; } = "";
    public string PadScalePressed { get; private set; } = "";
    public string PadScaleAlt { get; private set; } = "";
    public string PadScaleAltPressed { get; private set; } = "";
    
    #endregion
    
    #region Pad Colors - Piano
    
    public string PianoWhiteKey { get; private set; } = "";
    public string PianoWhiteKeyPressed { get; private set; } = "";
    public string PianoBlackKey { get; private set; } = "";
    public string PianoBlackKeyPressed { get; private set; } = "";
    public string PianoBlackKeyDark { get; private set; } = "";
    public string PianoStripBackground { get; private set; } = "";
    public string PianoStripHighlight { get; private set; } = "";
    public string PianoStripInactive { get; private set; } = "";
    
    #endregion
    
    #region Knob Colors
    
    public string KnobBase { get; private set; } = "";
    public string KnobHighlight { get; private set; } = "";
    public string KnobShadow { get; private set; } = "";
    public string KnobIndicator { get; private set; } = "";
    
    #endregion
    
    #region Button Colors
    
    public string ButtonOn { get; private set; } = "";
    public string ButtonOff { get; private set; } = "";
    public string ButtonBorder { get; private set; } = "";
    public string TypeButtonBase { get; private set; } = "";
    public string TypeButtonSelected { get; private set; } = "";
    public string TypeButtonHighlight { get; private set; } = "";
    
    #endregion
    
    #region Slider/EQ Colors
    
    public string SliderTrack { get; private set; } = "";
    public string SliderFill { get; private set; } = "";
    public string SliderThumb { get; private set; } = "";
    public string SliderThumbHighlight { get; private set; } = "";
    public string SliderCenterLine { get; private set; } = "";
    
    #endregion
    
    #region Effect Area Colors
    
    public string EffectButtonBackground { get; private set; } = "";
    public string EffectButtonSelected { get; private set; } = "";
    public string EffectIconNormal { get; private set; } = "";
    public string EffectIconSelected { get; private set; } = "";
    
    #endregion
    
    #region Disabled/Muted States
    
    public string Disabled { get; private set; } = "";
    public string DisabledBorder { get; private set; } = "";
    public string DisabledDark { get; private set; } = "";
    public string DisabledDarker { get; private set; } = "";
    public string DisabledTextLight { get; private set; } = "";
    
    #endregion
    
    #region Arrow Colors
    
    public string ArrowNormal { get; private set; } = "";
    public string ArrowGlow { get; private set; } = "";
    public string ArrowBackground { get; private set; } = "";
    
    #endregion
    
    #region Miscellaneous
    
    public string White { get; private set; } = "";
    public string Black { get; private set; } = "";
    public string TextShadow { get; private set; } = "";
    public string BorderDark { get; private set; } = "";
    public string BorderMedium { get; private set; } = "";
    
    #endregion
}

