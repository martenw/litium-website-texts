using System;
using System.Collections.Generic;
using System.Linq;
using Litium.Foundation;
using Litium.Websites;
using Microsoft.Extensions.Logging;

namespace Litium.Accelerator.Definitions.WebsiteTexts
{
    public class WebsiteTextServiceImpl : WebsiteTextService
    {
        private readonly ILogger<WebsiteTextService> _logger;
        private readonly Solution _solution;
        private readonly WebsiteService _websiteService;

        public WebsiteTextServiceImpl(Solution solution, WebsiteService websiteService, ILogger<WebsiteTextService> logger)
        {
            _solution = solution;
            _websiteService = websiteService;
            _logger = logger;
        }

        private string GetTextKey(string prefix, string key, bool client)
        {
            var fullKey = $"{prefix}.{key}";
            if (client)
                // For client key, add js. as prefix and make the key lower-cased
                fullKey = $"js.{fullKey}".ToLower();
            return fullKey;
        }

        public override void CreateWebsiteTexts(IWebsiteTextSource textSource)
        {
            try
            {
                using (_solution.SystemToken.Use("WebsiteTextSetup.CreateWebsiteStrings"))
                {
                    var allWebsites = _websiteService.GetAll();

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

                                foreach (var textValue in text.Name)
                                {
                                    // If the string should be avaliable on the server, in other words generated as defined only
                                    if (text.ServerAvailable)
                                    {
                                        var textKey = GetTextKey(textSource.Prefix, text.Id, false);
                                        AddOrUpdateValue(textSource, website, textValue, textKey);
                                    }

                                    // If the string shold be avaliable on client, create another string with js.-prefix
                                    // this makes the string avaliable in clientscript from window.__litium.translation
                                    // See https://docs.litium.com/documentation/litium-accelerators/develop/architecture/accelerator-mvc
                                    if (text.ClientAvailable)
                                    {
                                        var textKey = GetTextKey(textSource.Prefix, text.Id, true);
                                        AddOrUpdateValue(textSource, website, textValue, textKey);
                                    }
                                }
                            }
                            catch (Exception exception)
                            {
                                _logger.LogError($"Error creating website text '{text?.Id}'", exception);
                            }

                        _websiteService.Update(website);
                    }
                }
            }
            catch (Exception exception)
            {
                _logger.LogError("Error creating website texts", exception);
            }
        }

        private static void AddOrUpdateValue(IWebsiteTextSource textSource, Website website, KeyValuePair<string, string> textValue, string textKey)
        {
            var textIsNew = !website.Texts.Keys.Contains(textKey);

            if (textIsNew || textSource.UpdateExistingTexts)
            {
                website.Texts.AddOrUpdateValue(textKey, textValue.Key, textValue.Value);
            }
        }

        public override void DeleteMissingWebsiteTexts(IWebsiteTextSource textSource)
        {
            try
            {
                using (_solution.SystemToken.Use("WebsiteTextSetup.DeleteMissingWebsiteStrings"))
                {
                    var allWebsites = _websiteService.GetAll();
                    foreach (var ws in allWebsites)
                    {
                        var website = ws.MakeWritableClone();
                        DeleteRemovedWebsiteTexts(textSource.Prefix, website, textSource.GetTexts());
                        _websiteService.Update(website);
                    }
                }
            }
            catch (Exception exception)
            {
                _logger.LogError("Error deleting website texts", exception);
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

                _logger.LogWarning($"Removing unused website text '{textItem}' from website {website.Id}");
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
}