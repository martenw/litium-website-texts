using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Litium.Runtime;

namespace Litium.Accelerator.Definitions.WebsiteTexts;

[UsedImplicitly]
[Autostart]
public class WebsiteTextSetup(WebsiteTextService websiteTextService, IWebsiteTextSource websiteTextSource) : IAsyncAutostart
{
    public ValueTask StartAsync(CancellationToken cancellationToken)
    {
        // All definitions matching above prefix that are no longer in coded definitions are deleted from database
        websiteTextService.DeleteMissingWebsiteTexts(websiteTextSource);

        // Add any new text in coded definitions to database
        websiteTextService.CreateWebsiteTexts(websiteTextSource);

        return ValueTask.CompletedTask;
    }
}