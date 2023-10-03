using System;

namespace ServerCore.PortalAPI.Models
{
	public class AccountVpLevelInfo
	{
		public string username { get; set; }
		public string userFullName { get; set; }
		public int level { get; set; }
		public string levelName { get; set; }
		public long accountId { get; set; }
		public long accountVp { get; set; }
		public long totalAmount { get; set; }
		public long vpExchangeValue { get; set; }
	}
}
