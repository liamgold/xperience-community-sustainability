namespace XperienceCommunity.Sustainability.Models;

public class SustainabilityOptions
{
    /// <summary>
    /// Timeout in milliseconds for waiting for sustainability data to be collected.
    /// Default: 60000ms (60 seconds)
    /// </summary>
    public int TimeoutMilliseconds { get; set; } = 60000;

    /// <summary>
    /// Custom path where Playwright browsers should be installed.
    /// Only used when hosting on UNC paths (network shares starting with \\).
    /// For standard hosting, browsers are automatically installed in App_Data/playwright.
    /// </summary>
    public string? PlaywrightBrowserPath { get; set; }

    /// <summary>
    /// Enable browser console logging to Kentico Event Log for debugging.
    /// Default: false
    /// </summary>
    public bool EnableBrowserConsoleLogging { get; set; } = false;
}
