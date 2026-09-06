namespace FintechApp.Requests.SubMode
{
    // Drives the dependent dropdown on the Fund Request page: given the selected Mode,
    // return that Mode's SubModes.
    public class GetSubModeByModeRequest
    {
        public int ModeID { get; set; }
    }
}
