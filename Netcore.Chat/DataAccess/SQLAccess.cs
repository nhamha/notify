using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Netcore.Chat.Interfaces;
using Netcore.Chat.Models;
using NetCore.Utils.Interfaces;
using NetCore.Utils.Log;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Netcore.Chat.DataAccess
{
    public class SQLAccess : ISQLAccess
    {
        private readonly AppSettings _settings;
        private readonly IDBHelper _dbHepler;
        public SQLAccess(IOptions<AppSettings> options, IDBHelper dbHepler)
        {
            _settings = options.Value;
            _dbHepler = dbHepler;
            var connectionString = _settings.ConnectionString;
            _dbHepler.SetConnectionString(connectionString);
        }

        public List<Admin> LoadListAdmin()
        {
            try
            {
                var rs = _dbHepler.GetListSP<Admin>("SP_ChatFiler_LoadAdmins");

                if (rs.Count > 0)
                    return rs;
            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
            }
            return new List<Admin>();
        }

        public List<BlockAccount> LoadListBlockAccount()
        {
            try
            {
                var rs = _dbHepler.GetListSP<BlockAccount>("SP_ChatFiler_LoadBlockAccounts");

                if (rs.Count > 0)
                    return rs;
            }
            catch (Exception e)
            {
                NLogManager.LogException(e);
            }
            return new List<BlockAccount>();
        }

        public List<KeywordReplace> LoadListKeywordReplace()
        {
            try
            {
                var rs = _dbHepler.GetListSP<KeywordReplace>("SP_ChatFiler_LoadKeywordReplaces");

                if (rs.Count > 0)
                    return rs;
            }
            catch (Exception e)
            {
                NLogManager.LogException(e);
            }
            return new List<KeywordReplace>();
        }

        public List<BadWord> LoadListBadWord()
        {
            try
            {
                var rs = _dbHepler.GetListSP<BadWord>("SP_ChatFiler_LoadBadWords");

                if (rs.Count > 0)
                    return rs;
                else
                {
                    return new List<BadWord>();
                }
            }
            catch (Exception e)
            {
                NLogManager.LogException(e);
                return new List<BadWord>();
            }
        }

        public void DeleteBlockedAccount(int id)
        {
            try
            {
                var pars = new SqlParameter[1];
                pars[0] = new SqlParameter("@_id", id);
                _dbHepler.ExecuteNonQuerySP("SP_ChatFiler_DeleteBlockAccount", pars);
            }
            catch (Exception e)
            {
                NLogManager.LogException(e);
            }
        }

        public void BlockAccount(int id)
        {
            try
            {
                var pars = new SqlParameter[6];
                pars[0] = new SqlParameter("@_name", id);
                pars[1] = new SqlParameter("@_accountid", id);
                pars[2] = new SqlParameter("@_reasonblockid", id);
                pars[3] = new SqlParameter("@_namereasonblock", id);
                pars[4] = new SqlParameter("@_typeblock", id);
                pars[5] = new SqlParameter("@_endtimeblock", id);
                _dbHepler.ExecuteNonQuerySP("SP_ChatFiler_InsertBlockAccount", pars);
            }
            catch (Exception e)
            {
                NLogManager.LogException(e);
            }
        }
        public List<ChatDB> LoadListLastMessage(string channelId)
        {
            try
            {
                var pars = new SqlParameter[1];
                pars[0] = new SqlParameter("@_ChannelId", channelId);
                var rs = _dbHepler.GetListSP<ChatDB>("SP_ChatFiler_LoadListLastMessage", pars);
                if (rs.Count > 0)
                    return rs;
                else
                {
                    return new List<ChatDB>();
                }
            }
            catch (Exception e)
            {
                NLogManager.LogException(e);
                return new List<ChatDB>();
            }
        }
        public void SP_Chat_Insert(int _AccountID, string _AccountName, string _NickName, string _ChannerlID, string _Message)
        {
            try
            {
                var pars = new SqlParameter[5];
                pars[0] = new SqlParameter("@_AccountName", _AccountName);
                pars[1] = new SqlParameter("@_AccountID", _AccountID);
                pars[2] = new SqlParameter("@_NickName", _NickName);
                pars[3] = new SqlParameter("@_ChannerlID", _ChannerlID);
                pars[4] = new SqlParameter("@_Message", _Message);
                _dbHepler.ExecuteNonQuerySP("SP_Chat_Insert", pars);
            }
            catch (Exception ex)
            {
                NLogManager.LogInfo(">>> Exception SP_GroupFuntions_Edit: " + ex.Message);
                //throw (new Exception(ex.Message));
            }
        }

        //public List<SP_AccountName_Chat_Get_List> SP_AccountName_Chat_Get_List(int _AccountID, string _AccountName)
        //{
        //    try
        //    {
        //        var oCommand = new SqlCommand("cms.SP_AccountName_Chat_Get_List");
        //        oCommand.CommandType = CommandType.StoredProcedure;
        //        oCommand.Parameters.Add(new SqlParameter("@_AccountID", _AccountID));
        //        oCommand.Parameters.Add(new SqlParameter("@_AccountName", _AccountName));
        //        var lRet = db.GetList<SP_AccountName_Chat_Get_List>(oCommand);
        //        return lRet;
        //    }
        //    catch (Exception ex)
        //    {
        //        NLogManager.LogInfo(">>> Exception SP_AccountName_Chat_Get_List: " + ex.Message);
        //        throw (new Exception(ex.Message));
        //    }
        //}

        //#region [TABLE USER]
        //public void SP_User_Login(string _Email, string _ClientIP, out int _UserID, out int _ResponseStatus)
        //{
        //    try
        //    {
        //        var oCommand = new SqlCommand("cms.SP_User_Login");
        //        oCommand.CommandType = CommandType.StoredProcedure;
        //        oCommand.Parameters.Add(new SqlParameter("@_Email", _Email));
        //        oCommand.Parameters.Add(new SqlParameter("@_ClientIP", _ClientIP));
        //        var p_UserID = new SqlParameter("@_UserID", SqlDbType.Int);
        //        p_UserID.Direction = ParameterDirection.Output;
        //        oCommand.Parameters.Add(p_UserID);
        //        var p_ResponseStatus = new SqlParameter("@_ResponseStatus", SqlDbType.Int);
        //        p_ResponseStatus.Direction = ParameterDirection.Output;
        //        oCommand.Parameters.Add(p_ResponseStatus);
        //        db.ExecuteNonQuery(oCommand);
        //        _UserID = (int)p_UserID.Value;
        //        _ResponseStatus = (int)p_ResponseStatus.Value;
        //    }
        //    catch (Exception ex)
        //    {
        //        NLogManager.LogInfo(">>> Exception SP_User_Login: " + ex.Message);
        //        throw (new Exception(ex.Message));
        //    }
        //}
        //public List<ListUser> SP_User_GetAll()
        //{
        //    try
        //    {
        //        var oCommand = new SqlCommand("cms.SP_User_GetAll");
        //        oCommand.CommandType = CommandType.StoredProcedure;
        //        var lRet = db.GetList<ListUser>(oCommand);
        //        return lRet;
        //    }
        //    catch (Exception ex)
        //    {
        //        NLogManager.LogInfo(">>> Exception SP_User_GetAll: " + ex.Message);
        //        throw (new Exception(ex.Message));
        //    }
        //}
        //public void SP_User_Edit(int _ExeType, int _ExeUserID, string _ExeEmail, int _GroupID, int _UserID, string _Email, string _FullName, bool _IsActive, out int _ResponseStatus)
        //{
        //    try
        //    {
        //        var oCommand = new SqlCommand("cms.SP_User_Edit");
        //        oCommand.CommandType = CommandType.StoredProcedure;
        //        oCommand.Parameters.Add(new SqlParameter("@_ExeType", _ExeType));
        //        oCommand.Parameters.Add(new SqlParameter("@_ExeUserID", _ExeUserID));
        //        oCommand.Parameters.Add(new SqlParameter("@_ExeEmail", _ExeEmail));
        //        oCommand.Parameters.Add(new SqlParameter("@_GroupID", _GroupID));
        //        oCommand.Parameters.Add(new SqlParameter("@_UserID", _UserID));
        //        oCommand.Parameters.Add(new SqlParameter("@_Email", _Email));
        //        oCommand.Parameters.Add(new SqlParameter("@_FullName", _FullName));
        //        oCommand.Parameters.Add(new SqlParameter("@_IsActive", _IsActive));
        //        var p_ResponseStatus = new SqlParameter("@_ResponseStatus", SqlDbType.Int);
        //        p_ResponseStatus.Direction = ParameterDirection.Output;
        //        oCommand.Parameters.Add(p_ResponseStatus);
        //        db.ExecuteNonQuery(oCommand);
        //        _ResponseStatus = (int)p_ResponseStatus.Value;
        //    }
        //    catch (Exception ex)
        //    {
        //        NLogManager.LogInfo(">>> Exception SP_User_Edit: " + ex.Message);
        //        throw (new Exception(ex.Message));
        //    }
        //}
        //#endregion
    }
}