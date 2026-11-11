namespace FintechApp.Responses.Credential
{
    /// <summary>
    /// What the client needs to decide whether to prompt for an MPIN. `Required` is the effective
    /// answer (global Key Setting OR the user's own opt-in); `IsSet` says whether the user has one
    /// yet, so the client knows to route to "set MPIN" instead of "enter MPIN".
    /// </summary>
    public class MpinStatusResponse
    {
        public bool Required { get; set; }
        public bool IsSet { get; set; }
        public bool GlobalEnabled { get; set; }
        public bool UserEnabled { get; set; }
    }
}
