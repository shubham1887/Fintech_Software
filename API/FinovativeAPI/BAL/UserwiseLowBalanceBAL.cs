using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.UserwiseLowBalance;
using Project.Response.UserwiseLowBalance;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.BAL
{
    public class UserwiseLowBalanceBAL
    {
        internal string Insert(InsertRequest insertRequest, int UserID)
        {
            string failureMessage = insertRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            UserwiseLowBalance objModel = new UserwiseLowBalanceDAL().SelectByWalletID(insertRequest.WalletID, UserID);

            //failureMessage = Message.UserwiseLowBalanceMessage.AlreadyExists;

            int rowsAffected = -1;

            if (objModel != null)
            {
                objModel.WalletID = insertRequest.WalletID;
                objModel.Amount = insertRequest.Amount;
                objModel.UserID = UserID;

                try
                {
                    rowsAffected = new UserwiseLowBalanceDAL().Update(objModel);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                    return Messages.ExceptionOccured;
                }

                if (rowsAffected <= 0)
                    return UserwiseLowBalanceMessage.UpdateFail;

                return StatusType.Ok;
            }

            UserwiseLowBalance balance = new UserwiseLowBalance
            {
                WalletID = insertRequest.WalletID,
                Amount = insertRequest.Amount,
                UserID = UserID
            };

            try
            {
                rowsAffected = new UserwiseLowBalanceDAL().Insert(balance);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (rowsAffected <= 0)
                return UserwiseLowBalanceMessage.InsertFail;

            return StatusType.Ok;
        }

        internal string Update(UpdateRequest updateRequest)
        {
            string failureMessage = updateRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            UserwiseLowBalance ObjModel = null;
            try
            {
                ObjModel = new UserwiseLowBalanceDAL().SelectByID(updateRequest.UserwiseLowBalanceID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null) return failureMessage;

            ObjModel.WalletID = updateRequest.WalletID;
            ObjModel.Amount = updateRequest.Amount;

            int rowsAffected = -1;
            try
            {
                rowsAffected = new UserwiseLowBalanceDAL().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (rowsAffected <= 0)
                return UserwiseLowBalanceMessage.UpdateFail;

            return StatusType.Ok;
        }

        internal ViewModel SelectByID(SelectByIDRequest selectByIDRequest, out string failureMessage)
        {
            failureMessage = UserwiseLowBalanceMessage.DataNotFound;
            ViewModel report = null;
            UserwiseLowBalance model = null;
            try
            {
                model = new UserwiseLowBalanceDAL().SelectByID(selectByIDRequest.UserwiseLowBalanceID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (model == null) return report;

            report = new ViewModel
            {
                UserwiseLowBalanceID = model.UserwiseLowBalanceID,
                WalletID = model.WalletID,
                Amount = model.Amount
            };

            failureMessage = StatusType.Ok;
            return report;
        }

        internal List<ViewModel> SelectByUserID(int userID, out string failureMessage)
        {
            failureMessage = UserwiseLowBalanceMessage.DataNotFound;
            List<ViewModel> report = null;
            try
            {
                report = new UserwiseLowBalanceDAL().SelectByUserID(userID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (report == null) return report;

            failureMessage = StatusType.Ok;
            return report;
        }

        internal SelectLowBalanceResponse SelectLowBalance(int userID)
        {
            SelectLowBalanceResponse report = new SelectLowBalanceResponse();
            report.Message = UserwiseLowBalanceMessage.DataNotFound;
            report.Status = StatusType.Ok;
            List<ViewModel> model = new List<ViewModel>();
            try
            {
                model = new UserwiseLowBalanceDAL().SelectByUserID(userID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (model == null || model.Count <= 0)
            {
                return report;
            }

            bool messageFound = false;
            foreach (var item in model)
            {
                UserWalletMaster userWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(item.UserID, item.WalletID);
                if (userWallet == null || model.Count <= 0)
                {
                    continue;
                }

                if (userWallet.Balance >= item.Amount)
                {
                    continue;
                }
                messageFound = true;
                report.Message = $"Your {item.WalletName} balance is below {item.Amount.TwoDigitDecimal()}, ";
            }

            if (messageFound == false)
            {
                report.Status = StatusType.Ok;
                return report;
            }

            report.Status = StatusType.Failure;
            return report;
        }

        public void UserwiseLowBalanceAuto()
        {
            bool isDevelopment = new Util().isDevelopment();
            if (isDevelopment == true)
            {
                return;
            }

            SelectLowBalanceResponse report = new SelectLowBalanceResponse();
            report.Message = UserwiseLowBalanceMessage.DataNotFound;
            report.Status = StatusType.Failure;
            List<ViewModel> model = new List<ViewModel>();
            try
            {
                model = new UserwiseLowBalanceDAL().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return;
            }

            if (model == null || model.Count <= 0)
            {
                return;
            }

            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Math.Max(3 * Environment.ProcessorCount / 4, 1)
            };
            try
            {
                Parallel.ForEach(model.AsEnumerable(), parallelOptions, (item, state) =>
                {
                    //foreach (var item in model) {
                    UserWalletMaster userWallet = new UserWalletMasterBal().SelectByWalletIDAndUserID(item.UserID, item.WalletID);

                    if (userWallet == null || userWallet.Balance <= 0.0M)
                    {
                        return;
                    }

                    if (userWallet.Balance >= item.Amount)
                    {
                        return;
                    }

                    report.Message = $"Your {item.WalletName} balance is below {item.Amount.TwoDigitDecimal()}";

                    UserMaster user = new UserDAL().SelectByIDFromCache(item.UserID);

                    if (user == null)
                    {
                        return;
                    }

                    string Subject = "Low Balance";
                    string EmailID = user.EmailID;

                    if (!IsValidData(Subject, report.Message, EmailID))
                    {
                        return;
                    }

                    MessageProcessing.SendEmail(EmailID, report.Message, Subject, "");
                });
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return;
            }
            report.Status = StatusType.Ok;
        }

        private bool IsValidData(string Subject, string Message, string To)
        {
            Validation valid = new Validation();
            if (!valid.IsValidString(Subject))
                return false;
            if (!valid.IsValidString(Message))
                return false;
            if (!valid.IsValidString(To))
                return false;
            return true;
        }
    }
}