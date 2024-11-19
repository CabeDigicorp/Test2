using AutoMapper;
using ModelData.Dto;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace JoinWebUI.Models
{
    public class OperaModel : EntitaPermessiModel, IMappingAction<OperaDto, OperaModel>
    {
        public OperaModel() : base()
        {
            SetConstantProperties();
        }
        public void Process(OperaDto source, OperaModel destination, ResolutionContext context)
        {
            SetConstantProperties();
        }

        public void SetConstantProperties()
        {
            Type = nameof(OperaModel);
            TypeDescription = "Opera";
            ParentId = SettoreId;
            IsContainerOnly = true;
        }


        [MaxLength(255)]
        [Display(Name = "Nome")]
        [Required]
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
        
        [MaxLength(255)]
        [Display(Name = "Descrizione")]
        public string Descrizione { get; set; } = string.Empty;
        
        //public IEnumerable<TagModel> Tags { get; set; } = new List<TagModel>();
        
        //public IEnumerable<string>? TagNames { get; set; }
        
        //public bool DropdownOpen { get; set; }
        
        //public bool MouseOver { get; set; }
        
        //[Required]
        //public Guid ClienteId { get; set; }

        [Required]
        public Guid SettoreId
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
        public IEnumerable<Guid> TagIds { get; set; }
        //{ 
        //    get {
        //        return Tags.Select(x => x.Id).ToList();
        //    }
        //} 
    }
}
