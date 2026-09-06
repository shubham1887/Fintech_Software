using System.Xml;
using YesBankCore.Request.AEPS;

namespace YesBankCore.Controller
{
    internal class FingerPrintProcessing
    {
        public string isValidCapturedFingerPrint(string fingerPrint)
        {
            if (fingerPrint.Trim().Length <= 20)
            {
                return "Invalid fingerprint, blank data";
            }

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(fingerPrint);

                XmlElement root = doc.DocumentElement;

                string errCode = root.SelectSingleNode("/PidData/Resp/@errCode").Value;

                if (errCode != "0")
                {
                    string errInfo = root.SelectSingleNode("/PidData/Resp/@errInfo").Value;
                    return "Invalid fingerprint, " + errInfo;
                }
            }
            catch (Exception ex)
            {
                return "Invalid fingerprint, error occurred";
            }
            return YESBankCore.Controller.Message.OK;
        }


        public void extractAuthAttribute(string fingerPrint, AuthenticationRequest input)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(fingerPrint);

            input.DeviceMake = "";
            input.DeviceType = "";

            input.DeviceModel = "";
            input.AppVersion = "";

            XmlElement root = doc.DocumentElement;

            string errCode = root.SelectSingleNode("/PidData/Resp/@errCode").Value;

            if (errCode != "0")
            {
                return;
            }
            input.AppVersion = root.SelectSingleNode("/PidData/DeviceInfo/@rdsVer").Value;

            string dpId = root.SelectSingleNode("/PidData/DeviceInfo/@dpId").Value;

            input.DeviceMake = dpId.Split('.')[0];

            input.DeviceModel = root.SelectSingleNode("/PidData/DeviceInfo/@mi").Value;

            input.DeviceType = dpId.Split('.')[0];

        }
        public void extractCashWithdrawAttribute(string fingerPrint, CashWithdrawalRequest input)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(fingerPrint);

            input.DeviceMake = "";
            input.DeviceType = "";

            input.DeviceModel = "";
            input.AppVersion = "";
           
            XmlElement root = doc.DocumentElement;

            string errCode = root.SelectSingleNode("/PidData/Resp/@errCode").Value;

            if (errCode != "0")
            {
                return;
            }
            input.AppVersion = root.SelectSingleNode("/PidData/DeviceInfo/@rdsVer").Value;

            string dpId = root.SelectSingleNode("/PidData/DeviceInfo/@dpId").Value;

            input.DeviceMake = dpId.Split('.')[0];

            input.DeviceModel = root.SelectSingleNode("/PidData/DeviceInfo/@mi").Value;

            input.DeviceType = dpId.Split('.')[0];

        }

        public void extractBalanceEnquiryAttribute(string fingerPrint, BalanceEnquiryRequest input)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(fingerPrint);

            input.DeviceMake = "";
            input.DeviceType = "";

            input.DeviceModel = "";
            input.AppVersion = "";

            XmlElement root = doc.DocumentElement;

            string errCode = root.SelectSingleNode("/PidData/Resp/@errCode").Value;

            if (errCode != "0")
            {
                return;
            }
            input.AppVersion = root.SelectSingleNode("/PidData/DeviceInfo/@rdsVer").Value;

            string dpId = root.SelectSingleNode("/PidData/DeviceInfo/@dpId").Value;

            input.DeviceMake = dpId.Split('.')[0];

            input.DeviceModel = root.SelectSingleNode("/PidData/DeviceInfo/@mi").Value;

            input.DeviceType = dpId.Split('.')[0];
        }

        public void extractMiniStatementAttribute(string fingerPrint, MiniStatementRequest input)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(fingerPrint);

            input.DeviceMake = "";
            input.DeviceType = "";

            input.DeviceModel = "";
            input.AppVersion = "";

            XmlElement root = doc.DocumentElement;

            string errCode = root.SelectSingleNode("/PidData/Resp/@errCode").Value;

            if (errCode != "0")
            {
                return;
            }
            input.AppVersion = root.SelectSingleNode("/PidData/DeviceInfo/@rdsVer").Value;

            string dpId = root.SelectSingleNode("/PidData/DeviceInfo/@dpId").Value;

            input.DeviceMake = dpId.Split('.')[0];

            input.DeviceModel = root.SelectSingleNode("/PidData/DeviceInfo/@mi").Value;

            input.DeviceType = dpId.Split('.')[0];

        }
    }
}
