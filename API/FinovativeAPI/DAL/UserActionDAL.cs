using Project.Model;

namespace Project.DAL
{
    public class UserActionDAL
    {
        internal async void Insert(UserActionMaster ObjModel)
        {
            using (var context = new DatabaseContext())
            {
                context.UserActionMaster.Add(ObjModel);
                await context.SaveChangesAsync();
            }
        }
    }
}