using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.CategoryChannelMapping
{
    public class DeleteRequest
    {
        private int _CategoryChannelMappingID;

        public int CategoryChannelMappingID
        {
            get => _CategoryChannelMappingID;
            set => _CategoryChannelMappingID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new DeleteRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    public class DeleteRequestValidator : AbstractValidator<DeleteRequest>
    {
        public DeleteRequestValidator()
        {
            RuleFor(x => x.CategoryChannelMappingID)
         .Must(new Validation().IsValidInt)
         .WithMessage(CategoryChannelMessage.InValidID);
        }
    }
}