import React from "react";
import { Icon, Stack, Spacing } from "@kentico/xperience-admin-components";
import { SustainabilityData } from "../types";
import { ratingColors, getResourceTypeIcon, getResourceTypeColor } from "../../utils";

interface HistoricalReportCardProps {
  report: SustainabilityData;
  isExpanded: boolean;
  onToggle: () => void;
}

export const HistoricalReportCard = ({ report, isExpanded, onToggle }: HistoricalReportCardProps) => {
  const ratingColor = ratingColors[report.carbonRating] || ratingColors.C;
  const totalResources = report.resourceGroups.reduce(
    (sum, group) => sum + group.resources.length,
    0
  );

  return (
    <div
      style={{
        background: "white",
        border: "1px solid #e5e7eb",
        borderRadius: "8px",
        overflow: "hidden",
      }}
    >
      {/* Compact Header */}
      <div
        style={{
          padding: "16px 20px",
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
          cursor: "pointer",
          background: "#fafafa",
          borderBottom: isExpanded ? "1px solid #e5e7eb" : "none",
        }}
        onClick={onToggle}
      >
        <div style={{ display: "flex", alignItems: "center", gap: "16px" }}>
          {/* Date */}
          <div>
            <div
              style={{
                fontSize: "13px",
                fontWeight: 600,
                color: "#111827",
                marginBottom: "2px",
              }}
            >
              {report.lastRunDate}
            </div>
            <div style={{ fontSize: "12px", color: "#9ca3af" }}>
              {totalResources} resources • {report.totalSize.toFixed(2)} KB
            </div>
          </div>

          {/* Metrics */}
          <div style={{ display: "flex", gap: "12px", alignItems: "center" }}>
            {/* Carbon Rating Badge */}
            <div
              style={{
                padding: "6px 12px",
                background: ratingColor.bg,
                color: ratingColor.primary,
                fontSize: "16px",
                fontWeight: 700,
                borderRadius: "6px",
                border: `1px solid ${ratingColor.border}`,
              }}
            >
              {report.carbonRating}
            </div>

            {/* Emissions */}
            <div style={{ textAlign: "right" }}>
              <div
                style={{
                  fontSize: "12px",
                  color: "#6b7280",
                  textTransform: "uppercase",
                  letterSpacing: "0.5px",
                }}
              >
                CO₂
              </div>
              <div
                style={{
                  fontSize: "14px",
                  fontWeight: 600,
                  color: "#111827",
                }}
              >
                {report.totalEmissions.toFixed(3)}g
              </div>
            </div>

            {/* Page Weight */}
            <div style={{ textAlign: "right" }}>
              <div
                style={{
                  fontSize: "12px",
                  color: "#6b7280",
                  textTransform: "uppercase",
                  letterSpacing: "0.5px",
                }}
              >
                Weight
              </div>
              <div
                style={{
                  fontSize: "14px",
                  fontWeight: 600,
                  color: "#111827",
                }}
              >
                {(report.totalSize / 1024).toFixed(2)}MB
              </div>
            </div>
          </div>
        </div>

        {/* Expand Icon */}
        <Icon
          name={isExpanded ? "xp-chevron-up" : "xp-chevron-down"}
        />
      </div>

      {/* Expanded Content */}
      {isExpanded && (
        <div style={{ padding: "20px" }}>
          <Stack spacing={Spacing.M}>
            {/* Resource Groups */}
            {report.resourceGroups
              .sort((a, b) => b.totalSize - a.totalSize)
              .slice(0, 3)
              .map((group, idx) => (
                <div
                  key={idx}
                  style={{
                    display: "flex",
                    justifyContent: "space-between",
                    alignItems: "center",
                    padding: "12px",
                    background: "#f9fafb",
                    borderRadius: "6px",
                  }}
                >
                  <div style={{ display: "flex", alignItems: "center", gap: "8px" }}>
                    <div
                      style={{
                        width: "32px",
                        height: "32px",
                        borderRadius: "6px",
                        backgroundColor: getResourceTypeColor(group.name).bg,
                        color: getResourceTypeColor(group.name).color,
                        border: `1px solid ${getResourceTypeColor(group.name).border}`,
                        display: "flex",
                        alignItems: "center",
                        justifyContent: "center",
                        fontSize: "16px",
                      }}
                    >
                      <Icon name={getResourceTypeIcon(group.name)} />
                    </div>
                    <div>
                      <div
                        style={{
                          fontSize: "13px",
                          fontWeight: 600,
                          color: "#111827",
                        }}
                      >
                        {group.name}
                      </div>
                      <div style={{ fontSize: "12px", color: "#6b7280" }}>
                        {group.resources.length} resource{group.resources.length !== 1 ? "s" : ""}
                      </div>
                    </div>
                  </div>
                  <div
                    style={{
                      fontSize: "13px",
                      fontWeight: 600,
                      color: "#6b7280",
                    }}
                  >
                    {group.totalSize.toFixed(2)} KB
                  </div>
                </div>
              ))}
          </Stack>
        </div>
      )}
    </div>
  );
};
