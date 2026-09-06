using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.GSTHold
{
    public class GSTBulkUpdateUsersRequest
    {
        private int _Month;

        public int Month
        {
            get => _Month;
            set => _Month = value;
        }

        private int _Year;

        public int Year
        {
            get => _Year;
            set => _Year = value;
        }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private List<GSTHoldSearchRequest> _list;

        public List<GSTHoldSearchRequest> List
        {
            get => _list;
            set => _list = value;
        }

        internal string IsValidModalState()
        {
            var validator = new GSTBulkUpdateUsersRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class GSTBulkUpdateUsersRequestValidator : AbstractValidator<GSTBulkUpdateUsersRequest>
    {
        public GSTBulkUpdateUsersRequestValidator()
        {
            RuleFor(x => x.UserID)
               .Must(new Validation().IsValidInt)
               .WithMessage(UserMessage.InvalidUser);
        }
    }
}