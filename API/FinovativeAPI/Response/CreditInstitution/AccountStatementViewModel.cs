using Project.Processing;

namespace Project.Response.CreditInstitution
{
    public class AccountStatementViewModel
    {
        private string _Narration;

        public string Narration
        {
            get => _Narration.TrimIfNotNull();
            set => _Narration = value.TrimIfNotNull();
        }

        private string _Amount;

        public string Amount
        {
            get => _Amount.TrimIfNotNull();
            set => _Amount = value.TrimIfNotNull();
        }

        private string _Balance;

        public string Balance
        {
            get => _Balance.TrimIfNotNull();
            set => _Balance = value.TrimIfNotNull();
        }

        private string _TransDate;

        public string TransDate
        {
            get => _TransDate.TrimIfNotNull();
            set => _TransDate = value.TrimIfNotNull();
        }

        private string _CdType;

        public string CdType
        {
            get => _CdType.TrimIfNotNull();
            set => _CdType = value.TrimIfNotNull();
        }

        
    }
}
