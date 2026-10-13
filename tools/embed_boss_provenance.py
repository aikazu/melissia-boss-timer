"""Embed each boss portrait's generation prompt into the shipped PNG (impeccable embed-prompt).

Run after tools/process_boss_art.py, since resizing drops PNG metadata.
Usage: python tools/embed_boss_provenance.py <path-to-impeccable.exe>
Pass the engine exe (%USERPROFILE%/.impeccable/bin/<ver>/impeccable.exe), not impeccable.cmd:
cmd.exe cuts arguments at the first newline, which silently truncates the prompts.
"""
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
sys.path.insert(0, str(ROOT / "art"))
from prompts import PROMPTS  # noqa: E402

ASSETS = ROOT / "src" / "MelissiaBossTimer" / "Assets" / "Bosses"
NOTE = "\n\n[Original interpretation, not official game art. Post-processed by tools/process_boss_art.py: trimmed, squared, 512 px.]"


def main() -> int:
    exe = sys.argv[1]
    failed = 0
    for key, prompt in PROMPTS.items():
        r = subprocess.run([exe, "embed-prompt", str(ASSETS / f"{key}.png"), "--prompt", prompt + NOTE], capture_output=True, text=True)
        print(key, "ok" if r.returncode == 0 else "FAIL", (r.stdout + r.stderr).strip()[:160])
        failed += r.returncode != 0
    r = subprocess.run([exe, "embed-prompt", "--scan", str(ASSETS)], capture_output=True, text=True)
    print("scan:", r.returncode, (r.stdout + r.stderr).strip())
    return 1 if failed or r.returncode else 0


if __name__ == "__main__":
    sys.exit(main())
