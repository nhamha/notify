using GameServer.Utilities.Google;
using Newtonsoft.Json.Linq;
using ServerCore.Utilities.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore.Utilities.Google
{
//    {
// "issued_to": "407408718192.apps.googleusercontent.com",
// "audience": "407408718192.apps.googleusercontent.com",
// "user_id": "105281223532964808931",
// "scope": "https://www.googleapis.com/auth/userinfo.profile",
// "expires_in": 3569,
// "access_type": "offline"
//}


//    {
// "issued_to": "407408718192.apps.googleusercontent.com",
// "audience": "407408718192.apps.googleusercontent.com",
// "user_id": "105281223532964808931",
// "scope": "https://www.googleapis.com/auth/userinfo.email https://www.googleapis.com/auth/plus.me",
// "expires_in": 3571,
// "email": "ebank.gareport@gmail.com",
// "verified_email": true,
// "access_type": "offline"
//}

    public class GoogleUtil
    {
        public static GoogleAccount GetGoogleAccount(string UserInformation)
        {
            try
            {
                var GoogleResponseData = JObject.Parse(UserInformation.ToString());
                if (GoogleResponseData["email"] != null || GoogleResponseData["id"] != null)
                {
                    var GoogleId = (string)GoogleResponseData["id"];
                    var Name = (string)GoogleResponseData["email"];
                    var Email = (string)GoogleResponseData["email"];
                    var GoogleAccount = new GoogleAccount(GoogleId, Name, Email);
                    return GoogleAccount;
                }

                return new GoogleAccount();
            }
            catch (Exception exx)
            {
                NLogManager.Exception(exx);
                return new GoogleAccount();
            }
        }
    }
}
