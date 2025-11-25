using Microsoft.Playwright;
using XperienceCommunity.Sustainability.Models;

namespace XperienceCommunity.Sustainability.Services;

/// <summary>
/// Service for generating PDF reports from sustainability data.
/// </summary>
public interface ISustainabilityPdfService
{
    /// <summary>
    /// Generates a PDF report for a sustainability analysis.
    /// </summary>
    /// <param name="report">The sustainability report data.</param>
    /// <param name="pageTitle">The title of the page being analyzed.</param>
    /// <param name="pageUrl">The URL of the page being analyzed.</param>
    /// <returns>A byte array containing the PDF document.</returns>
    Task<byte[]> GeneratePdfReport(SustainabilityResponse report, string pageTitle, string pageUrl);
}

public class SustainabilityPdfService : ISustainabilityPdfService
{
    public async Task<byte[]> GeneratePdfReport(SustainabilityResponse report, string pageTitle, string pageUrl)
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new()
        {
            Headless = true
        });

        var page = await browser.NewPageAsync();

        var htmlContent = GeneratePdfHtml(report, pageTitle, pageUrl);
        await page.SetContentAsync(htmlContent);

        var pdfBytes = await page.PdfAsync(new PagePdfOptions
        {
            Format = "A4",
            PrintBackground = true,
            Margin = new Margin
            {
                Top = "1.5cm",
                Right = "1.5cm",
                Bottom = "1.5cm",
                Left = "1.5cm"
            }
        });

        return pdfBytes;
    }

    private string GeneratePdfHtml(SustainabilityResponse report, string pageTitle, string pageUrl)
    {
        var ratingColor = report.CarbonRating switch
        {
            "A+" => "#10b981",
            "A" => "#22c55e",
            "B" => "#84cc16",
            "C" => "#eab308",
            "D" => "#f97316",
            "E" => "#ef4444",
            "F" => "#dc2626",
            _ => "#6b7280"
        };

        var ratingDescription = report.CarbonRating switch
        {
            "A+" => "Extremely efficient",
            "A" => "Very efficient",
            "B" => "Efficient",
            "C" => "Moderate efficiency",
            "D" => "Low efficiency",
            "E" => "Poor efficiency",
            "F" => "Very poor efficiency",
            _ => "Not Rated"
        };

        var ratingSecondaryText = report.CarbonRating switch
        {
            "A+" or "A" => "This page has excellent carbon efficiency.",
            "B" or "C" => "This page has room for improvement.",
            "D" or "E" or "F" => "This page needs significant optimization.",
            _ => ""
        };

        var hostingBadge = report.GreenHostingStatus == "Green"
            ? "<span style=\"background: #d1fae5; color: #065f46; padding: 4px 12px; border-radius: 12px; font-size: 12px; font-weight: 600;\">✓ Green Hosting</span>"
            : report.GreenHostingStatus == "NotGreen"
            ? "<span style=\"background: #fee2e2; color: #991b1b; padding: 4px 12px; border-radius: 12px; font-size: 12px; font-weight: 600;\">Standard Hosting</span>"
            : "<span style=\"background: #f3f4f6; color: #4b5563; padding: 4px 12px; border-radius: 12px; font-size: 12px; font-weight: 600;\">Unknown</span>";

        var totalResources = report.ResourceGroups?.Sum(g => g.Resources?.Count ?? 0) ?? 0;

        var resourceBreakdown = new System.Text.StringBuilder();
        if (report.ResourceGroups != null && report.ResourceGroups.Count > 0)
        {
            resourceBreakdown.Append("<div class=\"resource-breakdown\">");
            resourceBreakdown.Append("<h2 style=\"font-size: 18px; font-weight: 700; color: #111827; margin-bottom: 16px;\">Resource Breakdown</h2>");

            foreach (var group in report.ResourceGroups.OrderByDescending(g => g.TotalSize))
            {
                var topResources = group.Resources?.OrderByDescending(r => r.Size).Take(5).ToList();
                if (topResources == null || topResources.Count == 0) continue;

                resourceBreakdown.Append($@"
                <div class=""resource-group"" style=""margin-bottom: 20px; padding: 16px; background: #f9fafb; border-radius: 8px;"">
                    <div style=""font-weight: 600; color: #111827; margin-bottom: 8px;"">{group.Name}</div>
                    <div style=""font-size: 12px; color: #6b7280; margin-bottom: 12px;"">{group.Resources?.Count ?? 0} resources • {group.TotalSize:F2} KB total</div>
                    <table style=""width: 100%; border-collapse: collapse;"">
                ");

                foreach (var resource in topResources)
                {
                    var fileName = System.IO.Path.GetFileName(resource.Url) ?? resource.Url ?? "Unknown";
                    if (fileName.Length > 60) fileName = fileName[..57] + "...";

                    resourceBreakdown.Append($@"
                        <tr style=""border-bottom: 1px solid #e5e7eb;"">
                            <td style=""padding: 8px 0; font-size: 12px; color: #374151; width: 70%;"">{System.Security.SecurityElement.Escape(fileName)}</td>
                            <td style=""padding: 8px 0; font-size: 12px; color: #6b7280; text-align: right;"">{resource.Size:F2} KB</td>
                        </tr>
                    ");
                }

                resourceBreakdown.Append("</table></div>");
            }

            resourceBreakdown.Append("</div>");
        }

        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <style>
        * {{ margin: 0; padding: 0; box-sizing: border-box; }}
        body {{
            font-family: system-ui, -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
            line-height: 1.6;
            color: #111827;
            padding: 20px;
        }}
        .resource-breakdown {{
            page-break-before: always;
        }}
        .resource-group {{
            page-break-inside: avoid;
            break-inside: avoid;
        }}
        .carbon-rating-hero {{
            page-break-inside: avoid;
            break-inside: avoid;
        }}
        .metrics-grid {{
            page-break-inside: avoid;
            break-inside: avoid;
        }}
    </style>
</head>
<body>
    <div style=""max-width: 800px; margin: 0 auto;"">
        <!-- Header -->
        <div style=""text-align: center; margin-bottom: 30px; padding-bottom: 20px; border-bottom: 2px solid #e5e7eb;"">
            <h1 style=""font-size: 28px; font-weight: 800; color: #111827; margin-bottom: 8px;"">Sustainability Report</h1>
            <div style=""font-size: 16px; color: #6b7280; margin-bottom: 4px;"">{System.Security.SecurityElement.Escape(pageTitle)}</div>
            <div style=""font-size: 12px; color: #9ca3af;"">{System.Security.SecurityElement.Escape(pageUrl)}</div>
            <div style=""font-size: 12px; color: #9ca3af; margin-top: 8px;"">Generated: {report.LastRunDate}</div>
        </div>

        <!-- Carbon Rating Hero -->
        <div class=""carbon-rating-hero"" style=""background: linear-gradient(135deg, {ratingColor}15 0%, white 100%); border: 2px solid {ratingColor}; border-radius: 12px; padding: 30px; margin-bottom: 30px; text-align: center;"">
            <div style=""font-size: 14px; font-weight: 600; color: #6b7280; text-transform: uppercase; letter-spacing: 0.5px; margin-bottom: 12px;"">Carbon Rating</div>
            <div style=""font-size: 72px; font-weight: 900; color: {ratingColor}; margin-bottom: 12px; line-height: 1;"">{report.CarbonRating}</div>
            <div style=""font-size: 18px; font-weight: 600; color: #111827; margin-bottom: 8px;"">{ratingDescription}</div>
            <div style=""font-size: 14px; color: #6b7280; margin-bottom: 16px;"">{ratingSecondaryText}</div>
            {hostingBadge}
        </div>

        <!-- Metrics Grid -->
        <div class=""metrics-grid"" style=""display: grid; grid-template-columns: 1fr 1fr; gap: 16px; margin-bottom: 30px;"">
            <div style=""background: white; border: 1px solid #e5e7eb; border-radius: 8px; padding: 20px;"">
                <div style=""font-size: 12px; font-weight: 600; color: #6b7280; text-transform: uppercase; margin-bottom: 8px;"">CO₂ Emissions</div>
                <div style=""font-size: 28px; font-weight: 700; color: #111827; margin-bottom: 4px;"">{report.TotalEmissions:F3}g</div>
                <div style=""font-size: 12px; color: #9ca3af;"">per page view</div>
            </div>
            <div style=""background: white; border: 1px solid #e5e7eb; border-radius: 8px; padding: 20px;"">
                <div style=""font-size: 12px; font-weight: 600; color: #6b7280; text-transform: uppercase; margin-bottom: 8px;"">Page Weight</div>
                <div style=""font-size: 28px; font-weight: 700; color: #111827; margin-bottom: 4px;"">{report.TotalSize:F2}KB</div>
                <div style=""font-size: 12px; color: #9ca3af;"">{(report.TotalSize / 1024):F2} MB total</div>
            </div>
            <div style=""background: white; border: 1px solid #e5e7eb; border-radius: 8px; padding: 20px;"">
                <div style=""font-size: 12px; font-weight: 600; color: #6b7280; text-transform: uppercase; margin-bottom: 8px;"">Resources</div>
                <div style=""font-size: 28px; font-weight: 700; color: #111827; margin-bottom: 4px;"">{totalResources}</div>
                <div style=""font-size: 12px; color: #9ca3af;"">{report.ResourceGroups?.Count ?? 0} categories</div>
            </div>
            <div style=""background: white; border: 1px solid #e5e7eb; border-radius: 8px; padding: 20px;"">
                <div style=""font-size: 12px; font-weight: 600; color: #6b7280; text-transform: uppercase; margin-bottom: 8px;"">Methodology</div>
                <div style=""font-size: 16px; font-weight: 700; color: #111827; margin-bottom: 4px;"">SWDM v4</div>
                <div style=""font-size: 12px; color: #9ca3af;"">Sustainable Web Design</div>
            </div>
        </div>

        {resourceBreakdown}

        <!-- Footer -->
        <div style=""margin-top: 40px; padding-top: 20px; border-top: 1px solid #e5e7eb; text-align: center; font-size: 11px; color: #9ca3af;"">
            <div>Powered by <strong><a href=""https://github.com/liamgold/xperience-community-sustainability"" style=""color: #111827; text-decoration: none;"">Xperience Community: Sustainability</a></strong></div>
            <div style=""margin-top: 4px;"">Carbon ratings based on Sustainable Web Design Model v4</div>
            <div style=""margin-top: 4px;"">https://sustainablewebdesign.org/digital-carbon-ratings/</div>
        </div>
    </div>
</body>
</html>";
    }
}
