﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelData.Dto
{
    public class GruppoUtentiCreateDto
    {
        [Required]
        [MaxLength(255)]
        public string Nome { get; set; } = string.Empty;

        [Required]
        public Guid OperaId { get; set; }

    }
}