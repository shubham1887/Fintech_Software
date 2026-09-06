using FluentValidation;
using FluentValidation.Results;
using YESBankCore.Controller;

namespace YesBankCore.Request.Payout
{
    public class FundTransferRequest
    {
        private string _RequestNumber;

        public string RequestNumber
        {
            get => _RequestNumber.TrimIfNotNull();
            set => _RequestNumber = value.TrimIfNotNull();
        }

        private string _partnerReferenceNo;

        public string partnerReferenceNo
        {
            get => _partnerReferenceNo.TrimIfNotNull();
            set => _partnerReferenceNo = value.TrimIfNotNull();
        }
        private string _AccountNumber;

        public string AccountNumber
        {
            get => _AccountNumber.TrimIfNotNull();
            set => _AccountNumber = value.TrimIfNotNull();
        }
        private string _AccountIFSC;

        public string AccountIFSC
        {
            get => _AccountIFSC.TrimIfNotNull();
            set => _AccountIFSC = value.TrimIfNotNull();
        }
        private string _Mode;

        public string Mode
        {
            get => _Mode.TrimIfNotNull();
            set => _Mode = value.TrimIfNotNull();
        }
        private string _Amount;

        public string Amount
        {
            get => _Amount.TrimIfNotNull();
            set => _Amount = value.TrimIfNotNull();
        }
        private string _BeneficiaryName;

        public string BeneficiaryName
        {
            get => _BeneficiaryName.TrimIfNotNull();
            set => _BeneficiaryName = value.TrimIfNotNull();
        }
        
        internal string IsValidModalState()
        {
            var validator = new FundTransferRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? Message.OK : failures[0].ToString();
        }

    }
    internal class FundTransferRequestValidator : AbstractValidator<FundTransferRequest>
    {
        public FundTransferRequestValidator()
        {

            RuleFor(x => x.AccountNumber)
            .Must(new Validation().isValidCustomerAccountNumber)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "AccountNumber"));

            RuleFor(x => x.AccountIFSC)
            .Must(new Validation().IsValidIFSC)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "AccountIFSC"));

            RuleFor(x => x.BeneficiaryName)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "BeneficiaryName"));
            
            RuleFor(x => x.Amount)
            .Must(new Validation().isValidIntegerAmount)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "Amount"));

            RuleFor(x => x.Mode)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "Mode"));

        }
    }
}
