using Microsoft.VisualStudio.TestTools.UnitTesting;
using Core;
using Core.DataMemberNames;
using JSON;
using Core.Timing;
using Logging;
using Shutdown;
using Core.Ticketing;
using System;
using System.Collections.Generic;
using System.Threading;
using Core.Messages.Responses;
using Core.Messages.Messages;

namespace Testing
{
    [TestClass]
    public class InterseTicketedSenderUnitTests
    {
        private InverseTicketedSender _InverseTicketedSender;

        [TestInitialize]
        public void Initialize()
        {
            ShutdownManager.Initialize((code) => { }, ()=>Logs.Default, throwErrorOnAlreadyInitialized:false);
            _InverseTicketedSender = new InverseTicketedSender();
        }

        [TestMethod]
        public void SendsCancelsCorrectly()
        {
            string sent = null;
            long responseTicket = 3433344;
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            Assert.ThrowsException<OperationCanceledException>(() => {
                _InverseTicketedSender.Send<TypedInverseTicketedMessage, TypedInverseTicketedMessage>(
                new TypedInverseTicketedMessage(TicketedMessageType.Ticketed) { },
                responseTicket, 5000, cancellationTokenSource.Token, (string beingSent) =>
                {
                    sent = beingSent;
                    cancellationTokenSource.Cancel();
                });
            });
            Assert.IsNotNull(sent);
            Assert.IsTrue(sent.Length > 0, "sent was empty string");
            TypedInverseTicketedMessage typedInverseTicketedMessage =
                Json.Deserialize<TypedInverseTicketedMessage>(sent);
            Assert.IsNotNull(sent);
            Assert.IsNotNull(typedInverseTicketedMessage.Ticket);
            Assert.IsTrue(typedInverseTicketedMessage.Ticket == responseTicket);
            Assert.IsNotNull(typedInverseTicketedMessage.InverseTicket);
            Assert.IsTrue(typedInverseTicketedMessage.Ticket != typedInverseTicketedMessage.InverseTicket);
        }
        
        [TestMethod]
        public void TimesOutCorrectly()
        {
            string sent = null;
            long responseTicket = 233232323;
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            Assert.ThrowsException<TimeoutException>(() => {
                _InverseTicketedSender.Send<TypedInverseTicketedMessage, TypedInverseTicketedMessage>(
                new TypedInverseTicketedMessage(TicketedMessageType.Ticketed) { },
                responseTicket, 1000, cancellationTokenSource.Token, (string beingSent) =>
                {
                    sent = beingSent;
                });
            });
            Assert.ThrowsException<TimeoutException>(() => {
                _InverseTicketedSender.Send<TypedInverseTicketedMessage, TypedInverseTicketedMessage>(
                new TypedInverseTicketedMessage(TicketedMessageType.Ticketed) { },
                responseTicket, 500, cancellationTokenSource.Token, (string beingSent) =>
                {
                    sent = beingSent;
                });
            });
            long started = TimeHelper.MillisecondsNow;
            Assert.ThrowsException<TimeoutException>(() => {
                _InverseTicketedSender.Send<TypedInverseTicketedMessage, TypedInverseTicketedMessage>(
               new TypedInverseTicketedMessage(TicketedMessageType.Ticketed) { },
               responseTicket, 20000, cancellationTokenSource.Token, (string beingSent) =>
               {
                   sent = beingSent;
               });
            });
            Assert.IsTrue(TimeHelper.MillisecondsNow - started >= 20000);
        }
        
        [TestMethod]
        public void HandlesResponseCorrectly()
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            long responseTicket = 123121;
            InverseTicketedResponseWithTicket response = _InverseTicketedSender.Send<TypedInverseTicketedMessage, InverseTicketedResponseWithTicket>(
               new TypedInverseTicketedMessage(TicketedMessageType.Ticketed) { },
               responseTicket, 20000, cancellationTokenSource.Token, (Action<string>)((string beingSent) =>
               {
                   TypedInverseTicketedMessage sent = Json.Deserialize<TypedInverseTicketedMessage>(beingSent);
                   InverseTicketedResponseWithTicket response = new InverseTicketedResponseWithTicket(sent.InverseTicket);
                   _InverseTicketedSender.HandleMessage(response, beingSent);
               }));
            Assert.IsNotNull(response);
            Assert.IsNotNull(response.InverseTicket);
            response = _InverseTicketedSender.Send<TypedInverseTicketedMessage, InverseTicketedResponseWithTicket>(
               new TypedInverseTicketedMessage(TicketedMessageType.Ticketed) { },
               responseTicket, 20000, cancellationTokenSource.Token, (Action<string>)((string beingSent) =>
               {
                   new Thread((ThreadStart)(() => {
                       Thread.Sleep(3000);
                       TypedInverseTicketedMessage sent = Json.Deserialize<TypedInverseTicketedMessage>(beingSent);
                       InverseTicketedResponseWithTicket response = new InverseTicketedResponseWithTicket(sent.InverseTicket);
                       _InverseTicketedSender.HandleMessage(response, beingSent);
                   })).Start();
               }));
            Assert.IsNotNull(response); 
            Assert.IsNotNull(response.InverseTicket);

        }
        [TestMethod]
        public void HandlesMultipleSendsAtOnce()
        {
            long responseTicket = 233223;
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            TypedInverseTicketedMessage toSend1 = new TypedInverseTicketedMessage(TicketedMessageType.Ticketed) { };
            TypedInverseTicketedMessage toSend2 = new TypedInverseTicketedMessage(TicketedMessageType.Ticketed) { };
            CountdownLatch countdownLatch = new CountdownLatch(2);
            CountdownLatch countdownLatchDone = new CountdownLatch(2);
            InverseTicketedResponseWithTicket response1 = null;
            InverseTicketedResponseWithTicket response2 = null;
            new Thread(() =>
            {
                response1 = _InverseTicketedSender.Send<TypedInverseTicketedMessage, InverseTicketedResponseWithTicket>(
                toSend1,
                responseTicket, 20000, cancellationTokenSource.Token, (Action<string>)((string beingSent) =>
                {
                    countdownLatch.Signal();
                    countdownLatch.Wait();
                    TypedInverseTicketedMessage sent = Json.Deserialize<TypedInverseTicketedMessage>(beingSent);
                    InverseTicketedResponseWithTicket response = new InverseTicketedResponseWithTicket(sent.InverseTicket);
                    _InverseTicketedSender.HandleMessage(response, beingSent);
                }));
                countdownLatchDone.Signal();
            }).Start();
            new Thread(() =>
            {
                response2 = _InverseTicketedSender.Send<TypedInverseTicketedMessage, InverseTicketedResponseWithTicket>(
                toSend2,
                responseTicket, 20000, cancellationTokenSource.Token, (Action<string>)((string beingSent) =>
                {
                    countdownLatch.Signal();
                    countdownLatch.Wait();
                    TypedInverseTicketedMessage sent = Json.Deserialize<TypedInverseTicketedMessage>(beingSent);
                    InverseTicketedResponseWithTicket response = new InverseTicketedResponseWithTicket(sent.InverseTicket);
                    _InverseTicketedSender.HandleMessage(response, beingSent);
                }));
                countdownLatchDone.Signal();
            }).Start();
            countdownLatchDone.Wait();
            Assert.IsFalse(toSend1.InverseTicket == toSend2.InverseTicket);
            Assert.IsNotNull(response1);
            Assert.IsNotNull(response2);
            Assert.IsNotNull(response1.InverseTicket);
            Assert.IsNotNull(response2.InverseTicket);
            Assert.IsTrue(response1.InverseTicket == toSend1.InverseTicket);
            Assert.IsTrue(response2.InverseTicket == toSend2.InverseTicket);
        }
        [TestMethod]
        public void HandlesManySendsAtOnce()
        {
            CancellationTokenSource cancellationTokenSourceWasExceptionThrown = new CancellationTokenSource();
            int N_TO_SEND = 1000;
            long responseTicket = 3434343;
            CountdownLatch countdownLatch = new CountdownLatch(N_TO_SEND);
            CountdownLatch countdownLatchGotAllResponses = new CountdownLatch(N_TO_SEND);
            InverseTicketedResponseWithTicket[] responses = new InverseTicketedResponseWithTicket[N_TO_SEND];
            Func<int, Action> createInstance = (int n) =>
            {
                return () =>
                {
                    TypedInverseTicketedMessage toSend1 = new TypedInverseTicketedMessage(TicketedMessageType.Ticketed) { };
                    new Thread(() =>
                    {
                        try
                        {
                            responses[n] =
                                  _InverseTicketedSender.Send<TypedInverseTicketedMessage, InverseTicketedResponseWithTicket>(
                                toSend1,
                                responseTicket, 20000, cancellationTokenSourceWasExceptionThrown.Token, (Action<string>)((string beingSent) =>
                                {
                                    countdownLatch.Signal();
                                    countdownLatch.Wait();
                                    TypedInverseTicketedMessage sent = Json.Deserialize<TypedInverseTicketedMessage>(beingSent);
                                    InverseTicketedResponseWithTicket response = new InverseTicketedResponseWithTicket(sent.InverseTicket);
                                    _InverseTicketedSender.HandleMessage(response, beingSent);
                                }));
                        }
                        catch (Exception ex)
                        {
                            cancellationTokenSourceWasExceptionThrown.Cancel();
                        }
                        finally
                        {
                            countdownLatchGotAllResponses.Signal();
                        }
                    }).Start();
                };
            };
            Action[] sends = new Action[N_TO_SEND];
            for (int i = 0; i < N_TO_SEND; i++)
                sends[i]=createInstance(i);
            for (int i = 0; i < N_TO_SEND; i++)
                sends[i]();
            countdownLatchGotAllResponses.Wait();
            Assert.IsFalse(cancellationTokenSourceWasExceptionThrown.IsCancellationRequested);
            HashSet<long> seenTokens = new HashSet<long>();
            foreach (InverseTicketedResponseWithTicket response in responses)
            {
                Assert.IsFalse(seenTokens.Contains(response.InverseTicket));
                seenTokens.Add(response.InverseTicket);
            }    
        }

        [TestCleanup]
        public void Cleanup()
        {
            _InverseTicketedSender.Dispose();
        }
    }
}
