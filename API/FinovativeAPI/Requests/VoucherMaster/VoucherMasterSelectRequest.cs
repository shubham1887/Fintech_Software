using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.VoucherMaster
{
    public class VoucherMasterSelectRequest
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


        private int _voucherTypeID;
        public int voucherTypeID
        {
            get => _voucherTypeID;
            set => _voucherTypeID = value;
        }
        private string _FromDate;

        public string FromDate
        {
            get => _FromDate.TrimIfNotNull();
            set => _FromDate = value.TrimIfNotNull();
        }
        private string _isReverse;

        public string isReverse
        {
            get => _isReverse.TrimIfNotNull();
            set => _isReverse = value.TrimIfNotNull();
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
            var validator = new VoucherMasterSelectRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class VoucherMasterSelectRequestValidator : AbstractValidator<VoucherMasterSelectRequest>
    {
        public VoucherMasterSelectRequestValidator()
        {

            // RuleFor(x => x.VoucherTypeID)
            //  .Must(new Validation().IsValidInt)
            //  .WithMessage(WalletMessage.Invalid);
        }
    }
}

