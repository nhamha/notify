using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServerCore.Utilities.Facebook
{
    public class FBAccount
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Link { get; set; }
        public string UserName { get; set; }
        public DateTime BirthDay { get; set; }
        public int Gender { get; set; }
        public string Email { get; set; }
        public int Verified { get; set; }
        public DateTime UpdateTime { get; set; }

        public FBAccount() { }

        public FBAccount(string accountID, string accountName, string email)
        {
            this.Id = accountID;
            this.Name = accountName;
            this.Email = email;
        }

    }
    [Serializable]
    public class IDs_Business
    {
        public string id { get; set; }
        public AppInfo app { get; set; }
    }

    [Serializable]
    public class AppInfo
    {
        public string name { get; set; }
        public string name_space { get; set; }
        public string id { get; set; }
    }

}


//{
// "issued_to": "407408718192.apps.googleusercontent.com",
// "audience": "407408718192.apps.googleusercontent.com",
// "user_id": "105281223532964808931",
// "scope": "https://www.googleapis.com/auth/userinfo.profile",
// "expires_in": 3569,
// "access_type": "offline"
//}
namespace GameServer.Utilities.Google
{
    public class GoogleAccount
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Link { get; set; }
        public string UserName { get; set; }
        public DateTime BirthDay { get; set; }
        public int Gender { get; set; }
        public string Email { get; set; }
        public int Verified { get; set; }
        public DateTime UpdateTime { get; set; }

        public GoogleAccount() { }

        public GoogleAccount(string accountID, string accountName, string email)
        {
            this.Id = accountID;
            this.Name = accountName;
            this.Email = email;
        }

    }
}
