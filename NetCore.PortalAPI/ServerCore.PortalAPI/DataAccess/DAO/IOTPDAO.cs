using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ServerCore.DataAccess.DTO;
using ServerCore.Utilities.Utils;

namespace ServerCore.DataAccess.DAO
{
    public interface IOTPDAO
    {
        OTPs.ResponMes SetupAppToken(OTPs.SetupAppTokenOutput input);

        //OTPs.ResponMes SetupAppTokenActive(OTPs.SetupAppTokenOutput input);


        int AccountVerifySMS(OTPs.AccountVerifySms input);

        int TokensSyncTime(OTPs.CheckActiveSyncTime input);



        int TokensCheckActive(OTPs.CheckActiveSyncTime input);

        int AccountVerifyOTP(OTPs.AccountVerifyOTPInput input);

        int UpdateInfo(OTPs.UpdateInfo input);
        int ActiveTwoFactor(OTPs.ActiveFactor input);
        int GetTwoFactorInfo(long accountID, out string tokenKey, out string secretKey);
    }
}
