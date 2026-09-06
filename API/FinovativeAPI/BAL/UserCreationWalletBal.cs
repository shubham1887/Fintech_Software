using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.StaticModel;
using System;

namespace Project.BAL
{
    public class UserCreationWalletBal
    {
        internal string Insert(int UserID)
        {
            UserCreationWallet Model = new UserCreationWalletDal().CheckDuplicate(UserID);

            if (Model != null)
                return UserCreationMessage.AlreadyExist;

            UserCreationWallet ObjModel = new UserCreationWallet
            {
                UserID = UserID,
                IdCount = 0,
            };
            try
            {
                int rowsAfftected = new UserCreationWalletDal().Insert(ObjModel);

                if (rowsAfftected <= 0)
                {
                    return UserCreationMessage.InsertFailure;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            return StatusType.Ok;
        }

        internal void Update(int UserID, int PurchaseCount, string CreditDebit)
        {
            UserCreationWallet ObjModel = null;
            try
            {
                ObjModel = new UserCreationWalletDal().SelectByID(UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null) return;
            if (CreditDebit.EqualsIgnoreCase(CommissionCreditDebitType.Credit))
                ObjModel.IdCount = ObjModel.IdCount + PurchaseCount;

            if (CreditDebit.EqualsIgnoreCase(CommissionCreditDebitType.Debit))
                ObjModel.IdCount = ObjModel.IdCount - PurchaseCount;

            int rowsAffected = -1;
            try
            {
                rowsAffected = new UserCreationWalletDal().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return;
            }
            if (rowsAffected <= 0)
                return;

            return;
        }

        internal bool IsValidForUserCreation(int UserID)
        {
            UserCreationWallet viewModel = new UserCreationWalletDal().SelectByID(UserID);
            if (viewModel == null)
                return false;
            if (viewModel.IdCount <= 0)
                return false;

            return true;
        }
    }
}