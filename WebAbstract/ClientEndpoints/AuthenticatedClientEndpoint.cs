using Core.Enums;
using Core.Messages;
using Core.Exceptions;
using System.Net;
using UserRouting;
using Logging;
using Core.Handlers;
using Core.InterserverComs;
using JSON;
using Authentication.Responses;
using Authentication.Requests;
using Core.Interfaces;
using Sessions;
using Authentication.Messages;
using Authentication.Enums;
using Authentication;
using WebAbstract;
using Core;

namespace WebAbstract.ClientEndpoints
{
    public class AuthenticatedClientEndpoint
    {
        private readonly object _LockObjectSessionInfo = new object();
        private readonly object _LockObjectAuthenticationOperation = new object();
        SessionInfo _SessionInfo;
        private IUserRoutingTable<IClientEndpoint> _UserRoutingTable;
        private string _DeviceIdentifier;
        private DelegateCreateNewUser _CreateNewUser;
        private IClientEndpoint _Endpoint;
        private bool _Disposed = false;
        private bool _DebugLoggingEnabled;
        public SessionInfo SessionInfoSafe { get { lock (_LockObjectSessionInfo) { return _SessionInfo; } } }
        private Func<string> _GetUserAgent;
        private Func<long, object?> _GetAdditionalPayloadForAuthenticatedUser;
        public AuthenticatedClientEndpoint(
            DelegateCreateNewUser createNewUser,
            IClientEndpoint endpoint,
            IUserRoutingTable<IClientEndpoint> userRoutingTable, 
            ClientMessageTypeMappingsHandler clientMessageTypeMappingsHandler,
            Func<string> getUserAgent,
            Func<long, object?> getAdditionalPayloadForAuthenticatedUser,
            bool debugLoggingEnabled
        )
        {
            _Endpoint = endpoint;
            _CreateNewUser = createNewUser;
            _UserRoutingTable = userRoutingTable;
            _GetUserAgent = getUserAgent;
            _DebugLoggingEnabled = debugLoggingEnabled;
            _GetAdditionalPayloadForAuthenticatedUser = getAdditionalPayloadForAuthenticatedUser;
            clientMessageTypeMappingsHandler.AddRange(
                new TupleList<string, DelegateHandleMessageOfType<TypeTicketedAndWholePayload>> {
                { Authentication.MessageTypes.AuthenticationLogIn, LogIn },
                { Authentication.MessageTypes.AuthenticationLogOut, LogOut },
                { Authentication.MessageTypes.AuthenticationRegister, Register},
                { Authentication.MessageTypes.AuthenticationLogInGuest, LogInGuest },
                {   Authentication.MessageTypes.AuthenticateWithToken, AuthenticateWithToken },
                { Authentication.MessageTypes.AuthenticationResetPasswordByEmail, ResetPasswordByEmail }
            });
        }
        protected void Initialize(IClientEndpoint endpoint)
        {
            _Endpoint = endpoint;
        }

        protected virtual void Authenticated()
        {

        }
        public void Dispose()
        {
            lock (_LockObjectSessionInfo)
            {
                if (_Disposed) return;
                _Disposed = true;
                UpdateSessionInfoAndDoDemapMap_NotLocking(null);
            }
        }

        public virtual void DoOnOpen(IPAddress ipAddress, string token, string deviceIdentifier)
        {
            _DeviceIdentifier = string.IsNullOrEmpty(deviceIdentifier) ? "device_1" : deviceIdentifier;//For now
            if (!string.IsNullOrEmpty(token))
                TryToAuthenticateWithTokeOnOpen(token);
        }
        private void ResetPasswordByEmail(TypeTicketedAndWholePayload message)
        {
            ResetPasswordByEmailRequest request= Json.Deserialize<ResetPasswordByEmailRequest>(message.JsonString);
            try
            {
                string userAgent = _GetUserAgent();
                UserAgentHelper.GetFriendlyOperatingSystemAndBrowerName(userAgent,
                    out string operatingSystem, out string browserName);
                bool success = global::Authentication.
                    AuthenticationManager.Instance.ResetPasswordByEmail(
                        request.Email,
                        operatingSystem,
                        browserName
                );
                _Endpoint.SendObject(
                    new ResetPasswordByEmailResponse(success, request.Ticket));
            }
            catch (Exception ex)
            {
                Logs.Default.Error(ex);
                _Endpoint.SendObject(
                    new ResetPasswordByEmailResponse(false, request.Ticket));
            }
        }
        private void TryToAuthenticateWithTokeOnOpen(string token)
        {
            try
            {
                try
                {

                    SessionInfo sessionInfo = UpdateSessionInfoAndDoDemapMap_Locking(global::Authentication.AuthenticationManager
                        .Instance.TryToAuthenticateWithToken(token, _DeviceIdentifier,
                        SessionIdSource.Instance));
                    Authenticated();
                    _Endpoint.SendObject(TokenAuthenticationResult.Success(sessionInfo.UserId, sessionInfo.SessionId, sessionInfo.Token, _GetAdditionalPayloadForAuthenticatedUser(sessionInfo.UserId)));
                }
                catch (BusyException)
                {
                    _Endpoint.SendObject(TokenAuthenticationResult.Failed(AuthenticationFailedReason.Busy));
                }
                catch (BadCredentialsException)
                {
                    _Endpoint.SendObject(TokenAuthenticationResult.Failed(AuthenticationFailedReason.BadCredentials));
                }
                catch (Exception ex)
                {
                    if (_DebugLoggingEnabled)
                    {
                        Logs.Default.Error(ex);
                    }
                    _Endpoint.SendObject(TokenAuthenticationResult.Failed(AuthenticationFailedReason.Unknown));
                }
            }
            catch (Exception ex2)
            {
                if (_DebugLoggingEnabled)
                {
                    Logs.Default.Error(ex2);
                }
            }
        }
        private void AuthenticateWithToken(TypeTicketedAndWholePayload message)
        {
            AuthenticateWithTokenRequest authenticateWithTokenRequest = Json.Deserialize<AuthenticateWithTokenRequest>(message.JsonString);

            try
            {
                try
                {
                    SessionInfo sessionInfo;
                    lock (_LockObjectAuthenticationOperation)
                    {
                        sessionInfo = UpdateSessionInfoAndDoDemapMap_Locking(global::
                            Authentication.AuthenticationManager
                            .Instance.TryToAuthenticateWithToken(authenticateWithTokenRequest.Token,
                                _DeviceIdentifier,
                                SessionIdSource.Instance));
                    }
                    Authenticated();
                    _Endpoint.SendObject(AuthenticateResponse.Successful(
                        authenticateWithTokenRequest.Ticket, sessionInfo.UserId, 
                        sessionInfo.Token, sessionInfo.SessionId, _GetAdditionalPayloadForAuthenticatedUser(sessionInfo.UserId)));
                }
                catch (MustWaitToRetryException mEx)
                {

                    _Endpoint.SendObject(AuthenticateResponse.Failed(authenticateWithTokenRequest.Ticket,
                        AuthenticationFailedReason.TooManyAttempts, mEx.SecondssDelay, 0));
                }
                catch (BadCredentialsException)
                {
                    _Endpoint.SendObject(AuthenticateResponse.Failed(authenticateWithTokenRequest.Ticket,
                        AuthenticationFailedReason.BadCredentials, null, 0));
                }
                catch (BusyException)
                {
                    _Endpoint.SendObject(AuthenticateResponse.Failed(authenticateWithTokenRequest.Ticket,
                        AuthenticationFailedReason.Busy, null, 0));
                }
                catch (Exception ex)
                {
                    Logs.Default.Error(ex);
                    _Endpoint.SendObject(AuthenticateResponse.Failed(authenticateWithTokenRequest.Ticket,
                        AuthenticationFailedReason.Unknown, null, 0));
                }
            }
            catch (Exception ex2)
            {
                if (_DebugLoggingEnabled)
                {
                    Logs.Default.Error(ex2);
                }
            }
        }
        private SessionInfo UpdateSessionInfoAndDoDemapMap_Locking(SessionInfo sessionInfo)
        {
            lock (_LockObjectSessionInfo)
            {
                return UpdateSessionInfoAndDoDemapMap_NotLocking(sessionInfo);
            }

        }
        private SessionInfo UpdateSessionInfoAndDoDemapMap_NotLocking(SessionInfo sessionInfo)
        {
            if (_SessionInfo != null)
            {
                _UserRoutingTable.Remove(_SessionInfo.UserId, _SessionInfo.SessionId);
                _SessionInfo.Dispose();
            }
            if (_Disposed) {
                sessionInfo?.Dispose();
                _SessionInfo = null;
            }
            _SessionInfo = sessionInfo;
            if (sessionInfo == null) return null;
            _UserRoutingTable.Add(_SessionInfo.UserId, _SessionInfo.SessionId, _Endpoint);
            return _SessionInfo;
        }
        private void LogOut(TypeTicketedAndWholePayload message)
        {
            LogOutRequest logOutRequest = Json.Deserialize<LogOutRequest>(message.JsonString);
            lock (_LockObjectAuthenticationOperation)
            {
                UpdateSessionInfoAndDoDemapMap_Locking(null);
            }
            _Endpoint.SendObject(new LogOutResponse(logOutRequest.Ticket));
        }
        private void LogInGuest(TypeTicketedAndWholePayload message)
        {
            try
            {
                LogInGuestRequest request= Json
                    .Deserialize<LogInGuestRequest>(message.JsonString);
                try
                {
                    SessionInfo sessionInfo;
                    lock (_LockObjectAuthenticationOperation)
                    {
                        lock (_LockObjectSessionInfo)
                        {
                            long userId = global::Authentication.AuthenticationManager.Instance
                                .LogInGuest(
                                    _CreateNewUser,
                                    _Endpoint.ClientIPAddress,
                                    request.Username
                                );
                            sessionInfo = UpdateSessionInfoAndDoDemapMap_NotLocking(global::
                                Authentication.AuthenticationManager.Instance.CreateToken(
                                userId, _DeviceIdentifier,
                                SessionIdSource.Instance));
                        }
                    }/*
                    try
                    {
                        SendNewToken(sessionInfo.Token);
                    }
                    catch (Exception ex)
                    {
                        Logs.Default.Error(ex);
                    }*/
                    Authenticated();
                    _Endpoint.SendObject(AuthenticateResponse.Successful(request.Ticket, _SessionInfo.UserId,
                        sessionInfo.Token, sessionInfo.SessionId, _GetAdditionalPayloadForAuthenticatedUser(sessionInfo.UserId)));
                }
                catch (MustWaitToRetryException mEx)
                {

                    _Endpoint.SendObject(AuthenticateResponse.Failed(request.Ticket,
                        AuthenticationFailedReason.TooManyAttempts, mEx.SecondssDelay, 0));
                }
                catch (BusyException)
                {
                    _Endpoint.SendObject(AuthenticateResponse.Failed(request.Ticket,
                        AuthenticationFailedReason.Busy, null, 0));
                }
                catch (Exception ex)
                {
                    Logs.Default.Error(ex);
                    _Endpoint.SendObject(AuthenticateResponse.Failed(request.Ticket,
                        AuthenticationFailedReason.Unknown, null, 0));
                }
            }
            catch (Exception ex2)
            {
                if (_DebugLoggingEnabled)
                {
                    Logs.Default.Error(ex2);
                }
            }
        }
        private void Register(TypeTicketedAndWholePayload message)
        {
            SessionInfo sessionInfo = null;
            try
            {
                RegisterRequest registerRequest = Json.Deserialize<RegisterRequest>(message.JsonString);
                try
                {
                    lock (_LockObjectAuthenticationOperation)
                    {
                        long userId = global::Authentication.AuthenticationManager.Instance.Register(_CreateNewUser,
                            registerRequest.Email, registerRequest.Username, registerRequest.Phone,
                            registerRequest.Password,
                            _Endpoint.ClientIPAddress, _DeviceIdentifier);
                        lock (_LockObjectSessionInfo)
                        {
                            sessionInfo = UpdateSessionInfoAndDoDemapMap_NotLocking(global::
                                Authentication.AuthenticationManager.Instance.CreateToken(
                                userId, _DeviceIdentifier,
                                SessionIdSource.Instance));
                        }
                    }
                    Authenticated();
                    _Endpoint.SendObject(AuthenticateResponse.Successful(registerRequest.Ticket, sessionInfo.UserId,
                        sessionInfo.Token, sessionInfo.SessionId, _GetAdditionalPayloadForAuthenticatedUser(sessionInfo.UserId)));
                    return;
                }
                catch (MustWaitToRetryException mEx)
                {

                    _Endpoint.SendObject(AuthenticateResponse.Failed(registerRequest.Ticket,
                        AuthenticationFailedReason.TooManyAttempts, mEx.SecondssDelay, 0));
                }
                catch (BusyException)
                {
                    _Endpoint.SendObject(AuthenticateResponse.Failed(registerRequest.Ticket,
                        AuthenticationFailedReason.Busy, null, 0));
                }
                catch (PhoneInvalidException pIEx)
                {
                    _Endpoint.SendObject(AuthenticateResponse.Failed(registerRequest.Ticket,
                        AuthenticationFailedReason.Phone, null, (int)pIEx.Reason));
                }
                catch (EmailInvalidException eIEx)
                {
                    _Endpoint.SendObject(AuthenticateResponse.Failed(registerRequest.Ticket,
                        AuthenticationFailedReason.Email, null, (int)eIEx.Reason));
                }
                catch (PasswordInvalidException pIEx)
                {
                    _Endpoint.SendObject(AuthenticateResponse.Failed(registerRequest.Ticket,
                        AuthenticationFailedReason.Password, null, (int)pIEx.Reason, pIEx.MinLength, pIEx.MaxLength));
                }
                catch (UsernameInvalidException uIEx) {
                    _Endpoint.SendObject(AuthenticateResponse.Failed(registerRequest.Ticket,
                        AuthenticationFailedReason.Username, null, (int)uIEx.Reason, 
                        uIEx.MinLength, uIEx.MaxLength));
                }
                catch (Exception ex)
                {
                    Logs.Default.Error(ex);
                    _Endpoint.SendObject(AuthenticateResponse.Failed(registerRequest.Ticket,
                        AuthenticationFailedReason.Unknown, null, 0));
                }
                sessionInfo?.Dispose();
            }
            catch (Exception ex2)
            {
                sessionInfo?.Dispose();
                if (_DebugLoggingEnabled)
                {
                    Logs.Default.Error(ex2);
                }
            }
        }
        private void LogIn(TypeTicketedAndWholePayload message)
        {
            try
            {
                LogInRequest logInRequest = Json.Deserialize<LogInRequest>(message.JsonString);
                try
                {
                    SessionInfo sessionInfo;
                    lock (_LockObjectAuthenticationOperation)
                    {
                         sessionInfo = UpdateSessionInfoAndDoDemapMap_Locking(
                           global::Authentication.AuthenticationManager.Instance.LogIn(
                               logInRequest, _Endpoint.ClientIPAddress, _DeviceIdentifier,
                               SessionIdSource.Instance));
                    }
                    _Endpoint.SendObject(AuthenticateResponse.Successful(
                        logInRequest.Ticket, sessionInfo.UserId, sessionInfo.Token, sessionInfo.SessionId, 
                        _GetAdditionalPayloadForAuthenticatedUser(sessionInfo.UserId)));
                }
                catch (MustWaitToRetryException mEx)
                {
                    _Endpoint.SendObject(AuthenticateResponse.Failed(logInRequest.Ticket,
                        AuthenticationFailedReason.TooManyAttempts, mEx.SecondssDelay, 0));
                }
                catch (BadCredentialsException bCEX)
                {
                    _Endpoint.SendObject(AuthenticateResponse.Failed(logInRequest.Ticket,
                        AuthenticationFailedReason.BadCredentials, null, 0));
                }
                catch (BusyException)
                {
                    _Endpoint.SendObject(AuthenticateResponse.Failed(logInRequest.Ticket,
                        AuthenticationFailedReason.Busy, null, 0));
                }
                catch (Exception ex)
                {
                    Logs.Default.Error(ex);
                    _Endpoint.SendObject(AuthenticateResponse.Failed(logInRequest.Ticket,
                        AuthenticationFailedReason.Unknown, null, 0));
                }
            }
            catch (Exception ex2)
            {
                if (_DebugLoggingEnabled)
                {
                    Logs.Default.Error(ex2);
                }
            }
        }
        /*private void LogInViaEmail(string message)
        {
            LogInViaEmailRequest logInViaEmailRequest = Json.Deserialize<LogInViaEmailRequest>(message);
            try
            {
                SessionInfo sessionInfo;
                lock (_LockObjectAuthenticationOperation)
                {
                    sessionInfo = UpdateSessionInfoAndDoDemapMap_Locking(AuthenticationHelper.LogInViaEmail(
                   logInViaEmailRequest, _ClientIPAddress, _DeviceIdentifier));
                }
                Send(AuthenticateResponse.Successful(logInViaEmailRequest.Ticket, sessionInfo.UserId, sessionInfo.Token));
            }
            catch (MustWaitToRetryException mEx)
            {

                Send(AuthenticateResponse.Failed(logInViaEmailRequest.Ticket,
                    AuthenticationFailedReason.TooManyAttempts, mEx.SecondssDelay, 0));
            }
            catch (BadCredentialsException)
            {
                Send(AuthenticateResponse.Failed(logInViaEmailRequest.Ticket,
                    AuthenticationFailedReason.BadCredentials, null, 0));
            }
            catch (BusyException)
            {
                Send(AuthenticateResponse.Failed(logInViaEmailRequest.Ticket,
                    AuthenticationFailedReason.Busy, null, 0));
            }
            catch (Exception ex)
            {
                _Log.Error(ex);
                Send(AuthenticateResponse.Failed(logInViaEmailRequest.Ticket,
                    AuthenticationFailedReason.Unknown, null, 0));
            }
        }*/
    }
}
