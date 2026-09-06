namespace FintechApp.Responses.SliderMaster
{
    public class SliderMasterResponse
    {
        public int SliderMasterID { get; set; }
        public string SliderPath { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
    }
}
