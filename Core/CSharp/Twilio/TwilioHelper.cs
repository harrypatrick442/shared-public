using Constants.Twilio;
using System;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using WebSocketSharp.Server;

namespace SnippetsCore.Twilio
{
    public static class TwilioHelper
    {
        public static void SendTxt(string body, string phoneNumberTo)
        {
            TwilioClient.Init(Constants.Twilio.Account.AccountSsid, 
                Constants.Twilio.Account.AuthToken);

            var message = MessageResource.Create(
                body: body,
                from: new PhoneNumber(PhoneNumbers.UK_Brighton),
                to: new PhoneNumber(phoneNumberTo)
            );

            Console.WriteLine(message.Sid);
        }
    }
}