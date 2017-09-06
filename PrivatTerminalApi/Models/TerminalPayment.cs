using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace PrivatTerminalApi.Models
{
	public abstract class TerminalPayment
	{
		protected Stream reqestResult;
		public abstract void LoadRequest(Stream stream);

	}
}