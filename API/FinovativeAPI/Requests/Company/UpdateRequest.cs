using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Company
{
    public class UpdateRequest
    {
        private int _CompanyMasterID;

        public int CompanyMasterID
        {
            get => _CompanyMasterID;
            set => _CompanyMasterID = value;
        }

        private string _Name;

        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }

        private string _Value;

        public string Value
        {
            get => _Value.TrimIfNotNull();
            set => _Value = value.TrimIfNotNull();
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

    internal class UpdateRequestValidator : AbstractValidator<UpdateRequest>
    {
        public UpdateRequestValidator()
        {
            RuleFor(x => x.CompanyMasterID)
                .Must(new Validation().IsValidInt)
                .WithMessage(Message.CompanyMessages.InvalidSetting);

            RuleFor(x => x.Name)
                .Must(new Validation().IsValidString)
                .WithMessage(Message.CompanyMessages.InvalidName);

            RuleFor(x => x.Value)
                .Must(new Validation().IsValidString)
                .WithMessage(Message.CompanyMessages.InvalidValue);
        }
    }
}