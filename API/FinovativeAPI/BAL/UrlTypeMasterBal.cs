using Project.DAL;
using Project.Processing;
using Project.Response.URLType;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class UrlTypeMasterBal
    {
        internal List<ViewModel> Select()
        {
            List<ViewModel> ReturnList = null;
            try
            {
                ReturnList = new UrlTypeMasterDal().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return ReturnList;
            }
            return ReturnList;
        }
    }
}