# Project Context

> **Last Updated:** 2025-12-17

## Overview

**MusicPad** is a .NET MAUI music synthesizer application targeting Android as the primary platform. It plays SFZ-based sampled instruments with polyphonic playback and touch-based pad interface.

## Technology Stack

| Component | Version/Details |
|-----------|-----------------|
| Framework | .NET 10 LTS (10.0.101) |
| UI Framework | .NET MAUI 10.0.1 |
| Language | C# |
| Primary Platform | Android 6.0+ (API 23+) |
| Audio | 44.1kHz, 32-bit float, SFZ sample playback |
| Testing | xUnit |

## Project Structure

```
MusicPad/
├── src/
│   ├── MusicPad/                    # MAUI application
│   │   ├── Controls/
│   │   │   ├── PadMatrixDrawable.cs # Touch pad grid with multi-touch
│   │   │   └── RotaryKnobDrawable.cs # Volume knob control
│   │   ├── Platforms/
│   │   │   └── Android/
│   │   │       ├── Assets/instruments/ # Bundled SFZ instruments
│   │   │       └── Services/
│   │   │           └── SfzService.cs   # Android SFZ playback
│   │   ├── Services/
│   │   │   ├── ISfzService.cs        # SFZ playback interface
│   │   │   ├── IPadreaService.cs     # Padrea management interface
│   │   │   ├── PadreaService.cs      # Padrea configuration
│   │   │   └── SfzService.Stub.cs    # Stub for non-Android
│   │   ├── Resources/
│   │   │   ├── Styles/               # Colors.xaml, Styles.xaml
│   │   │   ├── AppIcon/              # App icon SVGs
│   │   │   └── Fonts/                # OpenSans fonts
│   │   ├── MainPage.xaml             # Synthesizer UI
│   │   └── MauiProgram.cs
│   │
│   └── MusicPad.Core/                # Shared library (platform-independent)
│       ├── Audio/
│       │   ├── WaveTableGenerator.cs # Wavetable generation
│       │   ├── VoiceMixer.cs         # Polyphonic mixing with release
│       │   └── AHDSHRSettings.cs     # Envelope settings
│       ├── Models/
│       │   └── Padrea.cs             # Pad area configuration
│       └── Sfz/
│           ├── SfzParser.cs          # SFZ file parser
│           ├── SfzPlayer.cs          # Polyphonic sample playback
│           ├── SfzInstrument.cs      # Instrument data model
│           ├── SfzRegion.cs          # Region data model
│           └── WavLoader.cs          # WAV file loader
│
├── tests/
│   └── MusicPad.Tests/               # Unit tests
│       ├── Sfz/
│       │   ├── SfzParserTests.cs
│       │   ├── SfzPlayerTests.cs
│       │   └── WavLoaderTests.cs
│       ├── Models/
│       │   └── PadreaTests.cs
│       ├── WaveTableGeneratorTests.cs
│       └── VoiceMixerTests.cs
│
├── data/                             # Source instrument data
│   ├── glockenspiel_sf2/
│   ├── good_flutes_sf2/
│   ├── Gothorgn_sf2/
│   ├── Simmons_SDS7_sf2/
│   └── VocalsPapel_sf2/
│
├── .gitignore
├── global.json
├── PROJECT_RULES.md
├── PROJECT_CONTEXT.md
└── README.md
```

## Architecture

**Pattern:** Service-based with Dependency Injection

**Layers:**
- `MusicPad.Core` - Platform-independent audio logic (SFZ parsing, sample playback)
- `MusicPad` - MAUI app with platform-specific implementations

**Audio Engine:**
- Sample Rate: 44,100 Hz
- Bit Depth: 32-bit float (PcmFloat)
- Synthesis: SFZ sample-based with AHDSR envelope
- Polyphony: Up to 10 simultaneous voices

## Key Components

| Component | Location | Purpose |
|-----------|----------|---------|
| `SfzParser` | Core/Sfz | Parses SFZ instrument files |
| `SfzPlayer` | Core/Sfz | Polyphonic sample playback with envelope |
| `WavLoader` | Core/Sfz | Loads WAV audio samples |
| `Padrea` | Core/Models | Configurable pad area with note filtering |
| `PadMatrixDrawable` | Controls | Touch pad grid with multi-touch support |
| `RotaryKnobDrawable` | Controls | Volume knob with drag rotation |
| `ISfzService` | Services | SFZ playback interface |
| `PadreaService` | Services | Padrea management (Full Range, Pentatonic) |
| `MainPage` | MusicPad | Synth UI with pads, pickers, volume knob |

## Padrea System

**Padreas** (pad areas) define how notes are displayed and filtered:

| Padrea | Description |
|--------|-------------|
| Full Range | All chromatic notes from instrument range |
| Pentatonic | Major pentatonic scale (C, D, E, G, A) |

Features:
- Note filtering (chromatic, pentatonic major/minor)
- Custom grid layouts (columns, rows per viewpage)
- Viewpage navigation for large note ranges
- Custom colors per padrea

## UI Features

- **Pad Matrix**: Touch grid with multi-touch polyphony
- **Navigation Arrows**: Compact, neon-green arrows next to pads
- **Volume Knob**: Rotary control with drag interaction
- **Instrument Picker**: Dropdown for SFZ instrument selection
- **Padrea Picker**: Dropdown for pad configuration
- **Aggressive Colors**: High-contrast pressed states (white, yellow, hot pink)

## Tests

| Test Class | Purpose |
|------------|---------|
| `SfzParserTests` | SFZ parsing, region inheritance |
| `SfzPlayerTests` | Playback, envelope, looping |
| `WavLoaderTests` | WAV loading (16-bit, 24-bit) |
| `PadreaTests` | Padrea model properties |
| `WaveTableGeneratorTests` | Wavetable shape/amplitude |
| `VoiceMixerTests` | Polyphony, release handling |

## Test Devices

- Samsung Galaxy S24 Ultra (SM-S928B)
- Samsung Galaxy Tab S9 (SM-X710)
- Android Emulator (Pixel 9 Pro)

## Current State

**Phase:** Core Functionality Complete

**Completed:**
- [x] Project setup with .NET 10 LTS and MAUI 10
- [x] SFZ parsing and sample loading
- [x] Polyphonic playback with AHDSR envelope
- [x] Sample looping (loop_continuous, loop_sustain)
- [x] Touch pad matrix with multi-touch
- [x] Viewpage navigation for large instruments
- [x] Padrea system (Full Range, Pentatonic)
- [x] Volume knob control
- [x] 8 bundled SFZ instruments
- [x] Unit tests passing
- [x] GitHub repository connected

**Pending:**
- [ ] More padrea types (natural minor, modes)
- [ ] Save/load custom padreas
- [ ] Effects (reverb, delay)
- [ ] Recording functionality

## Notes

- Based on architecture from DrawSound project
- WiFi debugging enabled on physical devices
- Release builds used for deployment (Fast Deployment issues with Debug)
