using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.UI.FuzulVentures;

public sealed class FuzulLayout
{
    [JsonPropertyName("v")]
    public int V { get; set; } = 1;

    [JsonPropertyName("widgets")]
    public List<FuzulWidget> Widgets { get; set; } = [];

    [JsonPropertyName("pins")]
    public Dictionary<string, FuzulPin> Pins { get; set; } = new(StringComparer.Ordinal);
}

public sealed class FuzulWidget
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("section")]
    public string Section { get; set; } = "";

    [JsonPropertyName("type")]
    public string Type { get; set; } = "button";

    [JsonPropertyName("x")]
    public double X { get; set; }

    [JsonPropertyName("y")]
    public double Y { get; set; }

    [JsonPropertyName("w")]
    public double W { get; set; } = 18;

    [JsonPropertyName("h")]
    public double H { get; set; } = 8;

    [JsonPropertyName("z")]
    public int Z { get; set; } = 4;

    [JsonPropertyName("text")]
    public string Text { get; set; } = "";

    [JsonPropertyName("href")]
    public string Href { get; set; } = "";

    [JsonPropertyName("bg")]
    public string Bg { get; set; } = "";

    [JsonPropertyName("color")]
    public string Color { get; set; } = "";

    [JsonPropertyName("size")]
    public int Size { get; set; } = 16;

    [JsonPropertyName("radius")]
    public int Radius { get; set; }

    [JsonPropertyName("src")]
    public string Src { get; set; } = "";
}

public sealed class FuzulPin
{
    [JsonPropertyName("section")]
    public string Section { get; set; } = "";

    [JsonPropertyName("x")]
    public double X { get; set; }

    [JsonPropertyName("y")]
    public double Y { get; set; }

    [JsonPropertyName("w")]
    public double W { get; set; }

    [JsonPropertyName("h")]
    public double H { get; set; }

    [JsonPropertyName("z")]
    public int Z { get; set; } = 5;
}

public static class FuzulDesigner
{
    public const string PropertyAlias = "designerLayout";
    public const int MaxLayoutChars = 100_000;
    private const int MaxWidgets = 80;
    private const int MaxPins = 200;
    private static readonly HashSet<string> Types = ["button", "text", "heading", "image", "shape"];
    private static readonly Regex SectionKey = new("^(header|footer|[0-9a-f]{32})$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex PinKey = new("^[a-zA-Z0-9_-]{2,64}$", RegexOptions.CultureInvariant | RegexOptions.Compiled);
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
    };

    public static async Task<bool> CanDesignAsync(HttpContext http, IWebHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            return true;
        }

        AuthenticateResult result = await http.AuthenticateBackOfficeAsync();
        return result.Succeeded;
    }

    public static FuzulLayout Parse(IPublishedContent page)
        => Sanitize(page.Value<string>(PropertyAlias));

    public static FuzulLayout Sanitize(string? json)
    {
        var layout = new FuzulLayout();
        if (string.IsNullOrWhiteSpace(json) || json.Length > MaxLayoutChars)
        {
            return layout;
        }

        FuzulLayout? parsed;
        try
        {
            parsed = JsonSerializer.Deserialize<FuzulLayout>(json, Json);
        }
        catch (JsonException)
        {
            return layout;
        }

        if (parsed is null)
        {
            return layout;
        }

        layout.V = 1;
        foreach (FuzulWidget widget in parsed.Widgets.Take(MaxWidgets))
        {
            FuzulWidget? clean = CleanWidget(widget);
            if (clean is not null)
            {
                layout.Widgets.Add(clean);
            }
        }

        foreach (KeyValuePair<string, FuzulPin> pair in parsed.Pins.Take(MaxPins))
        {
            if (!PinKey.IsMatch(pair.Key))
            {
                continue;
            }

            FuzulPin? pin = CleanPin(pair.Value);
            if (pin is not null)
            {
                layout.Pins[pair.Key] = pin;
            }
        }

        return layout;
    }

    public static string Serialize(FuzulLayout layout) => JsonSerializer.Serialize(Sanitize(JsonSerializer.Serialize(layout, Json)), Json);

    public static IEnumerable<FuzulWidget> WidgetsFor(IPublishedContent page, string section)
        => Parse(page).Widgets.Where(x => string.Equals(x.Section, section, StringComparison.OrdinalIgnoreCase));

    public static IHtmlContent Markup(FuzulWidget widget)
    {
        string id = WebUtility.HtmlEncode(widget.Id);
        string style = Css(widget);
        string text = WebUtility.HtmlEncode(widget.Text);
        string cls = "fv-widget fv-widget--" + widget.Type;
        return widget.Type switch
        {
            "image" => new HtmlString($"<div class=\"{cls}\" data-fv-widget=\"{id}\" style=\"{style}\"><img src=\"{WebUtility.HtmlEncode(widget.Src)}\" alt=\"\"></div>"),
            "heading" => new HtmlString($"<div class=\"{cls}\" data-fv-widget=\"{id}\" style=\"{style}\"><h2>{text}</h2></div>"),
            "text" => new HtmlString($"<div class=\"{cls}\" data-fv-widget=\"{id}\" style=\"{style}\"><p>{text}</p></div>"),
            "shape" => new HtmlString($"<div class=\"{cls}\" data-fv-widget=\"{id}\" style=\"{style}\"></div>"),
            _ when widget.Href.Length > 0 => new HtmlString($"<a class=\"{cls}\" data-fv-widget=\"{id}\" style=\"{style}\" href=\"{WebUtility.HtmlEncode(widget.Href)}\">{text}</a>"),
            _ => new HtmlString($"<div class=\"{cls}\" data-fv-widget=\"{id}\" style=\"{style}\">{text}</div>"),
        };
    }

    public static IHtmlContent PinCss(IPublishedContent page)
    {
        FuzulLayout layout = Parse(page);
        if (layout.Pins.Count == 0)
        {
            return new HtmlString("");
        }

        var css = new System.Text.StringBuilder();
        foreach ((string key, FuzulPin pin) in layout.Pins)
        {
            css.Append("[data-fv-pin=\"").Append(WebUtility.HtmlEncode(key)).Append("\"]{position:absolute!important;left:")
                .Append(Pct(pin.X)).Append(";top:").Append(Pct(pin.Y));
            if (pin.W > 0)
            {
                css.Append(";width:").Append(Pct(pin.W));
            }

            if (pin.H > 0)
            {
                css.Append(";height:").Append(Pct(pin.H));
            }

            css.Append(";z-index:").Append(pin.Z.ToString(CultureInfo.InvariantCulture)).Append(";margin:0;}");
        }

        return new HtmlString(css.ToString());
    }

    public static string Css(FuzulWidget widget)
    {
        var rules = new List<string>
        {
            "left:" + Pct(widget.X),
            "top:" + Pct(widget.Y),
            "width:" + Pct(widget.W),
            "height:" + Pct(widget.H),
            "z-index:" + widget.Z.ToString(CultureInfo.InvariantCulture),
        };
        if (widget.Bg.Length > 0)
        {
            rules.Add("background:" + widget.Bg);
        }

        if (widget.Color.Length > 0)
        {
            rules.Add("color:" + widget.Color);
        }

        if (widget.Size > 0)
        {
            rules.Add("font-size:" + widget.Size.ToString(CultureInfo.InvariantCulture) + "px");
        }

        if (widget.Radius > 0)
        {
            rules.Add("border-radius:" + widget.Radius.ToString(CultureInfo.InvariantCulture) + "px");
        }

        return string.Join(";", rules);
    }

    private static FuzulWidget? CleanWidget(FuzulWidget widget)
    {
        string type = Types.Contains(widget.Type) ? widget.Type : "";
        string sectionRaw = widget.Section ?? "";
        string idRaw = widget.Id ?? "";
        string section = SectionKey.IsMatch(sectionRaw) ? sectionRaw.ToLowerInvariant() : "";
        string id = PinKey.IsMatch(idRaw) ? idRaw : "";
        if (type.Length == 0 || section.Length == 0 || id.Length == 0)
        {
            return null;
        }

        return new FuzulWidget
        {
            Id = id,
            Section = section,
            Type = type,
            X = Clamp(widget.X, -5, 105),
            Y = Clamp(widget.Y, -5, 140),
            W = Clamp(widget.W, 2, 100),
            H = Clamp(widget.H, 2, 100),
            Z = Math.Clamp(widget.Z, 0, 50),
            Text = Clip(widget.Text, 200),
            Href = FuzulBuilder.SafeUrl(widget.Href),
            Bg = FuzulBuilder.Color(widget.Bg, ""),
            Color = FuzulBuilder.Color(widget.Color, ""),
            Size = Math.Clamp(widget.Size <= 0 ? 16 : widget.Size, 10, 96),
            Radius = Math.Clamp(widget.Radius, 0, 80),
            Src = FuzulBuilder.SafeUrl(widget.Src),
        };
    }

    private static FuzulPin? CleanPin(FuzulPin pin)
    {
        string sectionRaw = pin.Section ?? "";
        string section = SectionKey.IsMatch(sectionRaw) ? sectionRaw.ToLowerInvariant() : "";
        if (section.Length == 0)
        {
            return null;
        }

        return new FuzulPin
        {
            Section = section,
            X = Clamp(pin.X, -5, 105),
            Y = Clamp(pin.Y, -5, 140),
            W = Clamp(pin.W, 0, 100),
            H = Clamp(pin.H, 0, 100),
            Z = Math.Clamp(pin.Z, 0, 50),
        };
    }

    private static double Clamp(double value, double min, double max)
        => double.IsFinite(value) ? Math.Clamp(Math.Round(value, 2), min, max) : min;

    private static string Clip(string? value, int max)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "";
        }

        var cleaned = new string(value.Where(ch => !char.IsControl(ch)).ToArray()).Trim();
        return cleaned.Length <= max ? cleaned : cleaned[..max];
    }

    private static string Pct(double value) => value.ToString("0.##", CultureInfo.InvariantCulture) + "%";
}
