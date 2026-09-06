using Project.Model;
using Project.Processing;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class InvoiceNumberHistoryDal
    {
        public static string key
        { get { return "_InvoiceNumberHistory"; } }

        internal void removeCache()
        {
            var cache = new MemoryCacheProcessing<List<InvoiceNumberHistory>>();
            cache.Remove(key);
        }

        internal List<InvoiceNumberHistory> Select()
        {
            var _list = new MemoryCacheProcessing<List<InvoiceNumberHistory>>();

            List<InvoiceNumberHistory> list = _list.GetOrCreate(key, () => SelectWithoutCache());

            return list;
        }

        private List<InvoiceNumberHistory> SelectWithoutCache()
        {
            List<InvoiceNumberHistory> list;
            using (var context = new DatabaseContext())
            {
                list = context.InvoiceNumberHistory.OrderBy(S => S.InvoiceNumberHistoryID).ToList();
            }
            return list;
        }

        public int Insert(InvoiceNumberHistory ObjModel)
        {
            using (var context = new DatabaseContext())
            {
                context.InvoiceNumberHistory.Add(ObjModel);
                int Result = context.SaveChanges();
                return Result;
            }
        }

        internal InvoiceNumberHistory SelectByInvoiceNo(string InvoiceNo)
        {
            InvoiceNumberHistory obj;
            using (var context = new DatabaseContext())
            {
                obj = (from invoice in context.InvoiceNumberHistory
                       where invoice.InvoiceNo == InvoiceNo
                       orderby invoice.InvoiceNumberHistoryID descending
                       select invoice).FirstOrDefault();
            }
            return obj;
        }

        internal InvoiceNumberHistory CheckInvoiceNumber(int UserID, int Month, int Year, string ServiceCategory)
        {
            InvoiceNumberHistory obj = null;
            using (var context = new DatabaseContext())
            {
                obj = (from invoice in context.InvoiceNumberHistory
                       where invoice.UserID == UserID &&
                       invoice.Month == Month &&
                       invoice.Year == Year &&
                       invoice.ServiceCategory == ServiceCategory
                       select invoice).FirstOrDefault();
            }
            return obj;
        }

        internal InvoiceNumberHistory CheckIFInvoiceGeneratedForthisMonth(Requests.Tax.GSTDetailRequest selectRequest)
        {
            InvoiceNumberHistory obj = null;
            using (var context = new DatabaseContext())
            {
                obj = (from invoice in context.InvoiceNumberHistory
                       where
                       invoice.Month == selectRequest.Month &&
                       invoice.Year == selectRequest.Year &&
                       invoice.ServiceCategory == selectRequest.ServiceCategory &&
                       invoice.InvoiceType == selectRequest.InvoiceType

                       select invoice).FirstOrDefault();
            }

            return obj;
        }

        internal int SelectLastIncrementValue(Requests.Tax.GSTDetailRequest selectRequest, int FinancialYear)
        {
            InvoiceNumberHistory obj;
            using (var context = new DatabaseContext())
            {
                obj = (from invoice in context.InvoiceNumberHistory

                       where invoice.InvoiceType == selectRequest.InvoiceType &&
                       invoice.Year == FinancialYear

                       orderby invoice.InvoiceNumberHistoryID descending
                       select invoice).FirstOrDefault();
            }

            if (obj == null)
            {
                return 0;
            }
            return obj.IncrementValue;
        }
    }
}