export const ratingDescriptions: Record<string, string> = {
  "A+": "Extremely efficient",
  A: "Very efficient",
  B: "Efficient",
  C: "Moderate efficiency",
  D: "Low efficiency",
  E: "Poor efficiency",
  F: "Very poor efficiency",
};

export const ratingColors: Record<string, { primary: string; bg: string; border: string }> = {
  "A+": { primary: "#059669", bg: "#d1fae5", border: "#6ee7b7" },
  A: { primary: "#16a34a", bg: "#dcfce7", border: "#86efac" },
  B: { primary: "#65a30d", bg: "#ecfccb", border: "#bef264" },
  C: { primary: "#ca8a04", bg: "#fef9c3", border: "#fde047" },
  D: { primary: "#ea580c", bg: "#ffedd5", border: "#fdba74" },
  E: { primary: "#dc2626", bg: "#fee2e2", border: "#fca5a5" },
  F: { primary: "#b91c1c", bg: "#fee2e2", border: "#f87171" },
};

export const getResourceTypeIcon = (resourceType: string): string => {
  const typeMap: Record<string, string> = {
    Images: "xp-picture",
    Scripts: "xp-braces",
    CSS: "xp-brush",
    Links: "xp-chain",
    Other: "xp-file",
  };
  return typeMap[resourceType] || "xp-file";
};

export const getResourceTypeColor = (resourceType: string): { bg: string; color: string; border: string } => {
  const colorMap: Record<string, { bg: string; color: string; border: string }> = {
    Images: { bg: "#dbeafe", color: "#1e40af", border: "#bfdbfe" }, // Blue
    Scripts: { bg: "#ede9fe", color: "#7c3aed", border: "#ddd6fe" }, // Purple
    CSS: { bg: "#fce7f3", color: "#db2777", border: "#fbcfe8" }, // Pink
    Links: { bg: "#d1fae5", color: "#059669", border: "#a7f3d0" }, // Green
    Other: { bg: "#f3f4f6", color: "#6b7280", border: "#e5e7eb" }, // Gray
  };
  return colorMap[resourceType] || colorMap.Other;
};

export const getHostingStatusDisplay = (status?: string): {
  text: string;
  color: string;
  icon: string;
  bgColor: string;
  borderColor: string;
  description: string;
} => {
  switch (status) {
    case "Green":
      return {
        text: "Green hosting",
        color: "#059669",
        icon: "●",
        bgColor: "#f0fdf4",
        borderColor: "#86efac",
        description: "This site is hosted on a green energy provider"
      };
    case "NotGreen":
      return {
        text: "Standard hosting",
        color: "#f97316",
        icon: "●",
        bgColor: "#fff7ed",
        borderColor: "#fdba74",
        description: "This site uses standard grid energy hosting"
      };
    case "Unknown":
    default:
      return {
        text: "Unknown hosting",
        color: "#6b7280",
        icon: "●",
        bgColor: "#f3f4f6",
        borderColor: "#d1d5db",
        description: "Unable to verify the hosting provider's energy source"
      };
  }
};
