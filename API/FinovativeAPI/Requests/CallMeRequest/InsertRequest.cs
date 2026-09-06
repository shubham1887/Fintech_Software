using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.CallMeRequest
{
    public class InsertRequest
    {
        private int _CallMeCategoryMasterID;

        public int CallMeCategoryMasterID
        {
            get => _CallMeCategoryMasterID;
            set => _CallMeCategoryMasterID = value;
        }

        private int _UplineUserID;

        public int UplineUserID
        {
            get => _UplineUserID;
            set => _UplineUserID = value;
        }

        private string _Description;

        public string Description
        {
            get => _Description;
            set => _Description = value;
        }

        internal string IsValidModalState()
        {
            var validator = new InsertRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class InsertRequestValidator : AbstractValidator<InsertRequest>
    {
        public InsertRequestValidator()
        {
            RuleFor(x => x.CallMeCategoryMasterID)
                .Must(new Validation().IsValidInt)
                .WithMessage(CallMeRequestMessage.InvalidName);
        }
    }
}