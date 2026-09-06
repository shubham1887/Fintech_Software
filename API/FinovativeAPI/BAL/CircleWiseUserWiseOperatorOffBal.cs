using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.CircleWiseUserWiseOperatorOff;
using Project.Response.CircleWiseUserWiseOperatorOff;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.BAL
{
    public class CircleWiseUserWiseOperatorOffBal
    {
        internal string BulkUpdate(List<UpdateRequest> mainRequest, out string failureMessge)
        {
            foreach (UpdateRequest request in mainRequest)
            {
                failureMessge = request.IsValidModalState();

                if (!failureMessge.EqualsIgnoreCase(StatusType.Ok))
                    return failureMessge;

                BulkUpdate(request.UserID, request.OperatorID, request.StateMasterList);
            }

            failureMessge = StatusType.Ok;
            return failureMessge;
        }

        internal void BulkUpdate(int UserID, int OperatorID, List<StateMasterList> stateMasterList)
        {
            try
            {
                stateMasterList = stateMasterList.GroupBy(x => x.ID).Select(x => x.FirstOrDefault()).ToList();

                List<CircleWiseUserWiseOperatorOff> listMapping = new List<CircleWiseUserWiseOperatorOff>();

                foreach (StateMasterList state in stateMasterList)
                {
                    CircleWiseUserWiseOperatorOff mapping = new CircleWiseUserWiseOperatorOffDal().SelectByUserIDAndStateID(UserID, state.ID, OperatorID);

                    if (mapping == null)
                    {
                        listMapping.Add(new CircleWiseUserWiseOperatorOff()
                        {
                            UserID = UserID,
                            StateID = state.ID,
                            OperatorID = OperatorID
                        });
                    }
                }

                if (listMapping == null || listMapping.Count <= 0)
                {
                    return;
                }

                new CircleWiseUserWiseOperatorOffDal().BulkInsert(listMapping);

                new CircleWiseUserWiseOperatorOffDal().removeCache();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return;
            }
        }

        internal List<ViewModel> SelectAllUserwiseCircleOffList()
        {
            List<ViewModel> list = new List<ViewModel>();
            try
            {
                List<ViewModel> UserWiseList = new CircleWiseUserWiseOperatorOffDal().SelectAllUserwiseCircleOffList();

                foreach (ViewModel item in UserWiseList)
                {
                    ViewModel view = new ViewModel();
                    view.CircleWiseUserWiseOperatorOffID = item.CircleWiseUserWiseOperatorOffID;
                    view.UserID = item.UserID;
                    view.UserName = item.UserName;
                    view.OperatorID = item.OperatorID;
                    view.OperatorName = item.OperatorName;
                    view.StateList = new CircleWiseUserWiseOperatorOffDal().SelectStateList(item.OperatorID, item.UserID);
                    list.Add(view);
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            List<ViewModel> distinctFlags = list.Distinct(new CircleWiseUserWiseResponseComparer()).ToList();

            return distinctFlags;
        }

        internal string IsValidCircleOff(int UserID, int OperatorID, int CircleID, string Circle)
        {
            List<CircleWiseUserWiseOperatorOff> list = new CircleWiseUserWiseOperatorOffDal().Select();
            if (list.Count <= 0)
                return StatusType.Ok;

            CircleWiseUserWiseOperatorOff response = null;
            response = (from item in list
                        where item.UserID == UserID
                        && item.OperatorID == OperatorID
                        && item.StateID == CircleID
                        select item).FirstOrDefault();

            if (response != null)
                return $"Please try it after some time. The {Circle} circle has been down.";
            return StatusType.Ok;
        }

        internal string DeleteByUserID(DeleteRequest deleteRequest)
        {
            string failureMessage = deleteRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;
            try
            {
                int rowsAffected = new CircleWiseUserWiseOperatorOffDal().DeleteByUserID(deleteRequest.UserID);
                if (rowsAffected <= 0)
                {
                    return CircleSwitchingMessages.UserwiseDeleteFail;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            return StatusType.Ok;
        }

        internal string DeleteAll()
        {
            try
            {
                int rowsAffected = new CircleWiseUserWiseOperatorOffDal().DeleteAll();
                if (rowsAffected <= 0)
                {
                    return SequenceMessage.ResetAllFail;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            return StatusType.Ok;
        }
    }
}