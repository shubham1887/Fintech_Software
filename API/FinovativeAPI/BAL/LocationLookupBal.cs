using Project.DAL;
using Project.Model;
using Project.Processing;
using Project.Response.Location;
using Project.StaticModel;
using System;
using System.Threading.Tasks;

namespace Project.BAL
{
    public class LocationLookupBAL
    {
        internal ViewModel Select(string IP, out string failureMessage)
        {
            failureMessage = Messages.DataNotFound;
            ViewModel report = null;

            try
            {
                report = new LocationLookUpDal().SelectByIPAddress(IP);
            }
            catch (Exception ex)
            {
                new Logger().Write(ex);
            }
            if (report == null)
                return report;

            failureMessage = StatusType.Ok;
            return report;
        }

        internal void InsertCore(string Type, string Latitude, string Longitude, string ip, Int32 transactionMasterID)
        {
            Task.Run(() =>
            {
                try
                {
                    LocationLookUp locationLookUp = new LocationProcessing().LocationDetails(ip);
                    locationLookUp.Type = Type;
                    locationLookUp.Latitude = Latitude;
                    locationLookUp.Longitude = Longitude;
                    locationLookUp.IPAddress = ip;
                    locationLookUp.TableID = transactionMasterID;

                    new LocationLookUpDal().Insert(locationLookUp);
                }
                catch (Exception ex)
                {
                    new Logger().Write(ex);
                }
            });
        }

         
    }
}