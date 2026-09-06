namespace FintechApp.Responses.PlanSetting
{
    public class PlanSettingResponse
    {
    public int PlanSettingID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string UserID { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public bool IsActiveWeb { get; set; }
    public bool IsActive { get; set; }
    public string Circle { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    }
}
