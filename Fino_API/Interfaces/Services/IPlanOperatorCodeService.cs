namespace FintechApp.Interfaces.Services
{
    /// <summary>
    /// Translates one of the app's own OperatorMaster rows into the code the plan-data provider
    /// (planapi.in / Easytm) expects. The two code sets are unrelated: OperatorMaster.Opcode holds
    /// the mnemonic used to ROUTE a recharge through the transaction provider ("AIRT", "TPLY"),
    /// while every plan / R-Offer / DTH lookup needs the plan provider's numeric code ("2", "28").
    /// </summary>
    public interface IPlanOperatorCodeService
    {
        /// <summary>
        /// The plan-provider OpCode for an app operator, or an empty string when the operator has
        /// no counterpart in the provider's catalog (e.g. MTNL prepaid, which it does not publish).
        /// </summary>
        Task<string> ResolveOpCode(int operatorId);
    }
}
