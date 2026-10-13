# Sketch brief: Melissia Boss Timer decision sketches

Each sketch is ONE self-contained HTML file showing the FIRST SCREEN of a native Windows desktop app window (the app's own client area, no browser chrome, no OS title bar), 1100 x 720 px.

Hard rules
- `<body>` fixed 1100x720, `overflow:hidden`, margin 0. Inline `<style>` only. No JavaScript. Nothing fetched except Google Fonts (optional). Under 200 lines.
- Real content only (below). No lorem ipsum, no placeholder tiles, no invented features, no game logos or art.
- It is a working product screen (Operate mode): a clear shell and navigation (Schedule / Bosses / Sounds / Settings), a distinct area for each job, standard controls (buttons, toggles, sliders, tabs, lists). The world contributes ONLY type, palette, density and one signature move. It never replaces the layout or controls with a costume (no fake instrument bezels, no paper textures standing in for tables).
- The decision page renders text under 22px as grey bars, so composition, palette, and display type carry the read. Make the hierarchy obvious at that abstraction: the next spawn + countdown must dominate.
- Craft bar: aligned grid, consistent spacing, real type scale, tabular numerals for times.

What the app does
Shows Melissia (Black Desert private server) world boss spawns converted from GMT to this PC's local time, counts down, and alarms (sound + popup) N minutes before. Users: players who keep it in the tray during fullscreen play, or open on a second monitor beside the game (countdown must read across a room).

Real screen state to show (PC time zone UTC+07:00, now 04:52:12, Wednesday 14 Oct)
- NEXT: Kzarka + Bulgasal at 05:00, in 7m 48s. Alarm set 5 min before (fires at 04:55). Sound for Kzarka: "War Horn"; volume 80%.
- Upcoming (local time, boss):
  05:00 Kzarka, Bulgasal · 05:15 Uturi · 05:30 Golden Pig King · 05:45 Sangoon · 09:00 Bulgasal · 09:15 Uturi · 09:30 Golden Pig King · 09:45 Sangoon · 13:00 Targargo, Bulgasal · 13:15 Uturi · 13:30 Katzvariak, Golden Pig King · 13:45 Sangoon · 14:00 Giant Mudster, Red Nose · 16:00 Stormbringer Karanda · 17:00 Vell, Bulgasal · 18:00 Kzarka · 19:30 Quint, Muraka · 20:00 Ancient Kutum · 21:00 Targargo, Bulgasal · 22:00 Karanda · 23:00 Garmoth · 01:00 Bulgasal · 01:30 Giant Mudster, Red Nose, Golden Pig King · 03:30 Thundercloud Ancient Kutum
- Boss rank: MAJOR = Garmoth, Kzarka, Karanda, Stormbringer Karanda, Vell, Quint, Muraka, Ancient Kutum, Thundercloud Ancient Kutum, Targargo, Katzvariak, Giant Mudster, Red Nose. REGULAR (every 4 h) = Bulgasal, Uturi, Golden Pig King, Sangoon. User has alarms OFF for Uturi and Sangoon (show them muted).
- Footer/status: "PC time zone UTC+07:00 (Bangkok, Hanoi, Jakarta) · schedule from wiki, checked 04:31 · times on wiki are GMT".

Window size note: it's a desktop window, so use the full 1100x720 as the app; a left nav rail or top tabs are both fine.
