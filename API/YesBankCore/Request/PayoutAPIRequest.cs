using Newtonsoft.Json;

namespace YesBankCore.Request
{
    internal class PayoutAPIRequest
    {
        [JsonProperty("Data")]
        public Data Data { get; set; }

        [JsonProperty("Risk")]
        public Risk Risk { get; set; }

        public PayoutAPIRequest()
        {
            Data = new Data();
            Risk = new Risk();
        }
    }

    internal class Data
    {
        [JsonProperty("ConsentId")]
        public string ConsentId { get; set; }

        [JsonProperty("Initiation")]
        public Initiation Initiation { get; set; }

        public Data()
        {
            Initiation = new Initiation();
        }
    }

    internal class Initiation
    {
        [JsonProperty("InstructionIdentification")]
        public string OrderID { get; set; }

        [JsonProperty("EndToEndIdentification")]
        public string EndToEndIdentification { get; set; }

        [JsonProperty("InstructedAmount")]
        public InstructedAmount InstructedAmount { get; set; }

        [JsonProperty("DebtorAccount")]
        public DebtorAccount DebtorAccount { get; set; }

        [JsonProperty("CreditorAccount")]
        public CreditorAccount CreditorAccount { get; set; }

        [JsonProperty("RemittanceInformation")]
        public RemittanceInformation RemittanceInformation { get; set; }

        [JsonProperty("ClearingSystemIdentification")]
        public string ClearingSystemIdentification { get; set; }

        public Initiation()
        {
            InstructedAmount = new InstructedAmount();
            DebtorAccount = new DebtorAccount();

            CreditorAccount = new CreditorAccount();
            RemittanceInformation = new RemittanceInformation();
        }
    }

    internal class CreditorAccount
    {
        [JsonProperty("SchemeName")]
        public string SchemeName { get; set; }

        [JsonProperty("Identification")]
        public string Identification { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Unstructured")]
        public CreditorAccountUnstructured Unstructured { get; set; }

        public CreditorAccount()
        {
            Unstructured = new CreditorAccountUnstructured();
        }
    }

    internal class CreditorAccountUnstructured
    {
        [JsonProperty("ContactInformation")]
        public ContactInformation ContactInformation { get; set; }

        public CreditorAccountUnstructured()
        {
            ContactInformation = new ContactInformation();
        }
    }

    internal class ContactInformation
    {
        [JsonProperty("EmailAddress")]
        public string EmailAddress { get; set; }

        [JsonProperty("MobileNumber")]
        public string MobileNumber { get; set; }
    }

    internal class DebtorAccount
    {
        [JsonProperty("Identification")]
        public string Identification { get; set; }

        [JsonProperty("SecondaryIdentification")]
        public string SecondaryIdentification { get; set; }
    }

    internal class InstructedAmount
    {
        [JsonProperty("Amount")]
        public string Amount { get; set; }

        [JsonProperty("Currency")]
        public string Currency { get; set; }
    }

    internal class RemittanceInformation
    {
        [JsonProperty("Reference")]
        public string Reference { get; set; }

        [JsonProperty("Unstructured")]
        public RemittanceInformationUnstructured Unstructured { get; set; }

        public RemittanceInformation()
        {
            Unstructured = new RemittanceInformationUnstructured();
        }
    }

    internal class RemittanceInformationUnstructured
    {
        [JsonProperty("CreditorReferenceInformation")]
        public string CreditorReferenceInformation { get; set; }
    }

    internal class Risk
    {
        [JsonProperty("DeliveryAddress")]
        public DeliveryAddress DeliveryAddress { get; set; }

        public Risk()
        {
            DeliveryAddress = new DeliveryAddress();
        }
    }

    internal class DeliveryAddress
    {
        [JsonProperty("AddressLine")]
        public List<string> AddressLine { get; set; }

        [JsonProperty("StreetName")]
        public string StreetName { get; set; }

        [JsonProperty("BuildingNumber")]
        public string BuildingNumber { get; set; }

        [JsonProperty("PostCode")]
        public string PostCode { get; set; }

        [JsonProperty("TownName")]
        public string TownName { get; set; }

        [JsonProperty("CountySubDivision")]
        public List<string> CountySubDivision { get; set; }

        [JsonProperty("Country")]
        public string Country { get; set; }
    }
}