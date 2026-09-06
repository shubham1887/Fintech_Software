using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.Provider;
using Project.Response.Provider;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class ProviderDetailsBAL
    {
        internal string Insert(ProviderDetailsInsertRequest insertRequest, out string failureMessage)
        {
            string response = string.Empty;
            failureMessage = insertRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            if (Company.IsWhiteLabel.EqualsIgnoreCase("Yes"))
            {
                failureMessage = Messages.WhiteLableUser;
                return failureMessage;
            }

            ProviderDetails ObjModel = new ProviderDetails
            {
                ServiceID = insertRequest.ServiceID,
                ProviderID = insertRequest.ProviderID,
                MethodType = insertRequest.MethodType,
                UrlType = insertRequest.UrlType,
                ProviderURL = insertRequest.ProviderURL,
                HeaderJson = insertRequest.HeaderJson ?? "",
                DateFormat = insertRequest.DateFormat ?? "",
                TransactionIDPrefix = insertRequest.TransactionIDPrefix ?? "",
                IsActive = true,
                IsDelete = false,
            };

            bool isInsert = true;
            try
            {
                ProviderDetails providerDetails = new ProviderDetailsDAL().SelectByID(insertRequest.ProviderDetailID);

                if (providerDetails != null)
                {
                    isInsert = false;
                    ObjModel = providerDetails;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (isInsert)
            {
                if (IsDuplicateDetails(insertRequest.ProviderID, insertRequest.ServiceID, insertRequest.UrlType))
                {
                    return ProviderMessage.DuplicateDetails;
                }

                try
                {
                    failureMessage = ProviderMessage.InsertProviderFail;
                    int rowsAffected = new ProviderDetailsDAL().Insert(ObjModel);
                    if (rowsAffected <= 0) return failureMessage;
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                    return Messages.ExceptionOccured;
                }
                failureMessage = StatusType.Ok;
                return ProviderMessage.InsertProviderSuccess;
            }

            ObjModel.ServiceID = insertRequest.ServiceID;
            ObjModel.ProviderID = insertRequest.ProviderID;
            ObjModel.UrlType = insertRequest.UrlType;
            ObjModel.ProviderURL = insertRequest.ProviderURL;
            ObjModel.MethodType = insertRequest.MethodType;
            ObjModel.HeaderJson = insertRequest.HeaderJson ?? "";
            ObjModel.DateFormat = insertRequest.DateFormat ?? "";
            ObjModel.TransactionIDPrefix = insertRequest.TransactionIDPrefix ?? "";
            ObjModel.UpdatedDOC = DateTime.Now;

            failureMessage = ProviderMessage.ProviderUpdateFail;
            try
            {
                int rowsAffected = new ProviderDetailsDAL().Update(ObjModel);
                if (rowsAffected <= 0) return response;
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            failureMessage = StatusType.Ok;
            return ProviderMessage.ProviderUpdateSuccess;
        }

        private bool IsDuplicateDetails(int ProviderID, int ServiceID, string Type)
        {
            ProviderDetails providerDetails = new ProviderDetailsDAL().SelectForDuplicate(ProviderID, ServiceID, Type);
            if (providerDetails != null)
                return true;

            return false;
        }

        internal string ActiveDeactive(ProviderDetailsSelectRequest providerDetailsRequest)
        {
            string failureMessage = providerDetailsRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            if (Company.IsWhiteLabel.EqualsIgnoreCase("Yes"))
            {
                failureMessage = Messages.WhiteLableUser;
                return failureMessage;
            }

            ProviderDetails ObjModel = null;
            try
            {
                ObjModel = new ProviderDetailsDAL().SelectByID(providerDetailsRequest.ProviderDetailID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null)
            {
                return ProviderMessage.ProviderNotFound; ;
            }
            ObjModel.IsActive = providerDetailsRequest.IsActive;
            ObjModel.UpdatedDOC = DateTime.Now;

            try
            {
                int rowsAffected = new ProviderDetailsDAL().Update(ObjModel);
                if (rowsAffected <= 0)
                {
                    return ProviderMessage.ProviderActiveDeactiveFail;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            return StatusType.Ok;
        }

        internal List<DetailsViewModel> Select()
        {
            List<DetailsViewModel> ReturnList = null;
            try
            {
                ReturnList = new ProviderDetailsDAL().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ReturnList;
            }
            return ReturnList;
        }

        internal string Update(ProviderDetailsUpdateRequest updateRequest)
        {
            string failureMessage = updateRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            if (Company.IsWhiteLabel.EqualsIgnoreCase("Yes"))
            {
                failureMessage = Messages.WhiteLableUser;
                return failureMessage;
            }
            ProviderDetails ObjModel = null;
            try
            {
                ObjModel = new ProviderDetailsDAL().SelectByID(updateRequest.ProviderDetailID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null)
            {
                return ProviderMessage.ProviderNotFound; ;
            }
            ObjModel.ServiceID = updateRequest.ServiceID;
            ObjModel.ProviderID = updateRequest.ProviderID;
            ObjModel.UrlType = updateRequest.UrlType;
            ObjModel.ProviderURL = updateRequest.ProviderURL;
            ObjModel.MethodType = updateRequest.MethodType;
            ObjModel.HeaderJson = updateRequest.HeaderJson ?? "";
            ObjModel.DateFormat = updateRequest.DateFormat ?? "";
            ObjModel.TransactionIDPrefix = updateRequest.TransactionIDPrefix ?? "";

            ObjModel.UpdatedDOC = DateTime.Now;
            try
            {
                int rowsAffected = new ProviderDetailsDAL().Update(ObjModel);

                if (rowsAffected <= 0)
                {
                    return ProviderMessage.ProviderUpdateFail;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }

            return StatusType.Ok;
        }

        internal DetailsViewModel SelectByProviderDetailID(ProviderDetailsSelectRequest selectRequest, out string failureMessage)
        {
            DetailsViewModel ProviderDetails = new DetailsViewModel();
            failureMessage = selectRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return ProviderDetails;
            try
            {
                ProviderDetails = new ProviderDetailsDAL().SelectByProviderDetailID(selectRequest.ProviderDetailID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return ProviderDetails;
        }

        internal string Delete(ProviderDetailsSelectRequest deleteRequest)
        {
            string failureMessage = deleteRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            if (Company.IsWhiteLabel.EqualsIgnoreCase("Yes"))
            {
                failureMessage = Messages.WhiteLableUser;
                return failureMessage;
            }
            ProviderDetails ObjModel = null;
            try
            {
                ObjModel = new ProviderDetailsDAL().SelectByID(deleteRequest.ProviderDetailID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null)
            {
                return ProviderMessage.ProviderNotFound;
            }

            ObjModel.ProviderDetailID = deleteRequest.ProviderDetailID;
            ObjModel.IsDelete = true;
            ObjModel.UpdatedDOC = DateTime.Now;

            try
            {
                int rowsAffected = new ProviderDetailsDAL().Update(ObjModel);
                if (rowsAffected <= 0)
                {
                    return ProviderMessage.ProviderDeleteFail;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            return StatusType.Ok;
        }

        internal ProviderDetails SelectURL(int ProviderID, int ServiceID, string UrlType)
        {
            ProviderDetails providerDetails = null;
            try
            {
                if (ServiceID == (int)ServiceCode.Mobile ||
                    ServiceID == (int)ServiceCode.Dth ||
                    ServiceID == (int)ServiceCode.PostPaid
                    )
                {
                    providerDetails = new ProviderDetailsDAL().SelectURL(ProviderID, ServiceID, UrlType);
                    if (providerDetails == null)
                        providerDetails = new ProviderDetailsDAL().SelectURLForMobile(ProviderID, UrlType);
                }
                else
                {
                    providerDetails = new ProviderDetailsDAL().SelectURL(ProviderID, ServiceID, UrlType);
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return providerDetails;
        }

        internal List<DetailsViewModel> SelectByProviderID(ProviderRequest selectRequest)
        {
            List<DetailsViewModel> ProviderDetailViewModel = new List<DetailsViewModel>();

            try
            {
                ProviderDetailViewModel = new ProviderDetailsDAL().SelectByProviderID(selectRequest.ProviderID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            return ProviderDetailViewModel;
        }
    }
}