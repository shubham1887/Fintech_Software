using Project.DAL;
using Project.Model;
using Project.Processing;
using Project.Requests;
using Project.Requests.SentSms;
using Project.Response.SentSms;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.BAL
{
    public class SentSmsBAL
    {
        internal ReportWrapper SelectForFilter(SelectRequest selectRequest, KeyDataModel keyDataModel, out string failureMessage)
        {
            ReportWrapper wrapper = new ReportWrapper();
            List<ViewModel> list = new List<ViewModel>();

            failureMessage = selectRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;

            try
            {
                failureMessage = FunctionalValidationProcessing.IsValidMPinForSentSMS(keyDataModel.UserID, selectRequest.MPin.ToString());
            }
            catch (Exception ex)
            {
                failureMessage = Messages.ExceptionOccured;
                new Logger().Write(ex);
                return wrapper;
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return wrapper;

            try
            {
                list = new SentSmsDAL().SelectForFilter(selectRequest);
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

            list = list.AsQueryable().WhereDynamic(selectRequest.Search).ToList();

            list = list.AsQueryable().OrderByDynamic(selectRequest.SortColumnName, selectRequest.SortDirection).ToList();
            wrapper.Count = list.Count;
            if (!selectRequest.IsExcelDownload)
            {
                list = list.AsQueryable().SkipAndTakeCount(selectRequest.SkipCount, selectRequest.TakeCount).ToList();
            }

            wrapper.list = list;
            wrapper.IsExcelDownload = selectRequest.IsExcelDownload;
            failureMessage = StatusType.Ok;
            return wrapper;
        }

        public SentSMS SelectForDuplicate(int UserID, string Mobile, string type, string TemplateID)
        {
            try
            {
                return new SentSmsDAL().SelectForDuplicate(UserID, Mobile, type, TemplateID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return null;
            }
        }

        public SentSMS SelectDuplicateByUrlAndUserID(SentSMS sentSMSModel, string url)
        {
            try
            {
                return new SentSmsDAL().SelectDuplicateByUrlAndUserID(sentSMSModel, url);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return null;
            }
        }
    }
}