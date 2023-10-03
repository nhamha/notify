using System;
using System.Collections.Generic;
using System.Text;

namespace ServerCore.Utilities.Facebook
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
