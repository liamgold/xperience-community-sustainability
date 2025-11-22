import React, { useState } from "react";
import {
  Card,
  Button,
  ButtonColor,
  ButtonSize,
  Stack,
  Headline,
  HeadlineSize,
  Spacing,
} from "@kentico/xperience-admin-components";
import { usePageCommand } from "@kentico/xperience-admin-base";
import {
  SustainabilityData,
  PageAvailabilityStatus,
  SustainabilityTabTemplateProps,
  Commands,
} from "./types";
import { ratingColors, getHostingStatusDisplay } from "../utils";
import { CurrentReportView } from "./current/CurrentReportView";
import { HistoryView } from "./history/HistoryView";

export const SustainabilityTabTemplate = (
  props: SustainabilityTabTemplateProps | null
) => {
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [data, setData] = useState<SustainabilityData | undefined | null>(
    props?.sustainabilityData
  );
  const [historicalReports, setHistoricalReports] = useState<
    SustainabilityData[]
  >(props?.historicalReports || []);
  const [expandedReportIndex, setExpandedReportIndex] = useState<number | null>(
    null
  );
  const [isLoadingMore, setIsLoadingMore] = useState(false);
  // Initialize hasMoreHistory from backend
  const [hasMoreHistory, setHasMoreHistory] = useState(props?.hasMoreHistory ?? false);
  const [showHistory, setShowHistory] = useState(false);
  const [nextPageIndex, setNextPageIndex] = useState(1); // Initial load got page 0, next is page 1

  const { execute: submit } = usePageCommand<SustainabilityTabTemplateProps>(
    Commands.RunReport,
    {
      after: (response) => {
        setData(response?.sustainabilityData);
        setIsLoading(false);
        setError(null);
        // Reload historical reports after running new report
        if (response?.historicalReports) {
          setHistoricalReports(response.historicalReports);
          setNextPageIndex(1); // Reset to page 1
          // Set hasMoreHistory from backend response
          setHasMoreHistory(response.hasMoreHistory ?? false);
        }
      },
      onError: (err) => {
        setIsLoading(false);
        setError("Failed to run sustainability report. Please try again.");
        console.error("Sustainability report error:", err);
      },
    }
  );

  const { execute: loadMoreHistory } = usePageCommand<
    { historicalReports: SustainabilityData[]; hasMoreHistory: boolean },
    { pageIndex: number }
  >(Commands.LoadMoreHistory, {
    after: (response) => {
      if (
        response?.historicalReports &&
        response.historicalReports.length > 0
      ) {
        setHistoricalReports((prev) => [
          ...prev,
          ...response.historicalReports,
        ]);
        setNextPageIndex((prev) => prev + 1); // Increment to next page
        // Set hasMoreHistory from backend response
        setHasMoreHistory(response.hasMoreHistory ?? false);
      } else {
        setHasMoreHistory(false);
      }
      setIsLoadingMore(false);
    },
    onError: (err) => {
      console.error("Load more history error:", err);
      setIsLoadingMore(false);
    },
  });

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
  const hostingStatus = getHostingStatusDisplay(data.greenHostingStatus);

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
            <Headline size={HeadlineSize.L}>
              {showHistory ? "Report History" : "Sustainability Report"}
            </Headline>
            {!showHistory && (
              <div
                style={{
                  fontSize: "14px",
                  color: "#6b7280",
                  marginTop: "4px",
                }}
              >
                Last analyzed: {data.lastRunDate}
              </div>
            )}
          </div>
          <div style={{ display: "flex", gap: "12px" }}>
            {showHistory ? (
              <Button
                label="Back to Current Report"
                color={ButtonColor.Secondary}
                size={ButtonSize.M}
                onClick={() => setShowHistory(false)}
              />
            ) : (
              <>
                {historicalReports.length > 0 && (
                  <Button
                    label="View History"
                    color={ButtonColor.Secondary}
                    size={ButtonSize.M}
                    onClick={() => setShowHistory(true)}
                  />
                )}
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
              </>
            )}
          </div>
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

        {/* Render appropriate view based on showHistory state */}
        {!showHistory ? (
          <CurrentReportView
            data={data}
            ratingColor={ratingColor}
            totalResources={totalResources}
            hostingStatus={hostingStatus}
          />
        ) : (
          <HistoryView
            currentReport={data}
            historicalReports={historicalReports}
            expandedReportIndex={expandedReportIndex}
            setExpandedReportIndex={setExpandedReportIndex}
            isLoadingMore={isLoadingMore}
            hasMoreHistory={hasMoreHistory}
            nextPageIndex={nextPageIndex}
            onLoadMore={(pageIndex) => {
              setIsLoadingMore(true);
              loadMoreHistory({ pageIndex });
            }}
          />
        )}
      </Stack>
    </div>
  );
};
