namespace NetCore.Utils.Interfaces
{
    public interface IAccountSession
    {
        int GetAccountID();

        string GetAccountName();

        string GetNickName();

        //string GetAccessToken();

        //string GetSourceID();
        string GetIpAddress();
    }
}