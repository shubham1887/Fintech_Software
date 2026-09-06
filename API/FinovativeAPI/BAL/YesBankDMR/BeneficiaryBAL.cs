using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.Sender;
using Project.Response.Beneficiary;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using YesBankCore.Response.MoneyTransfer;

namespace Project.BAL.YesBankDMR
{
    public class BeneficiaryBAL
    {
        public RegistrationResponseViewModel AddBeneficiary(Requests.Beneficiary.InsertRequest mainRequest)
        {
            RegistrationResponseViewModel resp = new RegistrationResponseViewModel();

            string failureMessage = mainRequest.IsValidModalState();
            resp.Status = StatusType.Failure;

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                resp.Message = failureMessage;
                return resp;
            }

            try
            {
                AddBeneOTPResponse bene = new Processing.YesBank.MoneyTransferProcessing().AddBeneficiary(mainRequest);

                if (bene == null)
                {
                    resp.Message = BeneficiaryMessage.InsertFailure;
                    return resp;
                }

                if (!bene.Status.EqualsIgnoreCase(StatusType.Success))
                {
                    resp.Message = bene.Message;
                    resp.Status = bene.Status;
                    return resp;
                }
                resp.Message = BeneficiaryMessage.BeneOTP;             
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                resp.Message = Messages.ExceptionOccured;
                return resp;
            }

            resp.Status = StatusType.Ok;
            return resp;
        }

        public AddBeneResponse BeneficiaryOTPValidate(Requests.Beneficiary.BeneficiaryVerifyOTPRequest mainRequest, int UserID, out string failureMessage)
        {
            AddBeneResponse resp = new();
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return resp;


            UserMaster user = null;

            user = new UserDAL().SelectByIDFromCache(UserID);

            try
            {
                resp = new Processing.YesBank.MoneyTransferProcessing().ValidateBeneficiaryOTP(mainRequest, user);

                if (resp == null)
                {
                    failureMessage = BeneficiaryMessage.InsertFailure;
                    return resp;
                }

                if (!resp.Status.EqualsIgnoreCase(StatusType.Success))
                {
                    failureMessage = resp.Message;
                    return resp;
                }
                failureMessage = StatusType.Ok;
                return resp;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return resp;
            }
            failureMessage = StatusType.Ok;
            return resp;
        }

        public List<ViewModel> Select(SelectRequest mainRequest, int UserID, out string failureMessage)
        {
            List<ViewModel> viewModel = null;
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return viewModel;

            try
            {
                BeneListResponse bene = new Processing.YesBank.MoneyTransferProcessing().ViewBeneficiaryList(mainRequest.Mobile);

                if (bene == null)
                {
                    failureMessage = BeneficiaryMessage.NoBeneficiaryFound;
                    return viewModel;
                }

                if (!bene.Status.EqualsIgnoreCase(StatusType.Success))
                {
                    failureMessage = bene.Message;
                    return viewModel;
                }

                if (bene.beneficiaryDetails == null || bene.beneficiaryDetails.Count <= 0)
                {
                    failureMessage = BeneficiaryMessage.NoBeneficiaryFound;
                    return viewModel;
                }

                viewModel = new List<ViewModel>();
                foreach (BeneficiaryDetail item in bene.beneficiaryDetails)
                {
                    ViewModel model = new ViewModel();

                    model.Name = item.name ?? "";
                    //model.BankName = item.bank;
                    model.AccountNo = item.accountNo;
                    model.IFSC = item.ifscCode;
                    model.BeneficiaryID = item.masterBeneficiaryId;
                    model.Status = item.status;

                    if (item.isVerifiedBenName != "0")
                    {
                        model.IsVerified = true;
                    }
                    else
                    {
                        model.IsVerified = false;
                    }

                    viewModel.Add(model);
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return viewModel;
            }

            return viewModel;
        }

        public DeActivateBeneResponse DeleteBeneficiary(Requests.Beneficiary.DeleteRequest mainRequest)
        {
            DeActivateBeneResponse resp = new DeActivateBeneResponse();

            string failureMessage = mainRequest.IsValidModalState();
            resp.Status = StatusType.Failure;

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                resp.Message = failureMessage;
                return resp;
            }

            try
            {
                resp = new Processing.YesBank.MoneyTransferProcessing().DeleteBeneficiary(mainRequest);

                if (resp == null)
                {
                    resp.Message = BeneficiaryMessage.DeleteFailure;
                    return resp;
                }

                if (!resp.Status.EqualsIgnoreCase(StatusType.Success))
                {
                    resp.Message = resp.Message;
                    return resp;
                }
                resp.Message ="Beneficiary deleted successfully";
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                resp.Message = Messages.ExceptionOccured;
                return resp;
            }

            resp.Status = StatusType.Ok;
            return resp;
        }
    }
}