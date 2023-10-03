namespace ServerCore.PortalAPI.Models
{
    public class UserRevenueAffilicateModel
    {
        public int UserParentActiveCount { get; set; }
	    public int UserOtherActiveCount { get; set; }
        public int UserNewParentCount { get; set; }
	    public int UserNewOtherCount { get; set; }
        public int TotalUserParent { get; set; }
	    public int TotalUserOther { get; set; }
        public decimal UserAffilicateParentTotal { get; set; }
	    public decimal UserAffilicateParent { get; set; }
        public decimal UserAffilicateOther { get; set; }
	    public decimal UserAmountParentTotal { get; set; }
        public decimal UserAmountParent { get; set; }
	    public decimal UserAmountOtherTotal { get; set; }
    }
}
