using Proj.DAL;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Project.BAL
{
    public class VoucherTypeBal
    {
        public (List<Response.VoucherType.ViewModel>, string) SelectForDropdown()
        {
            List<Project.Response.VoucherType.ViewModel> list = new VoucherTypeDal().SelectForDropdown();
            if (list == null || list.Count <= 0)
            {
                return (list, "NotFound");
            }
            return (list, StaticModel.StatusType.Ok);
        }
    }
}
