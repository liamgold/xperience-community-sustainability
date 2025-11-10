import { co2, hosting } from "@tgwf/co2";

// --- SWDM v4 setup + report ---
export async function reportEmissions() {
  try {
    await scrollPage();
    await delay(2000); // give lazy-loaded resources time to load

    const emissionsData = await getEmissionsData();

    const emissionsDiv = document.createElement("div");
    emissionsDiv.setAttribute("data-testid", "sustainabilityData");
    emissionsDiv.textContent = JSON.stringify(emissionsData);

    document.body.appendChild(emissionsDiv);
  } catch (error) {
    console.error("Error in reportEmissions:", error);

    // Create div with error info so backend doesn't timeout
    const errorDiv = document.createElement("div");
    errorDiv.setAttribute("data-testid", "sustainabilityData");
    errorDiv.textContent = JSON.stringify({
      error: error.message,
      stack: error.stack
    });
    document.body.appendChild(errorDiv);

    throw error;
  }
}

async function scrollPage() {
  window.scrollTo({ top: document.body.scrollHeight, behavior: "smooth" });
  return delay(2000);
}

function delay(ms) {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

async function getEmissionsData() {
  const resources = getResources();
  const bytesSent = getTransferSize(resources);

  // Try calling hosting as either a function or object method
  let hostCheck = null;
  let greenHostingStatus = "Unknown";

  try {
    // v0.16 might export hosting differently when bundled
    if (typeof hosting === "function") {
      hostCheck = await hosting(window.location.hostname);
    } else if (typeof hosting?.check === "function") {
      hostCheck = await hosting.check(window.location.hostname);
    }

    // Determine status based on result
    if (hostCheck !== null) {
      const isGreen = typeof hostCheck === "boolean" ? hostCheck : Boolean(hostCheck?.green);
      greenHostingStatus = isGreen ? "Green" : "NotGreen";
    }
  } catch (error) {
    console.error("Error checking green hosting:", error);
    // Keep status as "Unknown"
  }

  // For emissions calculation, use false if unknown (conservative estimate)
  const isGreenHost = greenHostingStatus === "Green";

  // ✅ Opt in to SWDM v4 with built-in rating system
  const co2Emission = new co2({ model: "swd", version: 4, rating: true });

  // ✅ v4 recommended method: perByteTrace (returns gCO2 per pageview with rating)
  const emissions = co2Emission.perByteTrace(bytesSent, isGreenHost);

  return {
    pageWeight: bytesSent,
    carbonRating: emissions.co2?.rating || "Unknown",
    greenHostingStatus: greenHostingStatus,
    emissions,
    resources
  };
}

function getResources() {
  const allResources = window.performance.getEntriesByType("resource");
  // Avoid counting our own checker script
  return allResources.filter((resource) => {
    const url = resource.name || "";
    return !url.split('?')[0].endsWith("resource-checker.js");
  });
}

function getTransferSize(resources) {
  return resources.reduce((total, entry) => total + (entry.transferSize || 0), 0);
}
