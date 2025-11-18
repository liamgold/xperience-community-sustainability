import React from "react";
import { SustainabilityData } from "../types";

interface TrendChartProps {
  currentReport: SustainabilityData;
  historicalReports: SustainabilityData[];
}

export const TrendChart = ({ currentReport, historicalReports }: TrendChartProps) => {
  // Combine current and historical reports, limiting to last 10
  const allReports = [currentReport, ...historicalReports].slice(0, 10).reverse();

  if (allReports.length < 2) {
    return null; // Need at least 2 data points for a trend
  }

  const width = 800;
  const height = 200;
  const padding = { top: 20, right: 60, bottom: 40, left: 60 };
  const chartWidth = width - padding.left - padding.right;
  const chartHeight = height - padding.top - padding.bottom;

  // Get data ranges
  const emissions = allReports.map((r) => r.totalEmissions);
  const sizes = allReports.map((r) => r.totalSize / 1024); // Convert to MB

  const maxEmissions = Math.max(...emissions);
  const minEmissions = Math.min(...emissions);
  const maxSize = Math.max(...sizes);
  const minSize = Math.min(...sizes);

  // Create scale functions
  const scaleX = (index: number) =>
    padding.left + (index / (allReports.length - 1)) * chartWidth;

  const scaleEmissions = (value: number) =>
    padding.top +
    chartHeight -
    ((value - minEmissions) / (maxEmissions - minEmissions || 1)) * chartHeight;

  const scaleSize = (value: number) =>
    padding.top +
    chartHeight -
    ((value - minSize) / (maxSize - minSize || 1)) * chartHeight;

  // Create path data for emissions line
  const emissionsPath = allReports
    .map((r, i) => {
      const x = scaleX(i);
      const y = scaleEmissions(r.totalEmissions);
      return i === 0 ? `M ${x} ${y}` : `L ${x} ${y}`;
    })
    .join(" ");

  // Create path data for size line
  const sizePath = allReports
    .map((r, i) => {
      const x = scaleX(i);
      const y = scaleSize(r.totalSize / 1024);
      return i === 0 ? `M ${x} ${y}` : `L ${x} ${y}`;
    })
    .join(" ");

  return (
    <div
      style={{
        background: "white",
        border: "1px solid #e5e7eb",
        borderRadius: "8px",
        padding: "20px",
        marginBottom: "16px",
      }}
    >
      <div
        style={{
          fontSize: "15px",
          fontWeight: 600,
          color: "#111827",
          marginBottom: "16px",
        }}
      >
        Trend Analysis
      </div>
      <div style={{ display: "flex", justifyContent: "center" }}>
        <svg width={width} height={height} style={{ overflow: "visible" }}>
          {/* Grid lines */}
          {[0, 1, 2, 3, 4].map((i) => {
            const y = padding.top + (chartHeight / 4) * i;
            return (
              <line
                key={i}
                x1={padding.left}
                y1={y}
                x2={width - padding.right}
                y2={y}
                stroke="#f3f4f6"
                strokeWidth="1"
              />
            );
          })}

          {/* Emissions line */}
          <path
            d={emissionsPath}
            fill="none"
            stroke="#dc2626"
            strokeWidth="3"
            strokeLinecap="round"
            strokeLinejoin="round"
          />

          {/* Size line */}
          <path
            d={sizePath}
            fill="none"
            stroke="#2563eb"
            strokeWidth="3"
            strokeLinecap="round"
            strokeLinejoin="round"
          />

          {/* Data points for emissions */}
          {allReports.map((r, i) => (
            <g key={`emissions-${i}`}>
              <circle
                cx={scaleX(i)}
                cy={scaleEmissions(r.totalEmissions)}
                r="4"
                fill="#dc2626"
                stroke="white"
                strokeWidth="2"
              />
            </g>
          ))}

          {/* Data points for size */}
          {allReports.map((r, i) => (
            <g key={`size-${i}`}>
              <circle
                cx={scaleX(i)}
                cy={scaleSize(r.totalSize / 1024)}
                r="4"
                fill="#2563eb"
                stroke="white"
                strokeWidth="2"
              />
            </g>
          ))}

          {/* Y-axis labels */}
          <text
            x={padding.left - 10}
            y={padding.top - 5}
            textAnchor="end"
            fontSize="11"
            fill="#6b7280"
          >
            {maxEmissions.toFixed(3)}g
          </text>
          <text
            x={padding.left - 10}
            y={height - padding.bottom + 5}
            textAnchor="end"
            fontSize="11"
            fill="#6b7280"
          >
            {minEmissions.toFixed(3)}g
          </text>

          {/* Right Y-axis labels for size */}
          <text
            x={width - padding.right + 10}
            y={padding.top - 5}
            textAnchor="start"
            fontSize="11"
            fill="#6b7280"
          >
            {maxSize.toFixed(2)}MB
          </text>
          <text
            x={width - padding.right + 10}
            y={height - padding.bottom + 5}
            textAnchor="start"
            fontSize="11"
            fill="#6b7280"
          >
            {minSize.toFixed(2)}MB
          </text>

          {/* X-axis */}
          <line
            x1={padding.left}
            y1={height - padding.bottom}
            x2={width - padding.right}
            y2={height - padding.bottom}
            stroke="#d1d5db"
            strokeWidth="2"
          />

          {/* Y-axes */}
          <line
            x1={padding.left}
            y1={padding.top}
            x2={padding.left}
            y2={height - padding.bottom}
            stroke="#d1d5db"
            strokeWidth="2"
          />
          <line
            x1={width - padding.right}
            y1={padding.top}
            x2={width - padding.right}
            y2={height - padding.bottom}
            stroke="#d1d5db"
            strokeWidth="2"
          />
        </svg>
      </div>

      {/* Legend */}
      <div
        style={{
          display: "flex",
          justifyContent: "center",
          gap: "24px",
          marginTop: "16px",
        }}
      >
        <div style={{ display: "flex", alignItems: "center", gap: "8px" }}>
          <div
            style={{
              width: "20px",
              height: "3px",
              background: "#dc2626",
              borderRadius: "2px",
            }}
          />
          <span style={{ fontSize: "13px", color: "#6b7280" }}>
            CO₂ Emissions (g)
          </span>
        </div>
        <div style={{ display: "flex", alignItems: "center", gap: "8px" }}>
          <div
            style={{
              width: "20px",
              height: "3px",
              background: "#2563eb",
              borderRadius: "2px",
            }}
          />
          <span style={{ fontSize: "13px", color: "#6b7280" }}>
            Page Weight (MB)
          </span>
        </div>
      </div>
    </div>
  );
};
