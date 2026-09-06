using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests;
using Project.Response.CMS;
using Project.Response.CMSSetting;
using Project.StaticModel;
using System;

namespace Project.BAL
{
    public class FingPayInsuranceBAL
    {
        internal InitiateResponse SignOn(KeyDataModel keyDataModel, string IPAddress, string userAgent, out string failureMessage)
        {
            InitiateResponse response = new InitiateResponse
            {
                Status = StatusType.Failure,
                Message = "",
            };
            int UserID = keyDataModel.UserID;
            //1.Check for user exists
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
                return response;

            string FingpayAgentID = user.FingpayAgentID;

            if (FingpayAgentID == null || FingpayAgentID == "0")
            {
                failureMessage = UserMessage.UserNotOnboarded;
                response.Message = UserMessage.UserNotOnboarded;
                return response;
            }
            int ServiceID = (int)ServiceCode.AffiliateInsurance;
            int OperatorID = new OperatorMasterDAL().SelectOperatorIDByServiceID(ServiceID);
            if (OperatorID <= 0)
            {
                failureMessage = Message.TransactionMessage.InvalidCMSOperarorCode;
                return response;
            }
            int DBServiceID = new OperatorMasterBAL().SelectServiceID(OperatorID);
            if (ServiceID != DBServiceID)
            {
                failureMessage = Message.TransactionMessage.InvalidCMSServiceCode;
                return response;
            }
            decimal tempAmount = 1;

            failureMessage = FunctionalValidationProcessing.FunctionalValidation(UserID, OperatorID, ServiceID, tempAmount, "", "", out decimal ServiceCharge);
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
            {
                response.Message = failureMessage;
                return response;
            }
            string requestID = new Util().GetRandomNumberWithDatePrefix();
            if (requestID.Length < 2)
            {
                failureMessage = "Invalid RequestID";
                return response;
            }
            string Through = new BrowserUserAgentBal().GetThrough(userAgent);

            ViewModel objCMS = new CMSSettingDal().SelectActiveCMS();
            try
            {
                if (objCMS == null)
                {
                    response.Message = CMSMessage.CMSNotActive;
                    return response;
                }
                if (objCMS.Name.EqualsIgnoreCase(CMSCode.FingpayInsurance))
                {
                    
                }

                else
                {
                    response.Message = CMSMessage.CMSNotSupported;
                    return response;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                response.Message = Messages.ExceptionOccured;
                return response;
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;

            failureMessage = StatusType.Ok;
            response.Message = failureMessage;
            response.Status = StatusType.Success;

            return response;
        }


    }
}
