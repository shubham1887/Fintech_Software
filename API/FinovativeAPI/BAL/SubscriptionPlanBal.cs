using Processing.Processing;
using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.ServiceSubscription;
using Project.Response.ServiceSubscription;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class SubscriptionPlanBal
    {
        internal string Insert(InsertRequest request)
        {
            string failureMessage;

            if (request.Document.Size.Length > 0 && request.Document.Type.Length > 0)
                failureMessage = request.IsValidModalStateForDocument();

            string path = "";
            try
            {
                path = new MediaProcessing().SaveImage(request.Document, MediaType.SubscriptionIMG, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            ServiceSubscriptionCreatePlanMaster ObjModel = new ServiceSubscriptionCreatePlanMaster
            {
                PlanName = request.PlanName,
                Amount = request.Amount,
                ImageURL = path,
                IsDefault = false
            };
            int rowsAfftected = 0;
            try
            {
                 rowsAfftected = new ServiceSubscriptionCreatePlanMasterDal().Insert(ObjModel);

                if (rowsAfftected <= 0)
                {
                    return Messages.InsertFailure;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return failureMessage;
            }
            foreach(ServiceList item in request.Services)
            {
                new ServiceSubscriptionPlanDetailBal().Insert(rowsAfftected, item.ServiceID);
            }
            return StatusType.Ok;
        }

        internal string Update(UpdateRequest updateRequest)
        {
            string failureMessage = updateRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            ServiceSubscriptionCreatePlanMaster ObjModel = null;
            try
            {
                ObjModel = new ServiceSubscriptionCreatePlanMasterDal().SelectByID(updateRequest.ServiceSubscriptionCreatePlanID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null) return failureMessage;

            //if (updateRequest.Document.Size.Length > 0 && updateRequest.Document.Type.Length > 0)
            //    failureMessage = updateRequest.IsValidModalStateForDocument();

            string path = "";
            try
            {
                path = new MediaProcessing().SaveImage(updateRequest.Document, MediaType.SubscriptionIMG, out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            ObjModel.ServiceSubscriptionCreatePlanMasterID = updateRequest.ServiceSubscriptionCreatePlanID;
            ObjModel.PlanName = updateRequest.PlanName;
            ObjModel.Amount = updateRequest.Amount;
            ObjModel.ImageURL = (!string.IsNullOrEmpty(path)) ? path : ObjModel.ImageURL;

            int rowsAffected = -1;
            try
            {
                rowsAffected = new ServiceSubscriptionCreatePlanMasterDal().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (rowsAffected <= 0)
                return PaymentGatewayMessage.UpdateFail;

            new ServiceSubscriptionPlanDetailDal().DeleteRange(updateRequest.ServiceSubscriptionCreatePlanID);
          
            foreach (ServiceList item in updateRequest.Services)
            {
                new ServiceSubscriptionPlanDetailBal().Insert(updateRequest.ServiceSubscriptionCreatePlanID, item.ServiceID);
            }
            return StatusType.Ok;
        }

        internal List<ViewModel> Select()
        {
            List<ViewModel> ResviewModel = new List<ViewModel>();
            try
            {
                List<ViewModel> MasterviewModel = new ServiceSubscriptionCreatePlanMasterDal().Select();

                foreach (ViewModel item in MasterviewModel)
                {
                    ViewModel responce = new ViewModel();
                    responce.ServiceSubscriptionCreatePlanMasterID = item.ServiceSubscriptionCreatePlanMasterID;
                    responce.PlanName = item.PlanName;
                    responce.Amount = item.Amount;
                    responce.ImageURL = item.ImageURL;
                    responce.Services = new ServiceSubscriptionPlanDetailDal().SelectServiceList(item.ServiceSubscriptionCreatePlanMasterID);
                    ResviewModel.Add(responce);
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ResviewModel;
            }
            return ResviewModel;
        }

        internal ViewModel SelectByID(SelectByIDRequest mainRequest, out string failureMessage)
        {
            ViewModel viewModel = null;
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return viewModel;

            failureMessage = DMTSettingMessages.NotFound;

            viewModel = new ServiceSubscriptionCreatePlanMasterDal().SelectByplanID(mainRequest.ServiceSubscriptionCreatePlanID);
         
            if (viewModel == null) return viewModel;

            viewModel.Services = new ServiceSubscriptionPlanDetailDal().SelectServiceList(mainRequest.ServiceSubscriptionCreatePlanID);

            failureMessage = StatusType.Ok;
            return viewModel;
        }           

        internal AssignPlanViewModel SelectPlanDetail(SelectByIDRequest mainRequest, out string failureMessage)
        {
            AssignPlanViewModel viewModel = null;
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return viewModel;

            failureMessage = DMTSettingMessages.NotFound;

            viewModel = new ServiceSubscriptionCreatePlanMasterDal().SelectByplanDetailID(mainRequest.ServiceSubscriptionCreatePlanID);

            if (viewModel == null) return viewModel;

            failureMessage = StatusType.Ok;
            return viewModel;
        }

        internal string Delete(SelectByIDRequest deleteRequest)
        {
            string failureMessage = deleteRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;
            try
            {
                int rowsAffected = new ServiceSubscriptionCreatePlanMasterDal().Delete(deleteRequest.ServiceSubscriptionCreatePlanID);
                if (rowsAffected <= 0)
                {
                    return SequenceMessage.DefaultsetFail;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            return StatusType.Ok;
        }

        internal List<ViewModel> SelectPlanList(int UserID,SelectByServiceIDRequest request, out string failureMessage)
        {
            List<ViewModel> ResviewModel = new List<ViewModel>();
            try
            {
                ResviewModel = new ServiceSubscriptionCreatePlanMasterDal().SelectPlan(request.ServiceID, UserID);              
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (ResviewModel == null)
            {
                failureMessage = Messages.NotFound;
                return ResviewModel;
            }

            failureMessage= StatusType.Ok;
            return ResviewModel;
        }
    }
}