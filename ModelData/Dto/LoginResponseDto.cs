using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ModelData.Dto
{
    public class LoginResponseDto
    {
        public string Token { get; set; }
        public long Expiration { get; set; }
    }
}
