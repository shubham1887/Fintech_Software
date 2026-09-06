using FluentValidation;
using FluentValidation.Results;
using YESBankCore.Controller;

namespace YesBankCore.Request.MoneyTransfer
{
    public class SenderAddRequest
    {
        private string _TxnId;

        public string TxnId
        {
            get => _TxnId.TrimIfNotNull();
            set => _TxnId = value.TrimIfNotNull();
        }

        private string _CustomerMob;

        public string CustomerMob
        {
            get => _CustomerMob.TrimIfNotNull();
            set => _CustomerMob = value.TrimIfNotNull();
        }

        private string _Name;

        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }

        private string _Address;

        public string Address
        {
            get => _Address.TrimIfNotNull();
            set => _Address = value.TrimIfNotNull();
        }

        private string _DateOfBirth;

        public string DateOfBirth
        {
            get => _DateOfBirth.TrimIfNotNull();
            set => _DateOfBirth = value.TrimIfNotNull();
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

        private string _KycStatus;

        public string KycStatus
        {
            get => _KycStatus.TrimIfNotNull();
            set => _KycStatus = value.TrimIfNotNull();
        }

        private string _PancardStatus;

        public string PancardStatus
        {
            get => _PancardStatus.TrimIfNotNull();
            set => _PancardStatus = value.TrimIfNotNull();
        }

        private string _IdentityDocType;

        public string IdentityDocType
        {
            get => _IdentityDocType.TrimIfNotNull();
            set => _IdentityDocType = value.TrimIfNotNull();
        }

        private string _IdentityDocNo;

        public string IdentityDocNo
        {
            get => _IdentityDocNo.TrimIfNotNull();
            set => _IdentityDocNo = value.TrimIfNotNull();
        }

        private string _AddressDocType;

        public string AddressDocType
        {
            get => _AddressDocType.TrimIfNotNull();
            set => _AddressDocType = value.TrimIfNotNull();
        }

        private string _AddressDocNo;

        public string AddressDocNo
        {
            get => _AddressDocNo.TrimIfNotNull();
            set => _AddressDocNo = value.TrimIfNotNull();
        }

        private string _CardStatus;

        public string CardStatus
        {
            get => _CardStatus.TrimIfNotNull();
            set => _CardStatus = value.TrimIfNotNull();
        }

        private string _City;

        public string City
        {
            get => _City.TrimIfNotNull();
            set => _City = value.TrimIfNotNull();
        }

        private string _Gender;

        public string Gender
        {
            get => _Gender.TrimIfNotNull();
            set => _Gender = value.TrimIfNotNull();
        }

        private string _Title;

        public string Title
        {
            get => _Title.TrimIfNotNull();
            set => _Title = value.TrimIfNotNull();
        }

        private string _Pincode;

        public string Pincode
        {
            get => _Pincode.TrimIfNotNull();
            set => _Pincode = value.TrimIfNotNull();
        }

        private string _District;

        public string District
        {
            get => _District.TrimIfNotNull();
            set => _District = value.TrimIfNotNull();
        }

        private string _State;

        public string State
        {
            get => _State.TrimIfNotNull();
            set => _State = value.TrimIfNotNull();
        }

        public List<string> ProductCode { get; set; }

        private string _VersionNo;

        public string VersionNo
        {
            get => _VersionNo.TrimIfNotNull();
            set => _VersionNo = value.TrimIfNotNull();
        }

        private string _Channel;

        public string Channel
        {
            get => _Channel.TrimIfNotNull();
            set => _Channel = value.TrimIfNotNull();
        }

        private string _IsOTPValidated;

        public string IsOTPValidated
        {
            get => _IsOTPValidated.TrimIfNotNull();
            set => _IsOTPValidated = value.TrimIfNotNull();
        }

        private string _MaritalStatus;

        public string MaritalStatus
        {
            get => _MaritalStatus.TrimIfNotNull();
            set => _MaritalStatus = value.TrimIfNotNull();
        }      

        private string _HashKey;

        public string HashKey
        {
            get => _HashKey.TrimIfNotNull();
            set => _HashKey = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new SenderAddRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? Message.OK : failures[0].ToString();
        }
    }

    internal class SenderAddRequestValidator : AbstractValidator<SenderAddRequest>
    {
        public SenderAddRequestValidator()
        {
            RuleFor(x => x.TxnId)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "txnId"));

            RuleFor(x => x.CustomerMob)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "customerMob"));

            RuleFor(x => x.Address)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "address"));

            RuleFor(x => x.Name)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "name"));

            RuleFor(x => x.AgentId)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "agentId"));

            RuleFor(x => x.Pincode)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "pincode"));

            RuleFor(x => x.PartnerId)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "partnerId"));

            RuleFor(x => x.KycStatus)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "kycStatus"));

            RuleFor(x => x.PancardStatus)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "pancardStatus"));

            RuleFor(x => x.CardStatus)
           .Must(new Validation().IsValidString)
           .WithMessage(Message.ParameterMissing.Replace("[Param]", "cardStatus"));

            RuleFor(x => x.City)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "city"));

            RuleFor(x => x.Gender)
           .Must(new Validation().IsValidString)
           .WithMessage(Message.ParameterMissing.Replace("[Param]", "gender"));

            RuleFor(x => x.Title)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "title"));         

            RuleFor(x => x.District)
          .Must(new Validation().IsValidString)
          .WithMessage(Message.ParameterMissing.Replace("[Param]", "gender"));

            RuleFor(x => x.State)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "State"));

            RuleFor(x => x.VersionNo)
          .Must(new Validation().IsValidString)
          .WithMessage(Message.ParameterMissing.Replace("[Param]", "versionNo"));

            RuleFor(x => x.Channel)
          .Must(new Validation().IsValidString)
          .WithMessage(Message.ParameterMissing.Replace("[Param]", "channel"));

        }
    }
}