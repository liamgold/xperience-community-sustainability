import React, { useState, useMemo } from "react";
import {
  Stack,
  Headline,
  HeadlineSize,
  Row,
  Column,
  Spacing,
  Card,
  Button,
  ButtonVariety,
} from "@kentico/xperience-admin-components";
import { ratingDescriptions } from "../utils";

// Dashboard data types
interface DashboardPageItem {
  webPageItemID: number;
  pageName: string;
  pageUrl: string;
  languageName: string;
  carbonRating?: string;
  totalEmissions: number;
  totalSize: number;
  greenHostingStatus?: string;
  lastRunDate: string;
}

interface DashboardSummary {
  totalPages: number;
  averageEmissions: number;
  averagePageWeight: number;
  ratingDistribution: Record<string, number>;
  greenHostingCount: number;
}

interface DashboardResponse {
  summary: DashboardSummary;
  pages: DashboardPageItem[];
}

interface SustainabilityDashboardProps {
  readonly dashboardData?: DashboardResponse;
}

type SortField = "pageName" | "carbonRating" | "totalEmissions" | "totalSize" | "lastRunDate";
type SortDirection = "asc" | "desc";

export const SustainabilityDashboardTemplate = ({
  dashboardData,
}: SustainabilityDashboardProps) => {
  const [sortField, setSortField] = useState<SortField>("pageName");
  const [sortDirection, setSortDirection] = useState<SortDirection>("asc");

  const handleSort = (field: SortField) => {
    if (sortField === field) {
      setSortDirection(sortDirection === "asc" ? "desc" : "asc");
    } else {
      setSortField(field);
      setSortDirection("asc");
    }
  };

  const sortedPages = useMemo(() => {
    if (!dashboardData?.pages) return [];

    return [...dashboardData.pages].sort((a, b) => {
      let aValue: any = a[sortField];
      let bValue: any = b[sortField];

      // Handle carbon rating sorting (A+ > A > B > C > D > E > F)
      if (sortField === "carbonRating") {
        const ratingOrder = ["A+", "A", "B", "C", "D", "E", "F"];
        aValue = ratingOrder.indexOf(a.carbonRating || "F");
        bValue = ratingOrder.indexOf(b.carbonRating || "F");
      }

      if (aValue < bValue) return sortDirection === "asc" ? -1 : 1;
      if (aValue > bValue) return sortDirection === "asc" ? 1 : -1;
      return 0;
    });
  }, [dashboardData?.pages, sortField, sortDirection]);

  if (!dashboardData || dashboardData.summary.totalPages === 0) {
    return (
      <Stack spacing={Spacing.XL}>
        <Headline size={HeadlineSize.L}>Sustainability Dashboard</Headline>
        <Card>
          <div style={{ padding: "40px", textAlign: "center", color: "#6b7280" }}>
            <p>No sustainability reports found.</p>
            <p style={{ marginTop: "8px", fontSize: "14px" }}>
              Run sustainability reports on individual pages to see them here.
            </p>
          </div>
        </Card>
      </Stack>
    );
  }

  const { summary } = dashboardData;

  return (
    <Stack spacing={Spacing.XL}>
      <Headline size={HeadlineSize.L}>Sustainability Dashboard</Headline>

      {/* Summary Cards */}
      <Row spacing={Spacing.L}>
        <Column colsLg={3} colsMd={6}>
          <Card>
            <div style={{ padding: "20px" }}>
              <div style={{ fontSize: "14px", color: "#6b7280", marginBottom: "8px" }}>
                Total Pages Analyzed
              </div>
              <div style={{ fontSize: "32px", fontWeight: 700, color: "#111827" }}>
                {summary.totalPages}
              </div>
            </div>
          </Card>
        </Column>
        <Column colsLg={3} colsMd={6}>
          <Card>
            <div style={{ padding: "20px" }}>
              <div style={{ fontSize: "14px", color: "#6b7280", marginBottom: "8px" }}>
                Average Emissions
              </div>
              <div style={{ fontSize: "32px", fontWeight: 700, color: "#111827" }}>
                {summary.averageEmissions.toFixed(3)}g
              </div>
              <div style={{ fontSize: "12px", color: "#9ca3af", marginTop: "4px" }}>
                CO₂ per page view
              </div>
            </div>
          </Card>
        </Column>
        <Column colsLg={3} colsMd={6}>
          <Card>
            <div style={{ padding: "20px" }}>
              <div style={{ fontSize: "14px", color: "#6b7280", marginBottom: "8px" }}>
                Average Page Weight
              </div>
              <div style={{ fontSize: "32px", fontWeight: 700, color: "#111827" }}>
                {summary.averagePageWeight.toFixed(0)} KB
              </div>
            </div>
          </Card>
        </Column>
        <Column colsLg={3} colsMd={6}>
          <Card>
            <div style={{ padding: "20px" }}>
              <div style={{ fontSize: "14px", color: "#6b7280", marginBottom: "8px" }}>
                Green Hosting
              </div>
              <div style={{ fontSize: "32px", fontWeight: 700, color: "#10b981" }}>
                {summary.greenHostingCount}
              </div>
              <div style={{ fontSize: "12px", color: "#9ca3af", marginTop: "4px" }}>
                {((summary.greenHostingCount / summary.totalPages) * 100).toFixed(0)}% of pages
              </div>
            </div>
          </Card>
        </Column>
      </Row>

      {/* Rating Distribution */}
      {Object.keys(summary.ratingDistribution).length > 0 && (
        <Card>
          <div style={{ padding: "24px" }}>
            <div style={{ fontSize: "16px", fontWeight: 600, marginBottom: "16px" }}>
              Rating Distribution
            </div>
            <Row spacing={Spacing.M}>
              {["A+", "A", "B", "C", "D", "E", "F"].map((rating) => {
                const count = summary.ratingDistribution[rating] || 0;
                const percentage = (count / summary.totalPages) * 100;
                return (
                  <Column key={rating} colsLg={1.714} colsMd={3}>
                    <div
                      style={{
                        textAlign: "center",
                        padding: "12px",
                        border: "1px solid #e5e7eb",
                        borderRadius: "8px",
                      }}
                    >
                      <div style={{ fontSize: "24px", fontWeight: 700 }}>{rating}</div>
                      <div style={{ fontSize: "18px", color: "#6b7280", marginTop: "4px" }}>
                        {count}
                      </div>
                      <div style={{ fontSize: "12px", color: "#9ca3af", marginTop: "2px" }}>
                        {percentage.toFixed(0)}%
                      </div>
                    </div>
                  </Column>
                );
              })}
            </Row>
          </div>
        </Card>
      )}

      {/* Pages Table */}
      <Card>
        <div style={{ padding: "24px" }}>
          <div style={{ fontSize: "16px", fontWeight: 600, marginBottom: "16px" }}>
            All Pages ({sortedPages.length})
          </div>
          <div style={{ overflowX: "auto" }}>
            <table style={{ width: "100%", borderCollapse: "collapse" }}>
              <thead>
                <tr style={{ borderBottom: "2px solid #e5e7eb" }}>
                  <th
                    onClick={() => handleSort("pageName")}
                    style={{
                      textAlign: "left",
                      padding: "12px",
                      fontSize: "14px",
                      fontWeight: 600,
                      cursor: "pointer",
                      userSelect: "none",
                    }}
                  >
                    Page Name {sortField === "pageName" && (sortDirection === "asc" ? "↑" : "↓")}
                  </th>
                  <th
                    onClick={() => handleSort("carbonRating")}
                    style={{
                      textAlign: "center",
                      padding: "12px",
                      fontSize: "14px",
                      fontWeight: 600,
                      cursor: "pointer",
                      userSelect: "none",
                    }}
                  >
                    Rating {sortField === "carbonRating" && (sortDirection === "asc" ? "↑" : "↓")}
                  </th>
                  <th
                    onClick={() => handleSort("totalEmissions")}
                    style={{
                      textAlign: "right",
                      padding: "12px",
                      fontSize: "14px",
                      fontWeight: 600,
                      cursor: "pointer",
                      userSelect: "none",
                    }}
                  >
                    CO₂ (g) {sortField === "totalEmissions" && (sortDirection === "asc" ? "↑" : "↓")}
                  </th>
                  <th
                    onClick={() => handleSort("totalSize")}
                    style={{
                      textAlign: "right",
                      padding: "12px",
                      fontSize: "14px",
                      fontWeight: 600,
                      cursor: "pointer",
                      userSelect: "none",
                    }}
                  >
                    Size (KB) {sortField === "totalSize" && (sortDirection === "asc" ? "↑" : "↓")}
                  </th>
                  <th
                    style={{
                      textAlign: "center",
                      padding: "12px",
                      fontSize: "14px",
                      fontWeight: 600,
                    }}
                  >
                    Green Hosting
                  </th>
                  <th
                    onClick={() => handleSort("lastRunDate")}
                    style={{
                      textAlign: "right",
                      padding: "12px",
                      fontSize: "14px",
                      fontWeight: 600,
                      cursor: "pointer",
                      userSelect: "none",
                    }}
                  >
                    Last Analyzed {sortField === "lastRunDate" && (sortDirection === "asc" ? "↑" : "↓")}
                  </th>
                </tr>
              </thead>
              <tbody>
                {sortedPages.map((page) => (
                  <tr
                    key={`${page.webPageItemID}-${page.languageName}`}
                    style={{ borderBottom: "1px solid #f3f4f6" }}
                  >
                    <td style={{ padding: "12px" }}>
                      <div style={{ fontWeight: 500 }}>{page.pageName}</div>
                      <div style={{ fontSize: "12px", color: "#6b7280", marginTop: "2px" }}>
                        {page.pageUrl}
                      </div>
                    </td>
                    <td style={{ textAlign: "center", padding: "12px" }}>
                      {page.carbonRating && (
                        <span
                          style={{
                            display: "inline-block",
                            padding: "4px 12px",
                            borderRadius: "4px",
                            fontSize: "14px",
                            fontWeight: 600,
                            backgroundColor:
                              page.carbonRating === "A+" || page.carbonRating === "A"
                                ? "#d1fae5"
                                : page.carbonRating === "B" || page.carbonRating === "C"
                                ? "#fef3c7"
                                : "#fee2e2",
                            color:
                              page.carbonRating === "A+" || page.carbonRating === "A"
                                ? "#065f46"
                                : page.carbonRating === "B" || page.carbonRating === "C"
                                ? "#92400e"
                                : "#991b1b",
                          }}
                        >
                          {page.carbonRating}
                        </span>
                      )}
                    </td>
                    <td style={{ textAlign: "right", padding: "12px" }}>
                      {page.totalEmissions.toFixed(3)}
                    </td>
                    <td style={{ textAlign: "right", padding: "12px" }}>
                      {page.totalSize.toFixed(0)}
                    </td>
                    <td style={{ textAlign: "center", padding: "12px" }}>
                      {page.greenHostingStatus === "Green" ? (
                        <span style={{ color: "#10b981" }}>✓</span>
                      ) : (
                        <span style={{ color: "#6b7280" }}>–</span>
                      )}
                    </td>
                    <td style={{ textAlign: "right", padding: "12px", fontSize: "14px", color: "#6b7280" }}>
                      {page.lastRunDate}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      </Card>
    </Stack>
  );
};
