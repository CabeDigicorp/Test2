using AutoMapper;
using ModelData.Dto;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace JoinWebUI.Models
{
    public class SettoreModel : EntitaPermessiModel, IListBoxCardBaseModel, IMappingAction<SettoreDto, SettoreModel>
    {
        public SettoreModel() : base()
        {
            SetConstantProperties();
        }

        public void Process(SettoreDto source, SettoreModel destination, ResolutionContext context)
        {
            SetConstantProperties();
        }

        public void SetConstantProperties()
        {
            Type = nameof(SettoreModel);
            TypeDescription = "Settore";
            ParentId = ClienteId;
            IsContainerOnly = true;
        }

        public Guid ClienteId
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
        private string _nome;

        public string TextLine1 { get => Nome; }

        public string TextLine2 { get => string.Empty; }

        public bool MultiLine { get => false; }

    }
}
