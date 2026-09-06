using Project.Processing;
using System.Collections.Generic;

namespace Project.Response.PaymentGateway
{
    public class WorldLineStatusCheckResponse
    {
        private string _merchantCode;

        public string merchantCode
        {
            get => _merchantCode.TrimIfNotNull();
            set => _merchantCode = value.TrimIfNotNull();
        }

        private string _merchantTransactionIdentifier;

        public string merchantTransactionIdentifier
        {
            get => _merchantTransactionIdentifier.TrimIfNotNull();
            set => _merchantTransactionIdentifier = value.TrimIfNotNull();
        }

        private string _merchantTransactionRequestType;

        public string merchantTransactionRequestType
        {
            get => _merchantTransactionRequestType.TrimIfNotNull();
            set => _merchantTransactionRequestType = value.TrimIfNotNull();
        }

        private string _responseType;

        public string responseType
        {
            get => _responseType.TrimIfNotNull();
            set => _responseType = value.TrimIfNotNull();
        }

        private string _transactionState;

        public string transactionState
        {
            get => _transactionState.TrimIfNotNull();
            set => _transactionState = value.TrimIfNotNull();
        }

        private string _merchantAdditionalDetails;

        public string merchantAdditionalDetails
        {
            get => _merchantAdditionalDetails.TrimIfNotNull();
            set => _merchantAdditionalDetails = value.TrimIfNotNull();
        }

        private string _error;

        public string error
        {
            get => _error.TrimIfNotNull();
            set => _error = value.TrimIfNotNull();
        }

        private string _merchantResponseString;

        public string merchantResponseString
        {
            get => _merchantResponseString.TrimIfNotNull();
            set => _merchantResponseString = value.TrimIfNotNull();
        }

        private string _pdfDownloadUrl;

        public string pdfDownloadUrl
        {
            get => _pdfDownloadUrl.TrimIfNotNull();
            set => _pdfDownloadUrl = value.TrimIfNotNull();
        }

        public PaymentMethod paymentMethod { get; set; }
    }

    public class ACS
    {
        private string _bankAcsFormName;

        public string bankAcsFormName
        {
            get => _bankAcsFormName.TrimIfNotNull();
            set => _bankAcsFormName = value.TrimIfNotNull();
        }

        private string _bankAcsHttpMethod;

        public string bankAcsHttpMethod
        {
            get => _bankAcsHttpMethod.TrimIfNotNull();
            set => _bankAcsHttpMethod = value.TrimIfNotNull();
        }

        private string _bankAcsUrl;

        public string bankAcsUrl
        {
            get => _bankAcsUrl.TrimIfNotNull();
            set => _bankAcsUrl = value.TrimIfNotNull();
        }

        public List<string> bankAcsParams { get; set; }
    }

    public class Authentication
    {
        private string _type;

        public string type
        {
            get => _type.TrimIfNotNull();
            set => _type = value.TrimIfNotNull();
        }

        private string _subType;

        public string subType
        {
            get => _subType.TrimIfNotNull();
            set => _subType = value.TrimIfNotNull();
        }
    }

    public class Error
    {
        private string _code;

        public string code
        {
            get => _code.TrimIfNotNull();
            set => _code = value.TrimIfNotNull();
        }

        private string _desc;

        public string desc
        {
            get => _desc.TrimIfNotNull();
            set => _desc = value.TrimIfNotNull();
        }
    }

    public class Instruction
    {
        private string _id;

        public string id
        {
            get => _id.TrimIfNotNull();
            set => _id = value.TrimIfNotNull();
        }

        private string _statusCode;

        public string statusCode
        {
            get => _statusCode.TrimIfNotNull();
            set => _statusCode = value.TrimIfNotNull();
        }

        private string _errorcode;

        public string errorcode
        {
            get => _errorcode.TrimIfNotNull();
            set => _errorcode = value.TrimIfNotNull();
        }

        private string _errordesc;

        public string errordesc
        {
            get => _errordesc.TrimIfNotNull();
            set => _errordesc = value.TrimIfNotNull();
        }
    }

    public class OTP
    {
        private string _initiator;

        public string initiator
        {
            get => _initiator.TrimIfNotNull();
            set => _initiator = value.TrimIfNotNull();
        }

        private string _message;

        public string message
        {
            get => _message.TrimIfNotNull();
            set => _message = value.TrimIfNotNull();
        }

        private string _numberOfDigit;

        public string numberOfDigit
        {
            get => _numberOfDigit.TrimIfNotNull();
            set => _numberOfDigit = value.TrimIfNotNull();
        }

        private string _target;

        public string target
        {
            get => _target.TrimIfNotNull();
            set => _target = value.TrimIfNotNull();
        }

        private string _type;

        public string type
        {
            get => _type.TrimIfNotNull();
            set => _type = value.TrimIfNotNull();
        }
    }

    public class PaymentMethod
    {
        private string _token;

        public string token
        {
            get => _token.TrimIfNotNull();
            set => _token = value.TrimIfNotNull();
        }

        private string _instrumentAliasName;

        public string instrumentAliasName
        {
            get => _instrumentAliasName.TrimIfNotNull();
            set => _instrumentAliasName = value.TrimIfNotNull();
        }

        private string _instrumentToken;

        public string instrumentToken
        {
            get => _instrumentToken.TrimIfNotNull();
            set => _instrumentToken = value.TrimIfNotNull();
        }

        private string _bankSelectionCode;

        public string bankSelectionCode
        {
            get => _bankSelectionCode.TrimIfNotNull();
            set => _bankSelectionCode = value.TrimIfNotNull();
        }

        private string _paymentMode;

        public string paymentMode
        {
            get => _paymentMode.TrimIfNotNull();
            set => _paymentMode = value.TrimIfNotNull();
        }

        private string _instrument;

        public string instrument
        {
            get => _instrument.TrimIfNotNull();
            set => _instrument = value.TrimIfNotNull();
        }

        public ACS aCS { get; set; }
        public OTP oTP { get; set; }
        public PaymentTransaction paymentTransaction { get; set; }
        public Authentication authentication { get; set; }
        public Error error { get; set; }
    }

    public class PaymentTransaction
    {
        private string _amount;

        public string amount
        {
            get => _amount.TrimIfNotNull();
            set => _amount = value.TrimIfNotNull();
        }

        private string _balanceAmount;

        public string balanceAmount
        {
            get => _balanceAmount.TrimIfNotNull();
            set => _balanceAmount = value.TrimIfNotNull();
        }

        private string _bankReferenceIdentifier;

        public string bankReferenceIdentifier
        {
            get => _bankReferenceIdentifier.TrimIfNotNull();
            set => _bankReferenceIdentifier = value.TrimIfNotNull();
        }

        private string _dateTime;

        public string dateTime
        {
            get => _dateTime.TrimIfNotNull();
            set => _dateTime = value.TrimIfNotNull();
        }

        private string _errorMessage;

        public string errorMessage
        {
            get => _errorMessage.TrimIfNotNull();
            set => _errorMessage = value.TrimIfNotNull();
        }

        private string _identifier;

        public string identifier
        {
            get => _identifier.TrimIfNotNull();
            set => _identifier = value.TrimIfNotNull();
        }

        private string _refundIdentifier;

        public string refundIdentifier
        {
            get => _refundIdentifier.TrimIfNotNull();
            set => _refundIdentifier = value.TrimIfNotNull();
        }

        private string _statusCode;

        public string statusCode
        {
            get => _statusCode.TrimIfNotNull();
            set => _statusCode = value.TrimIfNotNull();
        }

        private string _statusMessage;

        public string statusMessage
        {
            get => _statusMessage.TrimIfNotNull();
            set => _statusMessage = value.TrimIfNotNull();
        }

        private string _reference;

        public string reference
        {
            get => _reference.TrimIfNotNull();
            set => _reference = value.TrimIfNotNull();
        }

        public Instruction instruction { get; set; }

        private string _mandateDetails;

        public string mandateDetails
        {
            get => _mandateDetails.TrimIfNotNull();
            set => _mandateDetails = value.TrimIfNotNull();
        }
    }
}