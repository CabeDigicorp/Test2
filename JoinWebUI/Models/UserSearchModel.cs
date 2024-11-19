using System.ComponentModel.DataAnnotations;

namespace JoinWebUI.Models
{
	public class UserSearchModel
	{
		[Required]
		[EmailAddress]
		public string? Email { get; set; }
	}
}
