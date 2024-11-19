using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelData.Dto
{
    public class TeamDto
    {
        public Guid Id { get; set; }
        
        public string? Nome { get; set; }
        
        public Guid? ClienteId { get; set; }

        public List<Guid>? GruppiIds { get; set; }

        public bool? IsAdmin { get; set; }

        public bool? IsLicensed { get; set; }

    }
}
