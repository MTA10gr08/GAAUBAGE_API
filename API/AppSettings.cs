using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

public partial class AppSettings
{
    [JsonProperty("Logging")]
    public Logging Logging { get; set; } = null!;

    [JsonProperty("AllowedHosts")]
    public string AllowedHosts { get; set; } = null!;

    [JsonProperty("BackgroundCategories")]
    public string[] BackgroundCategories { get; set; } = null!;

    [JsonProperty("ContextCategories")]
    public string[] ContextCategories { get; set; } = null!;

    [JsonProperty("TrashCategories")]
    public TrashCategory[] TrashCategories { get; set; } = null!;

    [JsonProperty("Jwt")]
    public Jwt Jwt { get; set; } = null!;

    [JsonProperty("DB")]
    public DB? DB { get; set; }
}

public partial class DB
{
    [JsonProperty("Address")]
    public string Address { get; set; } = null!;

    [JsonProperty("Port")]
    public uint Port { get; set; }

    [JsonProperty("Database")]
    public string Database { get; set; } = null!;

    [JsonProperty("User")]
    public string User { get; set; } = null!;

    [JsonProperty("Password")]
    public string Password { get; set; } = null!;
}

public partial class Jwt
{
    [JsonProperty("Key")]
    public string Key { get; set; } = null!;

    [JsonProperty("Issuer")]
    public string Issuer { get; set; } = null!;

    [JsonProperty("Audience")]
    public string Audience { get; set; } = null!;
}

public partial class Logging
{
    [JsonProperty("LogLevel")]
    public LogLevel LogLevel { get; set; } = null!;
}

public partial class LogLevel
{
    [JsonProperty("Default")]
    public string Default { get; set; } = null!;

    [JsonProperty("Microsoft.AspNetCore")]
    public string MicrosoftAspNetCore { get; set; } = null!;
}

public partial class TrashCategory
{
    [JsonProperty("Name")]
    public string Name { get; set; } = null!;

    [JsonProperty("SubCategories")]
    public string[] SubCategories { get; set; } = null!;
}

public partial class AppSettings
{
    public static AppSettings? FromJson(string json) => JsonConvert.DeserializeObject<AppSettings>(json, Converter.Settings);
}

public static class Serialize
{
    public static string ToJson(this AppSettings self) => JsonConvert.SerializeObject(self, Converter.Settings);
}

internal static class Converter
{
    public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
    {
        MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
        DateParseHandling = DateParseHandling.None,
        Converters = {
            new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
        },
    };
}
