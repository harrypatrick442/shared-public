using Core.Messages;
using Logging;
using Core.Handlers;
using Core.InterserverComs;
using JSON;
using Core.Interfaces;
using Sessions;
using WebAPI.Responses;
using Core;
using WebAbstract.Requests;

namespace WebAbstract.ClientEndpoints
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
                { MessageTypes.IATAuthentication, HandleIATAuthentication}
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
