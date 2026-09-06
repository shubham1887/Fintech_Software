using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Channel
{
    public class SelectByIDMasterRequest
    {
        private int _ChannelSlabMasterID;
        public int ChannelSlabMasterID { get => _ChannelSlabMasterID; set => _ChannelSlabMasterID = value; }

        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new SelectByIDRequestValidator();
            ValidationResult results = validator.Validate(this);
            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;
            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class SelectByIDRequestValidator : AbstractValidator<SelectByIDMasterRequest>
    {
        public SelectByIDRequestValidator()
        {
            RuleFor(x => x.ChannelSlabMasterID)
                .Must(new Validation().IsValidInt)
                .WithMessage(Message.SlabMessage.InvalidName);
        }
    }
}