using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using ServerCore.Utilities.Utils;
using ServerCore.Utilities.Sessions;
using ServerCore.Utilities.Captcha;
using ServerCore.Utilities.Models;
using ServerCore.DataAccess.DAO;
using ServerCore.PortalAPI.Models;
using ServerCore.PortalAPI.Services;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;

namespace PortalAPI.Controllers
{
    [Route("Guild")]
    [ApiController]
    public class GuildController : ControllerBase
    {
        private readonly IAgencyDAO _agencyDAO;

        private readonly AccountSession _accountSession;

        private static string _loginActionName;
        private static int _loginFailAllow;
        private static int _loginFailTime;
        private static int _maxLengthUserName;
        private static int _maxLengthNickName;
        private readonly AppSettings _appSettings;
        private IAuthenticateService _authenticateService;

        public GuildController(AccountSession accountSession, IAgencyDAO agencyDAO, IAuthenticateService authenticateService, IOptions<AppSettings> options)
        {
            this._accountSession = accountSession;
            this._agencyDAO = agencyDAO;

            _appSettings = options.Value;
            _authenticateService = authenticateService;

            _maxLengthUserName = _appSettings.MaxLengthUserName;
            _maxLengthNickName = _appSettings.MaxLengthNickName;
            _loginFailAllow = _appSettings.LoginFailAllow;
            _loginFailTime = _appSettings.LoginFailTime;
            _loginActionName = _appSettings.LoginActionName;
        }
        #region Bang hội
        /// <summary>
        /// Danh sách bang hội level 1,
        /// có tổng số 20 bang hội
        /// 
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet("GameAgencies")]
        public ActionResult<ResponseBuilder> GameAgencies()
        {
            try
            {
                if (_accountSession.AccountID <= 0 || string.IsNullOrEmpty(_accountSession.AccountName))
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_NOT_LOGGED_IN, _accountSession.Language);

                GameAgency AdminGuild = null;
                List<GameAgency> listAg = _agencyDAO.GetGameAgencies();
                foreach(GameAgency item in listAg)
                {
                    if (item.AccountID == _accountSession.AccountID)
                    {
                        AdminGuild = item;
                        break;
                    }
                }

                // Bang chu
                if (AdminGuild != null)
                {
                   // AdminGuild.Members = _agencyDAO.GetListMember(_accountSession.AccountID, 1);
                    //AdminGuild.MemberRequests = _agencyDAO.GetListMember(_accountSession.AccountID, 0);
                  //  AdminGuild.MemberLefts = _agencyDAO.GetListMember(_accountSession.AccountID, 2);
                    foreach (GameAgency item in listAg)
                    {
                        if (item.AccountID != _accountSession.AccountID)
                        {
                            item.ContentRule = null;
                        }
                        else
                        {
                            item.IsMember = 2;
                        }
                    }
                    return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, listAg);
                }
                else // Member
                {
                    long agencyID = _agencyDAO.GetAgencyIDByMember(_accountSession.AccountID);
                    // Chưa gia nhập bang nào
                    if (agencyID > 0)
                    {
                        foreach (GameAgency item in listAg)
                        {
                            if (item.AccountID != agencyID)
                            {
                                item.ContentRule = null;
                            }
                            else
                            {
                                item.IsMember = 1;
                            }
                        }
                    }
                    else
                    {
                        foreach (GameAgency item in listAg)
                        {
                            item.ContentRule = null;
                        }
                    }
                }
             
                return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, listAg);

            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return new ResponseBuilder(ErrorCodes.SYSTEM_ERROR, _accountSession.Language);
            }

        }

        /// <summary>
        /// Danh sách bang hội level 1,
        /// có tổng số 20 bang hội
        /// 
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpPost("UpdateRule")]
        public ActionResult<ResponseBuilder> UpdateRule(dynamic data)
        {
            string content = data.content;
            try
            {
                if (_accountSession.AccountID <= 0 || string.IsNullOrEmpty(_accountSession.AccountName))
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_NOT_LOGGED_IN, _accountSession.Language);

                int responseStatus = _agencyDAO.UpdateRule(_accountSession.AccountID, content);
                return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, responseStatus);

            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return new ResponseBuilder(ErrorCodes.SYSTEM_ERROR, _accountSession.Language);
            }
        }

        /// <summary>
        /// Danh sách bang hội level 1,
        /// có tổng số 20 bang hội
        /// 
        /// Check: Nếu là Bang chủ => lấy ra danh sách thành viên, trả thêm type
        /// nếu là cá nhân
        ///  - Chưa tham gia bang: danh sách bang chủ
        ///  - Đã có bang: danh sách thành viên trong bang
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet("GameAgencyDetail")]
        public ActionResult<ResponseBuilder> GameAgencyDetail()
        {
            try
            {
                if (_accountSession.AccountID <= 0 || string.IsNullOrEmpty(_accountSession.AccountName))
                    return new ResponseBuilder(ErrorCodes.ACCOUNT_NOT_LOGGED_IN, _accountSession.Language);

                GameAgency AdminGuild = null;
                List<GameAgency> listAg = _agencyDAO.GetGameAgencies();
                foreach (GameAgency item in listAg)
                {
                    if (item.AccountID == _accountSession.AccountID)
                    {
                        AdminGuild = item;
                        if (AdminGuild.ContentRule == null)
                            AdminGuild.ContentRule = "";
                        break;
                    }
                }

                // Bang chu
                if (AdminGuild != null)
                {
                    AdminGuild.Members = _agencyDAO.GetListMember(_accountSession.AccountID, 1);
                    AdminGuild.MemberRequests = _agencyDAO.GetListMemberOther(_accountSession.AccountID, 0);
                   // AdminGuild.MemberLefts = _agencyDAO.GetListMemberOther(_accountSession.AccountID, 2);
                   
                    return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, AdminGuild);
                }
                else // Member
                {
                    GameAgency MemGuild = null;
                    long agencyID = _agencyDAO.GetAgencyIDByMember(_accountSession.AccountID);
                    // Chưa gia nhập bang nào
                    if (agencyID > 0)
                    {
                        foreach (GameAgency item in listAg)
                        {
                            if (item.AccountID == agencyID)
                            {
                                MemGuild = item;
                                if (MemGuild.ContentRule == null)
                                    MemGuild.ContentRule = "";
                                break;
                            }
                        }

                        if (MemGuild != null)
                        {
                            MemGuild.Members = _agencyDAO.GetListMember(MemGuild.AccountID, 1);
                            return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, MemGuild);
                        }
                    }
                }

                return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, null);

            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return new ResponseBuilder(ErrorCodes.SYSTEM_ERROR, _accountSession.Language);
            }

        }

        /// <summary>
        /// Gửi yêu cầu tham gia bang hội
        /// </summary>
        /// <param name="AgencyAccountId"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("RequestJoinAgency")]
        public ActionResult<ResponseBuilder> RequestJoinAgency(dynamic data)
        {
            if (_accountSession.AccountID <= 0 || string.IsNullOrEmpty(_accountSession.AccountName))
                return new ResponseBuilder(ErrorCodes.ACCOUNT_NOT_LOGGED_IN, _accountSession.Language);
            int agencyID = data.agencyid;
            int res = _agencyDAO.RequestJoinAgency(agencyID, _accountSession.AccountID, _accountSession.AccountName, _accountSession.NickName);
            //string agencyBlackList = ConfigurationManager.AppSettings["AgencyBlackList"];
            if(res > 0) return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language);
            if(res == -99) return new ResponseBuilder(ErrorCodes.GA_ERR_NOT_HAVE_TRANSACTION_99, _accountSession.Language);
            if (res == -98) return new ResponseBuilder(ErrorCodes.GA_ERR_CAN_NOT_ADD_YOUR_SELF_98, _accountSession.Language);
            if (res == -97) return new ResponseBuilder(ErrorCodes.GA_ERR_ALREADY_REQUESTED_97, _accountSession.Language);
            if (res == -96) return new ResponseBuilder(ErrorCodes.GA_ERR_ALREADY_JOINED_96, _accountSession.Language);
            if (res == -95) return new ResponseBuilder(ErrorCodes.GA_ERR_AGENCY_NOT_EXIST_95, _accountSession.Language);

            return new ResponseBuilder(ErrorCodes.SERVER_ERROR, _accountSession.Language, res);
        }
        /// <summary>
        /// Danh sách thành viên trong Bang
        /// Status:  
        /// 0: Requested : Chờ xác nhận
        /// 1: Accepted  : Đã tham gia
        /// -1: Rejected : Đã từ chối
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("GetListMember")]
        public ActionResult<ResponseBuilder> GetListMember(dynamic data)
        {
            try
            {
                if (_accountSession.AccountID <= 0 || string.IsNullOrEmpty(_accountSession.AccountName))
                return new ResponseBuilder(ErrorCodes.ACCOUNT_NOT_LOGGED_IN, _accountSession.Language);
                int status = data.status;
                int agencyAccountId = data.agencyAccountId;
                // Lấy danh sách thành viên theo account Id của bang chủ
                List<GameMember> list = _agencyDAO.GetListMember(agencyAccountId, status);
                return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language, list);
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return new ResponseBuilder(ErrorCodes.SYSTEM_ERROR, _accountSession.Language);
            }
        }
        /// <summary>
        /// 1: Đồng ý: -1 từ chối yêu cầu
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("AcceptRequest")]
        public ActionResult<ResponseBuilder> AcceptRequest(dynamic data)
        {
            try
            {
                if (_accountSession.AccountID <= 0 || string.IsNullOrEmpty(_accountSession.AccountName))
                return new ResponseBuilder(ErrorCodes.ACCOUNT_NOT_LOGGED_IN, _accountSession.Language);
                int status = data.status;
                if (status > 2 || status < -1)
                {
                    return new ResponseBuilder(ErrorCodes.FORBIDDEN_ACCESS, _accountSession.Language);
                }

                long agencyId = 0;
                string nickName = "";
                int isAgency = 0;
                // Check agency
                AgenciesInfo agency = _agencyDAO.AgencyGetInfo(_accountSession.NickName);
                if (agency.AgencyID < 0 || agency.LevelID > 1)
                {
                    // Là cá nhân
                    if (status != 2)
                        return new ResponseBuilder(ErrorCodes.FORBIDDEN_ACCESS, _accountSession.Language);
                    // Đã Join Bang nào chưa?
                    List<GameJoinRequest> rqJoined = _agencyDAO.GetListRequestJoinAgency((int)_accountSession.AccountID, 1);
                    if (rqJoined == null || rqJoined.Count < 1) // Chưa join Bang nào
                    {
                        return new ResponseBuilder(ErrorCodes.FORBIDDEN_ACCESS, _accountSession.Language);
                    }
                    // Đã gửi yêu cầu join bang nào chưa?
                    List<GameJoinRequest> rq = _agencyDAO.GetListRequestJoinAgency((int)_accountSession.AccountID, 10);
                    if (rq == null || rq.Count < 1) // Chưa join Bang nào
                    {
                        return new ResponseBuilder(ErrorCodes.FORBIDDEN_ACCESS, _accountSession.Language);
                    }

                    agencyId = rqJoined[0].AgencyId; // Lấy ra id bang chủ
                    nickName = _accountSession.NickName; // Chính account đang đăng nhập
                    isAgency = 0;
                }
                if (agency.AgencyID > 0 && agency.LevelID == 1) // Là bang chủ
                {
                    agencyId = _accountSession.AccountID;
                    nickName = data.nickname;
                    isAgency = 1;
                }

                NLogManager.Info(string.Format("{0}-{1}-{2}-{3}", agencyId, nickName, status, isAgency));
                int res = _agencyDAO.AcceptRequestInfo(agencyId, nickName, status, isAgency);
                if (res == -96)
                {
                    return new ResponseBuilder(ErrorCodes.GA_ERR_ALREADY_JOINED_96, _accountSession.Language);
                }
                else if (res < 0)
                {
                    return new ResponseBuilder(ErrorCodes.SERVER_ERROR, _accountSession.Language);
                }

                return new ResponseBuilder(ErrorCodes.SUCCESS, _accountSession.Language);
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return new ResponseBuilder(ErrorCodes.SYSTEM_ERROR, _accountSession.Language);
            }
        }
        #endregion
    }
}