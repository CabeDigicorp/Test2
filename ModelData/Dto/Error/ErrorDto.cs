﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelData.Dto.Error
{
    public class ErrorDto
    {
        public IDictionary<string, string> ErrorData { get; set; } = new Dictionary<string, string>();
    }
}
