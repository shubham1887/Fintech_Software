using Project.Model;
using System.Linq;

namespace Project.DAL
{
    public class SenderOTPDAL
    {
        public int Insert(SenderOTP ObjModel)
        {
            using (var context = new DatabaseContext())
            {
                context.SenderOTP.Add(ObjModel);
                int Result = context.SaveChanges();
                if (Result <= 0)
                    return 0;
                return ObjModel.SenderOTPID;
            }
        }

        internal int Update(SenderOTP ObjModel)
        {
            using (var context = new DatabaseContext())
            {
                context.SenderOTP.Update(ObjModel);
                context.SaveChanges();
                return ObjModel.SenderOTPID;
            }
        }

        internal SenderOTP SelectByID(int UserID, Sender sender)
        {
            SenderOTP obj;
            using (var context = new DatabaseContext())
            {
                obj = (from senderOTP in context.SenderOTP
                       where senderOTP.SenderID == sender.SenderID
                       //&& senderOTP.UserID == UserID // need discussion here about, is sender will consume dmt on one retailer acount or many
                       orderby senderOTP.SenderOTPID descending
                       select senderOTP).FirstOrDefault();
            }
            return obj;
        }

        internal void Delete(int DeleteDOCInt)
        {
            using (var context = new DatabaseContext())
            {
                var selectedrecord = context.SenderOTP.Where(U => U.DOCInt <= DeleteDOCInt);
                context.SenderOTP.RemoveRange(selectedrecord);
                context.SaveChanges();
            }
        }
    }
}