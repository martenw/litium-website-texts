using System;
using System.Collections.Generic;
using System.Linq;
using Litium.Runtime.DependencyInjection;
using Litium.Security;
using Litium.Websites;
using Microsoft.Extensions.Logging;

namespace Litium.Accelerator.Definitions.WebsiteTexts;

[Service]
public class WebsiteTextService(
    ILogger<WebsiteTextService> logger,
    WebsiteService websiteService,
    SecurityContextService securityContextService
)
{
    private static string GetTextKey(string prefix, string key, bool client)
    {
        var fullKey = $"{prefix}.{key}";
        if (client)
            // For client key, add js. as prefix and make the key lower-cased
            fullKey = $"js.{fullKey}".ToLower();
        return fullKey;
    }

    public void CreateWebsiteTexts(IWebsiteTextSource textSource)
    {
        try
        {
            using (securityContextService.ActAsSystem(nameof(WebsiteTextService)))
            {
                var allWebsites = websiteService.GetAll();

                foreach (var ws in allWebsites)
                {
                    var website = ws.MakeWritableClone();

                    foreach (var text in textSource.GetTexts())
                        try
                        {
                            if (text == null)
                                throw new Exception("Text is null");
                            if (text.Id == null)
                                throw new Exception("Text.Id cannot be null");

                            // If the text should be generated on specific websites only and current site is not among those then skip
                            if (text.WebsiteIds.Any() && !text.WebsiteIds.Any(w => website.SystemId.Equals(w)))
                                continue;

                            var serverTextKey = GetTextKey(textSource.Prefix, text.Id, false);
                            var clientTextKey = GetTextKey(textSource.Prefix, text.Id, true);

                            // If they don't exist here it's a first time insert, so then insert all cultures
                            var serverTextKeyInitialInsert = !website.Texts.Keys.Contains(serverTextKey);
                            var clientTextKeyInitialInsert = !website.Texts.Keys.Contains(clientTextKey);

                            foreach (var textValue in text.Name)
                            {
                                // If the string should be available on the server, in other words generated as defined only
                                if (text.ServerAvailable)
                                {
                                    AddOrUpdateValue(textSource, website, textValue, serverTextKey, serverTextKeyInitialInsert);
                                }

                                // If the string should be available on client, create another string with js.-prefix
                                // this makes the string available in client script from window.__litium.translation
                                // See https://docs.litium.com/documentation/litium-accelerators/develop/architecture/accelerator-mvc
                                if (text.ClientAvailable)
                                {
                                    AddOrUpdateValue(textSource, website, textValue, clientTextKey, clientTextKeyInitialInsert);
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            logger.LogError(exception, "Error creating website text '{TextId}'", text?.Id);
                        }

                    websiteService.Update(website);
                }
            }
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error creating website texts");
        }
    }

    private static void AddOrUpdateValue(IWebsiteTextSource textSource, Website website, KeyValuePair<string, string> textValue, string textKey, bool initialInsert)
    {
        if (initialInsert || textSource.UpdateExistingTexts)
        {
            website.Texts.AddOrUpdateValue(textKey, textValue.Key, textValue.Value);
        }
    }

    public void DeleteMissingWebsiteTexts(IWebsiteTextSource textSource)
    {
        try
        {
            using (securityContextService.ActAsSystem(nameof(WebsiteTextService)))
            {
                var allWebsites = websiteService.GetAll();
                foreach (var ws in allWebsites)
                {
                    var website = ws.MakeWritableClone();
                    DeleteRemovedWebsiteTexts(textSource.Prefix, website, textSource.GetTexts());
                    websiteService.Update(website);
                }
            }
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error deleting website texts");
        }
    }

    private void DeleteRemovedWebsiteTexts(string prefix, Website website, List<WebsiteTextDefinition> texts)
    {
        foreach (var textItem in website.Texts.GetTextItems().Select(ti => ti.Key).Distinct())
        {
            // Do not delete any text that is not using the defined prefix
            var isPrefixMatch = textItem.IndexOf($"{prefix}.", StringComparison.InvariantCultureIgnoreCase) >= 0;
            if (!isPrefixMatch)
                continue;

            // Keep any text that is still in the coded definitions
            var isStillDefined = TextIsInDefinitions(textItem, texts, prefix, website);
            if (isStillDefined)
                continue;

            logger.LogWarning("Removing unused website text '{WebsiteTextKey}' from website {WebsiteId}", textItem, website.Id);
            website.Texts.RemoveValue(textItem);
        }
    }

    private bool TextIsInDefinitions(string text, List<WebsiteTextDefinition> definitions, string prefix, Website website)
    {
        var isClientText = text.StartsWith("js.", StringComparison.InvariantCultureIgnoreCase);

        var isInDefinitions = definitions
            // First check if the string key is a match
            .Where(def => GetTextKey(prefix, def.Id, isClientText).Equals(text))
            // Then check if the definition is alse in the current site
            .Any(def => !def.WebsiteIds.Any() || def.WebsiteIds.Any(id => id.Equals(website.SystemId)));

        return isInDefinitions;
    }
}