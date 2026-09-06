using Project.Model;
using Project.Processing;
using System.Collections.Generic;
using System.Linq;

namespace Project.DAL
{
    public class TrustableDeviceDal
    {
        public static string TrustableDevice
        { get { return "_TrustableDevice"; } }

        private void removeCache()
        {
            var cache = new MemoryCacheProcessing<List<TrustableDevice>>();
            cache.Remove(TrustableDevice);
        }

        internal TrustableDevice Select(int userID, string fingerprint)
        {
            List<TrustableDevice> list = SelectAll();

            TrustableDevice device = list.FirstOrDefault(T => T.UserID == userID && T.Browser == fingerprint && T.IsVerified == true);
            return device;
        }

        private List<TrustableDevice> SelectAll()
        {
            var _trustableDevice = new MemoryCacheProcessing<List<TrustableDevice>>();

            List<TrustableDevice> device = _trustableDevice.GetOrCreate(TrustableDevice, () => SelectWithoutCache());
            return device;
        }

        private List<TrustableDevice> SelectWithoutCache()
        {
            using (var context = new DatabaseContext())
            {
                return context.TrustableDevice.ToList();
            }
        }

        internal int Insert(TrustableDevice device)
        {
            removeCache();
            using (var context = new DatabaseContext())
            {
                context.TrustableDevice.Add(device);
                context.SaveChanges();
                return device.ID;
            }
        }

        internal void Delete(int DeleteDOCInt)
        {
            removeCache();
            using (var context = new DatabaseContext())
            {
                var selectedrecord = context.TrustableDevice.Where(U => U.DOCInt <= DeleteDOCInt);
                context.TrustableDevice.RemoveRange(selectedrecord);
                context.SaveChanges();
            }
        }
    }
}