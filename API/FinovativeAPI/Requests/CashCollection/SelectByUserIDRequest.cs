using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.CashCollection
{
    public class SelectByUserIDRequest
    {
        private int _TakeCount;

        public int TakeCount
        {
            get => _TakeCount;
            set => _TakeCount = value;
        }

        private int _SkipCount;

        public int SkipCount
        {
            get => _SkipCount;
            set => _SkipCount = value;
        }
        private string _SearchText;

        public string SearchText
        {
            get => _SearchText.TrimIfNotNull();
            set => _SearchText = value.TrimIfNotNull();
        }

        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }
        private bool _IsExcelDownload;

        public bool IsExcelDownload
        {
            get => _IsExcelDownload;
            set => _IsExcelDownload = value;
        }

        internal string IsValidModalState()
        {
            var validator = new SelectByUserIDRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class SelectByUserIDRequestValidator : AbstractValidator<SelectByUserIDRequest>
    {
        public SelectByUserIDRequestValidator()
        {
            RuleFor(x => x.UserID)
                .Must(new Validation().IsValidInt)
                .WithMessage(Message.UserMessage.InvalidUser);
        }
    }
}