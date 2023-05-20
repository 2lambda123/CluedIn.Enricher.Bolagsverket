// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DuckDuckGoExternalSearchProvider.cs" company="Clued In">
//   Copyright (c) 2018 Clued In. All rights reserved.
// </copyright>
// <summary>
//   Implements the duck go external search provider class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using CluedIn.Core;
using CluedIn.Core.Data;
using CluedIn.Core.Data.Parts;
using CluedIn.Core.Data.Relational;
using CluedIn.Core.ExternalSearch;
using CluedIn.Core.Providers;
using CluedIn.Crawling.Helpers;
using CluedIn.ExternalSearch.Filters;
using CluedIn.ExternalSearch.Providers.Bolagsverket;
using CluedIn.ExternalSearch.Providers.Bolagsverket.Vocabularies;
using DomainNameParser;
using Newtonsoft.Json;
using RestSharp;
using EntityType = CluedIn.Core.Data.EntityType;

namespace CluedIn.ExternalSearch.Providers.Bolagsverket
{
    /// <summary>A duck go external search provider.</summary>
    /// <seealso cref="T:CluedIn.ExternalSearch.ExternalSearchProviderBase"/>
    public class BolagsverketExternalSearchProvider : ExternalSearchProviderBase, IExtendedEnricherMetadata, IConfigurableExternalSearchProvider
    {

        private static readonly EntityType[] AcceptedEntityTypes = { "/Organization" };


        /**********************************************************************************************************
         * CONSTRUCTORS
         **********************************************************************************************************/


        /// <summary>
        /// Initializes a new instance of the <see cref="BolagsverketExternalSearchProvider" /> class.
        /// </summary>
        public BolagsverketExternalSearchProvider()
            : base(BolagsverketConstants.ProviderId, AcceptedEntityTypes)
        {
        }

        /**********************************************************************************************************
         * METHODS
         **********************************************************************************************************/


        public override IEnumerable<IExternalSearchQuery> BuildQueries(ExecutionContext context, IExternalSearchRequest request)
        {
            foreach (var externalSearchQuery in InternalBuildQueries(context, request))
            {
                yield return externalSearchQuery;
            }
        }

        /// <summary>Builds the queries.</summary>
        /// <param name="context">The context.</param>
        /// <param name="request">The request.</param>
        /// <returns>The search queries.</returns>
        private IEnumerable<IExternalSearchQuery> InternalBuildQueries(ExecutionContext context, IExternalSearchRequest request)
        {
            if (!this.Accepts(request.EntityMetaData.EntityType))
                yield break;

            var existingResults = request.GetQueryResults<Organization>(this).Where(r => r.Data.organisationslista.First().identitetsbeteckning.organisationsnummer != null).ToList();

            // Query Input
            var entityType = request.EntityMetaData.EntityType;
            var companyId = request.QueryParameters.GetValue<string, HashSet<string>>("organization.identitetsbeteckning", new HashSet<string>());

            if (companyId != null)
            {
                var values = companyId.Select(NameNormalization.Normalize).ToHashSetEx();

                foreach (var value in values)
                    yield return new ExternalSearchQuery(this, entityType, ExternalSearchQueryParameter.Identifier, value);
            }
        }
        public override IEnumerable<IExternalSearchQueryResult> ExecuteSearch(ExecutionContext context, IExternalSearchQuery query)
        {
            var apiKey = this.TokenProvider.ApiToken;

            foreach (var externalSearchQueryResult in InternalExecuteSearch(query, apiKey)) yield return externalSearchQueryResult;
        }
        /// <summary>Executes the search.</summary>
        /// <param name="context">The context.</param>
        /// <param name="query">The query.</param>
        /// <returns>The results.</returns>
        private IEnumerable<IExternalSearchQueryResult> InternalExecuteSearch(IExternalSearchQuery query, string apiKey)
        {
            var id = query.QueryParameters[ExternalSearchQueryParameter.Identifier].FirstOrDefault();

            if (string.IsNullOrEmpty(id))
                yield break;

            var token = "";

            {
                var client = new RestClient("https://portal.api.bolagsverket.se");

                var request = new RestRequest("oauth2/token", Method.POST);
                request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
                request.AddHeader("Authorization", "Basic  " + apiKey);
                request.AddHeader("Access-Control-Allow-Origin", "*");

                request.AddJsonBody(new PostBody() { identitetsbeteckning = id, organisationInformationsmangd = new string[] { "FIRMATECKNING" } });
                token = client.Execute<TokenResponse>(request).Data.token;
            }

            {
                var client = new RestClient("https://gw.api.bolagsverket.se");

                var request = new RestRequest("foretagsinformation/v2/organisationer", Method.POST);
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", "Bearer " + token);
                request.AddJsonBody(new PostBody() { identitetsbeteckning = id, organisationInformationsmangd = new string[] { "FIRMATECKNING" } });
                var responseData = client.Execute<Organization>(request);

                if (responseData.Data != null)
                    yield return new ExternalSearchQueryResult<Organization>(query, responseData.Data);
                else
                    yield break;
            }
        }

        /// <summary>Builds the clues.</summary>
        /// <param name="context">The context.</param>
        /// <param name="query">The query.</param>
        /// <param name="result">The result.</param>
        /// <param name="request">The request.</param>
        /// <returns>The clues.</returns>
        public override IEnumerable<Clue> BuildClues(ExecutionContext context, IExternalSearchQuery query, IExternalSearchQueryResult result, IExternalSearchRequest request)
        {
            var resultItem = result.As<Organization>();

            var code = new EntityCode("/Organization", CodeOrigin.CluedIn.CreateSpecific("bolagsverkat"), resultItem.Data.organisationslista.First().identitetsbeteckning.organisationsnummer);

            var clue = new Clue(code, context.Organization);

            clue.Data.OriginProviderDefinitionId = this.Id;

            this.PopulateMetadata(clue.Data.EntityData, resultItem);

            yield return clue;
        }

        /// <summary>Gets the primary entity metadata.</summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        /// <param name="request">The request.</param>
        /// <returns>The primary entity metadata.</returns>
        public override IEntityMetadata GetPrimaryEntityMetadata(ExecutionContext context, IExternalSearchQueryResult result, IExternalSearchRequest request)
        {
            var resultItem = result.As<Organization>();


            return this.CreateMetadata(resultItem);
        }

        /// <summary>Gets the preview image.</summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        /// <param name="request">The request.</param>
        /// <returns>The preview image.</returns>
        public override IPreviewImage GetPrimaryEntityPreviewImage(ExecutionContext context, IExternalSearchQueryResult result, IExternalSearchRequest request)
        {
            var resultItem = result.As<Organization>();



            return null;
        }

        /// <summary>Creates the metadata.</summary>
        /// <param name="resultItem">The result item.</param>
        /// <returns>The metadata.</returns>
        private IEntityMetadata CreateMetadata(IExternalSearchQueryResult<Organization> resultItem)
        {
            var metadata = new EntityMetadataPart();

            this.PopulateMetadata(metadata, resultItem);

            return metadata;
        }

        /// <summary>Populates the metadata.</summary>
        /// <param name="metadata">The metadata.</param>
        /// <param name="resultItem">The result item.</param>
        private void PopulateMetadata(IEntityMetadata metadata, IExternalSearchQueryResult<Organization> resultItem)
        {
            var code = new EntityCode("/Organization", CodeOrigin.CluedIn.CreateSpecific("bolagsverkat"), resultItem.Data.organisationslista.First().identitetsbeteckning.organisationsnummer);

            metadata.EntityType = "/Organization";
            metadata.Name = resultItem.Data.organisationslista.First().identitetsbeteckning.gdNummer;
            metadata.OriginEntityCode = code;

            metadata.Properties[BolagsverketVocabulary.Organization.Rerksamhetsbeskrivning] = resultItem.Data.organisationslista.First().verksamhetsbeskrivning.PrintIfAvailable();
            metadata.Properties[BolagsverketVocabulary.Organization.RakenskapsarAvslutas] = resultItem.Data.organisationslista.First().rakenskapsar.rakenskapsarAvslutas.PrintIfAvailable();

            metadata.Codes.Add(code);
        }

        /// <summary>
        /// Formarts the label so it fits the style of the properties (e.g. "Company type" -> "companyType")
        /// </summary>
        /// <param name="label">The label to format</param>
        /// <returns>The formatted label</returns>
        private static string FormatLabelToProperty(string label)
        {
            return String.Join("", label.Split(' ').Select((x, i) => i == 0 ? x.ToLower() : FirstCharacterToUpper(x)));
        }

        /// <summary>
        /// Capitalizes the first character in the string
        /// </summary>
        /// <param name="text">The text that should be capitalized</param>
        /// <returns>The string with the first character capitalized</returns>
        private static string FirstCharacterToUpper(string text)
        {
            return $"{char.ToUpper(text[0])}{text.Substring(1)}";
        }

        /// <summary>
        /// Joins the properties of a list into a string
        /// </summary>
        /// <typeparam name="T">The object</typeparam>
        /// <param name="items">The list of objects to be joined</param>
        /// <param name="property">The property that should be joined</param>
        /// <returns>A comma separated string containing the properties</returns>
        private static string JoinValues<T>(List<T> items, Func<T, string> property, string separator = ";")
        {
            if (items != null && items.Any())
            {
                return String.Join(separator, items.Where(x => !String.IsNullOrEmpty(property(x))).ToList().ConvertAll(x => property(x)));
            }

            return null;
        }

        public string Icon { get; } = BolagsverketConstants.Icon;
        public string Domain { get; } = BolagsverketConstants.Domain;
        public string About { get; } = BolagsverketConstants.About;
        public AuthMethods AuthMethods { get; } = null;
        public IEnumerable<Control> Properties { get; } = null;
        public Guide Guide { get; } = null;
        public IntegrationType Type { get; } = IntegrationType.Cloud;

        public IEnumerable<EntityType> Accepts(IDictionary<string, object> config, IProvider provider)
        {
            return AcceptedEntityTypes;
        }

        public IEnumerable<IExternalSearchQuery> BuildQueries(ExecutionContext context, IExternalSearchRequest request, IDictionary<string, object> config,
            IProvider provider)
        {
            return BuildQueries(context, request);
        }

        public IEnumerable<IExternalSearchQueryResult> ExecuteSearch(ExecutionContext context, IExternalSearchQuery query, IDictionary<string, object> config, IProvider provider)
        {
            foreach (var externalSearchQueryResult in ExecuteSearch(context, query)) yield return externalSearchQueryResult;
        }

        public IEnumerable<Clue> BuildClues(ExecutionContext context, IExternalSearchQuery query, IExternalSearchQueryResult result,
            IExternalSearchRequest request, IDictionary<string, object> config, IProvider provider)
        {
            return BuildClues(context, query, result, request);
        }

        public IEntityMetadata GetPrimaryEntityMetadata(ExecutionContext context, IExternalSearchQueryResult result,
            IExternalSearchRequest request, IDictionary<string, object> config, IProvider provider)
        {
            return GetPrimaryEntityMetadata(context, result, request);
        }

        public IPreviewImage GetPrimaryEntityPreviewImage(ExecutionContext context, IExternalSearchQueryResult result,
            IExternalSearchRequest request, IDictionary<string, object> config, IProvider provider)
        {
            return GetPrimaryEntityPreviewImage(context, result, request);
        }
    }

    public class PostBody
    {
        public string identitetsbeteckning { get; set; }
        public string[] organisationInformationsmangd { get; set; }
    }


    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Aktiegranser
    {
        public AktiekapitalGranser aktiekapitalGranser { get; set; }
        public AntalAktierGranser antalAktierGranser { get; set; }
        public string valuta { get; set; }
    }

    public class Aktieinformation
    {
        public Aktiekapital aktiekapital { get; set; }
        public int antalAktier { get; set; }
        public Aktiegranser aktiegranser { get; set; }
        public List<Aktieslag> aktieslag { get; set; }
        public string fritext { get; set; }
    }

    public class Aktiekapital
    {
        public int belopp { get; set; }
        public string valuta { get; set; }
    }

    public class AktiekapitalGranser
    {
        public int lagst { get; set; }
        public int hogst { get; set; }
    }

    public class Aktieslag
    {
        public string namn { get; set; }
        public int antal { get; set; }
        public AktieslagGranser aktieslagGranser { get; set; }
        public string rostvarde { get; set; }
    }

    public class AktieslagGranser
    {
        public int lagst { get; set; }
        public int hogst { get; set; }
    }

    public class AntalAktierGranser
    {
        public int lagst { get; set; }
        public int hogst { get; set; }
    }

    public class Firmateckning
    {
        public string klartext { get; set; }
    }

    public class ForetagsnamnPaFrammandeSprak
    {
        public Organisationsnamn organisationsnamn { get; set; }
        public string registreringsdatum { get; set; }
        public string verksamhetsbeskrivning { get; set; }
        public string beslutAvStamma { get; set; }
        public string beslutstyp { get; set; }
    }

    public class Funktionarer
    {
        public List<Funktionarsroller> funktionarsroller { get; set; }
        public Personnamn personnamn { get; set; }
        public Organisationsnamn organisationsnamn { get; set; }
        public Identitetsbeteckning identitetsbeteckning { get; set; }
        public Postadress postadress { get; set; }
    }

    public class Funktionarsroller
    {
        public string kod { get; set; }
        public string klartext { get; set; }
    }

    public class Hemvistkommun
    {
        public string typ { get; set; }
        public LanForHemvistkommun lanForHemvistkommun { get; set; }
        public Kommun kommun { get; set; }
    }

    public class Identitetsbeteckning
    {
        public string organisationsnummer { get; set; }
        public string personnummer { get; set; }
        public string samordningsnummer { get; set; }
        public string gdNummer { get; set; }
        public string utlandskIdentitetsbeteckning { get; set; }
        public string fodelsedatum { get; set; }
    }

    public class Kommun
    {
        public string kod { get; set; }
        public string klartext { get; set; }
    }

    public class Land
    {
        public string kod { get; set; }
        public string klartext { get; set; }
    }

    public class LanForHemvistkommun
    {
        public string kod { get; set; }
        public string klartext { get; set; }
    }

    public class Organisationsadresser
    {
        public Postadress postadress { get; set; }
        public string epostadress { get; set; }
    }

    public class Organisationsdatum
    {
        public string registreringsdatum { get; set; }
        public string bildatDatum { get; set; }
    }

    public class Organisationsform
    {
        public string kod { get; set; }
        public string klartext { get; set; }
    }

    public class Organisationslistum
    {
        public Identitetsbeteckning identitetsbeteckning { get; set; }
        public int namnskyddslopnummer { get; set; }
        public Organisationsnamn organisationsnamn { get; set; }
        public Organisationsform organisationsform { get; set; }
        public List<Organisationsstatusar> organisationsstatusar { get; set; }
        public Organisationsdatum organisationsdatum { get; set; }
        public Hemvistkommun hemvistkommun { get; set; }
        public Rakenskapsar rakenskapsar { get; set; }
        public string verksamhetsbeskrivning { get; set; }
        public Organisationsadresser organisationsadresser { get; set; }
        public Firmateckning firmateckning { get; set; }
        public List<SarskildaForetagsnamn> sarskildaForetagsnamn { get; set; }
        public List<ForetagsnamnPaFrammandeSprak> foretagsnamnPaFrammandeSprak { get; set; }
        public List<Funktionarer> funktionarer { get; set; }
        public Aktieinformation aktieinformation { get; set; }
    }

    public class Organisationsnamn
    {
        public string typ { get; set; }
        public string namn { get; set; }
    }

    public class Organisationsstatusar
    {
        public string kod { get; set; }
        public string klartext { get; set; }
        public string datum { get; set; }
    }

    public class Personnamn
    {
        public string fornamn { get; set; }
        public string efternamn { get; set; }
    }

    public class Postadress
    {
        public string coAdress { get; set; }
        public string adress { get; set; }
        public string postnummer { get; set; }
        public string postort { get; set; }
        public Land land { get; set; }
    }

    public class Rakenskapsar
    {
        public string rakenskapsarInleds { get; set; }
        public string rakenskapsarAvslutas { get; set; }
    }

    public class Organization
    {
        public List<Organisationslistum> organisationslista { get; set; }
    }

    public class SarskildaForetagsnamn
    {
        public Organisationsnamn organisationsnamn { get; set; }
        public string registreringsdatum { get; set; }
        public string verksamhetsbeskrivning { get; set; }
        public string beslutAvStamma { get; set; }
        public string beslutstyp { get; set; }
    }


    public class TokenResponse
    {
        public string token { get; set; }
    }

}
