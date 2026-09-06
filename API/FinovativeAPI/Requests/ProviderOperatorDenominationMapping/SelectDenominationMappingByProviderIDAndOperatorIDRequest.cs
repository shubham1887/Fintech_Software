namespace Project.Requests.ProviderOperatorDenominationMapping
{
    public class SelectDenominationMappingByProviderIDAndOperatorIDRequest
    {
        private int _ProviderID;

        public int ProviderID { get => _ProviderID; set => _ProviderID = value; }

        private int _OperatorID;
        public int OperatorID { get => _OperatorID; set => _OperatorID = value; }
    }
}