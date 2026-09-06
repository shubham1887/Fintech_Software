using Project.Model;
using System.Linq;

namespace Project.DAL
{
    public class UpiSenderOTPDal
    {
        internal SenderOTP SelectByID(int UserID, UpiSender sender)
        {
            SenderOTP obj;
            using (var context = new DatabaseContext())
            {
                obj = (from senderOTP in context.SenderOTP
                       where senderOTP.SenderID == sender.UpiSenderID
                       orderby senderOTP.SenderOTPID descending
                       select senderOTP).FirstOrDefault();
            }
            return obj;
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
    }
}
