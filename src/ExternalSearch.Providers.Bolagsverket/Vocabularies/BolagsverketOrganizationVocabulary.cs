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
