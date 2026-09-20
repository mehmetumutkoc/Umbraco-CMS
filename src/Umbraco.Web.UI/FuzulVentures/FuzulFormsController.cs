using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Website.Controllers;

namespace Umbraco.Cms.Web.UI.FuzulVentures;

public sealed class FuzulFormsController : SurfaceController
{
    private const long MaxUploadBytes = 10 * 1024 * 1024;
    private readonly IContentService _contentService;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<FuzulFormsController> _logger;

    public FuzulFormsController(
        IUmbracoContextAccessor umbracoContextAccessor,
        IUmbracoDatabaseFactory databaseFactory,
        ServiceContext services,
        AppCaches appCaches,
        IProfilingLogger profilingLogger,
        IPublishedUrlProvider publishedUrlProvider,
        IContentService contentService,
        IWebHostEnvironment environment,
        ILogger<FuzulFormsController> logger)
        : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
    {
        _contentService = contentService;
        _environment = environment;
        _logger = logger;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Apply(FuzulApplyFormModel model, Guid sectionKey)
    {
        IPublishedElement? section = ResolveSection(sectionKey, "fuzulApplySection");
        ValidatePdf(model.Presentation, nameof(model.Presentation), required: true);
        if (ModelState.IsValid == false)
        {
            return CurrentUmbracoPage();
        }

        var filePath = await SavePdfAsync(model.Presentation!);
        var saved = SaveSubmission(
            "apply",
            $"Startup: {model.StartupName}{Environment.NewLine}Website: {model.Website}{Environment.NewLine}Industry: {model.Industry}{Environment.NewLine}Founder: {model.FounderName} <{model.FounderEmail}>{Environment.NewLine}{model.Summary}",
            filePath);

        if (!saved)
        {
            ModelState.AddModelError("", "Başvuru kaydedilemedi. Lütfen tekrar deneyin.");
            return CurrentUmbracoPage();
        }

        SetSuccess(section, sectionKey, "FuzulApplySuccess", "Başvurunuz alındı.");
        return RedirectToCurrentUmbracoPage();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Contact(FuzulContactFormModel model, Guid sectionKey)
    {
        IPublishedElement? section = ResolveSection(sectionKey, "fuzulContactSection");
        ValidatePdf(model.Attachment, nameof(model.Attachment), required: false);
        if (ModelState.IsValid == false)
        {
            return CurrentUmbracoPage();
        }

        var filePath = model.Attachment is { Length: > 0 }
            ? await SavePdfAsync(model.Attachment)
            : null;

        var saved = SaveSubmission(
            "contact",
            $"Name: {model.FullName}{Environment.NewLine}Phone: {model.Phone}{Environment.NewLine}Email: {model.Email}",
            filePath);

        if (!saved)
        {
            ModelState.AddModelError("", "Mesaj kaydedilemedi. Lütfen tekrar deneyin.");
            return CurrentUmbracoPage();
        }

        SetSuccess(section, sectionKey, "FuzulContactSuccess", "Mesajınız gönderildi.");
        return RedirectToCurrentUmbracoPage();
    }

    private IPublishedElement? ResolveSection(Guid key, string alias)
    {
        if (CurrentPage is null)
        {
            ModelState.AddModelError("", "Form sayfası bulunamadı.");
            return null;
        }

        if (key == Guid.Empty && CurrentPage.ContentType.Alias == FuzulVenturesAliases.Home && !CurrentPage.Value<bool>("builderEnabled")) return null;
        var block = FuzulView.Blocks(CurrentPage, "pageSections").FirstOrDefault(x => x.Content.Key == key && x.Content.ContentType.Alias == alias);
        if (block is null || block.Settings?.Value<bool>("hidden") == true)
        {
            ModelState.AddModelError("", "Bu form artık kullanılamıyor. Sayfayı yenileyin.");
            return null;
        }

        return block.Content;
    }

    private void SetSuccess(IPublishedElement? section, Guid key, string legacyKey, string fallback)
    {
        if (section is null)
        {
            TempData[legacyKey] = fallback;
            return;
        }

        TempData["FuzulFormSuccessKey"] = key.ToString("N");
        TempData["FuzulFormSuccess"] = FuzulView.Text(section, "successMessage", fallback);
    }

    private void ValidatePdf(IFormFile? file, string key, bool required)
    {
        if (file is null || file.Length == 0)
        {
            if (required)
            {
                ModelState.AddModelError(key, "PDF dosyası gerekli.");
            }

            return;
        }

        if (file.Length > MaxUploadBytes)
        {
            ModelState.AddModelError(key, "Dosya 10 MB sınırını aşıyor.");
            return;
        }

        var extension = Path.GetExtension(file.FileName);
        if (string.Equals(extension, ".pdf", StringComparison.OrdinalIgnoreCase) == false
            || (string.IsNullOrWhiteSpace(file.ContentType) == false
                && file.ContentType.Contains("pdf", StringComparison.OrdinalIgnoreCase) == false
                && string.Equals(file.ContentType, "application/octet-stream", StringComparison.OrdinalIgnoreCase) == false))
        {
            ModelState.AddModelError(key, "Sadece PDF yükleyin.");
        }
    }

    private async Task<string> SavePdfAsync(IFormFile file)
    {
        var folder = Path.Combine(_environment.WebRootPath, "fuzul", "uploads");
        Directory.CreateDirectory(folder);
        var storedName = $"{Guid.NewGuid():N}.pdf";
        var path = Path.Combine(folder, storedName);
        await using FileStream stream = System.IO.File.Create(path);
        await file.CopyToAsync(stream);
        return $"/fuzul/uploads/{storedName}";
    }

    private bool SaveSubmission(string kind, string payload, string? filePath)
    {
        try
        {
            var page = CurrentPage is null ? null : FuzulBuilder.Site(CurrentPage);
            if (page is null)
            {
                return false;
            }

            Guid parentKey = page.Key;
            IContent? folder = _contentService.GetPagedChildren(page.Id, 0, 50, out _, propertyAliases: null, filter: null, ordering: null)
                .FirstOrDefault(x => x.ContentType.Alias == FuzulVenturesAliases.FormFolder);
            if (folder is not null)
            {
                parentKey = folder.Key;
            }

            var name = $"{kind} {DateTime.UtcNow:yyyy-MM-dd HH:mm}";
            var submission = _contentService.Create(name, parentKey, FuzulVenturesAliases.FormSubmission);
            submission.SetValue("kind", kind);
            submission.SetValue("payload", payload);
            if (string.IsNullOrWhiteSpace(filePath) == false)
            {
                submission.SetValue("filePath", filePath);
            }

            return _contentService.Save(submission).Success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not store a Fuzul form submission.");
            return false;
        }
    }
}
