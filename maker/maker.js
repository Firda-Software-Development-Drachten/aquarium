// Vissenmaker: maakt van de keuzes in het formulier een visbestand (JSON).
// Het ontwerpen en het voorbeeld werken zonder dit script; dit script maakt alleen het bestand.

const form = document.querySelector("#fish-form");
const output = document.querySelector("#fish-json");
const message = document.querySelector("#message");
const maker = document.querySelector(".maker");

// Zet tint (0-360), verzadiging (0-100) en helderheid (0-100) om naar een hex-kleur.
function hsvToHex(hue, saturation, value) {
  const s = saturation / 100;
  const v = value / 100;
  const channel = (n) => {
    const k = (n + hue / 60) % 6;
    const c = v - v * s * Math.max(0, Math.min(k, 4 - k, 1));
    return Math.round(c * 255).toString(16).padStart(2, "0");
  };
  return "#" + channel(5) + channel(3) + channel(1);
}

// Haalt de kleur van een onderdeel op ("body", "fins" of "pattern") als hex.
function getColor(part) {
  const hue = Number(form.elements[part + "Hue"].value);
  const sat = Number(form.elements[part + "Sat"].value);
  const value = Number(form.elements[part + "Value"].value);
  return hsvToHex(hue, sat, value);
}

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
      body: getColor("body"),
      fins: getColor("fins"),
      pattern: getColor("pattern"),
    },
    size: Number(data.get("size")),
    speed: Number(data.get("speed")),
    behavior: data.get("behavior"),
  };
}

// Maakt een id van de naam: "Kleine Bubbel!" wordt "kleine-bubbel".
function makeId(name) {
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

// Zet de kleuren op het voorbeeld en op de sliders (bolletje en balk).
function syncColors() {
  for (const part of ["body", "fins", "pattern"]) {
    const hue = Number(form.elements[part + "Hue"].value);
    const sat = Number(form.elements[part + "Sat"].value);
    const value = Number(form.elements[part + "Value"].value);
    const sliders = [
      form.elements[part + "Hue"],
      form.elements[part + "Sat"],
      form.elements[part + "Value"],
    ];
    for (const slider of sliders) {
      slider.style.setProperty("--now", hsvToHex(hue, sat, value));
      slider.style.setProperty("--sat-from", hsvToHex(hue, 0, value));
      slider.style.setProperty("--sat-to", hsvToHex(hue, 100, value));
      slider.style.setProperty("--val-full", hsvToHex(hue, sat, 100));
    }
    maker.style.setProperty("--" + part + "-color", hsvToHex(hue, sat, value));
  }
}

function showFish() {
  syncColors();
  output.textContent = JSON.stringify(makeFish(), null, 2);
  message.textContent = "";
}

async function copyFish() {
  await navigator.clipboard.writeText(output.textContent);
}

function downloadFish() {
  const fish = makeFish();
  const id = makeId(fish.name);
  if (fish.name === "" || fish.maker === "") {
    message.textContent = "Vul eerst de naam van je vis en je eigen naam in.";
    return;
  }
  const file = new Blob([output.textContent + "\n"], { type: "application/json" });
  const link = document.createElement("a");
  link.href = URL.createObjectURL(file);
  link.download = id + ".json";
  link.click();
  URL.revokeObjectURL(link.href);
  message.textContent = `Gedownload als ${id}.json. Zet dit bestand in de map fish/.`;
}

form.addEventListener("input", showFish);
document.querySelector("#copy-button").addEventListener("click", copyFish);
document.querySelector("#download-button").addEventListener("click", downloadFish);
showFish();