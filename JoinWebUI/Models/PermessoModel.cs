using ModelData.Utilities;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


namespace JoinWebUI.Models
{
    public class PermessoModel 
    {
        [Required]
        public Guid Id { get; set; }

        public string IdString { get { return Id.ToString(); } }

        //[Required]
        //[MaxLength(255)]
        //public string Nome { get; set; } = string.Empty;

        [Required]
        public Guid SoggettoId { get; set; }

        [Required]
        public TipiOggettoPermessi OggettoTipo { get; set; }

        [Required]
        public Guid OggettoId { get; set; }

        [Required]
        public List<Guid> RuoliIds { get; set; } = new List<Guid>();


    }

    public class PermessiTreeNode
    {

        public Guid? Id { get; set; }

        public string? IdString { get { return Id?.ToString(); } }

        public EntitaPermessiModel? Soggetto { get; set; }
        public EntitaPermessiModel? Oggetto { get; set; }
        public List<PermessiTreeNode> Children { get; set; }
        public Dictionary<Guid, RuoloValues>? Ruoli { get; set; }

        public PermessiTreeNode()
        {
            Id = null;
            Soggetto = null;
            Oggetto = null;
            Children = new List<PermessiTreeNode>();
            Ruoli = null;
        }

        public PermessiTreeNode(bool fromOggetto, EntitaPermessiModel entita)
        {
            Id = null;
            Soggetto = fromOggetto ? null : entita;
            Oggetto = fromOggetto ? entita : null;
            Children = new List<PermessiTreeNode>();
            Ruoli = null;
        }

        public PermessiTreeNode(EntitaPermessiModel soggetto, EntitaPermessiModel oggetto, Dictionary<Guid, RuoloValues> ruoli = null)
        {
            Id = null;
            Soggetto = soggetto;
            Oggetto = oggetto;
            Children = new List<PermessiTreeNode>();
            Ruoli = ruoli;
        }

        public PermessiTreeNode(Guid guid, EntitaPermessiModel soggetto, EntitaPermessiModel oggetto, Dictionary<Guid, RuoloValues>? ruoli = null)
        {
            Id = guid;
            Soggetto = soggetto;
            Oggetto = oggetto;
            Children = new List<PermessiTreeNode>();
            Ruoli = ruoli;
        }

    }


}