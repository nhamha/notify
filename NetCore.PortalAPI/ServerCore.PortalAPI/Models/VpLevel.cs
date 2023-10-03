using System;

namespace ServerCore.PortalAPI.Models
{
	public class VpLevel
	{
		public int id { get; set; }
		public short vpType { get; set; }
		public int vpLevel { get; set; }
		public long vpThreshold { get; set; }
		public long vpExchangeValue { get; set; }
		public string vpLevelName { get; set; }
		public string description { get; set; }
		public short delFlag { get; set; }
		public DateTime createTime { get; set; }
		public DateTime updateTime { get; set; }
	}
}
