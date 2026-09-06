using Newtonsoft.Json;
using project.StaticModel;
using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests;
using Project.Requests.PaymentGateway;
using Project.Response.PaymentGateway;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Project.BAL
{
    public class PaymentGatewayBAL
    {

        internal InitiateResponse Initiate(InitRequest mainRequest, KeyDataModel keyDataModel, string iPAddress, out string failureMessage)
        {
            InitiateResponse response = new InitiateResponse();

            failureMessage = new Validation().ServerDownTime();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return response;
            }

            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;

            failureMessage = CheckForDelayBetweenTwoPaymentGateway(keyDataModel.UserID);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;

            //1.Check for user exists
            UserMaster user = null;
            try
            {
                user = new UserDAL().SelectByIDFromCache(keyDataModel.UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            failureMessage = Message.UserMessage.InvalidUser;
            if (user == null)
                return response;

            int ServiceID = 0;
            if (mainRequest.ServiceID.Length <= 0)
            {
                ServiceID = (int)ServiceCode.PaymentGateway;
            }
            else
            {
                ServiceID = Convert.ToInt32(mainRequest.ServiceID);
            }

            //1. check for service on
            bool isServiceOn = new ServiceMasterBal().IsServiceOn(ServiceID);
            if (!isServiceOn)
            {
                failureMessage = ServiceMessage.Off;
                return response;
            }

            //2. check for Userwise service
            string eMessage = "";
            bool isServiceActivated = new UserServiceMappingBAL().IsServiceActivatedForUser(user.UserID, ServiceID, out eMessage);
            if (!isServiceActivated)
            {
                failureMessage = eMessage;
                return response;
            }

            bool IsChannel = new UserDAL().SelectIsChannel(keyDataModel.UserID);
            if (IsChannel)
            {
                Project.Response.UserChannelSlabMapping.ViewModel userSlab = new UserChannelSlabMappingDal().SelectByUserID(keyDataModel.UserID);

                if (userSlab == null)
                {
                    failureMessage = Message.APIUserMessage.ChannelCommissionNotSet;
                    return response;
                }
            }

            if (Company.PaymentGatewayKYC.EqualsIgnoreCase("Yes"))
            {
                bool isKYCApproved = new UserBal().IsKyc(keyDataModel.UserID);
                if (!isKYCApproved)
                {
                    failureMessage = Message.UserMessage.KYCNotApproved;
                    return response;
                }
            }

            string PGDailyCount = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.PGDailyCount), "10");
            string PGDailyAmount = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.PGDailyAmount), "99000");
            PaymentGatewayViewModel getCount = new PaymentGatewayDAL().SelectByUserID(keyDataModel.UserID);

            failureMessage = "Daily gateway transaction limit is over";

            if (getCount.TotalCount > Convert.ToInt32(PGDailyCount) ||
                getCount.TotalAmount > Convert.ToInt32(PGDailyAmount))
            {
                return response;
            }

            int amt = Convert.ToInt32(mainRequest.Amount);
            if (amt <= 0 || amt >= 500001)
            {
                failureMessage = "Invalid amount";
                return response;
            }

            UserWalletMaster userWallet;
            userWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(Company.AdminUserID, mainRequest.WalletID);

            failureMessage = "Admin Invalid Wallet";
            if (userWallet == null)
                return response;

            failureMessage = "Insufficient Balance";
            if (userWallet.Balance <= Convert.ToDecimal(mainRequest.Amount))
                return response;

            ViewModel viewModel = CheckActivePaymentGateway(mainRequest.Type, keyDataModel.UserID);

            failureMessage = PaymentGatewayMessage.NotActive;
            if (viewModel == null)
                return response;

            failureMessage = AmountValidation(viewModel, Convert.ToDecimal(mainRequest.Amount));
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                return response;
            }

            response.PaymentGatewayName = viewModel.Name;

            response.LogoURL = Company.WebAPIURL + Messages.DefaultLogoPath;

            string PayMode = "";

            if (viewModel.Name.EqualsIgnoreCase(GatewayCode.Razorpay))
            {
                Random randomObj = new Random();
                string transactionId = randomObj.Next(10000000, 100000000).ToString();

                RazorpayOrderRequest request = new RazorpayOrderRequest();
                request.amount = Convert.ToInt32(mainRequest.Amount) * 100;
                request.currency = "INR";
                request.receipt = transactionId;

                string body = JsonConvert.SerializeObject(request);

                string Response = HttpProcessing.Razorpay_PostMethod(body, viewModel.MerchantID, viewModel.Token);

                RazorpayOrderResponse res = JsonConvert.DeserializeObject<RazorpayOrderResponse>(Response);

                string orderId = res?.id?.Trim();

                /*PaymentGateway status check log added*/
                PaymentGatewayLog paymentGatewayLog = new PaymentGatewayLog();
                paymentGatewayLog.Amount = Convert.ToDecimal(mainRequest.Amount);
                paymentGatewayLog.UserID = keyDataModel.UserID;
                paymentGatewayLog.Request = orderId;
                paymentGatewayLog.Response = Response;
                paymentGatewayLog.Type = "Init";
                paymentGatewayLog.OrderID = orderId;
                paymentGatewayLog.GatewayName = GatewayCode.Razorpay;
                new PaymentGatewayLogDAL().Insert(paymentGatewayLog);
                /*------------------*/

                if (res == null || res.id == null || res.id.Trim().Length <= 2)
                {
                    failureMessage = res?.Error?.Description?.Trim(); //"Error occurred for generating order, please try after some time";
                    return response;
                }

                orderId = res.id;
                int amount = Convert.ToInt32(mainRequest.Amount) * 100;

                response.MerchantID = viewModel.MerchantID;
                response.Key = "";
                response.Hash = viewModel.Slat.ToString();
                response.Amount = amount.ToString();
                response.Currency = request.currency;
                response.FailURL = viewModel.FailURL;
                response.SuccessURL = viewModel.SuccessURL;
                response.ActionURL = viewModel.ActionURL;
                response.IsActive = viewModel.IsActive;
                response.OrderID = orderId;
                response.Name = Company.ClientName;// user.FirmName;

                response.Email = user.EmailID;
                response.ContactNumber = user.MobileNumber;
            }
            else if (viewModel.Name.EqualsIgnoreCase(GatewayCode.YesBank))
            {
                string RequestID = Guid.NewGuid().ToString();
                RequestID = RequestID.Replace("-", "");
                RequestID = RequestID.ToString().Substring(0, 20);

                //if (keyDataModel.Platform.EqualsIgnoreCase(PlatformCode.Web))
                //{
                //    failureMessage = "Please use Payment gateway in Mobile APP only";
                //    return response;
                //}

                //string[] paramArr = viewModel.StatusCheckURL.Split(",");
                //if (paramArr.Length <= 1)
                //{
                //    failureMessage = "Invalid setting params, please contact to Customer Care";
                //    return response;
                //}
                //string mccode = new PaymentGatewayProcessing().getTerminalId(paramArr[0].Trim());

                Processing.YesBank.PaymentGatewayProcessing pgProcessing = new Processing.YesBank.PaymentGatewayProcessing();

                YESBankCore.Response.QRInitResponse qrResponse = pgProcessing.QRLogIntent(RequestID, mainRequest.Amount, keyDataModel.UserID);

                if (qrResponse.Status != StatusType.Success)
                {
                    failureMessage = qrResponse.Message;
                    return response;
                }

                string UPIURL = "upi://pay?pa=[UPIID]&pn=[PN]&tr=[TR]&am=[Amount]&cu=INR&mc=[MCCCode]";
                UPIURL = UPIURL.Replace("[UPIID]", qrResponse.AdminVPA);
                UPIURL = UPIURL.Replace("[PN]", qrResponse.MerchantName.Trim().Replace(" ", "%20"));
                UPIURL = UPIURL.Replace("[Amount]", mainRequest.Amount.ToString());
                UPIURL = UPIURL.Replace("[TR]", RequestID);
                UPIURL = UPIURL.Replace("[MCCCode]", qrResponse.MCCCode);

                /*PaymentGateway init log added*/
                PaymentGatewayLog paymentGatewayLog = new PaymentGatewayLog();
                paymentGatewayLog.Amount = Convert.ToDecimal(mainRequest.Amount);
                paymentGatewayLog.UserID = user.UserID;
                paymentGatewayLog.Request = UPIURL;
                paymentGatewayLog.Response = "Init started";
                paymentGatewayLog.Type = "Init";
                paymentGatewayLog.OrderID = RequestID;
                paymentGatewayLog.GatewayName = GatewayCode.YesBank;
                new PaymentGatewayLogDAL().Insert(paymentGatewayLog);
                /*------------------*/

                string qrImage = new QRCodeBAL().getQRImage(UPIURL);

                if (keyDataModel.Platform.EqualsIgnoreCase(PlatformCode.Web))
                {
                    response.Hash = qrImage;
                }
                else
                {
                    response.Hash = UPIURL;
                }

                //response.Hash = UPIURL;
                response.PaymentGatewayName = GatewayCode.ICICIQR;
                PayMode = "UPI";
                response.OrderID = RequestID;
                response.SenderVPA = "";
            }
            else if (viewModel.Name.EqualsIgnoreCase(GatewayCode.RazorpayQR))
            {
                DateTime currentTime = DateTime.UtcNow.AddMinutes(3);
                long unixTime = ((DateTimeOffset)currentTime).ToUnixTimeSeconds();

                RazorpayQRRequest request = new RazorpayQRRequest();
                request.payment_amount = Convert.ToInt32(mainRequest.Amount) * 100;
                request.type = "upi_qr";
                request.usage = "single_use";
                request.fixed_amount = true;
                request.close_by = unixTime;

                string body = JsonConvert.SerializeObject(request);

                string Response = HttpProcessing.RazorpayQR_PostMethod(body, viewModel.MerchantID, viewModel.Token);

                RazorpayQRResponse res = JsonConvert.DeserializeObject<RazorpayQRResponse>(Response);

                string orderId = res?.id.TrimIfNotNull();

                /*PaymentGateway status check log added*/
                PaymentGatewayLog paymentGatewayLog = new PaymentGatewayLog();
                paymentGatewayLog.Amount = Convert.ToDecimal(mainRequest.Amount);
                paymentGatewayLog.UserID = keyDataModel.UserID;
                paymentGatewayLog.Request = body;
                paymentGatewayLog.Response = Response;
                paymentGatewayLog.Type = "Init";
                paymentGatewayLog.OrderID = orderId;
                paymentGatewayLog.GatewayName = GatewayCode.RazorpayQR;
                new PaymentGatewayLogDAL().Insert(paymentGatewayLog);
                /*------------------*/

                if (res == null || res.id == null || res.id.TrimIfNotNull().Length <= 2)
                {
                    failureMessage = res?.description?.Trim();  //"Error occurred for generating order, please try after some time";
                    return response;
                }

                orderId = res.id;
                int amount = Convert.ToInt32(mainRequest.Amount) * 100;
                string imageURL = "";
                imageURL = String.Concat(Company.WebAPIURL, HttpProcessing.DownloadedRazorPayQRImageName(res.image_url, orderId));
                response.MerchantID = "";
                response.Key = "";
                response.Hash = "";
                response.Amount = mainRequest.Amount;
                response.SuccessURL = imageURL;
                response.FailURL = "";
                response.ActionURL = "";
                response.IsActive = viewModel.IsActive;
                response.OrderID = orderId;
                response.Name = Company.ClientName;
                response.Email = user.EmailID;
                response.ContactNumber = user.MobileNumber;
            }
            else if (viewModel.Name.EqualsIgnoreCase(GatewayCode.WorldLine))
            {
                DateTime currentTime = DateTime.UtcNow.AddMinutes(3);
                long unixTime = ((DateTimeOffset)currentTime).ToUnixTimeSeconds();

                string RequestID = Guid.NewGuid().ToString();
                RequestID = RequestID.Replace("-", "");
                RequestID = RequestID.ToString().Substring(0, 20);

                string orderId = RequestID;

                string consumerId = $"{user.UserID}_{user.AliasName}";

                string hashSequence = "merchantId|txnId|totalamount|accountNo|consumerId|MobileNo|email|udf1|udf2|udf3|udf4|udf5|udf6|udf7|udf8|SALT";
                string hash_string = "";
                string[] hashVarsSeq;
                hashVarsSeq = hashSequence.Split('|');

                System.Collections.Hashtable data = new System.Collections.Hashtable(); // adding values in gash table for data post

                data.Add("merchantId", viewModel.MerchantID);
                data.Add("txnId", orderId);
                data.Add("totalamount", mainRequest.Amount.ToString());
                data.Add("accountNo", "");
                data.Add("consumerId", consumerId);
                data.Add("MobileNo", user.MobileNumber.Trim());
                data.Add("email", user.EmailID.Trim());
                data.Add("udf1", "");
                data.Add("udf2", "");
                data.Add("udf3", "");
                data.Add("udf4", "");
                data.Add("udf5", "");
                data.Add("udf6", "");
                data.Add("udf7", "");
                data.Add("udf8", "");

                foreach (string hash_var in hashVarsSeq)
                {
                    hash_string = hash_string + (data.ContainsKey(hash_var) ? data[hash_var].ToString() : "");
                    hash_string = hash_string + '|';
                }

                hash_string += viewModel.Slat;

                string gen_hash = Generatehash512(hash_string).ToLower();  //generating hash
                response.Hash = gen_hash;

                PaymentGatewayLog paymentGatewayLog = new PaymentGatewayLog();
                paymentGatewayLog.Amount = Convert.ToDecimal(mainRequest.Amount);
                paymentGatewayLog.UserID = keyDataModel.UserID;
                paymentGatewayLog.Request = "" + RequestID;
                paymentGatewayLog.Response = "";
                paymentGatewayLog.Type = "Init";
                paymentGatewayLog.OrderID = orderId;
                paymentGatewayLog.GatewayName = GatewayCode.WorldLine;
                new PaymentGatewayLogDAL().Insert(paymentGatewayLog);

                int amount = Convert.ToInt32(mainRequest.Amount) * 100;
                response.MerchantID = viewModel.MerchantID;

                response.Key = viewModel.Token;//itemid
                response.ChannelID = viewModel.SuccessURL;//instrumentId
                response.SuccessURL = viewModel.FailURL;//bank code
                response.Amount = mainRequest.Amount;
                response.RedirectURL = Company.WebAPIURL + "WorldLinePGCallback/postWorldLinePG";
                response.FailURL = "";
                response.ActionURL = "";
                response.IsActive = viewModel.IsActive;
                response.OrderID = orderId;
                response.Name = Company.ClientName;
                response.Email = user.EmailID;
                response.ContactNumber = user.MobileNumber;
                response.Currency = "INR";
                response.CustID = consumerId.ToString();
            }
            else if (viewModel.Name.EqualsIgnoreCase(GatewayCode.PayU))
            {
                string OrderId = Guid.NewGuid().ToString("N").Substring(0, 15);
                OrderId = OrderId.Replace("-", "");
                // sha512(key | txnid | amount | productinfo | firstname | email | udf1 | udf2 | udf3 | udf4 | udf5 |||||| SALT)
                string strdata = $"{viewModel.Token}|{OrderId}|{mainRequest.Amount}|Ecom|{user.FirmName}|{user.EmailID}|||||||||||{viewModel.Slat}";
                string Hash = Generatehash512(strdata);
                response.Hash = Hash;
                response.Key = viewModel.Token;
                response.OrderID = OrderId;
                response.Amount = mainRequest.Amount.ToString();
                response.Name = user.FirmName;
                response.FailURL = viewModel.FailURL;
                response.SuccessURL = viewModel.SuccessURL;
                response.ActionURL = viewModel.ActionURL;
                response.Email = user.EmailID;
                response.ContactNumber = user.MobileNumber;
                response.Currency = "Ecom";
                response.IsActive = viewModel.IsActive;

            }

            PaymentGatewayViewModel view = new PaymentGatewayDAL().SelectByOrderID(response.OrderID);

            failureMessage = Message.PaymentGatewayMessage.InvalidOrderID;
            if (view != null)
                return response;

            UserWalletMaster receiverWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(keyDataModel.UserID, mainRequest.WalletID);

            if (receiverWallet == null)
            {
                failureMessage = "Invalid Wallet";
                return response;
            }

            PaymentGateway paymentGateway = new PaymentGateway
            {
                Status = StatusType.Process,
                PaymentStatus = StatusType.Process,
                UserID = keyDataModel.UserID,
                Amount = Convert.ToDecimal(mainRequest.Amount),
                GatewayCharge = 0,
                Commission = 0,
                IsUpdate = 0,
                PayMode = PayMode,
                TransactionID = "",
                Response = "",
                WalletID = mainRequest.WalletID,
                OrderID = response.OrderID,
                PaymentProviderID = viewModel.PaymentGatewaySettingID,
                OpeningBalance = 0,
                ClosingBalance = 0,
                ReferenceID = response.TxnToken,
                Sender = response.SenderVPA,
                ServiceID = ServiceID
            };

            int PaymentGatewayID = new PaymentGatewayDAL().Insert(paymentGateway);
            paymentGateway.PaymentGatewayID = PaymentGatewayID;

            response.Message = StatusType.Ok;
            failureMessage = StatusType.Ok;
            return response;
        }
        public bool IsValidForSettlement(string orderID, string TransactionID, string Status)
        {
            if (!Company.IsPGSettlementHold.EqualsIgnoreCase("Yes"))
                return true;

            if (Status == StatusType.Failure)
                return true;

            PaymentGateway objmodel = new PaymentGatewayDAL().SelectTransactionByOrderIDWithProcessOnly(orderID);

            if (objmodel.PaymentStatus != StatusType.Success)
            {
                objmodel.PaymentStatus = Status;
                objmodel.TransactionID = TransactionID;
                new PaymentGatewayDAL().Update(objmodel);
            }

            DateTime RchDate = objmodel.DOC;
            DateTime currDate = DateTime.Now;
            double diffInMin = (currDate - RchDate).TotalMinutes;

            ViewModel viewModel = new PaymentGatewaySettingDal().SelectByPGSettingID(objmodel.PaymentProviderID);
            if (viewModel == null)
                return true;

            if (diffInMin >= viewModel.SettlementHoldInMinutes)
                return true;

            return false;
        }

        internal List<PaymentGatewayViewModel> SelectLast10PaymentGateway(int userID, int ServicID)
        {
            List<PaymentGatewayViewModel> returnList = new List<PaymentGatewayViewModel>();
            try
            {
                returnList = new PaymentGatewayDAL().SelectLast10PaymentGateway(userID, ServicID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return returnList;
        }

        internal PaymentGatewayViewModel SelectForReceipt(SelectOrderIDRequest mainRequest, KeyDataModel keyDataModel, out string failureMessage)
        {
            PaymentGatewayViewModel view = new PaymentGatewayViewModel();

            failureMessage = mainRequest.IsValidModalState();

            try
            {
                view = new PaymentGatewayDAL().SelectByOrderID(mainRequest.OrderID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (view == null)
                return view;

            failureMessage = StatusType.Ok;
            return view;
        }

        internal int UpdateCore(PaymentGateway objmodel)
        {
            try
            {
                if (new ResponseProcess().CheckFailure(objmodel.Status))
                {
                    objmodel.ClosingBalance = objmodel.OpeningBalance;
                    objmodel.Commission = 0;
                    objmodel.SuperDistributerCommission = 0;
                    objmodel.DistributerCommission = 0;
                }

                decimal AdminGST = 0;
                decimal RetailerTDS = 0;
                decimal DistTDS = 0;
                decimal SuperDistTDS = 0;

                //added RetailerTDS
                try
                {
                    RetailerTDS = new TaxProcessing().GetTDSOnCommission(objmodel.Commission);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                    RetailerTDS = 0;
                }

                //added DistTDS
                try
                {
                    DistTDS = new TaxProcessing().GetTDSOnCommission(objmodel.DistributerCommission);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                    DistTDS = 0;
                }

                //added SuperDistTDS
                try
                {
                    SuperDistTDS = new TaxProcessing().GetTDSOnCommission(objmodel.SuperDistributerCommission);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                    SuperDistTDS = 0;
                }

                //added AdminGST
                try
                {
                    AdminGST = new TaxProcessing().GetAdminGSTOnCommission(objmodel.Commission, objmodel.DistributerCommission, objmodel.SuperDistributerCommission);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                    AdminGST = 0;
                }

                objmodel.RetailerTDS = RetailerTDS;
                objmodel.DistributerTDS = DistTDS;
                objmodel.SuperDistributerTDS = SuperDistTDS;
                objmodel.AdminGST = AdminGST;

                PaymentGatewayDAL dal = new PaymentGatewayDAL();

                FundTransaction ObjFundModel = new FundTransaction
                {
                    ReceiverID = objmodel.UserID,
                    WalletID = objmodel.WalletID,
                    Amount = objmodel.Amount,
                    Type = FundTransferType.Transfer,
                    Through = FundTransferType.PGTransfer,
                    AdminRemark = objmodel.OrderID,
                    UTRNumber = objmodel.TransactionID,
                };

                if (objmodel.TransactionID.TrimIfNotNull().Length <= 1 && objmodel.Status.EqualsIgnoreCase(StatusType.Success))
                {
                    objmodel.Status = StatusType.Process;
                    return -1;
                }

                using (ConcurrencyControl.AcquireLock(objmodel.UserID, objmodel.UserID.ToString()))
                {
                    if (objmodel.Status.EqualsIgnoreCase(StatusType.Success))
                    {


                        FundTransaction objDuplicate = SelectTransactionForDuplicatePGTransaction(ObjFundModel, objmodel.PaymentProviderID);

                        if (objDuplicate != null)
                        {
                            new Logger().Info("PaymentGatewayBAL", "UpdateCore", "Duplicate found for " + objmodel.Amount + " " + objmodel.OrderID);
                            return -1;
                        }
                    }
                    return dal.Update(objmodel);
                }

                //if (objmodel.Status.EqualsIgnoreCase(StatusType.Success))
                //{
                //    FundTransaction objDuplicate = new FundTransactionDal().SelectTransactionForDuplicateCheckForPGTransaction(ObjFundModel);

                //    if (objDuplicate != null)
                //    {
                //        new Logger().Info("PaymentGatewayBAL", "UpdateCore", "Duplicate found for " + objmodel.Amount + " " + objmodel.OrderID);
                //        return -1;
                //    }
                //}

                //return dal.Update(objmodel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return -1;
            }
            return 0;
        }

        private FundTransaction SelectTransactionForDuplicatePGTransaction(FundTransaction objFundModel, int PaymentProviderID)
        {
            FundTransaction objDuplicate = null;
            ViewModel viewModel = null;
            try
            {
                viewModel = new PaymentGatewaySettingDal().SelectByPGSettingID(PaymentProviderID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (viewModel == null)
            {
                return objDuplicate;
            }

            if (viewModel.Name.EqualsIgnoreCase(GatewayCode.PhonePay) && viewModel.FailURL.EqualsIgnoreCase("UAT")) return objDuplicate;

            objDuplicate = new FundTransactionDal().SelectTransactionForDuplicateCheckForPGTransaction(objFundModel);
            return objDuplicate;
        }

        private ViewModel CheckActivePaymentGateway(string type, int UserID)
        {
            ViewModel viewModel = null;
            string failureMessage = "";
            List<ViewModel> response = new List<ViewModel>();
            try
            {
                response = new PaymentGatewayBAL().SelectActivePaymentGatewayV2(UserID, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return viewModel;

            if (response == null || response.Count <= 0)
            {
                return viewModel;
            }

            if (type.Length <= 1)
            {
                viewModel = response[0];
                return viewModel;
            }

            if (response != null)
            {
                foreach (ViewModel setting in response)
                {
                    if (type.EqualsIgnoreCase("UPI") && setting.IsUPI)
                    {
                        return setting;
                    }
                    else if (type.EqualsIgnoreCase("PG") && !setting.IsUPI)
                    {
                        return setting;
                    }
                }
            }

            return viewModel;
        }

        private string AmountValidation(ViewModel viewModel, decimal Amount)
        {
            if (Amount < viewModel.MinimumAmount)
            {
                return $"Amount should greater than {viewModel.MinimumAmount.TwoDigitDecimal()}";
            }
            if (Amount > viewModel.MaximumAmount)
            {
                return $"Amount should less than {viewModel.MaximumAmount.TwoDigitDecimal()}"; ;
            }
            return StatusType.Ok;
        }

        private bool isPending(string Status)
        {
            if (Status.Trim().Length <= 0)
            {
                return true;
            }

            if (new ResponseProcess().checkProcess(Status))
            {
                return true;
            }
            return false;
        }

        internal string UpdateStatusManually(UpdateManualRequest mainRequest, int userID)
        {
            string failureMessage = "";
            try
            {
                failureMessage = FunctionalValidationProcessing.IsValidMPin(userID, mainRequest.Pin);

                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                    return failureMessage;

                int UserTypeID = new UserDAL().SelectUserTypeID(userID);

                if (!Util.isRoleAdminOrSupport(UserTypeID))
                {
                    failureMessage = "Invalid User";
                    return failureMessage;
                }

                PaymentGateway objmodel = new PaymentGatewayDAL().SelectByID(mainRequest.PaymentGatewayID);

                if (objmodel == null)
                {
                    failureMessage = "Invalid PaymentGatewayID";
                    return failureMessage;
                }

                objmodel.IsUpdate = 1;

                //if (!objmodel.Status.EqualsIgnoreCase(StatusType.Process))

                ViewModel viewModel = null;
                try
                {
                    viewModel = new PaymentGatewaySettingDal().SelectByPGSettingID(objmodel.PaymentProviderID);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }

                if (viewModel == null)
                {
                    failureMessage = "Gateway not active or not found ";
                    return failureMessage;
                }

                if (!IsValidUpdateStatusManually(viewModel.Name, viewModel.IsManualStatusChange))
                {
                    failureMessage = "Not allowed to update transaction, please use status check";
                    return failureMessage;
                }

                if (!isPending(objmodel.Status))
                {
                    failureMessage = "Transaction is not pending";
                    return failureMessage;
                }

                if (viewModel.PaymentGatewaySettingID != objmodel.PaymentProviderID)
                {
                    failureMessage = "Gateway not match";
                    return failureMessage;
                }

                objmodel.TransactionID = mainRequest.ReferenceNo;
                objmodel.Status = mainRequest.Status;

                int rowsAffected = -1;
                try
                {
                    rowsAffected = new PaymentGatewayBAL().UpdateCore(objmodel);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }

                if (rowsAffected <= 0)
                {
                    return failureMessage;
                }

                if (new ResponseProcess().CheckFailure(mainRequest.Status))
                {
                    return failureMessage;
                }

                if (new ResponseProcess().CheckSuccess(mainRequest.Status))
                {
                    PaymentGatewayProcessing objProcessing = new PaymentGatewayProcessing();
                    failureMessage = objProcessing.BalanceTransferAndCommissionProcessing(objmodel, checkDuplicate: false);
                }

                failureMessage = "Status updated successfully";
                return failureMessage;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return failureMessage;
        }

        internal SelectByIDResponse SelectByID(SelectByIDRequest mainRequest, int userID, out string failureMessage)
        {
            SelectByIDResponse Model = new SelectByIDResponse();

            try
            {
                PaymentGateway transactionMaster = new PaymentGatewayDAL().SelectByID(mainRequest.ID);

                if (transactionMaster == null)
                {
                    failureMessage = "Invalid PaymentGatewayID";
                    return Model;
                }

                Model.Status = transactionMaster.Status;
                Model.PaymentGatewayID = transactionMaster.PaymentGatewayID;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            failureMessage = StatusType.Ok;
            return Model;
        }

        internal string UpdateFreeUPIStatus(FreeUPIStatusUpdateRequest mainRequest, int userID, out string failureMessage)
        {
            failureMessage = mainRequest.IsValidModalState();

            string reqStr = JsonConvert.SerializeObject(mainRequest);

            new Logger().Info("FreeUPI", "StatusUpdate", reqStr);

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            try
            {
                PaymentGateway objmodel = new PaymentGatewayDAL().SelectTransactionByOrderIDWithProcessOnly(mainRequest.OrderID);

                if (objmodel == null)
                {
                    failureMessage = "Invalid data";
                    return failureMessage;
                }

                if (!new ResponseProcess().checkProcess(objmodel.Status))
                {
                    failureMessage = "Transaction " + objmodel.Status;
                    return failureMessage;
                }

                /*PaymentGateway status check log added*/
                PaymentGatewayLog paymentGatewayLog = new PaymentGatewayLog();
                paymentGatewayLog.Amount = Convert.ToDecimal(objmodel.Amount);
                paymentGatewayLog.UserID = userID;
                paymentGatewayLog.Request = mainRequest.OrderID;
                paymentGatewayLog.Response = reqStr;
                paymentGatewayLog.Type = "StatusUpdate";
                paymentGatewayLog.OrderID = mainRequest.OrderID;
                paymentGatewayLog.GatewayName = GatewayCode.FreeUPI;
                new PaymentGatewayLogDAL().Insert(paymentGatewayLog);
                /*------------------*/

                DateTime initDate = objmodel.DOC;
                DateTime currDate = DateTime.Now;

                double diffInSec = (currDate - initDate).TotalSeconds;

                if (diffInSec >= 90)
                {
                    failureMessage = "Transaction not allowed";
                    return failureMessage;
                }

                if (objmodel.UserID != userID)
                {
                    failureMessage = "Invalid User";
                    return failureMessage;
                }

                if (objmodel.IsUpdate == 1)
                {
                    failureMessage = "Transaction already proceed";
                    return failureMessage;
                }

                objmodel.OrderID = mainRequest.OrderID;
                objmodel.IsUpdate = 1;

                //if (!objmodel.Status.EqualsIgnoreCase(StatusType.Process))
                if (!new ResponseProcess().checkProcess(objmodel.Status))
                {
                    failureMessage = "Transaction is not pending";
                    return failureMessage;
                }

                ViewModel viewModel = null;
                try
                {
                    viewModel = new PaymentGatewaySettingDal().SelectByPGSettingID(objmodel.PaymentProviderID);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }

                int DBUserId = objmodel.UserID;
                decimal DBAmount = objmodel.Amount;

                if (!IsValidStatusCheckGateway(viewModel.Name))
                {
                    failureMessage = "Invalid Gateway";
                    return failureMessage;
                }

                if (viewModel.PaymentGatewaySettingID != objmodel.PaymentProviderID)
                {
                    failureMessage = "Gateway not match";
                    return failureMessage;
                }

                int rowsAffected = -1;

                objmodel.PayMode = "UPI";

                if (new ResponseProcess().CheckFailure(mainRequest.Status))
                {
                    objmodel.TransactionID = "";
                    objmodel.Status = StatusType.Failure;
                    objmodel.Response = mainRequest.Message;

                    try
                    {
                        rowsAffected = new PaymentGatewayBAL().UpdateCore(objmodel);
                    }
                    catch (Exception ex)
                    {
                        new Logger().Write(ex);
                    }
                    return "Transaction failed";
                }

                if (new ResponseProcess().checkProcess(mainRequest.Status))
                {
                    objmodel.TransactionID = "";
                    objmodel.Status = StatusType.Process;
                    objmodel.Response = mainRequest.Message;

                    try
                    {
                        rowsAffected = new PaymentGatewayBAL().UpdateCore(objmodel);
                    }
                    catch (Exception ex)
                    {
                        new Logger().Write(ex);
                    }
                    return "Transaction under process";
                }

                if (!mainRequest.Status.EqualsIgnoreCase(StatusType.Success))
                {
                    objmodel.TransactionID = "";
                    objmodel.Status = StatusType.Process;
                    objmodel.Response = mainRequest.Message;

                    try
                    {
                        rowsAffected = new PaymentGatewayBAL().UpdateCore(objmodel);
                    }
                    catch (Exception ex)
                    {
                        new Logger().Write(ex);
                    }
                    return "Transaction under process";
                }

                //message : txnId=aaazzzssss&txnRef=orderid&status=[Failed/success/failure]&responseCode=[00/01]
                string txnId = "";
                string msgOrderID = "";
                string msgStatus = StatusType.Process;
                string msgrespoCode = "";

                string[] DataList = mainRequest.Message.Split("&");

                foreach (string data in DataList)
                {
                    string[] arr = data.Split("=", 2);

                    if (arr.Length <= 1)
                    {
                        continue;
                    }
                    string Key = arr[0].Trim();
                    string value = arr[1].Trim();

                    if (Key.EqualsIgnoreCase("txnId"))
                    {
                        txnId = value;
                    }

                    if (Key.EqualsIgnoreCase("txnRef"))
                    {
                        msgOrderID = value;
                    }

                    if (Key.EqualsIgnoreCase("status"))
                    {
                        msgStatus = value;
                    }
                    if (Key.EqualsIgnoreCase("responseCode"))
                    {
                        msgrespoCode = value;
                    }
                }

                if (!mainRequest.OrderID.EqualsIgnoreCase(msgOrderID))
                {
                    failureMessage = "Order not match";
                    return failureMessage;
                }

                objmodel.TransactionID = txnId;
                objmodel.Response = mainRequest.Message;
                objmodel.Status = msgStatus;

                if (msgStatus.EqualsIgnoreCase("success"))
                {
                    objmodel.Status = StatusType.Success;
                }

                if (msgStatus.EqualsIgnoreCase("Failed"))
                {
                    objmodel.Status = StatusType.Failure;
                }

                if (txnId.ToLower().StartsWith("PTM".ToLower()))
                {
                    objmodel.Status = StatusType.Process; // because Paytm app has cracked by some hacker
                }

                if (msgStatus.EqualsIgnoreCase(""))
                {
                    objmodel.Status = StatusType.Process;
                }

                objmodel.PayMode = "UPI";
                if (Company.FreeUPIAutoSuccess.EqualsIgnoreCase("No"))
                {
                    failureMessage = "Transaction is pending contact to support";
                    return failureMessage;
                }
                try
                {
                    objmodel.Status = objmodel.Status.ToUpper();
                    rowsAffected = new PaymentGatewayBAL().UpdateCore(objmodel);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }

                if (rowsAffected <= 0)
                {
                    return failureMessage;
                }

                if (new ResponseProcess().CheckFailure(objmodel.Status))
                {
                    return failureMessage;
                }

                if (new ResponseProcess().CheckSuccess(objmodel.Status))
                {
                    PaymentGatewayProcessing objProcessing = new PaymentGatewayProcessing();
                    failureMessage = objProcessing.BalanceTransferAndCommissionProcessing(objmodel, checkDuplicate: false);
                }

                failureMessage = "Status updated successfully";
                return failureMessage;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return failureMessage;
            }
        }

        internal ViewModel SelectActivePaymentGateway(int userID, out string failureMessage)
        {
            ViewModel viewModel = null;
            List<ViewModel> userwise = null;
            try
            {
                userwise = new PaymentGatewayBAL().SelectActivePaymentGatewayV2(userID, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            failureMessage = Message.UserMessage.InvalidUser;
            if (userwise == null)
                return viewModel;

            viewModel = userwise[0];

            failureMessage = StatusType.Ok;
            return viewModel;
        }

        internal ViewModel SelectStaticQR(int UserID, out string failureMessage)
        {
            ViewModel viewModel = null;
            List<ViewModel> response = new List<ViewModel>();
            try
            {
                response = new PaymentGatewayBAL().SelectActivePaymentGatewayV2(UserID, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (response.Count <= 0)
            {
                failureMessage = Message.PaymentGatewayMessage.NotActive;
                return viewModel;
            }

            bool isStaticQRAvailable = false;

            foreach (ViewModel res in response)
            {
                if (res.Name.EqualsIgnoreCase(GatewayCode.ICICIQR))
                {
                    isStaticQRAvailable = true;
                    viewModel = new ViewModel();
                    viewModel.Name = res.Name;
                    viewModel.ActionURL = res.ActionURL;
                    viewModel.IsActive = res.IsActive;
                    viewModel.MaximumAmount = res.MaximumAmount;
                    viewModel.MinimumAmount = res.MinimumAmount;
                    viewModel.FailURL = res.FailURL;
                    viewModel.ICICIQR = res.ICICIQR;
                    viewModel.MerchantID = res.MerchantID;
                    viewModel.Slat = res.Slat;
                    viewModel.StatusCheckURL = res.StatusCheckURL;
                    viewModel.SuccessURL = res.SuccessURL;
                    viewModel.Token = res.Token;
                }

                if (res.Name.EqualsIgnoreCase(GatewayCode.Paysprint))
                {
                    isStaticQRAvailable = true;
                    viewModel = new ViewModel();
                    viewModel.Name = res.Name;
                    viewModel.ActionURL = res.ActionURL;
                    viewModel.IsActive = res.IsActive;
                    viewModel.MaximumAmount = res.MaximumAmount;
                    viewModel.MinimumAmount = res.MinimumAmount;
                }

                if (res.Name.EqualsIgnoreCase(GatewayCode.EasebuzzStaticQR))
                {
                    isStaticQRAvailable = true;
                    viewModel = new ViewModel();
                    viewModel.Name = res.Name;
                    viewModel.ActionURL = res.ActionURL;
                    viewModel.IsActive = res.IsActive;
                    viewModel.ICICIQR = res.ICICIQR;
                    viewModel.MaximumAmount = res.MaximumAmount;
                    viewModel.MinimumAmount = res.MinimumAmount;
                }
            }

            failureMessage = Message.PaymentGatewayMessage.NotActive;
            if (!isStaticQRAvailable)
                return viewModel;

            UserMaster user = null;
            try
            {
                user = new UserDAL().SelectByIDFromCache(UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            failureMessage = Message.UserMessage.InvalidUser;
            if (user == null)
                return viewModel;

            //QR Off for OutsideSignUp
            string IsStaticQRForOutsideUser = new SettingMasterBal().SelectByKeyWithDefaultValueWithoutWhiteList(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.IsStaticQRForOutsideSign), "No");
            if (!IsStaticQRForOutsideUser.EqualsIgnoreCase("Yes"))
            {
                if (user.RegistrationType.EqualsIgnoreCase(RegistrationType.OutsideSignUp))
                    return viewModel;
            }

            bool isKYCApproved = new UserBal().IsKyc(UserID);
            if (isKYCApproved == false)
            {
                failureMessage = UserMessage.KYCNotApproved;
                return viewModel;
            }



            if (viewModel.Name.EqualsIgnoreCase(GatewayCode.EasebuzzStaticQR))
            {
                Response.VirtualAccountDetails.ViewModel res = new Response.VirtualAccountDetails.ViewModel();

                res = new VirtualAccountDetailsBal().Select(UserID, viewModel.Name, VirtualAccountType.StaticQR);

                if (res == null)
                {
                    failureMessage = Message.UserMessage.InvalidData;
                    return viewModel;
                }

                viewModel.ICICIQR = res.Value;
            }

            if (viewModel.Name.EqualsIgnoreCase(GatewayCode.ICICIQR))
            {
                if (viewModel.ActionURL == null || viewModel.ActionURL.Length != 3)
                {
                    failureMessage = "3 letter prefix not set, please contact to customer care";
                    return viewModel;
                }

                if (user.ICICI_Static_GUID == null || user.ICICI_Static_GUID.Length <= 10)
                {
                    failureMessage = "User static code not generated, please contact to customer care";
                    return viewModel;
                }

                string UserUniqueID = viewModel.ActionURL + "" + user.ICICI_Static_GUID;

                string mccode = viewModel.StatusCheckURL;

                if (mccode.Length <= 2)
                {
                    mccode = "5411";
                }

                string UPIURL = "upi://pay?pa=[UPIID]&pn=[PN]&tr=[TR]&am=&cu=INR&mc=[mccode]&mam=[MiniumAmount]";
                UPIURL = UPIURL.Replace("[UPIID]", viewModel.Token);
                UPIURL = UPIURL.Replace("[PN]", user.FirmName);
                UPIURL = UPIURL.Replace("[TR]", UserUniqueID);
                UPIURL = UPIURL.Replace("[mccode]", mccode);
                UPIURL = UPIURL.Replace("[MiniumAmount]", viewModel.MinimumAmount.ToString());

                string qrImage = new QRCodeBAL().getQRImage(UPIURL);
                viewModel.ICICIQR = qrImage;
            }

            failureMessage = StatusType.Ok;
            return viewModel;
        }

        internal List<ViewModel> SelectActivePaymentGatewayV2(int UserID, out string failureMessage)
        {
            UserwiseGatewayViewModel userwisepg = new UserWisePayementGatewaySettingBal().SelectPGByUserID(UserID);
            List<ViewModel> userwiseandall = null;

            if (userwisepg == null || userwisepg.PaymentGatewaySettingID <= 0)
            {
                try
                {
                    userwiseandall = new PaymentGatewaySettingDal().SelectActivePaymentGatewayV2();
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }

                failureMessage = PaymentGatewayMessage.NotActive;
                if (userwiseandall == null) return userwiseandall;

                failureMessage = StatusType.Ok;
                return userwiseandall;
            }

            ViewModel pguser = null;
            try
            {
                pguser = new PaymentGatewaySettingDal().SelectByPGSettingID(userwisepg.PaymentGatewaySettingID);

                failureMessage = PaymentGatewayMessage.NotActive;
                if (pguser == null) return userwiseandall;

                userwiseandall = new List<ViewModel>();
                if (userwisepg.MinimumAmount > 0)
                    pguser.MinimumAmount = userwisepg.MinimumAmount;
                if (userwisepg.MaximumAmount > 0)
                    pguser.MaximumAmount = userwisepg.MaximumAmount;

                userwiseandall.Add(pguser);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            ViewModel reamingpg = null;
            try
            {
                reamingpg = new PaymentGatewaySettingDal().SelectActivePaymentGatewayBYIsUPI(pguser.IsUPI);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            failureMessage = StatusType.Ok;
            if (reamingpg == null) return userwiseandall;

            userwiseandall.Add(reamingpg);

            return userwiseandall;
        }

        internal string TransactionStatus(TransactionStatusRequest mainRequest, int userID, out string failureMessage)
        {
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            try
            {
                DateTime Date = DateTime.Now.AddDays(-30);
                int fromDateInt = Convert.ToInt32(Convert.ToDateTime(Date).ToString(Messages.DOCIntegerFomrat));

                PaymentGateway objmodel = new PaymentGatewayDAL().SelectTransactionByOrderIDWithProcessAndFromDate(mainRequest.OrderID, fromDateInt);

                if (objmodel == null)
                {
                    failureMessage = "Invalid data";
                    return failureMessage;
                }

                //if (!objmodel.Status.EqualsIgnoreCase(StatusType.Process))
                if (!new ResponseProcess().checkProcess(objmodel.Status))
                {
                    failureMessage = "Transaction " + objmodel.Status;
                    return failureMessage;
                }

                int UserTypeID = new UserDAL().SelectUserTypeID(userID);

                if (!Util.isRoleAdminOrSupport(UserTypeID))
                {
                    if (objmodel.UserID != userID)
                    {
                        failureMessage = "Invalid User";
                        return failureMessage;
                    }
                }

                ViewModel viewModel = null;
                try
                {
                    viewModel = new PaymentGatewaySettingDal().SelectByPGSettingID(objmodel.PaymentProviderID);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }

                if (viewModel == null)
                {
                    failureMessage = "Gateway not active or not found ";
                    return failureMessage;
                }

                if (!viewModel.Name.EqualsIgnoreCase(GatewayCode.OpenBank))
                {
                    if (objmodel.IsUpdate == 1)
                    {
                        failureMessage = "Transaction already proceed";
                        return failureMessage;
                    }
                }

                objmodel.OrderID = mainRequest.OrderID;
                objmodel.IsUpdate = 1;

                int DBUserId = objmodel.UserID;
                decimal DBAmount = objmodel.Amount;

                if (!IsValidStatusCheckGateway(viewModel.Name))
                {
                    failureMessage = "Invalid Gateway";
                    return failureMessage;
                }

                if (viewModel.PaymentGatewaySettingID != objmodel.PaymentProviderID)
                {
                    failureMessage = "Gateway not match";
                    return failureMessage;
                }

                int rowsAffected = -1;

                StatusCheckResponse statusCheckResponse = new StatusCheckResponse();

                if (viewModel.Name.EqualsIgnoreCase(GatewayCode.Razorpay))
                {
                    statusCheckResponse = StatusCheckRozarPay(objmodel.OrderID, objmodel.Amount, objmodel.UserID, objmodel.DOC, out failureMessage);

                    if (statusCheckResponse.Status.EqualsIgnoreCase(StatusType.Process))// some user did upi txn later
                    {
                        RandomNumberProcessing.RandomDelay(3200, 5800);
                        statusCheckResponse = StatusCheckRozarPay(objmodel.OrderID, objmodel.Amount, objmodel.UserID, objmodel.DOC, out failureMessage);

                        if (statusCheckResponse.Status.EqualsIgnoreCase(StatusType.Process))// some user did upi txn later
                        {
                            failureMessage = "Transaction under Process";
                            return failureMessage;
                        }
                    }
                }
                else if (viewModel.Name.EqualsIgnoreCase(GatewayCode.YesBank))
                {
                    statusCheckResponse = StatusCheckYesBank(objmodel.OrderID, objmodel.Amount, objmodel.UserID, out failureMessage);

                    if (statusCheckResponse.Status.EqualsIgnoreCase(StatusType.Process))// some user did upi txn later
                    {
                        RandomNumberProcessing.RandomDelay(3200, 5800);

                        statusCheckResponse = StatusCheckYesBank(objmodel.OrderID, objmodel.Amount, objmodel.UserID, out failureMessage);
                        if (statusCheckResponse.Status.EqualsIgnoreCase(StatusType.Process))// some user did upi txn later
                        {
                            failureIfOldPending(statusCheckResponse, objmodel, viewModel, 60);

                            if (statusCheckResponse.Status.EqualsIgnoreCase(StatusType.Process))// some user did upi txn later
                            {
                                failureMessage = "Transaction under Process";
                                return failureMessage;
                            }
                        }
                    }
                }
                else if (viewModel.Name.EqualsIgnoreCase(GatewayCode.RazorpayQR))
                {
                    statusCheckResponse = StatusCheckRozarPayQR(objmodel.OrderID, objmodel.Amount, objmodel.UserID, objmodel.DOC, out failureMessage);

                    if (statusCheckResponse.Status.EqualsIgnoreCase(StatusType.Process))// some user did upi txn later
                    {
                        RandomNumberProcessing.RandomDelay(3200, 5800);
                        statusCheckResponse = StatusCheckRozarPayQR(objmodel.OrderID, objmodel.Amount, objmodel.UserID, objmodel.DOC, out failureMessage);

                        if (statusCheckResponse.Status.EqualsIgnoreCase(StatusType.Process))// some user did upi txn later
                        {
                            failureMessage = "Transaction under Process";
                            return failureMessage;
                        }
                    }
                }
                else if (viewModel.Name.EqualsIgnoreCase(GatewayCode.WorldLine))
                {
                    statusCheckResponse = StatusCheckWorldLine(mainRequest.TransactionID, objmodel.Amount, objmodel.UserID, objmodel.DOC, out failureMessage);

                    if (statusCheckResponse.Status.EqualsIgnoreCase(StatusType.Process))// some user did upi txn later
                    {
                        RandomNumberProcessing.RandomDelay(3200, 5800);
                        statusCheckResponse = StatusCheckWorldLine(mainRequest.TransactionID, objmodel.Amount, objmodel.UserID, objmodel.DOC, out failureMessage);

                        if (statusCheckResponse.Status.EqualsIgnoreCase(StatusType.Process))// some user did upi txn later
                        {
                            failureMessage = "Transaction under Process";
                            return failureMessage;
                        }
                    }
                }
                 

                if (DBAmount != (statusCheckResponse.Amount) && statusCheckResponse.Status.EqualsIgnoreCase(StatusType.Success))
                {
                    failureMessage = "Invalid Payment";
                    return failureMessage;
                }

                objmodel.Sender = statusCheckResponse.Sender;
                objmodel.TransactionID = statusCheckResponse.TransactionID;
                objmodel.Status = statusCheckResponse.Status;
                objmodel.Response = statusCheckResponse.Message;
                objmodel.PayMode = getPaymode(statusCheckResponse.Paymode);

                if (objmodel.TransactionID.TrimIfNotNull().Length <= 1 && statusCheckResponse.Status.EqualsIgnoreCase(StatusType.Success))
                {
                    objmodel.Status = StatusType.Process;
                }

                if (new ResponseProcess().CheckFailure(objmodel.Status))
                {
                    objmodel.Remark = $"FAILED: Payment Gateway against BankRRN : {objmodel.TransactionID} for amount {objmodel.Amount.TwoDigitDecimal()}";
                }
                else
                {
                    objmodel.Remark = $"Payment Gateway Transfer against BankRRN : {objmodel.TransactionID} for amount {objmodel.Amount.TwoDigitDecimal()}";
                }
                if (objmodel.Status.EqualsIgnoreCase(StatusType.Process))
                {
                    failureMessage = "Transaction under Process";
                    return failureMessage;
                }
                //For PG Settlement
                if (!new PaymentGatewayBAL().IsValidForSettlement(objmodel.OrderID, objmodel.TransactionID, objmodel.Status))
                {
                    return "Transaction under Process";
                }

                try
                {
                    rowsAffected = new PaymentGatewayBAL().UpdateCore(objmodel);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                    failureMessage = "Transaction under Process";
                    return failureMessage;
                }

                if (rowsAffected <= 0)
                {
                    return failureMessage;
                }

                if (new ResponseProcess().CheckFailure(objmodel.Status))
                {
                    return failureMessage;
                }

                if (new ResponseProcess().CheckSuccess(objmodel.Status))
                {
                    PaymentGatewayProcessing objProcessing = new PaymentGatewayProcessing();
                    failureMessage = objProcessing.BalanceTransferAndCommissionProcessing(objmodel, checkDuplicate: false);
                }

                failureMessage = StatusType.Ok;
                return failureMessage;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return failureMessage;
            }
        }

        private StatusCheckResponse StatusCheckWorldLine(string OrderID, decimal Amount, int UserID, DateTime dateOfTxn, out string failureMessage)
        {
            failureMessage = "Invalid Payment";
            StatusCheckResponse statusCheckResponse = new StatusCheckResponse();
            try
            {
                ViewModel viewModel = null;
                try
                {
                    viewModel = new PaymentGatewaySettingDal().SelectPaymentGatewayByName(GatewayCode.WorldLine);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }

                WorldLineStatusCheckRequest worldLineStatusCheckRequest = new WorldLineStatusCheckRequest();
                Requests.PaymentGateway.Merchant merchant = new()
                {
                    identifier = viewModel.MerchantID
                };

                Requests.PaymentGateway.Transaction transaction = new()
                {
                    dateTime = dateOfTxn.LongFormatDate(),
                    token = OrderID
                };

                worldLineStatusCheckRequest.merchant = merchant;
                worldLineStatusCheckRequest.transaction = transaction;

                string request = JsonConvert.SerializeObject(worldLineStatusCheckRequest);
                string URLString = "https://www.paynimo.com/api/paynimoV2.req";

                string Response = HttpProcessing.Worldline_PostMethod(URLString, request);

                WorldLineStatusCheckResponse obj = new WorldLineStatusCheckResponse();
                obj = JsonConvert.DeserializeObject<WorldLineStatusCheckResponse>(Response.Trim());

                /*PaymentGateway status check log added*/
                PaymentGatewayLog paymentGatewayLog = new PaymentGatewayLog();
                paymentGatewayLog.Amount = Amount;
                paymentGatewayLog.UserID = UserID;
                paymentGatewayLog.Request = request;
                paymentGatewayLog.Response = Response;
                paymentGatewayLog.Type = "StatusCheck";
                paymentGatewayLog.OrderID = OrderID;
                paymentGatewayLog.GatewayName = GatewayCode.WorldLine;
                new PaymentGatewayLogDAL().Insert(paymentGatewayLog);
                /*------------------*/

                if (obj == null)
                {
                    statusCheckResponse.Amount = Amount;
                    statusCheckResponse.Message = failureMessage;
                    statusCheckResponse.OrderID = OrderID;
                    statusCheckResponse.Paymode = "";
                    statusCheckResponse.TransactionID = "";
                    statusCheckResponse.Status = StatusType.Process;
                    return statusCheckResponse;
                }

                if (obj.paymentMethod.paymentTransaction == null)
                {
                    statusCheckResponse.Amount = Amount;
                    statusCheckResponse.Message = "Order not found";
                    statusCheckResponse.OrderID = OrderID;
                    statusCheckResponse.Paymode = "";
                    statusCheckResponse.TransactionID = "";
                    statusCheckResponse.Status = getStatusBasedOnTime(dateOfTxn);// if txn is older than 30 min and order data not available, then fail
                    return statusCheckResponse;
                }

                string transactionId = "";
                string payMode = "";
                string requid = "";
                int statusCheckAmount = 0;

                string pgStatus = StatusType.Process;

                if (obj.paymentMethod.paymentTransaction.statusCode.EqualsIgnoreCase("0300"))
                {
                    pgStatus = StatusType.Success;
                }

                if (obj.paymentMethod.paymentTransaction.statusCode.EqualsIgnoreCase("0399"))
                {
                    pgStatus = StatusType.Failure;
                }
                try
                {
                    statusCheckAmount = Convert.ToInt32(Convert.ToDecimal(obj.paymentMethod.paymentTransaction.amount));
                }
                catch
                {
                    statusCheckAmount = 0;
                }

                if (!obj.paymentMethod.paymentTransaction.identifier.EqualsIgnoreCase(OrderID))
                {
                    requid = StatusType.Process;
                }

                transactionId = obj.paymentMethod.paymentTransaction.identifier;

                if (obj.paymentMethod.paymentMode != null) payMode = obj.paymentMethod.paymentMode;

                statusCheckResponse.Amount = statusCheckAmount;
                statusCheckResponse.Message = "Status updated Successfully";
                statusCheckResponse.OrderID = OrderID;
                statusCheckResponse.Paymode = payMode;
                statusCheckResponse.TransactionID = transactionId;
                statusCheckResponse.Status = pgStatus;
                failureMessage = StatusType.Ok;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return statusCheckResponse;
            }
            return statusCheckResponse;
        }

        private string getPaymode(string paymentMode)
        {
            if (paymentMode.EqualsIgnoreCase("CC"))
            {
                return "Credit Card";
            }

            if (paymentMode.EqualsIgnoreCase("DC"))
            {
                return "Debit Card";
            }

            if (paymentMode.EqualsIgnoreCase("NB"))
            {
                return "Net Banking";
            }

            if (paymentMode.EqualsIgnoreCase("WL"))
            {
                return "Wallets";
            }

            if (paymentMode.EqualsIgnoreCase("UP"))
            {
                return "UPI";
            }

            return paymentMode;
        }

        private bool IsValidSettingForfailureIfOldPending(ViewModel viewModel)
        {
            if (viewModel.Name.EqualsIgnoreCase(GatewayCode.ICICIQR))
            {
                return true;
            }

            if (viewModel.Name.EqualsIgnoreCase(GatewayCode.CashFree))
            {
                return true;
            }

            if (viewModel.Name.EqualsIgnoreCase(GatewayCode.YesBank))
            {
                return true;
            }
            if (viewModel.Name.EqualsIgnoreCase(GatewayCode.PayU))
            {
                return true;
            }

            if (viewModel.Name.EqualsIgnoreCase(GatewayCode.PhonePay))
            {
                return true;
            }

            return false;
        }

        private void failureIfOldPending(StatusCheckResponse sts, PaymentGateway objmodel, ViewModel viewModel, int Minute = 60)
        {
            if (!sts.Status.EqualsIgnoreCase(StatusType.Process))
            {
                return;
            }

            if (!IsValidSettingForfailureIfOldPending(viewModel))
            {
                return;
            }

            double minutes = (DateTime.Now - objmodel.DOC).TotalMinutes;

            if (minutes >= Minute)// if status is pending and txn older above 60 minutes then fail
            {
                sts.Status = StatusType.Failure;
            }
        }

        internal PaymentGatewayDetailViewModel TransactionDetail(TransactionStatusRequest mainRequest, int userID, out string failureMessage)
        {
            failureMessage = mainRequest.IsValidModalState();

            PaymentGatewayDetailViewModel objresult = new PaymentGatewayDetailViewModel();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return objresult;

            PaymentGateway objmodelpg = new PaymentGatewayDAL().SelectTransactionByOrderID(mainRequest.OrderID);

            if (objmodelpg == null)
            {
                failureMessage = "Invalid data";
                return objresult;
            }

            ViewModel viewModel = null;
            try
            {
                viewModel = new PaymentGatewaySettingDal().SelectByPGSettingID(objmodelpg.PaymentProviderID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (viewModel != null &&
                (viewModel.Name.EqualsIgnoreCase(GatewayCode.Razorpay) ||
                viewModel.Name.EqualsIgnoreCase(GatewayCode.CashFree))) // because Razor, cashfree not send callback to server, in case of Mobile app
            {
                TransactionStatus(mainRequest, userID, out failureMessage);
            }
            else
            {
                RandomNumberProcessing.RandomDelay(1000, 1500);
            }

            PaymentGatewayViewModel objmodel = new PaymentGatewayDAL().SelectByOrderID(mainRequest.OrderID);

            if (objmodel == null)
            {
                failureMessage = "Invalid data";
                return objresult;
            }

            if (objmodel.UserID != userID)
            {
                failureMessage = "Invalid data";
                return objresult;
            }

            objresult.Amount = objmodel.Amount;
            objresult.ClosingBalance = objmodel.ClosingBalance;

            objresult.OpeningBalance = objmodel.OpeningBalance;
            objresult.Commission = objmodel.Commission;

            objresult.DisplayDOC = objmodel.DisplayDOC;
            objresult.OrderID = objmodel.OrderID;

            objresult.PayMode = objmodel.PayMode;

            objresult.Status = objmodel.Status;

            objresult.TransactionID = objmodel.TransactionID;

            objresult.WalletName = objmodel.WalletName;

            failureMessage = StatusType.Ok;
            return objresult;
        }
        public StatusCheckResponse StatusCheckYesBank(string OrderID, decimal Amount, int UserID, out string failureMessage)
        {
            failureMessage = "Invalid Payment";
            StatusCheckResponse statusCheckResponse = new StatusCheckResponse();

            try
            {
                ViewModel viewModel = null;
                try
                {
                    viewModel = new PaymentGatewaySettingDal().SelectPaymentGatewayByName(GatewayCode.YesBank);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }

                if (viewModel == null)
                {
                    statusCheckResponse.Amount = Amount;
                    statusCheckResponse.Message = failureMessage;
                    statusCheckResponse.OrderID = OrderID;
                    statusCheckResponse.Paymode = "";
                    statusCheckResponse.TransactionID = "";
                    statusCheckResponse.Status = StatusType.Process;
                    return statusCheckResponse;
                }

                Processing.YesBank.PaymentGatewayProcessing pgProcessing = new Processing.YesBank.PaymentGatewayProcessing();

                YESBankCore.Response.StatusCheckResponse response = pgProcessing.TranactionStatus(OrderID, Amount, UserID);

                if (response == null)
                {
                    statusCheckResponse.Amount = Amount;
                    statusCheckResponse.Message = failureMessage;
                    statusCheckResponse.OrderID = OrderID;
                    statusCheckResponse.Paymode = "UPI";
                    statusCheckResponse.TransactionID = "";
                    statusCheckResponse.Status = StatusType.Process;
                    return statusCheckResponse;
                }

                string transactionId = "";
                string payMode = "UPI";
                int statusCheckAmount = 0;
                string pgStatus = "";

                pgStatus = response.Status;
                transactionId = response.TransactionID;
                string senderVPA = response.CustomerVPA;

                if (pgStatus.EqualsIgnoreCase("SUCCESS"))
                {
                    pgStatus = StatusType.Success;
                }
                else if (pgStatus.EqualsIgnoreCase("FAILURE"))
                {
                    transactionId = "";
                    pgStatus = StatusType.Failure;
                }
                else
                {
                    pgStatus = StatusType.Process;
                }

                //statusCheckAmount = GetIntegerAmount(response.Amount);

                statusCheckResponse.Sender = senderVPA;
                statusCheckResponse.Amount = Amount;
                statusCheckResponse.Message = "Status updated Successfully";
                statusCheckResponse.OrderID = OrderID;
                statusCheckResponse.Paymode = payMode;
                statusCheckResponse.TransactionID = transactionId;
                statusCheckResponse.Status = pgStatus;
                failureMessage = StatusType.Ok;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return statusCheckResponse;
            }
            return statusCheckResponse;
        }
        
        public StatusCheckResponse StatusCheckRozarPay(string OrderID, decimal Amount, int UserID, DateTime dateOfTxn, out string failureMessage)
        {
            failureMessage = "Invalid Payment";
            StatusCheckResponse statusCheckResponse = new StatusCheckResponse();
            try
            {
                ViewModel viewModel = null;
                try
                {
                    viewModel = new PaymentGatewaySettingDal().SelectPaymentGatewayByName(GatewayCode.Razorpay);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }

                string Response = HttpProcessing.Razorpay_OrderCheck(OrderID, viewModel.MerchantID, viewModel.Token);

                RazorpayStatusCheckResponse obj = new RazorpayStatusCheckResponse();
                obj = JsonConvert.DeserializeObject<RazorpayStatusCheckResponse>(Response);

                /*PaymentGateway status check log added*/
                PaymentGatewayLog paymentGatewayLog = new PaymentGatewayLog();
                paymentGatewayLog.Amount = Amount;
                paymentGatewayLog.UserID = UserID;
                paymentGatewayLog.Request = OrderID;
                paymentGatewayLog.Response = Response;
                paymentGatewayLog.Type = "StatusCheck";
                paymentGatewayLog.OrderID = OrderID;
                paymentGatewayLog.GatewayName = GatewayCode.Razorpay;
                new PaymentGatewayLogDAL().Insert(paymentGatewayLog);
                /*------------------*/

                if (obj == null)
                {
                    statusCheckResponse.Amount = Amount;
                    statusCheckResponse.Message = failureMessage;
                    statusCheckResponse.OrderID = OrderID;
                    statusCheckResponse.Paymode = "";
                    statusCheckResponse.TransactionID = "";
                    statusCheckResponse.Status = StatusType.Process;
                    return statusCheckResponse;
                }

                if (obj.items.Count <= 0)
                {
                    statusCheckResponse.Amount = Amount;
                    statusCheckResponse.Message = "Order not found";
                    statusCheckResponse.OrderID = OrderID;
                    statusCheckResponse.Paymode = "";
                    statusCheckResponse.TransactionID = "";
                    statusCheckResponse.Status = getStatusBasedOnTime(dateOfTxn);// if txn is older than 30 min and order data not available, then fail
                    return statusCheckResponse;
                }

                string transactionId = "";
                string payMode = "";
                int statusCheckAmount = 0;
                string pgStatus = "";
                int cnt = obj.items.Count;

                RazorpayStatusCheckItem stsitem = obj.items[cnt - 1];

                if (!stsitem.entity.EqualsIgnoreCase("payment"))
                {
                    statusCheckResponse.Amount = Amount;
                    statusCheckResponse.Message = failureMessage;
                    statusCheckResponse.OrderID = OrderID;
                    statusCheckResponse.Paymode = "";
                    statusCheckResponse.TransactionID = "";
                    statusCheckResponse.Status = StatusType.Process;
                    return statusCheckResponse;
                }

                pgStatus = stsitem.status;
                statusCheckAmount = stsitem.amount / 100;
                transactionId = stsitem.acquirer_data.rrn;
                payMode = stsitem.method;

                payMode = new PaymentGatewayProcessing().getPayMode(payMode, stsitem.id, viewModel.MerchantID, viewModel.Token);
                if (transactionId == null)
                {
                    transactionId = stsitem.acquirer_data.auth_code;
                }
                if (transactionId == null)
                {
                    transactionId = stsitem.acquirer_data.bank_transaction_id;
                }
                if (pgStatus.EqualsIgnoreCase("captured") || pgStatus.EqualsIgnoreCase("authorized"))
                {
                    pgStatus = StatusType.Success;
                }
                else if (pgStatus.EqualsIgnoreCase("failed"))
                {
                    transactionId = getDescriptionBasedOnTime(dateOfTxn, stsitem.error_description);
                    pgStatus = StatusType.Failure;
                }
                else
                {
                    pgStatus = StatusType.Process;
                }

                statusCheckResponse.Amount = statusCheckAmount;
                statusCheckResponse.Message = "Status updated Successfully";
                statusCheckResponse.OrderID = OrderID;
                statusCheckResponse.Paymode = payMode;
                statusCheckResponse.TransactionID = transactionId;
                statusCheckResponse.Status = pgStatus;
                failureMessage = StatusType.Ok;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return statusCheckResponse;
            }
            return statusCheckResponse;
        }

        public StatusCheckResponse StatusCheckRozarPayQR(string OrderID, decimal Amount, int UserID, DateTime dateOfTxn, out string failureMessage)
        {
            failureMessage = "Invalid Payment";
            StatusCheckResponse statusCheckResponse = new StatusCheckResponse();
            try
            {
                ViewModel viewModel = null;
                try
                {
                    viewModel = new PaymentGatewaySettingDal().SelectPaymentGatewayByName(GatewayCode.RazorpayQR);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }

                string URL = "https://api.razorpay.com/v1/payments/qr_codes/[qr_id]/payments?count=2";

                URL = URL.Replace("[qr_id]", OrderID);

                string Response = HttpProcessing.RazorpayQR_StatusCheck(URL, viewModel.MerchantID, viewModel.Token);

                RazorpayQRStatusCheckResponse obj = new RazorpayQRStatusCheckResponse();
                obj = JsonConvert.DeserializeObject<RazorpayQRStatusCheckResponse>(Response);

                /*PaymentGateway status check log added*/
                PaymentGatewayLog paymentGatewayLog = new PaymentGatewayLog();
                paymentGatewayLog.Amount = Amount;
                paymentGatewayLog.UserID = UserID;
                paymentGatewayLog.Request = OrderID;
                paymentGatewayLog.Response = Response;
                paymentGatewayLog.Type = "UpdateStatus";
                paymentGatewayLog.OrderID = OrderID;
                paymentGatewayLog.GatewayName = GatewayCode.RazorpayQR;
                new PaymentGatewayLogDAL().Insert(paymentGatewayLog);
                /*------------------*/

                if (obj == null)
                {
                    statusCheckResponse.Amount = Amount;
                    statusCheckResponse.Message = failureMessage;
                    statusCheckResponse.OrderID = OrderID;
                    statusCheckResponse.Paymode = "";
                    statusCheckResponse.TransactionID = "";
                    statusCheckResponse.Status = StatusType.Process;
                    return statusCheckResponse;
                }

                if (obj.items.Count <= 0)
                {
                    statusCheckResponse.Amount = Amount;
                    statusCheckResponse.Message = "Order not found";
                    statusCheckResponse.OrderID = OrderID;
                    statusCheckResponse.Paymode = "";
                    statusCheckResponse.TransactionID = "";
                    statusCheckResponse.Status = getStatusBasedOnTime(dateOfTxn);// if txn is older than 30 min and order data not available, then fail
                    return statusCheckResponse;
                }

                string transactionId = "";
                string payMode = "";
                int statusCheckAmount = 0;
                string pgStatus = "";
                int cnt = obj.items.Count;

                Item stsitem = obj.items[cnt - 1];

                if (!stsitem.entity.EqualsIgnoreCase("payment"))
                {
                    statusCheckResponse.Amount = Amount;
                    statusCheckResponse.Message = failureMessage;
                    statusCheckResponse.OrderID = OrderID;
                    statusCheckResponse.Paymode = "";
                    statusCheckResponse.TransactionID = "";
                    statusCheckResponse.Status = StatusType.Process;
                    return statusCheckResponse;
                }

                pgStatus = stsitem.status;
                statusCheckAmount = stsitem.amount / 100;
                transactionId = stsitem?.acquirer_data?.rrn;
                payMode = stsitem.method;

                payMode = new PaymentGatewayProcessing().getPayMode(payMode, stsitem.id, viewModel.MerchantID, viewModel.Token);

                if (transactionId == null)
                {
                    transactionId = "";
                }
                if (pgStatus.EqualsIgnoreCase("captured") || pgStatus.EqualsIgnoreCase("authorized"))
                {
                    pgStatus = StatusType.Success;
                }
                else if (pgStatus.EqualsIgnoreCase("failed"))
                {
                    transactionId = getDescriptionBasedOnTime(dateOfTxn, stsitem.error_description);
                    pgStatus = StatusType.Failure;
                }
                else
                {
                    pgStatus = StatusType.Process;
                }

                statusCheckResponse.Amount = statusCheckAmount;
                statusCheckResponse.Message = "Status updated Successfully";
                statusCheckResponse.OrderID = OrderID;
                statusCheckResponse.Paymode = payMode;
                statusCheckResponse.TransactionID = transactionId;
                statusCheckResponse.Sender = stsitem?.vpa;
                statusCheckResponse.Status = pgStatus;
                failureMessage = StatusType.Ok;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return statusCheckResponse;
            }
            return statusCheckResponse;
        }


        private bool IsValidStatusCheckGateway(string GatewayName)
        {
            return GatewayCode.validGatewayNames.Contains(GatewayName);
        }

        private bool IsValidUpdateStatusManually(string GatewayName, bool IsManualStatusChange)
        {
            if (GatewayName.EqualsIgnoreCase(GatewayCode.FreeUPI))
            {
                return true;
            }
            if (GatewayName.EqualsIgnoreCase(GatewayCode.OpenBank))
            {
                return true;
            }
            if (IsManualStatusChange)
            {
                return true;
            }

            return false;
        }

        internal ReportWrapper Select(SelectRequest selectRequest, int userID, out string failureMessage)
        {
            ReportWrapper wrapper = new ReportWrapper();
            List<PaymentGatewayViewModel> response = new List<PaymentGatewayViewModel>();
            int UserTypeID = new UserDAL().SelectUserTypeID(userID);

            if (UserTypeID == (int)UserType.DFOS || UserTypeID == (int)UserType.SDFOS)
            {
                int ParentID = new UserDAL().SelectParentID(userID);
                userID = ParentID;
            }

            failureMessage = selectRequest.IsValidModalState();
            isExcelOrPDF(selectRequest);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;

            try
            {
                response = new PaymentGatewayDAL().SelectReport(selectRequest, userID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            failureMessage = "No data found";
            if (response.Count <= 0)
                return wrapper;
            response = response.AsQueryable().WhereDynamic(selectRequest.CriteriaText).ToList();
            wrapper.Count = response.Count;

            if (selectRequest.SkipCount <= 0 && selectRequest.IsExcelDownload == false)
            {
                wrapper.HeaderSummary = new PaymentGatewayDAL().GetHeaderSummaryData(response);
            }
            if (!Util.IsDownload(selectRequest.IsExcelDownload, selectRequest.IsPDFDownload))
            {
                response = response.AsQueryable().SkipAndTakeCount(selectRequest.SkipCount, selectRequest.TakeCount).ToList();
            }

            if (UserTypeID != (int)UserType.Admin)
            {
                foreach (PaymentGatewayViewModel item in response)
                {
                    item.ProviderName = "";
                }
            }
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Math.Max(3 * Environment.ProcessorCount / 4, 1)
            };
            Parallel.ForEach(response, parallelOptions, res =>
            {
                res.SuperDistributorName = new UserDAL().SelectFirmNameByUserID(res.SuperDistributerUserID);
                res.DistributorName = new UserDAL().SelectFirmNameByUserID(res.DistributerUserID);
            });

            if (Util.IsDownload(selectRequest.IsExcelDownload, selectRequest.IsPDFDownload))
            {
                wrapper.IsDownload = true;
            }

            wrapper.list = response;

            failureMessage = StatusType.Ok;
            return wrapper;
        }

        internal static bool isExcelOrPDF(Requests.PaymentGateway.SelectRequest request)
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
        internal PaymentGatewayViewModel SelectByOrderID(SelectOrderIDRequest mainRequest, int userID, out string failureMessage)
        {
            PaymentGatewayViewModel view = new PaymentGatewayViewModel();

            failureMessage = mainRequest.IsValidModalState();

            try
            {
                view = new PaymentGatewayDAL().SelectByOrderID(mainRequest.OrderID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (view == null)
                return view;

            failureMessage = StatusType.Ok;
            return view;
        }

        public string Generatehash512(string text)
        {
            byte[] message = Encoding.UTF8.GetBytes(text);

            UnicodeEncoding UE = new UnicodeEncoding();
            byte[] hashValue;
            SHA512Managed hashString = new SHA512Managed();
            string hex = "";
            hashValue = hashString.ComputeHash(message);
            foreach (byte x in hashValue)
            {
                hex += String.Format("{0:x2}", x);
            }
            return hex;
        }

        private string CheckForDelayBetweenTwoPaymentGateway(int UserID)
        {
            PaymentGatewayViewModel viewModel = new PaymentGatewayDAL().SelectInfoByUserID(UserID);

            if (viewModel == null)
                return StatusType.Ok;
            int PGTransactionDelayInMintute = new SettingMasterBal().SelectByKeyWithIntegerDefaultValue(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.PGTransactionDelayInMintute), 5);

            if (PGTransactionDelayInMintute <= 1)
            {
                PGTransactionDelayInMintute = 2;
            }

            DateTime PreDate = viewModel.DOC;
            DateTime currDate = DateTime.Now;

            double diffInMin = (currDate - PreDate).TotalMinutes;
            if (diffInMin <= PGTransactionDelayInMintute)
                return $"Transaction within {PGTransactionDelayInMintute} minutes not allowed";

            return StatusType.Ok;
        }

        private string getStatusBasedOnTime(DateTime dateOfTxn)
        {
            DateTime currDate = DateTime.Now;

            double diffInMin = (currDate - dateOfTxn).TotalMinutes;
            if (diffInMin <= 60)
                return StatusType.Process;

            return StatusType.Failure;
        }

        private string getDescriptionBasedOnTime(DateTime dateOfTxn, string description)
        {
            DateTime currDate = DateTime.Now;

            double diffInMin = (currDate - dateOfTxn).TotalMinutes;
            if (diffInMin <= 60)
                return description;

            string newdesc = "";

            if (description.Length >= 21)
            {
                newdesc = description.Substring(0, 15) + "...";
            }

            return newdesc;
        }

        internal List<Response.TransactionMaster.CommissionViewModel> SelectRoleWithCommission(CommissionRequest selectRequest, out string failureMessage)
        {
            List<Response.TransactionMaster.CommissionViewModel> report = null;
            failureMessage = selectRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return report;

            failureMessage = Messages.DataNotFound;
            try
            {
                report = new PaymentGatewayDAL().SelectRoleWithCommission(selectRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (report == null || report.Count <= 0) return report;
            foreach (Response.TransactionMaster.CommissionViewModel item in report)
            {
                if (item.DistributorID == Company.AdminUserID)
                {
                    item.DistributorName = "Admin";
                    item.SuperDistributorName = "Admin";
                    item.UserType = "Admin";
                }
                if (item.SuperDistributorID == Company.AdminUserID)
                {
                    item.UserType = "Admin";
                    item.SuperDistributorName = "Admin";
                }
            }
            failureMessage = StatusType.Ok;
            return report;
        }

        internal string isValidForBulkStatusCheck(PaymentGatewayPendingViewModel transactionMaster)
        {
            if (!new ResponseProcess().checkProcess(transactionMaster.Status))// not in process
                return TransactionMessage.NotInProcess;

            if (new ResponseProcess().CheckFailure(transactionMaster.Status)) //already fail Refund
                return TransactionMessage.AllreadyFail;

            if (new ResponseProcess().CheckSuccess(transactionMaster.Status))
                return TransactionMessage.AllreadySucess;

            DateTime rchargeDate = transactionMaster.DOC;

            DateTime currDate = DateTime.Now;

            double diffInSec = (currDate - rchargeDate).TotalSeconds;

            if (diffInSec <= 180)
            {
                return "Transaction should 180 seconds older";
            }

            return StatusType.Ok;
        }

        public void PGStatusCheckAuto()
        {
            try
            {
                bool isDevelopment = new Util().isDevelopment();
                if (isDevelopment == true)
                {
                    return;
                }

                string failureMessage = new Validation().ServerDownTime();

                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                {
                    return; // "At ServerDownTime, status check not allowed";
                }

                failureMessage = new Validation().BankDownTime();

                if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                {
                    return; // "At BankDownTime, mostly bank down at night
                }

                List<PaymentGatewayPendingViewModel> list = null;
                try
                {
                    list = new PaymentGatewayDAL().SelectPendingPGTransactionForStatusCheck();
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                    return;
                }

                if (list == null)
                    return;

                if (list.Count <= 0)
                    return;

                List<PaymentGatewayPendingViewModel> random100 = list.OrderBy(x => Guid.NewGuid()).Take(50).ToList();

                foreach (PaymentGatewayPendingViewModel item in random100)
                {
                    double seconds = DateTimeExtensions.DifferenceInSeconds(item.DOC);

                    double ExpectedSecond = 180.0D; //3 min = 3 * 60

                    if (seconds <= ExpectedSecond)
                    {
                        return;
                    }

                    TransactionStatusRequest mainRequest = new TransactionStatusRequest();
                    mainRequest.OrderID = item.OrderID;

                    string faileureMessage = isValidForBulkStatusCheck(item);

                    if (!faileureMessage.EqualsIgnoreCase(StatusType.Ok))
                        return;

                    Task.Run(() => TransactionStatus(mainRequest, item.UserID, out string failureMessage));
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
        }
    }
}