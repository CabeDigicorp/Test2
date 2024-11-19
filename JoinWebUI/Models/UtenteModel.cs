using AutoMapper;
using ModelData.Dto;
using ModelData.Utilities;
using Syncfusion.Blazor.PdfViewer;
using System.ComponentModel;
using System.Security.Policy;

namespace JoinWebUI.Models
{
    public class UtenteModel : EntitaPermessiModel, IListBoxCardBaseModel, IEquatable<UtenteModel>, IMappingAction<UtenteDto, UtenteModel>, ICloneable
    {

        public UtenteModel() : base()
        {
            SetConstantProperties();
        }

        public void Process(UtenteDto source, UtenteModel destination, ResolutionContext context)
        {
            SetConstantProperties();
        }

        public void SetConstantProperties()
        {
            Type = nameof(UtenteModel);
            TypeDescription = "Utente";
            IsMemberOnly = true;
        }



        public string Nome
        {
            get => _nome;
            set
            {
                _nome = value;
                Info = _nome + " " + _cognome + " (" + _email + ")";
            }
        }
        private string _nome;
        public string Cognome
        {
            get => _cognome;
            set
            {
                _cognome = value;
                Info = _nome + " " + _cognome + " (" + _email + ")";
            }
        }
        private string _cognome;
        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                Info = _nome + " " + _cognome + " (" + _email + ")";
            }
        }
        private string _email;


        public string NomeCompleto { get => string.Join(" ", Nome, Cognome); }

        public bool PrivacyConsent { get; set; }

        public bool Disabled { get; set; }

        public string? DomainManagerInfo { get; set; } = null;

        public IEnumerable<string> Auth0Roles { get; set; } = new List<string>();


        public string TextLine1 { get => NomeCompleto; }

        public string TextLine2 { get => Email; }

        public bool MultiLine { get => true; }

        public bool Equals(UtenteModel? other)
        {
            return this.Id.Equals(other?.Id);
        }

        public object Clone()
        {
            var result = new UtenteModel();

            result.Id = this.Id;
            result.Email = this.Email;
            result.Nome = this.Nome;
            result.Cognome = this.Cognome;
            result.Disabled = this.Disabled;
            result.DomainManagerInfo = this.DomainManagerInfo;
            result.PrivacyConsent = this.PrivacyConsent;
            result.Info = this.Info;
            result.ParentId = this.ParentId;
            result.OggettiChildren = this.OggettiChildren.ToList();
            result.SoggettiChildren = this.SoggettiChildren.ToList();
            result.SoggettiMembers = this.SoggettiMembers.ToList();
            result.Auth0Roles = this.Auth0Roles.ToList();

            return result;

        }

    }
}
