using Project.DAL;
using Project.Model;
using System.Collections.Generic;
using System.Linq;

namespace Project.BAL
{
    public class AuthorizationBAL
    {
        private readonly AuthorizationDAL ObjDal = new AuthorizationDAL();

        internal bool Select(string controller, string action, int UserTypeID)
        {
            List<AuthorizationMaster> list = ObjDal.Select();

            list = (from item in list
                    where item.Controller == controller && item.Action == action && item.UserTypeID == UserTypeID
                    select item).ToList();

            if (list == null)
            {
                return false;
            }

            if (list.Count <= 0)
            {
                return false;
            }

            return true;
        }
    }
}