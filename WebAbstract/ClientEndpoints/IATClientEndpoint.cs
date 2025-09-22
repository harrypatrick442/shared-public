using Core.Enums;
using Core.Messages;
using Core.Exceptions;
using System.Net;
using UserRouting;
using Logging;
using Core.Handlers;
using Core.InterserverComs;
using JSON;
using Authentication;
using Authentication.Responses;
using Authentication.Requests;
using Core.Messages.Messages;
using Authentication.DAL;
using Core.Interfaces;
using Sessions;
using Authentication.Messages;
using WebAPI.Responses;
using WebAPI.Requests;

namespace Core.Authentication
{
    public class IATClientEndpoint
    {
        private IClientEndpointLight _Endpoint;
        private Action<bool, long> _Callback;
        private Action _RemoveMappings;
        public IATClientEndpoint(
            IClientEndpointLight endpoint,
            Action<bool, long> callback,
            ClientMessageTypeMappingsHandler clientMessageTypeMappingsHandler
        )
        {
            _Endpoint = endpoint;
            _Callback = callback;
            _RemoveMappings = clientMessageTypeMappingsHandler.AddRange(new TupleList<string, DelegateHandleMessageOfType<TypeTicketedAndWholePayload>> {
                { global::MessageTypes.MessageTypes.IATAuthentication, HandleIATAuthentication}
            });
        }
        private void HandleIATAuthentication(TypeTicketedAndWholePayload t) {
            IATAuthenticateRequest request = Json.Deserialize<IATAuthenticateRequest>(t.JsonString);
            try
            {
                if (SessionsMesh.Instance.Authenticate(request.NodeId, request.SessionId, request.Token,  out long userId)) {
                    _Callback(true, userId);
                    _Endpoint.SendObject(IATAuthenticateResponse.Successful(userId, request.Ticket));
                    return;
                }
                _Callback(false, 0);
                _Endpoint.SendObject(IATAuthenticateResponse.Failed(request.Ticket));
            }
            catch (Exception ex)
            {
                Logs.Default.Error(ex);
            }
        }
        public virtual void Dispose()
        {
            _RemoveMappings();
        }
    }
}
