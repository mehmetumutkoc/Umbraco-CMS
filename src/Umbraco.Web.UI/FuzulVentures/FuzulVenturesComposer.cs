using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Web.UI.FuzulVentures;

public sealed class FuzulVenturesComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddTransient<FuzulVenturesCmsSetup>();
        builder.AddNotificationAsyncHandler<UmbracoApplicationStartedNotification, FuzulVenturesSeedNotificationHandler>();
    }
}
