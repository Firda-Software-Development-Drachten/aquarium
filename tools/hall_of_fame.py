import json
import os
import glob
import pprint

fishList = []
path = '../fish/'

for filename in glob.glob(os.path.join(path, '*.json')):
    with open(filename, encoding='utf-8') as currentFile:
        data = json.load(currentFile)
        fish = {
            "name": data.get("name"),
            "maker": data.get("maker")
        }
        if fish is not None and fish not in fishList:
            fishList.append(fish)



pprint.pprint(fishList)