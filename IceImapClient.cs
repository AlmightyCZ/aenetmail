using System;
using System.Globalization;
using System.Linq;

namespace AE.Net.Mail
{
    public class IceImapClient : ImapClient
    {
        public IceImapClient(string host, string username, string password, AuthMethods method = AuthMethods.Login, int port = 143, bool secure = false, bool skipSslValidation = false)
          : base(host, username, password, method, port, secure, skipSslValidation)
        {
        }

        internal override void OnLogin(string login, string password)
        {
            base.OnLogin(login, password);
            SendCommand($"{GetTag()} x-icewarp-server iwconnector \"10.3.5.4853\"");
            string response;
            while ((response = GetResponse()).StartsWith("*")) { }
            if (!IsResultOK(response))
                throw new Exception(response);
        }

        public MailMessage[] GetMessagesBySince(DateTime changed)
        {
            return Search("SINCE " + changed.ToString("dd-MMM-yyyy", CultureInfo.GetCultureInfo("en-US")), true).Select(uid => GetMessage(uid, false)).Where(msg => msg.Date >= changed).ToArray();
        }

        public MailMessage[] GetMessagesByModSeq(long modSeq)
        {
            return GetMessages(0, 0, false, false, modSeq);
        }
    }
}
