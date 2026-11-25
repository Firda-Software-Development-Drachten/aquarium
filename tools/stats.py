from collections import defaultdict
from validate_fish import (
    load_options, validate_files, FISH_DIR,
    CHOICE_FIELDS, NUMBER_FIELDS
)


def count_stats():
    options = load_options()
    fishList, errors = validate_files(FISH_DIR.glob("*.json"), options)

    if errors:
        raise ValueError("\n".join(errors))

    stats = {
        field: defaultdict(int)
        for field in CHOICE_FIELDS + NUMBER_FIELDS
    }
    stats["colors"] = defaultdict(int)

    # Tel alle keuzes en kleuren van iedere vis.
    for fish in fishList.values():
        for field in CHOICE_FIELDS + NUMBER_FIELDS:
            stats[field][fish[field]] += 1

        for color in fish["colors"].values():
            stats["colors"][color.lower()] += 1

    return stats
