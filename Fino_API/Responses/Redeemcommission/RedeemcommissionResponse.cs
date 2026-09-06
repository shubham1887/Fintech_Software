namespace FintechApp.Responses.Redeemcommission
{
    public class RedeemcommissionResponse
    {
    public int RedeemcommissionID { get; set; }
    public int UserID { get; set; }
    public int WalletID { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public decimal Amount { get; set; }
    public string AdminRemark { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string LastYearMonth { get; set; } = string.Empty;
    public DateTime DateTime { get; set; } = DateTime.MinValue;
    }
}
