using Poject.DAL;
using Processing.Processing;
using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.PayoutBeneficiary;
using Project.Response.PayoutBeneficiary;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.BAL
{
    public class PayoutBeneficiaryBAL
    {
        public InsertResponse Insert(InsertRequest mainRequest, int UserID, string IPAddress, out string failureMessage)
        {
            InsertResponse obj = null;
            mainRequest.UserID = UserID;
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return obj;
            try
            {
                int beneCount = new PayoutBeneficaryDal().SelectBeneficiaryCountByUserID(UserID);

                UserMaster user = new UserDAL().SelectByIDFromCache(UserID);
                failureMessage = SenderMessage.InvalidUserID;
                if (user == null) return obj;

                if (user.PayoutAccountCount <= 0) return obj;

                failureMessage = BeneficiaryMessage.AccountLimitExceed;
                if (user.PayoutAccountCount < beneCount) return obj;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return obj;
            }
            if (mainRequest.Document.Size.Length > 0 && mainRequest.Document.Type.Length > 0)
                failureMessage = mainRequest.IsValidModalStateForDocument();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return obj;

            string path = "";
            try
            {
                path = new MediaProcessing().SaveImage(mainRequest.Document, MediaType.PayoutDocument, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            try
            {
                PayoutBeneficiary beneficiary = new PayoutBeneficaryDal().SelectByAccountIFSCSender(mainRequest.AccountNo, mainRequest.IFSC, UserID);
                if(beneficiary != null)
                {
                   failureMessage =  Message.BeneficiaryMessage.AlreadyExits;
                   return obj;
                }

                if (beneficiary == null)
                {
                    string Status = StatusType.Process;

                    if (!Company.PayoutAccountApprovalRequired.EqualsIgnoreCase("Yes"))
                    {
                        Status = StatusType.Approve;
                    }

                    PayoutBeneficiary ObjModel = new PayoutBeneficiary
                    {
                        AccountHolderName = mainRequest.Name,
                        BankName = mainRequest.BankName,
                        AccountNo = mainRequest.AccountNo,
                        IFSC = mainRequest.IFSC,
                        UserID = UserID,
                        IsOTPVerified = false,
                        IsNameVerified = mainRequest.IsNameVerified,
                        Status = Status,
                        Document = path,
                        AdminRemark = "",
                    };

                    bool IsRequiredOTP = IsOTPRequiredForBene();

                    if (IsRequiredOTP == false)
                    {
                        ObjModel.IsOTPVerified = true;
                    }

                    int affectedRows = new PayoutBeneficaryDal().Insert(ObjModel);

                    if (affectedRows <= 0)
                    {
                        failureMessage = BeneficiaryMessage.InsertFailure;
                    }
                    ObjModel.PayoutBeneficaryID = affectedRows;

                    if (obj != null && obj.VendorBeneID != null)
                    {
                        new PayoutVendorBeneficiaryMappingBal().Insert(mainRequest.ActivePayoutName, obj.VendorBeneID, affectedRows);
                    }

                    if (IsRequiredOTP == true)
                    {
                        string message = new PayoutBeneficiaryOTPBAL().Insert(ObjModel, UserID);

                        if (!message.EqualsIgnoreCase(StatusType.Ok))
                        {
                            failureMessage = message;
                            return obj;
                        }
                    }

                    failureMessage = StatusType.Ok;
                    obj = new InsertResponse();
                    obj.BeneficiaryID = ObjModel.PayoutBeneficaryID.ToString();
                    obj.Message = BeneficiaryMessage.InsertSuccess;

                    if (ObjModel.IsOTPVerified == false)
                    {
                        failureMessage = StatusType.Ok;
                        obj = new InsertResponse();
                        obj.BeneficiaryID = ObjModel.PayoutBeneficaryID.ToString();
                        obj.Message = BeneficiaryMessage.VerifyOTP;
                        obj.IsOtpVerify = true;
                        return obj;
                    }

                    return obj;
                }

                if (beneficiary.IsOTPVerified == false)
                {
                    failureMessage = StatusType.Ok;
                    obj = new InsertResponse();
                    obj.BeneficiaryID = beneficiary.PayoutBeneficaryID.ToString();
                    obj.Message = BeneficiaryMessage.VerifyOTP;
                    obj.IsOtpVerify = true;
                    return obj;
                }

                if (IsVendorBene(mainRequest.ActivePayoutName))
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
            string IsOTPRequired = new SettingMasterBal().SelectByKey(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.PayoutOTPRequired));

            if (!IsOTPRequired.EqualsIgnoreCase("Yes"))
            {
                return false;
            }

            return true;
        }

        internal string Delete(DeleteRequest mainRequest)
        {
            string failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;

            PayoutBeneficiary beneficiary = null;

            bool ISVendorBene = IsVendorBene(mainRequest.ActivePayoutName);
            if (ISVendorBene == true)
            {
                string vendorbeneficiary = new PayoutBeneficiaryBAL().VendorBeneDelete(mainRequest);
            }
            else
            {
                try
                {
                    beneficiary = new PayoutBeneficaryDal().SelectByID(mainRequest.BeneficiaryID);
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
                    Result = new PayoutBeneficaryDal().Delete(beneficiary);
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

        internal string VendorBeneDelete(DeleteRequest mainRequest)
        {
            string failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;

            PayoutBeneficiary beneficiary = null;

            Response.PayoutVendorBeneficiaryMapping.ViewModel Vendor = null;

            Vendor = new PayoutVendorBeneficiaryMappingDal().SelectByBeneID(mainRequest.BeneficiaryID);

            try
            {
                beneficiary = new PayoutBeneficaryDal().SelectByID(mainRequest.BeneficiaryID);
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
                Result = new PayoutBeneficaryDal().Delete(beneficiary);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            if (Result <= 0)
                return BeneficiaryMessage.DeleteFailure;

            return StatusType.Ok;
        }

        internal string Update(UpdateRequest mainRequest, int UserID)
        {
            string failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            try
            {
                PayoutBeneficiary obj = new PayoutBeneficaryDal().SelectByID(mainRequest.BeneficiaryID);
                if (obj == null)
                {
                    return BeneficiaryMessage.InvalidBeneficiary;
                }

                if (obj.IsOTPVerified == true)
                {
                    return BeneficiaryMessage.AlreadyVerified;
                }

                PayoutBeneficiaryOTP objOTP = new PayoutBeneficiaryOTPDal().SelectByID(obj.PayoutBeneficaryID);
                if (objOTP == null)
                {
                    return BeneficiaryMessage.InvalidBeneficiary;
                }

                if (objOTP.OTP != mainRequest.OTP)
                {
                    return BeneficiaryMessage.InvalidOTP;
                }

                objOTP.IsOTPVerified = true;
                new PayoutBeneficiaryOTPDal().Update(objOTP);

                obj.IsOTPVerified = true;
                new PayoutBeneficaryDal().Update(obj);
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
                PayoutBeneficiary obj = new PayoutBeneficaryDal().SelectByID(mainRequest.BeneficiaryID);
                if (obj == null)
                {
                    return BeneficiaryMessage.InvalidBeneficiary;
                }

                return new PayoutBeneficiaryOTPBAL().Insert(obj, UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return failureMessage;
            }
        }

        public BeneficiaryWrapper SelectAllBeneficiary(PayoutBeneficiaryReportRequest mainRequest, int UserID, out string failureMessage)
        {
            BeneficiaryWrapper wrapper = new BeneficiaryWrapper();
            List<BeneficiaryViewModel> viewModel = null;
            isExcelOrPDF(mainRequest);
            if (UserID != Company.AdminUserID)
            {
                mainRequest.UserID = UserID;
            }

            if (IsVendorBene(mainRequest.ActivePayoutName))
            {
                viewModel = new PayoutVendorBeneficiaryDal().SelectByVendorID(mainRequest, UserID);
            }
            else
            {
                viewModel = new PayoutBeneficaryDal().SelectAllBene(mainRequest, UserID);
            }
            if (viewModel == null)
            {
                failureMessage = SenderMessage.NotFound;
                return wrapper;
            }

            viewModel = viewModel.AsQueryable().WhereDynamic(mainRequest.Search).ToList();
            wrapper.Count = viewModel.Count;
            if (!Util.IsDownload(mainRequest.IsExcelDownload, mainRequest.IsPDFDownload))
            {
                viewModel = viewModel.Skip(mainRequest.SkipCount).Take(mainRequest.TakeCount).ToList();
            }

            if (Util.IsDownload(mainRequest.IsExcelDownload, mainRequest.IsPDFDownload))
            {
                wrapper.IsDownload = true;
            }
            wrapper.list = viewModel;

            failureMessage = StatusType.Ok;
            return wrapper;
        }
        internal static bool isExcelOrPDF(Requests.PayoutBeneficiary.PayoutBeneficiaryReportRequest request)
        {
            if (request.ExportType == "EXCEL")
            {
                request.IsExcelDownload = true;
                return true;
            }
            if (request.ExportType == "PDF")
            {
                request.IsPDFDownload = true;
                return true;
            }
            return false;
        }
        internal string UpdateStatus(UpdateStatusRequest mainRequest)
        {
            string failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;

            PayoutBeneficiary beneficiary = null;
            try
            {
                beneficiary = new PayoutBeneficaryDal().SelectByID(mainRequest.BeneficiaryID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            if (beneficiary == null) return BeneficiaryMessage.NotFound;

            beneficiary.Status = mainRequest.Status;
            beneficiary.UpdatedDOC = DateTime.Now;

            int Result = -1;
            try
            {
                Result = new PayoutBeneficaryDal().Update(beneficiary);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            if (Result <= 0)
                return BeneficiaryMessage.UpdateStatusfail;

            return StatusType.Ok;
        }

        internal PayoutBeneficiary SelectByBeneficiaryID(int BeneficiaryID)
        {
            PayoutBeneficiary beneficiary = null;
            try
            {
                beneficiary = new PayoutBeneficaryDal().SelectByID(BeneficiaryID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return beneficiary;
            }

            if (beneficiary == null) return beneficiary;
            return beneficiary;
        }
        internal (Project.Response.PayoutBeneficiary.CalculateGstResponse, string) PayoutReceipt(Project.Requests.PayoutBeneficiary.CalculateGstRequest mainRequest, int UserID)
        {
            Project.Response.PayoutBeneficiary.CalculateGstResponse response = new Project.Response.PayoutBeneficiary.CalculateGstResponse();
            Project.Response.Slab.DetailViewModel slabComm = new Project.Response.Slab.DetailViewModel();

            string OutMessage = "";
            slabComm = new SlabDetailBAL().SelectByAmountOperatorAndUser(UserID, Convert.ToInt32(mainRequest.Amount), mainRequest.OperatorID, 0, out OutMessage);
            if (slabComm == null)
            {
                OutMessage = SlabMessage.NotFound;

                return (response, OutMessage);
            }
            int ServiceID = new OperatorMasterBAL().SelectServiceID(mainRequest.OperatorID);

            if (mainRequest.Amount == 1 && (ServiceID == (int)ServiceCode.MoneyTransfer || ServiceID == (int)ServiceCode.Payout))
            {

                response.TotalAmount = Math.Round(Math.Abs(slabComm.RetailerComm) + mainRequest.Amount, 2);
                response.Gst = Math.Round(Math.Abs(slabComm.RetailerComm * TaxInfo.Inclusion18Percentage), 2);
                response.Charges = Math.Round(Math.Abs(slabComm.RetailerComm) - response.Gst, 2);
                OutMessage = StatusType.Ok;
                return (response, OutMessage);
            }
            if (slabComm.RetailerComm > 0.000001M)
                response.Commission = slabComm.RetailerComm;
            response.TDS = Math.Round(new TaxProcessing().GetTDS(slabComm.RetailerComm, out OutMessage), 2);
            response.Gst = Math.Round(new TaxProcessing().GetGSTForDMTForReatiler(mainRequest.Amount, slabComm.RetailerComm, ServiceID, out OutMessage), 2);
            response.Charges = Math.Round(GetCharge(ServiceID, mainRequest.Amount, slabComm.RetailerComm, response.Gst), 2);
            response.TotalAmount = Math.Round(Math.Abs(response.Charges + response.Gst + Convert.ToInt32(mainRequest.Amount)), 2);
            OutMessage = StatusType.Ok;

            return (response, OutMessage);

        }

        internal bool IsVendorBene(string PayoutName)
        {
            if (PayoutName.EqualsIgnoreCase("ZwitchPayout"))
                return true;

            return false;
        }
        private decimal GetCharge(int ServiceID, decimal Amount, decimal RetailerComm, decimal GST)
        {
            decimal Charge = 0;
            if (ServiceID == (int)ServiceCode.MoneyTransfer)
            {
                decimal CCFValue = new TaxProcessing().getCCF(Amount);
                return Charge = CCFValue - GST;
            }
            if (RetailerComm < 0.000001M)
                Charge = Math.Abs(RetailerComm) - GST;

            return Charge;

        }


    }
}