using Microsoft.EntityFrameworkCore;
using Project.Model;
using Project.StaticModel;
using System;
using System.Linq;

namespace Project.DAL
{
    public class InboxDAL
    {
        internal int Insert(Inbox inbox)
        {

            using (var context = new DatabaseContext())
            {
                context.Inbox.Add(inbox);
                try
                {
                    context.SaveChanges();
                }
                catch (DbUpdateException ex)
                {
                }
                return inbox.InboxID;
            }

        }

        internal bool IsDuplicateInbox(Inbox inbox)
        {
            int RepeatTime = 1;
            Inbox dbInbox = null;

            using (var context = new DatabaseContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = Microsoft.EntityFrameworkCore.QueryTrackingBehavior.NoTracking;
                int DOCInt = Convert.ToInt32(DateTime.Now.Date.ToString(Messages.DOCIntegerFomrat));

                dbInbox = (from item in context.Inbox
                           where item.DOCInt == DOCInt &&
                            item.ServiceID == inbox.ServiceID &&
                            item.OperatorID == inbox.OperatorID &&
                            item.UserID == inbox.UserID &&
                            item.Amount == inbox.Amount &&
                            item.ConsumerNumber == inbox.ConsumerNumber
                           orderby item.DOC descending
                           select item).FirstOrDefault();
            }

            if (dbInbox == null)
            {
                return false;
            }

            int lastRchDateInt = dbInbox.DOCInt;

            int docINT = Int32.Parse(DateTime.Now.ToString(Messages.DOCIntegerFomrat));
            if ((docINT - lastRchDateInt) >= 1)
            {
                return false;
            }

            DateTime rchDate = dbInbox.DOC;
            double diff = (DateTime.Now - dbInbox.DOC).TotalSeconds;

            if (diff > (RepeatTime * 60))
            {
                return false;
            }

            return true;
        }

        internal void Delete(int DeleteDOCInt)
        {
            using (var context = new DatabaseContext())
            {
                var selectedrecord = context.Inbox.Where(U => U.DOCInt <= DeleteDOCInt);
                context.Inbox.RemoveRange(selectedrecord);
                context.SaveChanges();
            }
        }
    }
}