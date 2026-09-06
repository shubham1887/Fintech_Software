using Project.Model;
using System.Linq;

namespace Project.DAL
{
    public class UserPlanUtilizationDal
    {

        internal UserPlanUtilization SelectByUserID(int UserID)
        {
            using var context = new DatabaseContext();

            return context.UserPlanUtilization.Where(u=> u.UserID == UserID).FirstOrDefault();
        }

    }
}
