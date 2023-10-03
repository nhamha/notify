using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServerCore.Utilities.Interfaces;
using ServerCore.Utilities.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ServerCore.Utilities.Facebook
{
    public class FacebookUtil
    {
        private readonly IDataService _dataService;
        public FacebookUtil(IDataService dataService)
        {
            _dataService = dataService;
        }

        public FBAccount GetFBAccount(string UserInformation)
        {
            try
            {
                NLogManager.Info(UserInformation);
                var FacebookResponseData = JObject.Parse(UserInformation);

                string facebookId = "";
                string fbName = "";

                if (FacebookResponseData["id"] != null)
                {
                    facebookId = (string)FacebookResponseData["id"];
                }

                if (FacebookResponseData["name"] != null)
                {
                    fbName = StringUtil.ConvertUnicodeToString((string)FacebookResponseData["name"]);
                }

                var FbAccount = new FBAccount(facebookId, fbName, "");
                return FbAccount;
            }
            catch (Exception exx)
            {
                NLogManager.Exception(exx);
                return null;
            }
        }
        private readonly ConcurrentDictionary<string, string> _usersBusiness = new ConcurrentDictionary<string, string>();
        public async  Task<string> GetIDsForBusiness(string fbid, string accessToken, string graphUrl)
        {
          
            var returnList = new List<IDs_Business>();
            try
            {
                bool isExist = _usersBusiness.ContainsKey(fbid);
                if (isExist)
                {
                    string bussinessValue = "";
                    _usersBusiness.TryGetValue(fbid, out bussinessValue);
                    if (bussinessValue != null && bussinessValue.Length > 0)
                    {
                        NLogManager.Info("business_data Cache: " + bussinessValue);
                       // returnList = JsonConvert.DeserializeObject<List<IDs_Business>>(bussinessValue);
                        return bussinessValue;
                    }
                }
                    
                // xử lý lấy listapp theo business để có được scope-userid
                var requestLink = string.Format(graphUrl, accessToken);
                var business = await _dataService.GetAsync(requestLink); //GetHttpResponse(requestLink);
                var business_data = business.ToString().Substring(business.ToString().IndexOf('['), business.ToString().IndexOf(']') - business.ToString().IndexOf('[') + 1);
                NLogManager.Info("business_data: " + business_data);
                if (business_data != "[]")
                {
                    var businessInfo = business_data.Replace("namespace", "name_space");
                    returnList = JsonConvert.DeserializeObject<List<IDs_Business>>(businessInfo);
                }

                string listPartnerIDs = "";
                if (returnList != null)
                {
                    foreach (var item in returnList)
                        listPartnerIDs += ";" + item.id;
                }

                if (listPartnerIDs.IndexOf(";") == 0)
                    listPartnerIDs = listPartnerIDs.Substring(1);
                _usersBusiness.TryAdd(fbid, listPartnerIDs);
                return listPartnerIDs;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return "";
            }
        }
    }
}
