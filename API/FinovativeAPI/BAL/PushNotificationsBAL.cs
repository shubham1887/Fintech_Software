using Project.DAL;
using Project.Message;
using Project.Processing;
using Project.Requests.PushNotifications;
using Project.Response.User;
using Project.StaticModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.BAL
{
    public class PushNotificationsBAL
    {
        internal string SendPushNotification(SendPushNotificationRequest mainRequest, out string failureMessage)
        {
            string response = string.Empty;
            failureMessage = mainRequest.IsValidModalState();

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok))
                return response;

            List<UserTypeList> userTypeList = mainRequest.UserTypeList;

            userTypeList = userTypeList.GroupBy(x => x.UserTypeID)
                    .Select(x => x.FirstOrDefault()).ToList();

            failureMessage = PushNotificationMessage.SendPushNotificationFail;

            FCMProcessing fcmProcessing = new FCMProcessing();

            foreach (UserTypeList userType in userTypeList)
            {
                List<UserViewModel> userViewModels = new UserDAL().SelectByType(userType.UserTypeID);

                foreach (UserViewModel user in userViewModels)
                {
                    try
                    {
                        Task.Run(() =>
                        {
                            fcmProcessing.SendNoticationMessage(user.FcmViewModel.AppFCMToken, mainRequest.Description, mainRequest.Title, "DEFAULT", 10000, PlatformCode.App);
                        }
                        );
                        Task.Run(() =>
                        {
                            fcmProcessing.SendNoticationMessage(user.FcmViewModel.WebFCMToken, mainRequest.Description, mainRequest.Title, "DEFAULT", 10000, PlatformCode.Web);
                        }
                        );
                    }
                    catch (Exception ex)
                    {
                        new Logger().Write(ex);
                    }
                }
            }

            response = PushNotificationMessage.SendPushNotificationSuccess;
            failureMessage = StatusType.Ok;
            return response;
        }
    }
}