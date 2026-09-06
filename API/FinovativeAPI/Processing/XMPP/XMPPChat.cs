using Project.StaticModel;
using S22.Xmpp.Client;
using System;
using System.IO;

namespace Project.Processing.XMPP
{
    public class XMPPChat
    {
        public static XmppClient client = new XmppClient(Company.XMPPDomain, 5222, false, null);

        public void Connect()
        {
            try
            {
                if (Company.XMPPDomain == null || Company.XMPPDomain.Length <= 2)
                {
                    return;
                }

                if (Company.XMPPWebSiteUserName == null || Company.XMPPWebSiteUserName.Length <= 2)
                {
                    return;
                }

                if (Company.XMPPWebSiteUserPassword == null || Company.XMPPWebSiteUserPassword.Length <= 2)
                {
                    return;
                }

                client = new XmppClient(Company.XMPPDomain, 5222, false, null);

                client.Error += OnError;
                client.Message += OnNewMessage;
                client.StatusChanged += client_StatusChanged;
                client.ChatStateChanged += client_ChatStateChanged;

                // client.RosterUpdated += Client_RosterUpdated; ;// += new PresenceHandler(xmpp_OnPresence);

                client.Connect();

                client.Authenticate(Company.XMPPWebSiteUserName, Company.XMPPWebSiteUserPassword);
                client.Im.Status += Im_Status;
            }
            catch (Exception)
            {
            }
        }



        private void Im_Status(object sender, S22.Xmpp.Im.StatusEventArgs e)
        {
            //ConnectedStatus = e.Status.Availability.ToString();
        }

        private void client_ChatStateChanged(object sender, S22.Xmpp.Extensions.ChatStateChangedEventArgs e)
        {
        }

        private void client_StatusChanged(object sender, S22.Xmpp.Im.StatusEventArgs e)
        {
            // ConnectedStatus = e.Status.Availability.ToString();
        }

        private void OnError(Object Sender, S22.Xmpp.Core.ErrorEventArgs e)
        {
            if (e.Exception.Message.Contains("The server has closed the XML"))
            {
            }
            else if (e.Exception.Message.Contains("Unexpected node"))
            {
            }
            else
            {
                //  msg("Error - Event - RosterForm", e.Exception.Message + ":" + e.Exception.ToString());
            }
        }

        public string RechargeCall(string message)
        {
            if (XMPPChat.client.Connected == false)
            {
                Connect();
            }

            try
            {
                string sendto = Company.XMPPOtomaxUser + "@" + Company.XMPPDomain;
                XMPPChat.client.SendMessage(sendto, message);
            }
            catch (IOException)
            {
            }

            return "recharge sent to xmpp, please wait for some time";
        }

        private void OnNewMessage(object sender, S22.Xmpp.Im.MessageEventArgs e)
        {
            try
            {
                String resID = e.Jid.Resource;
                String domain = e.Jid.Domain;
                String jid = e.Jid.ToString().Replace(resID, "");
                jid = jid.Replace(domain, "");
                jid = jid.Replace("@/", "");

                if (!Company.XMPPOtomaxUser.EqualsIgnoreCase(jid))
                {
                    return;
                }

                String chatResponse = e.Message.Body;
                int RechargeProviderID = Convert.ToInt16(Company.XMPPProviderID);

                CallbackProcessing callback = new CallbackProcessing();
                callback.PreExtractCallBack(RechargeProviderID, chatResponse.Trim());
            }
            catch (Exception)
            {
                //new Logger().write(ex);
            }
        }


    }
}