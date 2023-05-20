// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DuckDuckGoOrganizationVocabulary.cs" company="Clued In">
//   Copyright (c) 2018 Clued In. All rights reserved.
// </copyright>
// <summary>
//   Implements the duck go organization vocabulary class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using CluedIn.Core.Data;
using CluedIn.Core.Data.Vocabularies;

namespace CluedIn.ExternalSearch.Providers.Bolagsverket.Vocabularies
{
    public class BolagsverketOrganizationVocabulary : SimpleVocabulary
    {
        public BolagsverketOrganizationVocabulary()
        {
            this.VocabularyName = "Bolagsverket Organization";
            this.KeyPrefix      = "bolagsverket.organization";
            this.KeySeparator   = ".";
            this.Grouping       = "/Organization";

            this.Rerksamhetsbeskrivning = this.Add(new VocabularyKey("rerksamhetsbeskrivning"));
            this.RakenskapsarAvslutas = this.Add(new VocabularyKey("rakenskapsarAvslutas"));
       
        }

 
        public VocabularyKey Rerksamhetsbeskrivning { get; internal set; }
        public VocabularyKey RakenskapsarAvslutas { get; internal set; }
    }
}
