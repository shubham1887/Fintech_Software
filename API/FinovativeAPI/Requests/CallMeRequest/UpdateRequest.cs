using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.CallMeRequest
{
    public class UpdateRequest
    {
        private int _CallMeRequestID;

        public int CallMeRequestID
        {
            get => _CallMeRequestID;
            set => _CallMeRequestID = value;
        }

        private string _UplineRemark;

        public string UplineRemark
        {
            get => _UplineRemark.TrimIfNotNull();
            set => _UplineRemark = value.TrimIfNotNull();
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
            RuleFor(x => x.CallMeRequestID)
                .Must(new Validation().IsValidInt)
                .WithMessage(CallMeRequestMessage.InvalidID);
            RuleFor(x => x.UplineRemark)
              .Must(new Validation().IsValidString)
              .WithMessage(CallMeRequestMessage.InvalidRemark);
        }
    }
}