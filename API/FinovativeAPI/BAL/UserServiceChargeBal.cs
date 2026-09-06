using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests;
using Project.Requests.UserServiceCharge;
using Project.Response.Slab;
using Project.Response.User;
using Project.Response.UserServiceCharge;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.BAL
{
    public class UserServiceChargeBal
    {
        internal async void DeductServiceCharge(int userID, int ServiceID, string remark)
        {
            UserServiceCharge ObjModel = await new UserServiceChargeDal().Select(new UserServiceCharge
            {
                UserID = userID,
                ServiceID = ServiceID
            });

            if (ObjModel != null)
                return;

            decimal charge = new ServiceMasterDal().SelectChargeByServiceID(ServiceID);

            await new UserServiceChargeDal().Insert(new UserServiceCharge()
            {
                UserID = userID,
                ServiceID = ServiceID,
                Charge = charge,
                Remark = remark
            });
        }

        public decimal GetServiceCharge(int userID, int serviceID)
        {
            decimal charge = new ServiceMasterDal().SelectChargeByServiceID(serviceID);

            UserwiseServiceChargeMaster ObjModel = new UserwiseServiceChargeMasterDal().SelectByUserID(userID, serviceID);

            if (ObjModel == null)
                return charge;

            charge = ObjModel.OneTime;
            return charge;
        }

        public decimal SelectServiceCharge(int userID, int serviceID, out string failureMessage)
        {
            decimal charge = 0;
            failureMessage = StatusType.Ok;

            string ValidationMessage = CheckServiceValidation(userID);
            if (!ValidationMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                failureMessage = ValidationMessage;
                return charge;
            }

            UserServiceCharge objmodel = new UserServiceChargeDal().SelectByID(userID, serviceID);
            if (objmodel != null)
                return charge;

            int OperatorID = new OperatorMasterDAL().SelectOperatorIDByServiceID(serviceID);
            DetailViewModel slabDetailViewModel = new SlabDetailBAL().GetCommission(userID, 1, OperatorID, 0);
            if (slabDetailViewModel == null)
                return charge;
            if (slabDetailViewModel.RetailerComm > 0)
                return charge;
            charge = Math.Abs(slabDetailViewModel.RetailerComm);

            UserWalletMaster userWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(userID, (int)WalletCode.Recharge);
            if (userWallet == null)
                return charge;

            if (userWallet.Balance < charge)
            {
                failureMessage = $"Insufficient Balance in main wallet due to service charge {charge.TwoDigitDecimalValue()}";
                return charge;
            }
            return charge;
        }

        public decimal SelectCommissionCharge(int userID, int serviceID, out string failureMessage)
        {
            decimal charge = 0;
            failureMessage = StatusType.Ok;

            int OperatorID = new OperatorMasterDAL().SelectOperatorIDByServiceID(serviceID);
            DetailViewModel slabDetailViewModel = new SlabDetailBAL().GetCommission(userID, 1, OperatorID, 0);
            if (slabDetailViewModel == null)
                return charge;
            if (slabDetailViewModel.RetailerComm > 0)
                return charge;
            charge = Math.Abs(slabDetailViewModel.RetailerComm);

            UserWalletMaster userWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(userID, (int)WalletCode.Recharge);
            if (userWallet == null)
                return charge;

            if (userWallet.Balance < charge)
            {
                failureMessage = $"Insufficient Balance in main wallet due to service charge {charge.TwoDigitDecimalValue()}";
                return charge;
            }
            return charge;
        }

        private string CheckServiceValidation(int UserID)
        {
            bool isServiceOn = new ServiceMasterBal().IsServiceOn((int)ServiceCode.AEPS);
            if (!isServiceOn)
                return ServiceMessage.Off;

            string eMessage = "";
            bool isServiceActivated = new UserServiceMappingBAL().IsServiceActivatedForUser(UserID, (int)ServiceCode.AEPS, out eMessage);
            if (!isServiceActivated)
                return eMessage;

            Response.AEPSSetting.ViewModel viewModel = new AEPSSettingDal().SelectActiveAEPS();
            if (viewModel == null)
                return AEPSMessage.NotActive;

            return StatusType.Ok;
        }

        internal bool IsServiceChargeDeducted(int userID, int serviceID)
        {
            UserServiceCharge objmodel = new UserServiceChargeDal().SelectByID(userID, serviceID);
            if (objmodel != null)
                return true;
            return false;
        }

        internal DeductResponse Deduct(int userID, int serviceID)
        {
            DeductResponse response = new DeductResponse
            {
                IsChargeDeducted = false
            };

            if (serviceID == (int)ServiceCode.AEPS)// becaus aeps service changed to AEPSonbaoding for charge cutting
            {
                serviceID = (int)ServiceCode.AEPSOnboarding;
            }

            if (IsServiceChargeDeducted(userID, serviceID))
            {
                response.IsChargeDeducted = true;
                response.Message = "Charge already deducted";
                return response;
            }
            int OperatorID = new OperatorMasterDAL().SelectOperatorIDByServiceID(serviceID);
            DetailViewModel slabDetailViewModel = new SlabDetailBAL().GetCommission(userID, 1, OperatorID, 0);
            if (slabDetailViewModel == null)
            {
                response.IsChargeDeducted = true;
                response.Message = "Charge not set";
                return response;
            }

            if (slabDetailViewModel.RetailerComm > 0)
            {
                response.IsChargeDeducted = true;
                response.Message = "Service charge is less than zero";
                return response;
            }

            string serviceName = new ServiceMasterDal().SelectByID(serviceID).Name;

            int WalletID = new WalletServiceMappingBAL().SelectWalletIDByServiceID(serviceID);
            if (WalletID <= 0)
            {
                response.IsChargeDeducted = false;
                response.Message = APIUserMessage.WalletNotMapp;
                return response;
            }

            MemberDetailResponse sender = null;
            try
            {
                sender = new UserDAL().SelectDetailByUserID(userID, true);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                response.IsChargeDeducted = false;
                response.Message = "Invalid user";
                return response;
            }
            UserWalletMaster senderWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(userID, (int)WalletCode.Recharge);
            decimal charge = Math.Abs(slabDetailViewModel.RetailerComm);

            if (senderWallet.Balance < charge)
            {
                response.IsChargeDeducted = false;
                response.Message = $"Insufficient balance, for {serviceName} of charge Rs.{charge.TwoDigitDecimal()}, and your current main wallet balance {senderWallet.Balance.TwoDigitDecimal()}";
                return response;
            }

            int Result = 0;

            string remark = $"Deducted charge: {charge.TwoDigitDecimal()} for activating Service: {serviceName}";
            string DistTransactionDescription = $"{serviceName}, Commission: {slabDetailViewModel.DistributorComm.TwoDigitDecimal()} to: activating Service by {sender.MobileNumber}";
            string SuperDistTransactionDescription = $"{serviceName}, Commission: {slabDetailViewModel.SuperDistributorComm.TwoDigitDecimal()} to: activating Service by {sender.MobileNumber}";

            try
            {
                Result = new UserServiceChargeDal().Deduct(new UserServiceCharge()
                {
                    UserID = userID,
                    ServiceID = serviceID,
                    Charge = Math.Abs(slabDetailViewModel.RetailerComm),
                    Remark = remark
                });
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                response.IsChargeDeducted = false;
                response.Message = "Exception occurred";
                return response;
            }

            if (Result <= 0)
            {
                response.IsChargeDeducted = false;
                response.Message = "Exception occurred";
                return response;
            }

            response.Message = remark;
            int ReferenceID = Result;

            UplineDetailsWithCommission uplineComm = new SlabDetailBAL().GetUplineDetailsWithCommission(userID, slabDetailViewModel);
            int DistributerUserID = uplineComm.DistributerUserID;
            int SuperDistributerUserID = uplineComm.SuperDistributerUserID;

            decimal DistributerCommi = uplineComm.DistributerCommission;
            decimal SuperDistributerCommi = uplineComm.SuperDistributerCommission;
            try
            {
                Result = new AccountProcessing().DebitBalance(
                   UserWallet: senderWallet,
                    AmountWithCommission: charge,
                    Amount: charge,
                   ConsumerNumber: sender.MobileNumber,
                   CommissionCreditDebit: CommissionCreditDebitType.None,
                   DOCType: DOCType.ServiceCharge,
                   AssignedUserID: Company.AdminUserID,
                   ServiceID: serviceID,
                   OperatorID: OperatorID,
                   TransactionDescription: remark,
                   ReferenceID: ReferenceID,
                   TDS: 0,
                   GST: 0,
                   DiscountAmount: 0);

                if (Result <= 0)
                {
                    response.IsChargeDeducted = false;
                    response.Message = "Exception occurred";

                    return response;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);

                response.IsChargeDeducted = false;
                response.Message = "Exception occurred";

                return response;
            }

            MemberDetailResponse receiver = null;
            try
            {
                receiver = new UserDAL().SelectDetailByUserID(Company.AdminUserID, true);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            UserWalletMaster receiverWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(Company.AdminUserID, (int)WalletCode.Recharge);

            try
            {
                Result = new AccountProcessing().CreditBalance(
                   UserWallet: receiverWallet,
                   AmountWithCommission: charge,
                   Amount: charge,
                   ConsumerNumber: sender.MobileNumber,
                   CommissionCreditDebit: CommissionCreditDebitType.None,
                   DOCType: DOCType.ServiceCharge,
                   AssignedUserID: Company.AdminUserID,
                   ServiceID: serviceID,
                   OperatorID: OperatorID,
                   TransactionDescription: remark,
                   ReferenceID: ReferenceID,
                   TDS: 0,
                   GST: 0,
                   DiscountAmount: 0);

                if (Result <= 0)
                {
                    return response;
                }
                Parallel.Invoke(
               () =>
               {
                   new AccountCoreProcessing().UplineCreditCommission(userID,  serviceID, OperatorID, ReferenceID,
                                   DistributerCommi, sender.MobileNumber, DOCType.ServiceCharge, DistTransactionDescription);
               }
               ,
               () =>
               {
                   new AccountCoreProcessing().UplineCreditCommission(userID,  serviceID, OperatorID, ReferenceID,
                               SuperDistributerCommi, sender.MobileNumber, DOCType.ServiceCharge, SuperDistTransactionDescription, isForSuperDist: true);
               }

               );
                //Task.Run(() => new TransactionBal().AdminAccountDebitProcessing(userID, (int)WalletCode.Recharge, serviceID, 0, ReferenceID,
                //          charge, 0, 0, sender.MobileNumber,
                //          DOCType.ServiceCharge, remark));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            response.IsChargeDeducted = true;
            return response;
        }

        internal List<ReportResponse> Report(KeyDataModel keyDataModel, ReportRequest mainRequest, out string failureMessage)
        {
            failureMessage = Messages.DataNotFound;
            List<ReportResponse> report = new List<ReportResponse>();
            int currentUserID = keyDataModel.UserID;
            int currentUserTypeID = new UserDAL().SelectUserTypeID(currentUserID);

            report = new UserServiceChargeDal().SelectForReport(mainRequest);

            if (report == null || report.Count <= 0)
                return report;

            if (currentUserTypeID == (int)UserType.Admin)
            {
                if (mainRequest.UserID > 0)
                    report = report.Where(r => r.UserID == mainRequest.UserID).ToList();

                failureMessage = StatusType.Ok;
                return report;
            }

            if (currentUserTypeID == (int)UserType.Retailer ||
                currentUserTypeID == (int)UserType.APIUser)
                report = report.Where(l => l.UserID == currentUserID).ToList();
            else
            {
                report = GetDownLineReport(currentUserID, report);
                if (mainRequest.UserID > 0)
                    report = report.Where(r => r.UserID == mainRequest.UserID).ToList();
            }

            failureMessage = StatusType.Ok;
            return report;
        }

        private List<ReportResponse> GetDownLineReport(int UserID, List<ReportResponse> allList)
        {
            List<ReportResponse> filteredlist = new List<ReportResponse>();

            List<DownlineForDropDownResponse> users = new UserDAL().SelectDownlineForDropDown(UserID);

            foreach (DownlineForDropDownResponse user in users)
            {
                allList = allList.Where(u => u.UserID == user.UserID).ToList();
                filteredlist.AddRange(allList);
            }
            return filteredlist;
        }
    }
}