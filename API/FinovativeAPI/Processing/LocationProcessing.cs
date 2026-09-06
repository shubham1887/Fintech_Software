using Newtonsoft.Json;
using Project.BAL;
using Project.Model;
using Project.Requests;
using Project.Response.Location;
using Project.StaticModel;
using System;
using System.Globalization;
using System.Net;

namespace Project.Processing
{
    public class LocationProcessing
    {
        private string GetUserCountryByIp(string ip)
        {
            IpInfo ipInfo = new IpInfo();
            string info = "";
            try
            {
                info = new WebClient().DownloadString($"http://ipinfo.io/{ip}");
                ipInfo = JsonConvert.DeserializeObject<IpInfo>(info);
                ipInfo.Country = new RegionInfo(ipInfo.Country).EnglishName;
            }
            catch (Exception ex)
            {
                ipInfo.Country = "";
            }

            return info.ToString();
        }

        private ViewModel LocationDetailsSelect(string IP)
        {
            ViewModel ObjModel = null;

            string failureMessage = "";
            try
            {
                ObjModel = new LocationLookupBAL().Select(IP, out failureMessage);
            }
            catch (Exception ex)
            {
                Logger log = new Logger();
                log.Write(ex);
            }

            if (!failureMessage.EqualsIgnoreCase(StatusType.Ok) || ObjModel == null)
            {
                ObjModel = new ViewModel();
                ObjModel.Location = GetUserCountryByIp(IP);
            }

            return ObjModel;
        }

        public LocationLookUp LocationDetails(string IP)
        {
            LocationLookUp ObjModel = new LocationLookUp();
            ViewModel response = null;

            try
            {
                response = LocationDetailsSelect(IP);
                if (response == null)
                {
                    ObjModel.IPAddress = IP;
                    ObjModel.SimCardDetails = "";
                    ObjModel.Location = "";
                    return ObjModel;
                }

                ObjModel.IPAddress = IP;
                ObjModel.SimCardDetails = response.SimCardDetails;
                ObjModel.Location = response.Location;
            }
            catch (Exception ex)
            {
                Logger log = new Logger();
                log.Write(ex);
            }

            return ObjModel;
        }
    }
}