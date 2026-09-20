using System.Net;
using Microsoft.AspNetCore.Html;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.UI.FuzulVentures;

public static class FuzulView
{
    public static string Text(IPublishedElement model, string alias, string fallback)
    {
        var value = model.Value<string>(alias);
        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }

    public static string MediaUrl(IPublishedElement model, string alias, string fallback)
    {
        var media = model.Value<MediaWithCrops>(alias);
        var url = media?.Url();
        if (string.IsNullOrWhiteSpace(url) == false)
        {
            return url;
        }

        return Text(model, alias, fallback);
    }

    public static IHtmlContent Html(IPublishedElement model, string alias, string fallback)
    {
        var encoded = model.Value<IHtmlEncodedString>(alias);
        var markup = encoded?.ToHtmlString();
        if (string.IsNullOrWhiteSpace(markup) == false)
        {
            return new HtmlString(markup);
        }

        var text = model.Value<string>(alias);
        if (string.IsNullOrWhiteSpace(text) == false)
        {
            return new HtmlString($"<p>{System.Net.WebUtility.HtmlEncode(text)}</p>");
        }

        return new HtmlString($"<p>{System.Net.WebUtility.HtmlEncode(fallback)}</p>");
    }

    public static IReadOnlyList<BlockListItem> Blocks(IPublishedElement model, string alias)
        => model.Value<BlockListModel>(alias)?.ToArray() ?? [];

    public static IReadOnlyList<Link> Links(IPublishedElement model, string alias)
        => model.Value<IEnumerable<Link>>(alias)?.Where(x => string.IsNullOrWhiteSpace(x.Url) == false || string.IsNullOrWhiteSpace(x.Name) == false).ToArray()
           ?? [];
}
