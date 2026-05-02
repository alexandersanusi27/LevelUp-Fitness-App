# LevelUp – Fitness Rank Tracker

**Author:** Alexander Sanusi
**Module:** Mobile Computing – Manchester Metropolitan University

## What is it?

LevelUp is a fitness app I built with .NET MAUI for my mobile computing module. The idea is based on the anime Solo Leveling – you complete daily fitness challenges to earn XP and rank up through six tiers, from Rank E (Iron Hunter) all the way to Rank S (Shadow Monarch).

Basically I wanted to make working out feel like a game instead of a chore. You get four quests each day, completing them earns XP, and hitting rank thresholds triggers animations, sounds and voice announcements.

## Features

- XP system with six ranks (E → D → C → B → A → S)
- Four daily quests: 10,000 steps, 50 push-ups, 50 sit-ups, 100 squats (50 XP each)
- Rank-up animation (badge flash), sound effect and TTS announcement when you rank up
- Workout log page where you can log exercises with weight and reps, first exercise of the day gives bonus XP
- Last 7 days of workout history saved and shown on the log page
- XP and quest state saved to device storage and restored on restart
- Quests automatically reset at midnight, XP is never lost
- Daily 8pm push notification to remind you to do your quests, cancels itself when you finish
- Shake the phone to get a random motivational quote read out loud
- Haptic feedback and vibration on quest completion and rank-up
- S-rank maintenance mechanic where Shadow Monarch has to do at least 2 quests a day or falls back to Rank A
- GPS location shown on the workout log page when training
- Accessibility throughout: screen reader support, scalable fonts, WCAG 2.1 aligned

## Tech stack

- .NET MAUI (targeting Android mainly, also iOS/macOS/Windows from the same codebase)
- Plugin.Maui.Audio v3.1.0 for sound effects
- Plugin.LocalNotification v11.1.4 for push notifications
- System.Text.Json for saving workout history (no extra package, built into .NET)

## Project structure

The app uses MVVM:

- `AppState.cs` – global state holder, Save() and Load() handle persistence
- `MainPageViewModel.cs` – all the dashboard logic, quest commands, XP and rank stuff
- `WorkoutHistoryService.cs` – reads/writes workout sessions to a JSON file
- `NotificationService.cs` – schedules and cancels the daily notification
- `SoundService.cs` – wrapper for playing audio
- Pages (MainPage, WorkoutLogPage, HelpPage) – just UI, hardware stuff handled in code-behind

## How to run

You need:
- .NET 9 SDK
- MAUI workload installed: `dotnet workload install maui`
- Android SDK via Visual Studio or Android Studio

Steps:
1. Clone the repo
2. Open the folder in Visual Studio 2022
3. Pick an Android emulator from the dropdown
4. Hit F5 – packages restore automatically on first build

For a physical Android device:
1. Settings → About Phone → tap Build Number 7 times
2. Enable USB Debugging in Developer Options
3. Plug in, trust the PC when it asks
4. Select the device in Visual Studio and run

## Accessibility

Full details are in the Hunter's Guide page inside the app, but briefly:

- SemanticProperties on all buttons, inputs and headings for screen readers
- FontAutoScalingEnabled on all text so it respects the system font size setting
- Decorative elements hidden from the accessibility tree
- All pages have a title
- Form fields all have visible labels and hints
- Text contrast meets the 4.5:1 minimum on dark backgrounds

https://mmutube.mmu.ac.uk/media/Mobile+Computing+Level+up+app.mp4/1_ai6m3ck1
