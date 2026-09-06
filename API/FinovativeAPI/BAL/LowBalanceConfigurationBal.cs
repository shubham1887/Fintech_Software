using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.LowBalanceConfiguration;
using Project.StaticModel;
using System;

namespace Project.BAL
{
    public class LowBalanceConfigurationBal
    {
        internal string Insert(InsertRequest insertRequest)
        {
            string failureMessage = insertRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;

            LowBalanceConfiguration objModel = new LowBalanceConfigurationDal().SelectDuplicate(insertRequest);
            if (objModel == null)
                return LowBalanceMessage.AlreadyExist;

            objModel = new LowBalanceConfiguration
            {
                UserID = insertRequest.UserID,
                WalletID = insertRequest.UserID,
                Amount = insertRequest.Amount,
                MobileNumber = insertRequest.MobileNumber,
                EmailID = insertRequest.EmailID,
                WebHookURL = insertRequest.WebHookURL,
                IsActive = true,
                IsDelete = false
            };

            int Result;

            try
            {
                Result = new LowBalanceConfigurationDal().Insert(objModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            if (Result <= 0) return LowBalanceMessage.InsertFail;

            return StatusType.Ok;
        }
    }
}