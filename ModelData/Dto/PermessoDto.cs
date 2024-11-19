using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModelData.Utilities;

namespace ModelData.Dto
{
    public class PermessoDto
    {
        public Guid Id { get; set; }

        //public string? Nome { get; set; }

        public Guid SoggettoId { get; set; }

        public TipiOggettoPermessi OggettoTipo { get; set; }
        public Guid OggettoId { get; set; }

        public List<Guid> RuoliIds { get; set; }

    }

}
