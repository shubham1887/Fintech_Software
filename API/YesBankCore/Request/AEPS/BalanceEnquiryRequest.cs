using FluentValidation.Results;
using FluentValidation;
using YESBankCore.Controller;

namespace YesBankCore.Request.AEPS
{
    public class BalanceEnquiryRequest
   {
        private string _TxnId;

        public string TxnId
        {
            get => _TxnId.TrimIfNotNull();
            set => _TxnId = value.TrimIfNotNull();
        }

        private string _AgentId;

        public string AgentId
        {
            get => _AgentId.TrimIfNotNull();
            set => _AgentId = value.TrimIfNotNull();
        }

        private string _PartnerId;

        public string PartnerId
        {
            get => _PartnerId.TrimIfNotNull();
            set => _PartnerId = value.TrimIfNotNull();
        }

        private string _OrderId;

        public string OrderId
        {
            get => _OrderId.TrimIfNotNull();
            set => _OrderId = value.TrimIfNotNull();
        }

        private string _Channel;

        public string Channel
        {
            get => _Channel.TrimIfNotNull();
            set => _Channel = value.TrimIfNotNull();
        }

        private string _AadharNumber;

        public string AadharNumber
        {
            get => _AadharNumber.TrimIfNotNull();
            set => _AadharNumber = value.TrimIfNotNull();
        }

        private string _IsVid;

        public string IsVid
        {
            get => _IsVid.TrimIfNotNull();
            set => _IsVid = value.TrimIfNotNull();
        }

        private string _BankIIN;

        public string BankIIN
        {
            get => _BankIIN.TrimIfNotNull();
            set => _BankIIN = value.TrimIfNotNull();
        }

        private string _BiometricData;

        public string BiometricData
        {
            get => _BiometricData.TrimIfNotNull();
            set => _BiometricData = value.TrimIfNotNull();
        }

        private string _CustomerConsent;

        public string CustomerConsent
        {
            get => _CustomerConsent.TrimIfNotNull();
            set => _CustomerConsent = value.TrimIfNotNull();
        }

        private string _Latitude;

        public string Latitude
        {
            get => _Latitude.TrimIfNotNull();
            set => _Latitude = value.TrimIfNotNull();
        }

        private string _Longitude;

        public string Longitude
        {
            get => _Longitude.TrimIfNotNull();
            set => _Longitude = value.TrimIfNotNull();
        }

        private string _AppVersion;

        public string AppVersion
        {
            get => _AppVersion.TrimIfNotNull();
            set => _AppVersion = value.TrimIfNotNull();
        }

        private string _AppId;

        public string AppId
        {
            get => _AppId.TrimIfNotNull();
            set => _AppId = value.TrimIfNotNull();
        }

        private string _AgentName;

        public string AgentName
        {
            get => _AgentName.TrimIfNotNull();
            set => _AgentName = value.TrimIfNotNull();
        }

        private string _AgentAddress;

        public string AgentAddress
        {
            get => _AgentAddress.TrimIfNotNull();
            set => _AgentAddress = value.TrimIfNotNull();
        }

        private string _AgentPincode;

        public string AgentPincode
        {
            get => _AgentPincode.TrimIfNotNull();
            set => _AgentPincode = value.TrimIfNotNull();
        }

        private string _AgentDistrict;

        public string AgentDistrict
        {
            get => _AgentDistrict.TrimIfNotNull();
            set => _AgentDistrict = value.TrimIfNotNull();
        }

        private string _AgentState;

        public string AgentState
        {
            get => _AgentState.TrimIfNotNull();
            set => _AgentState = value.TrimIfNotNull();
        }

        private string _AgentCountry;

        public string AgentCountry
        {
            get => _AgentCountry.TrimIfNotNull();
            set => _AgentCountry = value.TrimIfNotNull();
        }

        private string _DeviceType;

        public string DeviceType
        {
            get => _DeviceType.TrimIfNotNull();
            set => _DeviceType = value.TrimIfNotNull();
        }

        private string _DeviceMake;

        public string DeviceMake
        {
            get => _DeviceMake.TrimIfNotNull();
            set => _DeviceMake = value.TrimIfNotNull();
        }

        private string _DeviceModel;

        public string DeviceModel
        {
            get => _DeviceModel.TrimIfNotNull();
            set => _DeviceModel = value.TrimIfNotNull();
        }

        private string _DeviceIp;

        public string DeviceIp
        {
            get => _DeviceIp.TrimIfNotNull();
            set => _DeviceIp = value.TrimIfNotNull();
        }

        private string _CustomerName;

        public string CustomerName
        {
            get => _CustomerName.TrimIfNotNull();
            set => _CustomerName = value.TrimIfNotNull();
        }

        private string _CustomerMob;

        public string CustomerMob
        {
            get => _CustomerMob.TrimIfNotNull();
            set => _CustomerMob = value.TrimIfNotNull();
        }


        private string _authType;

        public string authType
        {
            get => _authType.TrimIfNotNull();
            set => _authType = value.TrimIfNotNull();
        }
        internal string IsValidModalState()
        {
            var validator = new BalanceEnquiryRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? Message.OK : failures[0].ToString();
        }
    }

    internal class BalanceEnquiryRequestValidator : AbstractValidator<BalanceEnquiryRequest>
    {
        public BalanceEnquiryRequestValidator()
        {
            RuleFor(x => x.TxnId)
             .Must(new Validation().IsValidString)
             .WithMessage(Message.ParameterMissing.Replace("[Param]", "txnId"));

            RuleFor(x => x.AgentId)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "agentId"));

            RuleFor(x => x.PartnerId)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "partnerId"));

            RuleFor(x => x.OrderId)
           .Must(new Validation().IsValidString)
           .WithMessage(Message.ParameterMissing.Replace("[Param]", "orderId"));

            RuleFor(x => x.Channel)
          .Must(new Validation().IsValidString)
          .WithMessage(Message.ParameterMissing.Replace("[Param]", "channel"));

            RuleFor(x => x.AadharNumber)
         .Must(new Validation().IsValidString)
         .WithMessage(Message.ParameterMissing.Replace("[Param]", "aadharNumber"));

            RuleFor(x => x.BankIIN)
          .Must(new Validation().IsValidString)
          .WithMessage(Message.ParameterMissing.Replace("[Param]", "bankIIN"));

            RuleFor(x => x.BiometricData)
         .Must(new Validation().IsValidString)
         .WithMessage(Message.ParameterMissing.Replace("[Param]", "biometricData"));

            RuleFor(x => x.Latitude)
           .Must(new Validation().IsValidString)
           .WithMessage(Message.ParameterMissing.Replace("[Param]", "latitude"));

            RuleFor(x => x.Longitude)
          .Must(new Validation().IsValidString)
          .WithMessage(Message.ParameterMissing.Replace("[Param]", "longitude"));

            RuleFor(x => x.CustomerConsent)
           .Must(new Validation().IsValidString)
           .WithMessage(Message.ParameterMissing.Replace("[Param]", "customerConsent"));

            RuleFor(x => x.DeviceModel)
          .Must(new Validation().IsValidString)
          .WithMessage(Message.ParameterMissing.Replace("[Param]", "deviceModel"));

            RuleFor(x => x.DeviceIp)
          .Must(new Validation().IsValidString)
          .WithMessage(Message.ParameterMissing.Replace("[Param]", "deviceIp"));


            RuleFor(x => x.CustomerName)
          .Must(new Validation().IsValidString)
          .WithMessage(Message.ParameterMissing.Replace("[Param]", "customerName"));

            RuleFor(x => x.CustomerMob)
           .Must(new Validation().IsValidString)
           .WithMessage(Message.ParameterMissing.Replace("[Param]", "customerMob"));
        }
    }
}
