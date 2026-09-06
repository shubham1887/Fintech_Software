using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.IPMaster
{
    public class DeleteRequest
    {
        private int _IpMasterID;

        public int IpMasterID { get => _IpMasterID; set => _IpMasterID = value; }

        internal string IsValidModalState()
        {
            var validator = new DeleteRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class DeleteRequestValidator : AbstractValidator<DeleteRequest>
    {
        public DeleteRequestValidator()
        {
            RuleFor(x => x.IpMasterID)
             .Must(new Validation().IsValidInt)
             .WithMessage(Message.IPMasterMessage.InvalidIP);
        }
    }
}