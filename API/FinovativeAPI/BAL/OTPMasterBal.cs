using Project.DAL;
using Project.Model;
using Project.Processing;
using Project.Requests;
using Project.Requests.OTP;
using Project.StaticModel;
using System;

namespace Project.BAL
{
    public class OTPMasterBal
    {
        internal bool IsValid(int userID, string oTP, string type)
        {
            string enOTP = oTP;// new EncryptDecrypt().Encrypt(oTP);

            OTPMaster objModel = null;
            try
            {
                objModel = new OTPMasterDal().Select(userID, type);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return false;
            }

            if (objModel == null)
                return false;

            if (!objModel.OTP.EqualsIgnoreCase(enOTP))
                return false;

            objModel.IsVerified = true;
            int IsUpdate;
            try
            {
                IsUpdate = new OTPMasterDal().Update(objModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);

                return false;
            }
            if (IsUpdate <= 0)
                return false;

            return true;
        }

        internal bool Insert(SendRequest mainRequest, KeyDataModel keyDataModel, out string failureMessage)
        {
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return false;

            UserMaster user = new UserDAL().SelectByIDFromCache(keyDataModel.UserID);

            if (user == null) return false;

            if (mainRequest.Type.EqualsIgnoreCase("IPMaster"))
                new MessageSelection().SendOTPForIPMaster(user);

            if (mainRequest.Type.EqualsIgnoreCase("APIToken"))
                new MessageSelection().SendOTPForAPIToken(user);

            if (mainRequest.Type.EqualsIgnoreCase("SecureIPOTP"))
                new MessageSelection().SendOTPForSecureIPOTP(user);

            return true;
        }
    }
}