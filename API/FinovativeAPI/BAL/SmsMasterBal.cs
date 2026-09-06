using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.SMSMaster;
using Project.Response.SmsMaster;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class SmsMasterBal
    {
        internal string Insert(InsertRequest insertRequest, out string failureMessage)
        {
            string response = string.Empty;
            failureMessage = insertRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            SMSMaster ObjModel = new SMSMaster
            {
                SMSString = insertRequest.SMSString,
                SMSOTPURL = insertRequest.SMSOTPURL,
                BalanceURL = insertRequest.BalanceURL,
                APIName = insertRequest.APIName,
                WhatsAppURL = insertRequest.WhatsAppURL,
                MethodType = insertRequest.MethodType,
                HeaderJson = insertRequest.HeaderJson,
                MethodTypeWhatsApp = insertRequest.MethodTypeWhatsApp,
                HeaderJsonWhatsApp = insertRequest.HeaderJsonWhatsApp,
                MobileWithOrWithout91 = insertRequest.MobileWithOrWithout91,
            };
            SMSMaster model = null;
            try
            {
                model = new SmsMasterDal().SelectByID(insertRequest.SMSMasterID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            int rowsAffected;
            if (model == null)
            {
                try
                {
                    response = SMSMasterMessage.InsertFail;
                    rowsAffected = new SmsMasterDal().Insert(ObjModel);
                    if (rowsAffected > 0)
                        response = SMSMasterMessage.InsertSuccess;
                    else
                        failureMessage = response;
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                    return Messages.ExceptionOccured;
                }
            }
            else
            {
                response = SMSMasterMessage.UpdateFail;
                model.SMSString = insertRequest.SMSString;
                model.SMSOTPURL = insertRequest.SMSOTPURL;
                model.BalanceURL = insertRequest.BalanceURL;
                model.APIName = insertRequest.APIName;
                model.WhatsAppURL = insertRequest.WhatsAppURL;
                model.MethodType = insertRequest.MethodType;
                model.HeaderJson = insertRequest.HeaderJson;
                model.MethodTypeWhatsApp = insertRequest.MethodTypeWhatsApp;
                model.HeaderJsonWhatsApp = insertRequest.HeaderJsonWhatsApp;
                model.MobileWithOrWithout91 = insertRequest.MobileWithOrWithout91;
                try
                {
                    rowsAffected = new SmsMasterDal().Update(model);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                    return response;
                }
                if (rowsAffected > 0)
                    if (rowsAffected > 0)
                        response = SMSMasterMessage.UpdateSuccess;
                    else
                        failureMessage = response;
            }
            failureMessage = StatusType.Ok;
            return response;
        }

        internal List<ViewModel> Select()
        {
            List<ViewModel> viewModel = new List<ViewModel>();
            try
            {
                viewModel = new SmsMasterDal().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return viewModel;
        }

        private void DeactiveAll()
        {
            try
            {
                List<ViewModel> settings = new SmsMasterDal().Select();

                foreach (ViewModel item in settings)
                {
                    SMSMaster ObjSetting = new SmsMasterDal().SelectByID(item.SMSMasterID);
                    ObjSetting.IsActive = false;
                    new SmsMasterDal().Update(ObjSetting);
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
        }

        internal string ActiveDeactive(SelectRequest selectRequest)
        {
            string failureMessage = selectRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            SMSMaster ObjModel = null;
            try
            {
                ObjModel = new SmsMasterDal().SelectByID(selectRequest.SMSMasterID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null)
            {
                return failureMessage;
            }

            if (selectRequest.IsActive == true)
            {
                DeactiveAll();
            }

            ObjModel.IsActive = selectRequest.IsActive;
            ObjModel.UpdatedDOC = DateTime.Now;
            try
            {
                int rowsAffected = new SmsMasterDal().Update(ObjModel);
                if (rowsAffected <= 0)
                {
                    return SettingMessage.UpdateFail;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            return StatusType.Ok;
        }

        internal string CheckBalance(BalanceRequest balanceRequest, out string failureMessage)
        {
            failureMessage = balanceRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            string Response = string.Empty;
            try
            {
                Response = HttpProcessing.CallGetMethod(balanceRequest.BalanceURL, 5); ;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (Response == null || Response == "")
            {
                failureMessage = SMSMasterMessage.RecordNotFound;
                return Response;
            }
            failureMessage = StatusType.Ok;
            return Response;
        }

        internal ViewModel SelectActive()
        {
            ViewModel viewModel = new ViewModel();
            try
            {
                viewModel = new SmsMasterDal().SelectActive();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return viewModel;
        }
    }
}