import React, { useEffect, useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { usePageCommand } from "@kentico/xperience-admin-base";

interface CustomLayoutProps {
  readonly sustainabilityData: any;
}

interface SustainabilityResponseResult {
  readonly sustainabilityData: any;
}

const Commands = {
  SetLabel: "SetLabel",
};

export const SustainabilityTabTemplate = (props: CustomLayoutProps) => {
  const [sustainabilityResponse, setSustainabilityResponse] = useState<
    SustainabilityResponseResult | undefined
  >(props);

  const { execute: submit } = usePageCommand<SustainabilityResponseResult>(
    Commands.SetLabel,
    {
      after: (response) => {
        setSustainabilityResponse(response);
      },
    }
  );
  console.log("SustainabilityTabTemplate", sustainabilityResponse);

  if (sustainabilityResponse?.sustainabilityData === undefined) {
    return (
      <div>
        Loading...
        <Button onClick={() => submit()}>Get data</Button>
      </div>
    );
  }

  return (
    <div className="grid grid-cols-1 md:grid-cols-3 gap-4 p-4">
      <div className="md:col-span-2 space-y-4">
        <Card>
          <CardHeader>
            <CardTitle>Scripts</CardTitle>
            <p className="text-sm text-muted-foreground">Total size: 3.00KB</p>
          </CardHeader>
          <CardContent className="space-y-1 text-sm">
            <ul className="list-disc list-inside">
              <li>https://script1.com/example.js (1.00KB)</li>
              <li>https://script2.com/example.js (2.00KB)</li>
            </ul>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Images</CardTitle>
            <p className="text-sm text-muted-foreground">Total size: 0.00KB</p>
          </CardHeader>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Styles</CardTitle>
            <p className="text-sm text-muted-foreground">Total size: 1.00KB</p>
          </CardHeader>
          <CardContent className="text-sm">
            <ul className="list-disc list-inside">
              <li>https://example.com/example.css (1.00KB)</li>
            </ul>
          </CardContent>
        </Card>
      </div>

      <div className="space-y-4">
        <Card>
          <CardHeader>
            <CardTitle>Sustainability report</CardTitle>
            <p className="text-sm text-muted-foreground">
              Last tested: May 10 2025 21:32:01
            </p>
          </CardHeader>
          <CardContent>
            <Button onClick={() => submit()}>Run again</Button>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Page size</CardTitle>
          </CardHeader>
          <CardContent className="text-xl font-semibold">7.00KB</CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>CO₂ per page view</CardTitle>
          </CardHeader>
          <CardContent className="text-xl font-semibold">
            {sustainabilityResponse.sustainabilityData.totalEmissions.toFixed(
              4
            )}
            g
          </CardContent>
        </Card>
      </div>
    </div>
  );
};
