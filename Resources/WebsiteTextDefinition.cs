using System;
using System.Collections.Generic;

namespace Litium.Accelerator.Definitions.WebsiteTexts
{
    public class WebsiteTextDefinition
    {
        /// <summary>
        ///     Key used to get the translation in current language
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        ///     Dictionary with translations for the specified Id
        /// </summary>
        /// <example>
        ///     {"sv-SE", "Hej" }, {"en-US", "Hello" }
        /// </example>
        public IDictionary<string, string> Name { get; set; } = new Dictionary<string, string>();

        /// <summary>
        ///     System-IDs of the websites where this string will be generated/removed
        ///     If the list is empty the string will be implemented on all websites in the solution
        /// </summary>
        public IEnumerable<Guid> WebsiteIds { get; set; } = new List<Guid>();

        /// <summary>
        ///     Define if the text should be created server access (without js.-prefix)
        ///     (client access mean created a second time with js.-prefix)
        /// </summary>
        public bool ServerAvailable { get; set; } = true;

        /// <summary>
        ///     Define if the text should be created for client access (with js.-prefix)
        /// </summary>
        public bool ClientAvailable { get; set; }
    }
}