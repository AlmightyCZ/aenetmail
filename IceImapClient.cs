using System;
using System.Collections.Generic;
using System.Globalization;

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="processCallback"></param>
        /// <param name="changed">datum od(včetně) kterého budou zprávy stahovány</param>
        /// <param name="uidIgnoreList">uid zpráv, které jsou vynechány</param>
        public void GetMessagesBySince(Action<MailMessage[]> processCallback, DateTime changed, HashSet<string> uidIgnoreList)
        {
            List<MailMessage> list = new List<MailMessage>();
            foreach (string uid in Search("SINCE " + changed.ToString("dd-MMM-yyyy", CultureInfo.GetCultureInfo("en-US")), true))
            {
                if (uidIgnoreList != null && uidIgnoreList.Contains(uid))
                {
                    continue;
                }

                MailMessage message = GetMessage(uid, false, false);

                if (message == null)
                {
                    throw new BranoNullMessageException("Spojení přerušeno. MSG NULL"); //Speciální výjimka, která slouží k opakovanýcm pokusům
                }

                if (message.Date >= changed)
                {
                    list.Add(message);
                    if (list.Count >= MAX_MSG_PER_CALLBACK)
                    {
                        MailMessage[] mailMessageArray = list.ToArray();
                        processCallback(mailMessageArray);
                        list.Clear();
                    }
                }
            }
            processCallback(list.ToArray());
        }


        public void GetMessagesByModSeq(Action<MailMessage[]> processCallback, long modSeq)
        {
            GetMessages(processCallback, 0, 0, false, false, modSeq);
        }

        public class BranoNullMessageException : Exception
        {
            public BranoNullMessageException()
            {
            }

            public BranoNullMessageException(string message) : base(message)
            {
            }
        }

    }
}
