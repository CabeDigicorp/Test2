using System.ComponentModel.DataAnnotations;

namespace ModelData.Dto
{
	public class AssignTeamGruppoDto
	{
		[Required]
		public Guid TeamId { get; set; }
		[Required]
		public Guid GruppoId { get; set; }

	}
}
