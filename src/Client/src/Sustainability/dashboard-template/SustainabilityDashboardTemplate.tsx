import React, { useState } from "react";
import { Button, ButtonSize } from "@kentico/xperience-admin-components";
import { usePageCommand } from "@kentico/xperience-admin-base";

interface CustomLayoutProps {
  readonly label: string;
}

interface ResponseResult {
  readonly label: string;
}

const Commands = {
  SetLabel: "SetLabel",
};

export const SustainabilityDashboardTemplate = ({
  label,
}: CustomLayoutProps) => {
  const [labelValue, setLabelValue] = useState(label);

  return (
    <div>
      <h1>{labelValue}</h1>
      <p>Coming soon: https://github.com/liamgold/xperience-community-sustainability/issues/4.</p>
    </div>
  );
};
