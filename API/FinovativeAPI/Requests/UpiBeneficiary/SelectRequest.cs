using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.UpiBeneficiary
{
    public class SelectRequest
    {
        private string _UpiSenderID;

        public string UpiSenderID
        {
            get => _UpiSenderID.TrimIfNotNull();
            set => _UpiSenderID = value.TrimIfNotNull();
        }

        private string _ActiveUPITransferName;

        public string ActiveUPITransferName
        {
            get => _ActiveUPITransferName.TrimIfNotNull();
            set => _ActiveUPITransferName = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new SelectRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class SelectRequestValidator : AbstractValidator<SelectRequest>
    {
        public SelectRequestValidator()
        {
            RuleFor(x => x.UpiSenderID)
               .Must(new Validation().IsValidString)
               .WithMessage(SenderMessage.InvalidSender);
        }
    }
}