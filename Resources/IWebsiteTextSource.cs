using System.Collections.Generic;

namespace Litium.Accelerator.Definitions.WebsiteTexts;

public interface IWebsiteTextSource
{
    string Prefix { get; }

    bool UpdateExistingTexts { get; }

    List<WebsiteTextDefinition> GetTexts();
}