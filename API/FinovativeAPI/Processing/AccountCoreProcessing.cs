using Project.BAL;
using Project.DAL;
using Project.Model;
using Project.Response.Slab;
using Project.Response.User;
using Project.StaticModel;
using System;
using System.Threading.Tasks;

namespace Project.Processing
{
    public class AccountCoreProcessing
    {
        private void DebitDifferenceCommissionByRealAPI(TransactionMaster transactionMaster)
        {
            if (!new TransactionBal().IsMobileService(transactionMaster.ServiceID))
            {
                return;
            }

            if (!new ProviderMasterDAL().IsRealAPI(transactionMaster.ProviderID))
            {
                return;
            }

            DetailViewModel slabComm;
            string OutMessage = "";
            try
            {
                slabComm = new SlabDetailBAL().SelectByAmountOperatorAndUser(transactionMaster.UserID,
                                Convert.ToInt32(transactionMaster.Amount), transactionMaster.OperatorID, 0, out OutMessage);

                if (slabComm == null)
                    return;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return;
            }
            UserWalletMaster userWallet;
            int WalletID;
            try
            {
                WalletID = new WalletServiceMappingBAL().SelectWalletIDByServiceID(transactionMaster.ServiceID);

                if (WalletID <= 0) return;

                userWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(transactionMaster.UserID, WalletID);

                if (userWallet == null) return;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return;
            }

            decimal comm = slabComm.RealRetailerComm;

            decimal OldComm = transactionMaster.RetailerCommission;
            decimal diffCommAmount = OldComm - comm;

            if (diffCommAmount == 0)
            {
                return;
            }
            string TransactionDescription = $"Commission difference {diffCommAmount} deducted, due to Real API";

            try
            {
                new AccountProcessing().DebitBalance(
               UserWallet: userWallet,
               AmountWithCommission: diffCommAmount,
               Amount: comm,
               ConsumerNumber: transactionMaster.ConsumerNumber,
               CommissionCreditDebit: CommissionCreditDebitType.None,
               DOCType: DOCType.Recharge,
               AssignedUserID: transactionMaster.UserID,
               ServiceID: transactionMaster.ServiceID,
               OperatorID: transactionMaster.OperatorID,
               TransactionDescription: TransactionDescription,
               ReferenceID: transactionMaster.TransactionMasterID,
               TDS: 0,
               GST: 0,
               DiscountAmount: comm);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);

                return;
            }
        }

        public decimal GetAdminCommission(int ProviderID, int OperatorID, decimal Amount)
        {
            try
            {
                Response.ProviderConfigurationAdminCommission.DetailViewModel config = new ProviderConfigurationAdminCommissionDal().SelectAdminCommission(ProviderID, OperatorID, Amount);

                if (config == null)
                {
                    return 0;
                }

                if (Amount <= -0.00000001M)
                {
                    return 0;
                }

                if (config.AdminCommType == null)
                {
                    return 0;
                }

                if (config.AdminCommissionCategory == null)
                {
                    return 0;
                }

                decimal AdminCommission = new SlabDetailBAL().GetLower(config.AdminComm, config.AdminCommType, Convert.ToInt32(Amount), config.AdminCommissionCategory);

                return AdminCommission;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return 0;
        }

        public int SuccessTransaction(TransactionMaster transactionMaster)
        {
            if (transactionMaster == null) return 0;

            if (!transactionMaster.Status.EqualsIgnoreCase(StatusType.Success)) return 0;

            //add for Admin commi
            transactionMaster.AdminCommission = GetAdminCommission(transactionMaster.ProviderID, transactionMaster.OperatorID, transactionMaster.Amount);
            transactionMaster.AdminProfit = transactionMaster.AdminCommission - (transactionMaster.SuperDistributerCommission + transactionMaster.DistributerCommission + transactionMaster.RetailerCommission);
            // added for real api Commission reverese by difference if success of recharge
            //DebitDifferenceCommissionByRealAPI(transactionMaster);

            int rowsAfftected = new TransactionDAL().Update(transactionMaster);
            Task.Run(() =>
            {
                new CallBackPosting().select();
                if (transactionMaster.ServiceID != (int)ServiceCode.MoneyTransfer)
                    new MessageSelection().EndUserTransactionSMS(transactionMaster);
            });
            new TransactionBal().AutoBalanceTransfer(transactionMaster.UserID, transactionMaster.WalletID, transactionMaster.ClosingBalance);

            return rowsAfftected;
        }

        public void RefundTransaction(TransactionMaster transactionMaster, string message, string DOCType)
        {
            using (ConcurrencyControl.AcquireLock(transactionMaster.UserID, transactionMaster.TransactionMasterID.ToString()))
            {
                TransactionMaster item = new TransactionDAL().SelectByID(transactionMaster.TransactionMasterID);

                if (new ResponseProcess().CheckFailure(item.Status))
                {
                    return;
                }

                RefundTransactionCore(transactionMaster, message, DOCType);
            }
        }

        private void RefundTransactionCore(TransactionMaster transactionMaster, string message, string DOCType)
        {
            //transactionMaster.Status = StatusType.Failure;
            transactionMaster.PendingProviderStatus = StatusType.Failure;
            transactionMaster.Description = message;
            transactionMaster.RetailerCommissionHold = 0;

            int rowsAfftected = new TransactionDAL().Update(transactionMaster);
            if (rowsAfftected <= 0)
            {
                return;
            }

            message = $"{message}, PID : {transactionMaster.TransactionMasterID}";

            if (transactionMaster.ServiceID == (int)ServiceCode.AEPS || transactionMaster.ServiceID == (int)ServiceCode.MATM)
                return;
            UserWalletMaster userWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(transactionMaster.UserID, transactionMaster.WalletID);
            if (userWallet == null)
            {
                return;
            }
            decimal DiscountAmountWithTDS = transactionMaster.RetailerCommission - (transactionMaster.RetailerTDS);
            int Result = new AccountProcessing().CreditBalance(
               UserWallet: userWallet,
               AmountWithCommission: transactionMaster.DebitAmount,
               Amount: transactionMaster.Amount,
               ConsumerNumber: transactionMaster.ConsumerNumber,
               CommissionCreditDebit: CommissionCreditDebitType.None,
               DOCType: DOCType,
               AssignedUserID: transactionMaster.UserID,
               ServiceID: transactionMaster.ServiceID,
               OperatorID: transactionMaster.OperatorID,
               TransactionDescription: message,
               ReferenceID: transactionMaster.TransactionMasterID,
               TDS: transactionMaster.RetailerTDS * -1,
               GST: 0,
               DiscountAmount: DiscountAmountWithTDS);

            RetailerDebitCommission(transactionMaster.UserID, transactionMaster.ServiceID, transactionMaster.OperatorID, transactionMaster.TransactionMasterID,
                                     transactionMaster.RetailerCommission, transactionMaster.ConsumerNumber, DOCType, message, transactionMaster.RetailerTDS);

            UplineDebitCommission(transactionMaster.UserID, transactionMaster.ServiceID, transactionMaster.OperatorID, transactionMaster.TransactionMasterID,
                                      transactionMaster.DistributerCommission, transactionMaster.ConsumerNumber, DOCType, message, transactionMaster.DistributerTDS);

            UplineDebitCommission(transactionMaster.UserID, transactionMaster.ServiceID, transactionMaster.OperatorID, transactionMaster.TransactionMasterID,
                                      transactionMaster.SuperDistributerCommission, transactionMaster.ConsumerNumber, DOCType, message, transactionMaster.SuperDistributerTDS, isForSuperDist: true);

            // Task.Run(() => AdminAccountCreditProcessing(transactionMaster.UserID, transactionMaster.WalletID, transactionMaster.ServiceID, transactionMaster.OperatorID, transactionMaster.TransactionMasterID,
            //transactionMaster.RetailerCommission, transactionMaster.DistributerCommission, transactionMaster.SuperDistributerCommission, transactionMaster.ConsumerNumber, DOCType, message));

            Task.Run(() => new CallBackPosting().select());
        }

        private bool IsCurrentMonth(DateTime doc)
        {
            int holdmonth = doc.Month;

            int currentMonth = DateTime.Now.Month;

            if (holdmonth == currentMonth)
                return true;

            return false;
        }

        public void UplineCreditCommission(int RetailerID, int ServiceID, int OperatorID, int transactionMasterID,
               decimal UplineCommissionAmount, string ConsumerNumber, string DOCType, string TransactionDescription, decimal GST = 0, decimal TDS = 0, bool isForSuperDist = false)
        {
            if (UplineCommissionAmount == 0)
            {
                return;
            }

            UplineCommissionAmount = UplineCommissionAmount - TDS;

            ParentViewModel parent = new UserDAL().SelectParent(RetailerID);
            if (parent == null)
            {
                return;
            }

            int UplineID = parent.ParentID;

            if (UplineID <= 0)
            {
                return;
            }

            if (isForSuperDist == false)
            {
                int Parent_DistUserTypeID = new UserDAL().SelectUserTypeID(UplineID);

                if (Parent_DistUserTypeID != (int)UserType.Distributer)
                {
                    return;
                }
                // get userType from parent.UserID
                //if userType!=dist, return ;
            }
            else
            {
                int Parent_SD_UserTypeID = new UserDAL().SelectUserTypeID(UplineID);

                if (Parent_SD_UserTypeID != (int)UserType.SuperDistributer)
                {
                    parent = new UserDAL().SelectParent(UplineID);
                    if (parent == null)
                    {
                        return;
                    }

                    UplineID = parent.ParentID;

                    if (UplineID <= 0)
                    {
                        return;
                    }
                }
            }

            int parentUserTYpeID = new UserDAL().SelectUserTypeID(UplineID);

            if (Util.isRoleAdminOrSupport(parentUserTYpeID))
            {
                return;
            }

            UserWalletMaster userWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(UplineID, (int)WalletCode.AEPS);
            if (userWallet == null)
            {
                return;
            }

            int DistCommResult = new AccountProcessing().CreditBalance(
                   UserWallet: userWallet,
                   AmountWithCommission: UplineCommissionAmount,
                   Amount: 0,
                   ConsumerNumber: ConsumerNumber,
                   CommissionCreditDebit: CommissionCreditDebitType.Credit,
                   DOCType: DOCType,
                   AssignedUserID: RetailerID,
                   ServiceID: ServiceID,
                   OperatorID: OperatorID,
                   TransactionDescription: TransactionDescription,
                   ReferenceID: transactionMasterID,
                   TDS: TDS,
                   GST: GST,
                   DiscountAmount: UplineCommissionAmount);
        }
        public void RetailerCreditCommission(int RetailerID, int ServiceID, int OperatorID, int transactionMasterID,
               decimal UplineCommissionAmount, string ConsumerNumber, string DOCType, string TransactionDescription, decimal GST = 0, decimal TDS = 0, bool isForSuperDist = false)
        {

            if (UplineCommissionAmount <= 0.00001M)
                return;

            UserWalletMaster userWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(RetailerID, (int)WalletCode.AEPS);
            if (userWallet == null)
            {
                return;
            }

            int DistCommResult = new AccountProcessing().CreditBalance(
                   UserWallet: userWallet,
                   AmountWithCommission: UplineCommissionAmount,
                   Amount: 0,
                   ConsumerNumber: ConsumerNumber,
                   CommissionCreditDebit: CommissionCreditDebitType.Credit,
                   DOCType: DOCType,
                   AssignedUserID: RetailerID,
                   ServiceID: ServiceID,
                   OperatorID: OperatorID,
                   TransactionDescription: TransactionDescription,
                   ReferenceID: transactionMasterID,
                   TDS: TDS,
                   GST: GST,
                   DiscountAmount: UplineCommissionAmount);
        }

        public void UplineDebitCommission(int RetailerID, int ServiceID, int OperatorID, int transactionMasterID,
          decimal UplineCommissionAmount, string ConsumerNumber, string DOCType, string TransactionDescription, decimal TDS = 0, bool isForSuperDist = false)
        {
            if (UplineCommissionAmount == 0)
            {
                return;
            }
            UplineCommissionAmount = UplineCommissionAmount - TDS;
            ParentViewModel parent = new UserDAL().SelectParent(RetailerID);
            if (parent == null)
            {
                return;
            }

            int UplineID = parent.ParentID;

            if (UplineID <= 0)
            {
                return;
            }

            if (isForSuperDist == true)
            {
                ParentViewModel superparent = new UserDAL().SelectParent(UplineID);
                if (superparent == null)
                {
                    return;
                }

                UplineID = superparent.ParentID;

                if (UplineID <= 0)
                {
                    return;
                }

                if (UplineID == Company.AdminUserID)
                {
                    UplineID = parent.ParentID;
                }
            }

            UserWalletMaster userWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(UplineID, (int)WalletCode.AEPS);
            if (userWallet == null)
            {
                return;
            }

            int DistCommResult = new AccountProcessing().DebitBalance(
                   UserWallet: userWallet,
                   AmountWithCommission: UplineCommissionAmount,
                   Amount: 0,
                   ConsumerNumber: ConsumerNumber,
                   CommissionCreditDebit: CommissionCreditDebitType.Debit,
                   DOCType: DOCType,
                   AssignedUserID: RetailerID,
                   ServiceID: ServiceID,
                   OperatorID: OperatorID,
                   TransactionDescription: TransactionDescription,
                   ReferenceID: transactionMasterID,
                   TDS: 0,
                   GST: 0,
                   DiscountAmount: UplineCommissionAmount);
        }
        public void RetailerDebitCommission(int RetailerID, int ServiceID, int OperatorID, int transactionMasterID,
          decimal UplineCommissionAmount, string ConsumerNumber, string DOCType, string TransactionDescription, decimal TDS = 0)
        {
            if (UplineCommissionAmount <= 0)
                return;

            UplineCommissionAmount = UplineCommissionAmount - TDS;


            UserWalletMaster userWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(RetailerID, (int)WalletCode.AEPS);
            if (userWallet == null)
            {
                return;
            }

            int DistCommResult = new AccountProcessing().DebitBalance(
                   UserWallet: userWallet,
                   AmountWithCommission: UplineCommissionAmount,
                   Amount: 0,
                   ConsumerNumber: ConsumerNumber,
                   CommissionCreditDebit: CommissionCreditDebitType.Debit,
                   DOCType: DOCType,
                   AssignedUserID: RetailerID,
                   ServiceID: ServiceID,
                   OperatorID: OperatorID,
                   TransactionDescription: TransactionDescription,
                   ReferenceID: transactionMasterID,
                   TDS: 0,
                   GST: 0,
                   DiscountAmount: UplineCommissionAmount);
        }

        public void DebitTransaction(TransactionMaster transactionMaster, string message, string DOCType)
        {
            UserWalletMaster userWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(transactionMaster.UserID, transactionMaster.WalletID);

            if (userWallet == null)
                return;

            new AccountProcessing().DebitBalance(
                    UserWallet: userWallet,
                    AmountWithCommission: transactionMaster.DebitAmount,
                    Amount: transactionMaster.Amount,
                    ConsumerNumber: transactionMaster.ConsumerNumber,
                    CommissionCreditDebit: CommissionCreditDebitType.None,
                    DOCType: DOCType,
                    AssignedUserID: transactionMaster.UserID,
                    ServiceID: transactionMaster.ServiceID,
                    OperatorID: transactionMaster.OperatorID,
                    TransactionDescription: message,
                    ReferenceID: transactionMaster.TransactionMasterID,
                    TDS: 0,
                    GST: 0,
                    DiscountAmount: transactionMaster.RetailerCommission);

            UplineCreditCommission(transactionMaster.UserID, transactionMaster.ServiceID, transactionMaster.OperatorID, transactionMaster.TransactionMasterID,
                                   transactionMaster.DistributerCommission, transactionMaster.ConsumerNumber, DOCType, message);

            UplineCreditCommission(transactionMaster.UserID, transactionMaster.ServiceID, transactionMaster.OperatorID, transactionMaster.TransactionMasterID,
                                  transactionMaster.SuperDistributerCommission, transactionMaster.ConsumerNumber, DOCType, message, isForSuperDist: true);
        }

        private void AdminAccountCreditProcessing(int UserID, int WalletID, int ServiceID, int OperatorID, int transactionMasterID,
                              decimal RetailerComm, decimal DistributorComm, decimal SuperDistributorComm, string ConsumerNumber, string DOCType, string TransactionDescription)
        {
            string AdminAccounting = new SettingMasterBal().SelectByKeyWithoutDefault(Enum.GetName(typeof(SettingMasterKeys), SettingMasterKeys.AdminAccounting));

            if (!AdminAccounting.EqualsIgnoreCase(AdminAccountingType.True))
                return;

            int UplineID = 0;
            AdminCreditCommission(Company.AdminUserID, UserID, WalletID, ServiceID, OperatorID, transactionMasterID,
                            RetailerComm, ConsumerNumber, DOCType, TransactionDescription);

            ParentViewModel parent = new UserDAL().SelectParent(UserID);
            if (parent == null)
            {
                return;
            }
            UplineID = parent.ParentID;
            int Parent_DistUserTypeID = new UserDAL().SelectUserTypeID(UplineID);

            if (Parent_DistUserTypeID == (int)UserType.Distributer)
            {
                AdminCreditCommission(Company.AdminUserID, UplineID, WalletID, ServiceID, OperatorID, transactionMasterID,
                        DistributorComm, ConsumerNumber, DOCType, TransactionDescription);
            }
            else if (Parent_DistUserTypeID == (int)UserType.SuperDistributer)
            {
                AdminCreditCommission(Company.AdminUserID, UplineID, WalletID, ServiceID, OperatorID, transactionMasterID,
                       SuperDistributorComm, ConsumerNumber, DOCType, TransactionDescription, isForSuperDist: true);
                return;
            }

            ParentViewModel superparent = new UserDAL().SelectParent(UplineID);
            if (superparent == null)
            {
                return;
            }
            UplineID = superparent.ParentID;
            if (UplineID <= 0)
            {
                return;
            }
            int Parent_SuperUserTypeID = new UserDAL().SelectUserTypeID(UplineID);
            if (Parent_SuperUserTypeID == (int)UserType.SuperDistributer)
            {
                AdminCreditCommission(Company.AdminUserID, UplineID, WalletID, ServiceID, OperatorID, transactionMasterID,
                        SuperDistributorComm, ConsumerNumber, DOCType, TransactionDescription, isForSuperDist: true);
            }
        }

        public void AdminDebitCommission(int AdminUserID, int AssignedUserID, int WalletID, int ServiceID, int OperatorID, int transactionMasterID,
        decimal UplineCommissionAmount, string ConsumerNumber, string DOCType, string TransactionDescription, bool isForSuperDist = false)
        {
            if (UplineCommissionAmount == 0)
            {
                return;
            }
            UserWalletMaster userWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(AdminUserID, WalletID);
            if (userWallet == null)
            {
                return;
            }

            int DistCommResult = new AccountProcessing().DebitBalance(
                   UserWallet: userWallet,
                   AmountWithCommission: UplineCommissionAmount,
                   Amount: 0,
                   ConsumerNumber: ConsumerNumber,
                   CommissionCreditDebit: CommissionCreditDebitType.Debit,
                   DOCType: DOCType,
                   AssignedUserID: AssignedUserID,
                   ServiceID: ServiceID,
                   OperatorID: OperatorID,
                   TransactionDescription: TransactionDescription,
                   ReferenceID: transactionMasterID,
                   TDS: 0,
                   GST: 0,
                   DiscountAmount: UplineCommissionAmount);
        }

        public void AdminCreditCommission(int AdminUserID, int AssignedUserID, int WalletID, int ServiceID, int OperatorID, int transactionMasterID,
                 decimal UplineCommissionAmount, string ConsumerNumber, string DOCType, string TransactionDescription, bool isForSuperDist = false)
        {
            if (UplineCommissionAmount == 0)
            {
                return;
            }

            UserWalletMaster userWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(AdminUserID, WalletID);
            if (userWallet == null)
            {
                return;
            }

            int DistCommResult = new AccountProcessing().CreditBalance(
                   UserWallet: userWallet,
                   AmountWithCommission: UplineCommissionAmount,
                   Amount: 0,
                   ConsumerNumber: ConsumerNumber,
                   CommissionCreditDebit: CommissionCreditDebitType.Credit,
                   DOCType: DOCType,
                   AssignedUserID: AssignedUserID,
                   ServiceID: ServiceID,
                   OperatorID: OperatorID,
                   TransactionDescription: TransactionDescription,
                   ReferenceID: transactionMasterID,
                   TDS: 0,
                   GST: 0,
                   DiscountAmount: UplineCommissionAmount);
        }
    }
}