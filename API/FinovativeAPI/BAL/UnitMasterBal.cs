using Project.DAL;
using Project.Processing;
using Project.Response.UnitMaster;
using Project.StaticModel;
using System;
using System.Collections.Generic;

namespace Project.BAL
{
    public class UnitMasterBal
    {
        internal List<ViewModel> Select(out string failureMessage)
        {
            failureMessage = Messages.DataNotFound;
            List<ViewModel> list = null;
            try
            {
                list = new UnitMasterDal().Select();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (list == null || list.Count <= 0) return list;

            failureMessage = StatusType.Ok;
            return list;
        }
    }
}