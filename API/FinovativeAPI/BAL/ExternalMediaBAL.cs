using Project.DAL;
using Project.Processing;
using Project.Requests.ExternalMedia;
using Project.Response.ExternalMedia;
using System;

namespace Project.BAL
{
    public class ExternalMediaBal
    {

        internal ViewModel Select(SelectRequest selectRequest)
        {
            ViewModel list = new ViewModel();

            try
            {
                list = new ExternalMediaDal().SelectByKey(selectRequest.Key);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return list;
            }

            return list;
        }
    }
}