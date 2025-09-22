
using Core.Messages;
using Core.Handlers;
using System.Threading;
using JSON;
using Core.Messages.Messages;
using System;
using Logging;

namespace Core.InterserverComs
{
    public sealed class ClientMessageTypeMappingsHandler : MessageTypeMappingsHandler<TypeTicketedAndWholePayload>
    {

        public void HandleMessageOnNewThread(string jsonString) {
            new Thread(() =>
            {
                try
                {
                    TypedTicketedMessage message = Json
                            .Deserialize<TypedTicketedMessage>(jsonString);
                    TypeTicketedAndWholePayload typeAndWholePayload = new TypeTicketedAndWholePayload(
                            message.Type, message.Ticket, jsonString);
                    HandleMessage(typeAndWholePayload);
                }
                catch (Exception ex)
                {
                    Logs.Default.Error(ex);
                }
            }).Start();
        }
    }
}