using PrivatTerminalApi.Models.Enities;
using System.Data.Entity;

namespace PrivatTerminalApi.Models
{
	public class TerminalDb : DbContext
	{
		public TerminalDb()
			: base("TerminalDbConnection")
        { }
		public DbSet<Payer> Payers { get; set; }
	}
}