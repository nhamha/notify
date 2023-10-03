using GameServer.Utilities;
using ServerCore.Utilities.Security;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace PortalAPI.Libs
{
    public class OtpAppLib
    {
        //  General Token
        public string GenaralToken()
        {
            return "bonTokenServerClient";
        }

        public bool TokenCheckTime(string Token)
        {
            try
            {
                string _tokenPlain = Security.GetTokenPlainText(Token);

                //  check time
                DateTime _dateTime = Security.GetTokenTime(_tokenPlain);
                if (DateTime.Now > _dateTime.AddMinutes(int.Parse(ConfigurationManager.AppSettings["TimeoutToken"])))
                {
                    return false;
                }
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        //  get AccountID
        public int TokenGetAccountID(string Token)
        {
            string _tokenPlain = Security.GetTokenPlainText(Token);
            return Security.GetTokenAccountID(_tokenPlain);
        }

        //  get Mobile
        public string TokenGetAccountName(string Token)
        {
            string _tokenPlain = Security.GetTokenPlainText(Token);
            return Security.GetTokenAccountName(_tokenPlain);
        }

        //  get Mobile
        public string TokenGetMobile(string Token)
        {
            string _tokenPlain = Security.GetTokenPlainText(Token);
            return Security.GetTokenMobile(_tokenPlain);
        }
    }
}