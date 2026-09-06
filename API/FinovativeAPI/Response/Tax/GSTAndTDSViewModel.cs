using Project.Processing;

namespace Project.Response.Tax
{
    public class GSTAndTDSViewModel
    {
        private decimal _RetailerTDS;

        public decimal RetailerTDS
        {
            get => _RetailerTDS;
            set => _RetailerTDS = value;
        }

        private decimal _DistTDS;

        public decimal DistTDS
        {
            get => _DistTDS;
            set => _DistTDS = value;
        }

        private decimal _SuperDistTDS;

        public decimal SuperDistTDS
        {
            get => _SuperDistTDS;
            set => _SuperDistTDS = value;
        }

        private decimal _RetailerGST;

        public decimal RetailerGST
        {
            get => _RetailerGST;
            set => _RetailerGST = value;
        }

        private decimal _DistGST;

        public decimal DistGST
        {
            get => _DistGST;
            set => _DistGST = value;
        }

        private decimal _SuperDistGST;

        public decimal SuperDistGST
        {
            get => _SuperDistGST;
            set => _SuperDistGST = value;
        }

        private string _RetTaxDescrption;

        public string RetTaxDescrption
        {
            get => _RetTaxDescrption.TrimIfNotNull();
            set => _RetTaxDescrption = value.TrimIfNotNull();
        }

        private string _DistTaxDescrption;

        public string DistTaxDescrption
        {
            get => _DistTaxDescrption.TrimIfNotNull();
            set => _DistTaxDescrption = value.TrimIfNotNull();
        }

        private string _SuperDistTaxDescrption;

        public string SuperDistTaxDescrption
        {
            get => _SuperDistTaxDescrption.TrimIfNotNull();
            set => _SuperDistTaxDescrption = value.TrimIfNotNull();
        }
    }
}