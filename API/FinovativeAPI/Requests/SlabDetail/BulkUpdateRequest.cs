using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.SlabDetail
{
    public class BulkUpdateRequest
    {
        private int _UserTypeID;
        private string _SlabType;

        public string SlabType
        { get => _SlabType; set => _SlabType = value; }

        private int _ServiceID;

        public int ServiceID
        {
            get => _ServiceID; set => _ServiceID = value;
        }

        private int _OperatorID;
        public int OperatorID { get => _OperatorID; set => _OperatorID = value; }

        private decimal _FromAmount;
        public decimal FromAmount { get => _FromAmount; set => _FromAmount = value; }

        private decimal _ToAmount;
        public decimal ToAmount { get => _ToAmount; set => _ToAmount = value; }

        private decimal _Commission; public decimal Commission { get => _Commission; set => _Commission = value; }

        private string _CommissionType;
        public string CommissionType { get => _CommissionType; set => _CommissionType = value; }

        private string _CommissionCategory;

        public string CommissionCategory { get => _CommissionCategory; set => _CommissionCategory = value; }
        public int UserTypeID { get => _UserTypeID; set => _UserTypeID = value; }
        private int _SlabMasterID;

        public int SlabMasterID
        {
            get => _SlabMasterID;
            set => _SlabMasterID = value;
        }

        internal string IsValidModalState()
        {
            var validator = new BulkUpdateRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return (!success) ? failures[0].ToString() : StatusType.Ok;
        }
    }

    internal class BulkUpdateRequestValidator : AbstractValidator<BulkUpdateRequest>
    {
        public BulkUpdateRequestValidator()
        {
            RuleFor(x => x.ServiceID)
             .Must(new Validation().IsValidInt)
             .WithMessage(OperatorMessage.InvalidServiceID);
        }
    }
}