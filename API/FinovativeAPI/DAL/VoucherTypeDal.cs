using Project.Model;
using Project.Processing;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Proj.DAL
{
    public class VoucherTypeDal
    {
        private readonly string Key = "_VoucherType";

        private void removeCache()
        {
            var cache = new MemoryCacheProcessing<List<Project.Response.VoucherType.ViewModel>>();
            cache.Remove(Key);
        }
        internal List<Project.Response.VoucherType.ViewModel> Select()
        {
            var _list = new MemoryCacheProcessing<List<Project.Response.VoucherType.ViewModel>>();
            List<Project.Response.VoucherType.ViewModel> list = _list.GetOrCreate(Key, () => SelectWithoutCache());
            return list;
        }
        private List<Project.Response.VoucherType.ViewModel> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                return (from voucher in context.VoucherType
                        select new Project.Response.VoucherType.ViewModel
                        {
                            VoucherTypeID = voucher.VoucherTypeID,
                            VoucherTypeName = voucher.VoucherTypeName,
                        }).ToList();
            }
        }
        public List<Project.Response.VoucherType.ViewModel> SelectForDropdown()
        {
            List<Project.Response.VoucherType.ViewModel> list = Select();
            return (from voucherType in list
                    select new Project.Response.VoucherType.ViewModel
                    {
                        VoucherTypeID = voucherType.VoucherTypeID,
                        VoucherTypeName = voucherType.VoucherTypeName,

                    }).ToList();
        }
    }
}
