using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.DMTBank
{
    public class UpdateRequest
    {
        private int _DMTBankID;

        public int DMTBankID
        {
            get => _DMTBankID;
            set => _DMTBankID = value;
        }

        private string _DefaultIFSC;

        public string DefaultIFSC
        {
            get => _DefaultIFSC.TrimIfNotNull();
            set => _DefaultIFSC = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new UpdateRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class UpdateRequestValidator : AbstractValidator<UpdateRequest>
    {
        public UpdateRequestValidator()
        {
            RuleFor(x => x.DMTBankID)
                  .Must(new Validation().IsValidInt)
                  .WithMessage(DMTBankMessage.NoBankFound);

            RuleFor(x => x.DefaultIFSC)
                .Must(new Validation().IsValidIFSC)
                .WithMessage(DMTBankMessage.InvalidIFSC);
        }
    }
}