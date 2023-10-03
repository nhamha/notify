using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServerCore.Utilities.Utils;
using System.Web;
using System.Security.Principal;
using System.Threading;

namespace ServerCore.Utilities.Models
{
    public abstract class BettingGameSession : IDisposable
    {
        private static readonly string WhiteListIps;
        private static readonly string[] WhiteListIpArray;

        private static readonly int MaxLeave;
        private static readonly int MaxLeaveIP;
        private static readonly int BlockTime;
        private static readonly int MinTimeBlock;

        //static PooledRedisClientManager pooledClientManager = new PooledRedisClientManager(10, 10, "localhost:6379");
        //static PooledRedisClientManager pooledClientManagerCCU = new PooledRedisClientManager(10, 10, "chat.atxionline.com:6379");
       
        static BettingGameSession()
        {
            try
            {
                MaxLeave = Int32.Parse(ConfigurationManager.AppSettings["MaxLeave"]);
                MaxLeaveIP = Int32.Parse(ConfigurationManager.AppSettings["MaxLeaveIP"]);
                BlockTime = Int32.Parse(ConfigurationManager.AppSettings["BlockTime"]);
                MinTimeBlock = Int32.Parse(ConfigurationManager.AppSettings["MinTimeBlock"]);

                WhiteListIps = ConfigurationManager.AppSettings["WhiteListIPs"];
                WhiteListIpArray = WhiteListIps.Split(',');
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
        }

        #region config_properties

        /// <summary>
        /// Danh sách players trong session
        /// </summary>
        public ConcurrentDictionary<long, BettingGamePlayer> Players { get; set; }

        /// <summary>
        /// Phiên đang chơi
        /// </summary>
        /// 
        public bool IsPlaying { get; set; }

        /// <summary>
        /// Trạng thái confirm người chơi
        /// </summary>
        [JsonIgnore]
        public bool IsComfirming { get; set; }

        /// <summary>
        /// Chủ bàn
        /// </summary>
        public long OwnerId { get; set; }

        /// <summary>
        /// Mảng danh sách người chơi theo accountID
        /// </summary>
        public long[] Positions { get; set; }

        /// <summary>
        /// Người thắng ván trước
        /// </summary>
        [JsonIgnore]
        public long LastWinner { get; set; }

        /// <summary>
        /// Danh sách người chơi đăng ký rời bàn
        /// </summary>
        [JsonIgnore]
        public List<long> LeaveGameList { get; private set; }

        /// <summary>
        /// Đếm số người trong phiên chơi
        /// </summary>
        public int CountActivePlayer { get; set; }

        /// <summary>
        /// Số lượng người chơi tối đa trong bàn
        /// </summary>
        
        private int maxAllow;

        [JsonIgnore]
        public int MaxAllow
        {
            get
            {
                return maxAllow;
            }
            protected set
            {
                if (value > MaxPlayer)
                {
                    value = MaxPlayer;
                }
                maxAllow = value;
            }
        }

        [JsonIgnore]
        public bool Active { get; set; }

        [JsonIgnore]
        public DateTime LastStartGameLoopTime { get; set; }

        [JsonIgnore]
        public long SessionId { get; set; }

        public string Name { get; set; }

        [JsonIgnore]
        public byte Status { get; set; }

        [JsonIgnore]
        public byte GameId { get; set; }

        [JsonIgnore]
        public byte MaxPlayer { get; set; }

        [JsonIgnore]
        public byte MinLevel { get; set; }

        [JsonIgnore]
        public byte MaxLevel { get; set; }

        public int MinBet { get; set; }

        public int MaxBet { get; set; }

        [JsonIgnore]
        public bool IsPasswordProtected { get; set; }

        [JsonIgnore]
        public string RuleDescription { get; set; }

        [JsonIgnore]
        public bool IsPrivate { get; protected set; }

        [JsonIgnore]
        public DateTime CreatedTime { get; set; }

        [JsonIgnore]
        public string Password { get; set; }

        [JsonIgnore]
        public int BetStep { get; set; }

        public byte MoneyType { get; set; }

        public long CurrentGameLoopId { get; set; }

        //public byte Rule { get; protected set; }
        #endregion config_properties

        #region getter

        public int CountPlayers
        {
            get
            {
                return Players.Count;
            }
        }

        [JsonIgnore]
        public bool IsFull
        {
            get
            {
                return Players.Count == MaxAllow;
            }
        }

        [JsonIgnore]
        public bool IsEnough
        {
            get
            {
                return Players.Count >= 2;
            }
        }

        [JsonIgnore]
        public bool IsEmpty
        {
            get
            {
                return Players.Count == 0;
            }
        }

        /// <summary>
        /// Cho phep vao hay khong neu phong chi cho toi da 2,3 nguoi vao
        /// </summary>
        [JsonIgnore]
        public bool IsAllow
        {
            get
            {
                return (Players.Count < MaxAllow && Players.Count > 0);
            }
        }

        #endregion getter

        #region abstract_methods
        public abstract bool CanBePlayer(AccountDb account);
        public abstract bool EnableStartGame();
        public abstract void NotifyStartGame();
        protected abstract void PrepareDeactive();
        protected abstract void OwnerQuitAtStartGame(long oldOwner);
        protected abstract void WinnerQuitAtStartGame(long lastWinner);
        protected abstract void StopTimerIfNotEnough();
        public abstract JToken CloneData(long accountId);
        public abstract JToken GetCurrentGameState();
        public abstract int GetClientTotalTime();
        public abstract int GetClientElapsedTime();
        #endregion abstract_methods

        private BettingGameSession()
        {
        }

        protected BettingGameSession(byte gameId)
        {
            //long sessionId = IdGenerator.Instance.NextSessionId();
            long sessionId = -1;
            string roomName = string.Empty;
            byte maxPlayer = 0;
            int minBet = 10;
            int maxBet = 100;

            switch (gameId)
            {
                case GameIdTypes.BA_CAY:
                    //roomName = "3cay:" + sessionId;
                    maxPlayer = 8;
                    break;
                case GameIdTypes.PHOM:
                    //roomName = "tala:" + sessionId;
                    maxPlayer = 4;
                    break;
                case GameIdTypes.TIEN_LEN_MB:
                    //roomName = "tlmb:" + sessionId;
                    maxPlayer = 4;
                    break;
                case GameIdTypes.TIEN_LEN_MN:
                    //roomName = "tlmn:" + sessionId;
                    maxPlayer = 4;
                    break;
                case GameIdTypes.TIEN_LEN_MN_NHAT_AN_TAT:
                    //roomName = "tlmn:" + sessionId;
                    maxPlayer = 4;
                    break;
                case GameIdTypes.CHAN:
                    //roomName = "chan:" + sessionId;
                    maxPlayer = 4;
                    break;
                case GameIdTypes.POKER:
                    //roomName = "poker:" + sessionId;
                    maxPlayer = 6;
                    minBet = 100;
                    maxBet = 200;
                    break;
                case GameIdTypes.POKERHK:
                    //roomName = "poker:" + sessionId;
                    maxPlayer = 5;
                    minBet = 100;
                    maxBet = 200;
                    break;
                case GameIdTypes.MAU_BINH:
                    maxPlayer = 4;
                    break;
                case GameIdTypes.SAM_LOC:
                    maxPlayer = 4;
                    break;
                case GameIdTypes.LIENG:
                    maxPlayer = 7;
                    break;
                case GameIdTypes.XOC_DIA:
                    maxPlayer = 11;
                    break;
                case GameIdTypes.TIEN_LEN_MN_SOLO:
                    maxPlayer = 2;
                    break;
                case GameIdTypes.SAM_LOC_SOLO:
                    maxPlayer = 2;
                    break;
                default:
                    break;
            }

            byte roomStatus = 0;
            byte minLevel = 0;
            byte maxLevel = 100;
            bool isHassPass = false;
            string rule = string.Empty;
            string password = string.Empty;
            int betStep = 10;
            byte roomType = 0;

            SessionId = sessionId;
            IsPlaying = false;
            IsComfirming = false;
            Name = roomName;
            GameId = gameId;
            Status = roomStatus;
            MaxPlayer = maxPlayer;
            MaxAllow = maxPlayer;
            MinLevel = minLevel;
            MaxLevel = maxLevel;
            MinBet = minBet;
            MaxBet = maxBet;
            IsPasswordProtected = isHassPass;
            RuleDescription = rule;
            Password = password;
            BetStep = betStep;
            MoneyType = roomType;
            Active = true;
            LastStartGameLoopTime = DateTime.Now;

            Players = new ConcurrentDictionary<long, BettingGamePlayer>();
            LeaveGameList = new List<long>(MaxPlayer);
            Positions = new long[MaxPlayer];
        }

        #region add_remove_player

        /// <summary>
        /// Them nguoi choi vao phong.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public virtual bool AddPlayer(BettingGamePlayer player)
        {
            if (IsFull)
            {
                return false;
            }

            //Neu phong da ton tai nguoi nay.
            if (Players.ContainsKey(player.AccountID))
            {
                player.SetSessionID(SessionId);
                player.ChangeConnectionStatus(ConnectionStatus.CONNECTED);
                return true;
            }
            //NLogManager.LogMessage(string.Format("AddPlayer - Session {0} - Player {1}", SessionId, player.AccountID));

            //Neu phong chua co nguoi, chu phong la nguoi dau tien.
            if (IsEmpty)
            {
                OwnerId = player.AccountID;
            }
            //Add nguoi choi vao danh sach.
            if (!Players.TryAdd(player.AccountID, player)) return false;
            player.SetSessionID(SessionId);
            player.ChangeConnectionStatus(ConnectionStatus.CONNECTED);
            player.Status = PlayerStatus.VIEWER;
            player.RegisterLeaveRoom = LeaveGameList.Contains(player.AccountID);
            if(CountPlayers > 0)
                this.Active = true;
            // Update CCU
            //UpdatePlayersInGame(true);

            //Tim vi tri ngoi cho nguoi choi.
            for (int idx = 0; idx < Positions.Length; idx++)
            {
                //Neu cho chua co nguoi ngoi
                if (Positions[idx] != 0) continue;
                Positions[idx] = player.AccountID;
                player.Position = idx;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Remove nguoi choi khoi phong.
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public BettingGamePlayer RemovePlayer(long accountId)
        {
            BettingGamePlayer player = null;
            long nextOwner = -1;
            if (accountId == OwnerId)
            {
                nextOwner = SearchNext(accountId);
            }

            if (Players.TryRemove(accountId, out player))
            {
                player.ResetBeforeQuitRoom();

                for (int idx = 0; idx < Positions.Length; idx++)
                {
                    if (Positions[idx] != accountId) continue;
                    Positions[idx] = 0;

                    if (player.AccountID == OwnerId)
                    {
                        if (!IsEmpty)
                        {
                            if (nextOwner > 0 && Players.ContainsKey(nextOwner))
                            {
                                OwnerId = nextOwner;
                            }
                            else
                            {
                                OwnerId = Players.Keys.First();
                            }
                            OwnerQuitAtStartGame(player.AccountID);
                        }
                    }

                    if (player.AccountID == LastWinner)
                    {
                        LastWinner = -1;
                        WinnerQuitAtStartGame(player.AccountID);
                    }

                    if (CountPlayers <= 1)
                    {
                        StopTimerIfNotEnough();
                    }

                    break;
                }
            }

            // update users in game
            //UpdatePlayersInGame(false);
            
            //double dt = (DateTime.Now - player.LastLeaveRoom).TotalSeconds;
            //player.LastLeaveRoom = DateTime.Now;

            //try
            //{
            //    using (var redis = pooledClientManager.GetClient())
            //    {
            //        int count1 = redis.Get<int>("" + accountId);
            //        int count2 = redis.Get<int>(player.RemoteIP);

            //        bool change = false;
            //        if (dt <= MinTimeBlock)
            //        {
            //            count1++;
            //            count2++;
            //            change = true;
            //        }
            //        else
            //        {
            //            if (count1 > 0 && dt > 60)
            //            {
            //                count1--;
            //                change = true;
            //            }
            //            if (count2 > 0 && dt > 60)
            //            {
            //                count2--;
            //                change = true;
            //            }
            //        }

            //        if (count1 > MaxLeave)
            //        {
            //            redis.Set<int>("" + accountId, count1, TimeSpan.FromSeconds(BlockTime));
            //            HttpContext.Current.User = new GenericPrincipal(new GenericIdentity(string.Empty), null);
            //        }
            //        else if (change)
            //            redis.Set<int>("" + accountId, count1, TimeSpan.FromSeconds(600));

            //        if (count2 > MaxLeaveIP)
            //        {
            //            redis.Set<int>(player.RemoteIP, count2, TimeSpan.FromSeconds(BlockTime));
            //            HttpContext.Current.User = new GenericPrincipal(new GenericIdentity(string.Empty), null);
            //        }
            //        else if (change)
            //            redis.Set<int>(player.RemoteIP, count2, TimeSpan.FromSeconds(600));
            //    }
            //}
            //catch
            //{

            //}

            if (CountPlayers != 0) return player;
            this.Active = false;
            this.PrepareDeactive();

            return player;
        }

        private void CallbackFuncReport(object obj)
        {
            try
            {
                //UpdatePlayersInGame();
            }
            catch(Exception ex)
            {
                NLogManager.Exception(ex);
            }
        }

        private void UpdatePlayersInGame(bool increase)
        {
            //try
            //{
            //    using (var redis = pooledClientManagerCCU.GetClient())
            //    {
            //        string key = "CCU-" + GameId.ToString();
            //        string newValue = string.Format("{0},{1},{2}", GameId, MinBet, MoneyType);
            //        string currentValue = redis.Get<string>(key);

            //        if(string.IsNullOrEmpty(currentValue))
            //        {
            //            currentValue = newValue + ";" + this.CountPlayers;
            //        }
            //        else
            //        {
            //            //GameId,MinBet,MoneyType;Count|GameId1,MinBet1,MoneyType1;Count1
            //            string[] splitCurrentValue = currentValue.Split('|');
            //            int len = splitCurrentValue.Length;
            //            bool isExit = false;

            //            if(len > 0)
            //            {
            //                for(int i = 0; i < len; i++)
            //                {
            //                    string[] item = splitCurrentValue[i].Split(';');
            //                    if(item[0].CompareTo(newValue) == 0)
            //                    {
            //                        int count = Convert.ToInt32(item[1]) + (increase ? 1 : -1);
            //                        if(count <= 0)
            //                            count = 0;

            //                        newValue += ";" + count;
            //                        splitCurrentValue[i] = newValue;
            //                        isExit = true;
            //                        break;
            //                    }
            //                }
            //            }

            //            // neu chua ton tai gia tri trong key thi them gia tri vao key hien tai, ko thi update
            //            if(!isExit)
            //            {
            //                currentValue += "|" + newValue + ";" + this.CountPlayers;
            //            }
            //            else
            //            {
            //                currentValue = splitCurrentValue[0];
            //                for(int i = 1; i < len; i++)
            //                {
            //                    currentValue += "|" + splitCurrentValue[i];
            //                }
            //            }
            //        }
            //        redis.Set<string>(key, currentValue);
            //    }
            //}
            //catch(Exception ex)
            //{
            //    NLogManager.Exception(ex);
            //}
        }

        /// <summary>
        /// kiem tra ton tai accountId
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public bool ContainPlayer(long accountId)
        {
            return Players.ContainsKey(accountId);
        }

        /// <summary>
        /// Lay thong tin nguoi choi tu accountId
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public BettingGamePlayer GetPlayer(long accountId)
        {
            try
            {
                BettingGamePlayer player = null;
                Players.TryGetValue(accountId, out player);
                return player;

            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return null;
            }
        }

        /// <summary>
        /// Dang ki roi phong.
        /// </summary>
        /// <param name="accountId"></param>
        public void RegisterLeaveGame(long accountId)
        {
            if (LeaveGameList.Contains(accountId)) return;
            LeaveGameList.Add(accountId);
            BettingGamePlayer player = GetPlayer(accountId);
            if (player != null)
            {
                player.RegisterLeaveRoom = true;
            }
        }

        /// <summary>
        /// Dang ki roi phong.
        /// </summary>
        /// <param name="accountId"></param>
        public void UnregisterLeaveGame(long accountId)
        {
            if (!LeaveGameList.Contains(accountId)) return;
            LeaveGameList.Remove(accountId);
            BettingGamePlayer player = GetPlayer(accountId);
            if (player == null) return;
            player.RegisterLeaveRoom = false;
            player.LastActiveTime = DateTime.Now;
            player.ChangeConnectionStatus(ConnectionStatus.CONNECTED);
        }

        /// <summary>
        /// Remove nhung nguoi choi dang ki roi phong.
        /// </summary>
        public virtual List<long> RemovePlayerAfterEndGame()
        {
            List<long> outGame = new List<long>();
            outGame.AddRange(LeaveGameList);
            //Remove nguoi choi dang ki roi phong.
            for (int i = 0; i < LeaveGameList.Count; i++)
            {
                RemovePlayer(LeaveGameList[i]);
            }
            LeaveGameList.Clear();

            //Remove nhung nguoi bi mat ket noi.
            foreach (var key in Players.Keys)
            {
                BettingGamePlayer player = null;
                if (!Players.TryGetValue(key, out player)) continue;
                if (player.ConnectionStatus != ConnectionStatus.DISCONNECTED) continue;
                RemovePlayer(key);
                outGame.Add(key);
            }

            if (Players.ContainsKey(OwnerId)) return outGame;
            if (!Players.IsEmpty)
            {
                OwnerId = Players.Keys.First();
            }
            else
            {
                OwnerId = -1;
            }
            return outGame;
        }

        #endregion add_remove_player

        /// <summary>
        /// Xoa cho ngoi.
        /// </summary>
        private void ClearPositons()
        {
            for (int idx = 0; idx < Positions.Length; idx++)
            {
                Positions[idx] = 0;
            }
        }

        /// <summary>
        /// Thay doi ownerId.
        /// </summary>
        /// <param name="accountId"></param>
        public void TranferOwnerId(long accountId)
        {
            List<long> list = new List<long>(Players.Keys);
            if (list.Contains(accountId))
            {
                OwnerId = accountId;
            }
            else
            {
                if (list.Count > 0)
                {
                    OwnerId = list[0];
                }
                else
                {
                    OwnerId = -1;
                }
            }
        }

        /// <summary>
        /// Lấy tỷ lệ phí
        /// </summary>
        /// <returns></returns>
        public double GetMoneyPercent()
        {
            switch (MoneyType)
            {
                case (byte)GameMoneyType.BAC:
                    return MoneyPercent.BAC;
                case (byte)GameMoneyType.BON:
                    return MoneyPercent.BON;
                default:
                    return MoneyPercent.NORMAL;
            }
        }

        /// <summary>
        /// Kiểm tra điều kiện FindSession không được trùng IP
        /// </summary>
        /// <param name="newIp"></param>
        /// <returns></returns>
        public bool DuplicateIp(string newIp)
        {
            return false;
            try
            {
                if (CountPlayers < 2)
                    return false;

                if (WhiteListIpArray != null)
                {
                    if (WhiteListIpArray.Any(wli => wli.Equals(newIp)))
                    {
                        return false;
                    }
                }

                List<string> ips = new List<string>(Players.Values.Select(p => p.RemoteIP));
                int count = ips.Count;
                bool existDuplicate = false;

                for (int i = 0; i < count - 1; i++)
                {
                    for (int j = i + 1; j < count; j++)
                    {
                        if (!ips[i].Equals(ips[j])) continue;
                        existDuplicate = true;
                        break;
                    }

                    if (existDuplicate)
                    {
                        break;
                    }
                }

                return existDuplicate ^ ips.Exists(ip => ip.Equals(newIp));
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return false;
        }

        /// <summary>
        /// Huy phien.
        /// </summary>
        public virtual void Dispose()
        {
            CountActivePlayer = 0;
            IsPlaying = false;
            LeaveGameList.Clear();
            IsPlaying = false;
            ClearPositons();
            Players.Clear();

            //if(_timerReport != null)
            //    _timerReport.Dispose();
        }

        public virtual void Refresh()
        {
            this.CurrentGameLoopId = -1;
            CountActivePlayer = 0;
            IsPlaying = false;
            LeaveGameList.Clear();
        }

        /// <summary>
        /// thoi gian tinh bang giay
        /// </summary>
        /// <returns></returns>
        public long IdleTime()
        {
            return (long)((DateTime.Now - LastStartGameLoopTime).TotalSeconds);
        }

        public override string ToString()
        {
            return SessionId.ToString(CultureInfo.InvariantCulture);
        }


        /// <summary>
        /// Tìm kiếm người đánh tiếp theo trong các game tiến lên
        /// </summary>
        /// <param name="accountid"></param>
        /// <returns></returns>
        public long SearchNext(long accountid)
        {
            int currIdx = -1;
            try
            {
                for (int i = 0; i < Positions.Length; i++)
                {
                    if (Positions[i] != accountid) continue;
                    currIdx = i;
                    break;
                }

                if (currIdx < 0)
                    return -1;

                int nextIdx = (currIdx + 1) % Positions.Length;

                while (nextIdx != currIdx)
                {
                    if (Positions[nextIdx] > 0)
                    {
                        return Positions[nextIdx];
                    }
                    nextIdx = (nextIdx + 1) % Positions.Length;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return -1;
        }
    }
}