using Project.DAL;
using Project.Model;
using Project.Requests.Tax;

namespace Project.BAL
{
    public class InvoiceNumberHistoryBal
    {
        internal string InsertAndGetInvoiceNumberFromHistory(GSTDetailRequest selectRequest, int UserID, string InvoiceNumber, string ServiceCategory, int SequenceNumber, int FinancialYear)
        {
            InvoiceNumberHistory settings = null;
            settings = new InvoiceNumberHistoryDal().CheckInvoiceNumber(UserID, selectRequest.Month, FinancialYear, ServiceCategory);

            if (settings != null)
                return settings.InvoiceNo;

            InvoiceNumberHistory invoice = null;
            if (settings == null)
            {
                invoice = new InvoiceNumberHistory();
                invoice.UserID = UserID;
                invoice.Year = FinancialYear;
                invoice.Month = selectRequest.Month;
                invoice.InvoiceNo = InvoiceNumber;
                invoice.ServiceCategory = ServiceCategory;
                invoice.IncrementValue = SequenceNumber;
                invoice.InvoiceType = selectRequest.InvoiceType;
                new InvoiceNumberHistoryDal().Insert(invoice);
            }

            return invoice.InvoiceNo;
        }

        internal bool CheckIFInvoiceGeneratedForthisMonth(GSTDetailRequest selectRequest)
        {
            InvoiceNumberHistory mothwiseinvoice = null;
            mothwiseinvoice = new InvoiceNumberHistoryDal().CheckIFInvoiceGeneratedForthisMonth(selectRequest);

            if (mothwiseinvoice == null)
                return false;

            return true;
        }
    }
}