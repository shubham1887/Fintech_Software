using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Ledger
{
    public class AdminRequest
    {
        private string _FromDate;

        public string FromDate
        {
            get => _FromDate.TrimIfNotNull();
            set => _FromDate = value.TrimIfNotNull();
        }

        private string _ToDate;

        public string ToDate
        {
            get => _ToDate.TrimIfNotNull();
            set => _ToDate = value.TrimIfNotNull();
        }

        private string _Type;

        public string Type
        {
            get => _Type.TrimIfNotNull();
            set => _Type = value.TrimIfNotNull();
        }

        private int _WalletID;

        public int WalletID
        {
            get => _WalletID;
            set => _WalletID = value;
        }

        private bool _IsExcelDownload;
        public bool IsExcelDownload { get => _IsExcelDownload; set => _IsExcelDownload = value; }

        internal string IsValidModalState()
        {
            var validator = new AdminRequestValidator();
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

    internal class AdminRequestValidator : AbstractValidator<AdminRequest>
    {
        public AdminRequestValidator()
        {
            RuleFor(x => x.FromDate).NotEmpty().WithMessage(TransactionDetailMessage.Invalidfrom);

            RuleFor(x => x.ToDate).NotEmpty().WithMessage(TransactionDetailMessage.InvalidDate);
        }
    }
}