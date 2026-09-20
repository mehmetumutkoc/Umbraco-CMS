using System.Globalization;
using System.Text.RegularExpressions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.UI.FuzulVentures;

public sealed record FuzulSectionModel(BlockListItem Block, IPublishedContent Page, IPublishedContent Site, string Anchor)
{
    public IPublishedElement Content => Block.Content;
    public string Key => Content.Key.ToString("N");
    public string Class => FuzulBuilder.SectionClass(Block);
    public string Style => FuzulBuilder.SectionStyle(Block);
    public string Text(string alias, string fallback = "") => FuzulView.Text(Content, alias, fallback);
}

public static class FuzulBuilder
{
    public static readonly IReadOnlyDictionary<string, string> Templates = new Dictionary<string, string>
    {
        ["fuzulHeroSection"] = "Hero", ["fuzulTextSection"] = "Text", ["fuzulSectorsSection"] = "Sectors",
        ["fuzulProcessSection"] = "Process", ["fuzulCommitteeSection"] = "Committee", ["fuzulApplySection"] = "Form",
        ["fuzulContactSection"] = "Form", ["fuzulImageTextSection"] = "ImageText", ["fuzulCtaSection"] = "Cta",
        ["fuzulCardsSection"] = "Cards", ["fuzulStatsSection"] = "Cards", ["fuzulFaqSection"] = "Faq",
        ["fuzulCanvasSection"] = "Canvas",
    };

    public static IPublishedContent Site(IPublishedContent page)
        => page.AncestorsOrSelf().FirstOrDefault(x => x.ContentType.Alias == FuzulVenturesAliases.Home) ?? page;

    public static string SafeUrl(string? value, string fallback = "")
    {
        if (string.IsNullOrWhiteSpace(value) || value.Any(char.IsControl) || value.Contains('\\')) return fallback;
        if (value.StartsWith('#') || (value.StartsWith('/') && !value.StartsWith("//", StringComparison.Ordinal))) return value;
        return Uri.TryCreate(value, UriKind.Absolute, out Uri? uri) && uri.Scheme is "http" or "https" or "mailto" or "tel" ? value : fallback;
    }

    public static string SiteLink(Link link, IPublishedContent site)
        => link.Url?.StartsWith('#') == true ? site.Url() + link.Url : SafeUrl(link.Url);

    public static string CssImage(string? value)
    {
        string url = SafeUrl(value);
        if (url.Length == 0 || url.StartsWith('#') || url.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase) || url.StartsWith("tel:", StringComparison.OrdinalIgnoreCase)) return "none";
        return "url('" + url.Replace("'", "%27", StringComparison.Ordinal).Replace("\"", "%22", StringComparison.Ordinal).Replace("<", "%3C", StringComparison.Ordinal).Replace(">", "%3E", StringComparison.Ordinal) + "')";
    }

    public static string Color(string? value, string fallback)
        => value is not null && Regex.IsMatch(value, "^#[0-9a-fA-F]{6}$", RegexOptions.CultureInvariant) ? value : fallback;

    public static int Number(string? value, int fallback, int min, int max)
        => int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int number) ? Math.Clamp(number, min, max) : fallback;

    public static string Font(string? value) => value switch
    {
        "Syne" => "'Syne',sans-serif", "DM Sans" => "'DM Sans',sans-serif", "Arial" => "Arial,sans-serif",
        "Georgia" => "Georgia,serif", "System" => "system-ui,sans-serif", _ => "'DM Sans',sans-serif",
    };

    public static string Language(string? value)
        => value is not null && Regex.IsMatch(value, "^[a-zA-Z]{2,3}(-[a-zA-Z0-9]{2,8})*$", RegexOptions.CultureInvariant) ? value : "tr";

    public static string UniqueAnchor(BlockListItem block, ISet<string> used)
    {
        string requested = block.Settings?.Value<string>("anchor") ?? "";
        string basis = Regex.Replace(requested.Trim(), "[^a-zA-Z0-9_-]", "-", RegexOptions.CultureInvariant).Trim('-');
        if (basis.Length == 0) basis = "section-" + block.Content.Key.ToString("N");
        string candidate = basis;
        for (var suffix = 2; !used.Add(candidate); suffix++) candidate = basis + "-" + suffix.ToString(CultureInfo.InvariantCulture);
        return candidate;
    }

    public static string Theme(IPublishedContent site)
    {
        string Text(string alias) => site.Value<string>(alias) ?? "";
        var rules = new List<string>
        {
            "--fv-accent:" + Color(Text("accentColor"), "#64cdd2"),
            "--fv-bg:" + Color(Text("backgroundColor"), "#081d34"),
            "--fv-heading:" + Color(Text("headingColor"), "#ffffff"),
            "--fv-text:" + Color(Text("bodyColor"), "#8fa8bd"),
            "--fv-surface:" + Color(Text("surfaceColor"), "#102b44"),
            "--fv-heading-font:" + Font(string.IsNullOrWhiteSpace(Text("headingFont")) ? "Syne" : Text("headingFont")),
            "--fv-body-font:" + Font(Text("bodyFont")),
            "--fv-width:" + Number(Text("contentWidth"), 1320, 800, 1800) + "px",
            "--fv-font-size:" + Number(Text("baseFontSize"), 16, 12, 24) + "px",
            "--fv-hero-size:" + Number(Text("headingSize"), 90, 32, 140) + "px",
            "--fv-radius:" + Number(Text("cornerRadius"), 0, 0, 48) + "px",
            "--fv-gap:" + Number(Text("sectionGap"), 80, 0, 200) + "px",
            "--fv-logo-width:" + Number(Text("logoWidth"), 220, 80, 400) + "px",
        };
        string bodyImage = CssImage(FuzulView.MediaUrl(site, "bodyBackground", ""));
        if (bodyImage != "none")
        {
            rules.Add("--fv-body-image:" + bodyImage);
        }

        string footerImage = CssImage(FuzulView.MediaUrl(site, "footerBackground", ""));
        if (footerImage != "none")
        {
            rules.Add("--fv-footer-image:" + footerImage);
        }

        return string.Join(";", rules);
    }

    public static string SectionClass(BlockListItem block)
    {
        IPublishedElement? settings = block.Settings;
        var parts = new List<string>();
        if (settings?.Value<string>("alignment") is "Orta")
        {
            parts.Add("fv-align-center");
        }
        else if (settings?.Value<string>("alignment") is "Sağ")
        {
            parts.Add("fv-align-right");
        }

        if (settings?.Value<string>("spacing") is "Yok")
        {
            parts.Add("fv-space-none");
        }
        else if (settings?.Value<string>("spacing") is "Dar")
        {
            parts.Add("fv-space-small");
        }
        else if (settings?.Value<string>("spacing") is "Geniş")
        {
            parts.Add("fv-space-large");
        }

        if (settings?.Value<string>("width") is "Dar")
        {
            parts.Add("fv-width-narrow");
        }
        else if (settings?.Value<string>("width") is "Tam genişlik")
        {
            parts.Add("fv-width-full");
        }

        if (settings?.Value<bool>("hideMobile") == true)
        {
            parts.Add("fv-hide-mobile");
        }

        if (settings?.Value<bool>("hideDesktop") == true)
        {
            parts.Add("fv-hide-desktop");
        }

        return string.Join(" ", parts);
    }

    public static string SectionStyle(BlockListItem block)
    {
        IPublishedElement? settings = block.Settings;
        if (settings is null) return "";
        var rules = new List<string>();
        string background = Color(settings.Value<string>("backgroundColor"), "");
        string text = Color(settings.Value<string>("textColor"), "");
        if (background.Length > 0) rules.Add("background-color:" + background);
        if (text.Length > 0) { rules.Add("--fv-text:" + text); rules.Add("--fv-heading:" + text); }
        string image = CssImage(FuzulView.MediaUrl(settings, "backgroundImage", ""));
        if (image != "none") rules.Add("background-image:" + image);
        return string.Join(";", rules);
    }
}
