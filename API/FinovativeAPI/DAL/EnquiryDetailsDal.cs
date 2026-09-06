using Project.Model;
using Project.Processing;
using Project.Requests.EnquiryDetails;
using Project.Response.EnquiryDetails;
using Project.StaticModel;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Project.DAL
{
    public class EnquiryDetailsDal
    {
        public static string Key
        { get { return "_EnquiryDetails"; } }

        private void removeCache()
        {
            var cache = new MemoryCacheProcessing<List<ViewModel>>();
            cache.Remove(Key);
        }

        internal int Insert(EnquiryDetails model)
        {
            removeCache();

            using (var context = new DatabaseContext())
            {
                context.EnquiryDetails.Add(model);
                return context.SaveChanges();
            }
        }

        internal int Update(EnquiryDetails model)
        {
            removeCache();

            using (var context = new DatabaseContext())
            {
                context.EnquiryDetails.Update(model);
                return context.SaveChanges();
            }
        }

        internal EnquiryDetails SelectByID(int EnquiryDetailsID)
        {
            using var context = new DatabaseContext();
            return context.EnquiryDetails.Find(EnquiryDetailsID);
        }

        internal List<ViewModel> Select()
        {
            var _list = new MemoryCacheProcessing<List<ViewModel>>();
            List<ViewModel> list = _list.GetOrCreate(Key, () => SelectWithoutCache());

            return list;
        }

        private List<ViewModel> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                return (from enquiry in context.EnquiryDetails
                        select new ViewModel
                        {
                            EnquiryDetailsID = enquiry.EnquiryDetailsID,
                            PersonName = enquiry.PersonName,
                            MobileNumber = enquiry.MobileNumber,
                            EmailID = enquiry.EmailID,
                            EnquiryType = enquiry.EnquiryType,
                            Remark = enquiry.Remark,
                            AdminStatus = enquiry.AdminStatus,
                            DOC = enquiry.DOC.ToString(Messages.DateShortFormat, CultureInfo.InvariantCulture),
                            AdminRemark = enquiry.AdminRemark,
                            DOCInt = enquiry.DOCInt,
                        }).ToList();
            }
        }

        internal List<ViewModel> SelectByFilter(SelectRequest selectRequest)
        {
            List<ViewModel> List = Select();
            List<ViewModel> response = new List<ViewModel>();

            response = (from item in List
                        select item).ToList();
            if (!selectRequest.Status.EqualsIgnoreCase("ALL"))
            {
                response = (from item in response
                            where item.AdminStatus == selectRequest.Status
                            select item).ToList();
            }

            return response;
        }
    }
}