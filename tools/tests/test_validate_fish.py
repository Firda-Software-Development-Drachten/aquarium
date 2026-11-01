"""Tests voor de validator.

Draaien vanuit de hoofdmap van de repository:
    python -m unittest discover tools/tests
"""

import copy
import sys
import unittest
from pathlib import Path

# Zorg dat Python validate_fish.py in de map tools/ kan vinden.
sys.path.insert(0, str(Path(__file__).resolve().parent.parent))

from validate_fish import check_unique_names, load_options, validate_file_name, validate_fish

GOOD_FISH = {
    "name": "Testvis",
    "maker": "Tester",
    "body": "round",
    "fins": "small",
    "pattern": "none",
    "colors": {"body": "#ff8800", "fins": "#ffffff", "pattern": "#222222"},
    "size": 1.0,
    "speed": 1.0,
    "behavior": "calm",
}


class ValidateFishTest(unittest.TestCase):
    def setUp(self):
        self.options = load_options()
        # Elke test krijgt een eigen kopie, zodat tests elkaar niet beïnvloeden.
        self.fish = copy.deepcopy(GOOD_FISH)

    def test_good_fish_has_no_errors(self):
        self.assertEqual(validate_fish(self.fish, self.options, "testvis.json"), [])

    def test_missing_field(self):
        del self.fish["maker"]
        errors = validate_fish(self.fish, self.options, "testvis.json")
        self.assertEqual(len(errors), 1)
        self.assertIn("'maker' ontbreekt", errors[0])

    def test_unknown_option(self):
        self.fish["fins"] = "puntig"
        errors = validate_fish(self.fish, self.options, "testvis.json")
        self.assertEqual(len(errors), 1)
        self.assertIn("'fins' is 'puntig'", errors[0])

    def test_wrong_color(self):
        self.fish["colors"]["body"] = "oranje"
        errors = validate_fish(self.fish, self.options, "testvis.json")
        self.assertEqual(len(errors), 1)
        self.assertIn("colors.body", errors[0])

    def test_duplicate_name(self):
        other = copy.deepcopy(GOOD_FISH)
        other["name"] = "TESTVIS"
        errors = check_unique_names({"fish/a.json": self.fish, "fish/b.json": other})
        self.assertEqual(list(errors), ["fish/b.json"])

    def test_bad_file_name(self):
        self.assertNotEqual(validate_file_name("fish/Mijn Vis.json"), [])
        self.assertEqual(validate_file_name("fish/mijn-vis.json"), [])


if __name__ == "__main__":
    unittest.main()
