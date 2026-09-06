using System.Collections.Generic;

namespace Project.Response.TransactionMaster
{
    public class LiveTransactionWrapper
    {
        public List<ViewModel> list = new List<ViewModel>();

        private HeaderViewModel headerSummary;

        public HeaderViewModel HeaderSummary
        {
            get => headerSummary;
            set => headerSummary = value;
        }
    }
}