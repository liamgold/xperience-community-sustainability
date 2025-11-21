import React from "react";
import { Button, ButtonColor, ButtonSize, Stack, Spacing } from "@kentico/xperience-admin-components";
import { SustainabilityData } from "../types";
import { TrendChart } from "./TrendChart";
import { HistoricalReportCard } from "./HistoricalReportCard";

interface HistoryViewProps {
  currentReport: SustainabilityData;
  historicalReports: SustainabilityData[];
  expandedReportIndex: number | null;
  setExpandedReportIndex: (index: number | null) => void;
  isLoadingMore: boolean;
  hasMoreHistory: boolean;
  nextPageIndex: number;
  loadMoreHistory: (data: { pageIndex: number }) => void;
}

export const HistoryView = ({
  currentReport,
  historicalReports,
  expandedReportIndex,
  setExpandedReportIndex,
  isLoadingMore,
  hasMoreHistory,
  nextPageIndex,
  loadMoreHistory,
}: HistoryViewProps) => (
  <>
    {/* Trend Chart */}
    <TrendChart currentReport={currentReport} historicalReports={historicalReports} />

    {/* Historical Reports List */}
    <Stack spacing={Spacing.M}>
      {historicalReports.map((report, index) => (
        <HistoricalReportCard
          key={index}
          report={report}
          isExpanded={expandedReportIndex === index}
          onToggle={() =>
            setExpandedReportIndex(
              expandedReportIndex === index ? null : index
            )
          }
        />
      ))}
      {hasMoreHistory && (
        <div style={{ display: "flex", justifyContent: "center", marginTop: "16px" }}>
          <Button
            label="Load More History"
            color={ButtonColor.Secondary}
            size={ButtonSize.M}
            disabled={isLoadingMore}
            inProgress={isLoadingMore}
            onClick={() => {
              loadMoreHistory({ pageIndex: nextPageIndex });
            }}
          />
        </div>
      )}
    </Stack>
  </>
);
