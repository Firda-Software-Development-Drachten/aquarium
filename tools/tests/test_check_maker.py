"""Tests voor check_maker.py: de kleurkeuze in de Vissenmaker.

Draaien vanuit de hoofdmap van de repository:
    python -m unittest discover tools/tests
"""

import sys
import unittest
from pathlib import Path

# Zorg dat Python de scripts in de map tools/ kan vinden.
sys.path.insert(0, str(Path(__file__).resolve().parent.parent))

from check_maker import MakerParser, check_colors


def colors_in(html):
    parser = MakerParser()
    parser.feed(html)
    return check_colors(parser)


def sliders(part, hue_max="360"):
    return (
        f'<input type="range" name="{part}Hue" min="0" max="{hue_max}">'
        f'<input type="range" name="{part}Sat" min="0" max="100">'
        f'<input type="range" name="{part}Value" min="0" max="100">'
    )


class CheckColorsTest(unittest.TestCase):
    def test_radio_buttons_are_fine(self):
        html = "".join(f'<input type="radio" name="{part}Color" value="#ff8800">' for part in ["body", "fins", "pattern"])
        self.assertEqual(colors_in(html), [])

    def test_wrong_radio_color(self):
        html = '<input type="radio" name="bodyColor" value="oranje">' + sliders("fins") + sliders("pattern")
        errors = colors_in(html)
        self.assertEqual(len(errors), 1)
        self.assertIn("'oranje'", errors[0])

    def test_sliders_are_fine(self):
        self.assertEqual(colors_in(sliders("body") + sliders("fins") + sliders("pattern")), [])

    def test_missing_slider(self):
        html = sliders("body") + sliders("fins") + sliders("pattern").replace('name="patternSat"', 'name="iets"')
        errors = colors_in(html)
        self.assertEqual(len(errors), 1)
        self.assertIn("patternSat", errors[0])

    def test_wrong_slider_range(self):
        errors = colors_in(sliders("body", hue_max="100") + sliders("fins") + sliders("pattern"))
        self.assertEqual(len(errors), 1)
        self.assertIn("bodyHue", errors[0])

    def test_no_color_choice(self):
        errors = colors_in(sliders("body") + sliders("fins"))
        self.assertEqual(len(errors), 1)
        self.assertIn("'pattern'", errors[0])


if __name__ == "__main__":
    unittest.main()
