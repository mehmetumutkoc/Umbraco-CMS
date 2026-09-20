using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Web.UI.FuzulVentures;

[Route("fuzul/designer")]
public sealed class FuzulDesignerController : Controller
{
    private readonly IContentService _contentService;
    private readonly IWebHostEnvironment _environment;

    public FuzulDesignerController(IContentService contentService, IWebHostEnvironment environment)
    {
        _contentService = contentService;
        _environment = environment;
    }

    [HttpPost("save")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save([FromForm] Guid pageKey, [FromForm] string layout)
    {
        if (!await FuzulDesigner.CanDesignAsync(HttpContext, _environment))
        {
            return StatusCode(403, new { ok = false, error = "Tasarımı kaydetmek için backoffice oturumu gerekir." });
        }

        if (pageKey == Guid.Empty)
        {
            return BadRequest(new { ok = false, error = "Sayfa bulunamadı." });
        }

        if (string.IsNullOrWhiteSpace(layout) || layout.Length > FuzulDesigner.MaxLayoutChars)
        {
            return BadRequest(new { ok = false, error = "Düzen çok büyük veya boş." });
        }

        IContent? content = _contentService.GetById(pageKey);
        if (content is null || (content.ContentType.Alias != FuzulVenturesAliases.Home && content.ContentType.Alias != "fuzulPage"))
        {
            return NotFound(new { ok = false, error = "Bu sayfa tasarlanamaz." });
        }

        string sanitized = FuzulDesigner.Serialize(FuzulDesigner.Sanitize(layout));
        content.SetValue(FuzulDesigner.PropertyAlias, sanitized);
        var save = _contentService.Save(content);
        if (!save.Success)
        {
            return StatusCode(500, new { ok = false, error = "Kayıt başarısız." });
        }

        if (content.Published)
        {
            var publish = _contentService.Publish(content, ["*"]);
            if (!publish.Success)
            {
                return StatusCode(500, new { ok = false, error = "Kaydedildi ama yayınlanamadı. Backoffice’den yayınlayın." });
            }
        }

        return Json(new { ok = true, layout = sanitized });
    }
}
