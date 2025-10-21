import React, { useState } from "react";
import {
  Card,
  Button,
  ButtonColor,
  ButtonSize,
  Stack,
  Headline,
  HeadlineSize,
  Row,
  Column,
  Spacing,
  Divider,
  Icon,
} from "@kentico/xperience-admin-components";
import { usePageCommand } from "@kentico/xperience-admin-base";

enum PageAvailabilityStatus {
  Available,
  NotAvailable,
}

interface SustainabilityTabTemplateProps {
  pageAvailability: PageAvailabilityStatus;
  sustainabilityData: SustainabilityData;
}

type SustainabilityData = {
  resourceGroups: ExternalResourceGroup[];
  totalSize: number;
  totalEmissions: number;
  lastRunDate: string;
  carbonRating: string;
};

type ExternalResource = { url: string; size: number };

type ExternalResourceGroup = {
  type: string;
  name: string;
  totalSize: number;
  resources: ExternalResource[];
};

const ratingDescriptions: Record<string, string> = {
  "A+": "Extremely efficient",
  A: "Very efficient",
  B: "Efficient",
  C: "Moderate efficiency",
  D: "Low efficiency",
  E: "Poor efficiency",
  F: "Very poor efficiency",
};

const ratingColors: Record<string, { primary: string; bg: string; border: string }> = {
  "A+": { primary: "#059669", bg: "#d1fae5", border: "#6ee7b7" },
  A: { primary: "#16a34a", bg: "#dcfce7", border: "#86efac" },
  B: { primary: "#65a30d", bg: "#ecfccb", border: "#bef264" },
  C: { primary: "#ca8a04", bg: "#fef9c3", border: "#fde047" },
  D: { primary: "#ea580c", bg: "#ffedd5", border: "#fdba74" },
  E: { primary: "#dc2626", bg: "#fee2e2", border: "#fca5a5" },
  F: { primary: "#b91c1c", bg: "#fee2e2", border: "#f87171" },
};

const getResourceTypeIcon = (resourceType: string): string => {
  const typeMap: Record<string, string> = {
    Images: "xp-picture",
    Scripts: "xp-braces",
    CSS: "xp-brush",
    Links: "xp-chain",
    Other: "xp-file",
  };
  return typeMap[resourceType] || "xp-file";
};

const getResourceTypeColor = (resourceType: string): { bg: string; color: string; border: string } => {
  const colorMap: Record<string, { bg: string; color: string; border: string }> = {
    Images: { bg: "#dbeafe", color: "#1e40af", border: "#bfdbfe" }, // Blue
    Scripts: { bg: "#ede9fe", color: "#7c3aed", border: "#ddd6fe" }, // Purple
    CSS: { bg: "#fce7f3", color: "#db2777", border: "#fbcfe8" }, // Pink
    Links: { bg: "#d1fae5", color: "#059669", border: "#a7f3d0" }, // Green
    Other: { bg: "#f3f4f6", color: "#6b7280", border: "#e5e7eb" }, // Gray
  };
  return colorMap[resourceType] || colorMap.Other;
};

const Commands = {
  RunReport: "RunReport",
};

const StatCard = ({
  label,
  value,
  subtitle,
}: {
  label: string;
  value: string;
  subtitle?: string;
}) => (
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

// Style constants for ResourceGroupCard
const resourceGroupCardStyles = {
  container: {
    background: "white",
    border: "1px solid #e5e7eb",
    borderRadius: "8px",
    overflow: "hidden" as const,
  },
  header: {
    padding: "16px 20px",
    background: "#f9fafb",
    borderBottom: "1px solid #e5e7eb",
    display: "flex",
    justifyContent: "space-between" as const,
    alignItems: "center" as const,
  },
  title: {
    fontSize: "15px",
    fontWeight: 600,
    color: "#111827",
  },
  subtitle: {
    fontSize: "13px",
    color: "#6b7280",
    marginTop: "2px",
  },
  badge: {
    padding: "4px 12px",
    background: "#eff6ff",
    color: "#1e40af",
    fontSize: "13px",
    fontWeight: 600,
    borderRadius: "12px",
  },
  listContainer: {
    padding: "12px 20px",
  },
  resourceItem: (isLast: boolean) => ({
    padding: "12px 0",
    borderBottom: isLast ? "none" : "1px solid #f3f4f6",
  }),
  resourceRow: {
    display: "flex",
    justifyContent: "space-between" as const,
    alignItems: "flex-start" as const,
    gap: "12px",
  },
  resourceInfo: {
    flex: 1,
    minWidth: 0,
  },
  fileName: {
    fontSize: "13px",
    fontWeight: 500,
    color: "#111827",
    marginBottom: "2px",
    wordBreak: "break-word" as const,
  },
  filePath: {
    fontSize: "12px",
    color: "#9ca3af",
    overflow: "hidden",
    textOverflow: "ellipsis" as const,
    whiteSpace: "nowrap" as const,
  },
  fileSize: {
    fontSize: "13px",
    fontWeight: 600,
    color: "#6b7280",
    whiteSpace: "nowrap" as const,
  },
  expandButtonContainer: {
    marginTop: "12px",
    width: "100%",
    display: "flex" as const,
    justifyContent: "center" as const,
  },
};

const ResourceGroupCard = ({
  group,
  totalPageSize
}: {
  group: ExternalResourceGroup;
  totalPageSize: number;
}) => {
  const [expanded, setExpanded] = useState(false);
  const displayCount = expanded ? group.resources.length : 3;

  return (
    <div style={resourceGroupCardStyles.container}>
      <div style={resourceGroupCardStyles.header}>
        <div style={{ display: "flex", alignItems: "center", gap: "12px" }}>
          <div
            style={{
              display: "flex",
              alignItems: "center",
              justifyContent: "center",
              width: "48px",
              height: "48px",
              borderRadius: "8px",
              backgroundColor: getResourceTypeColor(group.name).bg,
              color: getResourceTypeColor(group.name).color,
              border: `1px solid ${getResourceTypeColor(group.name).border}`,
              flexShrink: 0,
              fontSize: "24px",
            }}
          >
            <Icon name={getResourceTypeIcon(group.name)} />
          </div>
          <div>
            <div style={resourceGroupCardStyles.title}>
              {group.name}
            </div>
            <div style={resourceGroupCardStyles.subtitle}>
              {group.resources.length} resource
              {group.resources.length !== 1 ? "s" : ""} •{" "}
              {group.totalSize.toFixed(2)} KB
            </div>
          </div>
        </div>
        <div style={resourceGroupCardStyles.badge}>
          {((group.totalSize / totalPageSize) * 100).toFixed(1)}% of page
        </div>
      </div>
      {group.resources.length > 0 && (
        <div style={resourceGroupCardStyles.listContainer}>
          {group.resources.slice(0, displayCount).map((resource, idx) => {
            const fileName = resource.url.split("/").pop() || resource.url;
            const path = resource.url.substring(
              0,
              resource.url.lastIndexOf("/") + 1
            );
            const isLast = idx === displayCount - 1;

            return (
              <div key={idx} style={resourceGroupCardStyles.resourceItem(isLast)}>
                <div style={resourceGroupCardStyles.resourceRow}>
                  <div style={resourceGroupCardStyles.resourceInfo}>
                    <div style={resourceGroupCardStyles.fileName}>
                      {fileName}
                    </div>
                    <div style={resourceGroupCardStyles.filePath} title={path}>
                      {path}
                    </div>
                  </div>
                  <div style={resourceGroupCardStyles.fileSize}>
                    {resource.size.toFixed(2)} KB
                  </div>
                </div>
              </div>
            );
          })}
          {group.resources.length > 3 && (
            <div style={resourceGroupCardStyles.expandButtonContainer}>
              <Button
                label={expanded ? "Show less" : `Show ${group.resources.length - 3} more`}
                onClick={() => setExpanded(!expanded)}
                color={ButtonColor.Secondary}
                size={ButtonSize.S}
              />
            </div>
          )}
        </div>
      )}
    </div>
  );
};

export const SustainabilityTabTemplate = (
  props: SustainabilityTabTemplateProps | null
) => {
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [data, setData] = useState<SustainabilityData | undefined | null>(
    props?.sustainabilityData
  );

  const { execute: submit } = usePageCommand<SustainabilityTabTemplateProps>(
    Commands.RunReport,
    {
      after: (response) => {
        setData(response?.sustainabilityData);
        setIsLoading(false);
        setError(null);
      },
      onError: (err) => {
        setIsLoading(false);
        setError("Failed to run sustainability report. Please try again.");
        console.error("Sustainability report error:", err);
      },
    }
  );

  if (data === undefined || data === null) {
    return (
      <div style={{ padding: "32px", maxWidth: "1400px", margin: "0 auto" }}>
        <Stack spacing={Spacing.XL}>
          <div
            style={{
              display: "flex",
              justifyContent: "space-between",
              alignItems: "center",
            }}
          >
            <Headline size={HeadlineSize.L}>Sustainability Report</Headline>
          </div>

          <div
            style={{
              display: "flex",
              alignItems: "center",
              justifyContent: "center",
              minHeight: "400px",
            }}
          >
            <Card headline="No Data Available" fullHeight={false}>
              <Stack spacing={Spacing.L}>
                <p style={{ fontSize: "14px", color: "#6b7280" }}>
                  {props?.pageAvailability === PageAvailabilityStatus.Available
                    ? "Run a sustainability analysis to see your page's environmental impact."
                    : "This page is not available for analysis."}
                </p>
                {error && (
                  <div
                    style={{
                      padding: "12px 16px",
                      background: "#fef2f2",
                      border: "1px solid #fecaca",
                      borderRadius: "6px",
                      fontSize: "13px",
                      color: "#dc2626",
                    }}
                  >
                    {error}
                  </div>
                )}
                {props?.pageAvailability ===
                  PageAvailabilityStatus.Available && (
                  <Button
                    label="Run Analysis"
                    color={ButtonColor.Primary}
                    size={ButtonSize.L}
                    disabled={isLoading}
                    inProgress={isLoading}
                    onClick={() => {
                      setIsLoading(true);
                      setError(null);
                      submit();
                    }}
                  />
                )}
              </Stack>
            </Card>
          </div>
        </Stack>
      </div>
    );
  }

  const ratingColor = ratingColors[data.carbonRating] || ratingColors.C;
  const totalResources = data.resourceGroups.reduce(
    (sum, group) => sum + group.resources.length,
    0
  );

  return (
    <div style={{ padding: "32px", maxWidth: "1400px", margin: "0 auto" }}>
      <Stack spacing={Spacing.XL}>
        {/* Header */}
        <div
          style={{
            display: "flex",
            justifyContent: "space-between",
            alignItems: "center",
            flexWrap: "wrap",
            gap: "16px",
          }}
        >
          <div>
            <Headline size={HeadlineSize.L}>Sustainability Report</Headline>
            <div
              style={{
                fontSize: "14px",
                color: "#6b7280",
                marginTop: "4px",
              }}
            >
              Last analyzed: {data.lastRunDate}
            </div>
          </div>
          <Button
            label="Run New Analysis"
            color={ButtonColor.Primary}
            size={ButtonSize.M}
            disabled={isLoading}
            inProgress={isLoading}
            onClick={() => {
              setIsLoading(true);
              setError(null);
              submit();
            }}
          />
        </div>

        {error && (
          <div
            style={{
              padding: "16px",
              background: "#fef2f2",
              border: "1px solid #fecaca",
              borderRadius: "8px",
              fontSize: "14px",
              color: "#dc2626",
            }}
          >
            {error}
          </div>
        )}

        {/* Hero Carbon Rating Card */}
        <div
          style={{
            background: `linear-gradient(135deg, ${ratingColor.bg} 0%, white 100%)`,
            border: `2px solid ${ratingColor.border}`,
            borderRadius: "12px",
            padding: "40px",
            position: "relative",
            overflow: "hidden",
          }}
        >
          <div
            style={{
              position: "absolute",
              top: "-50px",
              right: "-50px",
              width: "200px",
              height: "200px",
              background: ratingColor.bg,
              borderRadius: "50%",
              opacity: 0.3,
            }}
          />
          <Row spacing={Spacing.XL}>
            <Column colsLg={6} colsMd={12}>
              <div style={{ position: "relative", zIndex: 1 }}>
                <div
                  style={{
                    fontSize: "14px",
                    fontWeight: 600,
                    color: ratingColor.primary,
                    textTransform: "uppercase",
                    letterSpacing: "1px",
                    marginBottom: "12px",
                  }}
                >
                  Carbon Rating
                </div>
                <div
                  style={{
                    fontSize: "120px",
                    fontWeight: 900,
                    color: ratingColor.primary,
                    lineHeight: 1,
                    marginBottom: "16px",
                    textShadow: `0 2px 8px ${ratingColor.bg}`,
                  }}
                >
                  {data.carbonRating}
                </div>
                <div
                  style={{
                    fontSize: "18px",
                    fontWeight: 600,
                    color: "#111827",
                    marginBottom: "8px",
                  }}
                >
                  {ratingDescriptions[data.carbonRating]}
                </div>
                <div style={{ fontSize: "14px", color: "#6b7280" }}>
                  {data.carbonRating === "A+" || data.carbonRating === "A"
                    ? "This page has excellent carbon efficiency."
                    : data.carbonRating === "B" || data.carbonRating === "C"
                    ? "This page has room for improvement."
                    : "This page needs significant optimization."}
                </div>
              </div>
            </Column>
            <Column colsLg={6} colsMd={12}>
              <Row spacing={Spacing.L} spacingY={Spacing.L}>
                <Column colsLg={6} colsMd={6}>
                  <StatCard
                    label="CO₂ Emissions"
                    value={`${data.totalEmissions.toFixed(3)}g`}
                    subtitle="per page view"
                  />
                </Column>
                <Column colsLg={6} colsMd={6}>
                  <StatCard
                    label="Page Weight"
                    value={`${(data.totalSize / 1024).toFixed(2)}MB`}
                    subtitle={`${data.totalSize.toFixed(0)} KB total`}
                  />
                </Column>
                <Column colsLg={6} colsMd={6}>
                  <StatCard
                    label="Resources"
                    value={`${totalResources}`}
                    subtitle={`${data.resourceGroups.length} categories`}
                  />
                </Column>
                <Column colsLg={6} colsMd={6}>
                  <StatCard
                    label="Efficiency"
                    value={
                      data.totalEmissions < 0.1
                        ? "Excellent"
                        : data.totalEmissions < 0.2
                        ? "Good"
                        : data.totalEmissions < 0.3
                        ? "Fair"
                        : "Poor"
                    }
                    subtitle="Overall rating"
                  />
                </Column>
              </Row>
            </Column>
          </Row>
        </div>

        {/* Resource Breakdown */}
        <div>
          <Headline size={HeadlineSize.M} spacingBottom={Spacing.L}>
            Resource Breakdown
          </Headline>
          <Stack spacing={Spacing.L}>
            {data.resourceGroups
              .sort((a, b) => b.totalSize - a.totalSize)
              .map((group) => (
                <ResourceGroupCard key={group.type} group={group} totalPageSize={data.totalSize} />
              ))}
          </Stack>
        </div>

        {/* Tips Section */}
        <div
          style={{
            padding: "24px",
            background: "#f0f9ff",
            border: "1px solid #bae6fd",
            borderRadius: "8px",
          }}
        >
          <div
            style={{
              fontSize: "15px",
              fontWeight: 600,
              color: "#0c4a6e",
              marginBottom: "12px",
            }}
          >
            💡 Tips to improve your carbon footprint
          </div>
          <ul style={{ margin: 0, paddingLeft: "20px", color: "#075985" }}>
            <li style={{ marginBottom: "6px" }}>
              <strong>Use Image Variants</strong> - Configure responsive image variants
              with specific dimensions and aspect ratios to serve optimized versions
              for different contexts (hero banners, thumbnails, social media)
            </li>
            <li style={{ marginBottom: "6px" }}>
              <strong>Enable AIRA's Smart Optimization</strong> - Leverage AIRA's AI-powered
              features for automatic image format conversion, smart focal point detection,
              and automated quality optimization during uploads
            </li>
            <li style={{ marginBottom: "6px" }}>
              <strong>Automate Image Metadata</strong> - Use AIRA to automatically generate
              alt texts, descriptions, and tags for better SEO while reducing manual work
            </li>
            <li style={{ marginBottom: "6px" }}>
              <strong>Minimize CSS & JavaScript</strong> - Bundle and minify your assets to
              reduce file sizes and decrease the number of HTTP requests
            </li>
            <li style={{ marginBottom: "6px" }}>
              <strong>Enable Browser Caching</strong> - Configure cache headers for static
              resources to reduce repeat downloads and server load
            </li>
            <li>
              <strong>Implement Lazy Loading</strong> - Load images and resources only when
              they're needed, improving initial page load performance
            </li>
          </ul>
        </div>
      </Stack>
    </div>
  );
};
