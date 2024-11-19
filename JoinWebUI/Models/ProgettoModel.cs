using AutoMapper;
using ModelData.Dto;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace JoinWebUI.Models
{
    public class ProgettoModel : EntitaPermessiModel, IMappingAction<ProgettoDto, ProgettoModel>
    {
        public ProgettoModel() : base()
        {
            SetConstantProperties();
        }

        public void Process(ProgettoDto source, ProgettoModel destination, ResolutionContext context)
        {
            SetConstantProperties();
        }

        public void SetConstantProperties()
        {
            Type = nameof(ProgettoModel);
            TypeDescription = "Progetto";
            ParentId = OperaId;
            IsContainerOnly = true;
        }


        [Required]
        [MaxLength(255)]
        public string Nome
        {
            get
            {
                return _nome;
            }
            set
            {
                _nome = value;
                Info = value;
            }
        }
        private string _nome = string.Empty;

        [Required]
        public Guid OperaId
        {
            get
            {
                return ParentId ?? Guid.Empty;
            }
            set
            {
                ParentId = value;
            }
        }

        [Required]
        [MaxLength(255)]
        public string Descrizione { get; set; } = string.Empty;

        //public string TextLine1 => Nome;
        //public string TextLine2 => string.Empty;
        //public bool MultiLine => false;

        public string Valore { get => string.Empty; }
    }
}
