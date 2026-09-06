using FluentValidation;
using FluentValidation.Results;
using Project.Processing;
using Project.StaticModel;
using System.Collections.Generic;

namespace Project.Requests.IPMaster
{
    public class InsertRequest
    {
        private int _IpMasterID;
        private string _IP;

        public string IP
        {
            get => _IP.TrimIfNotNull();
            set => _IP = value.TrimIfNotNull();
        }

        public int IpMasterID
        {
            get => _IpMasterID; set => _IpMasterID = value;
        }

        private string _OTP;
        public string OTP { get => _OTP.TrimIfNotNull(); set => _OTP = value.TrimIfNotNull(); }

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
            RuleFor(x => x.IP)
             .Must(new Validation().IsValidString)
             .WithMessage(Message.IPMasterMessage.InvalidIP);

            RuleFor(x => x.OTP)
           .Must(new Validation().IsValidString)
           .WithMessage(Messages.InvalidOTP);
        }
    }
}