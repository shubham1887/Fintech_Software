using Project.Processing;
using System;

namespace Project.Response.Invoice
{
    public class ServicewiseSaleViewModel
    {
        public int ServiceID { get; set; }

        public string ServiceName { get; set; }

        public string HSN { get => ""; }

        private string _Type;

        public string Type { get => _Type.TrimIfNotNull(); set => _Type = value.TrimIfNotNull(); }

        private decimal _Quantity;

        public decimal Qty
        {
            get => Math.Round(_Quantity, 2);
            set => _Quantity = value;
        }

        public string Unit { get => "EA"; }

        private decimal _Rate;

        public decimal Rate
        {
            get => Math.Round(_Rate, 2);
            set => _Rate = value;
        }

        private decimal _DiscountAmount;

        public decimal DiscountAmount
        {
            get => Math.Round(_DiscountAmount, 2);
            set => _DiscountAmount = value;
        }

        private decimal _TaxableAmount;

        public decimal TaxableAmount
        {
            get => Math.Round(_TaxableAmount, 2);
            set => _TaxableAmount = value;
        }

        // public decimal TaxableAmount { get; set; }

        public decimal CGST
        {
            get => Math.Round(TaxableAmount * 9 / 100, 2);
        }

        public decimal SGST
        {
            get => Math.Round(TaxableAmount * 9 / 100, 2);
        }

        private decimal _GSTTotal;

        public decimal GSTTotal
        {
            get => _GSTTotal;
            set => _GSTTotal = value;
        }
    }
}