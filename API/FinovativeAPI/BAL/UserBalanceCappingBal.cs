using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.UserBalanceCapping;
using Project.Response.UserBalanceCapping;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project.BAL
{
    public class UserBalanceCappingBal
    {
        internal string Insert(InsertRequest insertRequest, out string failureMessage)
        {
            string response = string.Empty;
            string FailMessage = string.Empty;
            if (insertRequest.IsUserwise == true)
                FailMessage = insertRequest.IsValidModalStateUserwise();

            if (insertRequest.IsUserwise == false)
                FailMessage = insertRequest.IsValidModalStateDefault();

            failureMessage = FailMessage;
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            if (insertRequest.IsUserwise == false)
            {
                UserBalanceCapping model = null;
                try
                {
                    model = new UserBalanceCappingDal().SelectWalletID(insertRequest.WalletID);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }
                if (model != null)
                {
                    try
                    {
                        int i = new UserBalanceCappingDal().DeleteByWalletID(insertRequest.WalletID);
                    }
                    catch (Exception ex)
                    {
                        new Logger().Write(ex);
                    }
                }

                try
                {
                    List<UserMaster> userList = new UserDAL().Select().FindAll(T => T.UserID != ((int)UserType.Admin));

                    Parallel.ForEach(userList, user =>
                    {
                        UserBalanceCapping ObjModel = new UserBalanceCapping
                        {
                            WalletID = insertRequest.WalletID,
                            Amount = insertRequest.Amount,
                            UserID = user.UserID,
                        };
                        new UserBalanceCappingDal().Insert(ObjModel);
                    });
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                    return Messages.ExceptionOccured;
                }

                response = UserBalanceCappingMessages.InsertDeaultCappingSuccess;
                failureMessage = StatusType.Ok;
                return response;
            }
            else
            {
                UserBalanceCapping model = null;
                try
                {
                    model = new UserBalanceCappingDal().SelectByWalletIDAndUserID(insertRequest.UserID, insertRequest.WalletID);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }
                int affectedrow = 0;
                response = UserBalanceCappingMessages.InsertDeaultCappingFailure;

                if (model == null)
                {
                    UserBalanceCapping ObjModel = new UserBalanceCapping
                    {
                        WalletID = insertRequest.WalletID,
                        Amount = insertRequest.UserwiseAmount,
                        UserID = insertRequest.UserID,
                    };
                    try
                    {
                        affectedrow = new UserBalanceCappingDal().Insert(ObjModel);
                    }
                    catch (Exception ex)
                    {
                        new Logger().Write(ex);
                    }
                    if (affectedrow <= 0) return response;

                    response = UserBalanceCappingMessages.InsertCappingSuccess;
                    failureMessage = StatusType.Ok;
                    return response;
                }
                if (model != null)
                {
                    UserBalanceCapping ObjModel = new UserBalanceCapping
                    {
                        WalletID = model.WalletID,
                        Amount = insertRequest.UserwiseAmount,
                        UserID = model.UserID,
                        UserBalanceCappingID = model.UserBalanceCappingID,
                        UpdatedDOC = DateTime.Now
                    };
                    try
                    {
                        affectedrow = new UserBalanceCappingDal().Update(ObjModel);
                    }
                    catch (Exception ex)
                    {
                        new Logger().Write(ex);
                    }
                    if (affectedrow <= 0) return response;

                    response = UserBalanceCappingMessages.InsertCappingSuccess;
                    failureMessage = StatusType.Ok;
                    return response;
                }
            }
            return response;
        }

        internal string Update(UpdateRequest updateRequest, out string failureMessage)
        {
            string response = string.Empty;
            failureMessage = updateRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            UserBalanceCapping model = null;
            try
            {
                model = new UserBalanceCappingDal().SelectByID(updateRequest.UserBalanceCappingID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (model == null)
                return Message.UserBalanceCappingMessages.InvalidCapping;

            // model.UserID = updateRequest.UserID;
            //  model.WalletID = updateRequest.WalletID;
            model.Amount = updateRequest.Amount;
            model.UpdatedDOC = DateTime.Now;
            model.UserBalanceCappingID = updateRequest.UserBalanceCappingID;

            failureMessage = Message.UserBalanceCappingMessages.UpdateFailure;
            int Result = -1;
            try
            {
                Result = new UserBalanceCappingDal().Update(model);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            if (Result <= 0)
            {
                return failureMessage;
            }

            failureMessage = StatusType.Ok;
            return Message.UserBalanceCappingMessages.UpdateSuccess;
        }

        internal string Delete(DeleteRequest mainRequest, out string failureMessage)
        {
            string response = string.Empty;
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            UserBalanceCapping model = null;
            try
            {
                model = new UserBalanceCappingDal().SelectByID(mainRequest.UserBalanceCappingID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (model == null)
                return Message.UserBalanceCappingMessages.InvalidCapping;

            failureMessage = Message.UserBalanceCappingMessages.DeleteFail;
            int Result = -1;
            try
            {
                Result = new UserBalanceCappingDal().Delete(model);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            if (Result <= 0)
            {
                return failureMessage;
            }

            failureMessage = StatusType.Ok;
            return Message.UserBalanceCappingMessages.DeleteSuccess;
        }

        internal List<ViewModel> Select(SelectRequest selectRequest)
        {
            List<ViewModel> viewModel = new List<ViewModel>();

            try
            {
                viewModel = new UserBalanceCappingDal().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return viewModel;
        }

        internal decimal SelectByWalletIDAndUserID(int UserID, int WalletID)
        {
            try
            {
                UserBalanceCapping balancecapping = new UserBalanceCappingDal().SelectByWalletIDAndUserIDWithCache(UserID, WalletID);

                if (balancecapping == null)
                {
                    return 0;
                }

                return balancecapping.Amount;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return 0;
        }
    }
}