# Project Context

> **Last Updated:** 2025-12-21

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
│   │   │   ├── PadMatrixDrawable.cs # Touch pad grid with envelope glow
│   │   │   ├── PitchVolumeDrawable.cs # Continuous pitch-volume surface
│   │   │   ├── RotaryKnobDrawable.cs # Volume knob control
│   │   │   ├── EffectAreaDrawable.cs # Effect controls panel
│   │   │   ├── ArpHarmonyDrawable.cs # Arp + Harmony controls
│   │   │   ├── LpfDrawable.cs       # Low-pass filter controls
│   │   │   ├── EqDrawable.cs        # Equalizer controls
│   │   │   ├── ChorusDrawable.cs    # Chorus effect controls
│   │   │   ├── DelayDrawable.cs     # Delay effect controls
│   │   │   ├── ReverbDrawable.cs    # Reverb effect controls
│   │   │   └── RecAreaDrawable.cs   # Recording controls (rec/stop/play)
│   │   ├── Views/
│   │   │   ├── InstrumentsPage.xaml # List of available instruments
│   │   │   ├── InstrumentDetailPage.xaml # Instrument metadata/credits
│   │   │   ├── SongsPage.xaml       # Song list with export/manage
│   │   │   └── CreditsPage.xaml     # Application credits and licenses
│   │   ├── Platforms/
│   │   │   └── Android/
│   │   │       ├── Assets/instruments/ # Bundled SFZ instruments
│   │   │       └── Services/
│   │   │           └── SfzService.cs   # Android SFZ playback
│   │   ├── Services/
│   │   │   ├── ISfzService.cs        # SFZ playback interface (with offline rendering)
│   │   │   ├── IPadreaService.cs     # Padrea management interface
│   │   │   ├── PadreaService.cs      # Padrea configuration
│   │   │   ├── IRecordingService.cs  # Recording/playback interface
│   │   │   ├── RecordingService.cs   # Recording implementation
│   │   │   ├── ExportService.cs      # Song export (MIDI, WAV, FLAC)
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
│       │   ├── AHDSHRSettings.cs     # Envelope settings
│       │   ├── LowPassFilter.cs      # LPF DSP
│       │   ├── Equalizer.cs          # 4-band EQ DSP
│       │   ├── Chorus.cs             # Chorus DSP
│       │   ├── Delay.cs              # Delay DSP
│       │   └── Reverb.cs             # Reverb DSP
│       ├── Models/
│       │   ├── Padrea.cs             # Pad area configuration
│       │   ├── EffectType.cs         # Effect types enum
│       │   ├── HarmonyType.cs        # Harmony types enum
│       │   ├── HarmonySettings.cs    # Harmony settings model
│       │   ├── ArpPattern.cs         # Arpeggiator patterns enum
│       │   └── ArpeggiatorSettings.cs# Arpeggiator settings model
│       ├── NoteProcessing/
│       │   ├── Harmony.cs            # Auto harmony processor
│       │   ├── Arpeggiator.cs        # Arpeggiator processor (UI)
│       │   └── AudioArpeggiator.cs   # Sample-accurate audio-thread arpeggiator
│       ├── Recording/
│       │   ├── RecordedEvent.cs      # Event types and data for recording
│       │   ├── RecordingSession.cs   # Active recording session manager
│       │   ├── AudioPlayback.cs      # Sample-accurate playback on audio thread
│       │   └── Song.cs               # Song metadata model
│       ├── Export/
│       │   └── FlacEncoder.cs        # FLAC audio encoder
│       └── Sfz/
│           ├── SfzParser.cs          # SFZ file parser
│           ├── SfzPlayer.cs          # Polyphonic sample playback
│           ├── SfzInstrument.cs      # Instrument data model
│           ├── SfzRegion.cs          # Region data model
│           ├── SfzMetadata.cs        # Instrument metadata (credits, etc.)
│           └── WavLoader.cs          # WAV file loader
│
├── tests/
│   └── MusicPad.Tests/               # Unit tests
│       ├── Sfz/
│       │   ├── SfzParserTests.cs
│       │   ├── SfzPlayerTests.cs
│       │   ├── SfzMetadataTests.cs
│       │   └── WavLoaderTests.cs
│       ├── Models/
│       │   ├── PadreaTests.cs
│       │   ├── HarmonySettingsTests.cs
│       │   ├── ArpeggiatorSettingsTests.cs
│       │   └── EffectSelectorTests.cs
│       ├── NoteProcessing/
│       │   ├── HarmonyTests.cs
│       │   ├── ArpeggiatorTests.cs
│       │   └── AudioArpeggiatorTests.cs  # Audio-thread arpeggiator tests
│       ├── Recording/
│       │   ├── RecordingSessionTests.cs  # Recording session tests
│       │   └── SongTests.cs              # Song metadata tests
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
- Voicing Modes: Polyphonic (multiple notes) and Monophonic (single note)

**Signal Chain:**
```
[Note Input] → [Harmony] → [Arpeggiator] → [Audio Engine] → [LPF] → [EQ] → [Chorus] → [Delay] → [Reverb] → [Volume] → [Output]
```

## Key Components

| Component | Location | Purpose |
|-----------|----------|---------|
| `SfzParser` | Core/Sfz | Parses SFZ instrument files |
| `SfzPlayer` | Core/Sfz | Sample playback with envelope, poly/mono modes |
| `SfzMetadata` | Core/Sfz | Parses instrument metadata (credits, etc.) |
| `WavLoader` | Core/Sfz | Loads WAV audio samples |
| `Padrea` | Core/Models | Configurable pad area with note filtering |
| `Harmony` | Core/NoteProcessing | Auto harmony (chord generation) - disabled for monophonic instruments |
| `Arpeggiator` | Core/NoteProcessing | Arpeggiator with patterns (UI-based, legacy) |
| `AudioArpeggiator` | Core/NoteProcessing | Sample-accurate arpeggiator running on audio thread |
| `PadMatrixDrawable` | Controls | Touch pad grid with envelope-following glow |
| `PianoKeyboardDrawable` | Controls | Piano keyboard with envelope glow |
| `PitchVolumeDrawable` | Controls | Continuous pitch-volume surface |
| `RotaryKnobDrawable` | Controls | Volume knob with drag rotation |
| `EffectAreaDrawable` | Controls | Effect selection and controls |
| `ISfzService` | Services | SFZ playback interface |
| `PadreaService` | Services | Padrea management (Full Range, Pentatonic) |
| `SettingsService` | Services | App settings with persistence (glow toggles) |
| `InstrumentConfigService` | Services | Instrument configs (bundled + user-imported) |
| `RecordingService` | Services | Recording and playback of performances |
| `ExportService` | Services | Song export (MIDI, WAV, FLAC) |
| `RecordingSession` | Core/Recording | Active recording session with timestamped events |
| `AudioPlayback` | Core/Recording | Sample-accurate playback on audio thread |
| `Song` | Core/Recording | Song metadata (name, duration, instruments) |
| `FlacEncoder` | Core/Export | FLAC audio encoder |
| `SongsPage` | Views | Song list with rename/delete/export |
| `CreditsPage` | Views | Application credits and open source licenses |
| `RecAreaDrawable` | Controls | Recording controls (record/stop/play) |
| `PaletteService` | Core/Theme | Runtime palette switching with computed colors |
| `ColorHelper` | Core/Theme | Color manipulation (Lighter, Darker, Mix, WithAlpha) |
| `MainPage` | MusicPad | Synth UI with pads, pickers, volume knob |
| `InstrumentsPage` | Views | Unified instrument list with drag-and-drop |
| `InstrumentDetailPage` | Views | Instrument metadata and settings |
| `ImportInstrumentPage` | Views | Import SFZ+WAV files with settings |

## Note Processing

**Auto Harmony** - Generates chords from single notes:
| Type | Intervals | Result |
|------|-----------|--------|
| Octave | +12 | Root + Octave |
| Fifth | +7 | Root + Perfect 5th |
| Major | +4, +7 | Major triad |
| Minor | +3, +7 | Minor triad |

**Arpeggiator** - Cycles through held notes:
| Pattern | Description |
|---------|-------------|
| Up (▲) | Low to high |
| Down (▼) | High to low |
| UpDown (↕) | Ping-pong |
| Random (?) | Random order |

Rate knob controls speed (125ms to 500ms between notes).

**Audio-Thread Arpeggiator** - The arpeggiator runs on the audio thread for sample-accurate timing, eliminating jitter from UI thread delays. This ensures consistent tempo even at fast rates.

**Monophonic Harmony Bypass** - When an instrument is configured as monophonic (VoicingType.Monophonic), the harmony/chord effect is automatically disabled and the UI controls are grayed out since chords don't make sense for single-note instruments.

**Live Harmony Type Changes** - When changing harmony type during arpeggio playback, the notes are updated immediately using `ReharmonizeActiveNotes()`, which calculates the delta (notes to add/remove) and updates the arpeggiator in real-time.

## Voicing Modes

Instruments support two voicing modes, configurable per instrument:

| Mode | Description |
|------|-------------|
| **Polyphonic** | Multiple notes play simultaneously (default). Voice allocation prioritizes: idle voices → oldest releasing voices → oldest playing voices. |
| **Monophonic** | Single note at a time. New notes immediately cut off previous notes. Release phase plays only when last key released. |

**Voice Allocation (Polyphonic):**
- Prefers idle voices (no interruption)
- Then steals oldest voice in release phase (minimal disruption)
- Finally steals oldest playing voice (last resort)

**Thread-Safe Note Processing:**
- NoteOn/NoteOff events are queued and processed atomically at audio buffer boundaries
- Prevents race conditions between UI touch events and audio thread
- UI-level debounce (20ms) prevents duplicate events from Android touch quirks
- Minimum hold time (80ms) ensures quick taps produce audible notes

**Mute Button:**
- Positioned close to the padrea (left side in portrait, near padrea edge in landscape)
- Immediately stops all playing notes with a short release (~45ms) to avoid clicks

**Unpitched Instruments:**
- Instruments with PitchType.Unpitched are detected automatically
- When loaded, the Unpitched padrea is auto-selected
- Pads display sample/region labels instead of note names
- Each pad corresponds to a unique MIDI note defined in the SFZ file
- Supports both polyphonic and monophonic voicing

## Audio Effects

| Effect | Controls | Purpose |
|--------|----------|---------|
| **LPF** | Cutoff, Resonance | Low-pass filter for warmth |
| **EQ** | 4-band (Low/LowMid/HighMid/High) | Tone shaping |
| **Chorus** | Depth, Rate | Stereo width, detuning |
| **Delay** | Time, Feedback, Level | Echo effect |
| **Reverb** | Level, Type (Room/Hall/Plate/Church) | Space/ambience |

## Recording

**Recording captures raw pad touches** (before harmony/arpeggiator processing), allowing playback with different effect settings.

**Features:**
- Record/Stop/Play buttons in RecArea (above navigation bar)
- Directory-based storage: `Songs/{songId}/metadata.json + events.json`
- Auto-generated song names: `YYYY-MM-DD_HHmm_Instrument_Duration`
- Live mode playback: uses current instruments/effects instead of recorded ones
- Instrument changes during recording are captured as timestamped events

**Playback Modes:**
| Mode | Description |
|------|-------------|
| **Original** | Uses recorded instruments and settings |
| **Live** | Uses current UI instruments/effects, re-applies harmony and arpeggiator |

**Songs Page** - Accessible from hamburger menu:
- List of all recorded songs
- Rename songs
- Delete songs
- Export songs in multiple formats

**Export Formats:**
| Format | Description |
|--------|-------------|
| **MIDI Naked** | Notes and timing only |
| **MIDI Enhanced** | Notes + instrument changes + effects as metadata |
| **MIDI Complete** | Harmony and arpeggio baked into output |
| **WAV** | Rendered 16-bit stereo audio (offline synthesis) |
| **MP3** | Compressed audio (requires FFmpeg - coming soon) |
| **FLAC** | Lossless compressed audio (custom encoder) |

**Offline Rendering:**
- Uses `GenerateSamples()` for faster-than-realtime synthesis
- Full effects chain applied (LPF, EQ, Chorus, Delay, Reverb)
- 2 seconds added for release tails

**Future:**
- Overdubbing (layer recordings)
- Looping

## Padrea System

**Padreas** (pad areas) define how notes are displayed and filtered:

| Padrea | Description |
|--------|-------------|
| Full Range | All chromatic notes from instrument range |
| Pentatonic | Major pentatonic scale (C, D, E, G, A) |
| Scales 7x7 | Heptatonic scales with selectable root/scale (default C Major) |
| Scales 8x8 | 8x8 chromatic grid with 3-color scale coding (root/in-scale/out-of-scale) |
| Piano | Chromatic piano keyboard view (C3–C4 portrait, C2–C4 landscape) |
| Pitch-Volume | Continuous surface: X=pitch (full range), Y=volume (0-1) |
| Unpitched | Drum/percussion pads with sample labels (auto-selected for unpitched instruments) |

Features:
- Note filtering (chromatic, pentatonic major/minor)
- Custom grid layouts (columns, rows per viewpage)
- Viewpage navigation for large note ranges
- Custom colors per padrea

## UI Areas (Domain Names)

The main synthesizer interface is divided into named areas:

| Area | Domain Name | Description |
|------|-------------|-------------|
| **Pad Area** | `padrea` | Touch pad grid, piano keyboard, or pitch-volume surface |
| **Effect Area** | `efarea` | Effect controls (ArpHarmony, LPF, EQ, Chorus, Delay, Reverb) |
| **Recording Area** | `recarea` | Recording controls (record/stop/play buttons) |
| **Navigation Area** | `navarea` | Navigation arrows for viewpage switching |

## UI Features

- **Hamburger Menu**: Top-right menu button (☰) with dropdown actions
- **Efarea (Effect Area)**: First tab (ArpHarmony) with note processing, followed by LPF/EQ/Chorus/Delay/Reverb tabs
- **Padrea (Pad Area)**: Touch grid with multi-touch, envelope-following glow effect (pad and outline glow)
- **Piano Keyboard**: Keys glow with envelope level (same visual feedback as pads)
- **Pitch-Volume Surface**: Continuous control (X=pitch, Y=volume), circular touch glow
- **Navarea (Navigation)**: Compact, amber arrows next to pads for viewpage switching
- **Volume Knob**: Rotary control with drag interaction
- **Instrument Picker**: Dropdown for SFZ instrument selection
- **Padrea Picker**: Dropdown for pad configuration (Full, Pentatonic, Scales, Piano, Unpitched)
- **Scale Picker**: Shown when Scales padrea selected (roots + common scales)
- **Piano Padrea**: Piano keyboard (one octave+1 portrait, two octaves+1 landscape), strip with 88-key highlight, arrows/drag to shift range
- **Recarea (Recording Area)**: Record/Stop/Play controls above navigation bar
- **Instruments Page**: Unified list of bundled and user instruments with color legend
  - User instruments: Amber background with dark text
  - Bundled instruments: Teal background with light text
  - Drag-and-drop reordering (free, not grouped)
  - Rename/Delete for user instruments
- **Import Instrument Page**: Requires both SFZ and WAV files, with voicing/pitch settings
- **Instrument Detail Page**: Shows SFZ metadata, credits, and settings (Voicing, Pitch Type)
- **Settings Page**: Toggle glow effects, color palette picker
- **Aggressive Colors**: High-contrast pressed states (white, yellow, hot pink)
- **Runtime Color Palettes**: 7 core colors + computed derived colors (70+ total)
  - Palettes: Default, Sunset, Forest, Neon, Abyssal Forge, Northern Archive, Wild Echo
  - Switch palettes at runtime via Settings page

## Tests

| Test Class | Purpose |
|------------|---------|
| `SfzParserTests` | SFZ parsing, region inheritance |
| `SfzPlayerTests` | Playback, envelope, looping |
| `SfzPlayerVoicingTests` | Monophonic/polyphonic modes, mute |
| `UnpitchedPadreaTests` | Unpitched instrument padrea, region labels |
| `SfzMetadataTests` | Metadata extraction (credits, etc.) |
| `WavLoaderTests` | WAV loading (16-bit, 24-bit) |
| `PadreaTests` | Padrea model properties |
| `InstrumentConfigTests` | Instrument config, SFZ/WAV validation |
| `HarmonySettingsTests` | Harmony settings model |
| `HarmonyTests` | Harmony note processing |
| `ArpeggiatorSettingsTests` | Arpeggiator settings model |
| `ArpeggiatorTests` | Arpeggiator patterns |
| `AudioPlaybackTests` | Sample-accurate recording playback |
| `EffectSelectorTests` | Effect selection |
| `WaveTableGeneratorTests` | Wavetable shape/amplitude |
| `VoiceMixerTests` | Polyphony, release handling |
| `ColorHelperTests` | Color manipulation (Lighter, Darker, Mix) |
| `PaletteTests` | Palette definitions and computed colors |
| `FlacEncoderTests` | FLAC encoding tests |
| `OfflineRenderingTests` | Offline audio rendering, effects processing |

## Test Devices

- Samsung Galaxy S24 Ultra (SM-S928B)
- Samsung Galaxy Tab S9 (SM-X710)
- Android Emulator (Pixel 9 Pro)

## Current State

**Phase:** Full Feature Set

**Completed:**
- [x] Project setup with .NET 10 LTS and MAUI 10
- [x] SFZ parsing and sample loading
- [x] Polyphonic playback with AHDSR envelope
- [x] Sample looping (loop_continuous, loop_sustain)
- [x] Touch pad matrix with multi-touch
- [x] Viewpage navigation for large instruments
- [x] Padrea system (Full Range, Pentatonic, Scales, Piano)
- [x] Volume knob control
- [x] 10 bundled SFZ instruments
- [x] Audio effects (LPF, EQ, Chorus, Delay, Reverb)
- [x] Auto Harmony (Octave, Fifth, Major, Minor)
- [x] Arpeggiator (Up, Down, UpDown, Random patterns)
- [x] Hamburger menu with navigation
- [x] Instruments page with unified list (user + bundled)
- [x] Pitch-Volume padrea with continuous control
- [x] Envelope-following glow on all pads and piano keys
- [x] Settings page with glow toggles and palette picker
- [x] Instrument config system with individual JSON files
- [x] Import instrument page (SFZ + WAV files)
- [x] Instrument reordering (free, not grouped by type)
- [x] Instrument settings override for bundled instruments
- [x] Runtime color palette system (7 palettes)
- [x] Montserrat font family
- [x] Custom navigation headers on all pages
- [x] Voicing modes (polyphonic/monophonic) per instrument
- [x] Mute button with quick release
- [x] Recording functionality (basic record/playback)
- [x] Directory-based song storage
- [x] Sample-accurate playback via AudioPlayback class
- [x] Songs page with rename/delete/export
- [x] MIDI export (Naked, Enhanced, Complete)
- [x] WAV export (offline rendering)
- [x] FLAC export (lossless audio)
- [x] Scales 8x8 padrea with chromatic layout and 3-color coding
- [x] Unit tests passing (642 tests)
- [x] GitHub repository connected

**Pending:**
- [ ] FFmpeg integration for MP3 encoding (LGPL) - native bindings needed
- [ ] Save/load custom padreas
- [ ] Recording - overdubbing and looping

## Notes

- Based on architecture from DrawSound project
- WiFi debugging enabled on physical devices
- Release builds used for deployment (Fast Deployment issues with Debug)
