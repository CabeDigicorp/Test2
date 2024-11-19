using MongoDB.Bson.Serialization.Attributes;

namespace JoinApi.Models
{
    public class ClienteDoc : IOggettoPermessiBase
    {
        [BsonId]
        public Guid Id { get; set; }
        public Guid ParentId { get => Guid.Empty; }

        public string CodiceCliente { get; set; }
        public string? Nome { get; set; }

        public string Info
        {
            get
            {
                return CodiceCliente + " - " + Nome;
            }
        }
        
        public List<string> DominiAssociati { get; set; } = new List<string>();

        public string? ChiaveLicenza { get; set; }
        public List<string>? ArchivioLicenze { get; set; }


    }
}
