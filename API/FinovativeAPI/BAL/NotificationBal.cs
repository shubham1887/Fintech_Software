using Project.DAL;
using Project.Message;
using Project.Model;
using Project.Processing;
using Project.Requests.Notification;
using Project.Response.Notification;
using Project.StaticModel;
using System;

namespace Project.BAL
{
    public class NotificationBal
    {
        internal string Insert(InsertRequest insertRequest)
        {
            string failureMessage = insertRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return failureMessage;

            Notification ObjModel = new Notification
            {
                NotificationText = insertRequest.NotificationText,
            };

            Notification model = null;
            try
            {
                model = new NotificationDal().SelectByID(insertRequest.NotificationID);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            if (model == null)
            {
                try
                {
                    int rowsAffected = new NotificationDal().Insert(ObjModel);
                    if (rowsAffected <= 0)
                        return NotificationMessage.InsertFail;
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                    return Messages.ExceptionOccured;
                }

                return StatusType.Ok;
            }
            else
            {
                model.NotificationText = insertRequest.NotificationText;
                model.UpdatedDOC = DateTime.Now;
                try
                {
                    int rowsAffected = new NotificationDal().Update(model);
                    if (rowsAffected <= 0)
                        return NotificationMessage.UpdateFail;
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }
                return StatusType.Ok;
            }
        }

        internal ViewModel Select()
        {
            Notification notification = null;
            ViewModel response = new ViewModel();
            try
            {
                notification = new NotificationDal().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return response;
            }
            if (notification == null)
                return response;

            response.NotificationID = notification.NotificationID;
            response.NotificationText = notification.NotificationText;
            return response;
        }
    }
}