using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;

namespace Project.Requests.AutoTransferUserwiseMapping
{
    public class DeleteRequest
    {
        private int _AutoTransferUserwiseMappingID;

        public int AutoTransferUserwiseMappingID
        {
            get => _AutoTransferUserwiseMappingID;
            set => _AutoTransferUserwiseMappingID = value;
        }

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
            RuleFor(config => config.AutoTransferUserwiseMappingID)
                 .Must(new Validation().IsValidInt)
                 .WithMessage(AutoTransferMessages.InValidUser);
        }
    }
}