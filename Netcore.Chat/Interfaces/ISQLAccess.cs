using Netcore.Chat.Models;
using System.Collections.Generic;

namespace Netcore.Chat.Interfaces
{
    public interface ISQLAccess
    {
        List<BlockAccount> LoadListBlockAccount();
        void SP_Chat_Insert(int _AccountID, string _AccountName, string _NickName, string _ChannerlID, string _Message);
        List<Admin> LoadListAdmin();
        List<BadWord> LoadListBadWord();
        List<ChatDB> LoadListLastMessage(string channelId);
        List<KeywordReplace> LoadListKeywordReplace();
        void DeleteBlockedAccount(int id);
    }
}