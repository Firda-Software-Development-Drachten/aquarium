"""Bouwt de website voor GitHub Pages in de map site/.

Gebruik:
    python tools/build_site.py

Wat het doet:
- alle vissen komen in site/fish.json (foute vissen worden overgeslagen)
- shared/options.json wordt gekopieerd naar site/options.json
- maker/ wordt gekopieerd naar site/maker/
- de Unity WebGL-build in webgl/ komt in de root van site/;
  is webgl/ nog leeg, dan komt er een tijdelijke startpagina
"""

import json
import shutil
import sys
from datetime import datetime, timezone

from validate_fish import FISH_DIR, OPTIONS_FILE, ROOT, load_options, validate_files

SITE_DIR = ROOT / "site"
MAKER_DIR = ROOT / "maker"
WEBGL_DIR = ROOT / "webgl"

# Bestanden die in een lege map mogen staan zonder dat die als "gevuld" telt.
IGNORED_FILES = {".gitkeep", ".DS_Store", "Thumbs.db"}

PLACEHOLDER_PAGE = """<!doctype html>
<html lang="nl">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <title>Aquarium</title>
  <style>
    body { font-family: system-ui, sans-serif; background: #0b3d5c; color: #ffffff;
           margin: 0; min-height: 100vh; display: grid; place-items: center; padding: 16px; }
    main { max-width: 32rem; text-align: center; }
    a { color: #ffd166; }
  </style>
</head>
<body>
  <main>
    <h1>Aquarium</h1>
    <p>Het aquarium volgt nog. Er zwemmen al {count} vis(sen) in de wachtrij.</p>
    <p><a href="maker/">Ontwerp je eigen vis in de Vissenmaker</a></p>
  </main>
</body>
</html>
"""


def has_webgl_build():
    """True als er echte bestanden in webgl/ staan."""
    if not WEBGL_DIR.is_dir():
        return False
    return any(path.name not in IGNORED_FILES for path in WEBGL_DIR.iterdir())


def write_fish_json(valid_fish):
    """Schrijft alle geldige vissen, gesorteerd op id, naar site/fish.json."""
    fish_list = []
    for fish_id in sorted(valid_fish):
        fish_list.append({"id": fish_id, **valid_fish[fish_id]})

    content = {
        "generated": datetime.now(timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ"),
        "fish": fish_list,
    }
    with open(SITE_DIR / "fish.json", "w", encoding="utf-8") as file:
        json.dump(content, file, ensure_ascii=False, indent=2)


def copy_folder_contents(source, target):
    """Kopieert alles uit source naar target, behalve de genegeerde bestanden."""
    shutil.copytree(source, target, dirs_exist_ok=True, ignore=shutil.ignore_patterns(*IGNORED_FILES))


def main():
    options = load_options()

    # Begin elke keer met een lege site/, zodat er geen oude bestanden blijven staan.
    if SITE_DIR.exists():
        shutil.rmtree(SITE_DIR)
    SITE_DIR.mkdir()

    valid_fish, errors = validate_files(list(FISH_DIR.glob("*.json")), options)
    for message in errors:
        print(f"Waarschuwing, overgeslagen: {message}")
    write_fish_json(valid_fish)
    print(f"site/fish.json geschreven met {len(valid_fish)} vis(sen).")

    shutil.copy(OPTIONS_FILE, SITE_DIR / "options.json")

    if MAKER_DIR.is_dir():
        copy_folder_contents(MAKER_DIR, SITE_DIR / "maker")
        print("Vissenmaker gekopieerd naar site/maker/.")
    else:
        print("Waarschuwing: de map maker/ bestaat nog niet, de Vissenmaker ontbreekt op de site.")

    if has_webgl_build():
        copy_folder_contents(WEBGL_DIR, SITE_DIR)
        print("Unity WebGL-build gekopieerd naar site/.")
    else:
        with open(SITE_DIR / "index.html", "w", encoding="utf-8") as file:
            file.write(PLACEHOLDER_PAGE.replace("{count}", str(len(valid_fish))))
        print("webgl/ is nog leeg: tijdelijke startpagina geschreven.")

    print("Klaar, de site staat in site/.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
