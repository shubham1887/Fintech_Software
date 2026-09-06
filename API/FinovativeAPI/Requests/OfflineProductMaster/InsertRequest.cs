using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.OfflineProductMaster
{
    public class InsertRequest
    {
        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        private string _Name;

        public string Name
        {
            get => _Name.TrimIfNotNull();
            set => _Name = value.TrimIfNotNull();
        }

        private string _Description;

        public string Description
        {
            get => _Description.TrimIfNotNull();
            set => _Description = value.TrimIfNotNull();
        }

        private string _Validity;

        public string Validity
        {
            get => _Validity.TrimIfNotNull();
            set => _Validity = value.TrimIfNotNull();
        }

        private string _OpType;

        public string OpType
        {
            get => _OpType.TrimIfNotNull();
            set => _OpType = value.TrimIfNotNull();
        }

        private string _Remark;

        public string Remark
        {
            get => _Remark.TrimIfNotNull();
            set => _Remark = value.TrimIfNotNull();
        }

        private decimal _PackageMRP;

        public decimal PackageMRP
        {
            get => _PackageMRP;
            set => _PackageMRP = value;
        }

        private decimal _BookingAmount;

        public decimal BookingAmount
        {
            get => _BookingAmount;
            set => _BookingAmount = value;
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
            RuleFor(x => x.OperatorID)
                .Must(new Validation().IsValidInt)
                .WithMessage(OfflineProductMessage.InvalidOperator);

            RuleFor(x => x.Name)
               .Must(new Validation().IsValidString)
               .WithMessage(OfflineProductMessage.InvalidName);

            RuleFor(x => x.PackageMRP)
            .Must(new Validation().IsValidDecimal)
            .WithMessage(OfflineProductMessage.InvalidMRP);
            RuleFor(x => x.BookingAmount)
             .Must(new Validation().IsValidDecimal)
             .WithMessage(OfflineProductMessage.InvalidBookingAmount);

            RuleFor(x => x.Description)
               .Must(new Validation().IsValidString)
               .WithMessage(OfflineProductMessage.InvalidDescription);
        }
    }
}