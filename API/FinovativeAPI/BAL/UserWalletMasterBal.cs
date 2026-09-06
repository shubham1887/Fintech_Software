using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.Wallet;
using Project.Response.User;
using Project.Response.Wallet;
using Project.StaticModel;
using System;


namespace Project.BAL
{
    public class UserWalletMasterBal
    {
        internal UserWalletMaster SelectByWalletIDAndUserID(int UserID, int WalletID)
        {
            using (ConcurrencyControl.AcquireLock(UserID, WalletID.ToString()))
            {
                UserWalletMaster userWallet = new UserWalletMasterDal().SelectByWalletIDAndUserID(new UserWalletMaster
                {
                    UserID = UserID,
                    WalletID = WalletID,
                });

                if (userWallet == null)
                    return userWallet;

                if (!HashBalanceValidation(UserID, WalletID, userWallet.Balance, userWallet.BalanceHash))
                    return null;

                return userWallet;
            }
        }

        internal ViewModel SelectByWalletIDAndReceiverID(int UserID, SelectRequest mainReq, out string failureMessage)
        {
            ViewModel model = new ViewModel();
            failureMessage = mainReq.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return model;

            try
            {
                failureMessage = "Upline not found";
                ParentViewModel viewModel = new UserBal().SelectParent(mainReq.ReceiverID);
                if (viewModel == null)
                    return null;

                if (UserID != viewModel.ParentID && UserID != Company.AdminUserID)
                {
                    int usertypeid = 0;
                    try
                    {
                        usertypeid = new UserDAL().SelectUserTypeID(UserID);
                    }
                    catch (Exception ex)
                    {
                        new Logger().Write(ex);
                    }
                    if (usertypeid != (int)UserType.Support && !Util.isDFOSOrSDFOS(usertypeid))
                    {
                        failureMessage = Message.FundMessage.InvalidUser;
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return null;
            }

            failureMessage = "Wallet not found";
            UserWalletMaster wallet = SelectByWalletIDAndUserID(mainReq.ReceiverID, mainReq.WalletID);
            if (wallet == null)
                return null;

            CashCollectionMaster collectionMaster = new CashCollectionMasterDal().SelectByUserID(mainReq.ReceiverID);

            string cashCollection = new UserBal().IsCashCollection();

            if (collectionMaster == null || cashCollection.EqualsIgnoreCase("false"))
            {
                model = new ViewModel
                {
                    Balance = (int)wallet.Balance,
                    Outstanding = 0,
                    BalanceDetail = $"Balance : {(int)wallet.Balance}"
                };
                failureMessage = StatusType.Ok;
                return model;
            }

            model = new ViewModel
            {
                Balance = (int)wallet.Balance,
                Outstanding = (int)collectionMaster.Outstanding,
                BalanceDetail = $"Balance : {(int)wallet.Balance}, OS : {(int)collectionMaster.Outstanding}"
            };
            failureMessage = StatusType.Ok;
            return model;
        }

        internal List<DetailViewModel> SelectByUserID(int UserID, out string failureMessage)
        {
            failureMessage = "Wallet not found";

            List<DetailViewModel> wallet = new UserWalletMasterDal().SelectByUserID(UserID);

            if (wallet == null || wallet.Count <= 0)
                return null;

            failureMessage = StatusType.Ok;
            return wallet;
        }

        private bool IsvalidUserForOD(int UserID)
        {
            int UserTypeID = -1;
            try
            {
                UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (UserTypeID == (int)UserType.Client)
                return false;

            if (UserID == Company.AdminUserID)
                return true;

            bool IsActive = false;
            try
            {
                IsActive = new OverdraftMasterDAL().SelectActiveOverdraft(UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return IsActive;
        }

        private bool IsvalidUserForROS(int UserID)
        {
            int UserTypeID = -1;
            try
            {
                UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if ((UserTypeID == (int)UserType.Admin) || (UserTypeID == (int)UserType.Retailer) || (UserTypeID == (int)UserType.APIUser) || (UserTypeID == (int)UserType.Client))
                return false;

            return true;
        }

        private bool IsvalidUserForOS(int UserID)
        {
            int UserTypeID = -1;
            try
            {
                UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (UserTypeID == (int)UserType.Client)
                return false;

            return true;
        }

        private bool IsvalidUserForCommission(int UserID)
        {
            int UserTypeID = -1;
            try
            {
                UserTypeID = new UserDAL().SelectUserTypeID(UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if ((UserTypeID == (int)UserType.Admin))
                return false;

            string commission = new UserBal().IsShowCommission();

            if (commission.EqualsIgnoreCase("No"))
                return false;

            return true;
        }

        internal List<UserWalletBalanceViewModel> SelectForDashboardByUserID(int UserID, string Date, out string failureMessage)
        {
            failureMessage = Messages.DataNotFound;
            List<UserWalletBalanceViewModel> wallet = new List<UserWalletBalanceViewModel>();
            try
            {
                wallet = new UserWalletMasterDal().SelectForDashboardByUserID(UserID, Date);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (wallet.Count < 0 || wallet == null)
            {
                return wallet;
            }

            failureMessage = StatusType.Ok;
            return wallet;
        }

        internal string HashBalance(int UserID, int WalletID, decimal Balance)
        {
            string HashBalance = "";
            try
            {
                Balance = decimal.Round(Balance, 5, MidpointRounding.AwayFromZero);

                HashBalance = WalletMessage.HashBalanceMappingWithUser;
                HashBalance = HashBalance.Replace("[UserID]", UserID.ToString());
                HashBalance = HashBalance.Replace("[WalletID]", WalletID.ToString());
                HashBalance = HashBalance.Replace("[Balance]", Balance.ToString());
                HashBalance = HashBalance.Replace("[SystemName]", Company.ClientName);

                HashBalance = new Hashing().Hash(HashBalance);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return HashBalance;
            }
            return HashBalance;
        }

        private bool HashBalanceValidation(int UserID, int WalletID, decimal Balance, string BalanceHash)
        {
            try
            {
                string HashBalance = WalletMessage.HashBalanceMappingWithUser;
                HashBalance = HashBalance.Replace("[UserID]", UserID.ToString());
                HashBalance = HashBalance.Replace("[WalletID]", WalletID.ToString());
                HashBalance = HashBalance.Replace("[Balance]", Balance.ToString());
                HashBalance = HashBalance.Replace("[SystemName]", Company.ClientName);
                HashBalance = new Hashing().Hash(HashBalance);

                if (BalanceHash == null)
                    return false;

                if (!BalanceHash.EqualsIgnoreCase(HashBalance))
                    return false;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return false;
            }

            return true;
        }

        public int UpdateBalanceByConfirmation(int UserID,string Balance)
        {
            decimal clBal = decimal.Parse(Balance);
            clBal = decimal.Round(clBal, 5);
            decimal formattedClBal = decimal.Parse(clBal.ToString("0.00000"));
            UserWalletMaster UserWallet = SelectByWalletIDAndUserID(UserID, (int)WalletCode.Recharge);
            UserWallet.Balance = formattedClBal;
            UserWallet.BalanceHash = HashBalance(UserWallet.UserID, UserWallet.WalletID, UserWallet.Balance);
            try
            {
                int Result = new UserWalletMasterDal().UserWalletUpdate(UserWallet);
                if (Result <= 0)
                {
                    return Result;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return 1;
        }
    }
}