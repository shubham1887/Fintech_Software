using FluentValidation;
using FluentValidation.Results;
using YESBankCore.Controller;

namespace YesBankCore.Request
{
    public class PayoutRequest
    {
        private string _OrderID;

        public string orderID
        {
            get => _OrderID.TrimIfNotNull();
            set => _OrderID = value.TrimIfNotNull();
        }

        private string _Amount;

        public string amount
        {
            get => _Amount.TrimIfNotNull();
            set => _Amount = value.TrimIfNotNull();
        }

        private string _accountNumber;

        public string accountNumber
        {
            get => _accountNumber.TrimIfNotNull();
            set => _accountNumber = value.TrimIfNotNull();
        }

        private string _beneBankName;

        public string beneBankName
        {
            get => _beneBankName.TrimIfNotNull();
            set => _beneBankName = value.TrimIfNotNull();
        }

        private string _beneIfscCode;

        public string beneIfscCode
        {
            get => _beneIfscCode.TrimIfNotNull();
            set => _beneIfscCode = value.TrimIfNotNull();
        }

        private string _beneMobileNumber;

        public string beneMobileNumber
        {
            get => _beneMobileNumber.TrimIfNotNull();
            set => _beneMobileNumber = value.TrimIfNotNull();
        }

        private string _beneName;

        public string beneName
        {
            get => _beneName.TrimIfNotNull();
            set => _beneName = value.TrimIfNotNull();
        }

        private string _transactionMode;

        public string transactionMode
        {
            get => _transactionMode.TrimIfNotNull();
            set => _transactionMode = value.TrimIfNotNull();
        }

        private string _customerName;

        public string customerName
        {
            get => _customerName.TrimIfNotNull();
            set => _customerName = value.TrimIfNotNull();
        }

        private string _customerMobileNumber;

        public string customerMobileNumber
        {
            get => _customerMobileNumber.TrimIfNotNull();
            set => _customerMobileNumber = value.TrimIfNotNull();
        }

        private string _pincode;

        public string pincode
        {
            get => _pincode.TrimIfNotNull();
            set => _pincode = value.TrimIfNotNull();
        }

        private string _address;

        public string address
        {
            get => _address.TrimIfNotNull();
            set => _address = value.TrimIfNotNull();
        }
        private string _email;

        public string email
        {
            get => _email.TrimIfNotNull();
            set => _email = value.TrimIfNotNull();
        }
        internal string IsValidModalState()
        {
            var validator = new PayoutRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? Message.OK : failures[0].ToString();
        }
    }

    internal class PayoutRequestValidator : AbstractValidator<PayoutRequest>
    {
        public PayoutRequestValidator()
        {
            RuleFor(x => x.orderID)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "orderID"));

            RuleFor(x => x.accountNumber)
            .Must(new Validation().isValidCustomerAccountNumber)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "accountNumber"));

            RuleFor(x => x.beneIfscCode)
            .Must(new Validation().IsValidIFSC)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "IFSC"));

            RuleFor(x => x.amount)
            .Must(new Validation().isValidIntegerAmount)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "amount"));

            RuleFor(x => x.transactionMode)
            .Must(new Validation().IsValidTransferType)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "transactionMode"));

            RuleFor(x => x.pincode)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "pincode"));

            RuleFor(x => x.email)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.ParameterMissing.Replace("[Param]", "email"));
        }
    }
}