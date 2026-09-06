using Project.Model;
using Project.Processing;
using Project.Response.AEPS;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class FingerPrintDeviceTypeDal
    {
        private readonly string key = "_FingerPrintDeviceType";

        internal List<FingerPrintDeviceResponse> Select()
        {
            var _list = new MemoryCacheProcessing<List<FingerPrintDeviceResponse>>();
            List<FingerPrintDeviceResponse> list = _list.GetOrCreate(key, () => SelectWithoutCache());
            return list;
        }

        private List<FingerPrintDeviceResponse> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                return (from FingerPrintDeviceType in context.FingerPrintDeviceType
                        orderby FingerPrintDeviceType.FingerPrintDeviceTypeID ascending
                        select new FingerPrintDeviceResponse
                        {
                            FingerPrintDeviceTypeID = FingerPrintDeviceType.FingerPrintDeviceTypeID,
                            DeviceName = FingerPrintDeviceType.DeviceName,
                            Value = FingerPrintDeviceType.Value,
                        }).ToList();
            }
        }
    }
}