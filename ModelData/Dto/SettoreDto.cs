﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelData.Dto
{
    public class SettoreDto
    {
        public Guid Id { get; set; }
        public string? Nome { get; set; }
        public Guid? ClienteId { get; set; }
    }
}