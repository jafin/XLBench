using System.Text.Json;

// ChartGen — bakes the committed benchmark payload into a self-contained charts page.
//
// It reads docs/results-data.js (the `window.XLBENCH_DATA = { ... };` written by
// scripts/run-benchmarks.ps1), validates the JSON, and injects it into
// tools/ChartGen/charts.template.html at the `/*__XLBENCH_DATA__*/ null` placeholder,
// producing docs/charts.html with the data inlined (no runtime fetch of results-data.js).
//
// Usage (paths are relative to the repo root, which is the default working directory
// in CI and when run via `dotnet run --project tools/ChartGen`):
//
//   dotnet run --project tools/ChartGen
//   dotnet run --project tools/ChartGen -- <data.js> <template.html> <out.html>
//
// Exit codes: 0 ok, 1 bad usage / missing file, 2 malformed data, 3 template placeholder missing.

const string Placeholder = "/*__XLBENCH_DATA__*/ null";

string dataPath     = args.Length > 0 ? args[0] : Path.Combine("docs", "results-data.js");
string templatePath = args.Length > 1 ? args[1] : Path.Combine("tools", "ChartGen", "charts.template.html");
string outPath      = args.Length > 2 ? args[2] : Path.Combine("docs", "charts.html");

if (args.Length is not (0 or 3) && !(args.Length is 1 or 2))
{
    Console.Error.WriteLine("usage: ChartGen [<data.js> <template.html> <out.html>]");
    return 1;
}

foreach (var (label, path) in new[] { ("data", dataPath), ("template", templatePath) })
{
    if (!File.Exists(path))
    {
        Console.Error.WriteLine($"[ChartGen] {label} file not found: {path}");
        return 1;
    }
}

// results-data.js is `window.XLBENCH_DATA = { ... };` — pull out the JSON object.
string js = File.ReadAllText(dataPath);
int start = js.IndexOf('{');
int end = js.LastIndexOf('}');
if (start < 0 || end <= start)
{
    Console.Error.WriteLine($"[ChartGen] no JSON object found in {dataPath}");
    return 2;
}
string json = js.Substring(start, end - start + 1);

// Validate + re-serialize compactly so the injected literal is clean and minimal.
JsonDocument doc;
try
{
    doc = JsonDocument.Parse(json);
}
catch (JsonException ex)
{
    Console.Error.WriteLine($"[ChartGen] {dataPath} did not contain valid JSON: {ex.Message}");
    return 2;
}

if (!doc.RootElement.TryGetProperty("scenarios", out var scenarios) || scenarios.GetArrayLength() == 0)
    Console.Error.WriteLine("[ChartGen] warning: payload has no scenarios; the page will show the empty state.");

string compact = JsonSerializer.Serialize(doc.RootElement);

string template = File.ReadAllText(templatePath);
// Target the LAST occurrence: the render script's placeholder is the final one in the
// file, so this stays correct even if a comment elsewhere mentions the marker.
int idx = template.LastIndexOf(Placeholder, StringComparison.Ordinal);
if (idx < 0)
{
    Console.Error.WriteLine($"[ChartGen] placeholder '{Placeholder}' not found in {templatePath}");
    return 3;
}

// Keep the comment marker so the file stays regenerable/greppable; swap null → the data.
string html = string.Concat(
    template.AsSpan(0, idx),
    $"/*__XLBENCH_DATA__*/ {compact}",
    template.AsSpan(idx + Placeholder.Length));

Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outPath))!);
File.WriteAllText(outPath, html);

Console.WriteLine($"[ChartGen] wrote {outPath} ({html.Length:N0} bytes) from {dataPath} + {templatePath}");
return 0;
