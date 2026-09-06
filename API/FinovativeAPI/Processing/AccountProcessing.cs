using Project.BAL;
using Project.Model;
using Project.StaticModel;
using System;

namespace Project.Processing
{
    public class AccountProcessing
    {
        internal string BalanceValidationBeforeTransaction(UserWalletMaster UserWallet, decimal Amount, decimal commission)
        {
            using (ConcurrencyControl.AcquireLock(UserWallet.UserID, UserWallet.WalletID.ToString()))
            {
                if (UserWallet.Balance < Amount)
                    return "Insufficient Balance";

                if (UserWallet.Balance < -0.001M)
                    return "Insufficient Balance";

                if ((UserWallet.Balance + commission) < Amount)
                {
                    return $"Insufficient Balance, due to commission ({commission.TwoDigitDecimal()})";
                }

                decimal cappingAmount = new UserBalanceCappingBal().SelectByWalletIDAndUserID(UserWallet.UserID, UserWallet.WalletID);

                if ((UserWallet.Balance + commission - cappingAmount) < Amount)
                {
                    return $"Insufficient Balance, due to commission ({commission.TwoDigitDecimal()}) and capping Amount ({cappingAmount.TwoDigitDecimal()})";
                }

                return StatusType.Ok;
            }
        }

        internal int DebitBalance(
            UserWalletMaster UserWallet,
            decimal AmountWithCommission,
            decimal Amount,
            string ConsumerNumber,
            string CommissionCreditDebit,
            string DOCType,
            int AssignedUserID,
            int ServiceID,
            int OperatorID,
            string TransactionDescription,
            int ReferenceID,
            decimal TDS,
            decimal GST,
            decimal DiscountAmount
            )
        {
            int DOCInt = Int32.Parse(DateTime.Now.ToString(Messages.DOCIntegerFomrat));

            using (ConcurrencyControl.AcquireLock(DOCInt, DOCInt.ToString()))
            {
                new UserBalanceHistoryBAL().Select();
            }

            using (ConcurrencyControl.AcquireLock(UserWallet.UserID, UserWallet.WalletID.ToString()))
            {
                return Process(UserWallet,
                                AmountWithCommission * -1,
                                Amount,
                                ConsumerNumber,
                                AmountCreditDebitType.Debit,
                                CommissionCreditDebit,
                                DOCType,
                                AssignedUserID,
                                ServiceID,
                                OperatorID,
                                TransactionDescription,
                                ReferenceID,
                                TDS,
                                GST,
                                DiscountAmount);
            }
        }

        internal int CreditBalance(
            UserWalletMaster UserWallet,
            decimal AmountWithCommission,
            decimal Amount,
            string ConsumerNumber,
            string CommissionCreditDebit,
            string DOCType,
            int AssignedUserID,
            int ServiceID,
            int OperatorID,
            string TransactionDescription,
            int ReferenceID,
            decimal TDS,
            decimal GST,
            decimal DiscountAmount
            )
        {
            int DOCInt = Int32.Parse(DateTime.Now.ToString(Messages.DOCIntegerFomrat));

            using (ConcurrencyControl.AcquireLock(DOCInt, DOCInt.ToString()))
            {
                new UserBalanceHistoryBAL().Select();
            }

            using (ConcurrencyControl.AcquireLock(UserWallet.UserID, UserWallet.WalletID.ToString()))
            {
                return Process(UserWallet,
                                AmountWithCommission,
                                Amount,
                                ConsumerNumber,
                                AmountCreditDebitType.Credit,
                                CommissionCreditDebit,
                                DOCType,
                                AssignedUserID,
                                ServiceID,
                                OperatorID,
                                TransactionDescription,
                                ReferenceID,
                                TDS,
                                GST,
                                DiscountAmount);
            }
        }

        private int Process(
            UserWalletMaster UserWallet,
            decimal AmountWithFormula,
            decimal Amount,
            string ConsumerNumber,
            string AmountCreditDebitFlag,
            string CommissionCreditDebitFlag,
            string DOCType_str,
            int AssignedUserID,
            int ServiceID,
            int OperatorID,
            string TransactionDescription,
            int ReferenceID,
            decimal TDS,
            decimal GST,
            decimal DiscountAmount
            )
        {
            int Result = -1;

            UserWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(UserWallet.UserID, UserWallet.WalletID);

            decimal opBal = UserWallet.Balance;
            decimal clBal = UserWallet.Balance + AmountWithFormula;
            UserWallet.Balance = clBal;
            UserWallet.BalanceHash = new UserWalletMasterBal().HashBalance(UserWallet.UserID, UserWallet.WalletID, UserWallet.Balance);
            try
            {
                if (AmountCreditDebitFlag.EqualsIgnoreCase(AmountCreditDebitType.Debit) &&
                    UserWallet.UserID != Company.AdminUserID &&
                    clBal <= -0.0001M)
                {
                    return -1;
                }

                Result = UserWalletUpdate(UserWallet);
                if (Result <= 0)
                {
                    return Result;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            TransactionDetails transactionDetails = new TransactionDetails
            {
                UserID = UserWallet.UserID,
                TransactionAmount = Amount,
                ConsumerNumber = ConsumerNumber,
                AmountCreditDebit = AmountCreditDebitFlag,
                CommissionCreditDebit = CommissionCreditDebitFlag,
                DOCType = DOCType_str,
                AssignedUserID = AssignedUserID,
                OpeningBalance = opBal,
                ClosingBalance = UserWallet.Balance,
                ServiceID = ServiceID,
                OperatorID = OperatorID,
                WalletID = UserWallet.WalletID,
                TransactionDescription = TransactionDescription,
                ReferenceID = ReferenceID,
                TDS = TDS,
                GST = GST,
                DiscountAmount = DiscountAmount
            };

             
            try
            {
                Result = TransactionDetailsInsert(transactionDetails);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return Result;
        }

        private int UserWalletUpdate(UserWalletMaster userWalletMaster)
        {
            using (var context = new DatabaseContext())
            {
                context.UserWalletMaster.Update(userWalletMaster);
                return context.SaveChanges();
            }
        }

        private int TransactionDetailsInsert(TransactionDetails transactionDetails)
        {
            using (var context = new DatabaseContext())
            {
                context.TransactionDetails.Add(transactionDetails);
                return context.SaveChanges();
            }
        }
    }
}