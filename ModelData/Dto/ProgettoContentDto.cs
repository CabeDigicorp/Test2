using Microsoft.AspNetCore.Http;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ModelData.Dto
{
    public class ProgettoContentDto : ProgettoDto
    {
        /// <summary>
        /// Versione del contenuto del progetto (versione di Project)
        /// </summary>
        public int ContentVersion { get; set; } = -1;

        /// <summary>
        /// Contenuto del progetto Join (Project) serializzato e compresso
        /// </summary>
        public string Content { get; set; } = string.Empty;

    }
}
