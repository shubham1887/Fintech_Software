using Project.DAL;
using Project.Processing;
using Project.Requests.User;
using Project.Response.GSTState;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project.BAL
{
    public class ZoneMasterBal
    {
        private readonly ZoneMasterDAL ObjDal = new ZoneMasterDAL();

        internal List<ZoneViewModel> Select()
        {
            List<ZoneViewModel> states = new List<ZoneViewModel>();

            try
            {
                states = ObjDal.SelectAll();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return states;
            }

            return states;
        }

        internal List<ZoneViewModel> SelectStateForDropDown(SelectStateForDropDownRequest mainRequest)
        {
            List<ZoneViewModel> states = new List<ZoneViewModel>();

            try
            {
                states = ObjDal.SelectAll();
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
                return states;
            }

            if (states == null)
                return states;

            if (states.Count <= 0)
                return states;

            if (mainRequest != null)
            {
                if (!string.IsNullOrEmpty(mainRequest.Name))
                    states = states.AsQueryable().WhereDynamic(mainRequest.Name).ToList();
            }

            return states;
        }
    }
}