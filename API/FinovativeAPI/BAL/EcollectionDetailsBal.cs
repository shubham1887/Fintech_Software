using Project.DAL;
using Project.Model;
using Project.Processing;
using Project.Requests.Fund;
using Project.Response.Fund;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class EcollectionDetailsBal
    {
        internal void Update(int EcollectionDetailsID, string message)
        {
            EcollectionDetails ObjModel = null;
            try
            {
                ObjModel = new EcollectionDetailsDal().SelectByID(EcollectionDetailsID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            ObjModel.Status = StatusType.Success;
            ObjModel.Reason = message;
            if (!message.EqualsIgnoreCase(StatusType.Ok))
                ObjModel.Status = StatusType.Failure;

            int rowsAffected = -1;
            try
            {
                rowsAffected = new EcollectionDetailsDal().Update(ObjModel);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
        }

        internal EcollectionWrapper SelectEcollection(SelectEcollectionRequest selectRequest, out string failureMessage)
        {
            EcollectionWrapper wrapper = new EcollectionWrapper();
            failureMessage = selectRequest.IsValidModalState();
            List<EcollectionViewModel> list = new List<EcollectionViewModel>();
            try
            {
                list = new EcollectionDetailsDal().SelectEcollection(selectRequest);
            }
            catch (Exception ex)
            {
                failureMessage = Messages.ExceptionOccured;
                new Logger().Write(ex);
                return wrapper;
            }

            if (list == null)
            {
                failureMessage = Messages.DataNotFound;
                return wrapper;
            }

            if (list.Count <= 0)
            {
                failureMessage = Messages.DataNotFound;
                return wrapper;
            }
            wrapper.HeaderSummary = new EcollectionDetailsDal().GetHeaderSummaryData(selectRequest);

            wrapper.IsExcelDownload = selectRequest.IsExcelDownload;
            wrapper.Count = list.Count;
            wrapper.list = list;
            failureMessage = StatusType.Ok;
            return wrapper;
        }
    }
}