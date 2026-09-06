using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.SDKBBPSUserMapping
{
    public class InsertRequest
    {
        private string _ConfirmationCallbackURL;

        public string ConfirmationCallbackURL
        {
            get => _ConfirmationCallbackURL;
            set => _ConfirmationCallbackURL = value;
        }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private int _SDKBBPSUserMappingID;

        public int SDKBBPSUserMappingID
        {
            get => _SDKBBPSUserMappingID;
            set => _SDKBBPSUserMappingID = value;
        }

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
                RuleFor(x => x.ConfirmationCallbackURL)
                 .Must(new Validation().IsValidString)
                 .WithMessage(Message.CallbackMasterMessage.InvalidUrl);
            }
        }
    }
}