using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore.DataAccess.DTO
{
    public class AccountForm
    {
        private string _Username;

        public string UserName
        {
            set { _Username = value; }
        }

        public string username
        {
            get { return _Username; }
            set { _Username = value; }
        }

        private string _Password;

        public string UserPassword
        {
            set { _Password = value; }
        }

        public string password
        {
            get { return _Password; }
            set { _Password = value; }
        }

        public string ticks { get; set; }
    }

    public class Giftcode
    {

    }
}
