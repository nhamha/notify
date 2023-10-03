using System;

namespace ServerCore.PortalAPI.Models
{
	public class AccountVpLevelExt
	{
		public DateTime datetime { get; set; }
		public long accountVp { get; set; }
		public long totalAmount { get; set; }
		public int vpType { get; set; }
		public string vpLevelName { get; set; }
	}
}
