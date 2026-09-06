namespace FintechApp.Responses.BBPSConfiguration
{
    public class BBPSConfigurationResponse
    {
        public int BBPSConfigurationID { get; set; }
        public int OperatorID { get; set; }
        public string OperatorName { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string PlaceHolder { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
        public string ErrorMeeage { get; set; } = string.Empty;
        public int Sequence { get; set; }
        public int Length { get; set; }
        public string ConfigList { get; set; } = string.Empty;
        public bool IsDisplay { get; set; }
        public DateTime DateTime { get; set; }
        public DateTime UpdatedateTime { get; set; }
    }
}