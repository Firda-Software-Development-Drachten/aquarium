"""Controleert dat de Vissenmaker dezelfde keuzes heeft als shared/options.json.

Gebruik:
    python tools/check_maker.py

Heb je een nieuwe keuze toegevoegd, bijvoorbeeld een vinvorm? Dan herinnert
dit script je eraan om dat op alle plekken te doen.
Exitcode 0 als alles klopt, 1 als er verschillen zijn.
"""

import sys
from html.parser import HTMLParser

from validate_fish import COLOR_PATTERN, ROOT, load_options

MAKER_FILE = ROOT / "maker" / "index.html"

# Keuzes met een id en een Nederlands label in options.json.
CHOICE_FIELDS = ["body", "fins", "pattern", "behavior"]
# Keuzes die een getal zijn.
NUMBER_FIELDS = ["size", "speed"]
# De drie kleuren van een vis. De Vissenmaker kiest een kleur op een van twee manieren:
# - keuzerondjes met name="bodyColor" en een kleur als waarde, zoals #ff8800
# - drie schuifjes (tint, verzadiging, helderheid): name="bodyHue", "bodySat", "bodyValue"
COLOR_PARTS = ["body", "fins", "pattern"]
# Per schuifje: (achtervoegsel, min, max)
COLOR_SLIDERS = [("Hue", "0", "360"), ("Sat", "0", "100"), ("Value", "0", "100")]


class MakerParser(HTMLParser):
    """Leest index.html en onthoudt alle invoervelden en labels."""

    def __init__(self):
        super().__init__()
        self.radios = {}  # name -> lijst met (id, value)
        self.text_inputs = {}  # name -> maxlength
        self.ranges = {}  # name -> (min, max) van een schuifje
        self.labels = {}  # id van het invoerveld -> tekst van het label
        self.current_label = None

    def handle_starttag(self, tag, attrs):
        attrs = dict(attrs)
        if tag == "input" and attrs.get("type") == "radio":
            self.radios.setdefault(attrs.get("name"), []).append((attrs.get("id"), attrs.get("value")))
        elif tag == "input" and attrs.get("type") == "range":
            self.ranges[attrs.get("name")] = (attrs.get("min"), attrs.get("max"))
        elif tag == "input" and attrs.get("type") == "text":
            self.text_inputs[attrs.get("name")] = attrs.get("maxlength")
        elif tag == "label":
            self.current_label = attrs.get("for")
            self.labels[self.current_label] = ""

    def handle_data(self, data):
        if self.current_label is not None:
            self.labels[self.current_label] += data

    def handle_endtag(self, tag):
        if tag == "label":
            self.current_label = None


def check_choices(field, options, parser):
    """Vergelijkt de keuzes voor één veld, zoals fins, in beide richtingen."""
    errors = []
    in_options = {option["id"]: option["label"] for option in options[field]}
    in_maker = {value: radio_id for radio_id, value in parser.radios.get(field, [])}

    for option_id, label in in_options.items():
        if option_id not in in_maker:
            errors.append(f"'{field}' heeft in options.json de keuze '{option_id}', maar de Vissenmaker niet")
            continue
        maker_label = parser.labels.get(in_maker[option_id], "").strip()
        if maker_label != label:
            errors.append(
                f"'{field}': het label van '{option_id}' is '{maker_label}' in de Vissenmaker, "
                f"maar '{label}' in options.json"
            )
    for value in in_maker:
        if value not in in_options:
            errors.append(f"'{field}' heeft in de Vissenmaker de keuze '{value}', maar options.json niet")
    return errors


def check_numbers(field, options, parser):
    """Vergelijkt de getallen voor size of speed in beide richtingen."""
    errors = []
    in_options = set(options[field])
    in_maker = set()
    for _, value in parser.radios.get(field, []):
        try:
            in_maker.add(float(value))
        except (TypeError, ValueError):
            errors.append(f"'{field}' heeft in de Vissenmaker de waarde '{value}', maar dat is geen getal")

    for number in sorted(in_options - in_maker):
        errors.append(f"'{field}' heeft in options.json de waarde {number}, maar de Vissenmaker niet")
    for number in sorted(in_maker - in_options):
        errors.append(f"'{field}' heeft in de Vissenmaker de waarde {number}, maar options.json niet")
    return errors


def check_colors(parser):
    """Controleert dat elk onderdeel een kleurkeuze heeft: keuzerondjes of drie schuifjes."""
    errors = []
    for part in COLOR_PARTS:
        radio_name = part + "Color"
        values = [value for _, value in parser.radios.get(radio_name, [])]
        sliders = [part + suffix for suffix, _, _ in COLOR_SLIDERS]

        if values:
            # Keuzerondjes: elke waarde moet een kleur zijn zoals #ff8800.
            for value in values:
                if value is None or not COLOR_PATTERN.match(value):
                    errors.append(f"'{radio_name}' heeft in de Vissenmaker de kleur '{value}', maar moet een kleur zijn zoals #ff8800")
        elif any(name in parser.ranges for name in sliders):
            # Schuifjes: alle drie moeten er zijn, met het juiste bereik.
            for suffix, low, high in COLOR_SLIDERS:
                name = part + suffix
                if name not in parser.ranges:
                    errors.append(f"de Vissenmaker mist het schuifje '{name}'")
                elif parser.ranges[name] != (low, high):
                    found_low, found_high = parser.ranges[name]
                    errors.append(f"het schuifje '{name}' loopt van {found_low} tot {found_high}, maar moet van {low} tot {high} lopen")
        else:
            errors.append(
                f"de Vissenmaker heeft geen kleurkeuze voor '{part}': "
                f"gebruik keuzerondjes '{radio_name}' of de schuifjes {', '.join(sliders)}"
            )
    return errors


def check_text_limits(options, parser):
    """Controleert dat maxlength van naam en maker klopt met options.json."""
    errors = []
    limits = {"name": options["limits"]["nameMaxLength"], "maker": options["limits"]["makerMaxLength"]}
    for field, limit in limits.items():
        if field not in parser.text_inputs:
            errors.append(f"de Vissenmaker heeft geen tekstveld '{field}'")
        elif parser.text_inputs[field] != str(limit):
            errors.append(
                f"het tekstveld '{field}' heeft maxlength {parser.text_inputs[field]}, "
                f"maar options.json zegt {limit}"
            )
    return errors


def check_maker(options, html):
    """Geeft een lijst met alle verschillen terug. Een lege lijst betekent: alles klopt."""
    parser = MakerParser()
    parser.feed(html)

    errors = []
    for field in CHOICE_FIELDS:
        errors += check_choices(field, options, parser)
    for field in NUMBER_FIELDS:
        errors += check_numbers(field, options, parser)
    errors += check_colors(parser)
    errors += check_text_limits(options, parser)
    return errors


def main():
    options = load_options()
    html = MAKER_FILE.read_text(encoding="utf-8")
    errors = check_maker(options, html)

    for message in errors:
        print(f"maker/index.html: {message}")

    if errors:
        print(f"\n{len(errors)} verschil(len) gevonden tussen de Vissenmaker en shared/options.json.")
        return 1

    print("Alles klopt! De Vissenmaker en options.json hebben dezelfde keuzes.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
