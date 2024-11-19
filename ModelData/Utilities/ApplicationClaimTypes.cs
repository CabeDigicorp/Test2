using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ModelData.Utilities
{
    public static class ApplicationClaimTypes
    {
        public const string CodiceCliente = "CodiceCliente";
        public const string Email = "email";
        public const string Nome = "given_name";
        public const string Cognome = "family_name";
        public const string Azione = "azione";
        public const string Ereditabile = "ereditabile";
    }
}
