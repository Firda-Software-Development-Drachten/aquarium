import sys
import unittest
from pathlib import Path
from unittest.mock import patch

# Zorg dat Python de scripts in de map tools/ kan vinden.
sys.path.insert(0, str(Path(__file__).resolve().parent.parent))

from stats import count_stats


class TestStats(unittest.TestCase):
    @patch("stats.load_options")
    @patch("stats.validate_files")
    def test_count_stats(self, validate, options):
        fish = {
            "body": "round",
            "fins": "small",
            "pattern": "striped",
            "behavior": "normal",
            "size": 1,
            "speed": 2,
            "colors": {"body": "#ff0000", "fins": "#ff0000", "pattern": "#0000ff"},
        }

        validate.return_value = ({"fish1": fish}, [])

        stats = count_stats()

        self.assertEqual(stats["body"]["round"], 1)
        self.assertEqual(stats["colors"]["#ff0000"], 2)
        self.assertEqual(stats["colors"]["#0000ff"], 1)


if __name__ == "__main__":
    unittest.main()
