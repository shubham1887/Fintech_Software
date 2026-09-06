using Newtonsoft.Json;
using Project.Processing;

namespace Project.Response.PayGoal
{
    public class StatusCheckResponse
    {
        private string _ResponseDateTime;

        [JsonProperty("RESPONSE_DATE_TIME")]
        public string ResponseDateTime
        {
            get => _ResponseDateTime.TrimIfNotNull();
            set => _ResponseDateTime = value.TrimIfNotNull();
        }

        private string _ResponseCode;

        [JsonProperty("RESPONSE_CODE")]
        public string ResponseCode
        {
            get => _ResponseCode.TrimIfNotNull();
            set => _ResponseCode = value.TrimIfNotNull();
        }

        private string _TxnId;

        [JsonProperty("TXN_ID")]
        public string TxnId
        {
            get => _TxnId.TrimIfNotNull();
            set => _TxnId = value.TrimIfNotNull();
        }

        private string _MopType;

        [JsonProperty("MOP_TYPE")]
        public string MopType
        {
            get => _MopType.TrimIfNotNull();
            set => _MopType = value.TrimIfNotNull();
        }

        private string _CardMask;

        [JsonProperty("CARD_MASK")]
        public string CardMask
        {
            get => _CardMask.TrimIfNotNull();
            set => _CardMask = value.TrimIfNotNull();
        }

        private string _AcqId;

        [JsonProperty("ACQ_ID")]
        public string AcqId
        {
            get => _AcqId.TrimIfNotNull();
            set => _AcqId = value.TrimIfNotNull();
        }

        private string _Txntype;

        [JsonProperty("TXNTYPE")]
        public string Txntype
        {
            get => _Txntype.TrimIfNotNull();
            set => _Txntype = value.TrimIfNotNull();
        }

        private string _CurrencyCode;

        [JsonProperty("CURRENCY_CODE")]
        public string CurrencyCode
        {
            get => _CurrencyCode.TrimIfNotNull();
            set => _CurrencyCode = value.TrimIfNotNull();
        }

        private string _Rrn;

        [JsonProperty("RRN")]
        public string Rrn
        {
            get => _Rrn.TrimIfNotNull();
            set => _Rrn = value.TrimIfNotNull();
        }

        private string _SurchargeFlag;

        [JsonProperty("SURCHARGE_FLAG")]
        public string SurchargeFlag
        {
            get => _SurchargeFlag.TrimIfNotNull();
            set => _SurchargeFlag = value.TrimIfNotNull();
        }

        private string _PaymentType;

        [JsonProperty("PAYMENT_TYPE")]
        public string PaymentType
        {
            get => _PaymentType.TrimIfNotNull();
            set => _PaymentType = value.TrimIfNotNull();
        }

        private string _PgTxnMessage;

        [JsonProperty("PG_TXN_MESSAGE")]
        public string PgTxnMessage
        {
            get => _PgTxnMessage.TrimIfNotNull();
            set => _PgTxnMessage = value.TrimIfNotNull();
        }

        private string _Status;

        [JsonProperty("STATUS")]
        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        private string _PgRefNum;

        [JsonProperty("PG_REF_NUM")]
        public string PgRefNum
        {
            get => _PgRefNum.TrimIfNotNull();
            set => _PgRefNum = value.TrimIfNotNull();
        }

        private string _PayId;

        [JsonProperty("PAY_ID")]
        public string PayId
        {
            get => _PayId.TrimIfNotNull();
            set => _PayId = value.TrimIfNotNull();
        }

        private string _OrderId;

        [JsonProperty("ORDER_ID")]
        public string OrderId
        {
            get => _OrderId.TrimIfNotNull();
            set => _OrderId = value.TrimIfNotNull();
        }

        private string _Amount;

        [JsonProperty("AMOUNT")]
        public string Amount
        {
            get => _Amount.TrimIfNotNull();
            set => _Amount = value.TrimIfNotNull();
        }

        private string _ResponseMessage;

        [JsonProperty("RESPONSE_MESSAGE")]
        public string ResponseMessage
        {
            get => _ResponseMessage.TrimIfNotNull();
            set => _ResponseMessage = value.TrimIfNotNull();
        }

        private string _OrigTxnId;

        [JsonProperty("ORIG_TXN_ID")]
        public string OrigTxnId
        {
            get => _OrigTxnId.TrimIfNotNull();
            set => _OrigTxnId = value.TrimIfNotNull();
        }

        private string _TotalAmount;

        [JsonProperty("TOTAL_AMOUNT")]
        public string TotalAmount
        {
            get => _TotalAmount.TrimIfNotNull();
            set => _TotalAmount = value.TrimIfNotNull();
        }

        private string _CustName;

        [JsonProperty("CUST_NAME")]
        public string CustName
        {
            get => _CustName.TrimIfNotNull();
            set => _CustName = value.TrimIfNotNull();
        }

        private string _IsStatusFinal;

        [JsonProperty("IS_STATUS_FINAL")]
        public string IsStatusFinal
        {
            get => _IsStatusFinal.TrimIfNotNull();
            set => _IsStatusFinal = value.TrimIfNotNull();
        }
    }
}