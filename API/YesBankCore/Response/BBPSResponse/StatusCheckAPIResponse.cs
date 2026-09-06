using YesBankCore.Request.BBPS;
using YESBankCore.Controller;

namespace YesBankCore.Response.BBPSResponse
{
    public class StatusCheckAPIResponse
    {
        private string _respCode;
        public string respCode
        {
            get => _respCode.TrimIfNotNull();
            set => _respCode = value.TrimIfNotNull();
        }
        private string _response;
        public string response
        {
            get => _response.TrimIfNotNull();
            set => _response = value.TrimIfNotNull();
        }
        private string _status;
        public string status
        {
            get => _status.TrimIfNotNull();
            set => _status = value.TrimIfNotNull();
        }

        /*public class BillDetails1
        {
            public List<CustomerParam1> customerParams { get; set; }
        }

        public class CustomerParam1
        {
            private string _name;
            public string name
            {
                get => _name.TrimIfNotNull();
                set => _name = value.TrimIfNotNull();
            }
            private string _value;
            public string value
            {
                get => _value.TrimIfNotNull();
                set => _value = value.TrimIfNotNull();
            }

        }

        public class BillerResponse1
        {
        }


        public class Response1
        {
            private string _msgId;
            public string msgId
            {
                get => _msgId.TrimIfNotNull();
                set => _msgId = value.TrimIfNotNull();
            }

            private string _responseCode;
            public string responseCode
            {
                get => _responseCode.TrimIfNotNull();
                set => _responseCode = value.TrimIfNotNull();
            }

            private string _responseReason;
            public string responseReason
            {
                get => _responseReason.TrimIfNotNull();
                set => _responseReason = value.TrimIfNotNull();
            }
            
            private string _apiResponseCode;
            public string apiResponseCode
            {
                get => _apiResponseCode.TrimIfNotNull();
                set => _apiResponseCode = value.TrimIfNotNull();
            }

            private string _approvalRefNum;
            public string approvalRefNum
            {
                get => _approvalRefNum.TrimIfNotNull();
                set => _approvalRefNum = value.TrimIfNotNull();
            }


            public BillDetails1 billDetails { get; set; }
            public BillerResponse1 billerResponse { get; set; }


            private string _complianceReason;
            public string complianceReason
            {
                get => _complianceReason.TrimIfNotNull();
                set => _complianceReason = value.TrimIfNotNull();
            }

            private string _complianceRespCd;
            public string complianceRespCd
            {
                get => _complianceRespCd.TrimIfNotNull();
                set => _complianceRespCd = value.TrimIfNotNull();
            }

            private string _refId;
            public string refId
            {
                get => _refId.TrimIfNotNull();
                set => _refId = value.TrimIfNotNull();
            }

            private string _ts;
            public string ts
            {
                get => _ts.TrimIfNotNull();
                set => _ts = value.TrimIfNotNull();
            }

            private string _txnReferenceId;
            public string txnReferenceId
            {
                get => _txnReferenceId.TrimIfNotNull();
                set => _txnReferenceId = value.TrimIfNotNull();
            }

            private string _type;
            public string type
            {
                get => _type.TrimIfNotNull();
                set => _type = value.TrimIfNotNull();
            }

            private string _xchangeId;
            public string xchangeId
            {
                get => _xchangeId.TrimIfNotNull();
                set => _xchangeId = value.TrimIfNotNull();
            }

        }

        private string _respCode;
        public string respCode
        {
            get => _respCode.TrimIfNotNull();
            set => _respCode = value.TrimIfNotNull();
        }
        private string _status;
        public string status
        {
            get => _status.TrimIfNotNull();
            set => _status = value.TrimIfNotNull();
        }
       
        public Response1 response { get; set; }*/
    }
}
