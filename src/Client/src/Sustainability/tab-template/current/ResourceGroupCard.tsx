import React, { useState } from "react";
import { Button, ButtonColor, ButtonSize, Icon } from "@kentico/xperience-admin-components";
import { ExternalResourceGroup } from "../types";
import { getResourceTypeIcon, getResourceTypeColor } from "../../utils";

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

interface ResourceGroupCardProps {
  group: ExternalResourceGroup;
  totalPageSize: number;
}

export const ResourceGroupCard = ({ group, totalPageSize }: ResourceGroupCardProps) => {
  const [expanded, setExpanded] = useState(false);
  const displayCount = expanded ? group.resources.length : 3;

  return (
    <div style={resourceGroupCardStyles.container}>
      <div style={resourceGroupCardStyles.header}>
        <div style={{ display: "flex", alignItems: "center", gap: "12px" }}>
          {(() => {
            const resourceTypeColor = getResourceTypeColor(group.name);
            return (
              <div
                style={{
                  display: "flex",
                  alignItems: "center",
                  justifyContent: "center",
                  width: "48px",
                  height: "48px",
                  borderRadius: "8px",
                  backgroundColor: resourceTypeColor.bg,
                  color: resourceTypeColor.color,
                  border: `1px solid ${resourceTypeColor.border}`,
                  flexShrink: 0,
                  fontSize: "24px",
                }}
              >
                <Icon name={getResourceTypeIcon(group.name)} />
              </div>
            );
          })()}
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
                      {resource.contentHubUrl && (
                        <a
                          href={resource.contentHubUrl}
                          target="_blank"
                          rel="noopener noreferrer"
                          title="View in Content Hub"
                          style={{
                            marginLeft: "8px",
                            display: "inline-flex",
                            alignItems: "center",
                            verticalAlign: "top",
                            textDecoration: "none",
                            cursor: "pointer",
                            fontSize: "14px",
                            color: "var(--color-text-secondary)",
                            opacity: 0.7,
                          }}
                          onMouseEnter={(e) => {
                            e.currentTarget.style.opacity = "1";
                          }}
                          onMouseLeave={(e) => {
                            e.currentTarget.style.opacity = "0.7";
                          }}
                        >
                          <Icon name="xp-arrow-right-top-square" />
                        </a>
                      )}
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
