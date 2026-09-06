using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.Sender;
using Project.Response.Sender;
using Project.StaticModel;
using System;
using YesBankCore.Response.MoneyTransfer;

namespace Project.BAL.YesBankDMR
{
    public class SenderBAL
    {
        public ViewModel Select(SelectRequest mainRequest, int UserID, out string failureMessage)
        {
            ViewModel viewModel = new ViewModel();

            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return viewModel;
            try
            {
                SenderSelectResponse sender = new Processing.YesBank.MoneyTransferProcessing().SelectSenderDetail(mainRequest.Mobile);
                if (sender == null)
                {
                    failureMessage = SenderMessage.NotRegistered;
                    return viewModel;
                }
                if (!sender.Status.EqualsIgnoreCase(StatusType.Success))
                {
                    failureMessage = sender.Message;
                    return viewModel;
                }

                viewModel.Mobile = mainRequest.Mobile;
                viewModel.Message = sender.Message;
                viewModel.Name = sender.Name + sender.LastName;
                viewModel.RemainingLimit = (int)Convert.ToDouble(sender.RemainingLimit);
                viewModel.Consumed = (int)Convert.ToDouble(sender.ComsumedLimit);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return viewModel;
            }

            return viewModel;
        }

        public RegistrationResponseViewModel SenderRegistration(InsertRequest mainRequest, int UserID, string IPAddress)
        {
            RegistrationResponseViewModel resp = new RegistrationResponseViewModel();

            mainRequest.UserID = UserID;
            string failureMessage = mainRequest.IsValidModalState();
            resp.status = StatusType.Failure;

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                resp.Message = failureMessage;
                return resp;
            }

            UserMaster user = null;

            user = new UserDAL().SelectByIDFromCache(UserID);

            try
            {
                //kyc setting
                bool isKYCApproved = new UserBal().IsKyc(UserID);

                if (isKYCApproved == false)
                {
                    failureMessage = UserMessage.KYCNotApproved;
                    resp.Message = failureMessage;
                    return resp;
                }

                SenderAddResponse sender = new Processing.YesBank.MoneyTransferProcessing().SenderRegistration(mainRequest, user);

                if (sender == null)
                {
                    resp.Message = SenderMessage.NotRegistered;
                    return resp;
                }

                if (!sender.Status.EqualsIgnoreCase(StatusType.Success))
                {
                    resp.Message = sender.Message;
                    resp.status = sender.Status;
                    return resp;
                }

                resp.Message = sender.Message;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                resp.Message = Messages.ExceptionOccured;
                return resp;
            }

            resp.status = StatusType.Ok;
            return resp;
        }
    }
}