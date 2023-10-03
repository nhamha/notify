using System;
using System.Numerics;

namespace ServerCore.PortalAPI.Models
{
	public class HistoryDeductRevenue
	{
		public DateTime CreatedAt { get; set; }
		public long Amount { get; set; }
		public string Description { get;set; }
		public long TransCode { get; set; }
	}
}
