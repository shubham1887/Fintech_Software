using CircleCore;
using CircleCore.Requests;
using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.CircleAPI;
using Project.Response.CircleAPI;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class CircleAPIBal
    {
        internal string Insert(InsertRequest mainRequest, out string failureMessage)
        {
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            CircleAPIMaster ObjModel = new CircleAPIMaster
            {
                CircleAPIURL = mainRequest.CircleAPIURL
            };
            CircleAPIMaster model = null;
            try
            {
                model = new CircleAPIDal().SelectByID(mainRequest.CircleAPIMasterID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            int rowsAffected;

            string response;
            if (model == null)
            {
                failureMessage = response = CircleAPIMessages.InsertFail;
                try
                {
                    rowsAffected = new CircleAPIDal().Insert(ObjModel);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                    return Messages.ExceptionOccured;
                }
                if (rowsAffected <= 0)
                    return response;

                response = CircleAPIMessages.InsertSuccess;
                failureMessage = StatusType.Ok;
                return response;
            }

            failureMessage = response = CircleAPIMessages.UpdateFail;

            model.CircleAPIURL = mainRequest.CircleAPIURL;

            try
            {
                rowsAffected = new CircleAPIDal().Update(model);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return response;
            }

            if (rowsAffected <= 0)
                return response;

            response = CircleAPIMessages.UpdateSuccess;
            failureMessage = StatusType.Ok;
            return response;
        }

        internal List<ViewModel> Select(out string failureMessage)
        {
            List<ViewModel> viewModel = new List<ViewModel>();
            failureMessage = string.Empty;

            List<CircleAPIMaster> list = null;

            try
            {
                list = new CircleAPIDal().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            failureMessage = CircleAPIMessages.RecordNotFound;
            if (list == null || list.Count <= 0)
                return viewModel;

            foreach (CircleAPIMaster circle in list)
            {
                viewModel.Add(new ViewModel
                {
                    CircleAPIMasterID = circle.CircleAPIMasterID,
                    CircleAPIURL = circle.CircleAPIURL,
                    IsActive = circle.IsActive
                });
            }

            failureMessage = StatusType.Ok;
            return viewModel;
        }

        internal string ActiveDeactive(ActiveDeactiveRequest mainRequest)
        {
            string failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            CircleAPIMaster ObjModel = null;
            try
            {
                ObjModel = new CircleAPIDal().SelectByID(mainRequest.CircleAPIMasterID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null)
            {
                return failureMessage;
            }
            ObjModel.IsActive = mainRequest.IsActive;
            int rowsAffected;
            try
            {
                rowsAffected = new CircleAPIDal().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (rowsAffected <= 0)
                return SettingMessage.UpdateFail;

            return StatusType.Ok;
        }

        public void SetCircle()
        {
            try
            {
                List<CircleMaster> newCircles = new List<CircleMaster>();
                List<Model.ZoneMaster> list = new ZoneMasterDAL().Select();
                foreach (Model.ZoneMaster state in list)
                {
                    CircleMaster circle = new CircleMaster
                    {
                        CircleID = state.ZoneMasterID,
                        CircleName = state.Name
                    };
                    newCircles.Add(circle);
                }
                CircleProcessing.SetCircles(newCircles);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
        }

        public void SetOperatorList()
        {
            try
            {
                List<CircleOperatorMaster> newCircles = new List<CircleOperatorMaster>();
                List<Response.Operator.ViewModel> list = new OperatorMasterBAL().SelectOpertorListByServiceID((int)ServiceCode.Mobile);
                foreach (Response.Operator.ViewModel item in list)
                {
                    CircleOperatorMaster opItem = new CircleOperatorMaster
                    {
                        OperatorID = item.OperatorID,
                        OperatorName = item.OperatorName
                    };
                    newCircles.Add(opItem);
                }
                CircleProcessing.SetOperators(newCircles);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
        }
    }
}