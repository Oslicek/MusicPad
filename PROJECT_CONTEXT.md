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


## Tests


## Test Devices

- Samsung Galaxy S24 Ultra (SM-S928B)
- Samsung Galaxy Tab S9 (SM-X710)
- Android Emulator (Pixel 9 Pro)

## Current State

**Phase:** 

**Completed:**


**Current Features:**

  - To be applied to mixer output; algorithms pending implementation

## Notes

