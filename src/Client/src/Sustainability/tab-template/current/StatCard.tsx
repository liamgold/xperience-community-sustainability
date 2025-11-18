import React from "react";

interface StatCardProps {
  label: string;
  value: string;
  subtitle?: string;
}

export const StatCard = ({ label, value, subtitle }: StatCardProps) => (
  <div
    style={{
      padding: "20px",
      background: "white",
      border: "1px solid #e5e7eb",
      borderRadius: "8px",
      boxShadow: "0 1px 2px 0 rgba(0, 0, 0, 0.05)",
    }}
  >
    <div
      style={{
        fontSize: "13px",
        fontWeight: 600,
        color: "#6b7280",
        textTransform: "uppercase",
        letterSpacing: "0.5px",
        marginBottom: "8px",
      }}
    >
      {label}
    </div>
    <div
      style={{
        fontSize: "28px",
        fontWeight: 700,
        color: "#111827",
        marginBottom: subtitle ? "4px" : "0",
      }}
    >
      {value}
    </div>
    {subtitle && (
      <div style={{ fontSize: "12px", color: "#9ca3af" }}>{subtitle}</div>
    )}
  </div>
);
