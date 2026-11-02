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

// Zet een hex-kleur om naar tint (0-360), verzadiging (0-100) en helderheid (0-100).
function hexToHsv(hex) {
  const r = parseInt(hex.slice(1, 3), 16) / 255;
  const g = parseInt(hex.slice(3, 5), 16) / 255;
  const b = parseInt(hex.slice(5, 7), 16) / 255;
  const max = Math.max(r, g, b);
  const diff = max - Math.min(r, g, b);
  let hue = 0;
  if (diff !== 0) {
    if (max === r) hue = ((g - b) / diff) % 6;
    else if (max === g) hue = (b - r) / diff + 2;
    else hue = (r - g) / diff + 4;
  }
  return {
    hue: Math.round((hue * 60 + 360) % 360),
    sat: max === 0 ? 0 : Math.round((diff / max) * 100),
    value: Math.round(max * 100),
  };
}

// Zet de sliders van een onderdeel op een hex-kleur.
function setColor(part, hex) {
  const { hue, sat, value } = hexToHsv(hex);
  form.elements[part + "Hue"].value = hue;
  form.elements[part + "Sat"].value = sat;
  form.elements[part + "Value"].value = value;
}

// Options aanmaken
let options;

// Lijst met toegestane waarden uit options.json
async function loadOptions() {
  const response = await fetch("../shared/options.json")
  options = await response.json();
}

// Functie die controleert of waarde in opties staat
function isValidOption(key, value) {
  return options[key].some(option => option.id === value);
}

// Functie die de URL uitleest en waarden in formulier zet
function loadFromUrl() {
  const params = new URLSearchParams(window.location.search);
 
  for (const key of ["body", "fins", "pattern", "behavior"]) {
    const value = params.get(key);

    if (value && isValidOption(key, value)) {
      form.elements[key].value = value;
    }
  }

  // Kleuren (als hex in de URL, bijvoorbeeld bodyColor=#ff8800)
  for (const part of ["body", "fins", "pattern"]) {
    const value = params.get(part + "Color");

    if (value && /^#[0-9a-f]{6}$/i.test(value)) {
      setColor(part, value);
    }
  }

  // Grootte en snelheid
  for (const key of ["size", "speed"]) {
    const value = Number(params.get(key));

    if (options[key].includes(value)) {
      form.elements[key].value = value;
    }
  }
}

// Functie die URL bijwerkt
function updateUrl() {
  const params = new URLSearchParams(new FormData(form));
  // De sliders zelf niet in de URL, wel de kleur als hex.
  for (const part of ["body", "fins", "pattern"]) {
    params.delete(part + "Hue");
    params.delete(part + "Sat");
    params.delete(part + "Value");
    params.set(part + "Color", getColor(part));
  }
  params.set("size", form.elements.size.value);
  params.set("speed", form.elements.speed.value);
  history.replaceState(null, "", "?" + params.toString());
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
  // URL update functie
  updateUrl();
  message.textContent = "";

}

async function copyFish() {
  await navigator.clipboard.writeText(output.textContent);
}

function getRandomInt(inclMax) {

  return Math.round(Math.random() * inclMax);

}

async function randomFish() {

  const fieldsets = document.querySelectorAll("fieldset");

  fieldsets.forEach(fieldset => {

      const inputs = fieldset.querySelectorAll('input[type="radio"]');
      if (inputs.length != 0) {

          const randomInput = inputs[getRandomInt(inputs.length-1)];
          randomInput.checked = true;

      }

  });

  // Willekeurige kleuren met de sliders. Niet te donker, zodat je de vis goed ziet.
  for (const part of ["body", "fins", "pattern"]) {
    form.elements[part + "Hue"].value = getRandomInt(360);
    form.elements[part + "Sat"].value = 40 + getRandomInt(60);
    form.elements[part + "Value"].value = 50 + getRandomInt(50);
  }

  showFish();

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
document.querySelector("#random-button").addEventListener("click", randomFish);

// Eerst keuzes inladen
loadOptions().then(() => {
  loadFromUrl();
  showFish();
})
