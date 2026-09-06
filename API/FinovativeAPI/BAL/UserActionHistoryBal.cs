using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.UserActionHistory;
using Project.Response.UserActionHistory;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class UserActionHistoryBal
    {
        public string Insert(int UserID, string Action, string Description, string Status)
        {
            if (UserID <= 0) return "";

            UserActionHistory ObjModel = new UserActionHistory
            {
                UserID = UserID,
                Action = Action,
                Description = Description,
                Status = Status
            };
            try
            {
                if (UserID != 0)
                {
                    int rowsAfftected = new UserActionHistoryDal().Insert(ObjModel);

                    if (rowsAfftected <= 0)
                        return UserActionHistoryMessage.InsertFailure;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return StatusType.Ok;
        }

        internal UserActionHistoryWrapper SelectByUserID(SelectByUserIDRequest selectRequest, out string failureMessage)
        {
            UserActionHistoryWrapper wrapper = new UserActionHistoryWrapper();

            List<ViewModel> list = new List<ViewModel>();
            failureMessage = selectRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;
            failureMessage = Messages.DataNotFound;
            try
            {
                list = new UserActionHistoryDal().SelectByUserID(selectRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return wrapper;
            }
            if (list == null || list.Count <= 0)
            {
                return wrapper;
            }
            wrapper.list = list;
            wrapper.IsExcelDownload = selectRequest.IsExcelDownload;
            failureMessage = StatusType.Ok;
            return wrapper;
        }
    }
}