using System;
using System.Collections.Generic;
using System.Text;
using CluedIn.Core.Crawling;

namespace CluedIn.ExternalSearch.Providers.Bolagsverket
{
    public class BolagsverketExternalSearchJobData : CrawlJobData
    {
        public BolagsverketExternalSearchJobData(IDictionary<string, object> configuration)
        {
        }

        public IDictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>();
        }
    }
}
