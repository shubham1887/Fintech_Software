using Project.DAL;
using Project.Model;
using Project.Processing;
using Project.Requests.RedirectUrl;
using Project.Response.RedirectUrl;
using Project.StaticModel;
using System;

namespace Project.BAL
{
    public class RedirectUrlMasterBal
    {
        internal ViewModel SelectRedirectUrlByName(SelectRequest selectRequest,int UserID, out string failureMessage)
        {
            ViewModel viewModel = null;
            UserMaster user = new UserDAL().SelectByIDFromCache(UserID);
            failureMessage = selectRequest.IsValidModalState();
            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok)) return viewModel;
            try
            {
                viewModel = new RedirectUrlMasterDal().SelectUrlByName(selectRequest.Name);
                viewModel.Url = viewModel.Url.Replace("[MobileNumber]",user.MobileNumber);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (viewModel == null)
            {
                failureMessage = Message.RedirectUrlMessage.UrlNotFound;
                return viewModel;
            }
            failureMessage = StatusType.Ok;
            return viewModel;
        }
    }
}