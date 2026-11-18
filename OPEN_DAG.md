# Open dag: bezoekers maken een vis

Op de open dag ontwerpen bezoekers een vis in de Vissenmaker en sturen die in met een pull request. Hun vis zwemt een paar minuten later in het aquarium, ook als er geen docent bij is.

## Zo werkt het

1. De bezoeker ontwerpt een vis in de [Vissenmaker](https://firda-software-development-drachten.github.io/aquarium/maker/) en ziet meteen het visbestand (JSON).
2. Met **Stuur in via GitHub** opent GitHub een nieuw bestand `fish/<naam>.json`, al ingevuld.
3. **Commit changes** en **Propose changes**: GitHub maakt zelf een fork en een branch. Daarna **Create pull request**.
4. De workflow `opendag-automerge` controleert de vis en voegt hem samen. Daarna start hij de Pages-workflow.
5. Het aquarium haalt elke minuut `fish.json` op (`FishLoader.cs`). De vis verschijnt dus vanzelf, ook in de kioskpresentatie.

## Voorbereiden (eenmalig)

- [ ] Maak een GitHub-account voor de open dag, bijvoorbeeld `firda-opendag`. Log daarmee in op de laptop bij de stand.
- [ ] Maak met dat account vooraf één test-pull request en keur die zelf goed. Zo telt het niet meer als "first-time contributor", en hoeven de controles niet eerst handmatig te worden goedgekeurd.
- [ ] Settings > Actions > General: zet **Allow GitHub Actions to create and approve pull requests** aan.
- [ ] Settings > Secrets and variables > Actions > Variables:
  - `OPENDAG_ACCOUNT` = de naam van het open-dagaccount
  - `OPENDAG_AUTOMERGE` = `uit` (op de open dag zelf: `aan`)
- [ ] Controleer de ruleset van `main` (Settings > Rules > Rulesets > Bescherm main):
  - een goedkeuring door GitHub Actions moet meetellen voor de verplichte goedkeuring;
  - zet **Require review from Code Owners** aan: zonder die regel kan elke goedkeuring ook wijzigingen in `.github/` en `tools/` goedkeuren (`fish/` staat niet in CODEOWNERS, dus de open dag werkt dan nog steeds);
  - maak `validate` gerust een verplichte controle. `opendag-automerge` wacht tot die klaar is.
- [ ] Settings > Actions > General > "Approval for running fork pull request workflows": een eerste PR van een nieuw account wacht op goedkeuring. Bij "first-time contributors" is het open-dagaccount na één gemergede PR klaar.

## Op de open dag

- Zet `OPENDAG_AUTOMERGE` 's ochtends op `aan` en na afloop weer op `uit`.
- Pull requests van andere accounts blijven gewoon wachten op een docent.
- Een naam met een scheldwoord wordt afgekeurd, al in de Vissenmaker en daarna in de controle. In `shared/scheldwoorden.json` staan alleen hashes. Een woord toevoegen: `python tools/scheldwoorden.py voeg-toe <woord>`. Testen: `python tools/scheldwoorden.py test "<naam>"`. De leesbare lijsten staan buiten de repo, in `scheldwoorden-bron`.
- Komen er veel vissen tegelijk binnen, dan wordt elke PR apart gecontroleerd en samengevoegd. Een vis die niet automatisch is samengevoegd, staat gewoon open: kijk bij **Actions > opendag-automerge** waarom.
- Loop aan het eind van de dag even door de nieuwe vissen. Draai zo nodig een merge terug.
