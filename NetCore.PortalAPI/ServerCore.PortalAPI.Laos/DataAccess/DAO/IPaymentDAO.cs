using System;
using ServerCore.DataAccess.DTO;

namespace ServerCore.DataAccess.DAO
{
    public interface IPaymentDAO
    {
        [Obsolete("Old version of billing Vcoin - use method TopupStarByVTCCard", false)]
        int TopupVcoinByVTCCard(ref int vcoin, ref int gift, ref int totalVcoin, VTOCard cardInfo);

        int TopupStarByVTCCard(ref int vcoin, ref int gift, ref int totalVcoin, ref long totalStar, VTOCard cardInfo);
    }
}