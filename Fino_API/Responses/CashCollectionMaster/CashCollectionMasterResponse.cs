namespace FintechApp.Responses.CashCollectionMaster
{
    public class CashCollectionMasterResponse
    {
    public int CashCollectionMasterID { get; set; }
    public int UserID { get; set; }
    public decimal Outstanding { get; set; }
    public decimal MaxOutstanding { get; set; }
    public DateTime DateTime { get; set; } = DateTime.MinValue;
    }
}
