using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.AutoTransferUserwiseMapping
{
    public class GenerateOTPRequest
    {
        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new GenerateOTPRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class GenerateOTPRequestValidator : AbstractValidator<GenerateOTPRequest>
    {
        public GenerateOTPRequestValidator()
        {
            RuleFor(x => x.UserID)
                      .Must(new Validation().IsValidpositive)
                      .WithMessage(AutoTransferMessages.InValidUser);
        }
    }
}