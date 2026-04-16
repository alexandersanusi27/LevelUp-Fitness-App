# LevelUp – Fitness Rank Tracker

**Author:** Alexander Sanusi
**Module:** Mobile Computing – Manchester Metropolitan University

---

## Overview

LevelUp is a fitness gamification app built with .NET MAUI. The concept is based on *Solo Leveling* – you complete real fitness challenges each day to earn XP and climb through six hunter ranks, from Rank E (Iron Hunter) up to Rank S (Shadow Monarch).

The point of the app is to make daily exercise feel rewarding rather than just a chore. Each day you get four quests to complete, and finishing them earns XP, triggers rank-up animations, and plays sound and haptic feedback.

---

## Features

- **XP and rank progression** – six ranks (E → D → C → B → A → S), each requiring more XP to reach
- **Daily quests** – four quests per day: 10,000 steps, 50 push-ups, 50 sit-ups, 100 squats (50 XP each)
- **Rank-up animations** – the badge flashes, a sound plays and the app reads your new rank via TTS
- **Workout log** – log exercises with weight and reps; first exercise of the day gives bonus XP
- **Workout history** – sessions saved as JSON; last 7 days shown on the log page
- **Data persistence** – XP, rank and quest state saved via the MAUI Preferences API, survives restarts
- **Daily reset** – quest flags automatically clear at midnight, XP is never lost
- **Push notifications** – daily 8pm reminder via Android’s alarm system, cancels when all quests are done
- **Shake detection** – shaking the phone shows a random motivational quote read aloud via TTS
- **Text-to-speech** – voice announcements for rank-ups, quest completions and the quote system
- **Haptic feedback** – vibration on quest completion and rank-up events
- **S-rank maintenance** – Shadow Monarch must complete at least 2 quests daily or decays back to Rank A
- **Accessibility** – SemanticProperties on all interactive elements, FontAutoScalingEnabled throughout, WCAG 2.1 aligned

---

## Development plan

| # | Feature | Status |
|---|---------|--------|
| 1 | XP and rank progression | Done |
| 2 | Daily quest UI | Done |
| 3 | MVVM architecture and data binding | Done |
| 4 | Sound effects and haptic feedback | Done |
| 5 | Text-to-speech | Done |
| 6 | Workout log page | Done |
| 7 | Shake detection via accelerometer | Done |
| 8 | S-rank decay mechanic | Done |
| 9 | Data persistence | Done |
| 10 | Daily quest reset | Done |
| 11 | Push notifications | Done |
| 12 | Workout history with JSON persistence | Done |
| 13 | Accessibility – WCAG 2.1 | Done |

---

## Tech stack

- **.NET MAUI** – cross-platform, targets Android, iOS, macOS and Windows from a single codebase
- **Plugin.Maui.Audio** v3.1.0 – sound effect playback
- **Plugin.LocalNotification** v11.1.4 – scheduled push notifications
- **System.Text.Json** – workout history serialisation (built into .NET, no extra package needed)

---

## Architecture

The app uses MVVM to keep the UI and business logic separate:

- `AppState.cs` – global state with Save() and Load() methods for persistence
- `MainPageViewModel.cs` – all dashboard logic: quest commands, XP and rank calculations
- `WorkoutHistoryService.cs` – reads and writes workout sessions to a JSON file
- `NotificationService.cs` – schedules and cancels the daily push notification
- `SoundService.cs` – simple audio playback wrapper
- Pages (`MainPage`, `WorkoutLogPage`, `HelpPage`) – UI only; hardware things like sensors handled in code-behind

---

## How to run

### Requirements

- .NET 10 SDK
- MAUI workload: `dotnet workload install maui`
- Android SDK (API 21+) via Visual Studio or Android Studio

### Steps

1. Clone the repo
2. Open the `LevelUp` folder in Visual Studio 2022
3. Select an Android emulator from the device dropdown
4. Press **Run** (F5) – NuGet packages restore on first build

### Physical Android device

1. Settings → About Phone → tap Build Number 7 times to unlock Developer Options
2. Enable USB Debugging in Developer Options
3. Plug in via USB and allow the connection when prompted
4. Select the device in Visual Studio and press Run

---

## Accessibility

LevelUp follows WCAG 2.1 for mobile. Full details are in the in-app Hunter’s Guide page.

- `SemanticProperties.Description` and `.Hint` on all interactive elements for screen readers
- `SemanticProperties.HeadingLevel` on all section headings (Level 1 and Level 2)
- `FontAutoScalingEnabled="True"` throughout – respects system font size settings (WCAG 1.4.4)
- `IsInAccessibleTree="False"` on purely decorative elements to reduce screen reader noise
- All three pages have a title set (WCAG 2.4.2)
- All form fields have visible labels and screen reader hints (WCAG 3.3.2)
- Text contrast on dark backgrounds meets the 4.5:1 minimum ratio (WCAG 1.4.3)
