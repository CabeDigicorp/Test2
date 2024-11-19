using AutoMapper;
using ModelData.Dto;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace JoinWebUI.Models
{
    public class GruppoUtentiModel : EntitaPermessiModel, IListBoxCardBaseModel, IComparable<GruppoUtentiModel>, IMappingAction<GruppoUtentiDto, GruppoUtentiModel>
    {
        public GruppoUtentiModel() : base()
        {
            SetConstantProperties();
        }

        public void Process(GruppoUtentiDto source, GruppoUtentiModel destination, ResolutionContext context)
        {
            SetConstantProperties();
        }

        public void SetConstantProperties()
        {
            Type = nameof(GruppoUtentiModel);
            TypeDescription = "Gruppo";
        }

        [Required]
        public Guid OperaId
        {
            get => ParentId ?? Guid.Empty;
            set => ParentId = value;
        }

        [Required]
        [MaxLength(255)]
        public string Nome
        {
            get => _nome;
            set
            {
                _nome = value;
                Info = value;
            }
        }
        private string _nome = string.Empty;

        public string TextLine1 { get => Nome; }

        public string TextLine2 { get => string.Empty; }

        public bool MultiLine { get => false; }
 

        public int CompareTo(GruppoUtentiModel? other)
        {
            if(other == null)
            {
                return 1;
            }
            return string.Compare(Nome, other.Nome, StringComparison.CurrentCultureIgnoreCase);
        }

    }
}
