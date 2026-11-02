// Vissenmaker: maakt van de keuzes in het formulier een visbestand (JSON).
// Het ontwerpen en het voorbeeld werken zonder dit script; dit script maakt alleen het bestand.

const form = document.querySelector("#fish-form");
const output = document.querySelector("#fish-json");
const message = document.querySelector("#message");

// Maakt een visobject in precies het formaat van een bestand in fish/.
function makeFish() {
  const data = new FormData(form);
  return {
    name: data.get("name").trim(),
    maker: data.get("maker").trim(),
    body: data.get("body"),
    fins: data.get("fins"),
    pattern: data.get("pattern"),
    colors: {
      body: data.get("bodyColor"),
      fins: data.get("finsColor"),
      pattern: data.get("patternColor"),
    },
    size: Number(data.get("size")),
    speed: Number(data.get("speed")),
    behavior: data.get("behavior"),
  };
}

// Maakt een id van de naam: "Kleine Bubbel!" wordt "kleine-bubbel".
function makeId(name) {
  if(name.trim() === "" || /^[^a-zA-Z0-9]+$/.test(name)){
    alert("Je naam kan geen gebruik maken van speciale tekens en wordt daardoor ook verwijdert");
    return "";
  }
  return name
    .toLowerCase()
    .normalize("NFD")
    .replace(/[̀-ͯ]/g, "") // é wordt e
    .replace(/\s+/g, "-") // spaties worden -
    .replace(/[^a-z0-9-]/g, "") // overige tekens weg
    .replace(/-+/g, "-") // geen dubbele -
    .replace(/^-|-$/g, "") // geen - aan het begin of eind
    .slice(0, 30);
}

function showFish() {
  output.textContent = JSON.stringify(makeFish(), null, 2);
  message.textContent = "";
}

async function copyFish() {
  await navigator.clipboard.writeText(output.textContent);
}

function downloadFish() {
  const fish = makeFish();
  const id = makeId(fish.name);
  const file = new Blob([output.textContent + "\n"], { type: "application/json" });
  const link = document.createElement("a");
  link.href = URL.createObjectURL(file);
  link.download = id + ".json";
  if (fish.name === "" || fish.maker === "" || id === "") {
    message.textContent = "Vul eerst de naam van je vis en je eigen naam in.";
    return;
  }else {
    link.click();
  }
  URL.revokeObjectURL(link.href);
  message.textContent = `Gedownload als ${id}.json. Zet dit bestand in de map fish/.`;
}

form.addEventListener("input", showFish);
document.querySelector("#copy-button").addEventListener("click", copyFish);
document.querySelector("#download-button").addEventListener("click", downloadFish);
showFish();
