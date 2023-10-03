using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Builder;

namespace ServerCore.Utilities.Utils
{
    public class AppSettings
    {
        public bool IsMaintain { get; set; }
        // Lock chức năng đổi thẻ
        public bool CashoutLock { get; set; }
        public string JwtKey { get; set; }
        public string FishingKey { get; set; }
        public string EncryptKey { get; set; }
        public string ClaimTypeAccountId { get; set; }
        public string ClaimTypeUserName { get; set; }
        public string ClaimTypeNickName { get; set; }
        public string ClaimTypeMerchantId { get; set; }
        public string ClaimTypePlatformId { get; set; }
        public string ClaimTypeAgency { get; set; }
        public string ClaimTypeOTP { get; set; }
        public string ClaimLocationId { get; set; }
        public string ClaimPreFix { get; set; }
        public string ClaimRefCode { get; set; }
        public string LoginActionName { get; set; }
        public int TokenExpire { get; set; }
        public int CacheExpireHour { get; set; }
        public int MaxLengthUserName { get; set; }
        public int MaxLengthNickName { get; set; }
        public int MinLengthUserName { get; set; }
        public int LoginFailTime { get; set; }
        public int LoginFailAllow { get; set; }
        public int MinBonTransfer { get; set; }
        public int MaxBonTransfer { get; set; }
        public string GraphApiFbBusiness { get; set; }
        public string GraphApiFbMe { get; set; }
        public string SysPartnerKey { get; set; }
        public string OtpServiceApiUrl { get; set; }
        public string ReportServiceApiUrl { get; set; }
        public string LoyaltyServiceApiUrl { get; set; }
        public string PopupServiceApiUrl { get; set; }
        public int ServiceIdFrozen { get; set; }
        public string SmstUrl { get; set; }
        public string ReChargeGetwayUrl { get; set; }
        public string BundleID { get; set; }
        public string CaptchaFarmUrl { get; set; }
        public string RedisHost { get; set; }
        public bool IsRedisCache { get; set; }
		public string BlockchainApiUrl { get; set; }

		public string EventConnectionString { get; set; }
        public string PaymentDBConnectionString{ get; set; }
        public string SmsBankingConnectionString { get; set; }
        public string SSOConnectionString { get; set; }
        public string CardGameBettingAPIConnectionString { get; set; }
        public string BillingAgencyAPIConnectionString { get; set; }
        public string BillingGuildAPIConnectionString { get; set; }
        public string BillingGifcodeAPIConnectionString { get; set; }
        public string BillingAuthenticationAPIConnectionString { get; set; }
        public string BillingDatabaseAPIConnectionString { get; set; }
        public string EventAPIConnectionString { get; set; }
        public string EventTargetConnectionString { get; set; } 
        public string DeviceFingerConnectionString { get; set; }
        public string ReportConnectionString { get; set; }
		public string LoyaltyConnectionString { get; set; }

		public string GameLogConnectionString { get; set; }
        public string BotConnectionString { get; set; }
        public string EventDBConnectionString { get; set; }
        public string GiftcodeConnectionString { get; set; }
        public string MobileDBConnectionString { get; set; }
        public string LogConnectionString { get; set; }

        public string MiniPokerConnectionString { get; set; }
        public string MiniGameLogConnectionString { get; set; }
        public string HiLoConnectionString { get; set; }

        public string BlockchainUsername { get; set; }
        public string BlockchainPassword { get; set; }
        public string BlockchainAuthenUrl { get; set; }

        #region TX
        public string ADMINS { get; set; }
        public int TIME_INTERVAL { get; set; }
        public int MAX_IDLE_USER_ONLINE { get; set; }
        public int MAX_MESSAGE_IN_CHANNEL { get; set; }
        public int MAX_MESSAGE_LENGTH { get; set; }
        public int MIN_USER_INACTIVE_IN_CHANNEL { get; set; }
        public int ENABLE_FAKENAME_LV1_IN_CHANNEL { get; set; }
        public int ENABLE_FAKENAME_LV2_IN_CHANNEL { get; set; }
        public int TWO_MESSAGE_DURATION { get; set; }
        public int TEN_MESSAGE_DURATION { get; set; }
        public int DUPLICATE_MESSAGE_DURATION { get; set; }
        public int GLOBAL_TEN_SAME_MESSAGE_DURATION { get; set; }
        public string BotAccountFile { get; set; }

        public int MaxLeave { get; set; }
        public int MaxLeaveIP { get; set; }
        public int BlockTime { get; set; }
        public int MinTimeBlock { get; set; }
        public int Leave { get; set; }
        public int LeaveIP { get; set; }

        public string LuckyDiceConnectionString { get; set; }
        #endregion

        #region ThienDia
        public string ThienDiaConnectionString { get; set; }
        public string AllowRoomValue { get; set; }
        public int TimmerJackport { get; set; }
        public int TimmerFailRq { get; set; }
        public int TotalFailRq { get; set; }
        public double SecondsRequestSpin { get; set; }
        #endregion

        #region LocThu
        public string LocThuConnectionString { get; set; }
        #endregion

        #region Universe
        public string UniverseConnectionString { get; set; }
        public string SlotMachineLogConnectionString { get; set; }
        public string SlotMachineConnectionString { get; set; }
        public string EnableListBon { get; set; }
        #endregion

        #region Candy
        public int LockTimeout { get; set; }
        public string RoomValue { get; set; }
        public string CandyConnectionString { get; set; }
        #endregion
    }
}
