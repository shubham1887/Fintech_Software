using FluentValidation;
using FluentValidation.Results;
using Project.Message;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.Utility
{
    public class BillFetchRequest
    {
        private int _OperatorID;

        public int OperatorID
        {
            get => _OperatorID;
            set => _OperatorID = value;
        }

        private string _APIUserRequestID;

        public string APIUserRequestID
        {
            get => _APIUserRequestID.TrimIfNotNull();
            set => _APIUserRequestID = value.TrimIfNotNull();
        }
        private string _Label;

        public string Label
        {
            get => _Label.TrimIfNotNull();
            set => _Label = value.TrimIfNotNull();
        }

        private int _ServiceID;

        internal int ServiceID
        {
            get => _ServiceID;
            set => _ServiceID = value;
        }

        private string _ConsumerNumber;

        public string ConsumerNumber
        {
            get => _ConsumerNumber.TrimIfNotNull();
            set => _ConsumerNumber = value.TrimIfNotNull();
        }

        private string _Param1;

        public string Param1
        {
            get => _Param1.TrimIfNotNull();
            set => _Param1 = value.TrimIfNotNull();
        }

        private string _Param2;

        public string Param2
        {
            get => _Param2.TrimIfNotNull();
            set => _Param2 = value.TrimIfNotNull();
        }

        private string _Param3;

        public string Param3
        {
            get => _Param3.TrimIfNotNull();
            set => _Param3 = value.TrimIfNotNull();
        }

        private string _Param4;

        public string Param4
        {
            get => _Param4.TrimIfNotNull();
            set => _Param4 = value.TrimIfNotNull();
        }

        private string _Param5;

        public string Param5
        {
            get => _Param5.TrimIfNotNull();
            set => _Param5 = value.TrimIfNotNull();
        }
        
        private string _MobileNo;

        public string MobileNo
        {
            get => _MobileNo.TrimIfNotNull();
            set => _MobileNo = value.TrimIfNotNull();
        }

        internal string IsValidModalState()
        {
            var validator = new BillFetchRequestValidator();
            ValidationResult results = validator.Validate(this);

            bool success = results.IsValid;
            IList<ValidationFailure> failures = results.Errors;

            return success ? StatusType.Ok : failures[0].ToString();
        }
    }

    internal class BillFetchRequestValidator : AbstractValidator<BillFetchRequest>
    {
        public BillFetchRequestValidator()
        {
            RuleFor(x => x.OperatorID)
                        .Must(new Validation().IsValidInt)
                        .WithMessage(OperatorMessage.InValidOperator);
            
        }
    }
}