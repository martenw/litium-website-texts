using Litium.Owin.Lifecycle;

namespace Litium.Accelerator.Definitions.WebsiteTexts
{
    public class WebsiteTextSetup : IStartupTask
    {
        private readonly WebsiteTextService _websiteTextService;
        private readonly IWebsiteTextSource _websiteTextSource;

        public WebsiteTextSetup(WebsiteTextService websiteTextService, IWebsiteTextSource websiteTextSource)
        {
            _websiteTextService = websiteTextService;
            _websiteTextSource = websiteTextSource;
        }

        public void Start()
        {
            // All definitions matching above prefix that are no longer in coded definitions are deleted from database
            _websiteTextService.DeleteMissingWebsiteTexts(_websiteTextSource);

            // Add any new text in coded definitions to database
            _websiteTextService.CreateWebsiteTexts(_websiteTextSource);
        }
    }
}