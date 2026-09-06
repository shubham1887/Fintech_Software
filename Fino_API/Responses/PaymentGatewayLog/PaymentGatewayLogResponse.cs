namespace FintechApp.Responses.PaymentGatewayLog
{
    public class PaymentGatewayLogResponse
    {
    public int ID { get; set; }
    public int UserID { get; set; }
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty;
    public string GatewayName { get; set; } = string.Empty;
    public string Request { get; set; } = string.Empty;
    public string Response { get; set; } = string.Empty;
    public string OrderID { get; set; } = string.Empty;
    public DateTime DateTime { get; set; } = DateTime.MinValue;
    }
}
