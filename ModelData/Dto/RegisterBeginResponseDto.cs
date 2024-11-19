using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelData.Dto
{
    public class RegisterBeginResponseDto
    {

        public bool Success { get; set; }
        public string? CodiceCliente { get; set; }
        public string Token { get; set; }
        public long Expiration { get; set; }
    }
}
