using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;

namespace Project.Requests.UserBalanceCapping
{
    public class DeleteRequest
    {
        private int _UserBalanceCappingID;

        public int UserBalanceCappingID { get => _UserBalanceCappingID; set => _UserBalanceCappingID = value; }

        internal string IsValidModalState()
        {
            var validator = new DeleteRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            System.Collections.Generic.IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class DeleteRequestValidator : AbstractValidator<DeleteRequest>
    {
        public DeleteRequestValidator()
        {
            RuleFor(config => config.UserBalanceCappingID)
                 .Must(new Validation().IsValidInt)
                 .WithMessage(BBPSConfigurationMessages.InvalidConfiguration);
        }
    }
}