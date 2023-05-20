using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CluedIn.Core;
using CluedIn.Core.Crawling;
using CluedIn.Core.Data.Relational;
using CluedIn.Core.ExternalSearch;
using CluedIn.Core.Providers;
using CluedIn.Core.Webhooks;
using CluedIn.ExternalSearch.Providers.Bolagsverket;
using CluedIn.Providers.Models;

namespace CluedIn.ExternalSearch.Providers.Bolagsverket.Provider
{
    public class BolagsverketSearchProviderProvider : ProviderBase, IExtendedProviderMetadata, IExternalSearchProviderProvider
    {
        public IExternalSearchProvider ExternalSearchProvider { get; }

        public BolagsverketSearchProviderProvider(ApplicationContext appContext) : base(appContext, GetMetaData())
        {
            ExternalSearchProvider = appContext.Container.ResolveAll<IExternalSearchProvider>().Single(n => n.Id == BolagsverketConstants.ProviderId);
        }

        private static IProviderMetadata GetMetaData()
        {
            return new ProviderMetadata
            {
                Id = BolagsverketConstants.ProviderId,
                Name = BolagsverketConstants.ProviderName,
                ComponentName = BolagsverketConstants.ComponentName,
                AuthTypes = new List<string>(),
                SupportsConfiguration = true,
                SupportsAutomaticWebhookCreation = false,
                SupportsWebHooks = false,
                Type = "Enricher"
            };
        }

        public override async Task<CrawlJobData> GetCrawlJobData(ProviderUpdateContext context, IDictionary<string, object> configuration, Guid organizationId, Guid userId,
            Guid providerDefinitionId)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            var result = new BolagsverketExternalSearchJobData(configuration);

            return await Task.FromResult(result);
        }

        public override Task<bool> TestAuthentication(ProviderUpdateContext context, IDictionary<string, object> configuration, Guid organizationId, Guid userId,
            Guid providerDefinitionId)
        {
            return Task.FromResult(true);
        }

        public override Task<ExpectedStatistics> FetchUnSyncedEntityStatistics(ExecutionContext context, IDictionary<string, object> configuration, Guid organizationId,
            Guid userId, Guid providerDefinitionId)
        {
            throw new NotImplementedException();
        }

        public override async Task<IDictionary<string, object>> GetHelperConfiguration(ProviderUpdateContext context, CrawlJobData jobData, Guid organizationId, Guid userId,
            Guid providerDefinitionId)
        {
            if (jobData is BolagsverketExternalSearchJobData result)
            {
                return await Task.FromResult(result.ToDictionary());
            }

            throw new InvalidOperationException(
                $"Unexpected data type for AcceptanceExternalSearchJobData, {jobData.GetType()}");
        }

        public override Task<IDictionary<string, object>> GetHelperConfiguration(ProviderUpdateContext context, CrawlJobData jobData, Guid organizationId, Guid userId,
            Guid providerDefinitionId, string folderId)
        {
            return GetHelperConfiguration(context, jobData, organizationId, userId, providerDefinitionId);
        }

        public override Task<AccountInformation> GetAccountInformation(ExecutionContext context, CrawlJobData jobData, Guid organizationId, Guid userId,
            Guid providerDefinitionId)
        {
            return Task.FromResult(new AccountInformation(providerDefinitionId.ToString(), providerDefinitionId.ToString()));
        }

        public override string Schedule(DateTimeOffset relativeDateTime, bool webHooksEnabled)
        {
            return $"{relativeDateTime.Minute} 0/23 * * *";
        }

        public override Task<IEnumerable<WebHookSignature>> CreateWebHook(ExecutionContext context, CrawlJobData jobData, IWebhookDefinition webhookDefinition,
            IDictionary<string, object> config)
        {
            throw new NotImplementedException();
        }

        public override Task<IEnumerable<WebhookDefinition>> GetWebHooks(ExecutionContext context)
        {
            throw new NotImplementedException();
        }

        public override Task DeleteWebHook(ExecutionContext context, CrawlJobData jobData, IWebhookDefinition webhookDefinition)
        {
            throw new NotImplementedException();
        }

        public override Task<CrawlLimit> GetRemainingApiAllowance(ExecutionContext context, CrawlJobData jobData, Guid organizationId, Guid userId,
            Guid providerDefinitionId)
        {
            if (jobData == null) throw new ArgumentNullException(nameof(jobData));
            return Task.FromResult(new CrawlLimit(-1, TimeSpan.Zero));
        }

        public override IEnumerable<string> WebhookManagementEndpoints(IEnumerable<string> ids)
        {
            throw new NotImplementedException();
        }

        public string Icon { get; } = BolagsverketConstants.Icon;
        public string Domain { get; } = BolagsverketConstants.Domain;
        public string About { get; } = BolagsverketConstants.About;
        public AuthMethods AuthMethods { get; } = BolagsverketConstants.AuthMethods;
        public IEnumerable<Control> Properties { get; } = BolagsverketConstants.Properties;
        public Guide Guide { get; } = BolagsverketConstants.Guide;
        public new IntegrationType Type { get; } = BolagsverketConstants.IntegrationType;
    }
}
