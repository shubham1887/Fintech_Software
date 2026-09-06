using Project.DAL;
using Project.Processing;
using System;

namespace Project.BAL
{
    public class VirtualAccountDetailsBal
    {

        internal Response.VirtualAccountDetails.ViewModel Select(int userID, string ProviderName, string Key)
        {
            Response.VirtualAccountDetails.ViewModel response = new Response.VirtualAccountDetails.ViewModel();

            try
            {
                response = new VirtualAccountDetailDal().SelectByID(userID, ProviderName, Key);

                if (response == null)
                    return response;

            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }

            return response;
        }

    }
}