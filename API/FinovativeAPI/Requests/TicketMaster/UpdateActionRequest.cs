using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.TicketMaster
{
    public class UpdateActionRequest
    {
        private int _TicketMasterID;

        public int TicketMasterID
        {
            get => _TicketMasterID;
            set => _TicketMasterID = value;
        }

        private string _Resolution;

        public string Resolution
        {
            get => _Resolution.TrimIfNotNull();
            set => _Resolution = value.TrimIfNotNull();
        }

        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new UpdateActionRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class UpdateActionRequestValidator : AbstractValidator<UpdateActionRequest>
    {
        public UpdateActionRequestValidator()
        {
            RuleFor(x => x.Status)
            .Must(new Validation().IsValidString)
            .WithMessage(Message.TicketMessage.InvalidStatus);
        }
    }
}