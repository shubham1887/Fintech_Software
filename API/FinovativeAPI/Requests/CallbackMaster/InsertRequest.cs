using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.CallbackMaster
{
    public class InsertRequest
    {
        private string _CallbackURL;
        public string CallbackURL { get => _CallbackURL; set => _CallbackURL = value; }

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private int _CallbackMasterID;

        public int CallbackMasterID
        {
            get => _CallbackMasterID;
            set => _CallbackMasterID = value;
        }

        private int _ServiceID;

        internal string IsValidModalState()
        {
            var validator = new InsertRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }

        internal class InsertRequestValidator : AbstractValidator<InsertRequest>
        {
            public InsertRequestValidator()
            {
                RuleFor(x => x.CallbackURL)
                 .Must(new Validation().IsValidString)
                 .WithMessage(Message.CallbackMasterMessage.InvalidUrl);
                RuleFor(x => x.ServiceID)
                .Must(new Validation().IsValidInt)
                .WithMessage(Message.CallbackMasterMessage.InvalidService);
            }
        }
    }
}