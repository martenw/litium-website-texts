using System;
using System.Collections.Generic;

namespace Litium.Accelerator.Definitions.WebsiteTexts
{
    public static class WebsiteTextDefinitionExtensions
    {
        public static WebsiteTextDefinition AsWebsiteTextDefinition(this string id, string english, string swedish, IEnumerable<Guid> websiteIds = null, bool clientAvailable = false, bool serverAvailable = true)
        {
            return new WebsiteTextDefinition
            {
                Id = id,
                Name = new Dictionary<string, string>
                {
                    {"sv-SE", swedish}, {"en-US", english}
                },
                ClientAvailable = clientAvailable,
                ServerAvailable = serverAvailable,
                WebsiteIds = websiteIds ?? new List<Guid>()
            };
        }
    }
}