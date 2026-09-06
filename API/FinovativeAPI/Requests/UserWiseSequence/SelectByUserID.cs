using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.UserwiseSequence
{
    public class SelectByUserID
    {
        private int _UserwiseDMTMappingID;

        public int UserwiseDMTMappingID
        {
            get => _UserwiseDMTMappingID;
            set => _UserwiseDMTMappingID = value;
        }

        

        internal string IsValidModalState()
        {
            var validator = new SelectByUserIDValidator();
            ValidationResult results = validator.Validate(this);
            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;
            return success ? StatusType.Ok : failures[0].ToString();
        }

        public override string ToString()
        {
            return $"Userwise sequence deleted with UserID:{UserwiseDMTMappingID}";
        }
    }

    internal class SelectByUserIDValidator : AbstractValidator<SelectByUserID>
    {
        public SelectByUserIDValidator()
        {
            RuleFor(x => x.UserwiseDMTMappingID)
                .Must(new Validation().IsValidInt)
                .WithMessage(Message.UserMessage.InvalidUserID);
        }
    }
}