using Newtonsoft.Json;
using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests;
using Project.Requests.Beneficiary;
using Project.Requests.ePocketDMT;
using Project.Response.Beneficiary;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.BAL
{
    public class BeneficiaryBAL
    {
        public InsertResponse Insert(InsertRequest mainRequest, int UserID, string IPAddress, out string failureMessage)
        {
            InsertResponse obj = null;
            mainRequest.UserID = UserID;
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return obj;

            BlackListBankMaster blackListBankMaster = new BlackListBankMasterDal().SelectByIFSC(mainRequest.IFSC, mainRequest.BankName, mainRequest.AccountNo);
            if (blackListBankMaster != null)
            {
                failureMessage = SenderMessage.DeactiveBank;
                return obj;
            }
            try
            {
                Sender sender = new SenderDAL().SelectByGUID(mainRequest.SenderID);
                if (sender == null)
                {
                    failureMessage = SenderMessage.InvalidSender;
                    return obj;
                }

                if (sender.IsOTPVerified == false)
                {
                    failureMessage = SenderMessage.VerifyOTP;
                    return obj;
                }

                Beneficiary beneficiary = new BeneficiaryDAL().SelectByAccountIFSCSender(mainRequest.AccountNo, mainRequest.IFSC, sender.SenderID);
                
                if (beneficiary == null)
                {
                    Beneficiary ObjModel = new Beneficiary
                    {
                        Name = mainRequest.Name,
                        BankName = mainRequest.BankName,
                        AccountNo = mainRequest.AccountNo,
                        IFSC = mainRequest.IFSC,
                        SenderMobile = mainRequest.SenderMobile,
                        SenderID = sender.SenderID,
                        UserID = UserID,
                        GUID = Guid.NewGuid().ToString(),
                        IPAddress = IPAddress,
                        IsActive = true,
                        IsOTPVerified = false,
                        IsNameVerified = mainRequest.IsNameVerified
                    };

                    bool IsRequiredOTP = IsOTPRequiredForBene(); // as per client requirement need otp for bene add taking descion on the basis of SetingMaster

                    if (IsRequiredOTP == false)
                    {
                        ObjModel.IsOTPVerified = true;
                    }

                    int affectedRows = new BeneficiaryDAL().Insert(ObjModel);

                    if (affectedRows <= 0)
                    {
                        failureMessage = BeneficiaryMessage.InsertFailure;
                    }

                    ObjModel.BeneficiaryID = affectedRows;

                    if (obj != null && obj.VendorBeneID != null)
                    {
                        new VendorBeneficiaryMappingBal().Insert(mainRequest.DMTName, obj.VendorBeneID, affectedRows);
                    }
                    if (IsRequiredOTP == true)
                    {
                        string message = new BeneficiaryOTPBAL().Insert(ObjModel, UserID);

                        if (!message.EqualsIgnoreCase(StatusType.Ok))
                        {
                            failureMessage = message;
                            return obj;
                        }
                    }

                    failureMessage = StatusType.Ok;
                    obj = new InsertResponse();
                    obj.BeneficiaryID = ObjModel.GUID;
                    obj.Message = BeneficiaryMessage.InsertSuccess;

                    if (ObjModel.IsOTPVerified == false)
                    {
                        failureMessage = StatusType.Ok;
                        obj = new InsertResponse();
                        obj.BeneficiaryID = ObjModel.GUID;
                        obj.Message = BeneficiaryMessage.VerifyOTP;
                        return obj;
                    }
                    return obj;
                }

                if (beneficiary.IsOTPVerified == false)
                {
                    failureMessage = StatusType.Ok;
                    obj = new InsertResponse();
                    obj.BeneficiaryID = beneficiary.GUID;
                    obj.Message = BeneficiaryMessage.VerifyOTP;
                    return obj;
                }
                if (IsVendorBene(mainRequest.DMTName))
                {
                    failureMessage = StatusType.Ok;
                    obj.Message = BeneficiaryMessage.InsertSuccess;
                    return obj;
                }
                else
                {
                    failureMessage = BeneficiaryMessage.AlreadyExits;
                    return obj;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
            }

            return obj;
        }
        
        private bool IsOTPRequiredForBene()
        {
            string IsOTPRequired = new SettingMasterBal().SelectByKey(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.DMTOTP));

            if (IsOTPRequired.EqualsIgnoreCase("SenderOnly"))
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
                Beneficiary obj = new BeneficiaryDAL().SelectByGUID(mainRequest.BeneficiaryID);
                if (obj == null)
                {
                    return BeneficiaryMessage.InvalidBeneficiary;
                }

                if (obj.IsOTPVerified == true)
                {
                    return BeneficiaryMessage.AlreadyVerified;
                }

                BeneficiaryOTP objOTP = new BeneficiaryOTPDAL().SelectByID(obj.BeneficiaryID);
                if (objOTP == null)
                {
                    return BeneficiaryMessage.InvalidBeneficiary;
                }

                if (objOTP.OTP != mainRequest.OTP)
                {
                    return BeneficiaryMessage.InvalidOTP;
                }

                objOTP.IsOTPVerified = true;
                new BeneficiaryOTPDAL().Update(objOTP);

                obj.IsOTPVerified = true;
                new BeneficiaryDAL().Update(obj);
                return StatusType.Ok;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return failureMessage;
            }
        }

        internal void UpdateBeneficiaryName(TransactionMaster transactionMaster, string BeneficiaryName)
        {
            if (BeneficiaryName == null)
            {
                return;
            }

            try
            {
                Beneficiary obj = new BeneficiaryDAL().SelectByGUID(transactionMaster.DMT_BeneficiaryID);
                if (obj == null)
                {
                    return;
                }

                if (obj.AccountNo.EqualsIgnoreCase(BeneficiaryName))
                {
                    return;
                }

                obj.Name = BeneficiaryName;
                obj.IsNameVerified = true;
                new BeneficiaryDAL().Update(obj);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
        }

        public string ResendOTP(ResendOTPRequest mainRequest, int UserID)
        {
            string failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            try
            {
                Beneficiary obj = new BeneficiaryDAL().SelectByGUID(mainRequest.BeneficiaryID);
                if (obj == null)
                {
                    return BeneficiaryMessage.InvalidBeneficiary;
                }

                return new BeneficiaryOTPBAL().Insert(obj, UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return failureMessage;
            }
        }

        public List<ViewModel> Select(SelectRequest mainRequest, int UserID, out string failureMessage)
        {
            List<ViewModel> viewModel = null;
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return viewModel;

            try
            {
                Sender sender = new SenderDAL().SelectByGUID(mainRequest.SenderID);
                if (sender == null)
                {
                    failureMessage = BeneficiaryMessage.InvalidSender;
                    return viewModel;
                }

                if (sender.IsOTPVerified == false)
                {
                    failureMessage = SenderMessage.VerifyOTP;
                    return viewModel;
                }

                bool ISVendorBene = IsVendorBene(mainRequest.DMTName);
                if (ISVendorBene == true)
                {
                    viewModel = new VendorBeneficiaryDal().SelectBySenderID(sender.SenderID);
                    return viewModel;
                }

                List<Beneficiary> list = new BeneficiaryDAL().SelectBySenderID(sender.SenderID);

                if (list == null || list.Count <= 0)
                {
                    failureMessage = BeneficiaryMessage.NoBeneficiaryFound;
                    return viewModel;
                }

                viewModel = new List<ViewModel>();
                foreach (Beneficiary bene in list)
                {
                    ViewModel model = new ViewModel();
                    model.Mobile = bene.SenderMobile;
                    model.Name = bene.Name;
                    model.BankName = bene.BankName;
                    model.AccountNo = bene.AccountNo;
                    model.IFSC = bene.IFSC;

                    model.BeneficiaryID = bene.GUID;

                    model.SenderID = sender.GUID;
                    model.IsVerified = bene.IsNameVerified;
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

        internal CheckBranchResponse CheckBranch(CheckBranchRequest mainRequest)
        {
            CheckBranchResponse response = new CheckBranchResponse();

            response.Status = mainRequest.IsValidModalState();

            if (!response.Status.EqualsIgnoreCase(StatusType.Ok))
                return response;

            response.Status = StatusType.Failure;

            try
            {
                string Url = "https://ifsc.razorpay.com/[IFSC]";

                Url = Url.Replace("[IFSC]", mainRequest.IFSC);

                string res = HttpProcessing.CallGetMethod(Url);

                if (res == null)
                {
                    return response;
                }

                if (res.EqualsIgnoreCase("The remote server returned an error: (404) Not Found."))
                {
                    response.Status = StatusType.Ok;
                    return response;
                }

                if (res.ToLower().Contains("403 Forbidden".ToLower()))
                {
                    response.Status = StatusType.Ok;
                    return response;
                }

                dynamic apiRespnse = JsonConvert.DeserializeObject(res);
                response.Branch = apiRespnse.BRANCH;
                response.Address = apiRespnse.ADDRESS;
                response.BankName = apiRespnse.BANK;

                List<Response.DMTBank.ViewModel> obj = new List<Response.DMTBank.ViewModel>();
                try
                {
                    obj = new DMTBankBAL().Select();
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }

                foreach (Response.DMTBank.ViewModel bank in obj)
                {
                    if (bank.Name.ToUpper().Contains(response.BankName.ToUpper())
                        || response.BankName.ToUpper().Contains(bank.Name.ToUpper()))
                    {
                        response.IFSC = bank.DefaultIFSC;
                        response.BankName = bank.Name;
                        break;
                    }
                }

                response.Status = StatusType.Ok;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return response;
            }

            return response;
        }

        public ViewModel SelectByGUID(BeneficiarySelectRequest mainRequest, int UserID, out string failureMessage)
        {
            ViewModel viewModel = null;
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return viewModel;

            try
            {
                Beneficiary bene = new BeneficiaryDAL().SelectByGUID(mainRequest.BeneficiaryID);
                if (bene == null)
                {
                    failureMessage = BeneficiaryMessage.InvalidBeneficiary;
                    return viewModel;
                }

                Sender sender = new SenderDAL().SelectByID(bene.SenderID);
                if (sender == null)
                {
                    failureMessage = BeneficiaryMessage.InvalidSender;
                    return viewModel;
                }

                if (sender.IsOTPVerified == false)
                {
                    failureMessage = SenderMessage.VerifyOTP;
                    return viewModel;
                }

                viewModel = new ViewModel
                {
                    Mobile = bene.SenderMobile,
                    Name = bene.Name,
                    BankName = bene.BankName,
                    AccountNo = bene.AccountNo,
                    IFSC = bene.IFSC,
                    SenderID = sender.GUID,
                    BeneficiaryID = bene.GUID,
                    IsVerified = bene.IsNameVerified
                };
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return viewModel;
            }

            return viewModel;
        }

        internal string Delete(DeleteRequest mainRequest)
        {
            string failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;

            Beneficiary beneficiary = null;

            bool ISVendorBene = IsVendorBene(mainRequest.DMTName);
            if (ISVendorBene == true)
            {
                //string vendorbeneficiary = new BeneficiaryBAL().VendorBeneDelete(mainRequest);
            }
            else
            {
                try
                {
                    beneficiary = new BeneficiaryDAL().SelectByGUID(mainRequest.BeneficiaryID);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                    return Messages.ExceptionOccured;
                }

                if (beneficiary == null) return BeneficiaryMessage.NotFound;

                int Result = -1;
                try
                {
                    Result = new BeneficiaryDAL().Delete(beneficiary);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                    return Messages.ExceptionOccured;
                }

                if (Result <= 0)
                    return BeneficiaryMessage.DeleteFailure;
            }

            return StatusType.Ok;
        }

        internal Response.TransactionMaster.TransactionResponse VerifyOTPRefund(VerifyOTPRefundRequest mainRequest, KeyDataModel keyDataModel, out string failureMessage)
        {
            Response.TransactionMaster.TransactionResponse resp = new Response.TransactionMaster.TransactionResponse();

            failureMessage = mainRequest.IsValidModalState();
            resp.Status = StatusType.Failure;
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                resp.Message = failureMessage;
                return resp;
            }
            try
            {
                TransactionMaster transactionMaster = new TransactionDAL().SelectByID(mainRequest.TransactionMasterID);

                if (transactionMaster == null)
                {
                    failureMessage = "Invalid TransactionID";
                    resp.Message = failureMessage;
                    return resp;
                }

                 if (transactionMaster.ProviderID == Company.YesBankProviderID)
                {
                    resp = YesBankOTPRefund(transactionMaster, mainRequest.OTP);
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                resp.Message = Messages.ExceptionOccured;
                return resp;
            }

            failureMessage = StatusType.Ok;
            return resp;
        }

       
        private Response.TransactionMaster.TransactionResponse YesBankOTPRefund(TransactionMaster transactionMaster, string OTP)
        {
            Response.TransactionMaster.TransactionResponse resp = new Response.TransactionMaster.TransactionResponse();
            UserMaster user = null;
            try
            {
                user = new UserDAL().SelectByIDFromCache(transactionMaster.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (user == null)
            {
                resp.Message = Message.UserMessage.InvalidUser;
                return resp;
            }

            YesBankCore.Response.MoneyTransfer.RefundTransferResponse response = new YesBankCore.Response.MoneyTransfer.RefundTransferResponse();
            if (transactionMaster.Amount == 1)
            {
                response = new Processing.YesBank.MoneyTransferProcessing().BeneVerifyOTPRefund(transactionMaster, OTP);
            }
            else
            {
                response = new Processing.YesBank.MoneyTransferProcessing().TransferVerifyOTPRefund(transactionMaster, OTP);
            }
            if (resp == null)
            {
                resp.Status = StatusType.Failure;
                resp.Message = Messages.InvalidOTP;
                resp.IsOTP = true;
                return resp;
            }

            if (resp.Message.EqualsIgnoreCase("Wrong TID or OTP"))
            {
                resp.Status = StatusType.Failure;
                resp.Message = Messages.InvalidOTP;
                resp.IsOTP = true;
                return resp;
            }

            if (resp.Status.EqualsIgnoreCase(StatusType.Failure))
            {
                resp.Message = response.Message;
                resp.Status = StatusType.Failure;
                return resp;
            }

            resp.Message = response.Message;
            resp.Status = response.Status;
            resp.IsOTP = false;
            if (resp.Message.Equals(Messages.RefundedSuccessfully))
            {
                new DMTProcessing().UpdateDMTStatusByOTP(transactionMaster.TransactionMasterID, resp);
            }
            return resp;
        }

        public BeneficiaryWrapper SelectAllBeneficiary(BeneficiaryReportRequest mainRequest, int UserID, out string failureMessage)
        {
            BeneficiaryWrapper wrapper = new BeneficiaryWrapper();
            List<BeneficiaryViewModel> viewModel = null;

            try
            {
                viewModel = new BeneficiaryDAL().SelectAllBene(mainRequest);
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
                viewModel = viewModel.Skip(mainRequest.SkipCount).Take(mainRequest.TakeCount).ToList();
            }

            wrapper.IsExcelDownload = mainRequest.IsExcelDownload;
            wrapper.list = viewModel;

            failureMessage = StatusType.Ok;
            return wrapper;
        }

        internal string BeneficiaryActDeact(Requests.Beneficiary.ActivateRequest updateRequest)
        {
            string failureMessage = updateRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            Beneficiary ObjModel = null;
            try
            {
                ObjModel = new BeneficiaryDAL().SelectByID(updateRequest.BeneficiaryID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null) return BeneficiaryMessage.NotFound;

            ObjModel.IsActive = updateRequest.IsActive;
            ObjModel.UpdatedDOC = DateTime.Now;
            try
            {
                int rowsAffected = new BeneficiaryDAL().Update(ObjModel);

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

        internal bool IsVendorBene(string DMTName)
        {
            if (DMTName.EqualsIgnoreCase("ZwitchDMT"))
                return true;

            return false;
        }
    }
}