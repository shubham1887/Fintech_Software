using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.User
{
    public class SelectDownlineReportRequest
    {
        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }

        private bool _IsExcelDownload;
        public bool IsExcelDownload { get => _IsExcelDownload; set => _IsExcelDownload = value; }

        internal string IsValidModalState()
        {
            var validator = new SelectDownlineReportRequestValidator();
            FluentValidation.Results.ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class SelectDownlineReportRequestValidator : AbstractValidator<SelectDownlineReportRequest>
    {
        public SelectDownlineReportRequestValidator()
        {
            RuleFor(x => x.UserID)
                .Must(new Validation().IsValidInt)
                .WithMessage(UserMessage.InvalidUser);
        }
    }
}