using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelData.Dto
{
	public class RemoveTeamOperaDto
	{
		[Required]
		public Guid TeamId { get; set; }
		[Required]
		public Guid OperaId { get; set; }

	}
}
