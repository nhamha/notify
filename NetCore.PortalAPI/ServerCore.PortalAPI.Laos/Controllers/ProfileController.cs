using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNetCore.Mvc;
using ServerCore.DataAccess.DAO;
using ServerCore.Utilities.Sessions;
using ServerCore.Utilities.Models;
using ServerCore.Utilities.Utils;

namespace PortalAPI.Controllers
{
    [Route("Profile")]
    public class ProfileController : ControllerBase
    {
        private readonly IBettingGameDAO _gameDAO;
        private readonly AccountSession _accountSession;

        public ProfileController(AccountSession accountSession, IBettingGameDAO gameDAO)
        {
            this._accountSession = accountSession;
            this._gameDAO = gameDAO;
        }

        /// <summary>
        /// get user profile
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetUserProfile")]
        public ActionResult<ResponseBuilder> GetUserProfile(byte gameId, long accountID)
        {
            if (accountID < 1)
            {
                return new ResponseBuilder(ErrorCodes.DATA_INVALID, _accountSession.Language);
            }
            //if (Request.Method == HttpMethod.Options)
            //{
            //    return new ResponseBuilder(ErrorCodes.OK, "Accept HttpOptions", "Accept HttpOptions");
            //}
            try
            {
                long accountid = _accountSession.AccountID;
                if (accountid <= 0 || accountid != accountID)
                {
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_NOT_LOGGED_IN, _accountSession.Language);
                }
                List<ProfileDb> list = _gameDAO.GetProfile(accountID, gameId, 99);

                if (list == null)
                {
                    return new ResponseBuilder(ErrorCodes.PROFILE_NOT_FOUND, _accountSession.Language);
                }

                string ip = "";
                if (accountID == accountid)
                    ip = StringUtil.MaskIpAddress(_accountSession.IpAddress);

                var maxLogin = -1;
                int count = list.Count;
                for (int i = 0; i < count; i++)
                {
                    int winlose = list[i].TotalWin + list[i].TotalLose;
                    if (winlose < list[i].TotalPlayGameInDay)
                    {
                        list[i].TotalLose += (list[i].TotalPlayGameInDay - winlose);
                    }
                    if (list[i].NumberLoginTimes > maxLogin)
                    {
                        maxLogin = list[i].NumberLoginTimes;
                    }

                    if (list[i].AccountName.Equals(list[i].CharacterName))
                    {
                        list[i].AccountName = list[i].CharacterName = StringUtil.MaskUserName(list[i].AccountName);
                    }
                    else
                    {
                        list[i].AccountName = StringUtil.MaskUserName(list[i].AccountName);
                    }
                    list[i].IpAddress = ip;
                }

                if (count < 2)
                {
                    if (!list.Exists(p => p.RoomType == 0))
                    {
                        list.Add(new ProfileDb { AccountID = accountID, RoomType = 0, NumberLoginTimes = maxLogin, CharacterName = "", IpAddress = "" });
                    }

                    if (!list.Exists(p => p.RoomType == 1))
                    {
                        list.Add(new ProfileDb { AccountID = accountID, RoomType = 1, CharacterName = "", IpAddress = "" });
                    }
                }
                return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, JsonConvert.SerializeObject(list));
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return new ResponseBuilder(ErrorCodes.SYSTEM_ERROR, _accountSession.Language);
        }

        /// <summary>
        /// Getleaderboard
        /// </summary>
        /// <returns></returns>
        [HttpGet("getLeaderBoard")]
        public ActionResult<ResponseBuilder> GetLeaderBoard(byte gameId, byte roomType)
        {
            //if (Request.Method == HttpMethod.Options)
            //{
            //    return new ResponseBuilder(ErrorCodes.OK, "Accept HttpOptions", "Accept HttpOptions");
            //}

            if (_accountSession.AccountID <= 0 || string.IsNullOrEmpty(_accountSession.AccountName) || roomType > 1)
            {
                return new ResponseBuilder(ErrorCodes.DATA_INVALID, _accountSession.Language);
            }

            var listTotalCoin = _gameDAO.GetLeaderBoard(gameId, 1, false);
            if (listTotalCoin == null)
            {
                return new ResponseBuilder(ErrorCodes.LEADER_BOARD_NOT_FOUND, _accountSession.Language);
            }
            else
            {
                for (int i = 0; i < listTotalCoin.Count; i++)
                {
                    listTotalCoin[i].AccountName = StringUtil.MaskUserName(listTotalCoin[i].AccountName);
                }
            }

            var listTotalXu = _gameDAO.GetLeaderBoard(gameId, 0, false);
            if (listTotalXu != null)
            {
                for (int i = 0; i < listTotalXu.Count; i++)
                {
                    listTotalXu[i].AccountName = StringUtil.MaskUserName(listTotalXu[i].AccountName);
                }
            }
            else
            {
                listTotalXu = new List<ProfileDb>();
            }

            //var listWeek = _gameDAO.GetLeaderBoard(gameId, roomType, true);
            //if (listWeek != null)
            //{
            //    for (int i = 0; i < listWeek.Count; i++)
            //    {
            //        listWeek[i].Mobile = StringUtil.MaskUserName(listWeek[i].Mobile);
            //    }
            //}
            //else
            //{
            //    listWeek = new List<ProfileDb>();
            //}

            List<List<ProfileDb>> data = new List<List<ProfileDb>>(2);
            data.Add(listTotalCoin);
            data.Add(listTotalXu);

            return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, data);
        }

        [HttpGet("getProfileStatus")]
        public ActionResult<ResponseBuilder> GetProfileStatus()
        {
            //if (Request.Method == HttpMethod.Options)
            //{
            //    return new ResponseBuilder(ErrorCodes.OK, "Accept HttpOptions", "Accept HttpOptions");
            //}
            try
            {
                long accountid = _accountSession.AccountID;
                if (accountid < 1)
                {
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_NOT_LOGGED_IN, _accountSession.Language);
                }
                List<ProfileDb> list = _gameDAO.GetAllProfile(accountid);
                int profileStatus = 0;
                if (list == null) profileStatus = 0;
                if (list != null)
                {
                    if (list.Count == 0)
                    {
                        //chua choi van nao
                        profileStatus = 0;
                    }
                    else if (list.Count == 1 && (list[0].TotalWin + list[0].TotalLose == 1))
                    {
                        profileStatus = 1;//da choi 1 van
                    }
                    else
                    {
                        profileStatus = 2;//choi nhieu hon 1 van
                    }
                }
                return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, new { profileStatus = profileStatus, accountId = accountid });

            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return new ResponseBuilder(ErrorCodes.SYSTEM_ERROR, _accountSession.Language);

            }
        }
    }
}