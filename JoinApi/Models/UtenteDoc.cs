using AspNetCore.Identity.MongoDbCore.Models;
using MongoDB.Bson.Serialization.Attributes;
using MongoDbGenericRepository.Attributes;

namespace JoinApi.Models
{
    [CollectionName("utenti")]
    public class UtenteDoc : MongoIdentityUser<Guid>
    {
        public UtenteDoc() : base()
        {

        }

        public string? Nome { get; set; } = null;

        public string? Cognome { get; set; } = null;

        public List<Guid> GruppiIds { get; set; } = new List<Guid>();

        //public List<Guid> ClientiIds { get; set; } = new List<Guid>();

        public List<Guid> TeamsIds { get; set; } = new List<Guid>();

        public List<Guid> FoundUsersIds { get; set; } = new List<Guid>();

        public bool PrivacyConsent { get; set; } = false;
        public bool Disabled { get; set; } = false;


        [BsonIgnore]
		public string NomeCompleto { 
            get
			{
                return string.Join(" ", Nome, Cognome);
			}
        }

        

	}
}
