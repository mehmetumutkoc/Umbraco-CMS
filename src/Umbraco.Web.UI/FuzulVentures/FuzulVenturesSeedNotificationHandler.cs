using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Web.UI.FuzulVentures;

public sealed class FuzulVenturesSeedNotificationHandler : INotificationAsyncHandler<UmbracoApplicationStartedNotification>
{
    private static readonly SemaphoreSlim Gate = new(1, 1);

    private readonly FuzulVenturesCmsSetup _cmsSetup;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly ILogger<FuzulVenturesSeedNotificationHandler> _logger;
    private readonly IRuntimeState _runtimeState;
    private readonly ITemplateService _templateService;

    public FuzulVenturesSeedNotificationHandler(
        IRuntimeState runtimeState,
        ITemplateService templateService,
        FuzulVenturesCmsSetup cmsSetup,
        IHostEnvironment hostEnvironment,
        ILogger<FuzulVenturesSeedNotificationHandler> logger)
    {
        _runtimeState = runtimeState;
        _templateService = templateService;
        _cmsSetup = cmsSetup;
        _hostEnvironment = hostEnvironment;
        _logger = logger;
    }

    public async Task HandleAsync(UmbracoApplicationStartedNotification notification, CancellationToken cancellationToken)
    {
        if (_runtimeState.Level != RuntimeLevel.Run)
        {
            return;
        }

        await Gate.WaitAsync(cancellationToken);
        try
        {
            ITemplate template = await EnsureTemplateAsync();
            await _cmsSetup.EnsureAsync(template);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fuzul Ventures site seed failed.");
        }
        finally
        {
            Gate.Release();
        }
    }

    private async Task<ITemplate> EnsureTemplateAsync()
    {
        ITemplate? existing = await _templateService.GetAsync(FuzulVenturesAliases.Template);
        if (existing is not null)
        {
            return existing;
        }

        var viewPath = Path.Combine(_hostEnvironment.ContentRootPath, "Views", "fuzulHome.cshtml");
        var razor = System.IO.File.Exists(viewPath)
            ? await System.IO.File.ReadAllTextAsync(viewPath)
            : "@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage\n@{ Layout = null; }";

        Attempt<ITemplate, TemplateOperationStatus> created = await _templateService.CreateAsync(
            "Fuzul Home",
            FuzulVenturesAliases.Template,
            razor,
            Constants.Security.SuperUserKey);

        if (created.Success == false || created.Result is null)
        {
            throw new InvalidOperationException($"Could not create the Fuzul Home template: {created.Status}");
        }

        return created.Result;
    }
}
