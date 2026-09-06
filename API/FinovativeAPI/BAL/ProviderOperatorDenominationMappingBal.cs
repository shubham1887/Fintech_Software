using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.ProviderOperatorDenominationMapping;
using Project.Response.ProviderOperatorDenominationMapping;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.BAL
{
    public class ProviderOperatorDenominationMappingBal
    {
        internal string Insert(InsertRequest mainRequest, out string failureMessage)
        {
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;

            ProviderOperatorDenominationMapping ObjModel = new ProviderOperatorDenominationMapping
            {
                ProviderID = mainRequest.ProviderID,
                OperatorID = mainRequest.OperatorID,
                DenominationMasterID = mainRequest.DenominationMasterID
            };

            if (AlreadyExists(ObjModel))
            {
                failureMessage = ProviderOperatorDenominationMappingMessage.AllreadyExists;
                return failureMessage;
            }

            int Result = -1;
            try
            {
                Result = new ProviderOperatorDenominationMappingDal().Insert(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            failureMessage = ProviderOperatorDenominationMappingMessage.InsertFail;
            if (Result <= -1) return failureMessage;

            failureMessage = StatusType.Ok;
            return ProviderOperatorDenominationMappingMessage.InsertSuccess;
        }

        private bool AlreadyExists(ProviderOperatorDenominationMapping mainRequest)
        {
            List<ViewModel> response = null;
            try
            {
                response = new ProviderOperatorDenominationMappingDal().SelectByProviderAndOperatorID(mainRequest.ProviderID, mainRequest.OperatorID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (response == null || response.Count <= 0) return false;

            if (response.Exists(r => r.ProviderID == mainRequest.ProviderID && r.OperatorID == mainRequest.OperatorID && r.DenominationMasterID == mainRequest.DenominationMasterID))
                return true;

            return false;
        }

        //Todo: Testing not done
        internal string Update(UpdateRequest mainRequest, out string failureMessage)
        {
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;
            ProviderOperatorDenominationMapping ObjModel = null;
            try
            {
                ObjModel = new ProviderOperatorDenominationMappingDal().SelectByPrimaryID(mainRequest.ProviderOperatorDenominationMappingID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            failureMessage = ProviderOperatorDenominationMappingMessage.NotFound;
            if (ObjModel == null)
                return failureMessage;

            ObjModel.ProviderID = mainRequest.ProviderID;
            ObjModel.OperatorID = mainRequest.OperatorID;
            ObjModel.DenominationMasterID = mainRequest.DenominationMasterID;

            int Result = -1;
            try
            {
                Result = new ProviderOperatorDenominationMappingDal().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            failureMessage = ProviderOperatorDenominationMappingMessage.UpdateFail;
            if (Result <= -1) return failureMessage;

            failureMessage = StatusType.Ok;
            return ProviderOperatorDenominationMappingMessage.UpdateSuccess;
        }

        //  internal List<ViewModel> Select(out string failureMessage)
        internal List<DenominationSwitchingModel> Select(out string failureMessage, SelectMappingRequest selectMappingRequest)
        {
            List<DenominationSwitchingModel> response = null;

            failureMessage = selectMappingRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;

            failureMessage = ProviderOperatorDenominationMappingMessage.NotFound;

            try
            {
                //  response = new ProviderOperatorDenominationMappingDal().SelectAll();
                response = new ProviderOperatorDenominationMappingDal().SelectForUI(selectMappingRequest.ServiceID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (response == null) return response;

            failureMessage = StatusType.Ok;
            return response;
        }

        internal string Delete(DeleteRequest mainRequest, out string failureMessage)
        {
            failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return failureMessage;
            int Result = -1;
            ProviderOperatorDenominationMapping ObjModel = null;
            failureMessage = ProviderOperatorDenominationMappingMessage.NotFound;
            try
            {
                ObjModel = new ProviderOperatorDenominationMappingDal().SelectByPrimaryID(mainRequest.ProviderOperatorDenominationMappingID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (ObjModel == null) return failureMessage;

            failureMessage = ProviderOperatorDenominationMappingMessage.DeleteFail;

            try
            {
                Result = new ProviderOperatorDenominationMappingDal().Delete(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (Result <= 0) return failureMessage;

            failureMessage = StatusType.Ok;
            return ProviderOperatorDenominationMappingMessage.DeleteSuccess;
        }

        internal ViewModel SelectByID(SelectByIDRequest mainRequest, out string failureMessage)
        {
            ViewModel response = null;
            failureMessage = mainRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return response;

            try
            {
                response = new ProviderOperatorDenominationMappingDal().SelectByID(mainRequest.ProviderOperatorDenominationMappingID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (response == null) return response;

            failureMessage = StatusType.Ok;
            return response;
        }

        internal List<ViewModel> SelectDenominationMappingByProviderIDAndOperatorID(SelectDenominationMappingByProviderIDAndOperatorIDRequest mainRequest, out string failureMessage)
        {
            failureMessage = ProviderOperatorDenominationMappingMessage.NotFound;
            List<ViewModel> response = null;
            List<Project.Response.DenominationMaster.ViewModel> DenominationResponse = null;

            try
            {
                DenominationResponse = new DenominationMasterBal().Select(out failureMessage);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (DenominationResponse == null || DenominationResponse.Count <= 0) return response;

            try
            {
                response = new ProviderOperatorDenominationMappingDal().SelectAll();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            response = response.Where(r => r.ProviderID == mainRequest.ProviderID && r.OperatorID == mainRequest.OperatorID).ToList();

            foreach (Project.Response.DenominationMaster.ViewModel Denomination in DenominationResponse)
            {
                ViewModel objModel = new ViewModel
                {
                    DenominationMasterID = Denomination.DenominationMasterID,
                    DenominationRange = Denomination.DenominationRange,
                    //  MaxAmount = Denomination.MaxAmount,
                    // MinAmount = Denomination.MinAmount,
                    Description = Denomination.Description,
                    OperatorID = mainRequest.OperatorID,
                    ProviderID = mainRequest.ProviderID,
                    IsStop = Denomination.IsStop,
                };

                if (!response.Exists(r => r.DenominationMasterID == objModel.DenominationMasterID))
                    response.Add(objModel);
            }

            if (response == null || response.Count <= 0) return response;

            response = response.OrderBy(res => res.DenominationRange).ToList();

            failureMessage = StatusType.Ok;
            return response;
        }

        internal List<ViewModel> SelectDenominationMapping(out string failureMessage)
        {
            failureMessage = ProviderOperatorDenominationMappingMessage.NotFound;
            List<ViewModel> response = null;
            try
            {
                response = new ProviderOperatorDenominationMappingDal().SelectAll();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (response == null) return response;

            failureMessage = StatusType.Ok;
            return response;
        }
    }
}