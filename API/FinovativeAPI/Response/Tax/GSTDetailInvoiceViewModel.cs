using Project.Processing;
using System;

namespace Project.Response.Tax
{
    public class GSTDetailInvoiceViewModel
    {
        private string _ServiceName;

        public string ServiceName
        {
            get => _ServiceName.TrimIfNotNull();
            set => _ServiceName = value.TrimIfNotNull();
        }

        private string _HSN;

        public string HSN
        {
            get => _HSN.TrimIfNotNull();
            set => _HSN = value.TrimIfNotNull();
        }

        private decimal _Transaction;

        public decimal Transaction
        {
            get => Math.Round(_Transaction, 2);
            set => _Transaction = value;
        }

        private decimal _CCF;
        public decimal CCF
        {
            get => _CCF;
            set => _CCF = value;
        }
        private decimal _CashBack;
        public decimal CashBack
        {
            get => _CashBack;
            set => _CashBack = value;
        }
        private decimal _Discount;

        public decimal Discount
        {
            get => Math.Round(_Discount, 2);
            set => _Discount = value;
        }

        private int _Count;

        public int Count
        {
            get => _Count;
            set => _Count = value;
        }

        private decimal _Taxable;

        public decimal Taxable
        {
            get => _Taxable;
            set => _Taxable = value;
        }

        private string _GSTNo;

        public string GSTNo
        {
            get => _GSTNo.TrimIfNotNull();
            set => _GSTNo = value.TrimIfNotNull();
        }

        private string _InvoiceNo;

        public string InvoiceNo
        {
            get => _InvoiceNo.TrimIfNotNull();
            set => _InvoiceNo = value.TrimIfNotNull();
        }

        private string _State;

        public string State
        {
            get => _State.TrimIfNotNull();
            set => _State = value.TrimIfNotNull();
        }

        private string _InvoiceMonth;

        public string InvoiceMonth
        {
            get => _InvoiceMonth.TrimIfNotNull();
            set => _InvoiceMonth = value.TrimIfNotNull();
        }

        private string _GSTType;

        public string GSTType
        {
            get => _GSTType.TrimIfNotNull();
            set => _GSTType = value.TrimIfNotNull();
        }

        private string _FirmName;

        public string FirmName
        {
            get => _FirmName.TrimIfNotNull();
            set => _FirmName = value.TrimIfNotNull();
        }

        private decimal _IGST;

        public decimal IGST
        {
            get => _IGST;
            set => _IGST = value;
        }

        private decimal _CGST;

        public decimal CGST
        {
            get => _CGST;
            set => _CGST = value;
        }

        private decimal _SGST;

        public decimal SGST
        {
            get => _SGST;
            set => _SGST = value;
        }

        private decimal _GST;

        public decimal GST
        {
            get => Math.Round(_GST, 2);
            set => _GST = value;
        }

        
    }
}