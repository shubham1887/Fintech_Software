using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Channel
{
    public class SelectByIDRequest
    {
        private int _ChannelSlabDetailID;

        public int ChannelSlabDetailID
        {
            get => _ChannelSlabDetailID;
            set => _ChannelSlabDetailID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new SelectByIDReqValidator();
            ValidationResult results = validator.Validate(this);
            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;
            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class SelectByIDReqValidator : AbstractValidator<SelectByIDRequest>
    {
        public SelectByIDReqValidator()
        {
            RuleFor(x => x.ChannelSlabDetailID)
                .Must(new Validation().IsValidInt)
                .WithMessage(Message.SlabDetailMessage.InValidID);
        }
    }
}