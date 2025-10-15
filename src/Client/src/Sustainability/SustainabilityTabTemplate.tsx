import React, { useEffect, useState } from "react";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Loader2 } from "lucide-react";
import { cn } from "@/lib/utils";
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
  "A+": "Extremely efficient (≤ 0.040g CO₂ per page view)",
  A: "Very efficient (≤ 0.079g CO₂ per page view)",
  B: "Efficient (≤ 0.145g CO₂ per page view)",
  C: "Moderate efficiency (≤ 0.209g CO₂ per page view)",
  D: "Low efficiency (≤ 0.278g CO₂ per page view)",
  E: "Poor efficiency (≤ 0.359g CO₂ per page view)",
  F: "Very poor efficiency (> 0.359g CO₂ per page view)",
};

const ratingColor: Record<string, string> = {
  "A+": "text-emerald-600", // 🌿 Ultra efficient
  A: "text-green-600", // ✅ Very efficient
  B: "text-lime-600", // 👍 Efficient
  C: "text-yellow-600", // 😐 Moderate
  D: "text-amber-600", // ⚠️ Low
  E: "text-orange-600", // 🚨 Poor
  F: "text-red-600", // ❌ Very poor
};

const Commands = {
  RunReport: "RunReport",
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
      <div className="p-4 space-y-4">
        <h1 className="text-2xl font-semibold tracking-tight text-foreground">
          Sustainability Report
        </h1>

        <div className="flex items-center justify-center min-h-[60vh]">
          <Card className="w-full max-w-md text-center bg-card text-card-foreground">
            <CardHeader>
              <CardTitle className="text-xl">
                No Sustainability Data Found
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              {props?.pageAvailability === PageAvailabilityStatus.Available ? (
                <>
                  <p className="text-muted-foreground">
                    We haven't retrieved any sustainability data for this page
                    yet.
                  </p>
                  {error && (
                    <div className="text-sm text-red-600 bg-red-50 border border-red-200 rounded p-3">
                      {error}
                    </div>
                  )}
                  <Button
                    disabled={isLoading}
                    onClick={() => {
                      setIsLoading(true);
                      setError(null);
                      submit();
                    }}
                  >
                    {isLoading && <Loader2 className="animate-spin" />}
                    Run Sustainability Report
                  </Button>
                </>
              ) : (
                <>
                  <p className="text-muted-foreground">
                    The page is not available, so we cannot retrieve the data.
                  </p>
                </>
              )}
            </CardContent>
          </Card>
        </div>
      </div>
    );
  }

  const renderResourceList = (resources: ExternalResource[]) =>
    resources.length > 0 ? (
      <ul className="list-disc list-inside text-sm space-y-0.5">
        {resources.map((item, i) => (
          <li key={i}>
            {item.url} ({item.size.toFixed(2)}KB)
          </li>
        ))}
      </ul>
    ) : (
      <p className="text-muted-foreground text-sm">No resources found.</p>
    );

  return (
    <div className="p-4 space-y-4">
      {/* New Title */}
      <h1 className="text-2xl font-semibold tracking-tight text-foreground">
        Sustainability Report
      </h1>
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        {/* Left column: Resource groups */}
        <div className="md:col-span-2 space-y-4">
          {data.resourceGroups.map((group) => (
            <Card key={group.type}>
              <CardHeader>
                <CardTitle>{group.name}</CardTitle>
                <CardDescription>
                  Total size: {group.totalSize.toFixed(2)}KB
                </CardDescription>
              </CardHeader>
              <CardContent>{renderResourceList(group.resources)}</CardContent>
            </Card>
          ))}
        </div>

        {/* Right column: Summary */}
        <div className="space-y-4">
          <Card className="max-w-sm w-full">
            <CardHeader>
              <CardTitle>Sustainability report</CardTitle>
              <CardDescription>Last tested: {data.lastRunDate}</CardDescription>
            </CardHeader>
            <CardContent className="space-y-3">
              {error && (
                <div className="text-sm text-red-600 bg-red-50 border border-red-200 rounded p-3">
                  {error}
                </div>
              )}
              <Button
                disabled={isLoading}
                onClick={() => {
                  setIsLoading(true);
                  setError(null);
                  submit();
                }}
                className="bg-primary text-primary-foreground hover:bg-primary/90 w-full"
              >
                {isLoading && <Loader2 className="animate-spin" />}
                Run again
              </Button>
            </CardContent>
          </Card>

          <Card className="max-w-sm w-full">
            <CardHeader>
              <CardTitle>Page size</CardTitle>
            </CardHeader>
            <CardContent className="text-xl font-semibold">
              {data.totalSize.toFixed(2)}KB
            </CardContent>
          </Card>

          <Card className="max-w-sm w-full">
            <CardHeader>
              <CardTitle>CO₂ per page view</CardTitle>
            </CardHeader>
            <CardContent className="text-xl font-semibold">
              {data.totalEmissions.toFixed(4)}g
            </CardContent>
          </Card>

          {/* Carbon rating */}
          <Card className="max-w-sm w-full">
            <CardHeader>
              <CardTitle>Carbon rating</CardTitle>
              <CardDescription>
                {ratingDescriptions[data.carbonRating] || "No rating available"}
              </CardDescription>
            </CardHeader>
            <CardContent
              className={cn(
                "text-3xl font-bold",
                ratingColor[data.carbonRating] || "text-muted-foreground"
              )}
            >
              {data.carbonRating}
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
};
