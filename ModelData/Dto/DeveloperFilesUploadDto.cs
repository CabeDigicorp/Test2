using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelData.Dto
{
    public class DeveloperFilesUploadDto
    {
        [Required]
        public string? Foo { get; set; }
        [Required]
        public int? Bar { get; set; }
        [Required]
        public DateTime? Baz { get; set; }
        [Required]
        public List<IFormFile>? Files { get; set; }
    }
}
