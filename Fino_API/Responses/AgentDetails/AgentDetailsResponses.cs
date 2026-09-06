namespace FintechApp.Responses.AgentDetails
{
    public class PinCodeMasterResponse
    {
        public int SR_NO { get; set; }
        public string PINCODE { get; set; } = string.Empty;
        public string HPY_DISTRICT { get; set; } = string.Empty;
        public string HYP_STATE { get; set; } = string.Empty;
        public string HPY_TALUKA_CITY { get; set; } = string.Empty;
        public string REGIONNAME { get; set; } = string.Empty;
        public string HPY_REGION { get; set; } = string.Empty;
        public string HPY_ZONE { get; set; } = string.Empty;
        public string HPY_AREA { get; set; } = string.Empty;
        public string HPY_NSM { get; set; } = string.Empty;
        public string HPY_RH { get; set; } = string.Empty;
        public string HPY_ZSM { get; set; } = string.Empty;
        public string HPY_SH { get; set; } = string.Empty;
        public string HPY_CM { get; set; } = string.Empty;
        public string HPY_ASM { get; set; } = string.Empty;
        public string HPY_FLS { get; set; } = string.Empty;
        public string ACTIVATED { get; set; } = string.Empty;
        public string ACTIVATION_DATE { get; set; } = string.Empty;
        public string LOCATION { get; set; } = string.Empty;
        public string WITHIN_CITY_LIMITS { get; set; } = string.Empty;
        public string DISTANCE_FROM_CBD { get; set; } = string.Empty;
        public string INDIAPOST_OFFICE_NAME { get; set; } = string.Empty;
        public string DIVISIONNAME { get; set; } = string.Empty;
        public string LONGITUDE { get; set; } = string.Empty;
        public string LATITUDE { get; set; } = string.Empty;
    }

    public class AgentUserWrapper
    {
        public List<PinCodeMasterResponse> List { get; set; } = new();
        public int Count { get; set; }
    }
}
