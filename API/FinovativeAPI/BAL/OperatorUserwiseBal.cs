using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.OperatorUserwise;
using Project.Response.OperatorUserwise;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.BAL
{
    public class OperatorUserwiseBal
    {
        internal List<ViewModel> Select(SelectRequest selectRequest, int UserID)
        {
            List<ViewModel> viewModel = new List<ViewModel>();
            string failureMessage = selectRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return viewModel;
            try
            {
                // List<Response.Operator.ViewModel> allOperator = new OperatorMasterDAL().SelectActiveOperatorOnly();
                List<Response.Operator.ViewModelForDropDown> allOperator = new OperatorMasterDAL().SelectByServiceID(selectRequest.ServiceID, UserID);
                Parallel.ForEach(allOperator, item =>
                //foreach (Response.Operator.ViewModel item in allOperator)
                {
                    ViewModel Model = new ViewModel();

                    Model.UserID = selectRequest.UserID;

                    Model.OperatorID = item.OperatorID;
                    Model.OperatorName = item.OperatorName;
                    Model.ServiceID = item.ServiceID;
                    Model.ServiceName = item.ServiceName;
                    Model.UsageLimit = 0;
                    Model.OperatorUserwiseID = 0;
                    Model.RemainingSale = 0;
                    Model.IsActive = true;
                    decimal SumAmount = new TransactionDAL().SelectTodaySumByUserIDAndOperatorID(selectRequest.UserID, item.OperatorID);
                    Model.Todaysale = SumAmount;

                    ViewModel model = new OperatorUserwiseDal().SelectDetailByUserIDwithOperatorID(selectRequest.UserID, item.OperatorID);

                    if (model != null)
                    {
                        if (model.UsageLimit <= 0)
                            Model.Todaysale = 0;
                        Model.UsageLimit = model.UsageLimit;
                        Model.OperatorUserwiseID = model.OperatorUserwiseID;
                        Model.BlockedAmount = model.BlockedAmount;
                        Model.IsActive = model.IsActive;
                        Model.RemainingSale = model.UsageLimit - Model.Todaysale;
                    }

                    viewModel.Add(Model);
                });
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            viewModel = viewModel.OrderBy(V => V.OperatorName).ToList();

            return viewModel;
        }

        internal string Insert(InsertRequest insertRequest, out string failureMessage)
        {
            string response = string.Empty;
            failureMessage = insertRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            OperatorUserwise ObjModel = new OperatorUserwise
            {
                UserID = insertRequest.UserID,
                ServiceID = insertRequest.ServiceID,
                OperatorID = insertRequest.OperatorID,
                UsageLimit = insertRequest.UsageLimit,
                BlockedAmount = insertRequest.BlockedAmount,
                IsActive = true
            };

            OperatorUserwise model = null;
            try
            {
                model = new OperatorUserwiseDal().SelectByID(insertRequest.OperatorUserwiseID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            int rowsAffected;
            if (model == null)
                try
                {
                    response = OperatorMessage.InsertUserWiseFailure;
                    rowsAffected = new OperatorUserwiseDal().Insert(ObjModel);
                    if (rowsAffected > 0)
                        response = OperatorMessage.InsertUserWiseSuccess;
                    else
                        failureMessage = response;
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                    return Messages.ExceptionOccured;
                }
            else
            {
                response = OperatorMessage.UserWiseUpdateFail;
                model.UsageLimit = insertRequest.UsageLimit;
                model.BlockedAmount = insertRequest.BlockedAmount;
                model.UpdatedDOC = DateTime.Now;

                try
                {
                    rowsAffected = new OperatorUserwiseDal().Update(model);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                    return response;
                }
                if (rowsAffected > 0)
                    response = OperatorMessage.UserWiseUpdateSuccess;
                else
                    failureMessage = response;
            }
            failureMessage = StatusType.Ok;
            return response;
        }

        internal string DeleteByUserID(SelectRequest deleteRequest)
        {
            string failureMessage = deleteRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            try
            {
                int rowsAffected = new OperatorUserwiseDal().DeleteByUserID(deleteRequest.UserID);
                if (rowsAffected <= 0)
                {
                    return OperatorMessage.DefaultsetFail;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            return StatusType.Ok;
        }

        internal string DeleteAll(int UserID)
        {
            try
            {
                int UserTypeID = new UserDAL().SelectUserTypeID(UserID);

                if (UserTypeID != (int)UserType.Admin)
                {
                    return OperatorMessage.InvalidUser;
                }

                int rowsAffected = new OperatorUserwiseDal().DeleteAll();
                if (rowsAffected <= 0)
                {
                    return OperatorMessage.ResetAllFail;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            return StatusType.Ok;
        }

        internal string isValidDailyUsageLimit(int UserID, int OperatorID, int TxnAmount)
        {
            ViewModel ObjModel = new OperatorUserwiseDal().SelectByUserIDwithOperatorID(UserID, OperatorID);

            if (ObjModel == null)
                return StatusType.Ok;

            if (!ObjModel.IsActive)
                return "Operator is down for you, please contact to admin.";

            if (ObjModel.UsageLimit <= 0)
                return StatusType.Ok;

            decimal sumAmount = new TransactionDAL().SelectTodaySumByUserIDAndOperatorID(UserID, OperatorID);
            if ((sumAmount + TxnAmount) >= ObjModel.UsageLimit)
            {
                return $"Your daily usage limit Rs. {ObjModel.UsageLimit} is over for selected operator, please contact admin";
            }

            return StatusType.Ok;
        }

        internal string Activate(SelectActiveRequest updateRequest)
        {
            string failureMessage = updateRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            OperatorUserwise ObjModel = new OperatorUserwise
            {
                UserID = updateRequest.UserID,
                ServiceID = updateRequest.ServiceID,
                OperatorID = updateRequest.OperatorID,
                UsageLimit = 50000,
                IsActive = false
            };
            OperatorUserwise model = null;
            try
            {
                model = new OperatorUserwiseDal().SelectByID(updateRequest.OperatorUserwiseID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            int rowsAffected;
            if (model == null)
            {
                try
                {
                    rowsAffected = new OperatorUserwiseDal().Insert(ObjModel);
                    if (rowsAffected <= 0)
                        return OperatorMessage.OperatorActiveDeactiveFail;
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                    return Messages.ExceptionOccured;
                }
                return StatusType.Ok;
            }
            else
            {
                model.IsActive = updateRequest.IsActive;
                model.UpdatedDOC = DateTime.Now;
                try
                {
                    rowsAffected = new OperatorUserwiseDal().Update(model);
                    if (rowsAffected <= 0)
                        return OperatorMessage.OperatorActiveDeactiveFail;
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                    return Messages.ExceptionOccured;
                }
                return StatusType.Ok;
            }
        }

        internal bool IsValidUserWiseBlockAmount(int UserID, int OperatorID, int Amount)
        {
            string BlockedAmount = new OperatorUserwiseDal().SelectBlockedAmountByUserIDAndOperatorID(UserID, OperatorID);

            if (BlockedAmount == null)
                return true;

            BlockedAmount = BlockedAmount.Trim();

            if (BlockedAmount.Length <= 0)
                return true;

            if (BlockedAmount == "0")
                return true;

            string[] CommaSeperatedArr = BlockedAmount.Split(',');
            for (int counter = 0; counter < CommaSeperatedArr.Length; counter++)
            {
                string CommaSeperatedStr = CommaSeperatedArr[counter];

                if (CommaSeperatedStr.Trim().Length <= 0)
                {
                    continue;
                }

                if (!CommaSeperatedStr.Contains("-"))
                {
                    if (Convert.ToInt32(CommaSeperatedStr) == Amount)
                    {
                        return false;
                    }
                    continue;
                }

                string[] DashSeperatedArr = CommaSeperatedStr.Split('-');
                if (DashSeperatedArr.Length <= 1)
                {
                    continue;
                }

                int From = Convert.ToInt32(DashSeperatedArr[0]);
                int To = Convert.ToInt32(DashSeperatedArr[1]);
                if (Amount >= From && Amount <= To)
                {
                    return false;
                }
            }

            return true;
        }
    }
}