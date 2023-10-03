using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServerCore.Utilities.Utils;

namespace ServerCore.Utilities.Models
{
    public class BettingGamePlayer
    {
        public BettingGamePlayer(AccountDb account)
        {
            Account = account;
            AccountID = account.AccountID;
            Status = PlayerStatus.VIEWER;
            ConnectionStatus = ConnectionStatus.CONNECTED;
            RegisterLeaveRoom = false;
            LastActiveTime = DateTime.Now;
            LastLeaveRoom = DateTime.Now;
            WarningHack = false;
        }

        public AccountDb Account { get; private set; }

        public long AccountID { get; private set; }

        [JsonIgnore]
        public long SessionID { get; private set; }

        public PlayerStatus Status { get; set; }

        public int Position { get; set; }

        [JsonIgnore]
        public List<long> DislikeRooms { get; private set; }

        public bool RegisterLeaveRoom { get; set; }

        //[JsonIgnore]
        public string RemoteIP { get; set; }

        [JsonIgnore]
        public DateTime LastActiveTime { get; set; }

        [JsonIgnore]
        public DateTime LastActionTime { get; set; }

        public ConnectionStatus ConnectionStatus { get; private set; }

        [JsonIgnore]
        public DateTime LastLeaveRoom { get; set; }

        [JsonIgnore]
        public bool WarningHack { get; set; }

        public bool IsEnoughMoney(byte moneyType, long threshold)
        {
            switch (moneyType)
            {
                case 0:
                    return Account.TotalXu >= threshold;
                case 1:
                    return Account.TotalCoin >= threshold;
                default:
                    return false;
            }
        }

        public void ChangeConnectionStatus(ConnectionStatus status)
        {
            ConnectionStatus = status;
        }

        public void SetSessionID(long sessionId)
        {
            SessionID = sessionId;
        }

        public void ResetBeforeQuitRoom()
        {
            ClearData();
            SetSessionID(-1);
            Status = PlayerStatus.NOT_INGAME;
        }

        public virtual void ClearData()
        {
            RegisterLeaveRoom = false;
        }

        /// <summary>
        /// thoi gian tinh bang giay
        /// </summary>
        /// <returns></returns>
        public long IdleTime()
        {
            return (long)((DateTime.Now - LastActiveTime).TotalSeconds);
        }

        public long IdleAction()
        {
            return (long)((DateTime.Now - LastActionTime).TotalSeconds);
        }

        public void SetActive()
        {
            LastActiveTime = DateTime.Now;
            ConnectionStatus = ConnectionStatus.CONNECTED;
        }

        public JToken CloneAccount()
        {
            try
            {
                var obj = JToken.FromObject(this);
                //mask username
                if (obj["Account"] == null) return null;
                string username = obj["Account"]["UserName"].ToString();
                username = StringUtil.MaskUserName(username);
                obj["Account"]["UserName"].Replace(JToken.FromObject(username));

                return obj["Account"];
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return null;
        }

        public JToken Clone()
        {
            try
            {
                var obj = JToken.FromObject(this);
                //mask username
                if (obj["Account"] == null) return obj;
                string username = obj["Account"]["UserName"].ToString();
                username = StringUtil.MaskUserName(username);
                obj["Account"]["UserName"].Replace(JToken.FromObject(username));
                return obj;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return null;
        }

        public override string ToString()
        {
            return Account.UserName;
        }
    }
}