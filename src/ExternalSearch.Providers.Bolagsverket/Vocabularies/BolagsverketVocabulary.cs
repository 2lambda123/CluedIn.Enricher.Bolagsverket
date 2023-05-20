// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DuckDuckGoVocabulary.cs" company="Clued In">
//   Copyright (c) 2018 Clued In. All rights reserved.
// </copyright>
// <summary>
//   Implements the duck go vocabulary class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CluedIn.ExternalSearch.Providers.Bolagsverket.Vocabularies
{
    /// <summary>A duck go vocabulary.</summary>
    public static class BolagsverketVocabulary
    {
        public static BolagsverketOrganizationVocabulary Organization { get; } = new BolagsverketOrganizationVocabulary();
    }
}
