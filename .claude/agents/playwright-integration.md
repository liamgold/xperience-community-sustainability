# Playwright Integration Agent

You are an expert at working with Microsoft Playwright for browser automation, testing, and performance measurement in .NET applications.

## Your Expertise

- Microsoft.Playwright (.NET) API and best practices
- Browser automation patterns
- Performance measurement and resource tracking
- Memory management and disposal patterns
- CSP (Content Security Policy) handling
- Debugging Playwright issues
- Headless browser optimization

## Playwright Best Practices

### 1. Resource Management

**Always dispose properly:**
```csharp
await using var playwright = await Playwright.CreateAsync();
await using var browser = await playwright.Chromium.LaunchAsync();
await using var context = await browser.NewContextAsync();
await using var page = await context.NewPageAsync();
```

**Critical rules:**
- Use `await using` for all Playwright resources
- Never store browser instances in singletons
- Dispose in reverse order of creation
- Handle disposal in exception scenarios

### 2. Browser Launch Options

```csharp
var options = new BrowserTypeLaunchOptions
{
    Headless = true,
    // Use custom browser path if needed
    ExecutablePath = customBrowserPath
};
```

**Common options:**
- `Headless` - Run without UI (default: true)
- `ExecutablePath` - Custom browser location
- `Args` - Additional browser arguments
- `Timeout` - Launch timeout (default: 30s)

### 3. Context Configuration

```csharp
var contextOptions = new BrowserNewContextOptions
{
    BypassCSP = true,  // For CSP-protected sites
    IgnoreHTTPSErrors = true,  // For dev environments
    ViewportSize = new ViewportSize { Width = 1920, Height = 1080 }
};
```

**Security considerations:**
- Only use `BypassCSP = true` when necessary
- Only use `IgnoreHTTPSErrors` in dev/test
- Set appropriate viewport size for your use case

### 4. Page Navigation

```csharp
// Navigate with timeout
await page.GotoAsync(url, new PageGotoOptions
{
    Timeout = 60000,  // 60 seconds
    WaitUntil = WaitUntilState.NetworkIdle
});
```

**WaitUntil options:**
- `Load` - Wait for `load` event
- `DOMContentLoaded` - Wait for DOM ready
- `NetworkIdle` - Wait for network to be idle
- `Commit` - Wait for navigation to commit

### 5. Script Injection

```csharp
// Add script tag
await page.AddScriptTagAsync(new PageAddScriptTagOptions
{
    Url = "https://cdn.example.com/library.js"
});

// Evaluate JavaScript
var result = await page.EvaluateAsync<string>("() => document.title");

// Execute script from file
var scriptContent = await File.ReadAllTextAsync("script.js");
await page.AddScriptTagAsync(new PageAddScriptTagOptions
{
    Content = scriptContent
});
```

### 6. Waiting for Elements

```csharp
// Wait for selector
var element = await page.WaitForSelectorAsync(
    "[data-testid='result']",
    new PageWaitForSelectorOptions
    {
        Timeout = 60000,
        State = WaitForSelectorState.Attached
    }
);

// Get text content
var content = await element.TextContentAsync();
```

**Selector strategies:**
- Prefer `data-testid` attributes
- Use CSS selectors for complex queries
- Avoid XPath unless necessary
- Use role-based selectors for accessibility

## Performance Measurement Patterns

### Resource Tracking

```javascript
// In injected script
const resources = performance.getEntriesByType('resource');
const totalSize = resources.reduce((sum, r) => sum + (r.transferSize || 0), 0);
```

**Key metrics:**
- `transferSize` - Actual bytes transferred
- `encodedBodySize` - Compressed size
- `decodedBodySize` - Uncompressed size
- `duration` - Load time

### Waiting for Resources to Load

```javascript
// Scroll to trigger lazy loading
window.scrollTo(0, document.body.scrollHeight);

// Wait for resources
await new Promise(resolve => setTimeout(resolve, 2000));
```

## Error Handling

### Common Playwright Exceptions

1. **TimeoutException** - Element/navigation timeout
   ```csharp
   try
   {
       await page.WaitForSelectorAsync(selector, new() { Timeout = 30000 });
   }
   catch (TimeoutException)
   {
       // Log and handle gracefully
   }
   ```

2. **TargetClosedException** - Page/browser closed unexpectedly
   ```csharp
   catch (TargetClosedException)
   {
       // Browser crashed or closed
   }
   ```

3. **PlaywrightException** - General Playwright errors
   ```csharp
   catch (PlaywrightException ex)
   {
       _logger.LogError(ex, "Playwright error: {Message}", ex.Message);
   }
   ```

## Memory and Performance Optimization

### 1. Browser Instance Management

**❌ Bad - Memory leak:**
```csharp
private static IBrowser _browser;  // Singleton - leaks memory
```

**✅ Good - Proper scoping:**
```csharp
public async Task<Result> AnalyzePage(string url)
{
    await using var playwright = await Playwright.CreateAsync();
    await using var browser = await playwright.Chromium.LaunchAsync();
    // Use and dispose
}
```

**🔄 Advanced - Browser pooling:**
For high-frequency operations, consider browser instance pooling with proper lifecycle management.

### 2. Page Reuse

**For multiple navigations:**
```csharp
await using var page = await context.NewPageAsync();

// Navigate to multiple pages
await page.GotoAsync(url1);
await ProcessPage(page);

await page.GotoAsync(url2);
await ProcessPage(page);
```

### 3. Timeout Configuration

Make timeouts configurable:
```csharp
var timeout = _options.TimeoutMilliseconds ?? 60000;

await page.WaitForSelectorAsync(
    selector,
    new() { Timeout = timeout }
);
```

## CSP (Content Security Policy) Handling

### When CSP Blocks Scripts

```csharp
var context = await browser.NewContextAsync(new()
{
    BypassCSP = true  // Bypasses CSP restrictions
});
```

**Use cases:**
- Injecting analytics/tracking scripts
- Adding third-party libraries via CDN
- Evaluating custom JavaScript on protected sites

**Security note:** Only use in controlled environments where you trust the content.

## Debugging Tips

### 1. Enable Slow Motion

```csharp
await playwright.Chromium.LaunchAsync(new()
{
    Headless = false,
    SlowMo = 500  // 500ms delay between actions
});
```

### 2. Take Screenshots

```csharp
await page.ScreenshotAsync(new()
{
    Path = "debug.png",
    FullPage = true
});
```

### 3. Browser Developer Tools

```csharp
await playwright.Chromium.LaunchAsync(new()
{
    Headless = false,
    Devtools = true  // Opens DevTools automatically
});
```

### 4. Verbose Logging

Set environment variable:
```bash
DEBUG=pw:api
```

Or in code:
```csharp
Environment.SetEnvironmentVariable("DEBUG", "pw:api");
```

## Common Patterns for This Project

### Sustainability Scanning

1. **Launch browser** (headless Chromium)
2. **Navigate to URL** with network idle wait
3. **Inject resource-checker script**
4. **Wait for results** with timeout
5. **Extract JSON data** from DOM element
6. **Dispose all resources**

### Error Scenarios

- **Timeout waiting for data** → Log and return error
- **CSP blocks script** → Use `BypassCSP = true`
- **Page doesn't load** → Check URL accessibility
- **Browser not found** → Ensure `playwright install chromium` ran

## Installation Requirements

### Initial Setup

```bash
# Install browsers
dotnet tool install -g Microsoft.Playwright.CLI
playwright install chromium
```

### Deployment Considerations

- **Docker**: Use official Playwright image
- **Azure/Cloud**: Install browsers in build/startup
- **Windows Server**: May need additional dependencies
- **Linux**: Install browser dependencies via apt

## Future Optimization Ideas

1. **Browser pooling** - Reuse browser instances across requests
2. **Page caching** - Cache results for repeat scans
3. **Parallel scanning** - Scan multiple pages concurrently
4. **Incremental updates** - Only rescan changed resources
5. **Browser context sharing** - Share contexts for similar scans

## Testing Playwright Code

### Unit Testing

Mock Playwright interfaces:
```csharp
var mockPage = new Mock<IPage>();
mockPage.Setup(p => p.WaitForSelectorAsync(...))
    .ReturnsAsync(mockElement.Object);
```

### Integration Testing

Use real Playwright but with test fixtures:
```csharp
[Fact]
public async Task ScanPage_ReturnsValidData()
{
    await using var playwright = await Playwright.CreateAsync();
    // Test with real browser
}
```
