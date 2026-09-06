using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.IPMaster
{
    public class UpdateRequest
    {
        private int _CallbackMasterID;
        private string _IP;

        public string IP
        {
            get => _IP.TrimIfNotNull();
            set => _IP = value.TrimIfNotNull();
        }

        public int CallbackMasterID { get => _CallbackMasterID; set => _CallbackMasterID = value; }

        internal string IsValidModalState()
        {
            var validator = new UpdateRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class UpdateRequestValidator : AbstractValidator<UpdateRequest>
    {
        public UpdateRequestValidator()
        {
            RuleFor(x => x.IP)
             .Must(new Validation().IsValidString)
             .WithMessage(Message.IPMasterMessage.InvalidIP);
        }
    }
}