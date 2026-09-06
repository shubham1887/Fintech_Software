using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.EnquiryDetails;
using Project.Response.EnquiryDetails;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project.BAL
{
    public class EnquiryDetailsBal
    {
        internal string Insert(InsertRequest insertRequest)
        {
            string failureMessage = insertRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            EnquiryDetails ObjModel = new EnquiryDetails
            {
                PersonName = insertRequest.PersonName,
                MobileNumber = insertRequest.MobileNumber,
                EmailID = insertRequest.EmailID,
                Address = insertRequest.Address,
                EnquiryType = insertRequest.EnquiryType,
                Remark = insertRequest.Remark,
                AdminStatus = EnquiryStatusType.Pending,
            };
            try
            {
                int rowsAfftected = new EnquiryDetailsDal().Insert(ObjModel);

                if (rowsAfftected <= 0)
                {
                    return EnquiryDetailsMessage.InsertFail;
                }
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                failureMessage = Messages.ExceptionOccured;
                return failureMessage;
            }
            new MessageSelection().SendEnquiryMessage(insertRequest.PersonName, insertRequest.MobileNumber, insertRequest.EmailID, IsSubmit: true);
            return StatusType.Ok;
        }

        internal List<ViewModel> Select(SelectRequest selectRequest)
        {
            List<ViewModel> viewModel = null;
            try
            {
                viewModel = new EnquiryDetailsDal().SelectByFilter(selectRequest);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return viewModel;
            }
            return viewModel;
        }

        internal string Update(UpdateRequest updateRequest)
        {
            string failureMessage = updateRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            EnquiryDetails ObjModel = null;
            try
            {
                ObjModel = new EnquiryDetailsDal().SelectByID(updateRequest.EnquiryDetailsID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (ObjModel == null) return failureMessage;
            ObjModel.AdminRemark = updateRequest.AdminRemark;
            ObjModel.AdminStatus = EnquiryStatusType.Completed;
            int rowsAffected = -1;
            try
            {
                rowsAffected = new EnquiryDetailsDal().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return Messages.ExceptionOccured;
            }
            if (rowsAffected <= 0)
                return CallMeRequestMessage.UpdateFail;

            Task.Run(() => new MessageSelection().SendEnquiryMessage(ObjModel.PersonName, ObjModel.MobileNumber, ObjModel.EmailID));
            return StatusType.Ok;
        }
    }
}