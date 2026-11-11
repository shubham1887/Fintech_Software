using FintechApp.Requests.Tds;
using FintechApp.Responses.Tds;

namespace FintechApp.Interfaces.Services
{
    /// <summary>
    /// The TDS desk: the rules, a way to try one before a month is deducted on it, the monthly
    /// list of who owes what, and the two things that can be done with a month of it.
    /// </summary>
    public interface ITdsAdminService
    {
        /// <summary>Every rule, most specific first, with the scope spelled out in words.</summary>
        Task<TdsSettingView> GetRules();

        /// <summary>Create or update one rule. Returns the id, or a refusal the caller can act on.</summary>
        Task<(int RuleID, string Failure)> SaveRule(SaveTdsRuleRequest request);

        /// <summary>Withdraw a rule. The house default may be withdrawn — unlike GST, there is no
        /// statutory floor here, and "no rule" is a legitimate state meaning "deduct nothing".</summary>
        Task<string> DeleteRule(int ruleId);

        /// <summary>
        /// Work a commission through whichever rule wins and show every step in words. The same
        /// reason the tax simulator exists: a rate that looks like 0.01% and behaves like 1% is
        /// worth catching before a month of deductions is booked on it.
        /// </summary>
        Task<(TdsPreviewResponse? Preview, string Failure)> Preview(TdsPreviewRequest request);

        /// <summary>
        /// One month, one row per party: what was withheld, how many deductions it is made of,
        /// whether the money was actually collected, and what has been done with it.
        /// </summary>
        Task<TdsMonthlyReport> MonthlyReport(TdsMonthlyReportRequest request);

        /// <summary>Every deduction behind one party's monthly figure — what a certificate lists.</summary>
        Task<TdsPartyDetailResponse> PartyDetail(string periodKey, int partyUserID);

        /// <summary>
        /// Record that a party's withheld TDS for the period was DEPOSITED with the government,
        /// against its challan reference. It records a fact; it moves no money here.
        /// </summary>
        Task<string> MarkDeposited(TdsDepositRequest request);

        /// <summary>
        /// Give the withheld TDS back to the party as wallet balance instead of depositing it.
        /// This MOVES REAL MONEY, so it is Admin's alone and it is idempotent per (period, party).
        /// </summary>
        Task<(decimal Credited, string Failure)> SettleToWallet(TdsWalletSettleRequest request);
    }
}
