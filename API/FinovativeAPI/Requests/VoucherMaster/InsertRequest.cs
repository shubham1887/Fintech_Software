using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.Requests.VoucherMaster
{
    public class InsertRequest
    {


        private int _VoucherTypeID;

        public int VoucherTypeID
        {
            get => _VoucherTypeID;
            set => _VoucherTypeID = value;
        }
        private string _Mode;

        public string Mode
        {
            get => _Mode;
            set => _Mode = value;
        }
        private DateTime _Date;

        public DateTime Date
        {
            get => _Date;
            set => _Date = value;
        }

        private decimal _Amount;

        public decimal Amount
        {
            get => _Amount;
            set => _Amount = value;
        }
        private string _CrGroup;

        public string CrGroup
        {
            get => _CrGroup.TrimIfNotNull();
            set => _CrGroup = value.TrimIfNotNull();
        }
        private string _CrName;

        public string CrName
        {
            get => _CrName.TrimIfNotNull();
            set => _CrName = value.TrimIfNotNull();
        }
        private string _DrGroup;

        public string DrGroup
        {
            get => _DrGroup.TrimIfNotNull();
            set => _DrGroup = value.TrimIfNotNull();
        }
        private string _DrName;

        public string DrName
        {
            get => _DrName.TrimIfNotNull();
            set => _DrName = value.TrimIfNotNull();
        }
        private string _Remark;

        public string Remark
        {
            get => _Remark.TrimIfNotNull();
            set => _Remark = value.TrimIfNotNull();
        }
        private int _ApproveBy;

        public int ApproveBy
        {
            get => _ApproveBy;
            set => _ApproveBy = value;
        }
        private int _UserID;

        public int UserID
        {
            get => _UserID;
            set => _UserID = value;
        }
        
        internal string IsValidModalState()
        {
            var validator = new InsertRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class InsertRequestValidator : AbstractValidator<InsertRequest>
    {
        public InsertRequestValidator()
        {
           // RuleFor(x => x.UserID)
           //   .Must(new Validation().IsValidInt)
           //   .WithMessage(Message.UserMessage.InvalidUser);

            // RuleFor(x => x.VoucherTypeID)
            //  .Must(new Validation().IsValidInt)
            //  .WithMessage(WalletMessage.Invalid);
        }
    }
}
