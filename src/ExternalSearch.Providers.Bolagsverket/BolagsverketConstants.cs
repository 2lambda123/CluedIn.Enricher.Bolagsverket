using System;
using System.Collections.Generic;
using System.Text;
using CluedIn.Core.Data.Relational;
using CluedIn.Core.Providers;

namespace CluedIn.ExternalSearch.Providers.Bolagsverket
{
    public static class BolagsverketConstants
    {
        public const string ComponentName = "Bolagsverket";
        public const string ProviderName = "Bolagsverket";
        public static readonly Guid ProviderId = Guid.Parse("C7DDBEA4-D5A2-4F25-B2A0-EBFD36D2E8D6");

        public static string About { get; set; } = "Bolagsverket";
        public static string Icon { get; set; } = "Resources.duckduckgo.svg";
        public static string Domain { get; set; } = "N/A";

        public static AuthMethods AuthMethods { get; set; } = new AuthMethods { token = new List<Control>()};

        public static IEnumerable<Control> Properties { get; set; } = new List<Control>()
        {
            // NOTE: Leaving this commented as an example - BF
            //new()
            //{
            //    displayName = "Some Data",
            //    type = "input",
            //    isRequired = true,
            //    name = "someData"
            //}
        };

        public static Guide Guide { get; set; } = null;
        public static IntegrationType IntegrationType { get; set; } = IntegrationType.Enrichment;
    }
}
