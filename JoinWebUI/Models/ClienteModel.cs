using AutoMapper;
using ModelData.Dto;
using System.ComponentModel.DataAnnotations;

namespace JoinWebUI.Models
{
    public class ClienteModel : EntitaPermessiModel, IMappingAction<ClienteDto, ClienteModel>
    {
        public ClienteModel() : base()
        {
            SetConstantProperties();
        }

        public void Process(ClienteDto source, ClienteModel destination, ResolutionContext context)
        {
            SetConstantProperties();
        }

        public void SetConstantProperties()
        {
            Type = nameof(ClienteModel);
            TypeDescription = "Cliente";
            ParentId = null;
            IsContainerOnly = true;
        }

        public string CodiceCliente { get; set; }
        public string Nome
        {
            get
            {
                return _nome;
            }
            set
            {
                _nome = value;
                Info = CodiceCliente + " - " + value;
            }
        }
        private string _nome;

        public List<string>? DominiAssociati { get; set; }
    }
}
