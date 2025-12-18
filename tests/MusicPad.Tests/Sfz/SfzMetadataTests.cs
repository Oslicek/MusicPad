using MusicPad.Core.Sfz;
using Xunit;

namespace MusicPad.Tests.Sfz;

public class SfzMetadataTests
{
    [Fact]
    public void ParseMetadata_ExtractsInternalName()
    {
        var sfzContent = @"/*
// Internal Name  : Glockenspiel
*/
<global>
sample=test.wav
";
        var metadata = SfzMetadata.Parse(sfzContent);
        
        Assert.Equal("Glockenspiel", metadata.InternalName);
    }

    [Fact]
    public void ParseMetadata_ExtractsSoundEngineer()
    {
        var sfzContent = @"/*
// Sound Engineer : Ethan Winer
*/
<global>
sample=test.wav
";
        var metadata = SfzMetadata.Parse(sfzContent);
        
        Assert.Equal("Ethan Winer", metadata.SoundEngineer);
    }

    [Fact]
    public void ParseMetadata_ExtractsCreationDate()
    {
        var sfzContent = @"/*
// Creation Date  : May 26, 2002
*/
<global>
sample=test.wav
";
        var metadata = SfzMetadata.Parse(sfzContent);
        
        Assert.Equal("May 26, 2002", metadata.CreationDate);
    }

    [Fact]
    public void ParseMetadata_ExtractsParentFile()
    {
        var sfzContent = @"/*
// Parent file    : C:/Users/opica/OneDrive/_1_PROJECTS/Audio Apps Line/SoundFonts/glockenspiel.sf2
*/
<global>
sample=test.wav
";
        var metadata = SfzMetadata.Parse(sfzContent);
        
        Assert.Equal("C:/Users/opica/OneDrive/_1_PROJECTS/Audio Apps Line/SoundFonts/glockenspiel.sf2", metadata.ParentFile);
    }

    [Fact]
    public void ParseMetadata_ExtractsSoundfontVersion()
    {
        var sfzContent = @"/*
// Soundfont      : 2.1
*/
<global>
sample=test.wav
";
        var metadata = SfzMetadata.Parse(sfzContent);
        
        Assert.Equal("2.1", metadata.SoundfontVersion);
    }

    [Fact]
    public void ParseMetadata_ExtractsEditorUsed()
    {
        var sfzContent = @"/*
// Editor Used    : SFEDT v1.36:
*/
<global>
sample=test.wav
";
        var metadata = SfzMetadata.Parse(sfzContent);
        
        Assert.Equal("SFEDT v1.36:", metadata.EditorUsed);
    }

    [Fact]
    public void ParseMetadata_ExtractsConverterInfo()
    {
        var sfzContent = @"/*
// Converted with SF22SFZ Converter v1.929
// Copyright Jun 17 2019 , Plogue Art et Technologie, Inc
*/
<global>
sample=test.wav
";
        var metadata = SfzMetadata.Parse(sfzContent);
        
        Assert.Equal("SF22SFZ Converter v1.929", metadata.Converter);
        Assert.Equal("Jun 17 2019 , Plogue Art et Technologie, Inc", metadata.ConverterCopyright);
    }

    [Fact]
    public void ParseMetadata_ExtractsConversionDate()
    {
        var sfzContent = @"/*
// Conversion Date: Tue Dec 16 15:25:28 2025
*/
<global>
sample=test.wav
";
        var metadata = SfzMetadata.Parse(sfzContent);
        
        Assert.Equal("Tue Dec 16 15:25:28 2025", metadata.ConversionDate);
    }

    [Fact]
    public void ParseMetadata_ExtractsOptimisedFor()
    {
        var sfzContent = @"/*
// Optimised for  : X-Fi
*/
<global>
sample=test.wav
";
        var metadata = SfzMetadata.Parse(sfzContent);
        
        Assert.Equal("X-Fi", metadata.OptimisedFor);
    }

    [Fact]
    public void ParseMetadata_ExtractsIntendedFor()
    {
        var sfzContent = @"/*
// Intendend for  : SBAWE32
*/
<global>
sample=test.wav
";
        var metadata = SfzMetadata.Parse(sfzContent);
        
        Assert.Equal("SBAWE32", metadata.IntendedFor);
    }

    [Fact]
    public void ParseMetadata_HandlesMultipleFields()
    {
        var sfzContent = @"/*
// **********************************************************************
// Converted with SF22SFZ Converter v1.929
// Copyright Jun 17 2019 , Plogue Art et Technologie, Inc
// **********************************************************************
// Conversion Date: Tue Dec 16 15:25:28 2025
// **********************************************************************
// Parent file    : C:/path/to/file.sf2
// Soundfont      : 2.1
// Internal Name  : Test Instrument
// Optimised for  : X-Fi
// Intendend for  : SBAWE32
// Editor Used    : SFEDT v1.36:
// Sound Engineer : John Doe
// Creation Date  : May 26, 2002
*/

<control>
<global>
sample=sf2_smpl.wav
";
        var metadata = SfzMetadata.Parse(sfzContent);
        
        Assert.Equal("Test Instrument", metadata.InternalName);
        Assert.Equal("John Doe", metadata.SoundEngineer);
        Assert.Equal("May 26, 2002", metadata.CreationDate);
        Assert.Equal("C:/path/to/file.sf2", metadata.ParentFile);
        Assert.Equal("2.1", metadata.SoundfontVersion);
        Assert.Equal("SF22SFZ Converter v1.929", metadata.Converter);
    }

    [Fact]
    public void ParseMetadata_HandlesEmptySfz()
    {
        var sfzContent = @"<global>
sample=test.wav
";
        var metadata = SfzMetadata.Parse(sfzContent);
        
        Assert.Null(metadata.InternalName);
        Assert.Null(metadata.SoundEngineer);
    }

    [Fact]
    public void ParseMetadata_HandlesMissingCommentBlock()
    {
        var sfzContent = @"// Just a simple comment
<global>
sample=test.wav
";
        var metadata = SfzMetadata.Parse(sfzContent);
        
        Assert.NotNull(metadata);
    }

    [Fact]
    public void GetDisplayName_ReturnsInternalNameIfAvailable()
    {
        var metadata = new SfzMetadata { InternalName = "Test Instrument" };
        
        Assert.Equal("Test Instrument", metadata.GetDisplayName("fallback.sfz"));
    }

    [Fact]
    public void GetDisplayName_ReturnsFallbackIfNoInternalName()
    {
        var metadata = new SfzMetadata();
        
        Assert.Equal("fallback.sfz", metadata.GetDisplayName("fallback.sfz"));
    }

    [Fact]
    public void GetCredits_CombinesAvailableInfo()
    {
        var metadata = new SfzMetadata
        {
            SoundEngineer = "Ethan Winer",
            CreationDate = "May 26, 2002"
        };
        
        var credits = metadata.GetCredits();
        
        Assert.Contains("Ethan Winer", credits);
        Assert.Contains("May 26, 2002", credits);
    }
}

