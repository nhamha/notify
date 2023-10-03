using ServerCore.PortalAPI.Models;
using System;
using System.Collections.Generic;

namespace ServerCore.PortalAPI.DataAccess.DAO
{
	public interface ILoyaltyDAO
	{
		List<VpLevel> GetVpLevel(int? type);
		VipPointByAccountId FindVipPointInfo(long accountID);
		List<AccountVpLevelExt> FindTopAccountVpTransaction(long accountID, int top);
		List<AccountVpLevelInfo> FindTopRankVipPointLevelInfo(long accountID, int top, DateTime date, int type);
	}
}
