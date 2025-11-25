export type SustainabilityData = {
  resourceGroups: ExternalResourceGroup[];
  totalSize: number;
  totalEmissions: number;
  lastRunDate: string;
  carbonRating: string;
  greenHostingStatus?: string;
};

export type ExternalResource = {
  url: string;
  size: number;
  contentHubUrl?: string;
};

export type ExternalResourceGroup = {
  type: string;
  name: string;
  totalSize: number;
  resources: ExternalResource[];
};

export enum PageAvailabilityStatus {
  Available,
  NotAvailable,
}

export interface SustainabilityTabTemplateProps {
  pageAvailability: PageAvailabilityStatus;
  sustainabilityData: SustainabilityData;
  historicalReports: SustainabilityData[];
  hasMoreHistory?: boolean;
}

export const Commands = {
  RunReport: "RunReport",
  LoadMoreHistory: "LoadMoreHistory",
  ExportReportAsPdf: "ExportReportAsPdf",
};
