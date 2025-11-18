import React from "react";
import {
  Stack,
  Headline,
  HeadlineSize,
  Row,
  Column,
  Spacing,
} from "@kentico/xperience-admin-components";
import { SustainabilityData } from "../types";
import { ratingDescriptions, getHostingStatusDisplay } from "../../utils";
import { StatCard } from "./StatCard";
import { ResourceGroupCard } from "./ResourceGroupCard";

interface CurrentReportViewProps {
  data: SustainabilityData;
  ratingColor: { primary: string; bg: string; border: string };
  totalResources: number;
  hostingStatus: ReturnType<typeof getHostingStatusDisplay>;
}

export const CurrentReportView = ({
  data,
  ratingColor,
  totalResources,
  hostingStatus,
}: CurrentReportViewProps) => (
  <>
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
            <div style={{
              fontSize: "12px",
              color: "#9ca3af",
              marginTop: "12px",
              paddingTop: "12px",
              borderTop: "1px solid #e5e7eb"
            }}>
              Rating based on{" "}
              <a
                href="https://sustainablewebdesign.org/digital-carbon-ratings/"
                target="_blank"
                rel="noopener noreferrer"
                style={{
                  color: ratingColor.primary,
                  textDecoration: "underline",
                  fontWeight: 500
                }}
              >
                Sustainable Web Design Model v4
              </a>
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

    {/* Hosting Status Info Banner */}
    <div
      style={{
        padding: "16px 20px",
        background: hostingStatus.bgColor,
        border: `1px solid ${hostingStatus.borderColor}`,
        borderRadius: "8px",
        display: "flex",
        alignItems: "center",
        gap: "12px",
      }}
    >
      <span
        style={{
          fontSize: "20px",
          color: hostingStatus.color,
          lineHeight: 1,
        }}
      >
        {hostingStatus.icon}
      </span>
      <div>
        <div
          style={{
            fontSize: "14px",
            fontWeight: 600,
            color: hostingStatus.color,
            marginBottom: "2px",
          }}
        >
          {hostingStatus.text}
        </div>
        <div style={{ fontSize: "13px", color: "#6b7280" }}>
          {hostingStatus.description}
        </div>
      </div>
    </div>

    {/* Resource Breakdown */}
    <div>
      <Headline size={HeadlineSize.M} spacingBottom={Spacing.L}>
        Resource Breakdown
      </Headline>
      <Stack spacing={Spacing.L}>
        {data.resourceGroups
          .sort((a, b) => {
            // Sort by logical category order: Images, CSS, Scripts, Links, Other
            const order = ['Images', 'CSS', 'Scripts', 'Links', 'Other'];
            return order.indexOf(a.name) - order.indexOf(b.name);
          })
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
  </>
);
