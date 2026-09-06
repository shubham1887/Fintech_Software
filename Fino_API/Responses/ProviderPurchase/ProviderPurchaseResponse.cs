namespace FintechApp.Responses.ProviderPurchase
{
    public class ProviderPurchaseResponse
    {
    public int ProviderPurchaseID { get; set; }
    public int ProviderID { get; set; }
    public int WalletID { get; set; }
    public decimal Amount { get; set; }
    public string Remark { get; set; } = string.Empty;
    public DateTime PurchaseDate { get; set; } = DateTime.MinValue;
    public int PurchaseDOCInt { get; set; }
    public bool IsDelete { get; set; }
    public DateTime DateTime { get; set; } = DateTime.MinValue;
    }
}
