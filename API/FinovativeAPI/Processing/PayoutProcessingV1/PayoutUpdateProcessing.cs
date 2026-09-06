using Poject.DAL;
using Project.BAL;
using Project.DAL;
using Project.Model;
using Project.Response.TransactionMaster;
using Project.StaticModel;
using System;
using System.Globalization;

namespace Project.Processing.PayoutProcessingV1
{
    public class PayoutUpdateProcessing
    {
        private string SelectDOCType(int ServiceID)
        {
            if (ServiceID == (int)ServiceCode.VendorPay)
            {
                return DOCType.VendorPay;
            }
            else
            {
                return DOCType.Payout;
            }
        }

        private bool isValidBeneficiaryName(string BeneficiaryName, string ConsumerNumber)
        {
            if (BeneficiaryName == null)
            {
                return false;
            }

            if (BeneficiaryName.Length <= 1)
            {
                return false;
            }

            if (BeneficiaryName.EqualsIgnoreCase(ConsumerNumber))
            {
                return false;
            }

            //if (BeneficiaryName.ToLower().Contains("UNREGISTERED".ToLower()))
            //{
            //    return false;
            //}

            if (BeneficiaryName.ToLower().Contains("006Credit".ToLower()))
            {
                return false;
            }
            if (BeneficiaryName.ToLower().Contains("impsuser".ToLower()))
            {
                return false;
            }
            if (BeneficiaryName.ToLower().Contains("unknown".ToLower()))
            {
                return false;
            }
            return true;
        }


        internal void UpdateYesBankStatus(YesBankCore.Response.PayoutResponse resp, int transactionMasterID, TransactionResponse retObj)
        {
            retObj.Status = StatusType.Process;
            retObj.Message = "Transaction is in process";

            if (resp == null)
            {
                return;
            }

            if (resp.Status == null)
            {
                return;
            }

            TransactionMaster transactionMaster = new TransactionDAL().SelectByID(transactionMasterID);
            string DOCType = SelectDOCType(transactionMaster.ServiceID);
            if (new ResponseProcess().CheckFailure(resp.Status))// Failure, it is outer status
            {
                if (!new ResponseProcess().checkProcess(transactionMaster.Status))
                {
                    return;
                }

                if (resp.Message.ToLower().Contains("Your Current Balance is low. Please, Contact to customer care".ToLower()))
                {
                    resp.Message = Message.TransactionMessage.ProviderLowBalance;
                }

                new AccountCoreProcessing().RefundTransaction(transactionMaster, resp.Message, DOCType);

                retObj.Status = StatusType.Failure;
                retObj.Message = resp.Message;

                return;
            }

            transactionMaster.DMT_StatusCode = resp.Status;
            transactionMaster.Description = resp.Message;

            if (isValidBeneficiaryName(resp.BeneficiaryName, transactionMaster.ConsumerNumber))
            {
                transactionMaster.DMT_BeneficaryNameByBank = resp.BeneficiaryName;
                retObj.BeneficiaryName = resp.BeneficiaryName;

                new BeneficiaryBAL().UpdateBeneficiaryName(transactionMaster, transactionMaster.DMT_BeneficaryNameByBank);
            }

            retObj.BeneficiaryName = resp.BeneficiaryName;
            retObj.TransactionID = resp.BankRRN;

            transactionMaster.ProviderRefNo = resp.RequestId;
            transactionMaster.ProviderDOCNo = resp.BankRRN;

            if (new ResponseProcess().CheckSuccess(resp.Status))// success
            {
                transactionMaster.Status = StatusType.Success;
                transactionMaster.ReferenceNo = resp.BankRRN;
                new AccountCoreProcessing().SuccessTransaction(transactionMaster);

                retObj.Status = StatusType.Success;
                retObj.Message = resp.Message;
                retObj.TransactionID = resp.BankRRN;
                return;
            }

            if (!new ResponseProcess().checkProcess(transactionMaster.Status))
            {
                return;
            }

            if (new ResponseProcess().CheckFailure(resp.Status))// Failure,  it is inner status
            {
                transactionMaster.Status = StatusType.Failure;
                new AccountCoreProcessing().RefundTransaction(transactionMaster, resp.Message, DOCType);

                retObj.Status = StatusType.Failure;
                retObj.Message = resp.Message;
                if (retObj.Message.ToLower().Contains("Your Current Balance is low. Please, Contact to customer care".ToLower()))
                {
                    retObj.Message = Message.TransactionMessage.ProviderLowBalance;
                }

                return;
            }

            new TransactionDAL().Update(transactionMaster);
        }

        internal void UpdateYesbankStatus(YesBankCore.Response.Payout.PayoutResponse resp, int transactionMasterID, TransactionResponse retObj)
        {
            retObj.Status = StatusType.Process;
            retObj.Message = "Transaction is in process";
            if (resp == null)
            {
                return;
            }
            if (resp.Status == null)
            {
                return;
            }
            TransactionMaster transactionMaster = new TransactionDAL().SelectByID(transactionMasterID);
            string DOCType = SelectDOCType(transactionMaster.ServiceID);
            if (new ResponseProcess().CheckFailure(resp.Status))// Failure, it is outer status
            {
                if (!new ResponseProcess().checkProcess(transactionMaster.Status))
                {
                    return;
                }
                new AccountCoreProcessing().RefundTransaction(transactionMaster, resp.Message, DOCType);
                retObj.Status = StatusType.Failure;
                retObj.Message = resp.Message;
                return;
            }
            transactionMaster.DMT_StatusCode = resp.Status;
            transactionMaster.Description = resp.Message;
            if (isValidBeneficiaryName(resp.BeneficiarName, transactionMaster.ConsumerNumber))
            {
                transactionMaster.DMT_BeneficaryNameByBank = resp.BeneficiarName;
                retObj.BeneficiaryName = resp.BeneficiarName;
                new BeneficiaryBAL().UpdateBeneficiaryName(transactionMaster, transactionMaster.DMT_BeneficaryNameByBank);
            }
            retObj.BeneficiaryName = resp.BeneficiarName;
            retObj.TransactionID = resp.ReferenceNo;
            transactionMaster.ReferenceNo = resp.ReferenceNo;
            retObj.DateAndTime = transactionMaster.DOC.ToString(Messages.DOCLong24HrDisplayFormat, CultureInfo.InvariantCulture);
            retObj.GST = Math.Round(Math.Abs(transactionMaster.RetailerCommission) * TaxInfo.Inclusion18Percentage, 2).ToString();
            retObj.Charges = Convert.ToString(Math.Round(Math.Abs(transactionMaster.RetailerCommission) - Convert.ToDecimal(retObj.GST), 2));
            retObj.Amount = Math.Round(Math.Abs(Convert.ToDecimal(retObj.Charges) + Convert.ToDecimal(retObj.GST) + transactionMaster.Amount), 2);

            retObj.BankName = transactionMaster.DMT_Bank;
            PayoutBeneficiary ObjModel = new PayoutBeneficaryDal().SelectBeneByAccountIFSC(transactionMaster.ConsumerNumber, transactionMaster.DMT_IFSC);

            if (new ResponseProcess().CheckSuccess(resp.Status))// success
            {
                transactionMaster.Status = StatusType.Success;
                ObjModel.IsNameVerified = true;
                int result = new PayoutBeneficaryDal().Update(ObjModel);

                new AccountCoreProcessing().SuccessTransaction(transactionMaster);
                retObj.Status = StatusType.Success;
                retObj.Message = resp.Message;
                retObj.TransactionID = resp.ReferenceNo;
                return;
            }
            new TransactionDAL().Update(transactionMaster);
        }

    }
}