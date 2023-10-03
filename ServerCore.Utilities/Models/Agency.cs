using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore.Utilities.Models
{
    public class AgencyBank
    {
        public string BankName
        {
            get;
            set;
        }

        public string AccountBank
        {
            get;
            set;
        }

        public string AccountName
        {
            get;
            set;
        }

        public int Rate
        {
            get;
            set;
        }
    }
    [Serializable]
    public class Agency
    {
      //  [AgencyID]
      //,[AgencyName]
      //,[AgencyMobile]
      //,[AgencyParentID]
      //,[AccountID]
      //,[UserName]
      //,[NickName]
      //,[Status]
      //,[LevelID]
      //,[Rate]
      //,[AgencyAddress]
      //,[NickFacebook]
      //,[FullName]
      //,[StaffName]
      //,[CreateDate]
      //,[LastChanged]
        public Agency()
        {

        }

        public Agency(string agencyName, int agencyID, string agencyMobile, string agencyFB)
        {
            this.AgencyName = agencyName;
            this.AgencyID = agencyID;
            this.AgencyMobile = agencyMobile;
            this.NickFacebook = agencyFB;
        }

        public string AgencyName
        {
            get;
            set;
        }

        [JsonIgnore]
        public long AgencyID
        {
            get;
            set;
        }

        public string AgencyMobile
        {
            get;
            set;
        }
        //[JsonIgnore]
        public long AgencyParentID
        {
            get;
            set;
        }

        //[JsonIgnore]
        public long AccountID
        {
            get;
            set;
        }

        [JsonIgnore]
        public string UserName
        {
            get;
            set;
        }

        public string NickName
        {
            get;
            set;
        }

        [JsonIgnore]
        public int Status
        {
            get;
            set;
        }

        //[JsonIgnore]
        public int LevelID
        {
            get;
            set;
        }

        [JsonIgnore]
        public int Rate
        {
            get;
            set;
        }

        public int Position
        {
            get;
            set;
        }
        public string PositionName
        {
            get;
            set;
        }

        public string AgencyAddress
        {
            get;
            set;
        }

        public string NickFacebook
        {
            get;
            set;
        }

        public string Telegram
        {
            get;
            set;
        }

        [JsonIgnore]
        public string FullName
        {
            get;
            set;
        }
        public string StaffName
        {
            get;
            set;
        }

        [JsonIgnore]
        public DateTime CreateDate
        {
            get;
            set;
        }

        [JsonIgnore]
        public DateTime LastChanged
        {
            get;
            set;
        }

        public List<AgencyBank> lsBanks
        {
            get;
            set;
        }
    }
    /// <summary>
    /// Trả về trong danh sách bang hội
    /// </summary>
    public class GameAgency
    {
        public int STT { get; set; }
        public int AgencyID { get; set; }
        public string AgencyName { get; set; }
        public string AgencyMobile { get; set; }
        public long AgencyParentID { get; set; }
        public int Rate { get; set; }
        [JsonIgnore]
        public long AccountID { get; set; }
        [JsonIgnore]
        public string UserName { get; set; }
        public string NickName { get; set; }
        public int TotalVipPoint { get; set; }
        public int RateVipPoint { get; set; }
        public int MonthlyBonus { get; set; }
        public int IsMember { get; set; }
        public string ContentRule { get; set; }
        public int TotalMember { get; set; }
        public int TotalPendingMember { get; set; }
        public List<GameMember> Members{ get; set; }
        public List<GameMember> MemberRequests { get; set; }
        public List<GameMember> MemberLefts { get; set; }
    }

    public class AgenciesInfo
    {
        public long AgencyID { get; set; }
        //public string AgencyName { get; set; }
        public string AgencyMobile { get; set; }
        public string NickName { get; set; }
        public int LevelID { get; set; }
        public int AccountID { get; set; }
    }

    /// <summary>
    /// Thông tin Member trong bang hội
    /// </summary>
    public class GameMember
    {
        public int STT { get; set; }
        public long AgencyID { get; set; }
        public long AccountId { get; set; }
        public string NickName { get; set; }
        public int VipPoint { get; set; }
        public int Status { get; set; }
        public int Rate { get; set; }
    }

    /// <summary>
    /// Yêu cầu tham gia bang. (AccountAgency)
    /// </summary>
    public class GameJoinRequest
    {
        public int Id { get; set; }
        public long AgencyId { get; set; }
        public long AccountId { get; set; }
        public int Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime JoinDate { get; set; }
        public DateTime LeftDate { get; set; }
    }
}
