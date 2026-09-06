using Project.DAL;
using Project.Response.DateFormat;
using System.Collections.Generic;

namespace Project.BAL
{
    public class DateFormatMasterBAL
    {
        private readonly DateFormatMasterDAL ObjDal = new DateFormatMasterDAL();

        internal List<ViewModel> Select()
        {
            List<ViewModel> list = ObjDal.Select();

            return list;
        }
    }
}