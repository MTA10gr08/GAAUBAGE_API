using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

public partial class AppSettings
{
    [JsonProperty("Logging")]
    public Logging Logging { get; set; }

    [JsonProperty("AllowedHosts")]
    public string AllowedHosts { get; set; }

    [JsonProperty("BackgroundCategories")]
    public string[] BackgroundCategories { get; set; }

    [JsonProperty("TrashCategories")]
    public TrashCategory[] TrashCategories { get; set; }

    [JsonProperty("Jwt")]
    public Jwt Jwt { get; set; }
}

public partial class Jwt
{
    [JsonProperty("Key")]
    public string Key { get; set; }

    [JsonProperty("Issuer")]
    public string Issuer { get; set; }

    [JsonProperty("Audience")]
    public string Audience { get; set; }
}

public partial class Logging
{
    [JsonProperty("LogLevel")]
    public LogLevel LogLevel { get; set; }
}

public partial class LogLevel
{
    [JsonProperty("Default")]
    public string Default { get; set; }

    [JsonProperty("Microsoft.AspNetCore")]
    public string MicrosoftAspNetCore { get; set; }
}

public partial class TrashCategory
{
    [JsonProperty("Name")]
    public string Name { get; set; }

    [JsonProperty("SubCategories")]
    public string[] SubCategories { get; set; }
}

public partial class AppSettings
{
    public static AppSettings FromJson(string json) => JsonConvert.DeserializeObject<AppSettings>(json, Converter.Settings);
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
