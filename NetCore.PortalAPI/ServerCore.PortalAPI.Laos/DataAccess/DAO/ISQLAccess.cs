
using ServerCore.Utilities.Database;
using System.Collections.Generic;

namespace Netcore.Gateway.Interfaces
{
    public interface ISQLAccess
    {
        DBHelper getAuthen();

        DBHelper getBilling();

        DBHelper getAgency();

        DBHelper getGuild();

        DBHelper getGifcode();
    }
}