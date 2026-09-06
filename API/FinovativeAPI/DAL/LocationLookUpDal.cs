using Project.Model;
using Project.Response.Location;
using System.Linq;

namespace Project.DAL
{
    public class LocationLookUpDal
    {
        internal LocationLookUp Select(string IPAddress)
        {
            using (var context = new DatabaseContext())
            {
                return context.LocationLookUp.OrderByDescending(x => x.LocationLookUpID).Where(U => U.IPAddress == IPAddress).FirstOrDefault();
            }
        }

        internal ViewModel SelectByIPAddress(string IPAddress)
        {
            LocationLookUp item = Select(IPAddress);
            if (item == null) return null;

            ViewModel data = new ViewModel();
            data.Location = item.Location;
            data.SimCardDetails = item.SimCardDetails;
            data.IPAddress = item.IPAddress;

            return data;
        }

        internal int Insert(LocationLookUp objModel)
        {
            if (objModel.SimCardDetails == null)
            {
                objModel.SimCardDetails = "0";
            }

            using (var context = new DatabaseContext())
            {
                context.LocationLookUp.Add(objModel);
                context.SaveChanges();

                return objModel.LocationLookUpID;
            }
        }
    }
}