using System.ComponentModel;

namespace ModelData.Dto
{
    public class ProgettoDto
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string OperaId { get; set; } = string.Empty;
        public string Descrizione { get; set; } = string.Empty;
        
        //Data di creazione del contenuto del progetto (dovrebbe coincidere con la data di creazione del progetto)
        public DateTime? ContentCreationDate { get; set; } = null;

        //Data ultima modifica del contenuto del progetto
        public DateTime? ContentLastWriteDate { get; set; } = null;
        public long ContentSize { get; set; } = 0;
    }
}
