## Scope

Main window of the Melissia Boss Timer WPF app (Schedule, Bosses, Sounds, Settings pages) plus the alarm popup. Mode: Operate.

Physical scene: a player at night in a dim room, the window either glanced at between fullscreen sessions or left open on a second monitor beside the game. Dark ground, with the countdown readable across a desk.

## Audience and task

Players on the Melissia server. Task: know the next spawn in local time and how long until it, scan the day, and tune alarms (lead time, sound, volume, per-boss on/off and per-boss sound) without hunting. Key states: next spawn, alarm due/firing, alarm off for a boss, schedule offline/fallback, no bosses selected, no custom sounds yet, sound file missing.

## Direction contract

THESIS: Every spawn is billed like a match fixture, and the next one is on air in a lower-third, all spoken in Persona 5's UI language (user-pinned, 2026-10-14). It refuses the category default: a grey list with a small "next" label, or a stock Fluent settings utility.

OWN-WORLD: Persona 5 menu grammar on a night ground. Black #0B0B0D ground, ink white #FFFFFF, Phantom red #E5191C as the one live colour (next spawn, countdown field, selection, alarm, majors); off-black #18181B surfaces; halftone dot fields and jagged red shards as the only texture, kept behind content. Anton for display and ransom letters; Barlow Condensed ExtraBold for times and countdown (tabular); Barlow for UI text. Every block is a skewed or jagged polygon (rail items, banner, block headers, tags, buttons); nothing is rounded. Selection inverts: white text on red shard over black. Boss chips: major = red, regular = white with black ink, alarm off = dashed grey outline.

STORY: At a glance the player reads which boss is next, at what local time, and in how long; scans the day as fixtures grouped by part of day; adjusts alarm lead, sound and volume from the side panel; uses Bosses to toggle and assign per-boss sounds; Sounds to add, preview and remove custom WAV/MP3; Settings for startup, schedule source and time zone.

FIRST VIEWPORT: 1100x720 window, layout as picked from the Match-Day sketch. A 176px black left rail holds the brand, Schedule/Bosses/Sounds/Settings as tilted menu words (active = white on a jagged red shard), and the live local clock plus UTC offset at the bottom. A 172px lower-third banner spans the rest: a red skewed field on a black-and-white slash, a white skewed status tag (alarm time and sound), boss names as ransom-note letters (each glyph in its own tilted black/white/red box), the spawn time at 40px, and the countdown at 112px white on black in a jagged frame on the right. Below sit fixtures in two columns of part-of-day cards (skewed red header tabs; rows of 21px condensed time plus boss chips; the next row inverted red with a NEXT badge) and a 264px alarm panel (next alarm time, lead, sound + Test, volume, quick per-boss toggles). A footer status line names the PC time zone, schedule source and the GMT note.

FORM: Match-Day Broadcast layout (Impeccable's pick, rank 1 on the grounded list, seed key 7e662525), re-skinned to the user-pinned Persona 5 world. Signature interaction: ransom-note boss names that re-deal their letters when the next spawn changes, the banner snapping in along its slant (180ms, sharp ease-out); when an alarm fires, the popup arrives as a calling card (red/black card, ransom-note boss names, spawn time, countdown). Motion grammar: fast snaps and slides along the slant, no soft fades.

FINISH: unreviewed and undocumented is unfinished; this build ends with the finish review, the verdict, DESIGN.md, and every shipping raster carrying its provenance

## Constraints

Persona 5 style only, no Persona or Atlus logos, characters, art or quoted game text. No kickers above headings (status tags carry data instead); no thick colored side borders; self-hosted Anton, Barlow and Barlow Condensed (OFL); icons from Segoe Fluent Icons / MDL2, never Unicode glyphs; native WPF controls, restyled.
