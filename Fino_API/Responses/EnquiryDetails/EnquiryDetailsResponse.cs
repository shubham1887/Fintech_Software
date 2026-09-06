namespace FintechApp.Responses.EnquiryDetails
{
    public class EnquiryDetailsResponse
    {
    public int EnquiryDetailsID { get; set; }
    public string PersonName { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string EmailID { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string EnquiryType { get; set; } = string.Empty;
    public string Remark { get; set; } = string.Empty;
    public string AdminStatus { get; set; } = string.Empty;
    public string AdminRemark { get; set; } = string.Empty;
    public DateTime DateTime { get; set; } = DateTime.MinValue;
    }
}
