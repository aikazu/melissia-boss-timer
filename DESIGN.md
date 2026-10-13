---
name: Melissia Boss Timer
description: World-boss fixtures for the Melissia server, billed in local time and spoken in a Persona 5 homage of red, black and white.
colors:
  phantom-red: "#E5191C"
  night-ground: "#0B0B0D"
  rail-black: "#050506"
  surface: "#18181B"
  surface-raised: "#222226"
  line: "#34343A"
  ink: "#FFFFFF"
  mute: "#B4B4BC"
  dim: "#8A8A94"
  "off": "#3A3A40"
typography:
  display:
    fontFamily: "Anton"
    fontSize: "34px"
    fontWeight: 400
  headline:
    fontFamily: "Anton"
    fontSize: "22px"
    fontWeight: 400
  title:
    fontFamily: "Anton"
    fontSize: "20px"
    fontWeight: 400
  countdown:
    fontFamily: "Barlow Condensed"
    fontSize: "96px"
    fontWeight: 800
    fontFeature: "tnum"
  time:
    fontFamily: "Barlow Condensed"
    fontSize: "21px"
    fontWeight: 800
    fontFeature: "tnum"
  button:
    fontFamily: "Barlow Condensed"
    fontSize: "16px"
    fontWeight: 800
  nav:
    fontFamily: "Anton"
    fontSize: "25px"
    fontWeight: 400
  label:
    fontFamily: "Barlow Condensed"
    fontSize: "14px"
    fontWeight: 700
  body:
    fontFamily: "Barlow"
    fontSize: "14px"
    fontWeight: 500
  control:
    fontFamily: "Barlow"
    fontSize: "14px"
    fontWeight: 600
  chip:
    fontFamily: "Barlow"
    fontSize: "13px"
    fontWeight: 700
  meta:
    fontFamily: "Barlow"
    fontSize: "12px"
    fontWeight: 500
spacing:
  2xs: "4px"
  xs: "6px"
  sm: "8px"
  md: "12px"
  lg: "14px"
  xl: "18px"
  2xl: "22px"
components:
  button-primary:
    backgroundColor: "{colors.phantom-red}"
    textColor: "{colors.ink}"
    typography: "{typography.button}"
    padding: "8px 20px"
    height: "36px"
  button-primary-hover:
    backgroundColor: "{colors.ink}"
    textColor: "{colors.night-ground}"
  button-primary-disabled:
    backgroundColor: "{colors.off}"
    textColor: "{colors.dim}"
  button-ghost:
    backgroundColor: "transparent"
    textColor: "{colors.ink}"
    typography: "{typography.button}"
    padding: "7px 18px"
    height: "36px"
  button-ghost-hover:
    backgroundColor: "{colors.ink}"
    textColor: "{colors.night-ground}"
  button-icon:
    backgroundColor: "{colors.surface-raised}"
    textColor: "{colors.ink}"
    width: "38px"
    height: "34px"
  button-icon-hover:
    backgroundColor: "{colors.phantom-red}"
    textColor: "{colors.ink}"
  button-link:
    backgroundColor: "transparent"
    textColor: "{colors.ink}"
  button-link-hover:
    textColor: "{colors.phantom-red}"
  nav-item:
    backgroundColor: "transparent"
    textColor: "{colors.dim}"
    typography: "{typography.nav}"
    height: "50px"
  nav-item-active:
    backgroundColor: "{colors.phantom-red}"
    textColor: "{colors.ink}"
  heading-tab:
    backgroundColor: "{colors.phantom-red}"
    textColor: "{colors.ink}"
    typography: "{typography.title}"
    padding: "2px 18px 3px 12px"
  chip-major:
    backgroundColor: "{colors.phantom-red}"
    textColor: "{colors.ink}"
    typography: "{typography.chip}"
    padding: "2px 8px 3px"
  chip-regular:
    backgroundColor: "{colors.ink}"
    textColor: "{colors.night-ground}"
    typography: "{typography.chip}"
    padding: "2px 8px 3px"
  chip-major-on-red:
    backgroundColor: "{colors.night-ground}"
    textColor: "{colors.ink}"
  chip-alarm-off:
    backgroundColor: "transparent"
    textColor: "{colors.mute}"
  status-tag:
    backgroundColor: "{colors.night-ground}"
    textColor: "{colors.ink}"
    padding: "3px 14px 3px 12px"
  status-tag-firing:
    backgroundColor: "{colors.ink}"
    textColor: "{colors.phantom-red}"
  status-tag-muted:
    backgroundColor: "{colors.night-ground}"
    textColor: "{colors.phantom-red}"
  input-select:
    backgroundColor: "{colors.surface-raised}"
    textColor: "{colors.ink}"
    typography: "{typography.control}"
    padding: "0 36px 0 16px"
    height: "36px"
  input-select-item-highlighted:
    backgroundColor: "{colors.phantom-red}"
    textColor: "{colors.ink}"
  switch-off:
    backgroundColor: "{colors.off}"
    textColor: "{colors.mute}"
    width: "48px"
    height: "22px"
  switch-on:
    backgroundColor: "{colors.phantom-red}"
    textColor: "{colors.ink}"
  tooltip:
    backgroundColor: "{colors.night-ground}"
    textColor: "{colors.ink}"
    padding: "6px 10px"
  fixture-row-next:
    backgroundColor: "{colors.phantom-red}"
    textColor: "{colors.ink}"
    height: "36px"
---

# Design System: Melissia Boss Timer

## Overview

**Creative North Star: "The Match-Day Calling Card"**

Every boss spawn is billed like a fixture, and the next one is on air in a broadcast lower-third, spoken in a Persona 5 homage: night-black ground, white ink, and one live red. The window is a native WPF desktop app (1100 x 720 default) used in a dim room, either glanced at between fullscreen sessions or left on a second monitor, so the countdown is built to read across a desk and everything else steps back.

The world is made of cut paper, not panels. Every block is a skewed or jagged polygon (rail items, banner, heading tabs, tags, buttons, frames), stacked as flat planes: a white cut behind a red plate behind black type. Halftone dots and red shards are the only texture, and they stay behind content. Boss names are dealt as ransom-note letters; when an alarm fires, a calling card slides in on its slant. Motion is a snap along the diagonal, never a soft fade.

This is an homage to the language only. No Atlus or Persona logos, characters, artwork or quoted text, and no Pearl Abyss or Melissia logos or game art. Boss portraits are original white-and-black stencil art; the red behind them always comes from the UI.

**Key Characteristics:**
- Black night ground, white ink, Phantom Red as the one live colour.
- Every shape is a polygon drawn by `Shard` in 0..1 coordinates; nothing is rounded.
- Layered cut planes (white offset cut under a red plate) instead of shadows.
- Anton for display and ransom letters, Barlow Condensed tabular numerals for every time, Barlow for UI text.
- Buttons are marked by a cut, an outline or a leading icon; headings are flat red tabs and never carry any of those.
- Fast exponential snaps along the slant (120 to 200ms).

## Colors

A three-ink palette: near-black grounds in a cool violet-grey family, pure white ink, and a single saturated red that means "live".

### Primary
- **Phantom Red** (`phantom-red`, Theme key `Red`): the one live colour. The banner field, the next fixture row, the active rail shard, heading tabs, major-boss chips, primary button plates, switch-on tracks, slider fill, dragged scrollbar thumb, ComboBox highlight and open stroke, tooltip and popup borders, the firing/muted status text, and the plate behind every boss portrait. It marks what is next, selected, armed or firing; nothing decorative.

### Neutral
- **Night Ground** (`night-ground`, `Ground`): window background, the countdown frame fill, status-tag plate, chip ink on white, row separators, popup and tooltip fills.
- **Rail Black** (`rail-black`, `Rail`): the 176px left rail and the 30px footer status line; one step darker than the ground so the chrome recedes.
- **Surface** (`surface`, `Surface`): fixture card bodies, the alarm side panel, settings groups, enabled boss rows, sound rows.
- **Surface Raised** (`surface-raised`, `Surface2`): ComboBox box, icon-button plates, the selected dropdown item, the sound row that is currently previewing.
- **Line** (`line`, `Line`): the 1px resting stroke of the ComboBox box. Nowhere else.
- **Ink** (`ink`, `Ink`): primary text, the white cut planes, regular-boss chips, outlines of ghost buttons and frames, dashed focus rings, switch knob when on, slider thumb.
- **Mute** (`mute`, `Mute`): secondary text: body copy, field labels, the countdown caption, the footer line, switch knob when off, alarm-off chip text.
- **Dim** (`dim`, `Dim`): inactive rail words, times of rows with every alarm off, the dashed outline of alarm-off chips, disabled button labels.
- **Off** (`off`, `Off`): inert fills: switch-off track, slider rest track, resting scrollbar thumb, disabled button plate, dimmed portrait plate.

### Named Rules
**The One Live Colour Rule.** Red is the only hue. It marks the next, the selected, the armed and the firing; a screen with red on something that is none of those is wrong.

**The Red Comes From The UI Rule.** Boss art is white and black only. Red reaches a portrait from the plate or banner behind it, never from the artwork.

## Typography

**Display Font:** Anton (self-hosted, OFL), single weight
**Numerals Font:** Barlow Condensed Bold / ExtraBold (self-hosted, OFL), tabular figures
**Body Font:** Barlow Medium / SemiBold / Bold (self-hosted, OFL)
**Icon Font:** Segoe Fluent Icons, falling back to Segoe MDL2 Assets (system)

**Character:** a poster face shouting over a condensed sports-clock numeral, with a plain grotesk doing the reading. Only the shipped weights exist: Anton 400; Barlow 500, 600, 700; Barlow Condensed 700, 800.

### Hierarchy
- **Display** (Anton, 34px, skewed -6deg): page titles (`PageTitle`: FIXTURES and its siblings). Also the brand word (28px) and rail menu words (25px, tilted -4deg).
- **Headline** (Anton, 22px): panel titles (`SectionTitle`, e.g. ALARM).
- **Title** (Anton, 20px, uppercase): heading-tab text; Anton 13 to 14px for the NEXT and DEFAULT badges.
- **Countdown** (Barlow Condensed 800, tabular, 96px; 74px once it needs h:mm:ss): the banner countdown, scaled down only (DownOnly Viewbox) to stay inside its frame.
- **Time** (Barlow Condensed 800, tabular): every clock value. Alarm card 52px, next-alarm 40px, banner spawn time 36px, rail clock 34px, fixture rows 21px, volume and per-day counts 20px.
- **Label** (Barlow Condensed 700, 14 to 15px, uppercase by content): field labels in Mute above controls, the banner date meta and status text.
- **Button** (Barlow Condensed 800, 16px primary, 15px ghost, uppercase): button labels in Ink.
- **Body** (Barlow 500, 14px, Mute, wraps): explanatory copy; notes at 13px.
- **Control** (Barlow 600, 14px): ComboBox, switch labels, link buttons (13px, underlined).
- **Chip** (Barlow 700, 13px): boss chips.
- **Meta** (Barlow 500, 12 to 12.5px): rail clock meta, footer status line, "tmrw" suffix. This is the floor.

### Named Rules
**The Tabular Clock Rule.** Every time, countdown, percentage or count is Barlow Condensed with tabular numerals, so digits never jitter as they tick.

**The Eleven Pixel Floor.** Functional text is never below 11px; the build's smallest is 12px.

**The Ransom Rule.** Ransom-note lettering is reserved for boss names in the banner (54px letters) and the alarm card (58px). It is never body or UI text.

## Layout

A fixed broadcast frame. The main window defaults to 1100 x 720 and stops at 980 x 640. A 176px rail runs down the left (brand, menu words, live local clock with UTC offset); a 172px lower-third banner spans the rest of the top; a 30px Rail-black status line runs the full width at the bottom (PC time zone, schedule source, GMT note, Refresh link). Pages fill the remaining field.

Within the banner: a status tag and date meta on the first line, ransom boss names in the middle, spawn time with the following spawn underneath; the lead boss cutout stands on the banner's bottom edge; the countdown sits in a 300px column in a frame tilted -3deg.

The Schedule page splits into fixtures and a 286px alarm panel (the direction contract said 264; the build widened it). Fixtures run in two balanced columns with a 14px gutter, each part of day a heading tab over a Surface card of 36px-minimum rows, with a 76px time column. Pages inset 22px left and 18px top; panels pad 18px; labels sit 14px below the control above and 6px above their own control. Rows separate with a 1 to 2px Night Ground bottom rule, not a stroke colour.

Spacing reuses a short scale (4, 6, 8, 12, 14, 18, 22px). There are no breakpoints: this is a desktop window, and the only adaptive behaviour is DownOnly scaling of the ransom names and countdown, column balancing, and scrolling inside the fixtures and the panel.

## Elevation & Depth

Flat. There are no shadows and no blur anywhere. Depth is made by stacking flat planes in a fixed order: a white cut, offset about 4px down and right (or extending past the plate), then the red or black plate, then the type. The banner is a white slash behind a red field; the active rail item is a red shard over a white cut; the primary button is a red plate on a white cut; the alarm card is a red shard, a white cut and a black card, three planes deep. Halftone dots (white, 6px tile, 1.1px dots, 12 to 22% opacity, faded in toward the slanted edge) are the only texture, and only on red or black fields behind content.

### Named Rules
**The Cut Plane Rule.** The offset white cut is a plane with its own ink, part of the silhouette, and it inverts with state (red on primary-button hover). It is never a grey or blurred shadow, and it never appears without a solid plate on top.

**The Texture Stays Behind Rule.** Halftone sits on fields, behind content, masked off before the text; it never runs under body copy.

## Shapes

Every shape is a polygon. The `Shard` control draws a polygon in 0..1 coordinates that fills the box it is given and never asks for space itself (it measures to 0 x 0), so the text beside it sets the size and the shape follows. That is the basis of every plate, tab, frame, tag, badge, switch track and portrait in the app.

- Parallelograms for controls: buttons (6% lean), icon buttons and switch tracks (12%), ComboBox (3%), slider thumb (35%).
- Jagged quads for frames and fields: no two edges parallel, a few percent off true (rail shard, countdown frame, next-alarm frame, calling card).
- Heading tabs lean 4% on the left and right edges.
- Corners are always sharp: zero radius, mitre joins on strokes.
- Strokes, where used, run the full perimeter: 1px Line at rest, 2px Ink on ghost buttons and frames, 3px Ink on the countdown frame, 2px Red on popups and tooltips.
- Dashed strokes carry two meanings only: keyboard focus (2px Ink, 2 2 dashes, offset 3 to 4px outside the shape) and "off" (alarm-off chips, muted status tag, 3 2 dashes).
- Boss portraits are clipped to the same jagged quad as their red plate; the banner cutout is clipped by a single left slash parallel to the banner's slant.

## Components

### Buttons
Tactile, poster-loud, unmistakably pressable.
- **Shape:** parallelogram plate, sharp corners, minimum height 36px.
- **Primary:** Phantom Red plate on a white cut offset 4px down and right; Barlow Condensed 800 uppercase, 16px, padding 8 x 20px. Dialog-scale variant on the alarm card at 20px with 10 x 30px padding.
- **Hover:** the plate turns white, the cut turns red, the label turns Night Ground: the selection inversion.
- **Pressed:** the whole button shifts 2px down and right onto its cut.
- **Focus:** a dashed 2px Ink ring in the same parallelogram, 4px outside.
- **Disabled:** Off plate, Dim label, no cut.
- **Ghost (secondary):** transparent plate with a 2px Ink outline and no cut; 15px label, 7 x 18px padding; hover fills white with a black label.
- **Leading icon:** an optional Segoe Fluent glyph (14px, 8px gap) before the label (Add sound, Test alarm).
- **Icon button:** 38 x 34 Surface Raised parallelogram holding a 15px Fluent glyph; hover turns the plate red; a playing preview stays red; disabled drops to 35% opacity.
- **Link button:** Barlow 600 13px underlined Ink text, red on hover, dashed 1px focus rectangle.

### Chips
- **Major:** Phantom Red block, white text.
- **Regular:** white block, Night Ground text.
- **Alarm off:** no fill, 1px dashed Dim outline, Mute text.
- **On the red next row:** majors flip to Night Ground with white text; alarm-off chips use a white dashed outline and white text.
- Chips are rectangles (the one unskewed block), padded 2 x 8px, wrapping inside the row.

### Heading Tabs
The header of every block: a flat Phantom Red shard leaning 4% at both ends, Anton uppercase 20px in white, with an optional Barlow Condensed 700 15px tabular aside (a time range, "· TOMORROW"). No cut, no outline, no icon, no hover, ever: that absence is what tells a heading from a button.

### Cards / Containers
- **Corner Style:** sharp, zero radius.
- **Background:** Surface for cards and panels; Night Ground for disabled boss rows; Surface Raised for the active sound row.
- **Shadow Strategy:** none; see Elevation & Depth.
- **Border:** none on cards; rows divide with a 1 to 2px Night Ground bottom rule.
- **Internal Padding:** 18px on panels; rows inset 12px.
- **Next row:** the whole row inverts to Phantom Red, the time stays white, and a black NEXT badge (Anton 13px on a parallelogram) sits under the time.

### Inputs / Fields
- **ComboBox:** Surface Raised parallelogram (3% lean) with a 1px Line stroke, Barlow 600 14px, a solid white triangle caret 16px from the right, minimum height 36px.
- **Hover:** stroke turns Ink. **Focus:** 2px dashed Ink stroke. **Open:** solid Red stroke; the dropdown is a Night Ground panel with a 2px Red border, items 8 x 14px, selected on Surface Raised, highlighted inverted to Red.
- **Switch:** a 48 x 22 parallelogram track with a square knob (18 x 14), label on the left. Off: Off track, Mute knob. On: Red track, white knob sliding 18px. Hover adds a 1px Ink stroke; focus a dashed ring.
- **Slider:** 6px track, Red fill to the thumb and Off beyond; a 14 x 24 white parallelogram thumb that turns red with a white stroke on hover; dashed focus rectangle.
- **Scrollbar:** 8px, vertical only, a bare thumb (Off, Mute on hover, Red while dragged); no arrows, no track.
- **Tooltip:** Night Ground with a 2px Red border, Barlow 500 13px white, 6 x 10px padding.
- **Disabled:** 35 to 40% opacity.

### Navigation
The left rail holds tilted Anton menu words (25px, rotated -4deg, 50px rows). Resting words are Dim; hover turns them white. The active item is white on a jagged red shard over a white cut, and the shard slides in 40px along the rail (140ms, exponential ease-out). Keyboard focus draws a dashed white outline of the shard. The rail tabs as a single stop (arrow keys move between items).

### Status Tag
The banner's first line: a Night Ground parallelogram tag carrying data (alarm time and sound, "ALARM AT SPAWN", "ALL ALARMS OFF"), Barlow Condensed 800 15px. Armed: white text. Firing: white plate, red text. Muted (sound off or every boss off): 1.5px dashed white outline with red text, the alarm-off grammar, because an alarm nobody will hear must not look armed. It carries data; it is never a decorative label above a heading.

### Ransom Letters (signature)
Boss names in the banner and on the alarm card, one glyph per tilted box. The first letter of each word is the loud one (red box, white letter, 1.12x); the rest are dealt white-on-black, black-on-white with a 2px white edge, or red, at 0.82 to 1.02x, rotated within +/-7deg, 30% of them in Barlow Condensed. The deal is seeded by the text, so a name always looks the same. Multiple bosses are separated by a red star rotated 12deg. A new name re-deals with a snap: each box scales from 1.6 to 1 over 160ms, 16ms stagger per letter.

### Countdown Frame (signature)
The banner's right column: a jagged quad tilted -3deg, Night Ground fill with a 3px white edge, holding the 96px tabular countdown and an UNTIL SPAWN caption in Mute. Inside the alarm window the frame fills Red and the caption turns white.

### Boss Portraits
Fifteen original stencil portraits (white and black with halftone, no red), generated for this app with prompts embedded as `impeccable:prompt` PNG text and recorded in `art/prompts.py`, processed by `tools/process_boss_art.py` and `tools/embed_boss_provenance.py`. They are not official game art.
- **Portrait:** art clipped into a jagged red shard (40px in boss rows, 180px on the alarm card, tilted 4deg); dimmed bosses drop to an Off plate and 55% art; bosses without art show their Anton initial.
- **Cutout:** art straight on the red banner, no plate, cut by one slash parallel to the banner's slant.

### Calling Card (alarm popup)
A borderless, topmost 780 x 360 window: red shard, white cut, black card with corner halftone, three planes deep. A red BOSS ALARM tab, ransom boss names, the local spawn time at 52px, the time left in red, and a large primary DISMISS button. It arrives sliding 140px and unskewing from -14deg in 200ms.

## Do's and Don'ts

### Do:
- **Do** give every button a mark of action: the white cut under a solid plate, a 2px outline, or a leading Fluent icon (or more than one).
- **Do** keep heading tabs flat: a red shard with white Anton text, no cut, no outline, no icon.
- **Do** build every plate, frame, tag and track from a `Shard` polygon so content sets the size and the shape follows.
- **Do** spend Phantom Red (#E5191C) only on what is next, selected, armed or firing.
- **Do** set every time, countdown and count in Barlow Condensed with tabular numerals.
- **Do** take icons from Segoe Fluent Icons / Segoe MDL2 Assets through `Ui.Glyph`.
- **Do** show focus as a dashed 2px white outline that follows the control's own polygon.
- **Do** use dashed outlines for "off" (alarm-off chips, muted status tag) so a silent alarm never looks armed.
- **Do** keep halftone on red or black fields behind content, masked away from text.
- **Do** animate as fast exponential snaps along the slant (120 to 200ms) and skip the signature motion when Windows client-area animation is off.
- **Do** keep boss art white and black and let the red plate or banner behind it supply the colour.

### Don't:
- **Don't** put a white cut, outline or icon on a heading tab; that is what makes a button.
- **Don't** use Unicode symbols or emoji as icons (no arrows, triangles, stars or check marks typed as text).
- **Don't** round any corner, on any element.
- **Don't** use a coloured single-edge accent bar wider than 1px on a card, row or callout; full-perimeter strokes on frames and controls are fine.
- **Don't** put a kicker or eyebrow label above a heading; status tags carry live data instead.
- **Don't** set functional text below 11px.
- **Don't** add a second accent hue, a gradient fill, a blur or a soft drop shadow.
- **Don't** use Atlus or Persona logos, characters, artwork or quoted game text, or Pearl Abyss or Melissia logos or game art; this is a homage to the UI language only.
- **Don't** fade things in softly; motion snaps.
