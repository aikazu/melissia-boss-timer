# Product

<!-- impeccable:product-schema 1 -->

## Platform

windows (native desktop app; not one of web/ios/android/adaptive)

## Stack

WPF on .NET 10 for the UI (user-approved rewrite from WinForms, 2026-10-14). `MelissiaBossTimer.Core` (schedule, time conversion, alarm planning, wiki parser) and its xUnit tests are kept. Ships as a self-contained single-file win-x64 exe.

## Users

Players on the Melissia Games server (a Black Desert Online private server) who want to be at world bosses on time. The primary user plays in GMT+7, but the app must be correct for any PC time zone, since the same exe is copied to other PCs.

## Product Purpose

Turn the wiki's GMT boss schedule into "what spawns next, in my local time, and how long until it does", and sound an alarm early enough to get there. Success: no missed boss the player cared about, and no alarm noise for bosses they don't care about.

## Positioning

A local, always-correct companion to the in-game Boss Timer. It converts using the PC's actual Windows time zone (with daylight saving and live zone changes), alarms while the game is fullscreen, and refreshes the schedule from the community wiki on its own.

## Operating Context

- Used two ways, equally: it sits in the tray while the game runs fullscreen and the player opens it between sessions to check what's next or change settings, or it stays open on a second monitor beside the game, where the countdown must read from a distance.
- During fullscreen play, Windows may suppress notifications and popups, so sound is the alarm channel that always works.
- Source schedule: https://melissiagamesinfohub.vercel.app/en/wiki/boss-schedule (32 daily rows in GMT; the page's "Spawn Frequencies" summary is stale, so the table is authoritative).

## Capabilities and Constraints

- Upcoming spawns in local time with countdowns; next spawn first.
- Alarm N minutes before spawn (0 = at spawn): looping sound, topmost popup, tray notification.
- Per-boss alarm on/off.
- Alarm sounds: built-in presets plus user-added custom files (WAV and MP3), a preview button, a volume slider that works separately from Windows volume, and an optional sound per boss (e.g. loud for Garmoth/Kzarka, soft for the frequent minor bosses).
- Schedule refresh from the wiki on start and every 12 h, falling back to the saved copy and then to a built-in snapshot.
- Tray residency, single instance, optional start with Windows (minimized).
- Every row is treated as daily; the page has no day-of-week column.
- Terminology: boss names exactly as the wiki writes them (Kzarka, Garmoth, Golden Pig King, ...).

## Brand Commitments

- Visual style pinned by the user (2026-10-14): Persona 5 UI language (red/black/white, jagged diagonal blocks, ransom-note lettering), as an homage. No Persona/Atlus logos, characters, artwork or quoted game text.

## Evidence on Hand

- Real schedule data: the built-in snapshot in `src/MelissiaBossTimer.Core/BossSchedule.cs` and the fixture `tests/MelissiaBossTimer.Tests/Fixtures/boss-schedule.html`.
- No official Melissia or Pearl Abyss logos or artwork are licensed for use; do not ship game or server branding assets.

## Product Principles

1. Local time is the only time the user should have to think in; GMT appears only as provenance.
2. The next relevant spawn is the answer; everything else is secondary.
3. An alarm the user can't hear has failed; sound outranks visuals.
4. Fewer, wanted alarms beat many ignored ones; filtering must be quick.
5. Never be wrong silently: show which schedule copy and which time zone are in use.
