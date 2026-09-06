using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.Sender;
using Project.Response.Sender;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.BAL
{
    public class SenderBAL
    {
        public string Insert(InsertRequest mainRequest, int UserID, string IPAddress, out string failureMessage)
        {
            mainRequest.UserID = UserID;

            failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;
            try
            {
                Sender sender = new SenderDAL().SelectByNumber(mainRequest.Mobile);
                if (sender == null)
                {
                    Sender ObjModel = new Sender
                    {
                        Name = mainRequest.Name,
                        Mobile = mainRequest.Mobile,
                        Address = mainRequest.Address,
                        UserID = UserID,
                        GUID = Guid.NewGuid().ToString(),
                        IsActive = true,
                        IsOTPVerified = false,
                        IsKYC = false,
                        IPAddress = IPAddress,
                        KYCDocumentURL = "",
                        Pincode = mainRequest.PinCode
                    };

                    bool IsRequiredOTP = IsSenderOTPRequired(); // as per client requirement need otp for bene add taking descion on the basis of SetingMaster

                    if (IsRequiredOTP == false)
                    {
                        ObjModel.IsOTPVerified = true;
                    }

                    int affectedRows = new SenderDAL().Insert(ObjModel);
                    if (affectedRows <= 0)
                    {
                        failureMessage = SenderMessage.InsertFailure;
                        return failureMessage;
                    }
                    ObjModel.SenderID = affectedRows;

                    string message = "";

                    if (IsRequiredOTP == true)
                    {
                        message = new SenderOTPBAL().Insert(UserID, ObjModel);
                    }

                    if (ObjModel.IsOTPVerified == false)
                    {
                        failureMessage = StatusType.Ok;
                        return SenderMessage.VerifyOTP;
                    }

                    failureMessage = StatusType.Ok;
                    return SenderMessage.InsertSuccess;
                }
                if (sender.IsOTPVerified == false)
                {
                    failureMessage = StatusType.Ok;
                    return SenderMessage.VerifyOTP;
                }
                else
                {
                    failureMessage = SenderMessage.MobileAlreadyExits;
                    return failureMessage;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return failureMessage;
            }
        }

        private bool IsSenderOTPRequired()
        {
            string IsOTPRequired = new SettingMasterBal().SelectByKey(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.DMTOTP));

            if (IsOTPRequired.EqualsIgnoreCase("BeneOnly"))
            {
                return false;
            }

            return true;
        }

        internal string Update(UpdateRequest mainRequest, int UserID)
        {
            string failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            try
            {
                Sender sender = new SenderDAL().SelectByNumber(mainRequest.Mobile);
                if (sender == null)
                {
                    return SenderMessage.InvalidSender;
                }

                if (sender.IsOTPVerified == true)
                {
                    return SenderMessage.AlreadyVerified;
                }

                SenderOTP senderOTP = new SenderOTPDAL().SelectByID(UserID, sender);
                if (senderOTP == null)
                {
                    return SenderMessage.InvalidSender;
                }

                if (senderOTP.OTP != mainRequest.OTP)
                {
                    return SenderMessage.InvalidOTP;
                }

                senderOTP.IsOTPVerified = true;
                new SenderOTPDAL().Update(senderOTP);

                sender.IsOTPVerified = true;
                new SenderDAL().Update(sender);
                return StatusType.Ok;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return failureMessage;
            }
        }

        public string ResendOTP(ResendOTPRequest mainRequest, int UserID)
        {
            string failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;
            try
            {
                Sender sender = new SenderDAL().SelectByNumber(mainRequest.Mobile);
                if (sender == null)
                {
                    return SenderMessage.InvalidSender;
                }

                return new SenderOTPBAL().Insert(UserID, sender);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return failureMessage;
            }
        }

        public ViewModel Select(SelectRequest mainRequest, int UserID, out string failureMessage)
        {
            ViewModel viewModel = null;
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return viewModel;

            //1.check for service on
            bool isServiceOn = new ServiceMasterBal().IsServiceOn((int)ServiceCode.MoneyTransfer);
            if (!isServiceOn)
            {
                failureMessage = ServiceMessage.Off;
                return viewModel;
            }

            try
            {
                bool isKYCApproved = new UserBal().IsKyc(UserID);

                if (isKYCApproved == false)
                {
                    failureMessage = UserMessage.KYCNotApproved;
                    return viewModel;
                }

                Sender sender = new SenderDAL().SelectByNumber(mainRequest.Mobile);
                if (sender == null)
                {
                    failureMessage = SenderMessage.NotRegistered;
                    return viewModel;
                }
                if (sender.IsOTPVerified == false)
                {
                    failureMessage = SenderMessage.NotVerified;
                    return viewModel;
                }
                viewModel = new ViewModel();
                viewModel.Name = sender.Name;
                viewModel.Mobile = sender.Mobile;

                viewModel.SenderID = sender.GUID.ToString();
                viewModel.Address = sender.Address;
                viewModel.Consumed = new TransactionDAL().SelectConsumedLimitBySenderID(viewModel.SenderID);
                DMTSetting settings = new DMTSettingsBal().GetActiveDMTSetting(mainRequest.ProviderID);

                int DMTSenderLimit = 0;

                if (settings == null)
                {
                    DMTSenderLimit = new SettingMasterBal().getDMTSenderLimit();
                    viewModel.RemainingLimit = DMTSenderLimit - viewModel.Consumed;
                    return viewModel;
                }

                DMTSenderLimit = getSenderLimit(settings.SenderLimit);

                viewModel.RemainingLimit = DMTSenderLimit - viewModel.Consumed;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return viewModel;
            }

            return viewModel;
        }

        private int getSenderLimit(decimal amt)
        {
            if (amt >= 1000.0M)
            {
                return Convert.ToInt32(amt);
            }

            int DMTSenderLimit = new SettingMasterBal().getDMTSenderLimit();
            return DMTSenderLimit;
        }

        internal List<SelectByAccountNumberResponse> SelectByAccountNumber(SelectByAccountNumberRequest mainRequest, int userID, out string failureMessage)
        {
            List<SelectByAccountNumberResponse> viewModel = null;
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return viewModel;

            try
            {
                viewModel = new BeneficiaryDAL().SelectByAccountNumber(mainRequest.AccountNumber);
                if (viewModel == null)
                {
                    failureMessage = BeneficiaryMessage.InvalidAccountNo;
                    return viewModel;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return viewModel;
            }

            return viewModel;
        }

        public SenderWrapper SelectAllSender(SenderReportRequest mainRequest, int UserID, out string failureMessage)
        {
            SenderWrapper wrapper = new SenderWrapper();
            List<SenderViewModel> viewModel = null;

            try
            {
                viewModel = new SenderDAL().SelectAllSender();
                if (viewModel == null)
                {
                    failureMessage = SenderMessage.NotFound;
                    return wrapper;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);

                failureMessage = Messages.ExceptionOccured;
                return wrapper;
            }

            viewModel = viewModel.AsQueryable().WhereDynamic(mainRequest.Search).ToList();

            wrapper.Count = viewModel.Count;
            if (!mainRequest.IsExcelDownload)
            {
                viewModel = viewModel.AsQueryable().Skip(mainRequest.SkipCount).Take(mainRequest.TakeCount).ToList();
            }

            foreach (SenderViewModel item in viewModel)
            {
                item.BeneficiaryCount = new BeneficiaryDAL().SelectAllBeneficiary(item.SenderID);
            }

            wrapper.IsExcelDownload = mainRequest.IsExcelDownload;
            wrapper.list = viewModel;

            failureMessage = StatusType.Ok;
            return wrapper;
        }

        internal string SenderActiveDeactive(Requests.Sender.ActivateRequest updateRequest)
        {
            string failureMessage = updateRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            Sender ObjModel = null;
            try
            {
                ObjModel = new SenderDAL().SelectBySenderID(updateRequest.SenderID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null) return SenderMessage.NotFound;

            ObjModel.IsActive = updateRequest.IsActive;
            ObjModel.UpdatedDOC = DateTime.Now;
            try
            {
                int rowsAffected = new SenderDAL().Update(ObjModel);

                if (rowsAffected <= 0)

                    return SenderMessage.Activedeactfail;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            return StatusType.Ok;
        }
    }
}