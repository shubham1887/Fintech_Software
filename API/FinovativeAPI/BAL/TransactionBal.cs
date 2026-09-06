
using Newtonsoft.Json;
using project.Requests.Transaction;
using project.Requests.TransactionReport;
using project.Response.TransactionMaster;
using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests;
using Project.Requests.Sequence;
using Project.Requests.Transaction;
using Project.Requests.TransactionReport;
using Project.Response.ProviderResponse;
using Project.Response.Slab;
using Project.Response.Tax;
using Project.Response.TransactionMaster;
using Project.Response.TransactionReport;
using Project.Response.User;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Project.BAL
{
    public class TransactionBal
    {

        internal Response.TransactionMaster.TransactionResponse RechargeForAPIUser(RechargeRequest rechargeRequest, string IP, string userAgent, KeyDataModel keyDataModel, out string failureMessage)
        {
            failureMessage = "";
            Response.TransactionMaster.TransactionResponse transactionResponse = new Response.TransactionMaster.TransactionResponse();
            try
            {
                transactionResponse = new TransactionBal().Recharge(rechargeRequest, IP, userAgent, keyDataModel, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                new Logger().Info("RechargeForAPIUser", "Exception ", transactionResponse.APIUserRequestID);
            }

            //if (transactionResponse != null && transactionResponse.VendorID <= 0)
            //{
            //    bool isExists = new TransactionAPIUserRequestBAL().selectForExists(keyDataModel.UserID, rechargeRequest.APIUserRequestID);
            //    if (isExists == false)
            //    {
            //        transactionResponse.Status = StatusType.Failure;
            //        transactionResponse.Message = "Not processed";
            //    }
            //}
            return transactionResponse;
        }

        internal Response.TransactionMaster.TransactionResponse Recharge(RechargeRequest rechargeRequest, string IP, string userAgent, KeyDataModel keyDataModel, out string failureMessage, bool thirdPartyService = false)
        {
            Response.TransactionMaster.TransactionResponse response = new Response.TransactionMaster.TransactionResponse
            {
                Status = StatusType.Failure,
                Message = "",
                APIUserRequestID = rechargeRequest.APIUserRequestID,
                ConsumerNumber = rechargeRequest.ConsumerNumber,
                Amount = rechargeRequest.Amount,
                Operator = rechargeRequest.OperatorID,
                MobileNo = rechargeRequest.MobileNo,
                ConsumerName = rechargeRequest.BBPS_CustomerName,
            };

            failureMessage = rechargeRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                response.Message = failureMessage;
                return response;
            }

            int UserID = keyDataModel.UserID;

            RechargeCoreValidationResponse ValidationResponse = RechargeCoreValidation(rechargeRequest, IP, userAgent, UserID, out failureMessage);

            response.ConsumerNumber = rechargeRequest.ConsumerNumber;
            response.OpeningBalance = ValidationResponse.OpeningBalance;
            response.ClosingBalance = ValidationResponse.ClosingBalance;

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                response.Message = failureMessage;
                return response;
            }

            try
            {
                RechargeCorePart(response, rechargeRequest, ValidationResponse.slabComm,
                            UserID, ValidationResponse.WalletID, ValidationResponse.ServiceID,
                            IP, ValidationResponse.CashBack, ValidationResponse.RequestID,
                            ValidationResponse.IsROfferRecharge, ValidationResponse.CircleResponse, thirdPartyService,
                            ValidationResponse.InboxID, ValidationResponse.locationLookUp, out failureMessage);

                if (response.Message == "insuficient api balance")
                {
                    response.Message = "Please try after some time";
                    return response;
                }

                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))

                    return response;

            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            new FCMSelection().SendTransactionMessage(UserID, rechargeRequest, response.Status);

            failureMessage = StatusType.Ok;
            return response;
        }

        private string CheckDuplicateAPIUserRequestID(int UserID, string APIUserRequestID)
        {
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            if (UserTypeID != (int)UserType.APIUser)
            {
                return StatusType.Ok;
            }

            string CheckDupliateAPIUserRequestID = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys),
                                                       SettingMasterKeys.CheckDupliateAPIUserRequestID), "Yes");

            if (CheckDupliateAPIUserRequestID.EqualsIgnoreCase("Yes"))
            {
                bool isExists = new TransactionAPIUserRequestBAL().isAPIUserRequestIDExists(UserID, APIUserRequestID);
                if (isExists)
                {
                    return Message.UserMessage.DuplicateRequestID;
                }
            }
            return StatusType.Ok;
        }

        public RechargeCoreValidationResponse RechargeCoreValidation(RechargeRequest rechargeRequest, string IP, string userAgent, int UserID, out string failureMessage)
        {
            RechargeCoreValidationResponse response = new RechargeCoreValidationResponse();
            failureMessage = rechargeRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return response;
            }

            failureMessage = FunctionalValidationProcessing.IsValidMPin(UserID, rechargeRequest.Pin);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;

            if (!new OperatorMasterBAL().IsValid(rechargeRequest.ConsumerNumber, rechargeRequest.OperatorID, rechargeRequest.Amount, out failureMessage))
            {
                return response;
            }
            bool IsViewRequired = new OperatorMasterBAL().IsViewRequired(rechargeRequest.OperatorID);
            if (IsViewRequired)
            {
                if (rechargeRequest.BBPS_CustomerName.Length <= 0)
                {
                    failureMessage = OperatorMessage.ViewRequired;
                    return response;
                }
            }

            // Check here for BBPS Configuration
            bool IssBBPS = new OperatorMasterBAL().IsBBPSOperator(rechargeRequest.OperatorID);

            if (IssBBPS)
            {
                failureMessage = FunctionalValidationProcessing.IsValidRequestForBBPS(rechargeRequest.OperatorID, rechargeRequest.Param1, rechargeRequest.Param2, rechargeRequest.Param3, rechargeRequest.Param4, rechargeRequest.Param5);

                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                {
                    return response;
                }
            }

            response.ServiceID = new OperatorMasterBAL().SelectServiceID(rechargeRequest.OperatorID);

            if (response.ServiceID <= 0)
            {
                failureMessage = OperatorMessage.InValidOperator;
                return response;
            }

            if (response.ServiceID == (int)ServiceCode.Fasttag)
            {
                rechargeRequest.ConsumerNumber = rechargeRequest.ConsumerNumber.ToUpperCase();
            }

            if (!NotInMobileAndUtilityService(response.ServiceID))
            {
                failureMessage = OperatorMessage.InValidOperator;
                return response;
            }

            failureMessage = FunctionalValidationProcessing.IsValidNumbericConsumerNumber(response.ServiceID, rechargeRequest.ConsumerNumber);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;

            int CircleID = 0;
            if (response.ServiceID == (int)ServiceCode.Mobile)
            {
                response.CircleResponse = new SequenceSelection().GetCircleCode(rechargeRequest.ConsumerNumber);
                if (!response.CircleResponse.Message.EqualsIgnoreCase(Messages.InvalidURL))
                {
                    if (response.CircleResponse != null)
                        CircleID = response.CircleResponse.CircleID;

                    if (response.CircleResponse != null)
                        failureMessage = new CircleWiseUserWiseOperatorOffBal().IsValidCircleOff(UserID, rechargeRequest.OperatorID, response.CircleResponse.CircleID, response.CircleResponse.Circle);

                    if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                        return response;

                    if (response.CircleResponse != null)
                        failureMessage = new CircleWiseOperatorLimitBal().isValidDailyUsageCircleLimit(UserID, rechargeRequest.OperatorID, response.CircleResponse.CircleID, Convert.ToInt32(rechargeRequest.Amount));

                    if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                        return response;
                }
            }

            if (response.ServiceID == (int)ServiceCode.Mobile || response.ServiceID == (int)ServiceCode.Dth)
            {
                failureMessage = new OperatorAmountWiseSettingBal().isValidDailyOperatorAmountwiseLimit(rechargeRequest.OperatorID, Convert.ToInt32(rechargeRequest.Amount));
                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                    return response;
            }
            failureMessage = FunctionalValidationProcessing.FunctionalValidation(UserID, rechargeRequest.OperatorID,
                response.ServiceID, rechargeRequest.Amount, rechargeRequest.APIUserRequestID, rechargeRequest.ConsumerNumber,
                out decimal ServiceCharge);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return response;

            failureMessage = new OperatorUserwiseBal().isValidDailyUsageLimit(UserID, rechargeRequest.OperatorID, Convert.ToInt32(rechargeRequest.Amount));
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return response;

            if (!rechargeRequest.Through.EqualsIgnoreCase("API"))
                rechargeRequest.Through = new BrowserUserAgentBal().GetThrough(userAgent);

            failureMessage = CheckDuplicateAPIUserRequestID(UserID, rechargeRequest.APIUserRequestID);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return response;

            response.RequestID = new Util().GetRandomNumberWithDatePrefix();

            if (response.RequestID.Length < 2)
            {
                failureMessage = "Invalid RequestID";
                return response;
            }

            UserWalletMaster userWallet;

            try
            {
                response.WalletID = new WalletServiceMappingBAL().SelectWalletIDByServiceID(response.ServiceID);

                if (response.WalletID <= 0)
                {
                    failureMessage = APIUserMessage.WalletNotMapp;
                    return response;
                }

                userWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(UserID, response.WalletID);

                failureMessage = "Invalid Wallet";
                if (userWallet == null)
                    return response;

                response.OpeningBalance = userWallet.Balance;
                response.ClosingBalance = userWallet.Balance;

                failureMessage = FunctionalValidationProcessing.IsValidAmountForMinServiceBalance(response.ServiceID, response.OpeningBalance, rechargeRequest.Amount);

                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                    return response;

                failureMessage = FunctionalValidationProcessing.IsValidReversalAmountBlocked(UserID, response.WalletID, userWallet.Balance, rechargeRequest.Amount);

                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                    return response;

                failureMessage = $"Insufficient Balance for amount {rechargeRequest.Amount.TwoDigitDecimal()}";
                if (userWallet.Balance < rechargeRequest.Amount)
                    return response;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = TransactionMessage.Failure;
                return response;
            }

            response.IsROfferRecharge = false;

            string OutMessage = "";

            try
            {
                response.slabComm = new SlabDetailBAL().SelectByAmountOperatorAndUser(UserID, Convert.ToInt32(rechargeRequest.Amount), rechargeRequest.OperatorID, CircleID, out OutMessage);

                failureMessage = Message.APIUserMessage.CommissionNotSet;
                if (response.slabComm == null)
                    return response;

                if (!OutMessage.EqualsIgnoreCase(StatusType.Ok))
                {
                    failureMessage = OutMessage;
                    return response;
                }

                if (response.slabComm.IsActive == false)
                {
                    failureMessage = "Commission not set/active for this operator";
                    return response;
                }
                response.IsROfferRecharge = response.slabComm.IsROffer;

                if (response.IsROfferRecharge == true)
                {
                    response.CashBack = response.slabComm.RetailerComm;
                }

                if ((userWallet.Balance + response.slabComm.RetailerComm) < rechargeRequest.Amount)
                {
                    failureMessage = "Insufficient Balance, due to commission " + response.slabComm.RetailerComm;
                    return response;
                }

                decimal cappingAmount = new UserBalanceCappingBal().SelectByWalletIDAndUserID(UserID, response.WalletID);

                if ((userWallet.Balance + response.slabComm.RetailerComm - cappingAmount) < rechargeRequest.Amount)
                {
                    failureMessage = $"Insufficient Balance, due to commission and minimum wallet balance is {cappingAmount.TwoDigitDecimal()} {response.slabComm.RetailerComm.TwoDigitDecimal()}";
                    return response;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = TransactionMessage.Failure;
                return response;
            }

            response.locationLookUp.IPAddress = IP;
            response.locationLookUp.SimCardDetails = "";
            response.locationLookUp.Location = "";

            try
            {
                int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

                if (UserTypeID != (int)UserType.APIUser)
                {
                    response.locationLookUp = new LocationProcessing().LocationDetails(IP);
                }

                if (IsMobileService(response.ServiceID))
                {
                    List<ProviderList> providerIDList = new OperatorSequenceDal().SelectByOperatorID(rechargeRequest.OperatorID);
                    if (providerIDList.Count <= 0)
                    {
                        failureMessage = TransactionMessage.sequenceNotFound;
                        return response;
                    }
                }

                response.InboxID = new InboxBAL().Insert(UserID, response.ServiceID, rechargeRequest.OperatorID,
                                          rechargeRequest.Amount, rechargeRequest.ConsumerNumber,
                                          rechargeRequest.Through, StatusType.Process, IP, out failureMessage);


                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                {
                    return response;
                }

                if (response.InboxID <= 0)
                {
                    failureMessage = "Exception in inbox, please contact admin";
                    return response;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = TransactionMessage.Failure;
                return response;
            }

            if (response.ServiceID == (int)ServiceCode.Mobile || response.ServiceID == (int)ServiceCode.Dth)
            {
                Task.Run(() => new PlanSettingDAL().UpdateUserPlanUtilization(UserID, isPlanApi: true, resetCounts: true));
            }

            Task.Run(() => new PlanSettingDAL().UpdateUserBillFetchUtilization(UserID, resetCounts: true));


            failureMessage = StatusType.Ok;
            return response;
        }

        public void RechargeCorePart(Response.TransactionMaster.TransactionResponse response, RechargeRequest rechargeRequest,
            DetailViewModel slabComm, int UserID, int WalletID, int ServiceID, string IP, decimal CashBack, string requestID,
            bool IsROfferRecharge, CircleCore.Response.CoreResponse CircleResponse, bool thirdPartyService,
            int InboxID, LocationLookUp locationLookUp, out string failureMessage)
        {
            UserWalletMaster userWallet;
            TransactionMaster transactionMaster;
            using (ConcurrencyControl.AcquireLock(UserID, UserID.ToString()))
            {
                try
                {
                    userWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(UserID, WalletID);
                    failureMessage = "Invalid Wallet";
                    if (userWallet == null)
                        return;

                    failureMessage = new AccountProcessing().BalanceValidationBeforeTransaction(userWallet, rechargeRequest.Amount, slabComm.RetailerComm);
                    if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                    {
                        return;
                    }

                    if (!new TransactionDetailsBal().IsAmountValid(userWallet.Balance, UserID, WalletID))
                    {
                        failureMessage = "Invalid data, error code RT-5D40";
                        return;
                    }
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                    failureMessage = TransactionMessage.Failure;
                    return;
                }
                int ParentID = new UserDAL().SelectParentID(UserID);
                UplineDetailsWithCommission uplineComm = new SlabDetailBAL().GetUplineDetailsWithCommission(UserID, slabComm);
                int DistributerUserID = uplineComm.DistributerUserID;
                int SuperDistributerUserID = uplineComm.SuperDistributerUserID;
                decimal DistributerCommi = uplineComm.DistributerCommission;//IsCommissionHoldSuperDistAndDist(DistributerUserID, uplineComm.DistributerCommission, out DistributerCommissionHold);
                decimal SuperDistributerCommi = uplineComm.SuperDistributerCommission;// IsCommissionHoldSuperDistAndDist(SuperDistributerUserID, uplineComm.SuperDistributerCommission, out SuperDistributerCommissionHold);

                GSTAndTDSViewModel taxModel = new TaxProcessing().GetCalculateGSTandTDS(rechargeRequest.Amount, slabComm.RetailerComm, DistributerCommi, SuperDistributerCommi,
                0, UserID, DistributerUserID, SuperDistributerUserID, ServiceID);

                decimal comm = slabComm.RetailerComm - taxModel.RetailerTDS;
                DistributerCommi = DistributerCommi - taxModel.DistTDS;
                SuperDistributerCommi = SuperDistributerCommi - taxModel.SuperDistTDS;

                decimal DebitAmount = 0;
                DebitAmount = GetDebitAmount(rechargeRequest.Amount, slabComm.RetailerComm);

                response.Commission = comm;
                response.ClosingBalance = userWallet.Balance - DebitAmount;
                ServiceMaster serName = new ServiceMasterDal().SelectByID(ServiceID);

                response.Status = StatusType.Process;
                response.Message = Message.TransactionMessage.ProcessMessage;

                UserMaster user = new UserDAL().SelectByIDFromCache(UserID);

                string TransactionDescription = $"{serName.Name}, to: {rechargeRequest.ConsumerNumber} Amount: {rechargeRequest.Amount.TwoDigitDecimal()} Commission: {slabComm.RetailerComm.TwoDigitDecimal()} {taxModel.RetTaxDescrption}";
                string DistTransactionDescription = $"{serName.Name}, from txn by {user.FirmName} to: {rechargeRequest.ConsumerNumber} Commission: {slabComm.DistributorComm.TwoDigitDecimal()} {taxModel.DistTaxDescrption}";
                string SuperDistTransactionDescription = $"{serName.Name}, from txn by {user.FirmName} to: {rechargeRequest.ConsumerNumber} Commission: {slabComm.SuperDistributorComm.TwoDigitDecimal()} {taxModel.SuperDistTaxDescrption}";

                OperatorMaster opmaster = new OperatorMasterDAL().SelectByIDFromCache(rechargeRequest.OperatorID);

                if (ServiceID == (int)ServiceCode.LoanRePayment)
                {
                    locationLookUp.Location = rechargeRequest.Longitude;
                    locationLookUp.SimCardDetails = rechargeRequest.Latitude;
                }

                transactionMaster = new TransactionMaster
                {
                    Status = StatusType.Process,
                    UserID = UserID,
                    DistributerUserID = DistributerUserID,
                    SuperDistributerUserID = SuperDistributerUserID,
                    WalletID = WalletID,
                    Amount = rechargeRequest.Amount,
                    DebitAmount = DebitAmount,
                    RetailerCommission = slabComm.RetailerComm,
                    DistributerCommission = DistributerCommi,
                    SuperDistributerCommission = SuperDistributerCommi,
                    OpeningBalance = userWallet.Balance,
                    ClosingBalance = userWallet.Balance - DebitAmount,
                    OperatorID = rechargeRequest.OperatorID,
                    ConsumerNumber = rechargeRequest.ConsumerNumber,
                    IPAddress = IP,
                    Location = locationLookUp.Location,
                    SimCardDetails = locationLookUp.SimCardDetails,
                    Through = rechargeRequest.Through,
                    ServiceID = ServiceID,
                    InboxID = InboxID,
                    ProviderRequestID = requestID,
                    APIUserRequestID = rechargeRequest.APIUserRequestID ?? "0",
                    Circle = CircleResponse?.Circle ?? "",
                    StateID = CircleResponse?.CircleID ?? 0,
                    Bill_Optional1 = rechargeRequest.Param1,
                    Bill_Optional2 = rechargeRequest.Param2,
                    Bill_Optional3 = rechargeRequest.Param3,
                    Bill_Optional4 = rechargeRequest.BillDate,
                    Bill_Optional5 = rechargeRequest.BillerID,
                    Bill_Optional6 = rechargeRequest.BBPSViewID,
                    DMT_PaymentType = rechargeRequest.PaymentType,
                    DMT_IFSC = rechargeRequest.IFSC,
                    IsROffer = IsROfferRecharge,
                    CashBack = CashBack,
                    ResponseType = ResponseType.Initial.ToString(),
                    DMT_BeneficiaryName = rechargeRequest.BeneficiaryName,
                    BBPS_CustomerName = rechargeRequest.BBPS_CustomerName,
                    RetailerTDS = taxModel.RetailerTDS,
                    DistributerTDS = taxModel.DistTDS,
                    SuperDistributerTDS = taxModel.SuperDistTDS,

                    RetailerGST = taxModel.RetailerGST,
                    DistributerGST = taxModel.DistGST,
                    SuperDistributerGST = taxModel.SuperDistGST,
                    Bill_CustomerMessageMobileNo = rechargeRequest.MobileNo,
                    NetCommission = comm
                };
                int transactionMasterID = new TransactionMasterBAL().InsertNonDMTCore(transactionMaster, out failureMessage);
                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                {
                    response.Message = failureMessage;
                    return;
                }

                transactionMaster.TransactionMasterID = transactionMasterID;
                response.VendorID = transactionMasterID;
                response.OperatorName = opmaster.OperatorName;
                response.ServiceName = serName.Name;
                response.DOC = transactionMaster.DOC.DOCFormatDate();             
                response.ProviderRequestID = requestID;
                response.BillDate = transactionMaster.Bill_Optional4;
                //response.MobileNo = user.MobileNumber;

                if (transactionMasterID <= 0)
                {
                    response.Status = StatusType.Failure;
                    failureMessage = TransactionMessage.Failure;
                    response.Message = failureMessage;
                    return;
                }
                try
                {
                    int Result = 0;

                    Result = new AccountProcessing().DebitBalance(
                    UserWallet: userWallet,
                    AmountWithCommission: DebitAmount,
                    Amount: rechargeRequest.Amount,
                    ConsumerNumber: rechargeRequest.ConsumerNumber,
                    CommissionCreditDebit: CommissionCreditDebitType.None,
                    DOCType: DOCType.Recharge,
                    AssignedUserID: UserID,
                    ServiceID: ServiceID,
                    OperatorID: rechargeRequest.OperatorID,
                    TransactionDescription: TransactionDescription,
                    ReferenceID: transactionMasterID,
                    TDS: 0,
                    GST: 0,
                    DiscountAmount: comm);

                    if (Result <= 0)
                    {
                        failureMessage = TransactionMessage.Failure;
                        return;
                    }

                    Parallel.Invoke(
                    () =>
                     {
                         new AccountCoreProcessing().RetailerCreditCommission(UserID, ServiceID, rechargeRequest.OperatorID, transactionMasterID,
                                              comm, rechargeRequest.ConsumerNumber, DOCType.Recharge, TransactionDescription, taxModel.RetailerGST, taxModel.RetailerTDS);
                     },
                    () =>
                    {
                        new AccountCoreProcessing().UplineCreditCommission(UserID, ServiceID, rechargeRequest.OperatorID, transactionMasterID,
                                        DistributerCommi, rechargeRequest.ConsumerNumber, DOCType.Recharge, DistTransactionDescription, taxModel.DistGST, taxModel.DistTDS);
                    }
                    ,
                    () =>
                    {
                        new AccountCoreProcessing().UplineCreditCommission(UserID, ServiceID, rechargeRequest.OperatorID, transactionMasterID,
                                    SuperDistributerCommi, rechargeRequest.ConsumerNumber, DOCType.Recharge, SuperDistTransactionDescription, taxModel.SuperDistGST, taxModel.SuperDistTDS, isForSuperDist: true);
                    },
                    () =>
                    {
                        int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
                        if (UserTypeID != (int)UserType.APIUser)
                        {
                            new LocationLookupBAL().InsertCore(LocationLookUpType.Recharge, "", "", IP, transactionMasterID);
                        }
                    }

                    );
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);

                    response.Status = StatusType.Process;
                    response.Message = TransactionMessage.TransactionInProcess;
                    failureMessage = StatusType.Ok;
                    return;
                }
                //Task.Run(() => AdminAccountDebitProcessing(UserID, WalletID, ServiceID, rechargeRequest.OperatorID, transactionMaster.TransactionMasterID,
                //              slabComm.RetailerComm, transactionMaster.DistributerCommission, transactionMaster.SuperDistributerCommission, rechargeRequest.ConsumerNumber,
                //              DOCType.Recharge, TransactionDescription));
            }

            if (new UserBal().SelectResponseType(UserID) == UserResponseType.Callback)
            {
                FireAndForgetRecharge(transactionMaster, response, thirdPartyService);
                failureMessage = StatusType.Ok;
                return;
            }
            if (thirdPartyService == false)
            {
                ProviderReturnResponse providerReturnResponse = new ProviderProcessing().Hit(transactionMaster, rechargeRequest.Label);
                response.BillerID = providerReturnResponse.ResponseParam5;
                UpdateRechargeStatus(providerReturnResponse, transactionMaster.TransactionMasterID, response, isInitialResponse: true);
            }
        }



        public decimal IsCommissionHoldSuperDistAndDist(int UserID, decimal comm, out decimal CommissionHold)
        {
            CommissionHold = 0;
            UserMaster user = new UserDAL().SelectByIDFromCache(UserID);
            if (user == null)
            {
                return comm;
            }

            if (user.IsChannel)
            {
                return comm;
            }

            if (user.IsCommissionHold && comm >= 0.00001M)
            {
                CommissionHold = comm;
                comm = 0;
                return comm;
            }

            return comm;
        }
        public decimal GetDebitAmount(decimal Amount, decimal comm)
        {
            if (comm >= 0.00001M)
                return Amount;

            Amount = Amount + Math.Abs(comm);
            return Amount;
        }

        private void FireAndForgetRecharge(TransactionMaster transactionMaster, Response.TransactionMaster.TransactionResponse response, bool thirdPartyService)
        {
            Task.Run(() =>
            {
                if (thirdPartyService == false)
                {
                    ProviderReturnResponse providerReturnResponse = new ProviderProcessing().Hit(transactionMaster, "");
                    UpdateRechargeStatus(providerReturnResponse, transactionMaster.TransactionMasterID, response, isInitialResponse: false);// dont change here for isInitialResponse: false
                }
            }
            );

            response.Status = StatusType.Process;
            response.Message = TransactionMessage.RechargeAccepted;
        }

        public void AdminAccountDebitProcessing(int UserID, int WalletID, int ServiceID, int OperatorID, int transactionMasterID,
                                                 decimal RetailerComm, decimal DistributorComm, decimal SuperDistributorComm, string ConsumerNumber,
                                                 string DOCType, string TransactionDescription)
        {
            try
            {
                string AdminAccounting = new SettingMasterBal().SelectByKeyWithoutDefault(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.AdminAccounting));

                if (!AdminAccounting.EqualsIgnoreCase(AdminAccountingType.True))
                    return;

                int UplineID = 0;
                new AccountCoreProcessing().AdminDebitCommission(Company.AdminUserID, UserID, WalletID, ServiceID, OperatorID, transactionMasterID,
                                RetailerComm, ConsumerNumber, DOCType, TransactionDescription);

                ParentViewModel parent = new UserDAL().SelectParent(UserID);
                if (parent == null)
                {
                    return;
                }
                UplineID = parent.ParentID;
                int Parent_DistUserTypeID = new UserDAL().SelectUserTypeID(UplineID);

                if (Parent_DistUserTypeID == (int)UserType.Distributer)
                {
                    new AccountCoreProcessing().AdminDebitCommission(Company.AdminUserID, UplineID, WalletID, ServiceID, OperatorID, transactionMasterID,
                                DistributorComm, ConsumerNumber, DOCType, TransactionDescription);
                }
                else if (Parent_DistUserTypeID == (int)UserType.SuperDistributer)
                {
                    new AccountCoreProcessing().AdminDebitCommission(Company.AdminUserID, UplineID, WalletID, ServiceID, OperatorID, transactionMasterID,
                           SuperDistributorComm, ConsumerNumber, DOCType, TransactionDescription, isForSuperDist: true);
                    return;
                }
                ParentViewModel superparent = new UserDAL().SelectParent(UplineID);
                if (superparent == null)
                {
                    return;
                }
                UplineID = superparent.ParentID;

                if (UplineID <= 0)
                {
                    return;
                }
                int Parent_SuperUserTypeID = new UserDAL().SelectUserTypeID(UplineID);
                if (Parent_SuperUserTypeID == (int)UserType.SuperDistributer)
                {
                    new AccountCoreProcessing().AdminDebitCommission(Company.AdminUserID, UplineID, WalletID, ServiceID, OperatorID, transactionMasterID,
                            SuperDistributorComm, ConsumerNumber, DOCType, TransactionDescription, isForSuperDist: true);
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return;
            }
        }

        internal string ResendAllTransactions(List<ResendRequest> mainRequest, int userID)
        {
            string failureMessge = TransactionMessage.InvalidTransaction;
            if (mainRequest == null || mainRequest.Count <= 0)
                return failureMessge;

            foreach (ResendRequest request in mainRequest)
                failureMessge = ResendTransaction(request, userID);

            return failureMessge;
        }

        public bool NotInMobileAndUtilityService(int ServiceID)
        {
            if (ServiceID == (int)ServiceCode.FundTransfer)
                return false;

            if (ServiceID == (int)ServiceCode.AEPS)
                return false;

            if (ServiceID == (int)ServiceCode.MATM)
                return false;

            if (ServiceID == (int)ServiceCode.PrepaidActivation)
                return false;

            if (ServiceID == (int)ServiceCode.CashDeposit)
                return false;

            if (ServiceID == (int)ServiceCode.CreditCardPayment)
                return false;

            if (ServiceID == (int)ServiceCode.PhoneBooking)
                return false;

            if (ServiceID == (int)ServiceCode.PhoneBooking)
                return false;

            if (ServiceID == (int)ServiceCode.PaymentGateway)
                return false;

            if (ServiceID == (int)ServiceCode.MoneyTransfer)
                return false;

            if (ServiceID == (int)ServiceCode.Payout)
                return false;

            if (ServiceID == (int)ServiceCode.DTHBooking)
                return false;

            if (ServiceID == (int)ServiceCode.LapuLoading)
                return false;

            if (ServiceID == (int)ServiceCode.WalletTopUp)
                return false;

            if (ServiceID == (int)ServiceCode.FRC)
                return false;

            return true;
        }

        public bool IsMobileService(int ServiceID)
        {
            if (ServiceID == (int)ServiceCode.Mobile ||
                    ServiceID == (int)ServiceCode.Dth ||
                    ServiceID == (int)ServiceCode.PostPaid)
                return true;

            return false;
        }

        internal void UpdateRechargeStatus(ProviderReturnResponse extratedResponse, int transactionMasterID, Response.TransactionMaster.TransactionResponse retObj, bool isInitialResponse = false)
        {
            // retObj data should preserve in Recharge page, like vendorid, OpBal, Txn ID
            retObj.Status = StatusType.Process;
            retObj.Message = "Transaction is in process";

            if (extratedResponse == null) return;
            if (extratedResponse.Status == null) return;

            TransactionMaster transactionMaster = new TransactionDAL().SelectByID(transactionMasterID);
            // Provider ID Check
            if (transactionMaster.ProviderID != extratedResponse.ProviderID)
                return;

            if (isInitialResponse == false)
            {
                transactionMaster.ResponseType = ResponseType.Callback.ToString();
            }
            transactionMaster.Response = extratedResponse.Response;
            transactionMaster.Description = extratedResponse.Description;
            //transactionMaster.LapuNo = extratedResponse.LapuNo;
            transactionMaster.ReferenceNo = extratedResponse.TransactionID;
            transactionMaster.ProviderRefNo = extratedResponse.ProviderRefNo.ToUpper();// this is provider reference no, some api need, own ref ID, like Instant pay, RoundPay
            transactionMaster.ProviderDOCNo = extratedResponse.ProviderRefNo.ToUpper();
            transactionMaster.Bill_Optional5 = extratedResponse.ResponseParam5;

            transactionMaster.DMT_BeneficaryNameByBank = extratedResponse.CustomerName;

            if (!extratedResponse.CustomerName.EqualsIgnoreCase(""))
                transactionMaster.DMT_BeneficiaryName = extratedResponse.CustomerName;

            retObj.BeneficiaryName = extratedResponse.CustomerName;

            TransactionResponseProcessing trp = new TransactionResponseProcessing();
            transactionMaster.ProviderClosingBalance = Convert.ToDecimal(trp.ParseBalance(extratedResponse.Balance));
            transactionMaster.ROfferCommission = Convert.ToDecimal(trp.ParseBalance(extratedResponse.ROfferCommission));
            transactionMaster.ProviderCommission = Convert.ToDecimal(trp.ParseBalance(extratedResponse.ProviderCommission));
            decimal ProviderCommissionPercentage = Convert.ToDecimal(trp.ParseBalance(extratedResponse.ProviderCommissionPercentage));

            if (transactionMaster.ProviderCommission <= 0 && ProviderCommissionPercentage > 0)
                transactionMaster.ProviderCommission = GetCalculateCommi(ProviderCommissionPercentage, transactionMaster.Amount);

            retObj.TransactionID = transactionMaster.ReferenceNo;
            retObj.RefrenceID = extratedResponse.ProviderRefNo;

            //
            InsertTransactionStatusChange(transactionMaster, extratedResponse.Status);
            // Compalint Callback Refund
            if (IsValidComplaintCallback(transactionMasterID, transactionMaster.Status))
            {
                if (new ResponseProcess().CheckSuccess(extratedResponse.Status))
                {
                    CompliantUpdate(transactionMaster);

                    return;
                }
                if (new ResponseProcess().CheckFailure(extratedResponse.Status))
                {
                    RefundByComplaintCallback(transactionMaster);
                    return;
                }
            }

            if (isInitialResponse == true)
            {
                transactionMaster.HitBackCount = 1;
            }

            if (new ResponseProcess().CheckSuccess(extratedResponse.Status))// success
            {
                if (transactionMaster.Response.TrimIfNotNull().Length >= 64)
                {
                    transactionMaster.Response = "";// no need to store response if success
                }

                transactionMaster.Status = StatusType.Success;
                new AccountCoreProcessing().SuccessTransaction(transactionMaster);
                retObj.Status = StatusType.Success;
                retObj.Message = TransactionMessage.Successmessage;
                HitCallBackToCallbackStatusChangeAllowed(transactionMaster);
                WebHookCall(transactionMaster, retObj.Operator);

                return;
            }

            if (new ResponseProcess().CheckFailure(extratedResponse.Status))// Failure
            {
                transactionMaster.Status = StatusType.Failure;
                retObj.Status = StatusType.Failure;
                retObj.Message = CheckMessage(extratedResponse.Description);
                retObj.Commission = 0;
                retObj.ClosingBalance = retObj.OpeningBalance;

                new AccountCoreProcessing().RefundTransaction(transactionMaster, extratedResponse.Description, DOCType.Recharge);
                HitCallBackToCallbackStatusChangeAllowed(transactionMaster);
                AutoBalanceTransfer(transactionMaster.UserID, transactionMaster.WalletID, transactionMaster.ClosingBalance);

                return;
            }
        }
        private void InsertTransactionStatusChange(TransactionMaster tranModel, string Newstatus)
        {
            int CurrentdateInt = Int32.Parse(DateTime.Now.ToString(Messages.DOCIntegerFomrat));
            if (tranModel.DOCInt == CurrentdateInt)
                return;
            if (tranModel.Status.EqualsIgnoreCase(Newstatus))
                return;
            Task.Run(() => new TransactionStatusChangeBal().Insert(tranModel, tranModel.Status, Newstatus));
        }

        private void WebHookCall(TransactionMaster transactionMaster, int OperatorID)
        {
            Task.Run(() => WebHookCallCore(transactionMaster, OperatorID));
        }
        private void WebHookCallCore(TransactionMaster transactionMaster, int OperatorID)
        {
            OperatorMaster opmaster = new OperatorMasterDAL().SelectByID(OperatorID);

            Response.OperatorwiseCallbackSetting.OperatorWiseCallbackSettingResponse Resp =
                new OperatorwiseCallbackSettingDal().SelectByOperatorID(OperatorID);
            if (Resp == null)
                return;
            if (Resp.Url.Length <= 10)
                return;

            UserMaster User = new UserDAL().SelectByIDFromCache(transactionMaster.UserID);

            string headerJson = Resp.HeaderJson.ToString().Trim();
            string Doc = transactionMaster.DOC.ToString();
            if (Resp.DateFormat.Length > 5)
            {
                Doc = transactionMaster.DOC.ToString(Resp.DateFormat, CultureInfo.InvariantCulture);
            }
            string urlWithParam = new TransactionBal().URLFormatting(methodType: Resp.MethodType, baseURL: Resp.Url,
                                                 ConsumerNumber: transactionMaster.ConsumerNumber,
                                                 Amount: transactionMaster.Amount, BBPS_CustomerName: transactionMaster.BBPS_CustomerName,
                                                 ReferenceNo: transactionMaster.ReferenceNo, Operator: opmaster.OperatorName,
                                                 Date: Doc, RetailerName: User.FirmName, Location: transactionMaster.Location,
                                                 SimCardDetails: transactionMaster.SimCardDetails, Status: transactionMaster.Status);
            if (Resp.MethodType.EqualsIgnoreCase("GET"))
            {
                HttpProcessing.CallGetMethod(urlWithParam);
            }
            else
            {
                HttpProcessing.CallPostMethod(urlWithParam, headerJson);
            }
        }

        public string URLFormatting(string methodType, string baseURL, string ConsumerNumber, decimal Amount, string BBPS_CustomerName, string ReferenceNo, string Operator, string Date, string RetailerName, string Location, string SimCardDetails, string Status)
        {
            string str;
            try
            {
                str = baseURL;
                str = str.Replace("[Amount]", getAmountAsIntergerIfInterger(Amount));

                str = getURLEncode(methodType, str, "[consumernumber]", ConsumerNumber);
                str = getURLEncode(methodType, str, "[BBPS_CustomerName]", BBPS_CustomerName);
                str = getURLEncode(methodType, str, "[ReferenceNo]", ReferenceNo);

                str = getURLEncode(methodType, str, "[Date]", Date);
                str = getURLEncode(methodType, str, "[Operator]", Operator);

                str = getURLEncode(methodType, str, "[RetailerName]", RetailerName);
                str = getURLEncode(methodType, str, "[Location]", Location);
                str = getURLEncode(methodType, str, "[SimCardDetails]", SimCardDetails);
                str = getURLEncode(methodType, str, "[Status]", Status);
            }
            catch (Exception ex)
            {
                str = "";
                new Logger().Write(ex);
            }

            return str;
        }

        private string getAmountAsIntergerIfInterger(decimal Amount)
        {
            if ((Amount % 1) == 0)
            {
                return Convert.ToInt32(Amount).ToString();
            }
            else
            {
                return Amount.ToString();
            }
        }

        private string getURLEncode(string methodType, string mainStr, string replacewith, string replaceWord)
        {
            if (methodType.EqualsIgnoreCase(MethodType.Get))
            {
                mainStr = mainStr.Replace(replacewith, System.Web.HttpUtility.UrlEncode(replaceWord));
            }
            else
            {
                mainStr = mainStr.Replace(replacewith, replaceWord);
            }
            return mainStr;
        }

        private string CheckMessage(string DescriptionMessage)
        {
            if (DescriptionMessage.EqualsIgnoreCase("INSUFFICIENT BALANCE. CONTACT CUSTOMER CARE!"))
            {
                DescriptionMessage = "Service is down, error code LB, please contact customer care";

            }

            return DescriptionMessage;
        }

        private void HitCallBackToCallbackStatusChangeAllowed(TransactionMaster transactionMaster)
        {
            Task.Run(() =>
            {
                if (transactionMaster.HitBackCount <= 0)
                    return;
                int UserTypeID = new UserDAL().SelectUserTypeID(transactionMaster.UserID);
                if (UserTypeID != (int)UserType.APIUser)
                    return;
                if (!IsCallbackStatusChangeAndSuccessToFail())
                    return;
                CallBackToAPIUser(transactionMaster, UserTypeID);
            });
        }
        private bool IsCallbackStatusChangeAndSuccessToFail()
        {
            if (Company.CallbackStatusChangeAllowed.EqualsIgnoreCase("Yes"))
                return true;
            if (Company.CallbackStatusSuccessToFailOnly.EqualsIgnoreCase("Yes"))
                return true;
            return false;
        }
        private void CallBackToAPIUser(TransactionMaster transactionMaster, int UserTypeID)
        {
            string CallbackURL = new CallbackMasterDal().SelectCallbackURL(transactionMaster.UserID, transactionMaster.ServiceID);
            Task.Run(() => CallBackPosting.postCallBackToAPIUser(transactionMaster, UserTypeID, CallbackURL));
        }

        public void AutoBalanceTransfer(int UserID, int WalletID, decimal ClosingBalance)
        {
            Task.Run(() => AutoBalanceCore(UserID, WalletID, ClosingBalance));
        }

        private void AutoBalanceCore(int UserID, int WalletID, decimal ClosingBalance)
        {
            AutoTransferUserwiseMapping ObjModel = new AutoTransferUserwiseMappingDal().SelectByUserIDOTPVerified(UserID);
            if (ObjModel == null)
                return;

            if (!IsValidForAutoTransfer(ClosingBalance, UserID, WalletID, ObjModel)) return;

            if (WalletID == (int)WalletCode.Recharge)
                new FundBal().AutoBalanceTransfer(UserID, WalletID, ObjModel.RechargeAmountIncreasedBy);

            if (WalletID == (int)WalletCode.DMT)
                new FundBal().AutoBalanceTransfer(UserID, WalletID, ObjModel.DMTAmountIncreasedBy);
        }

        private bool IsValidForAutoTransfer(decimal ClosingBalance, int UserID, int WalletID, AutoTransferUserwiseMapping ObjModel)
        {
            if (!Company.AutoBalanceTransfer.EqualsIgnoreCase("Yes")) return false;

            if (ObjModel == null) return false;

            if (WalletID == (int)WalletCode.Recharge)
            {
                if (!IsRechargeTransactionCheck(ObjModel, ClosingBalance))
                {
                    return false;
                }
            }

            if (WalletID == (int)WalletCode.DMT)
            {
                if (!IsDMTTransactionCheck(ObjModel, ClosingBalance))
                {
                    return false;
                }
            }

            float Outstanding = new CashCollectionMasterDal().SelectOutstandingByUserID(UserID);

            if (ObjModel.MaxOutstanding <= Convert.ToDecimal(Outstanding)) return false;

            return true;
        }

        private bool IsDMTTransactionCheck(AutoTransferUserwiseMapping ObjModel, decimal ClosingBalance)
        {
            if (ObjModel.DMTAmountIncreasedBy <= 0)
                return false;

            if (ObjModel.DMTLowBalanceReached < 0)
                return false;

            if (ClosingBalance > ObjModel.DMTLowBalanceReached)
                return false;

            return true;
        }

        private bool IsRechargeTransactionCheck(AutoTransferUserwiseMapping ObjModel, decimal ClosingBalance)
        {
            if (ObjModel.RechargeAmountIncreasedBy <= 0)
                return false;

            if (ObjModel.RechargeLowBalanceReached < 0)
                return false;

            if (ClosingBalance > ObjModel.RechargeLowBalanceReached)
                return false;

            return true;
        }


        public decimal GetCalculateCommi(decimal comm, decimal amount)
        {
            // comm always +ve
            if (comm == 0)
                return comm;

            decimal flexi = amount * (comm / 100);
            return flexi;
        }

        public bool IsValidComplaintCallback(int TransactionMasterID, string dbStatus)
        {
            if (!new ResponseProcess().CheckSuccess(dbStatus))// in database we need only success status
            {
                return false;
            }
            ComplaintMaster responseObj = new ComplaintMasterDal().SelectByTransactionMasterID(TransactionMasterID);
            if (responseObj == null)
                return false;

            if (!responseObj.ComplaintStatus.EqualsIgnoreCase(StatusType.Process))
                return false;

            return true;
        }

        private void RefundByComplaintCallback(TransactionMaster transactionMaster)
        {
            transactionMaster.Status = StatusType.Failure;
            new AccountCoreProcessing().RefundTransaction(transactionMaster, "Refund from compaint callback", DOCType.Recharge);
            CompliantUpdate(transactionMaster);

        }

        private void CompliantUpdate(TransactionMaster transactionMaster)
        {
            ComplaintMaster responseObj = new ComplaintMasterDal().SelectByTransactionMasterID(transactionMaster.TransactionMasterID);
            if (responseObj == null)
                return;

            ComplaintMaster ObjModel = new ComplaintMasterDal().SelectByID(responseObj.ComplaintMasterID);
            if (ObjModel == null)
                return;

            int OperatorID = ObjModel.OperatorID;
            OperatorMaster opmaster = new OperatorMasterDAL().SelectByID(OperatorID);

            if (transactionMaster.Status.EqualsIgnoreCase(StatusType.Failure))
            {
                ObjModel.ComplaintStatus = ComplaintStatusType.Accept;
                ObjModel.AdminRemark = "Refund from complaint callback";
                Task.Run(() =>
                {
                    SendComplaintCallback(transactionMaster);
                    int WalletID = new WalletServiceMappingBAL().SelectWalletIDByServiceID(transactionMaster.ServiceID);
                    UserWalletMaster userWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(ObjModel.UserID, WalletID);
                    new MessageSelection().SendComplaintAcceptMessage(ObjModel.UserID, ObjModel.ConsumerNumber, ObjModel.Amount, userWallet.Balance, opmaster.OperatorName);
                });
            }
            if (transactionMaster.Status.EqualsIgnoreCase(StatusType.Success))
            {
                ObjModel.ComplaintStatus = ComplaintStatusType.Reject;
                ObjModel.AdminRemark = "Reject from complaint callback";
                Task.Run(() =>
                {
                    SendComplaintCallback(transactionMaster);
                    int WalletID = new WalletServiceMappingBAL().SelectWalletIDByServiceID(transactionMaster.ServiceID);
                    UserWalletMaster userWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(ObjModel.UserID, WalletID);
                    new MessageSelection().SendComplaintRejectMessage(ObjModel.UserID, ObjModel.ConsumerNumber, ObjModel.Amount, userWallet.Balance, opmaster.OperatorName);
                });
            }

            int Result = -1;
            try
            {
                Result = new ComplaintMasterDal().Update(ObjModel);
            }
            catch (Exception ex)

            {
                new Logger().Write(ex);
                return;
            }
        }

        private void SendComplaintCallback(TransactionMaster transactionMaster)
        {
            int UserTypeID = new UserDAL().SelectUserTypeID(transactionMaster.UserID);
            if (UserTypeID == (int)UserType.APIUser)
            {
                string CallbackURL = new CallbackMasterDal().SelectCallbackURL(transactionMaster.UserID, transactionMaster.ServiceID);
                Task.Run(() => CallBackPosting.postCallBackToAPIUser(transactionMaster, UserTypeID, CallbackURL));
            }
        }

        internal async Task<(string, LiveTransactionWrapper)> LiveTransaction(ReportRequest txReportRequest, KeyDataModel keydata)
        {
            string failureMessage = "No data found";

            LiveTransactionWrapper wrapper = new LiveTransactionWrapper();
            List<Response.TransactionMaster.ViewModel> list = new List<Response.TransactionMaster.ViewModel>();
            failureMessage = txReportRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return (failureMessage, wrapper);
            }

            HeaderViewModel head = new HeaderViewModel();

            try
            {
                var listData = new TransactionDAL().LiveTransaction(txReportRequest, keydata.UserID);
                var headerData = new TransactionDAL().LiveHeaderSummary(txReportRequest, keydata.UserID);

                await Task.WhenAll(listData, headerData);

                list = listData.Result;
                head = headerData.Result;
            }
            catch (Exception ex)
            {
                failureMessage = Messages.ExceptionOccured;
                new Logger().Write(ex);
                return (failureMessage, wrapper);
            }
            failureMessage = Messages.DataNotFound;

            if (list == null || list.Count <= 0) return (failureMessage, wrapper);

            wrapper.HeaderSummary = head;
            wrapper.list = list;
            failureMessage = StatusType.Ok;
            return (failureMessage, wrapper);
        }

        internal ReportWrapper SelectForFilter(ReportRequest txReportRequest, KeyDataModel keydata, out string failureMessage)
        {
            ReportWrapper wrapper = new ReportWrapper();
            int UserTypeID = new UserDAL().SelectUserTypeID(keydata.UserID);

            failureMessage = txReportRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;

            List<Response.TransactionMaster.ViewModel> list = new List<Response.TransactionMaster.ViewModel>();
            try
            {
                if (txReportRequest.IsExcelDownload || new Validation().IsValidString(txReportRequest.CriteriaText))
                {
                    list = new TransactionDAL().SelectForFilterAndExcelDownload(txReportRequest, keydata.UserID);
                }
                else
                {
                    list = new TransactionDAL().SelectForFilter(txReportRequest, keydata.UserID);
                }
            }
            catch (Exception ex)
            {
                failureMessage = Messages.ExceptionOccured;
                new Logger().Write(ex);
                return wrapper;
            }
            failureMessage = Messages.DataNotFound;

            if (list == null || list.Count <= 0) return wrapper;

            if (txReportRequest.SkipCount <= 0)
            {
                wrapper.HeaderSummary = GetHeaderSummaryData(txReportRequest, keydata.UserID, "Recharge");
            }

            list = list.AsQueryable().WhereDynamic(txReportRequest.Search).ToList();

            list = list.AsQueryable().WhereDynamic(txReportRequest.CriteriaText).ToList();

            list = list.AsQueryable().OrderByDynamic(txReportRequest.SortColumnName, txReportRequest.SortDirection).ToList();

            try
            {
                wrapper.Count = new TransactionDAL().GetTransactionCount(txReportRequest, keydata.UserID);
            }
            catch (Exception)
            {
            }

            wrapper.IsExcelDownload = txReportRequest.IsExcelDownload;

            string ShowCommission = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.ShowCommission), "Yes");

            if (ShowCommission.EqualsIgnoreCase("No") && keydata.UserID != Company.AdminUserID)
            {
                foreach (Response.TransactionMaster.ViewModel item in list)
                {
                    item.RetailerCommission = 0;
                    item.DistributerCommission = 0;
                    item.SuperDistributerCommission = 0;
                }
            }
            if (keydata.UserID != Company.AdminUserID)
            {
                foreach (Response.TransactionMaster.ViewModel item in list)
                {
                    item.ProviderID = 0;
                    item.ProviderName = "";
                }
            }

            foreach (Response.TransactionMaster.ViewModel item in list)
            {
                if (Util.isRoleAdminOrSupport(UserTypeID))
                    item.CommissionInfo = $"R :{item.RetailerCommission.TwoDigitDecimal()}{Environment.NewLine} D :{item.DistributerCommission.TwoDigitDecimal()}{Environment.NewLine} S :{item.SuperDistributerCommission.TwoDigitDecimal()}";

                if (UserTypeID == (int)UserType.SuperDistributer)
                    item.CommissionInfo = $"R :{item.RetailerCommission.TwoDigitDecimal()}{Environment.NewLine} D :{item.DistributerCommission.TwoDigitDecimal()}{Environment.NewLine} S :{item.SuperDistributerCommission.TwoDigitDecimal()}";

                if (UserTypeID == (int)UserType.Distributer)
                    item.CommissionInfo = $"R :{item.RetailerCommission.TwoDigitDecimal()}{Environment.NewLine} D :{item.DistributerCommission.TwoDigitDecimal()}";
                if (UserTypeID == (int)UserType.Retailer || UserTypeID == (int)UserType.APIUser)
                    item.CommissionInfo = $"R :{item.RetailerCommission.TwoDigitDecimal()} ";
                if (item.ComplaintStatus.EqualsIgnoreCase(ComplaintStatusType.Accept))
                    item.ComplaintStatus = ComplaintStatusType.Refund;
            }

            wrapper.list = list;

            failureMessage = StatusType.Ok;
            return wrapper;
        }

        internal List<HourlyViewModel> HourlyPerformance()
        {
            List<HourlyViewModel>? list = new List<HourlyViewModel>();
            bool PerfomenceGraph = new SettingMasterBal().GetPerfomenceGraph();
            if (PerfomenceGraph == true)
            {
                list = new TransactionDAL().HourlyPerformance();

                if (list.Count <= 0)
                    return list;

                return list;
            }


            return list;
        }


        internal List<Last12hrViewModel> Last12hrPerformance()
        {
            List<Last12hrViewModel>? list = new List<Last12hrViewModel>();
            bool PerfomenceGraph = new SettingMasterBal().GetPerfomenceGraph();
            if (PerfomenceGraph == true)
            {
                list = new TransactionDAL().Last12hrPerformance();
                return list;
            }

            return list;
        }

        internal List<AllProviderWisePerformenceViewModel> AllProviderWisePerformance(ProviderWisePerformanceGraphRequest mainRequest)
        {
            List<AllProviderWisePerformenceViewModel>? list = new List<AllProviderWisePerformenceViewModel>();

            bool PerfomenceGraph = new SettingMasterBal().GetPerfomenceGraph();
            if (PerfomenceGraph == true)
            {
                list = new TransactionDAL().AllProviderWisePerformance(mainRequest);
                return list;
            }
            return list;
        }

        internal List<Last12hrViewModel> ProviderWisePerformance(ProviderWisePerformanceGraphRequest mainRequest)
        {
            List<Last12hrViewModel>? list = new List<Last12hrViewModel>();
            bool PerfomenceGraph = new SettingMasterBal().GetPerfomenceGraph();
            if (PerfomenceGraph == true)
            {
                list = new TransactionDAL().ProviderWisePerformance(mainRequest);
                return list;
            }
            return list;
        }





        internal HeaderViewModel GetRofferHeaderSummaryData(ReportRequest mainRequest, int UserID)
        {
            List<Response.TransactionMaster.ViewModel> list = new List<Response.TransactionMaster.ViewModel>();
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            list = new TransactionReportDAL().SelectForROfferAndHeaderSummary(mainRequest, UserID);
            var Success = (from item in list
                           where item.Status == StatusType.Success
                           group item by new { item.Status } into grp
                           select new
                           {
                               SuccessAmount = grp.Sum(X => X.Amount),
                               RetCommi = grp.Sum(X => X.RetailerCommission),
                               DistCommi = grp.Sum(X => X.DistributerCommission),
                               SuperCommi = grp.Sum(X => X.SuperDistributerCommission),
                               SuccessCount = grp.Count(),
                           }).FirstOrDefault();

            var Pending = (from item in list
                           where item.Status == StatusType.Process
                           group item by new { item.Status } into grp
                           select new
                           {
                               PendingAmount = grp.Sum(X => X.Amount),
                               PendingCount = grp.Count(),
                           }).FirstOrDefault();

            HeaderViewModel headerlist = new HeaderViewModel();
            headerlist.SuccessAmount = Success?.SuccessAmount ?? 0;
            headerlist.SuccessCount = Success?.SuccessCount ?? 0;

            headerlist.FailedAmount = 0;
            headerlist.FailedCount = 0;

            headerlist.ManualFailAmount = 0;
            headerlist.ManualFailCont = 0;

            headerlist.PendingAmount = Pending?.PendingAmount ?? 0;
            headerlist.PendingCount = Pending?.PendingCount ?? 0;

            headerlist.TotalAmount = headerlist.SuccessAmount + headerlist.FailedAmount + headerlist.PendingAmount;
            headerlist.TotalCount = headerlist.SuccessCount + headerlist.FailedCount + headerlist.PendingCount;

            if (UserTypeID == (int)UserType.Admin)
            {
                headerlist.Commission = Success?.RetCommi ?? 0 + Success?.DistCommi ?? 0 + Success?.SuperCommi ?? 0;
            }
            if (UserTypeID == (int)UserType.SuperDistributer)
            {
                headerlist.Commission = Success?.SuperCommi ?? 0;
            }
            if (UserTypeID == (int)UserType.Distributer)
            {
                headerlist.Commission = Success?.DistCommi ?? 0;
            }
            if (UserTypeID == (int)UserType.Retailer || UserTypeID == (int)UserType.APIUser)
            {
                headerlist.Commission = Success?.RetCommi ?? 0;
            }

            return headerlist;
        }

        internal HeaderViewModel GetHeaderSummaryData(ReportRequest mainRequest, int UserID, string type)
        {
            List<Response.TransactionMaster.ViewModel> list = new List<Response.TransactionMaster.ViewModel>();
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            if (type.EqualsIgnoreCase("DMT"))
            {
                list = new TransactionDAL().SelectForFilterAndDMTHeaderSummary(mainRequest, UserID);
            }
            if (type.EqualsIgnoreCase("Recharge"))
            {
                list = new TransactionDAL().SelectForFilterAndHeaderSummary(mainRequest, UserID);
            }

            var Success = (from item in list
                           where item.Status == StatusType.Success
                           group item by new { item.Status } into grp
                           select new
                           {
                               SuccessAmount = grp.Sum(X => X.Amount),
                               RetCommi = grp.Sum(X => X.RetailerCommission),
                               DistCommi = grp.Sum(X => X.DistributerCommission),
                               SuperCommi = grp.Sum(X => X.SuperDistributerCommission),
                               SuccessCount = grp.Count(),
                           }).FirstOrDefault();

            var Fail = (from item in list
                        where item.Status == StatusType.Failure
                        group item by new { item.Status } into grp
                        select new
                        {
                            FailedAmount = grp.Sum(X => X.Amount),
                            FailedCount = grp.Count(),
                        }).FirstOrDefault();

            var ManualFail = (from item in list
                              where item.Description == StatusType.ManualFail
                              group item by new { item.Description } into grp
                              select new
                              {
                                  FailedAmount = grp.Sum(X => X.Amount),
                                  FailedCount = grp.Count(),
                              }).FirstOrDefault();

            var Pending = (from item in list
                           where item.Status == StatusType.Process
                           group item by new { item.Status } into grp
                           select new
                           {
                               PendingAmount = grp.Sum(X => X.Amount),
                               PendingCount = grp.Count(),
                           }).FirstOrDefault();

            HeaderViewModel headerlist = new HeaderViewModel();
            headerlist.SuccessAmount = Success?.SuccessAmount ?? 0;
            headerlist.SuccessCount = Success?.SuccessCount ?? 0;

            headerlist.FailedAmount = Fail?.FailedAmount ?? 0;
            headerlist.FailedCount = Fail?.FailedCount ?? 0;

            headerlist.ManualFailAmount = ManualFail?.FailedAmount ?? 0;
            headerlist.ManualFailCont = ManualFail?.FailedCount ?? 0;

            headerlist.PendingAmount = Pending?.PendingAmount ?? 0;
            headerlist.PendingCount = Pending?.PendingCount ?? 0;

            headerlist.TotalAmount = headerlist.SuccessAmount + headerlist.FailedAmount + headerlist.PendingAmount;
            headerlist.TotalCount = headerlist.SuccessCount + headerlist.FailedCount + headerlist.PendingCount;

            if (UserTypeID == (int)UserType.Admin)
            {
                headerlist.Commission = Success?.RetCommi ?? 0 + Success?.DistCommi ?? 0 + Success?.SuperCommi ?? 0;
            }
            if (UserTypeID == (int)UserType.SuperDistributer)
            {
                headerlist.Commission = Success?.SuperCommi ?? 0;
            }
            if (UserTypeID == (int)UserType.Distributer)
            {
                headerlist.Commission = Success?.DistCommi ?? 0;
            }
            if (UserTypeID == (int)UserType.Retailer || UserTypeID == (int)UserType.APIUser)
            {
                headerlist.Commission = Success?.RetCommi ?? 0;
            }

            return headerlist;
        }

        internal HeaderViewModel GetHeaderSummaryDataByList(List<Response.TransactionMaster.ViewModel> list, int UserID)
        {
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            var Success = (from item in list
                           where item.Status == StatusType.Success
                           group item by new { item.Status } into grp
                           select new
                           {
                               SuccessAmount = grp.Sum(X => X.Amount),
                               RetCommi = grp.Sum(X => X.RetailerCommission),
                               DistCommi = grp.Sum(X => X.DistributerCommission),
                               SuperCommi = grp.Sum(X => X.SuperDistributerCommission),
                               SuccessCount = grp.Count(),
                           }).FirstOrDefault();

            var Fail = (from item in list
                        where item.Status == StatusType.Failure
                        group item by new { item.Status } into grp
                        select new
                        {
                            FailedAmount = grp.Sum(X => X.Amount),
                            FailedCount = grp.Count(),
                        }).FirstOrDefault();

            var ManualFail = (from item in list
                              where item.Description == StatusType.ManualFail
                              group item by new { item.Description } into grp
                              select new
                              {
                                  FailedAmount = grp.Sum(X => X.Amount),
                                  FailedCount = grp.Count(),
                              }).FirstOrDefault();

            var Pending = (from item in list
                           where item.Status == StatusType.Process
                           group item by new { item.Status } into grp
                           select new
                           {
                               PendingAmount = grp.Sum(X => X.Amount),
                               PendingCount = grp.Count(),
                           }).FirstOrDefault();

            HeaderViewModel headerlist = new HeaderViewModel();
            headerlist.SuccessAmount = Success?.SuccessAmount ?? 0;
            headerlist.SuccessCount = Success?.SuccessCount ?? 0;

            headerlist.FailedAmount = Fail?.FailedAmount ?? 0;
            headerlist.FailedCount = Fail?.FailedCount ?? 0;

            headerlist.ManualFailAmount = ManualFail?.FailedAmount ?? 0;
            headerlist.ManualFailCont = ManualFail?.FailedCount ?? 0;

            headerlist.PendingAmount = Pending?.PendingAmount ?? 0;
            headerlist.PendingCount = Pending?.PendingCount ?? 0;

            headerlist.TotalAmount = headerlist.SuccessAmount + headerlist.FailedAmount + headerlist.PendingAmount;
            headerlist.TotalCount = headerlist.SuccessCount + headerlist.FailedCount + headerlist.PendingCount;

            if (UserTypeID == (int)UserType.Admin)
            {
                headerlist.Commission = Success?.RetCommi ?? 0 + Success?.DistCommi ?? 0 + Success?.SuperCommi ?? 0;
            }
            if (UserTypeID == (int)UserType.SuperDistributer)
            {
                headerlist.Commission = Success?.SuperCommi ?? 0;
            }
            if (UserTypeID == (int)UserType.Distributer)
            {
                headerlist.Commission = Success?.DistCommi ?? 0;
            }
            if (UserTypeID == (int)UserType.Retailer || UserTypeID == (int)UserType.APIUser)
            {
                headerlist.Commission = Success?.RetCommi ?? 0;
            }

            return headerlist;
        }

        internal Response.TransactionMaster.ViewModel SelectByRequestID(string ProviderRequestID)
        {
            Response.TransactionMaster.ViewModel transactionMasterViewModel = null;

            try
            {
                transactionMasterViewModel = new TransactionDAL().SelectByRequestID(ProviderRequestID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return transactionMasterViewModel;
        }

        internal Response.TransactionMaster.ViewModel SelectByRequestIDWithProviderID(int ProviderID, string ProviderRequestID)
        {
            Response.TransactionMaster.ViewModel transactionMasterViewModel = null;

            try
            {
                transactionMasterViewModel = new TransactionDAL().SelectByRequestIDWithProviderID(ProviderID, ProviderRequestID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return transactionMasterViewModel;
        }

        internal string UpdateProviderID(int ProviderID, int TransactionMasterID)
        {
            TransactionMaster ObjModel = null;
            try
            {
                ObjModel = new TransactionDAL().SelectByID(TransactionMasterID);
                if (ObjModel == null)
                    return Message.TransactionMessage.NotFound;

                ObjModel.ProviderID = ProviderID;
                int rechargeAmount = Convert.ToInt32(ObjModel.Amount);

                int Result = new TransactionDAL().Update(ObjModel);
                if (Result <= 0)
                    return Message.TransactionMessage.ProviderUpdateFailure;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return StatusType.Ok;
        }

        internal string UpdatePendingProviderStatus(string PendingProviderStatus, int TransactionMasterID)
        {
            TransactionMaster ObjModel = null;
            try
            {
                ObjModel = new TransactionDAL().SelectByID(TransactionMasterID);
                if (ObjModel == null)
                    return Message.TransactionMessage.NotFoundID;

                ObjModel.PendingProviderStatus = PendingProviderStatus;

                int Result = new TransactionDAL().Update(ObjModel);
                if (Result <= 0)
                    return Message.TransactionMessage.ProviderUpdateFailure;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return StatusType.Ok;
        }

        internal float SelectTodaysCommission(int UserID)
        {
            return new TransactionDAL().SelectTodaysCommission(UserID);
        }

        private bool isValidaUserForReceipt(RechargeReceiptViewModel receipt, int UserID)
        {
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            if (Util.isRoleAdminOrSupport(UserTypeID))
            {
                return true;
            }

            if (receipt.UserID == UserID)
            {
                return true;
            }

            if (receipt.DistributorID == UserID)
            {
                return true;
            }
            if (receipt.SuperDistributorID == UserID)
            {
                return true;
            }
            return false;
        }

        private bool isValidaUserForReceipt(Project.Response.BusBooking.PrintResponse receipt, int UserID)
        {
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            if (Util.isRoleAdminOrSupport(UserTypeID))
            {
                return true;
            }

            if (receipt.UserID == UserID)
            {
                return true;
            }

            if (receipt.DistributorID == UserID)
            {
                return true;
            }
            if (receipt.SuperDistributorID == UserID)
            {
                return true;
            }
            return false;
        }

        internal RechargeReceiptViewModel SelectForReceipt(SelectByIDRequest mainRequest, KeyDataModel keydata, out string failureMessage)
        {
            RechargeReceiptViewModel response = new RechargeReceiptViewModel();
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return response;

            try
            {
                RechargeReceiptViewModel receipt = new TransactionDAL().SelectDetailByID(mainRequest.ID);

                string CompanyName = new SettingMasterBal().SelectByKeyWithoutDefault(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.ClientName));

                if (!isValidaUserForReceipt(receipt, keydata.UserID))
                {
                    failureMessage = Messages.InvalidUserInput;
                    return response;
                }

                UserMaster user = new UserDAL().SelectByIDFromCache(keydata.UserID);

                string IsRetailerNumber = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.RetailerNumberInReceipt), "No");

                response.AEPSResponseJSON = receipt.AEPSResponseJSON;



                response.IsBBPS = receipt.IsBBPS;
                response.FirmName = receipt.FirmName;
                response.FirmName = receipt.FirmName.ConcatNameAliasName(user.AliasName);
                if (IsRetailerNumber.EqualsIgnoreCase("Yes"))
                    response.MobileNumber = receipt.MobileNumber;
                response.FirmAddress = receipt.FirmAddress;
                response.CompanyName = CompanyName;
                response.DOC = receipt.DOC;
                response.ConsumerNumber = receipt.ConsumerNumber;
                response.Operator = receipt.Operator;
                response.OperatorImagePath = receipt.OperatorImagePath;
                response.Amount = Convert.ToInt32(receipt.AmountDecimal).ToString();
                response.ReferenceNo = receipt.ReferenceNo;
                response.ProviderRequestID = receipt.ProviderRequestID;
                response.Status = receipt.Status;
                response.ServiceID = receipt.ServiceID;
                response.DMT_BeneficiaryName = receipt.DMT_BeneficiaryName ?? "";
                response.BBPS_CustomerName = receipt.BBPS_CustomerName ?? "";
                response.ProviderRefNo = receipt.ProviderRefNo;
                response.ServiceName = receipt.ServiceName;
                response.BillerID = receipt.BillerID;
                response.BillDate = receipt.BillDate;
                response.BankName = receipt.BankName;
                response.AgentID = receipt.AgentID;
                response.STANNo = receipt.STANNo;
                response.DebitAmount = receipt.DebitAmount;
                response.RetailerGST = receipt.RetailerGST;
                response.ChargeAmount = receipt.ChargeAmount;
                response.IFSC =receipt.IFSC;
                if (response.ServiceID == (int)ServiceCode.Payout)
                {
                    response.ChargeAmount = Math.Round(response.DebitAmount - response.AmountDecimal ,2);
                    response.ChargeAmount = Math.Round(response.ChargeAmount - response.RetailerGST ,2);
                }
            }

            catch (Exception ex)
            {
                failureMessage = Messages.ExceptionOccured;
                new Logger().Write(ex);
                return response;
            }

            if (response == null)
            {
                failureMessage = Messages.DataNotFound;
                return response;
            }

            failureMessage = StatusType.Ok;
            return response;
        }

        internal Project.Response.BusBooking.PrintResponse SelectForBusPrint(SelectByIDRequest mainRequest, KeyDataModel keydata, out string failureMessage)
        {
            Project.Response.BusBooking.PrintResponse response = new Project.Response.BusBooking.PrintResponse();
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return response;

            try
            {
                Project.Response.BusBooking.PrintResponse receipt = new TransactionDAL().SelectBus(mainRequest.ID);

                string CompanyName = new SettingMasterBal().SelectByKeyWithoutDefault(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.ClientName));

                if (!isValidaUserForReceipt(receipt, keydata.UserID))
                {
                    failureMessage = Messages.InvalidUserInput;
                    return response;
                }
                response.AEPSResponseJSON = receipt.AEPSResponseJSON;

                if (response.AEPSResponseJSON.Length <= 10)
                {
                    failureMessage = "No data found";
                    return response;
                }

                Response.BusBooking.BusTicketBookingConfirmationCallback resp = new Response.BusBooking.BusTicketBookingConfirmationCallback();

                resp = JsonConvert.DeserializeObject<Response.BusBooking.BusTicketBookingConfirmationCallback>(response.AEPSResponseJSON);
                response.AEPSResponseJSON = resp.eventName;

                response.BusType = resp.param?.seat_details?.busType ?? "";
                response.pickupLocation = resp.param?.seat_details?.pickupLocation;
                response.dropLocation = resp.param?.seat_details?.dropLocation;
                response.refid = resp.param?.refid;
                response.pickupTime = resp.param?.seat_details?.pickupTime;
                response.dropTime = resp.param?.seat_details?.dropTime;
                response.customer_mobile = resp.param?.customer_mobile;
                response.Status = resp.param?.seat_details?.status;
                response.seatName = resp.param?.seat_details?.inventoryItems[0]?.seatName;
                response.address = resp.param?.seat_details?.inventoryItems[0]?.passenger.address;
                response.age = resp.param?.seat_details?.inventoryItems[0]?.passenger.age;
                response.name = resp.param?.seat_details?.inventoryItems[0]?.passenger.name;
                response.email = resp.param?.seat_details?.inventoryItems[0]?.passenger.email;
                response.mobile = resp.param?.seat_details?.inventoryItems[0]?.passenger.mobile;
                response.gender = resp.param?.seat_details?.inventoryItems[0]?.passenger.gender;
                response.amount = resp.param?.amount;
                response.base_fare = resp.param?.base_fare;
                response.comm = resp.param?.comm;
                response.tds = resp.param?.tds;
                response.total_deduction = resp.param?.total_deduction;
            }
            catch (Exception ex)
            {
                failureMessage = Messages.ExceptionOccured;
                new Logger().Write(ex);
                return response;
            }

            if (response == null)
            {
                failureMessage = Messages.DataNotFound;
                return response;
            }

            failureMessage = StatusType.Ok;
            return response;
        }

        internal List<SearchViewModel> SelectByConsumerNumber(SearchRequest searchRequest, int UserID, out string failureMessage)
        {
            failureMessage = searchRequest.IsValidModalState();

            List<SearchViewModel> searchViewModel = new List<SearchViewModel>();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return searchViewModel;

            try
            {
                searchViewModel = new TransactionDAL().SelectByConsumerNumber(searchRequest, UserID);
            }
            catch (Exception ex)
            {
                failureMessage = Messages.ExceptionOccured;
                new Logger().Write(ex);
                return searchViewModel;
            }
            failureMessage = Messages.DataNotFound;
            if (searchViewModel == null || searchViewModel.Count <= 0) return searchViewModel;

            failureMessage = StatusType.Ok;
            return searchViewModel;
        }

        internal List<ChangeStatusViewModel> SelectChangeStatusByCallback(SearchRequest searchRequest, int UserID, out string failureMessage)
        {
            failureMessage = searchRequest.IsValidModalState();

            List<ChangeStatusViewModel> changeStatusViewModel = new List<ChangeStatusViewModel>();

            try
            {
                changeStatusViewModel = new TransactionDAL().SelectChangeStatusByCallback(searchRequest, UserID);
            }
            catch (Exception ex)
            {
                failureMessage = Messages.ExceptionOccured;
                new Logger().Write(ex);
                return changeStatusViewModel;
            }
            failureMessage = Messages.DataNotFound;
            if (changeStatusViewModel.Count <= 0) return changeStatusViewModel;

            failureMessage = StatusType.Ok;
            return changeStatusViewModel;
        }

        private string GetDocType(int walletID)
        {
            WalletMaster wallet = new WalletBal().SelectByID(walletID);
            return wallet.Name;
        }

        internal string UpdateStatusManually(UpdateManualRequest updateRequest, int UserID)
        {
            string failureMessage = updateRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;

            TransactionMaster transactionMaster = new TransactionDAL().SelectByID(updateRequest.TransactionMasterID);
            if (transactionMaster.ServiceID == (int)ServiceCode.AEPS)
                return TransactionMessage.NotUpdateStatus;
            int LoginUserTypeID = new UserDAL().SelectUserTypeID(UserID);

            if (!Util.isRoleAdminOrSupport(LoginUserTypeID))
                return TransactionMessage.InvalidUser;

            int UserTypeID = new UserDAL().SelectUserTypeID(transactionMaster.UserID);

            failureMessage = new UserDAL().IsValidUserPin(UserID, updateRequest.Pin);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;


            string docType = GetDocType(transactionMaster.WalletID);

            RechargeRequest mainrequest = new RechargeRequest();
            mainrequest.ConsumerNumber = transactionMaster.ConsumerNumber;
            mainrequest.Amount = transactionMaster.Amount;
            mainrequest.OperatorID = transactionMaster.OperatorID;

            if (new ResponseProcess().checkProcess(transactionMaster.Status))  // Original status process
            {
                if (new ResponseProcess().CheckSuccess(updateRequest.Status))// success
                {
                    if (updateRequest.ReferenceNo == null || updateRequest.ReferenceNo == "") return TransactionMessage.ValidTransectionID;

                    new TransactionManualUpdateBal().Insert(transactionMaster.TransactionMasterID, UserID, updateRequest.ReferenceNo, transactionMaster.Status, updateRequest.Status);

                    transactionMaster.Status = updateRequest.Status;
                    transactionMaster.ReferenceNo = updateRequest.ReferenceNo;

                    transactionMaster.Description = TransactionMessage.TransSucessDescription;

                    int rowsAfftected = new AccountCoreProcessing().SuccessTransaction(transactionMaster);
                    if (rowsAfftected <= 0)
                        return TransactionMessage.UpdateFail;
                }

                if (new ResponseProcess().CheckFailure(updateRequest.Status))// Failure
                {
                    new TransactionManualUpdateBal().Insert(transactionMaster.TransactionMasterID, UserID, updateRequest.ReferenceNo, transactionMaster.Status, StatusType.Failure);

                    transactionMaster.Status = StatusType.Failure;
                    transactionMaster.Description = TransactionMessage.TransFailDescription;
                    new AccountCoreProcessing().RefundTransaction(transactionMaster, transactionMaster.Description, docType);

                }
                new FCMSelection().SendAPPBarRefreshFCM(Company.AdminUserID);

                new FCMSelection().SendTransactionMessage(transactionMaster.UserID, mainrequest, updateRequest.Status);
                ///Call back Send manual
                if (UserTypeID == (int)UserType.APIUser)
                {
                    TransactionMaster Model = new TransactionDAL().SelectByID(updateRequest.TransactionMasterID);
                    string CallbackURL = new CallbackMasterDal().SelectCallbackURL(Model.UserID, Model.ServiceID);
                    Task.Run(() => CallBackPosting.postCallBackToAPIUser(Model, UserTypeID, CallbackURL));
                }
                return StatusType.Ok;
            }

            int Difference = 0;
            Difference = Convert.ToInt32((DateTime.Now - transactionMaster.DOC).TotalDays);

            if (Difference > 180)
                return TransactionMessage.InvalidUserToUpdateTransaction;

            if (new ResponseProcess().CheckSuccess(transactionMaster.Status))  // Original status Success
            {
                if (new ResponseProcess().CheckSuccess(updateRequest.Status))
                {
                    if (updateRequest.ReferenceNo == null || updateRequest.ReferenceNo == "") return TransactionMessage.ValidTransectionID;
                    new TransactionManualUpdateBal().Insert(transactionMaster.TransactionMasterID, UserID, updateRequest.ReferenceNo, transactionMaster.Status, updateRequest.Status);

                    transactionMaster.ReferenceNo = updateRequest.ReferenceNo;
                    transactionMaster.Description = TransactionMessage.TransSucessDescription;
                    int rowsAfftected = new AccountCoreProcessing().SuccessTransaction(transactionMaster);
                    if (rowsAfftected <= 0)
                        return TransactionMessage.UpdateFail;
                }
                if (new ResponseProcess().CheckFailure(updateRequest.Status))// Failure
                {
                    new TransactionManualUpdateBal().Insert(transactionMaster.TransactionMasterID, UserID, updateRequest.ReferenceNo, transactionMaster.Status, StatusType.Failure);
                    InsertTransactionStatusChange(transactionMaster, StatusType.Failure);
                    transactionMaster.Status = StatusType.Failure;

                    transactionMaster.Description = TransactionMessage.TransFailDescription;

                    new AccountCoreProcessing().RefundTransaction(transactionMaster, transactionMaster.Description, docType);
                }
                new FCMSelection().SendAPPBarRefreshFCM(Company.AdminUserID);
                new FCMSelection().SendTransactionMessage(transactionMaster.UserID, mainrequest, updateRequest.Status);
                ///Call back Send manual
                if (UserTypeID == (int)UserType.APIUser)
                {
                    TransactionMaster Model = new TransactionDAL().SelectByID(updateRequest.TransactionMasterID);
                    string CallbackURL = new CallbackMasterDal().SelectCallbackURL(Model.UserID, Model.ServiceID);
                    Task.Run(() => CallBackPosting.postCallBackToAPIUser(Model, UserTypeID, CallbackURL));
                }
                return StatusType.Ok;
            }

            if (new ResponseProcess().CheckFailure(transactionMaster.Status))  // Original status Fail
            {
                if (new ResponseProcess().CheckFailure(updateRequest.Status)) return TransactionMessage.AllreadyFail;

                if (new ResponseProcess().CheckSuccess(updateRequest.Status))// Success
                {
                    if (!IsValidBalance(transactionMaster.UserID, transactionMaster.WalletID, transactionMaster.Amount))
                        return $"Insufficient Balance for amount {transactionMaster.Amount.TwoDigitDecimal()}";
                    new TransactionManualUpdateBal().Insert(transactionMaster.TransactionMasterID, UserID, updateRequest.ReferenceNo, transactionMaster.Status, StatusType.Success);
                    InsertTransactionStatusChange(transactionMaster, StatusType.Success);
                    transactionMaster.Status = StatusType.Success;
                    transactionMaster.ReferenceNo = updateRequest.ReferenceNo;
                    transactionMaster.Description = TransactionMessage.TransSucessDescription;

                    new AccountCoreProcessing().DebitTransaction(transactionMaster, transactionMaster.Description, docType);

                    int rowsAfftected = new AccountCoreProcessing().SuccessTransaction(transactionMaster);
                    if (rowsAfftected <= 0)
                        return TransactionMessage.UpdateFail;
                }
                new FCMSelection().SendAPPBarRefreshFCM(Company.AdminUserID);
                new FCMSelection().SendTransactionMessage(transactionMaster.UserID, mainrequest, updateRequest.Status);
                ///Call back Send manual

                TransactionMaster Model = new TransactionDAL().SelectByID(updateRequest.TransactionMasterID);
                string CallbackURL = new CallbackMasterDal().SelectCallbackURL(Model.UserID, Model.ServiceID);
                Task.Run(() => CallBackPosting.postCallBackToAPIUser(Model, UserTypeID, CallbackURL));

                return StatusType.Ok;
            }

            return StatusType.Ok;
        }
        private bool IsValidBalance(int UserID, int WalletID, decimal Amount)
        {
            UserWalletMaster userWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(UserID, WalletID);
            if (userWallet == null)
                return false;
            if (userWallet.Balance < Amount)
                return false;
            return true;
        }

        internal string UpdateStatusManuallyPenidngProvider(UpdateManualRequest updateRequest, int UserID)
        {
            string failureMessage = updateRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;

            TransactionMaster transactionMaster = new TransactionDAL().SelectByID(updateRequest.TransactionMasterID);
            if (transactionMaster.ServiceID == (int)ServiceCode.AEPS)
                return TransactionMessage.NotUpdateStatus;
            int LoginUserTypeID = new UserDAL().SelectUserTypeID(UserID);

            if (!Util.isRoleAdminOrSupport(LoginUserTypeID))
                return TransactionMessage.InvalidUser;

            int UserTypeID = new UserDAL().SelectUserTypeID(transactionMaster.UserID);
            failureMessage = FunctionalValidationProcessing.IsValidMPin(UserID, updateRequest.Pin);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            string docType = GetDocType(transactionMaster.WalletID);

            RechargeRequest mainrequest = new RechargeRequest();
            mainrequest.ConsumerNumber = transactionMaster.ConsumerNumber;
            mainrequest.Amount = transactionMaster.Amount;
            mainrequest.OperatorID = transactionMaster.OperatorID;

            if (!new ResponseProcess().checkProcess(transactionMaster.PendingProviderStatus))  // Original status process
            {
                return TransactionMessage.UpdateFail;
            }
            if (new ResponseProcess().CheckSuccess(updateRequest.Status))// success
            {
                if (updateRequest.ReferenceNo == null || updateRequest.ReferenceNo == "") return TransactionMessage.ValidTransectionID;

                transactionMaster.PendingProviderStatus = updateRequest.Status;
                transactionMaster.ReferenceNo = updateRequest.ReferenceNo;
                transactionMaster.Description = TransactionMessage.TransSucessDescription;

                int rowsAfftected = new AccountCoreProcessing().SuccessTransaction(transactionMaster);
                if (rowsAfftected <= 0)
                    return TransactionMessage.UpdateFail;
            }

            if (new ResponseProcess().CheckFailure(updateRequest.Status))// Failure
            {
                transactionMaster.Status = StatusType.Failure;
                transactionMaster.Description = TransactionMessage.TransFailDescription;
                new TransactionBal().UpdatePendingProviderStatus(StatusType.Failure, transactionMaster.TransactionMasterID);
                new AccountCoreProcessing().RefundTransaction(transactionMaster, transactionMaster.Description, docType);
            }
            new FCMSelection().SendAPPBarRefreshFCM(Company.AdminUserID);

            new FCMSelection().SendTransactionMessage(transactionMaster.UserID, mainrequest, updateRequest.Status);
            ///Call back Send manual
            if (UserTypeID == (int)UserType.APIUser)
            {
                TransactionMaster Model = new TransactionDAL().SelectByID(updateRequest.TransactionMasterID);
                string CallbackURL = new CallbackMasterDal().SelectCallbackURL(Model.UserID, Model.ServiceID);
                Task.Run(() => CallBackPosting.postCallBackToAPIUser(Model, UserTypeID, CallbackURL));
            }
            return StatusType.Ok;
        }

        internal OperatorViewModel SelectOperatorByConsumerNumber(SelectOperatorByConsumerNumberRequest mainRequest, out string failureMessage)
        {
            OperatorViewModel response = new OperatorViewModel();
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return response;

            try
            {
                CircleCore.Response.CoreResponse CircleResponse = null;

                CircleResponse = new SequenceSelection().GetCircleCode(mainRequest.ConsumerNumber);
                if (CircleResponse == null || CircleResponse.Message == null)
                {
                    CircleResponse.Message = "Cirecle getting but message is null";
                }

                if (CircleResponse.Message.ToLower().EqualsIgnoreCase("Circle API url not set".ToLower())
                    || CircleResponse.Message.ToLower().EqualsIgnoreCase("Invalid response from circle api".ToLower()))
                {
                    failureMessage = CircleResponse.Message;
                    return response;
                }

                if (CircleResponse.Operator.EqualsIgnoreCase("") || CircleResponse.Operator.Length <= 0)
                {
                    failureMessage = Messages.DataNotFound;
                    return response;
                }

                Response.Operator.ViewModel model = new Response.Operator.ViewModel();
                List<Response.Operator.ViewModel> operators = new List<Response.Operator.ViewModel>();
                try
                {
                    operators = new OperatorMasterDAL().SelectActiveOperatorOnly();
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }

                if (CircleResponse.Operator.EqualsIgnoreCase("Reliance Jio Infocomm Limited"))
                {
                    CircleResponse.Operator = "Jio";
                }

                if (CircleResponse.Operator.EqualsIgnoreCase("BSNL GSM"))
                {
                    CircleResponse.Operator = "BSNL";
                }

                model = operators.Where(o => o.OperatorName.ToLower().Contains(CircleResponse.Operator.ToLower())).FirstOrDefault();
                if (model == null)
                {
                    response.OperatorName = CircleResponse.Operator;
                    response.OperatorID = CircleResponse.OperatorID;
                    return response;
                }

                response.Circle = CircleResponse.Circle;
                response.OperatorName = model.OperatorName;
                response.OperatorID = model.OperatorID;
            }
            catch (Exception ex)
            {
                failureMessage = Messages.ExceptionOccured;
                new Logger().Write(ex);
                return response;
            }

            if (response == null)
            {
                failureMessage = Messages.DataNotFound;
                return new OperatorViewModel();
            }

            failureMessage = StatusType.Ok;
            return response;
        }

        internal List<ResendDMTModel> SelectResendDMTTransaction(KeyDataModel keyDataModel, out string failureMessage)
        {
            List<ResendDMTModel> resendModel = new List<ResendDMTModel>();
            try
            {
                resendModel = new TransactionDAL().SelectResendDMTTransaction();
            }
            catch (Exception ex)
            {
                failureMessage = Messages.ExceptionOccured;
                new Logger().Write(ex);
                return resendModel;
            }

            failureMessage = StatusType.Ok;
            return resendModel;
        }
        internal UserWiseDMTRangeWrapper SelectUserWiseDMTRangeReport(UserWiseDMTRangeReportRequest mainRequest, KeyDataModel keyDataModel, out string failureMessage)
        {
            failureMessage = mainRequest.IsValidModalState();
            isExcelOrPDF(mainRequest);
            UserWiseDMTRangeWrapper Wrapper = new UserWiseDMTRangeWrapper();
            List<UserWiseDMTRangeViewModel> list = new TransactionDAL().SelectUserWiseDMTRangeReport(mainRequest);

            if (list == null || list.Count < 0)
            {
                failureMessage = Message.TransactionMessage.NotFound;
                return Wrapper;
            }

            UserWiseDMTRangeViewModel rangewise = new UserWiseDMTRangeViewModel();
            int TotalRange1_2 = 0;
            int TotalRange3_1000 = 0;
            int TotalRange1001_2000 = 0;
            int TotalRange2001_3000 = 0;
            int TotalRange3001_4000 = 0;
            int TotalRange4001_5000 = 0;
            int TotalRange5001_10000 = 0;
            int TotalRange10001_15000 = 0;
            int TotalRange15001_20000 = 0;
            int TotalRange20001_25000 = 0;
            int TotalRange25001_50000 = 0;
            int TotalRange50001_1L = 0;
            int TotalTotalTransactions = 0;

            foreach (UserWiseDMTRangeViewModel item in list)
            {
                TotalRange1_2 += item.Range1_2;
                TotalRange3_1000 += item.Range3_1000;
                TotalRange1001_2000 += item.Range1001_2000;
                TotalRange2001_3000 += item.Range2001_3000;
                TotalRange3001_4000 += item.Range3001_4000;
                TotalRange4001_5000 += item.Range4001_5000;
                TotalRange5001_10000 += item.Range5001_10000;
                TotalRange10001_15000 += item.Range10001_15000;
                TotalRange15001_20000 += item.Range15001_20000;
                TotalRange20001_25000 += item.Range20001_25000;
                TotalRange25001_50000 += item.Range25001_50000;
                TotalRange50001_1L += item.Range50001_1L;
                TotalTotalTransactions += item.TotalTransactions;
            }
            rangewise.UserName = "Total";
            rangewise.Range1_2 = TotalRange1_2;
            rangewise.Range3_1000 = TotalRange3_1000;
            rangewise.Range1001_2000 = TotalRange1001_2000;
            rangewise.Range2001_3000 = TotalRange2001_3000;
            rangewise.Range3001_4000 = TotalRange3001_4000;
            rangewise.Range4001_5000 = TotalRange4001_5000;
            rangewise.Range5001_10000 = TotalRange5001_10000;
            rangewise.Range10001_15000 = TotalRange10001_15000;
            rangewise.Range15001_20000 = TotalRange15001_20000;
            rangewise.Range20001_25000 = TotalRange20001_25000;
            rangewise.Range25001_50000 = TotalRange25001_50000;
            rangewise.Range50001_1L = TotalRange50001_1L;
            rangewise.TotalTransactions = TotalTotalTransactions;


            list.Add(rangewise);



            if (mainRequest.IsPDFDownload)
            {
                string FileName = "UserwiseDMTrangewise_Report";

                Wrapper.ReportPath = new ExcelProcessing().GeneratePdf(FileName, list, keyDataModel.UserID);

            }
            if (mainRequest.IsExcelDownload)
            {
                string FileName = "UserwiseDMTrangewise_Report";
                Wrapper.ReportPath = new ExcelProcessing().GenerateExcel(FileName, list, keyDataModel.UserID);
            }
            if (Util.IsDownload(mainRequest.IsExcelDownload, mainRequest.IsPDFDownload))
            {
                Wrapper.IsDownload = true;
            }
            Wrapper.list = list;
            failureMessage = StaticModel.StatusType.Ok;
            return Wrapper;
        }


        internal RangewiseDMTWrapper SelectRangewiseDMT(RangewiseDMTRequest mainRequest, KeyDataModel keyDataModel, out string failureMessage)
        {
            RangewiseDMTWrapper wrapper = new RangewiseDMTWrapper();

            failureMessage = mainRequest.IsValidModalState();
            isExcelOrPDF(mainRequest);
            List<RangewiseDMTModel> rangeModel = new List<RangewiseDMTModel>();
            RangewiseDMTModel model = new RangewiseDMTModel();
            try
            {
                string[] Amount;
                if (mainRequest.Range.EqualsIgnoreCase("ALL"))
                {
                    mainRequest.Range = "1-1,2-1000,1001-2000,2001-3000,3001-4000,4001-5000,5001-100000";
                    string[] Range = mainRequest.Range.Split(",", 7);
                    int i = 0;
                    foreach (string str in Range)
                    {
                        Amount = str.Split("-", 2);
                        if (Amount.Length <= 1)
                        {
                            Amount = new string[2];
                            Amount[0] = "0";
                            Amount[1] = "1000000";
                        }
                        mainRequest.Range = Range[i];
                        model = new TransactionDAL().SelectRangewiseDMT(mainRequest, Convert.ToDecimal(Amount[0]), Convert.ToDecimal(Amount[1]));
                        rangeModel.Add(model);
                        i++;
                    }
                    if (mainRequest.IsPDFDownload)
                    {
                        string FileName = "DMTRangewise_Report";

                        model.ReportPath = new ExcelProcessing().GeneratePdf(FileName, rangeModel, keyDataModel.UserID);
                        rangeModel.Add(model);
                    }
                    if (mainRequest.IsExcelDownload)
                    {
                        string FileName = "DMTRangewise_Report";
                        model.ReportPath = new ExcelProcessing().GenerateExcel(FileName, rangeModel, keyDataModel.UserID);
                        rangeModel.Add(model);
                    }
                }
                else
                {
                    Amount = mainRequest.Range.Split("-", 2);
                    if (Amount.Length <= 1)
                    {
                        Amount = new string[2];
                        Amount[0] = "0";
                        Amount[1] = "1000000";
                    }
                    model = new TransactionDAL().SelectRangewiseDMT(mainRequest, Convert.ToDecimal(Amount[0]), Convert.ToDecimal(Amount[1]));
                    if (mainRequest.IsPDFDownload)
                    {
                        string FileName = "DMTRangewise_Report";

                        model.ReportPath = new ExcelProcessing().GeneratePdf(FileName, rangeModel, keyDataModel.UserID);
                        rangeModel.Add(model);
                    }
                    if (mainRequest.IsExcelDownload)
                    {
                        string FileName = "DMTRangewise_Report";
                        model.ReportPath = new ExcelProcessing().GenerateExcel(FileName, rangeModel, keyDataModel.UserID);
                    }
                    rangeModel.Add(model);
                }

                RangewiseDMTModel rangewise = new RangewiseDMTModel();
                int TotalCount = 0;
                decimal TotalSum = 0;
                int TotalFailCount = 0;
                decimal TotalFailureSum = 0;
                int TotalSuccessCount = 0;
                decimal TotalSuccessSum = 0;
                int TotalProcessCount = 0;
                decimal TotalProcessSum = 0;
                foreach (RangewiseDMTModel item in rangeModel)
                {
                    TotalCount += item.Count;
                    TotalSum += item.Sum;
                    TotalFailCount += item.FailCount;
                    TotalFailureSum += item.FailSum;
                    TotalSuccessCount += item.SuccessCount;
                    TotalSuccessSum += item.SuccessSum;
                    TotalProcessCount += item.ProcessCount;
                    TotalProcessSum += item.ProcessSum;
                }
                rangewise.Range = "Total";
                rangewise.Count = TotalCount;
                rangewise.Sum = TotalSum;
                rangewise.FailCount = TotalFailCount;
                rangewise.FailSum = TotalFailureSum;
                rangewise.SuccessCount = TotalSuccessCount;
                rangewise.SuccessSum = TotalSuccessSum;
                //   rangewise.FailureSum = TotalFailureSum;
                rangewise.ProcessCount = TotalProcessCount;
                rangewise.ProcessSum = TotalProcessSum;

                rangeModel.Add(rangewise);
                if (Util.IsDownload(mainRequest.IsExcelDownload, mainRequest.IsPDFDownload))
                {
                    wrapper.IsDownload = true;
                }

                wrapper.list = rangeModel;
            }
            catch (Exception ex)
            {
                failureMessage = Messages.ExceptionOccured;
                new Logger().Write(ex);
                return wrapper;
            }

            failureMessage = StatusType.Ok;
            return wrapper;
        }

        internal static bool isExcelOrPDF(Requests.TransactionReport.RangewiseDMTRequest request)
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
        internal RangewiseRechargeWrapper SelectRangewiseRecharge(RangewiseRechargeRequest mainRequest, KeyDataModel keyDataModel, out string failureMessage)
        {
            RangewiseRechargeWrapper wrapper = new RangewiseRechargeWrapper();

            failureMessage = mainRequest.IsValidModalState();
            isExcelOrPDF(mainRequest);
            List<RangewiseRechargeModel> rangeModel = new List<RangewiseRechargeModel>();
            RangewiseRechargeModel model = new RangewiseRechargeModel();
            try
            {
                string[] Amount;
                if (mainRequest.MinRange == 0 && mainRequest.MaxRange == 0)
                {
                    mainRequest.Range = "10-50,51-100,101-200,201-300,301-500,501-700,701-1000,1001-2000,2001-10000";
                    string[] Range = mainRequest.Range.Split(",", 20);
                    int i = 0;
                    foreach (string str in Range)
                    {
                        Amount = str.Split("-", 2);
                        if (Amount.Length <= 1)
                        {
                            Amount = new string[2];
                            Amount[0] = "0";
                            Amount[1] = "1000000";
                        }
                        mainRequest.Range = Range[i];

                        decimal Amount1 = Convert.ToDecimal(Amount[0]);
                        decimal Amount2 = Convert.ToDecimal(Amount[1]);

                        model = new TransactionDAL().SelectRangewiseRecharge(mainRequest, Amount1, Amount2, mainRequest.MinRange, mainRequest.MaxRange);
                        rangeModel.Add(model);
                        i++;
                    }
                    if (mainRequest.IsPDFDownload)
                    {
                        string FileName = "RechargeRangewise_Report";

                        model.ReportPath = new ExcelProcessing().GeneratePdf(FileName, rangeModel, keyDataModel.UserID);
                        rangeModel.Add(model);
                    }
                    if (mainRequest.IsExcelDownload)
                    {
                        string FileName = "RechargeRangewise_Report";
                        model.ReportPath = new ExcelProcessing().GenerateExcel(FileName, rangeModel, keyDataModel.UserID);
                        rangeModel.Add(model);
                    }
                }
                else
                {
                    Amount = mainRequest.Range.Split("-", 2);
                    if (Amount.Length <= 1)
                    {
                        Amount = new string[2];
                        Amount[0] = mainRequest.MinRange.ToString();
                        Amount[1] = mainRequest.MaxRange.ToString();

                    }
                    model = new TransactionDAL().SelectRangewiseRecharge(mainRequest, Convert.ToDecimal(Amount[0]), Convert.ToDecimal(Amount[1]), mainRequest.MinRange, mainRequest.MaxRange);
                    if (mainRequest.IsPDFDownload)
                    {
                        string FileName = "RechargeRangewise_Report";

                        model.ReportPath = new ExcelProcessing().GeneratePdf(FileName, rangeModel, keyDataModel.UserID);
                        rangeModel.Add(model);
                    }
                    if (mainRequest.IsExcelDownload)
                    {
                        string FileName = "RechargeRangewise_Report";
                        model.ReportPath = new ExcelProcessing().GenerateExcel(FileName, rangeModel, keyDataModel.UserID);
                    }
                    rangeModel.Add(model);
                }

                RangewiseRechargeModel rangewise = new RangewiseRechargeModel();
                int TotalCount = 0;
                decimal TotalSum = 0;
                int TotalFailCount = 0;
                decimal TotalFailSum = 0;
                int TotalSuccessCount = 0;
                decimal TotalSuccessSum = 0;
                int TotalProcessCount = 0;
                decimal TotalProcessSum = 0;
                foreach (RangewiseRechargeModel item in rangeModel)
                {
                    TotalCount += item.Count;
                    TotalSum += item.Sum;
                    TotalFailCount += item.FailCount;
                    TotalFailSum += item.FailSum;
                    TotalSuccessCount += item.SuccessCount;
                    TotalSuccessSum += item.SuccessSum;
                    TotalProcessCount += item.ProcessCount;
                    TotalProcessSum += item.ProcessSum;
                }
                rangewise.Range = "Total";
                rangewise.Count = TotalCount;
                rangewise.Sum = TotalSum;
                rangewise.FailCount = TotalFailCount;
                rangewise.FailSum = TotalFailSum;
                rangewise.SuccessCount = TotalSuccessCount;
                rangewise.SuccessSum = TotalSuccessSum;
                rangewise.ProcessCount = TotalProcessCount;
                rangewise.ProcessSum = TotalProcessSum;
                rangeModel.Add(rangewise);

                if (Util.IsDownload(mainRequest.IsExcelDownload, mainRequest.IsPDFDownload))
                {
                    wrapper.IsDownload = true;
                }
                wrapper.list = rangeModel;
            }
            catch (Exception ex)
            {
                failureMessage = Messages.ExceptionOccured;
                new Logger().Write(ex);
                return wrapper;
            }

            failureMessage = StatusType.Ok;
            return wrapper;
        }

        internal static bool isExcelOrPDF(Requests.TransactionReport.RangewiseRechargeRequest request)
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
        internal SelectByIDResponse SelectByID(int TransactionMasterID)
        {
            SelectByIDResponse Model = new SelectByIDResponse();

            try
            {
                TransactionMaster transactionMaster = new TransactionDAL().SelectByID(TransactionMasterID);

                if (transactionMaster == null) return Model;

                Model.Status = transactionMaster.Status;
                Model.TransactionMasterID = transactionMaster.TransactionMasterID;
                Model.ConsumerNumber = transactionMaster.ConsumerNumber;
                Model.Amount = transactionMaster.Amount;
                Model.DateStr = transactionMaster.DOC.LongFormatDate();
                UserMaster user = new UserDAL().SelectByIDFromCache(transactionMaster.UserID);
                Model.FirmName = user.FirmName.ConcatNameMobileAliasName(user.MobileNumber, user.AliasName);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Model;
            }

            return Model;
        }

        internal PendingResponseWrapper SelectPendingTransaction()
        {
            PendingResponseWrapper wrapper = new PendingResponseWrapper();
            List<PendingResponse> list = new List<PendingResponse>();
            try
            {
                list = new TransactionDAL().SelectPendingTransaction();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            wrapper.HeaderSummary = new TransactionDAL().SelectPendingHeaderSummary();
            wrapper.list = list;
            return wrapper;
        }

        internal PendingResponseWrapper SelectPendingProviderTransaction(SelectPendingTransactionWithFilterRequest mainRequest)
        {
            PendingResponseWrapper wrapper = new PendingResponseWrapper();
            List<PendingResponse> list = new List<PendingResponse>();
            try
            {
                list = new TransactionDAL().SelectPendingProviderTransaction(mainRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (list == null || list.Count <= 0) return wrapper;
            if (mainRequest.ConsumerNumber != "")
                list = list.AsQueryable().WhereDynamic(mainRequest.ConsumerNumber).ToList();

            wrapper.HeaderSummary = new TransactionDAL().SelectPendingProviderHeaderSummary(list);
            wrapper.list = list;
            wrapper.IsExcelDownload = mainRequest.IsExcelDownload;
            return wrapper;
        }

        internal PendingResponseWrapper SelectPendingTransactionWithFilter(SelectPendingTransactionWithFilterRequest mainRequest, out string failureMessage)
        {
            PendingResponseWrapper wrapper = new PendingResponseWrapper();
            List<PendingResponse> list = new List<PendingResponse>();
            failureMessage = StatusType.Ok;
            if (mainRequest.ProviderID == 0 && mainRequest.OperatorID == 0 && mainRequest.ConsumerNumber == "")
            {
                list = new TransactionDAL().SelectPendingTransaction();
                failureMessage = StatusType.Ok;
                wrapper.HeaderSummary = new TransactionDAL().SelectPendingHeaderSummaryWithFilter(mainRequest);
                wrapper.list = list;
                return wrapper;
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;

            if (mainRequest.ProviderID == 0 && mainRequest.OperatorID == 0 && mainRequest.ConsumerNumber != "")
            {
                list = new TransactionDAL().SelectPendingTransaction();
                list = list.AsQueryable().WhereDynamic(mainRequest.ConsumerNumber).ToList();
                failureMessage = StatusType.Ok;
                wrapper.HeaderSummary = new TransactionDAL().SelectPendingHeaderSummaryWithFilter(mainRequest);
                wrapper.list = list;
                return wrapper;
            }
            try
            {
                list = new TransactionDAL().SelectPendingTransactionWithFilter(mainRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (list == null || list.Count <= 0)
            {
                failureMessage = TransactionMessage.InvalidTransaction;
                return wrapper;
            }
            if (mainRequest.ConsumerNumber != "")
                list = list.AsQueryable().WhereDynamic(mainRequest.ConsumerNumber).ToList();

            wrapper.HeaderSummary = new TransactionDAL().SelectPendingHeaderSummaryWithFilter(mainRequest);
            wrapper.list = list;
            failureMessage = StatusType.Ok;
            return wrapper;
        }

        internal PendingResponseWrapper SelectDMTPendingTransactionWithFilter(SelectPendingTransactionWithFilterRequest mainRequest, out string failureMessage)
        {
            PendingResponseWrapper wrapper = new PendingResponseWrapper();
            List<PendingResponse> list = new List<PendingResponse>();
            failureMessage = StatusType.Ok;

            try
            {
                list = new TransactionDAL().SelectDMTPendingTransactionWithFilter(mainRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (list == null || list.Count <= 0)
            {
                failureMessage = TransactionMessage.InvalidTransaction;
                return wrapper;
            }
            if (mainRequest.ConsumerNumber != "")
                list = list.AsQueryable().WhereDynamic(mainRequest.ConsumerNumber).ToList();

            if (mainRequest.SenderNo != "")
                list = list.AsQueryable().WhereDynamic(mainRequest.SenderNo).ToList();

            if (mainRequest.PaymentType != "")
                list = list.AsQueryable().WhereDynamic(mainRequest.PaymentType).ToList();

            wrapper.HeaderSummary = new TransactionDAL().SelectDMTPendingHeaderSummaryWithFilter(mainRequest);
            wrapper.list = list;
            failureMessage = StatusType.Ok;
            return wrapper;
        }

        internal PendingResponseWrapper SelectDMTPending()
        {
            PendingResponseWrapper wrapper = new PendingResponseWrapper();
            List<PendingResponse> list = new List<PendingResponse>();
            try
            {
                list = new TransactionDAL().SelectDmtPendingTransaction();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            wrapper.HeaderSummary = new TransactionDAL().SelectDMTPendingHeaderSummary();
            wrapper.list = list;
            return wrapper;
        }

        internal PendingResponseWrapper SelectUPIPendingTransactionWithFilter(SelectPendingTransactionWithFilterRequest mainRequest, out string failureMessage)
        {
            PendingResponseWrapper wrapper = new PendingResponseWrapper();
            List<PendingResponse> list = new List<PendingResponse>();
            failureMessage = StatusType.Ok;

            try
            {
                list = new TransactionDAL().SelectUPIPendingTransactionWithFilter(mainRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (list == null || list.Count <= 0)
            {
                failureMessage = TransactionMessage.InvalidTransaction;
                return wrapper;
            }
            if (mainRequest.ConsumerNumber != "")
                list = list.AsQueryable().WhereDynamic(mainRequest.ConsumerNumber).ToList();

            if (mainRequest.PaymentType != "")
                list = list.AsQueryable().WhereDynamic(mainRequest.PaymentType).ToList();

            wrapper.HeaderSummary = new TransactionDAL().SelectUPIPendingHeaderSummaryWithFilter(mainRequest);
            wrapper.list = list;
            failureMessage = StatusType.Ok;
            return wrapper;
        }

        internal PendingResponseWrapper SelectUPIPending()
        {
            PendingResponseWrapper wrapper = new PendingResponseWrapper();
            List<PendingResponse> list = new List<PendingResponse>();
            try
            {
                list = new TransactionDAL().SelectUPIPendingTransaction();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            wrapper.HeaderSummary = new TransactionDAL().SelectUPIPendingHeaderSummary();
            wrapper.list = list;
            return wrapper;
        }

        internal PendingResponseWrapper SelectCMSOfflineDMT(CMSOfflineDMTRequest mainrequest, KeyDataModel keyDataModel, out string failureMessage)
        {
            failureMessage = DMTSettingMessages.NotFound;
            PendingResponseWrapper wrapper = new PendingResponseWrapper();
            List<PendingResponse> list = new List<PendingResponse>();
            try
            {
                list = new TransactionDAL().SelectCMSOfflineDMT(mainrequest);
                if (list == null)
                {
                    failureMessage = Messages.DataNotFound;
                    return wrapper;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            wrapper.HeaderSummary = new TransactionDAL().SelectCMSOfflineDMTHeaderSummary(mainrequest);
            wrapper.list = list;

            failureMessage = StatusType.Ok;
            return wrapper;
        }

        internal string ResendTransaction(ResendRequest mainRequest, int UserID)
        {
            string failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            try
            {
                TransactionMaster transactionMaster = new TransactionDAL().SelectByID(mainRequest.TransactionMasterID);
                if (transactionMaster == null) return TransactionMessage.InvalidID;

                int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

                if (!Util.isRoleAdminOrSupport(UserTypeID))
                    return TransactionMessage.InvalidUser;

                string result = new ResendProcessing().ResendRecharge(transactionMaster, mainRequest.IsStatusCheck);
                failureMessage = result;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            return failureMessage;
        }

        internal string ShiftTransaction(ShiftRequest mainRequest, int UserID)
        {
            string failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            try
            {
                TransactionMaster transactionMaster = new TransactionDAL().SelectByID(mainRequest.TransactionMasterID);
                if (transactionMaster == null)

                    return TransactionMessage.InvalidID;

                int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

                if (!Util.isRoleAdminOrSupport(UserTypeID))
                    return TransactionMessage.InvalidUser;

                string result = new ShiftProcessing().Shift(transactionMaster, mainRequest);
                failureMessage = result;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            return failureMessage;
        }

        internal string ShiftAllTransaction(List<ShiftRequest> mainRequest, int userID)
        {
            string failureMessge = TransactionMessage.TransactionNotFound;
            if (mainRequest == null || mainRequest.Count <= 0) return failureMessge;
            foreach (ShiftRequest request in mainRequest) failureMessge = ShiftTransaction(request, userID);

            return failureMessge;
        }

        internal List<CallbackResponse> TransactionSelectForAPIUserCallback()
        {
            List<CallbackResponse> list = null;
            try
            {
                list = new TransactionDAL().TransactionSelectForAPIUserCallback();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return list;
        }

        internal void UpdateHitBack(int TransactionMasterID)
        {
            Task.Run(() => UpdateHitBackCore(TransactionMasterID));
        }
        internal void UpdateHitBackCore(int TransactionMasterID)
        {
            TransactionMaster ObjModel = null;
            try
            {
                ObjModel = new TransactionDAL().SelectByID(TransactionMasterID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return;
            }

            if (ObjModel == null)
                return;

            ObjModel.HitBackCount = ObjModel.HitBackCount + 1;
            ObjModel.UpdatedDOC = DateTime.Now;
            try
            {
                new TransactionDAL().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return;
        }

        internal string isValidForBulkStatusCheck(TransactionMaster transactionMaster)
        {
            if (!new ResponseProcess().checkProcess(transactionMaster.Status))// not in process
                return TransactionMessage.NotInProcess;

            if (new ResponseProcess().CheckFailure(transactionMaster.Status)) //already fail Refund
                return TransactionMessage.AllreadyFail;

            if (new ResponseProcess().CheckSuccess(transactionMaster.Status))
                return TransactionMessage.AllreadySucess;

            if (transactionMaster.ProviderID <= 0)
                return TransactionMessage.TransactionNotYetSend;

            DateTime rchargeDate = transactionMaster.DOC;
            DateTime currDate = DateTime.Now;
            double diffInSec = (currDate - rchargeDate).TotalSeconds;

            if (diffInSec <= 90)
            {
                return "Transaction should 90 seconds older";
            }

            if (!isValidForStatuscheckBySchedule(transactionMaster.TransactionMasterID, transactionMaster.ProviderID))
                return "Transaction should BySchedule";

            return StatusType.Ok;
        }

        private bool isValidForStatuscheckBySchedule(int TransactionMasterID, int ProviderID)
        {
            if (ProviderID != Company.InstantPayPayoutProviderID)
                return true;
            List<ProviderRequestResponseLog> list = new ProviderRequestResponseLogDal().SelectByTansactionMasterIDandProviderID(TransactionMasterID, ProviderID);
            if (list.Count <= 1)
                return true;

            DateTime currentDate = DateTime.Now;
            DateTime firastDate = list[0].DOC;
            DateTime secondDate = list[1].DOC;

            double diffInMin = Math.Abs((firastDate - secondDate).TotalMinutes);
            double netxhitMin = diffInMin * 5;
            if (netxhitMin <= 5)
            {
                netxhitMin = 15;
            }

            if (netxhitMin >= 240)
            {
                netxhitMin = 240;
            }

            DateTime expectedDate = firastDate.AddMinutes(netxhitMin);

            if (currentDate >= expectedDate)
                return true;

            return false;
        }

        private List<SuccessFailAllRequest> GetDistinctValue(List<SuccessFailAllRequest> transactionMasterIDList)
        {
            List<SuccessFailAllRequest> distinct =
                    transactionMasterIDList.GroupBy(item => item.ID).Select(g => g.FirstOrDefault()).ToList();
            return distinct;
        }

        internal string StatusCheckAll(List<SuccessFailAllRequest> transactionMasterIDList, int UserID)
        {
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            if (!Util.isRoleAdminOrSupport(UserTypeID))
                return TransactionMessage.InvalidUser;

            transactionMasterIDList = GetDistinctValue(transactionMasterIDList);

            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Math.Max(3 * Environment.ProcessorCount / 4, 1)
            };

            Parallel.ForEach(transactionMasterIDList.AsEnumerable(), parallelOptions, item =>
            {
                TransactionMaster transactionMaster = new TransactionDAL().SelectByID(item.ID);

                string faileureMessage = isValidForBulkStatusCheck(transactionMaster);
                if (!faileureMessage.EqualsIgnoreCase(StatusType.Ok))
                    return;

                TransctionStatusRequest mainRequest = new TransctionStatusRequest();
                mainRequest.TransactionMasterID = transactionMaster.TransactionMasterID;
                new StatusCheckProcessing().RechargeTransactionStatusCheck(mainRequest, UserID);
            }
            );
            return StatusType.Ok;
        }

        private string isValidForBulkUpdate(TransactionMaster transactionMaster)
        {
            if (!new ResponseProcess().checkProcess(transactionMaster.Status))// not in process
                return TransactionMessage.NotInProcess;

            if (new ResponseProcess().CheckFailure(transactionMaster.Status)) //already fail Refund
                return TransactionMessage.AllreadyFail;

            if (new ResponseProcess().CheckSuccess(transactionMaster.Status))
                return TransactionMessage.AllreadySucess;

            return StatusType.Ok;
        }

        private string isValidPendingAPIForBulkUpdate(TransactionMaster transactionMaster)
        {
            if (!new ResponseProcess().checkProcess(transactionMaster.PendingProviderStatus))// not in process
                return TransactionMessage.NotInProcess;

            if (new ResponseProcess().CheckFailure(transactionMaster.PendingProviderStatus)) //already fail Refund
                return TransactionMessage.AllreadyFail;

            if (new ResponseProcess().CheckSuccess(transactionMaster.PendingProviderStatus))
                return TransactionMessage.AllreadySucess;

            return StatusType.Ok;
        }

        internal string RefundAll(List<SuccessFailAllRequest> transactionMasterIDList, int UserID)
        {
            if (transactionMasterIDList == null || transactionMasterIDList.Count <= 0)
                return "Select atleast one transaction";

            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            if (!Util.isRoleAdminOrSupport(UserTypeID))
                return TransactionMessage.InvalidUser;

            transactionMasterIDList = GetDistinctValue(transactionMasterIDList);

            foreach (SuccessFailAllRequest item in transactionMasterIDList)
            {
                TransactionMaster transactionMaster = new TransactionDAL().SelectByID(item.ID);
                string docType = GetDocType(transactionMaster.WalletID);
                string faileureMessage = isValidForBulkUpdate(transactionMaster);

                if (!faileureMessage.EqualsIgnoreCase(StatusType.Ok))
                    continue;

                if (transactionMaster.ServiceID == (int)ServiceCode.MoneyTransfer)
                {
                    if (!transactionMaster.IsOnlineDMT)
                        continue;
                }

                transactionMaster.Status = StatusType.Failure;
                transactionMaster.Description = TransactionMessage.TransFailDescription;
                new AccountCoreProcessing().RefundTransaction(transactionMaster, transactionMaster.Description, docType);
            }
            return StatusType.Ok;
        }

        internal string RefundAllProviderPenidng(List<SuccessFailAllRequest> transactionMasterIDList, int UserID)
        {
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            if (!Util.isRoleAdminOrSupport(UserTypeID))
                return TransactionMessage.InvalidUser;

            transactionMasterIDList = GetDistinctValue(transactionMasterIDList);

            foreach (SuccessFailAllRequest item in transactionMasterIDList)
            {
                TransactionMaster transactionMaster = new TransactionDAL().SelectByID(item.ID);
                string docType = GetDocType(transactionMaster.WalletID);
                string faileureMessage = isValidPendingAPIForBulkUpdate(transactionMaster);

                if (!faileureMessage.EqualsIgnoreCase(StatusType.Ok))
                    continue;

                transactionMaster.Status = StatusType.Failure;
                transactionMaster.Description = TransactionMessage.TransFailDescription;
                new TransactionBal().UpdatePendingProviderStatus(StatusType.Failure, transactionMaster.TransactionMasterID);

                new AccountCoreProcessing().RefundTransaction(transactionMaster, transactionMaster.Description, docType);
            }
            return StatusType.Ok;
        }

        internal string SuccessAll(List<SuccessFailAllRequest> transactionMasterIDList, int UserID)
        {
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            if (!Util.isRoleAdminOrSupport(UserTypeID))

                return TransactionMessage.InvalidUser;

            transactionMasterIDList = GetDistinctValue(transactionMasterIDList);
            foreach (SuccessFailAllRequest item in transactionMasterIDList)
            {
                TransactionMaster transactionMaster = new TransactionDAL().SelectByID(item.ID);

                string faileureMessage = isValidForBulkUpdate(transactionMaster);
                if (!faileureMessage.EqualsIgnoreCase(StatusType.Ok))
                    continue;

                if (new ResponseProcess().CheckSuccess(StatusType.Success))// success
                {
                    transactionMaster.Status = StatusType.Success;
                    transactionMaster.ReferenceNo = new Util().GetRandomTransactionID();
                    transactionMaster.Description = TransactionMessage.TransSucessDescription;

                    new AccountCoreProcessing().SuccessTransaction(transactionMaster);
                }
            }
            return StatusType.Ok;
        }

        internal string SuccessAllProviderPenidng(List<SuccessFailAllRequest> transactionMasterIDList, int UserID)
        {
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            if (!Util.isRoleAdminOrSupport(UserTypeID))

                return TransactionMessage.InvalidUser;

            transactionMasterIDList = GetDistinctValue(transactionMasterIDList);
            foreach (SuccessFailAllRequest item in transactionMasterIDList)
            {
                TransactionMaster transactionMaster = new TransactionDAL().SelectByID(item.ID);

                string faileureMessage = isValidPendingAPIForBulkUpdate(transactionMaster);
                if (!faileureMessage.EqualsIgnoreCase(StatusType.Ok))
                    continue;
                transactionMaster.PendingProviderStatus = StatusType.Success;
                transactionMaster.Description = TransactionMessage.TransSucessDescription;

                new AccountCoreProcessing().SuccessTransaction(transactionMaster);
            }
            return StatusType.Ok;
        }

        internal string DMTTransactionRefund(SuccessFailAllRequest item, int UserID)
        {
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            if (UserTypeID != (int)UserType.Admin || UserTypeID != (int)UserType.Retailer)
                return TransactionMessage.InvalidUser;

            TransactionMaster transactionMaster = new TransactionDAL().SelectByID(item.ID);
            string docType = GetDocType(transactionMaster.WalletID);
            string faileureMessage = isValidForBulkUpdate(transactionMaster);

            if (!faileureMessage.EqualsIgnoreCase(StatusType.Ok))
                return faileureMessage;

            transactionMaster.Status = StatusType.Failure;
            transactionMaster.Description = TransactionMessage.TransFailDescription;
            new AccountCoreProcessing().RefundTransaction(transactionMaster, transactionMaster.Description, docType);

            return StatusType.Ok;
        }

        internal string UpdateConsumerNumber(int TransactionMasterID, string NewConsumerNumber)
        {
            TransactionMaster ObjModel = null;
            try
            {
                ObjModel = new TransactionDAL().SelectByID(TransactionMasterID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            if (ObjModel == null)
                return TransactionMessage.UpdateFail;

            ObjModel.ConsumerNumber = NewConsumerNumber;
            ObjModel.UpdatedDOC = DateTime.Now;
            try
            {
                int rowsAffected = new TransactionDAL().Update(ObjModel);

                if (rowsAffected <= 0)

                    return TransactionMessage.UpdateFail;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            return StatusType.Ok;
        }

        internal string GetDMTTransctionStatusCheckAll(List<SuccessFailAllRequest> transactionMasterIDList, int UserID)
        {
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            if (!Util.isRoleAdminOrSupport(UserTypeID))
                return TransactionMessage.InvalidUser;

            transactionMasterIDList = GetDistinctValue(transactionMasterIDList);

            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Math.Max(3 * Environment.ProcessorCount / 4, 1)
            };

            Parallel.ForEach(transactionMasterIDList.AsEnumerable(), parallelOptions, item =>
            {
                try
                {
                    TransactionMaster transactionMaster = new TransactionDAL().SelectByID(item.ID);
                    if (transactionMaster == null)
                    {
                        return;
                    }
                    string failureMessage = isValidForBulkStatusCheck(transactionMaster);
                    if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                        return;
                    DMTTransctionStatusRequest mainRequest = new DMTTransctionStatusRequest();
                    mainRequest.ID = item.ID.ToString();
                    new DMTProcessing().DMTTransactionStatusCheck(mainRequest, UserID);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }
            });
            return StatusType.Ok;
        }

        public void ResendXmpp()
        {
            try
            {
                if (Company.XMPPResend.EqualsIgnoreCase("No"))
                    return;

                if (Company.XMPPProviderID <= 0)
                    return;

                List<TransactionMaster> list = null;
                try
                {
                    list = new TransactionDAL().SelectPendingXmppTransaction();
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex); return;
                }
                if (list == null) return;

                if (list.Count <= 0)
                    return;

                List<TransactionMaster> randomLast20 = list.OrderBy(x => Guid.NewGuid()).Take(50).ToList();

                foreach (TransactionMaster item in randomLast20)
                {
                    new ResendProcessing().ResendRecharge(item, false);
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex); return;
            }

            return;
        }

        internal string CMSRefundAll(List<SuccessFailAllRequest> transactionMasterIDList, int UserID)
        {
            int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

            if (!Util.isRoleAdminOrSupport(UserTypeID))
                return TransactionMessage.InvalidUser;

            transactionMasterIDList = GetDistinctValue(transactionMasterIDList);

            foreach (SuccessFailAllRequest item in transactionMasterIDList)
            {
                TransactionMaster transactionMaster = new TransactionDAL().SelectByID(item.ID);
                string docType = GetDocType(transactionMaster.WalletID);
                string faileureMessage = isValidForBulkUpdate(transactionMaster);

                if (!faileureMessage.EqualsIgnoreCase(StatusType.Ok))
                    continue;

                if (transactionMaster.IsOnlineDMT)
                    continue;

                transactionMaster.Status = StatusType.Failure;
                transactionMaster.Description = TransactionMessage.TransFailDescription;
                new AccountCoreProcessing().RefundTransaction(transactionMaster, transactionMaster.Description, docType);
            }
            return StatusType.Ok;
        }




        internal static bool isExcelOrPDF(UserWiseDMTRangeReportRequest mainRequest)
        {

            if (mainRequest.ExportType == "EXCEL")
            {
                mainRequest.IsExcelDownload = true;
                return true;
            }
            if (mainRequest.ExportType == "PDF")
            {
                mainRequest.IsPDFDownload = true;
                return true;
            }
            return false;
        }
    }
}