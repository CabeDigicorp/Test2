using AutoMapper;
using ModelData.Dto;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace JoinWebUI.Models
{
    public class TeamModel : EntitaPermessiModel, IListBoxCardBaseModel, IComparable<TeamModel>, IMappingAction<TeamDto, TeamModel>
    {
        public TeamModel() : base()
        {
            SetConstantProperties();
        }
        public void Process(TeamDto source, TeamModel destination, ResolutionContext context)
        {
            SetConstantProperties();
        }

        public void SetConstantProperties()
        {
            Type = nameof(TeamModel);
            TypeDescription = "Team";
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


        public Guid ClienteId { get; set; } = Guid.Empty;

        [Required]
        public bool IsAdmin { get; set; } = false;

        public bool IsLicensed { get; set; } = false;

        public List<Guid> GruppiIds { get; set; } = new List<Guid>();

        public string TextLine1 { get => Nome; }

        public string TextLine2 { get => string.Empty; }

        public bool MultiLine { get => false; }
            
        public int CompareTo(TeamModel? other)
        {
            if (other == null)
            {
                return 1;
            }
            return ClienteId.CompareTo(other.ClienteId) + string.Compare(Nome, other.Nome, StringComparison.CurrentCultureIgnoreCase);
        }

    }
}
