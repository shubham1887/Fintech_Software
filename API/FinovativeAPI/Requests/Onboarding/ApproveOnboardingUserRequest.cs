using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Onboarding
{
    public class ApproveOnboardingUserRequest
    {
        private int _OnboardingUserID;
        public int OnboardingUserID
        {
            get => _OnboardingUserID;
            set => _OnboardingUserID = value;
        }
        private string _Remark;
        public string Remark
        {
            get => _Remark.TrimIfNotNull();
            set => _Remark = value.TrimIfNotNull();
        }
        private string _Status;

        public string Status
        {
            get => _Status.TrimIfNotNull();
            set => _Status = value.TrimIfNotNull();
        }


        internal string IsValidModalState()
        {
            var validator = new ApproveOnboardingUserRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            if (success)
            {
                return StatusType.Ok;
            }

            return failures[0].ToString();
        }
    }

    internal class ApproveOnboardingUserRequestValidator : AbstractValidator<ApproveOnboardingUserRequest>
    {
        public ApproveOnboardingUserRequestValidator()
        {
            RuleFor(x => x.OnboardingUserID).NotEmpty().WithMessage(Message.OnboardingMessages.InvalidID);
        }
    }
}