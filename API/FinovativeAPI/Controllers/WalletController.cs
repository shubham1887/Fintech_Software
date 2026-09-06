using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Project.BAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests;
using Project.Requests.Wallet;
using Project.Response;
using Project.Response.Wallet;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.Controllers
{
    [CustomAuthorization]
    [Route("[controller]/[action]")]
    [EnableCors("CORSProject")]
    [ApiController]
    public class WalletController : ControllerBase
    {
        private readonly EncryptDecrypt ed = new EncryptDecrypt();

        [HttpPost]
        public APIResponse Select([FromForm] APIRequest aPIRequest)
        {
            List<WalletMaster> wallets;
            try
            {
                wallets = new WalletBal().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(wallets));
        }

        [HttpPost]
        public APIResponse SelectForPaymentGateway([FromForm] APIRequest aPIRequest)
        {
            List<WalletMaster> wallets;
            try
            {
                wallets = new WalletBal().SelectForPaymentGateway();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(wallets));
        }

        [HttpPost]
        public APIResponse SelectForFundTransfer([FromForm] APIRequest aPIRequest)
        {
            List<WalletMaster> wallets;
            try
            {
                wallets = new WalletBal().SelectForFundTransfer();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(wallets));
        }

        [HttpPost]
        public APIResponse SelectByWalletIDAndReceiverID([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            SelectRequest mainRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                mainRequest = JsonConvert.DeserializeObject<SelectRequest>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }

            Response.Wallet.ViewModel wallet;
            string failureMessage;
            try
            {
                wallet = new UserWalletMasterBal().SelectByWalletIDAndReceiverID(keyDataModel.UserID, mainRequest, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            if (wallet == null)
            {
                return ResponseProcess.GetFailReason(WalletMessage.Invalid);
            }

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(wallet));
        }

        [HttpPost]
        public APIResponse SelectByUserID([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;

            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }

            List<DetailViewModel> wallet;
            string failureMessage;
            try
            {
                wallet = new UserWalletMasterBal().SelectByUserID(keyDataModel.UserID, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            if (wallet == null)
                return ResponseProcess.GetFailReason(WalletMessage.Invalid);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(wallet));
        }

        [HttpPost]
        public APIResponse SelectRechargeAndDMT([FromForm] APIRequest aPIRequest)
        {
            List<WalletMaster> wallets;
            try
            {
                wallets = new WalletBal().SelectRechargeAndDMT();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(wallets));
        }

        [HttpPost]
        public APIResponse SelectForStaticQR([FromForm] APIRequest aPIRequest)
        {
            List<WalletMaster> wallets;
            try
            {
                wallets = new WalletBal().SelectForStaticQR();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(wallets));
        }

        [HttpPost]
        public APIResponse SelectForeCollection([FromForm] APIRequest aPIRequest)
        {
            List<WalletMaster> wallets;
            try
            {
                wallets = new WalletBal().SelectForeCollection();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(wallets));
        }

        [HttpPost]
        public APIResponse SelectForDashboardByUserID([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;
            Requests.TodaySummary.SelectRequest selectRequest;
            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
                selectRequest = JsonConvert.DeserializeObject<Requests.TodaySummary.SelectRequest>(ed.Decrypt(aPIRequest.Request));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }

            List<UserWalletBalanceViewModel> wallet;
            string failureMessage;
            try
            {
                wallet = new UserWalletMasterBal().SelectForDashboardByUserID(keyDataModel.UserID, selectRequest.Date, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            if (wallet == null)
                return ResponseProcess.GetFailReason(WalletMessage.Invalid);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(wallet));
        }

        [HttpPost]
        public APIResponse SelectServiceWiseWallet([FromForm] APIRequest aPIRequest)
        {
            KeyDataModel keyDataModel;

            try
            {
                keyDataModel = JsonConvert.DeserializeObject<KeyDataModel>(ed.Decrypt(aPIRequest.KeyData));
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.InvalidUserInput);
            }

            List<ServicesWiseWalletViewModel> wallet;
            string failureMessage;
            try
            {
                wallet = new WalletServiceMappingBAL().SelectServiceWiseWallet(out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ResponseProcess.GetFailReason(failureMessage);

            if (wallet == null)
                return ResponseProcess.GetFailReason(WalletMessage.Invalid);

            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(wallet));
        }

        [HttpPost]
        public APIResponse SelectForUserCreation([FromForm] APIRequest aPIRequest)
        {
            List<WalletMaster> wallets;
            try
            {
                wallets = new WalletBal().SelectForUserCreation();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResponseProcess.GetFailReason(Messages.ExceptionOccured);
            }
            return ResponseProcess.GetSucessReason(JsonConvert.SerializeObject(wallets));
        }
    }
}