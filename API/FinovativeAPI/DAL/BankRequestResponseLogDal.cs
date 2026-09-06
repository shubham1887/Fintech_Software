using Project.Model;

namespace Project.DAL
{
    public class BankRequestResponseLogDal
    {
        public int Insert(BankRequestResponseLog ObjModel)
        {
            int result = -1;
            using (var context = new DatabaseContext())
            {
                context.BankRequestResponseLog.Add(ObjModel);
                context.SaveChanges();
                result = ObjModel.BankRequestResponseLogID;
            }
            return result;
        }
    }
}
