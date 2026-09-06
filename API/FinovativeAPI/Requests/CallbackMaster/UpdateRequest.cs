using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.CallbackMaster
{
    public class UpdateRequest
    {
        private int _IpMasterID;
        private string _CallbackURL;

        public string CallbackURL
        {
            get => _CallbackURL.TrimIfNotNull();
            set => _CallbackURL = value.TrimIfNotNull();
        }

        public int IpMasterID { get => _IpMasterID; set => _IpMasterID = value; }

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
            RuleFor(x => x.CallbackURL)
             .Must(new Validation().IsValidString)
             .WithMessage(Message.CallbackMasterMessage.InvalidUrl);
        }
    }
}