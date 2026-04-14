using Microsoft.Extensions.Configuration;
using Serilog;

namespace Singleton.App;

// PATTERN CONCEPT: Same Lazy<T> approach — the JSON file is parsed once on first
// access, then the same parsed dictionary is returned to every caller.
//
// In production, prefer IOptions<T> via Microsoft.Extensions.Configuration, which
// is registered as a singleton through the DI container. This class demonstrates
// the raw Singleton pattern without DI so the mechanism is visible.
public sealed class ConfigurationService
{
    private static readonly Lazy<ConfigurationService> _instance =
        new(() => new ConfigurationService());

    public static ConfigurationService Instance => _instance.Value;

    private readonly IConfiguration _config;

    private ConfigurationService()
    {
        Log.Information("ConfigurationService initialised — parsing appsettings.json (hash: {Hash})", GetHashCode());

        _config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();
    }

    public string GetRequiredValue(string key)
    {
        var value = _config[key]
            ?? throw new InvalidOperationException($"Configuration key '{key}' is missing.");
        return value;
    }

    public string GetValue(string key, string defaultValue = "") =>
        _config[key] ?? defaultValue;

    public IConfiguration GetSection(string sectionName) =>
        _config.GetSection(sectionName);
}
