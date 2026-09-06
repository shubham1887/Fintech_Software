using MplanCore.Controller;
using Newtonsoft.Json;

namespace MplanCore.Response
{
    public class DTHPlansRecords
    {
        [JsonProperty("records")]
        public DTHRecords DTHRecord { get; set; }

        public DTHPlansRecords()
        {
            DTHRecord = new DTHRecords();
        }
    }

    public partial class DTHRecords
    {
        [JsonProperty("Add-On Pack")]
        public AddOnPack[] AddOnPack { get; set; }

        [JsonProperty("Plan")]
        public Plan[] Plan { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }

    public partial class AddOnPack
    {
        private string _Desc;

        [JsonProperty("desc")]
        public string Desc
        {
            get => _Desc.TrimIfNotNull();
            set => _Desc = value.TrimIfNotNull();
        }

        private string _LastUpdate;

        [JsonProperty("last_update")]
        public string LastUpdate
        {
            get => _LastUpdate.TrimIfNotNull();
            set => _LastUpdate = value.TrimIfNotNull();
        }

        private string _PlanName;

        [JsonProperty("plan_name")]
        public string PlanName
        {
            get => _PlanName.TrimIfNotNull();
            set => _PlanName = value.TrimIfNotNull();
        }

        [JsonProperty("rs")]
        public AddOnPackRs Rs { get; set; }

        public AddOnPack()
        {
            Rs = new AddOnPackRs();
        }

        private string _month1;

        [JsonProperty("1Month")]
        public string month1
        {
            get => _month1.TrimIfNotNull();
            set => _month1 = value.TrimIfNotNull();
        }
    }

    public partial class AddOnPackRs
    {
        private string _The1Months;

        [JsonProperty("1 MONTHS")]
        public string The1Months
        {
            get
            {
                string result = _The1Months ?? The3Months ?? The6Months ?? The1Year;
                return result.TrimIfNotNull();
            }
            set => _The1Months = value.TrimIfNotNull();
        }

        [JsonProperty("3 MONTHS")]
        public string The3Months { get; set; }

        [JsonProperty("6 MONTHS")]
        public string The6Months { get; set; }

        [JsonProperty("1 YEAR")]
        public string The1Year { get; set; }
    }

    public partial class Plan
    {
        private string _Desc;

        [JsonProperty("desc")]
        public string Desc
        {
            get => _Desc.TrimIfNotNull();
            set => _Desc = value.TrimIfNotNull();
        }

        private string _LastUpdate;

        [JsonProperty("last_update")]
        public string LastUpdate
        {
            get => _LastUpdate.TrimIfNotNull();
            set => _LastUpdate = value.TrimIfNotNull();
        }

        private string _PlanName;

        [JsonProperty("plan_name")]
        public string PlanName
        {
            get => _PlanName.TrimIfNotNull();
            set => _PlanName = value.TrimIfNotNull();
        }

        [JsonProperty("rs")]
        public PlanRs Rs { get; set; }

        public Plan()
        {
            Rs = new PlanRs();
        }

        private string _month1;

        [JsonProperty("1Month")]
        public string month1
        {
            get => _month1.TrimIfNotNull();
            set => _month1 = value.TrimIfNotNull();
        }
    }

    public partial class PlanRs
    {
        private string _The1Months;

        [JsonProperty("1 MONTHS")]
        public string The1Months
        {
            get
            {
                string result = _The1Months ?? The3Months ?? The6Months ?? The1Year;
                return result.TrimIfNotNull();
            }
            set => _The1Months = value.TrimIfNotNull();
        }

        [JsonProperty("3 MONTHS")]
        public string The3Months { get; set; }

        [JsonProperty("6 MONTHS")]
        public string The6Months { get; set; }

        [JsonProperty("1 YEAR")]
        public string The1Year { get; set; }
    }
}