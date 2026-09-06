using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Provider
{
    public class ProviderOperatorMasterRequest
    {
        private int _ProviderID;
        private int _ServiceID;
        private string _Operatorcode;
        private string _MarginType;
        private decimal _CommissionAdmin;
        private int _PendingCount;
        private int _FailureCount;
        private bool _IsActive;

        public int ProviderID { get => _ProviderID; set => _ProviderID = value; }
        public int ServiceID { get => _ServiceID; set => _ServiceID = value; }
        public string Operatorcode { get => _Operatorcode; set => _Operatorcode = value; }
        public string MarginType { get => _MarginType; set => _MarginType = value; }
        public decimal CommissionAdmin { get => _CommissionAdmin; set => _CommissionAdmin = value; }
        public int PendingCount { get => _PendingCount; set => _PendingCount = value; }
        public int FailureCount { get => _FailureCount; set => _FailureCount = value; }
        public bool IsActive { get => _IsActive; set => _IsActive = value; }
        public int OperatorID { get; internal set; }

        internal string IsValidModalState()
        {
            var validator = new ProviderOperatorMasterValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class ProviderOperatorMasterValidator : AbstractValidator<ProviderOperatorMasterRequest>
    {
        public ProviderOperatorMasterValidator()
        {
            RuleFor(x => x.ProviderID)
                .Must(new Validation().IsValidInt)
                .WithMessage(ProviderMessage.InValidProvider);

            RuleFor(x => x.ServiceID)
                .Must(new Validation().IsValidInt)
                .WithMessage(ServiceMessage.Invalid);

            RuleFor(x => x.Operatorcode)
                .Must(new Validation().IsValidString)
                .WithMessage(OperatorMessage.InvalidOperatorcode);

            RuleFor(x => x.MarginType)
                .Must(new Validation().IsValidString)
                .WithMessage(Message.TransactionMessage.InvalidMarginType);
        }
    }
}