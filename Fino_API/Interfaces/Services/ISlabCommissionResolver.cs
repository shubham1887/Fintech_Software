namespace FintechApp.Interfaces.Services
{
    // The commissions resolved for one recharge-family transaction from the retailer's slab.
    // Values are SIGNED: a surcharge slab yields a negative value (the user pays extra),
    // a commission slab yields a positive value (the user earns).
    public class ResolvedCommission
    {
        public bool SlabFound { get; set; }

        // What the transacting user earns/pays.
        public decimal RetailerComm { get; set; }

        // Paid only when the matching upline exists in the chain; otherwise 0 and the slab value
        // implicitly stays with admin (Case 2: Admin → Retailer direct).
        public decimal DistributorComm { get; set; }
        public decimal SuperDistributorComm { get; set; }

        public int DistributorUserID { get; set; }
        public int SuperDistributorUserID { get; set; }
    }

    // Resolves the slab commission (or surcharge) for a user + operator + amount, walking the
    // user's own slab mapping first and then the parent chain, mirroring the legacy SlabDetailBAL.
    public interface ISlabCommissionResolver
    {
        Task<ResolvedCommission> Resolve(int userID, int serviceID, int operatorID, decimal amount);
    }
}
