using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.UserWisePlanLimitSetting;
using Project.Response.UserWisePlanLimitSetting;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.BAL
{
    public class UserWisePlanLimitSettingBal
    {

        internal string Insert(InsertRequest insertRequest)
        {

            string failureMessage = insertRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            ViewModel Model = new UserWisePlanLimitSettingDal().SelectDuplicate(insertRequest.UserID);

            if (Model != null)
                return UserCreationMessage.AlreadyExist;


            UserWisePlanLimit UserWisePlan = new UserWisePlanLimit
            {
                UserID = insertRequest.UserID,
                UserTypeID = insertRequest.UserTypeID,
                CircleHitCount = insertRequest.CircleHitCount,
                PlanHitCount = insertRequest.PlanHitCount,
                BillFetchHitCount = insertRequest.BillFetchHitCount,

            };

            try
            {
                int rowsAfftected = new UserWisePlanLimitSettingDal().Insert(UserWisePlan);

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
            new UserCreationWalletBal().Insert(insertRequest.UserID);
            return StatusType.Ok;

        }

        internal ReportWrapper Select(SelectRequest mainRequest, int UserID, out string failureMessage)
        {
            List<ViewModel> response = null;
            ReportWrapper wrapper = new ReportWrapper();
            try
            {
                response = new UserWisePlanLimitSettingDal().SelectReport();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                
            }


            if (response.Count <= 0 || response == null)
            {
                failureMessage = Messages.DataNotFound;
                return wrapper;
            }

            response = response.AsQueryable().WhereDynamic(mainRequest.Search).ToList();

            response = response.AsQueryable().OrderByDynamic(mainRequest.SortColumnName, mainRequest.SortDirection).ToList();
            wrapper.Count = response.Count;

            if (!mainRequest.IsExcelDownload)
            {
                response = response.AsQueryable().SkipAndTakeCount(mainRequest.SkipCount, mainRequest.TakeCount).ToList();
            }

            wrapper.IsExcelDownload = mainRequest.IsExcelDownload;
            wrapper.list = response;
            failureMessage = StatusType.Ok;
            return wrapper;
        }

        internal string Update(UpdateRequest updateRequest)
        {
            string failureMessage = updateRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            UserWisePlanLimit ObjModel = null;
            try
            {
                ObjModel = new UserWisePlanLimitSettingDal().SelectByID(updateRequest.UserWisePlanLimitID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null) return failureMessage;

            ObjModel.CircleHitCount = updateRequest.CircleHitCount;
            ObjModel.PlanHitCount = updateRequest.PlanHitCount;
            ObjModel.BillFetchHitCount = updateRequest.BillFetchHitCount;
            ObjModel.UpdatedDOC = DateTime.Now;

            int rowsAffected = -1;
            try
            {
                rowsAffected = new UserWisePlanLimitSettingDal().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (rowsAffected <= 0)
                return UserCreationMessage.UpdateFailure;

            return StatusType.Ok;
        }

        internal string Delete(DeleteRequest deleteRequest)
        {
            string failureMessage = deleteRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            UserWisePlanLimit ObjModel = null;
            try
            {
                ObjModel = new UserWisePlanLimitSettingDal().SelectByID(deleteRequest.UserWisePlanLimitID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null) return failureMessage;

            int rowsAffected = -1;
            try
            {
                rowsAffected = new UserWisePlanLimitSettingDal().Delete(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (rowsAffected <= 0)
                return UserCreationMessage.DeleteFail;

            return StatusType.Ok;
        }

        public string ActiveDeactive(ActiveDeactiveRequest Request)
        {
            string failureMessage = Request.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            UserWisePlanLimit ObjModel;
            try
            {
                ObjModel = new UserWisePlanLimitSettingDal().SelectByID(Request.UserWisePlanLimitID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (ObjModel == null)
                return Message.UserWisePlanLimitMessage.SettingFound;

            ObjModel.IsActive = Request.IsActive; int Result;
            try
            {
                Result = new UserWisePlanLimitSettingDal().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (Result <= 0)
                return Message.UserCreationMessage.UpdateFailure;

            return StatusType.Ok;
        }

        internal async Task<string> AllActiveDeactive(AllActiveDeactiveRequest mainRequest, int UserID)
        {
            string failureMessage = Message.UserMessage.UserNotFound;

            UserMaster userMaster = null;
            try
            {
                userMaster = new UserDAL().SelectByID(UserID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            if (userMaster == null) return failureMessage;
            List<UserWisePlanLimit> objModel = await new UserWisePlanLimitSettingDal().SelectAll();


            if (objModel == null || objModel.Count <= 0)
            {
                return failureMessage;
            }


            foreach (UserWisePlanLimit viewmodel in objModel)
            {
                viewmodel.IsActive = mainRequest.IsActive;
            }

            Task.Run(() =>
            {
                new UserWisePlanLimitSettingDal().UpdateRange(objModel);
            });


            return StatusType.Ok;
        }

    }
}
