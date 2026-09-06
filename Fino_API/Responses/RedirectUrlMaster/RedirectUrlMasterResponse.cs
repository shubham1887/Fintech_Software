namespace FintechApp.Responses.RedirectUrlMaster
{
    public class RedirectUrlMasterResponse
    {
    public int RedirectUrlMasterID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string RedirectUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    }
}
