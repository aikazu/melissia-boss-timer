"""Renders src/MelissiaBossTimer/app.ico: a white alarm bell on a jagged red burst with a white offset cut, no background.

The art is drawn on an oversized canvas, then cropped to its own pixels and centred with an even margin,
so no spike is ever clipped by the icon frame. Each icon size is resampled from that master.

Provenance: authored geometry, drawn with Pillow. Rerun with `python tools/make_icon.py` after changing it.
"""
import math
from pathlib import Path

from PIL import Image, ImageDraw

RED = (229, 25, 28, 255)
BLACK = (11, 11, 13, 255)
WHITE = (255, 255, 255, 255)
S = 1400  # oversized working canvas; cropped afterwards
MASTER = 1024
MARGIN = 0.03  # transparent margin around the art, per side, as a fraction of the icon


def rotate(points, deg, cx, cy):
    a = math.radians(deg)
    return [(cx + (x - cx) * math.cos(a) - (y - cy) * math.sin(a),
             cy + (x - cx) * math.sin(a) + (y - cy) * math.cos(a)) for x, y in points]


def burst(cx, cy, scale):
    # Alternating radii with an irregular rhythm so it reads as cut paper, not a star.
    radii = [500, 360, 470, 330, 510, 380, 450, 340, 490, 360, 515, 335, 455, 375]
    pts = []
    for i, r in enumerate(radii):
        a = 2 * math.pi * i / len(radii) - math.pi / 2 + 0.12
        pts.append((cx + r * scale * math.cos(a), cy + r * scale * math.sin(a)))
    return pts


def draw():
    img = Image.new("RGBA", (S, S), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    c = S / 2

    # White cut behind the red burst, offset down-right: the same layering as the app's buttons and nav.
    d.polygon(burst(c + 34, c + 40, 1.0), fill=WHITE)
    d.polygon(burst(c, c, 1.0), fill=RED)

    # Bell, tilted like it's ringing: dome + flared skirt, white with black cuts (no outline).
    bell = []
    for i in range(0, 181, 5):
        a = math.radians(180 + i)
        bell.append((c + 185 * math.cos(a), c - 30 + 215 * math.sin(a)))
    bell += [(c + 205, c + 170), (c + 255, c + 225), (c - 255, c + 225), (c - 205, c + 170)]
    tilt = -12
    d.polygon(rotate(bell, tilt, c, c), fill=WHITE)
    # Black lip band across the skirt and a black clapper below it.
    band = [(c - 228, c + 150), (c + 228, c + 150), (c + 236, c + 172), (c - 236, c + 172)]
    d.polygon(rotate(band, tilt, c, c), fill=BLACK)
    clapper = [(c - 62, c + 245), (c + 62, c + 245), (c + 40, c + 315), (c - 40, c + 315)]
    d.polygon(rotate(clapper, tilt, c, c), fill=WHITE)
    knob = [(c - 38, c - 300), (c + 38, c - 300), (c + 38, c - 240), (c - 38, c - 240)]
    d.polygon(rotate(knob, tilt, c, c), fill=WHITE)
    # A black highlight slash on the dome, the one cut that keeps the bell from reading as a blob.
    slash = [(c - 120, c - 120), (c - 80, c - 150), (c - 30, c + 60), (c - 70, c + 80)]
    d.polygon(rotate(slash, tilt, c, c), fill=BLACK)
    return img


def fit(img):
    """Crop to the art and centre it on a square with an even transparent margin."""
    art = img.crop(img.getchannel("A").getbbox())
    inner = round(MASTER * (1 - 2 * MARGIN))
    scale = inner / max(art.size)
    art = art.resize((round(art.width * scale), round(art.height * scale)), Image.Resampling.LANCZOS)
    out = Image.new("RGBA", (MASTER, MASTER), (0, 0, 0, 0))
    out.alpha_composite(art, ((MASTER - art.width) // 2, (MASTER - art.height) // 2))
    return out


def main():
    out = Path(__file__).resolve().parents[1] / "src" / "MelissiaBossTimer" / "app.ico"
    master = fit(draw())
    sizes = [16, 20, 24, 32, 40, 48, 64, 128, 256]
    master.resize((256, 256), Image.Resampling.LANCZOS).save(out, sizes=[(s, s) for s in sizes])
    print(f"wrote {out}")


if __name__ == "__main__":
    main()
