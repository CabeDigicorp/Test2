namespace ModelData.Dto
{
    public class UtenteDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Cognome { get; set; } = string.Empty;

        public List<string> Auth0Roles { get; set; } = new List<string>();

        public bool PrivacyConsent { get; set; }
        public bool Disabled { get; set; }
        public string? DomainManagerInfo { get; set; } = null;

    }

}
