import React from "react";
import { SustainabilityData } from "../types";

// Maximum number of reports to display in trend charts (matches pagination limit)
const MAX_TREND_REPORTS = 10;

interface TrendChartProps {
  currentReport: SustainabilityData;
  historicalReports: SustainabilityData[];
}

// Single chart component for one metric
const SingleTrendChart = ({
  title,
  data,
  color,
  unit,
  formatValue,
}: {
  title: string;
  data: number[];
  color: string;
  unit: string;
  formatValue: (val: number) => string;
}) => {
  const width = 550;
  const height = 200;
  const padding = { top: 30, right: 20, bottom: 30, left: 50 };
  const chartWidth = width - padding.left - padding.right;
  const chartHeight = height - padding.top - padding.bottom;

  const maxValue = Math.max(...data);
  const minValue = Math.min(...data);
  const range = maxValue - minValue || 1;

  // Add 10% padding to the range for visual breathing room
  const paddedMax = maxValue + range * 0.1;
  const paddedMin = Math.max(0, minValue - range * 0.1);
  const paddedRange = paddedMax - paddedMin;

  const scaleX = (index: number) =>
    padding.left + (index / (data.length - 1)) * chartWidth;

  const scaleY = (value: number) =>
    padding.top + chartHeight - ((value - paddedMin) / paddedRange) * chartHeight;

  const path = data
    .map((value, i) => {
      const x = scaleX(i);
      const y = scaleY(value);
      return i === 0 ? `M ${x} ${y}` : `L ${x} ${y}`;
    })
    .join(" ");

  return (
    <div
      style={{
        flex: 1,
        background: "white",
        border: "1px solid #e5e7eb",
        borderRadius: "8px",
        padding: "16px",
      }}
    >
      <div
        style={{
          fontSize: "14px",
          fontWeight: 600,
          color: "#111827",
          marginBottom: "12px",
          textAlign: "center",
        }}
      >
        {title}
      </div>
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

        {/* Trend line */}
        <path
          d={path}
          fill="none"
          stroke={color}
          strokeWidth="2.5"
          strokeLinecap="round"
          strokeLinejoin="round"
        />

        {/* Data points */}
        {data.map((value, i) => (
          <circle
            key={i}
            cx={scaleX(i)}
            cy={scaleY(value)}
            r="4"
            fill={color}
            stroke="white"
            strokeWidth="2"
          />
        ))}

        {/* Y-axis labels */}
        <text
          x={padding.left - 8}
          y={padding.top - 5}
          textAnchor="end"
          fontSize="11"
          fill="#6b7280"
        >
          {formatValue(paddedMax)}
        </text>
        <text
          x={padding.left - 8}
          y={height - padding.bottom + 5}
          textAnchor="end"
          fontSize="11"
          fill="#6b7280"
        >
          {formatValue(paddedMin)}
        </text>

        {/* Axes */}
        <line
          x1={padding.left}
          y1={height - padding.bottom}
          x2={width - padding.right}
          y2={height - padding.bottom}
          stroke="#d1d5db"
          strokeWidth="2"
        />
        <line
          x1={padding.left}
          y1={padding.top}
          x2={padding.left}
          y2={height - padding.bottom}
          stroke="#d1d5db"
          strokeWidth="2"
        />

        {/* Y-axis label */}
        <text
          x={padding.left - 35}
          y={height / 2}
          textAnchor="middle"
          fontSize="11"
          fill="#6b7280"
          transform={`rotate(-90, ${padding.left - 35}, ${height / 2})`}
        >
          {unit}
        </text>
      </svg>
    </div>
  );
};

export const TrendChart = ({ currentReport, historicalReports }: TrendChartProps) => {
  // Combine current and historical reports, limiting to last MAX_TREND_REPORTS
  const allReports = [currentReport, ...historicalReports].slice(0, MAX_TREND_REPORTS).reverse();

  if (allReports.length < 2) {
    return (
      <div
        style={{
          background: "white",
          border: "1px solid #e5e7eb",
          borderRadius: "8px",
          padding: "24px",
          textAlign: "center",
          color: "#6b7280",
          marginBottom: "20px",
        }}
      >
        Trend analysis requires at least 2 data points for a trend chart.
      </div>
    );
  }

  const emissions = allReports.map((r) => r.totalEmissions);
  const sizes = allReports.map((r) => r.totalSize / 1024); // Convert to MB

  return (
    <div style={{ marginBottom: "20px" }}>
      <div
        style={{
          fontSize: "15px",
          fontWeight: 600,
          color: "#111827",
          marginBottom: "8px",
        }}
      >
        Trend Analysis
      </div>
      <div
        style={{
          fontSize: "13px",
          color: "#6b7280",
          marginBottom: "16px",
        }}
      >
        Showing trends for the last {allReports.length} report{allReports.length !== 1 ? "s" : ""} (oldest to newest)
      </div>
      <div style={{ display: "flex", gap: "20px", justifyContent: "space-between", flexWrap: "wrap" }}>
        <SingleTrendChart
          title="CO₂ Emissions"
          data={emissions}
          color="#dc2626"
          unit="Grams (g)"
          formatValue={(val) => val.toFixed(3)}
        />
        <SingleTrendChart
          title="Page Weight"
          data={sizes}
          color="#2563eb"
          unit="Megabytes (MB)"
          formatValue={(val) => val.toFixed(2)}
        />
      </div>
    </div>
  );
};
