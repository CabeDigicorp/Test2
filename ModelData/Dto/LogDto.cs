using ModelData.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelData.Dto
{
    public class LogDto
    {
        public Guid? Id { get; set; }
        public LogType Type { get; set; } = LogType.Information;
        public string? Message { get; set; }
        public string? AssemblyName { get; set; }
        public string? AssemblyVersion { get; set; }
        public string? EnvironmentName { get; set; }
        public string? User { get; set; }
        public string? RequestPath { get; set; }
        public string? RequestFunction { get; set; }
        public int RequestLine { get; set; }
    }
}
