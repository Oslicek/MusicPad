# MusicPad

A .NET MAUI music synthesizer application for Android.

## Getting Started

### Prerequisites
- .NET 10 SDK
- Android SDK with API level 23+
- MAUI workloads installed

### Install .NET MAUI Workload

```bash
dotnet workload install maui
```

### Build and Run

```bash
cd src/MusicPad
dotnet build -f net10.0-android
dotnet build -t:Run -f net10.0-android
```

### Run Tests

```bash
cd tests/MusicPad.Tests
dotnet test
```

## Project Structure

```
MusicPad/
├── src/
│   ├── MusicPad/                    # MAUI application
│   │   ├── Platforms/
│   │   │   └── Android/
│   │   │       └── Services/
│   │   │           └── TonePlayer.cs # AudioTrack player
│   │   ├── Services/
│   │   │   └── ITonePlayer.cs        # Audio interface
│   │   ├── MainPage.xaml             # Synthesizer UI
│   │   └── MauiProgram.cs
│   │
│   └── MusicPad.Core/                # Shared library (platform-independent)
│       └── Audio/
│           ├── WaveTableGenerator.cs # Wavetable generation
│           ├── VoiceMixer.cs         # Polyphonic mixing
│           └── AHDSHRSettings.cs     # Envelope settings
│
├── tests/
│   └── MusicPad.Tests/               # Unit tests
│       ├── WaveTableGeneratorTests.cs
│       └── VoiceMixerTests.cs
│
├── .gitignore
├── global.json
├── PROJECT_RULES.md
├── PROJECT_CONTEXT.md
└── README.md
```

## Technology Stack

| Component | Version/Details |
|-----------|-----------------|
| Framework | .NET 10 LTS (10.0.101) |
| UI Framework | .NET MAUI 10.0.1 |
| Language | C# |
| Primary Platform | Android 6.0+ (API 23+) |
| Audio | 44.1kHz, 32-bit float, wavetable synthesis |
| Testing | xUnit |

## Audio Architecture

- **Sample Rate:** 44,100 Hz
- **Bit Depth:** 32-bit float (PcmFloat)
- **Synthesis:** Wavetable (single cycle buffer, looped)
- **Envelope:** AHDSHR (Attack, Hold1, Decay, Sustain, Hold2, Release)

## License

MIT

