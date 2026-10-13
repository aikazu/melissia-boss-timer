"""Turn raw generated boss portraits (art/raw/*.png) into the app's assets.

Each portrait is trimmed to its visible pixels, padded to a square (bust sitting on the bottom edge),
and resized to 512 px into src/MelissiaBossTimer/Assets/Bosses/. A raw file without a real alpha
channel or with an opaque corner is rejected rather than shipped with a baked background.

Usage: python tools/process_boss_art.py [--sheet out.png]
"""
import sys
from pathlib import Path

from PIL import Image

ROOT = Path(__file__).resolve().parent.parent
RAW = ROOT / "art" / "raw"
OUT = ROOT / "src" / "MelissiaBossTimer" / "Assets" / "Bosses"
SIZE = 512
RED = (229, 25, 28, 255)


def process(src: Path) -> Image.Image:
    im = Image.open(src)
    if im.mode != "RGBA":
        raise ValueError(f"{src.name}: mode {im.mode}, expected RGBA")
    w, h = im.size
    alpha = im.getchannel("A")
    corners = [alpha.getpixel(p) for p in [(0, 0), (w - 1, 0), (0, h - 1), (w - 1, h - 1)]]
    # The bust may run off the bottom edge, so only the top corners must be clear.
    if max(int(c or 0) for c in corners[:2]) > 8:  # type: ignore[arg-type]
        raise ValueError(f"{src.name}: opaque top corner alpha {corners[:2]}, background not transparent")
    # Ignore faint alpha fringe when trimming.
    bbox = alpha.point([255 if a > 24 else 0 for a in range(256)]).getbbox()
    if bbox is None:
        raise ValueError(f"{src.name}: empty image")
    im = im.crop(bbox)
    side = max(im.size)
    square = Image.new("RGBA", (side, side), (0, 0, 0, 0))
    square.alpha_composite(im, ((side - im.width) // 2, side - im.height))
    return square.resize((SIZE, SIZE), Image.Resampling.LANCZOS)


def main() -> int:
    OUT.mkdir(parents=True, exist_ok=True)
    done, failed = [], []
    for src in sorted(RAW.glob("*.png")):
        try:
            img = process(src)
        except ValueError as e:
            failed.append(str(e))
            continue
        img.save(OUT / src.name, optimize=True)
        done.append((src.stem, img))
    for name, _ in done:
        print("ok  ", name)
    for msg in failed:
        print("FAIL", msg)

    if "--sheet" in sys.argv and done:
        # Contact sheet on the red plate, for review only.
        cell = 200
        cols = 5
        rows = (len(done) + cols - 1) // cols
        sheet = Image.new("RGBA", (cols * cell, rows * cell), (11, 11, 13, 255))
        for i, (_, img) in enumerate(done):
            tile = Image.new("RGBA", (cell - 8, cell - 8), RED)
            tile.alpha_composite(img.resize((cell - 8, cell - 8), Image.Resampling.LANCZOS))
            sheet.alpha_composite(tile, ((i % cols) * cell + 4, (i // cols) * cell + 4))
        sheet.save(sys.argv[sys.argv.index("--sheet") + 1])
    return 1 if failed else 0


if __name__ == "__main__":
    sys.exit(main())
