using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.VoucherDetails
{
    public class VoucherDetailsSelectRequest
    {
        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }
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
        private bool _IsExcelDownload;

        public bool IsExcelDownload
        {
            get => _IsExcelDownload;
            set => _IsExcelDownload = value;
        }


        private bool _IsPDFDownload;
        public bool IsPDFDownload
        {
            get => _IsPDFDownload;
            set => _IsPDFDownload = value;
        }
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
        private string _ExportType;

        public string ExportType
        {
            get => _ExportType.TrimIfNotNull();
            set => _ExportType = value.TrimIfNotNull();
        }
        internal string IsValidModalState()
        {
            var validator = new VoucherDetailsSelectRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class VoucherDetailsSelectRequestValidator : AbstractValidator<VoucherDetailsSelectRequest>
    {
        public VoucherDetailsSelectRequestValidator()
        {

            // RuleFor(x => x.VoucherTypeID)
            //  .Must(new Validation().IsValidInt)
            //  .WithMessage(WalletMessage.Invalid);
        }
    }
}

