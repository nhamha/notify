using NLog.Fluent;

namespace ServerCore.PortalAPI.Models
{
	public class VipPointByAccountId
	{
		public long? vpDay { get; set; }
		public long? vpMonth { get; set; }
		public long? totalJMonth { get; set; }
		public long? totalJDay { get; set; }
		public int? levelDay { get; set; }
		public int? levelMonth { get; set; }
	}
}
