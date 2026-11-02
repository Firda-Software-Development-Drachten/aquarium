import json
import os
import glob
import pprint
from pathlib import Path
from collections import defaultdict

grouped = defaultdict(list) # Defaultdict dat automatisch default waardes aan keys kan geven.
fishList = []
path = '../fish/'



# Loop door het ../fish directory, opent elke bestand in utf-8 om de vereiste keywords af te lezen.
for filename in glob.glob(os.path.join(path, '*.json')):
    with open(filename, encoding='utf-8') as currentFile:
        data = json.load(currentFile)
        fish = {
            "name": data.get("name"),
            "maker": data.get("maker")
        }
        if fish is not None and fish not in fishList: # Validatie check voor het geval dat incorrect data wordt doorgebracht.
            fishList.append(fish)
        else:
            raise Exception("Er zijn geen vissen gevonden om toe te voegen.")

readme = Path("../README.md")
start, end = "<!-- hall-of-fame:start -->", "<!-- hall-of-fame:end -->"

text = readme.read_text(encoding="utf-8") # README.md content aflezen in utf-8 formaat.
before, rest = text.split(start, 1) # Scheidt alles voor de start comment op
_, after = rest.split(end, 1) # Scheidt alles na de start comment op

for fish in fishList:
    grouped[fish["maker"]].append(fish["name"])

# Schrijf alle markdown content in een enkel variabel.
content = "\n".join(
    f"### {maker} \n" + "\n".join(f"- {name}" for name in names)
    for maker, names in grouped.items()
)

# Herschrijf README.md met het bijgewerkte hall_of_fame.py
readme.write_text(
    f"{before}{start}\n{content}\n{end}{after}",
    encoding="utf-8"
)

pprint.pprint(fishList)
