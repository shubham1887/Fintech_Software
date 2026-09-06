using Project.Model;
using System.Linq;

namespace Project.DAL
{
    public class UserWisePlanLimitDal
    {
       
        internal UserWisePlanLimit SelectByUserID(int UserID)
        {
            using var context = new DatabaseContext();

            return context.UserWisePlanLimit.Where(u => u.UserID == UserID && u.IsActive && !u.IsDelete ).FirstOrDefault();
        }
     
    }
}
