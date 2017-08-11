using System.ComponentModel.DataAnnotations;
namespace PrivatTerminalApi.Models.Enities
{
	public class Payer
	{
		public int Id { get; set; }
		[Required]
		public string Name { get; set; }
        [Required]
        public string AccountNumber { get; set; }
	}
}