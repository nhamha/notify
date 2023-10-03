using ServerCore.DataAccess.DTO;
using System.Collections.Generic;

namespace ServerCore.DataAccess.DAO
{
    public interface IMobileDAO
    {
        int GetFacebookAccount(long accountid, out string facebook);

        int UpdateFacebookAccount(long accountid, string facebook);

        int SetBitEvent(long accountid, int quantity, int playedQuantity);

        int SetInvitedEvent(int accountid, int quantity, int offset);

        List<InvitedEvent> GetEventAccountInfo(int accountid);

        List<MobileBaseMoney> GetBaseMoney();

        List<MobileLink> GetLinks(int type);

        List<MobileLink3> GetLinks3(int type);

        List<MobileVersion> GetVersion(string os, int pa);

        int UpdateEmail(long accountid, string email);

        int SetEvent(string eventUrl, string eventImageUrl, string gitfUrl);
    }
}