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

export const SustainabilityTabTemplate = ({ label }: CustomLayoutProps) => {
  const [labelValue, setLabelValue] = useState(label);

  const { execute: submit } = usePageCommand<ResponseResult>(
    Commands.SetLabel,
    {
      after: (response) => {
        setLabelValue(response.label);
      },
    }
  );

  return (
    <div>
      <h1>{labelValue}</h1>
      <Button label="Get data" size={ButtonSize.S} onClick={() => submit()} />
    </div>
  );
};
