using System;
using System.Collections.Generic;

namespace Litium.Accelerator.Definitions.WebsiteTexts
{
    public static class WebsiteTextDefinitionExtensions
    {
        public static WebsiteTextDefinition AsWebsiteTextDefinition(this string id, string english, string swedish, IEnumerable<Guid> websiteIds = null, bool clientAvaliable = false, bool serverAvaliable = true)
        {
            return new WebsiteTextDefinition
            {
                Id = id,
                Name = new Dictionary<string, string>
                {
                    {"sv-SE", swedish}, {"en-US", english}
                },
                ClientAvailable = clientAvaliable,
                ServerAvailable = serverAvaliable,
                WebsiteIds = websiteIds ?? new List<Guid>()
            };
        }
    }
}