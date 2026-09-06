using Project.DAL;
using Project.Model;
using Project.Processing;
using Project.Requests.DenominationMaster;
using Project.Response.DenominationMaster;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class DenominationMasterBal
    {
        internal string Insert(InsertRequest insertRequest, out string failureMessage)
        {
            string response = string.Empty;
            failureMessage = insertRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return response;

            DenominationMaster objModel = new DenominationMaster
            {
                DenominationRange = insertRequest.DenominationRange,
                Description = insertRequest.Description,
                IsStop = insertRequest.IsStop
            };

            DenominationMaster denominationMaster = null;
            try
            {
                //     denominationMaster = new DenominationMasterDal().SelectByAmount(insertRequest.MinAmount, insertRequest.MaxAmount);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            failureMessage = Message.DenominationMasterMessage.Dupliactedenomination;
            //   if (denominationMaster != null) return failureMessage;

            failureMessage = Message.DenominationMasterMessage.InsertFail;
            int Result;
            try
            {
                Result = new DenominationMasterDal().Insert(objModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return response;
            }
            if (Result <= 0)
                return response;

            response = Message.DenominationMasterMessage.InsertSuccess;
            failureMessage = StatusType.Ok;
            return response;
        }

        internal List<ViewModel> Select(out string failureMessage)

        {
            List<DenominationMaster> list = null;
            failureMessage = Message.DenominationMasterMessage.NotFound;
            try
            {
                list = new DenominationMasterDal().SelectAll();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            List<ViewModel> viewModel = null;

            if (list == null || list.Count <= 0) return viewModel;

            viewModel = new List<ViewModel>();

            foreach (DenominationMaster denomination in list)
            {
                viewModel.Add(new ViewModel
                {
                    DenominationMasterID = denomination.DenominationMasterID,
                    Description = denomination.Description,
                    DenominationRange = denomination.DenominationRange,
                    IsStop = denomination.IsStop
                });
            }

            failureMessage = StatusType.Ok;
            return viewModel;
        }

        internal string Delete(DeleteRequest deleteRequest, out string failureMessage)
        {
            string response = string.Empty;
            failureMessage = deleteRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return response;

            DenominationMaster ObjModel;
            try
            {
                ObjModel = new DenominationMasterDal().SelectByID(deleteRequest.DenominationMasterID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return response;
            }
            if (ObjModel == null) return Message.DenominationMasterMessage.NotFound;

            failureMessage = Message.DenominationMasterMessage.DeleleFail;
            int Result;
            try
            {
                Result = new DenominationMasterDal().Delete(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return response;
            }
            if (Result <= 0) return response;

            response = Message.DenominationMasterMessage.DeleleSuccess;
            failureMessage = StatusType.Ok;
            return response;
        }

        internal string Update(UpdateRequest updateRequest, out string failureMessage)
        {
            string response = string.Empty;
            failureMessage = updateRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return response;

            DenominationMaster ObjModel;
            try
            {
                ObjModel = new DenominationMasterDal().SelectByID(updateRequest.DenominationMasterID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return response;
            }
            if (ObjModel == null) return Message.DenominationMasterMessage.NotFound;

            ObjModel.DenominationRange = updateRequest.DenominationRange;
            ObjModel.Description = updateRequest.Description;
            ObjModel.IsStop = updateRequest?.IsStop ?? ObjModel.IsStop;

            failureMessage = Message.DenominationMasterMessage.UpdateFail;

            int rowsAffected;
            try
            {
                rowsAffected = new DenominationMasterDal().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return response;
            }
            if (rowsAffected <= 0) return response;

            response = Message.DenominationMasterMessage.UpdateSuccess;
            failureMessage = StatusType.Ok;
            return response;
        }

        internal ViewModel SelectByID(SelectRequest selectRequest, out string failureMessage)

        {
            ViewModel viewModel = null;
            DenominationMaster list = null;
            failureMessage = Message.DenominationMasterMessage.NotFound;
            try
            {
                list = new DenominationMasterDal().SelectByID(selectRequest.DenominationMasterID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (list == null) return viewModel;

            viewModel = new ViewModel
            {
                DenominationMasterID = list.DenominationMasterID,
                DenominationRange = list.DenominationRange,
                Description = list.Description,
                IsStop = list.IsStop
            };

            failureMessage = StatusType.Ok;
            return viewModel;
        }
    }
}