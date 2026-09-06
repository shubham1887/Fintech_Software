using Project.Model;
using Project.Processing;
using Project.Response.OfflineProductMaster;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class OfflineProductMasterDal
    {
        public static string key
        { get { return "_OfflineProductMaster"; } }

        internal void removeCache()
        {
            var cache = new MemoryCacheProcessing<List<OfflineProductMaster>>();
            cache.Remove(key);
        }

        internal int Insert(OfflineProductMaster objModel)
        {
            removeCache();
            using (var context = new DatabaseContext())
            {
                context.OfflineProductMaster.Add(objModel);
                return context.SaveChanges();
            }
        }

        internal int Update(OfflineProductMaster ObjModel)
        {
            removeCache();
            using (var context = new DatabaseContext())
            {
                context.OfflineProductMaster.Update(ObjModel);
                context.SaveChanges();
                return ObjModel.OperatorID;
            }
        }

        internal List<ViewModel> Select()
        {
            List<OfflineProductMaster> productList = SelectAll();
            List<Response.Operator.ViewModel> opList = new OperatorMasterDAL().SelectActiveOperatorOnly();
            List<ViewModel> list = new List<ViewModel>();

            list = (from product in productList
                    join Operator in opList on product.OperatorID equals Operator.OperatorID

                    where product.IsDelete == false
                    select new ViewModel
                    {
                        OfflineProductMasterID = product.OfflineProductMasterID,
                        OpType = product.OpType,
                        Name = product.Name,
                        OperatorID = product.OperatorID,
                        OperatorName = Operator.OperatorName,
                        Validity = product.Validity,
                        PackageMRP = product.PackageMRP,
                        BookingAmount = product.BookingAmount,
                        Description = product.Description,
                        Remark = product.Remark,
                        IsActive = product.IsActive,
                    }).ToList();

            return list;
        }

        internal bool CheckDuplicate(string Name, int OperatorID)
        {
            List<OfflineProductMaster> list = SelectAll();
            OfflineProductMaster obj = null;
            if (list != null)
            {
                obj = (from item in list
                       where item.OperatorID == OperatorID
                       && item.Name.EqualsIgnoreCase(Name)
                       select item).FirstOrDefault();
            }
            if (obj == null)
                return true;
            return false;
        }

        internal List<OfflineProductMaster> SelectAll()
        {
            var _list = new MemoryCacheProcessing<List<OfflineProductMaster>>();

            List<OfflineProductMaster> list = _list.GetOrCreate(key, () => SelectWithoutCache());

            return list;
        }

        private List<OfflineProductMaster> SelectWithoutCache()
        {
            using var context = new DatabaseContext();
            {
                return (from product in context.OfflineProductMaster
                        select product).ToList();
            }
        }

        internal OfflineProductMaster SelectByID(int OfflineProductMasterID)
        {
            List<OfflineProductMaster> list = SelectAll();
            return list.FirstOrDefault(S => S.OfflineProductMasterID == OfflineProductMasterID);
        }

        internal CostViewModel SelectByOfflineProductMasterID(int OfflineProductMasterID)
        {
            List<OfflineProductMaster> list = SelectAll();

            return (from item in list
                    where item.OfflineProductMasterID == OfflineProductMasterID

                    select new CostViewModel
                    {
                        OfflineProductMasterID = item.OfflineProductMasterID,
                        Validity = item.Validity,
                        Description = item.Description,
                        PackageMRP = item.PackageMRP,
                        BookingAmount = item.BookingAmount,
                        Name = item.Name,
                    }).FirstOrDefault();
        }

        internal List<PackageNameViewModel> SelectPackageName(int OperatorID, string OpType)
        {
            List<OfflineProductMaster> list = SelectAll();

            return (from item in list
                    where item.OperatorID == OperatorID
                    && item.OpType.Contains(OpType)
                    && item.IsDelete == false
                    select new PackageNameViewModel
                    {
                        OfflineProductMasterID = item.OfflineProductMasterID,
                        Name = item.Name
                    }).ToList();
        }
    }
}