using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Transaction
{
    public class SelectTransferModeByIFSCRequest
    {
        private string _IFSC;

        public string IFSC
        {
            get => _IFSC.TrimIfNotNull();
            set => _IFSC = value.TrimIfNotNull();
        }

        private string _BankName;

        public string BankName
        {
            get => _BankName.TrimIfNotNull();
            set => _BankName = value.TrimIfNotNull();
        }

        private string _ActiveDMT;

        public string ActiveDMT
        {
            get => _ActiveDMT.TrimIfNotNull();
            set => _ActiveDMT = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new SelectTransferModeByIFSCRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class SelectTransferModeByIFSCRequestValidator : AbstractValidator<SelectTransferModeByIFSCRequest>
    {
        public SelectTransferModeByIFSCRequestValidator()
        {
            RuleFor(x => x.IFSC)
              .Must(new Validation().IsValidIFSC)
              .WithMessage(BeneficiaryMessage.InvalidIFSC);

            RuleFor(x => x.ActiveDMT)
              .Must(new Validation().IsValidIFSC)
              .WithMessage(DMTSettingMessages.InvalidActiveDMT);
        }
    }
}