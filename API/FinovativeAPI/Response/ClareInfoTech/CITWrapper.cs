using Project.Processing;
using System.Collections.Generic;

namespace Project.Response.ClareInfoTech
{
    public class CITWrapper
    {
        public List<BeneficiaryViewModel> benelist1 = new List<BeneficiaryViewModel>();

        public SenderDetail sender1 = new SenderDetail();

        // public SenderDetail sender { get; set; }

        private string _Messege;

        public string Messege
        {
            get => _Messege.TrimIfNotNull();
            set => _Messege = value.TrimIfNotNull();
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }
    }
}