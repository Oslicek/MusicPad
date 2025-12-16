# Project Context

> **Last Updated:** 2025-12-16

## Overview

**MusicMap** is a .NET MAUI music synthesizer application targeting Android as the primary platform.

## Technology Stack

| Component | Version/Details |
|-----------|-----------------|
| Framework | .NET 10 LTS (10.0.101) |
| UI Framework | .NET MAUI 10.0.1 |
| Language | C# |
| Primary Platform | Android 6.0+ (API 23+) |
| Audio | 44.1kHz, 32-bit float, wavetable synthesis |
| Testing | xUnit |

## Project Structure

```
MusicMap/
├── src/
│   ├── MusicMap/                    # MAUI application
│   │   ├── Platforms/
│   │   │   └── Android/
│   │   │       └── Services/
│   │   │           └── TonePlayer.cs # AudioTrack player
│   │   ├── Services/
│   │   │   ├── ITonePlayer.cs        # Audio interface
│   │   │   ├── AudioSettings.cs      # Audio configuration
│   │   │   └── TonePlayer.Stub.cs    # Stub for non-Android platforms
│   │   ├── Resources/
│   │   │   ├── Styles/               # Colors.xaml, Styles.xaml
│   │   │   ├── AppIcon/              # App icon SVGs
│   │   │   └── Fonts/                # OpenSans fonts
│   │   ├── MainPage.xaml             # Synthesizer UI
│   │   └── MauiProgram.cs
│   │
│   └── MusicMap.Core/                # Shared library (platform-independent)
│       └── Audio/
│           ├── WaveTableGenerator.cs # Wavetable generation
│           ├── VoiceMixer.cs         # Polyphonic mixing with release
│           └── AHDSHRSettings.cs     # Envelope settings
│
├── tests/
│   └── MusicMap.Tests/               # Unit tests
│       ├── WaveTableGeneratorTests.cs
│       └── VoiceMixerTests.cs
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
- `MusicMap.Core` - Platform-independent audio logic (wavetable, poly mixer)
- `MusicMap` - MAUI app with platform-specific implementations

**Audio Engine:**
- Sample Rate: 44,100 Hz
- Bit Depth: 32-bit float (PcmFloat)
- Synthesis: Wavetable (single cycle buffer, looped)

## Key Components

| Component | Location | Purpose |
|-----------|----------|---------|
| `WaveTableGenerator` | Core | Generates sine wave tables |
| `VoiceMixer` | Core | Polyphonic voice mixing with per-voice release |
| `AHDSHRSettings` | Core | Envelope settings (Attack, Hold1, Decay, Sustain, Hold2, Release) |
| `ITonePlayer` | MusicMap | Audio playback interface |
| `TonePlayer` | Android | AudioTrack player using `VoiceMixer` |
| `MainPage` | MusicMap | Synth UI (placeholder) |

## Tests

| Test Class | Purpose |
|------------|---------|
| `WaveTableGeneratorTests` | Sine wavetable shape/count/amplitude |
| `VoiceMixerTests` | Polyphony mixing, release, max-voice handling |

## Test Devices

- Samsung Galaxy S24 Ultra (SM-S928B)
- Samsung Galaxy Tab S9 (SM-X710)
- Android Emulator (Pixel 9 Pro)

## Current State

**Phase:** Initial Setup Complete

**Completed:**
- [x] Project setup with .NET 10 LTS and MAUI 10
- [x] Core audio library with WaveTableGenerator and VoiceMixer
- [x] Android AudioTrack TonePlayer implementation
- [x] AHDSHR envelope support in VoiceMixer
- [x] Multi-platform structure (Android, iOS, macOS, Windows, Tizen)
- [x] Unit tests (18 tests passing)
- [x] Dark theme UI styles
- [x] Initial commit to Git

**Pending:**
- [ ] Create GitHub repository and push
- [ ] Build synthesizer UI (piano keyboard, waveform editor)
- [ ] Additional synthesis features

## Notes

- Based on architecture from DrawSound project
- WiFi debugging enabled on physical devices (phone + tablet)
