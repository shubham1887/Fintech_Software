using Project.DAL;
using Project.Model;
using Project.StaticModel;
using System;

namespace Project.BAL
{
    internal class TransactionAPIUserRequestBAL
    {

        public void DeleteOldData()
        {
            int last10Days = Convert.ToInt32(Convert.ToDateTime(DateTime.Now.AddDays(-10)).ToString(Messages.DOCIntegerFomrat));
            new TransactionAPIUserRequestDal().DeleteBulk( last10Days);
        }

        public bool isAPIUserRequestIDExists(int UserID, string apiUserRequestID)
        {
            bool flag = selectForExists(UserID, apiUserRequestID);
            if (flag)
            {
                return true;
            }

            TransactionAPIUserRequestMaster transactionAPIUserRequest = new TransactionAPIUserRequestMaster();
            transactionAPIUserRequest.ApiUserRequestID = apiUserRequestID;
            transactionAPIUserRequest.UserID = UserID;

            int TransactionAPIUserRequestMasterID = new TransactionAPIUserRequestDal().Insert(transactionAPIUserRequest);
            if (TransactionAPIUserRequestMasterID <= 0)
            {
                return true;
            }
            return false;
        }

        public bool selectForExists(int UserID, string apiUserRequestID)
        {
            return new TransactionAPIUserRequestDal().selectForExists(UserID, apiUserRequestID);
        }
    }
}
