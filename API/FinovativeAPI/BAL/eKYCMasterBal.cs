using Project.DAL;
using Project.Model;
using Project.Processing;
using Project.StaticModel;
using System;

namespace Project.BAL
{
    public class eKYCMasterBal
    {
        public int Insert(int ProviderID, int UserID, string OrderID, string OTPGenerateHash, string OTPGenerateRequest, string OTPGenerateResponse)
        {
            eKYCMaster objModel = new eKYCMaster
            {
                ProviderID = ProviderID,
                UserID = UserID,
                ChargeAmount = 0,
                OrderID = OrderID,
                OTPGenerateHash = OTPGenerateHash,
                OTPGenerateRequest = OTPGenerateRequest,
                OTPGenerateResponse = OTPGenerateResponse,
                VerifyRequest = "",
                VerifyResponse = ""
            };

            int Result;

            try
            {
                Result = new eKYCMasterDal().Insert(objModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return 0;
            }

            return Result;
        }

        public string Update(string OrderID, string VerifyRequest, string VerifyResponse)
        {
            int eKYCMasterID = new eKYCMasterDal().SelectByOrderID(OrderID);
            eKYCMaster objModel = new eKYCMasterDal().SelectByID(eKYCMasterID);

            if (objModel == null)
                return "";

            objModel.VerifyRequest = VerifyRequest;
            objModel.VerifyResponse = VerifyResponse;

            int Result;
            try
            {
                Result = new eKYCMasterDal().Update(objModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return "";
            }

            return StatusType.Ok; ;
        }

        internal void Delete(int userID)
        {
            try
            {
                new eKYCMasterDal().Delete(userID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
        }
    }
}