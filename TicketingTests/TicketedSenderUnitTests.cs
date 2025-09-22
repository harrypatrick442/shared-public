using Microsoft.VisualStudio.TestTools.UnitTesting;
using Core;
using JSON;
using Core.Timing;
using Logging;
using Shutdown;
using Core.Ticketing;
using System;
using System.Collections.Generic;
using System.Threading;
using Core.Messages.Messages;

namespace Testing
{
    [TestClass]
    public class TicketedSendUnitTests
    {
        // In order to run the below test(s), 
        // please follow the instructions from https://docs.microsoft.com/en-us/microsoft-edge/webdriver-chromium
        // to install Microsoft Edge WebDriver.

        private TicketedSender _TicketedSender;

        [TestInitialize]
        public void Initialize()
        {
            ShutdownManager.Initialize((code) => { }, ()=>Logs.Default, throwErrorOnAlreadyInitialized:false);
            _TicketedSender = new TicketedSender();
        }

        [TestMethod]
        public void SendsCancelsCorrectly()
        {
            string sent = null;
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            Assert.ThrowsException<OperationCanceledException>(() => {
                _TicketedSender.Send<TypedTicketedMessage, TypedTicketedMessage>(
                new TypedTicketedMessage() { }, 5000, cancellationTokenSource.Token, (string beingSent) =>
                {
                    sent = beingSent;
                    cancellationTokenSource.Cancel();
                });
            });
            Assert.IsNotNull(sent);
            Assert.IsTrue(sent.Length > 0, "sent was empty string");
            TypedTicketedMessage typedTicketedMessageSent = Json.Deserialize<TypedTicketedMessage>(sent);
            Assert.IsNotNull(sent);
            Assert.IsNotNull(typedTicketedMessageSent.Ticket);
            Assert.IsTrue(typedTicketedMessageSent.Ticket.Length > 0, "ticket was empty string");
        }
        [TestMethod]
        public void TimesOutCorrectly()
        {
            string sent = null;
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            Assert.ThrowsException<TimeoutException>(() => {
                _TicketedSender.Send<TypedTicketedMessage, TypedTicketedMessage>(
                new TypedTicketedMessage() { }, 1000, cancellationTokenSource.Token, (string beingSent) =>
                {
                    sent = beingSent;
                });
            });
            Assert.ThrowsException<TimeoutException>(() => {
                _TicketedSender.Send<TypedTicketedMessage, TypedTicketedMessage>(
                new TypedTicketedMessage() { }, 500, cancellationTokenSource.Token, (string beingSent) =>
                {
                    sent = beingSent;
                });
            });
            long started = TimeHelper.MillisecondsNow;
            Assert.ThrowsException<TimeoutException>(() => {
                _TicketedSender.Send<TypedTicketedMessage, TypedTicketedMessage>(
                new TypedTicketedMessage() { }, 20000, cancellationTokenSource.Token, (string beingSent) =>
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
            TypedTicketedMessage response = _TicketedSender.Send<TypedTicketedMessage, TypedTicketedMessage>(
                new TypedTicketedMessage() { }, 1000, cancellationTokenSource.Token, (Action<string>)((string beingSent) =>
                {
                    _TicketedSender.HandleMessage((TicketedMessageBase)Json.Deserialize<TypedTicketedMessage>(beingSent), beingSent);
                }));
            Assert.IsNotNull(response);
            Assert.IsNotNull(response.Ticket);
            Assert.IsTrue(response.Ticket.Length > 0, "ticket was empty string");
            response = _TicketedSender.Send<TypedTicketedMessage, TypedTicketedMessage>(
                new TypedTicketedMessage() { }, 10000, cancellationTokenSource.Token, (Action<string>)((string beingSent) =>
                {
                    new Thread((ThreadStart)(() => {
                        Thread.Sleep(3000);
                        _TicketedSender.HandleMessage((TicketedMessageBase)Json.Deserialize<TypedTicketedMessage>(beingSent), beingSent);
                    })).Start();
                }));
            Assert.IsNotNull(response);
        }
        [TestMethod]
        public void HandlesMultipleSendsAtOnce()
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            TypedTicketedMessage toSend1 = new TypedTicketedMessage() { };
            TypedTicketedMessage toSend2 = new TypedTicketedMessage() { };
            CountdownLatch countdownLatch = new CountdownLatch(2);
            CountdownLatch countdownLatchDone = new CountdownLatch(2);
            TypedTicketedMessage response1 = null;
            TypedTicketedMessage response2 = null;
            new Thread(() =>
            {
                response1 = _TicketedSender.Send<TypedTicketedMessage, TypedTicketedMessage>(toSend1
                , 1000, cancellationTokenSource.Token, (Action<string>)((string beingSent) =>
                {
                    countdownLatch.Signal();
                    countdownLatch.Wait();
                    _TicketedSender.HandleMessage((TicketedMessageBase)Json.Deserialize<TypedTicketedMessage>(beingSent), beingSent);
                }));
                countdownLatchDone.Signal();
            }).Start();
            new Thread(() =>
            {
                response2 = _TicketedSender.Send<TypedTicketedMessage, TypedTicketedMessage>(toSend2
                , 1000, cancellationTokenSource.Token, (Action<string>)((string beingSent) =>
                {
                    countdownLatch.Signal();
                    countdownLatch.Wait();
                    _TicketedSender.HandleMessage((TicketedMessageBase)Json.Deserialize<TypedTicketedMessage>(beingSent), beingSent);
                }));
                countdownLatchDone.Signal();
            }).Start();
            countdownLatchDone.Wait();
            Assert.IsFalse(toSend1.Ticket == toSend2.Ticket);
            Assert.IsNotNull(response1);
            Assert.IsNotNull(response2);
            Assert.IsNotNull(response1.Ticket);
            Assert.IsNotNull(response2.Ticket);
            Assert.IsTrue(response1.Ticket == toSend1.Ticket);
            Assert.IsTrue(response2.Ticket == toSend2.Ticket);
        }
        [TestMethod]
        public void HandlesManySendsAtOnce()
        {
            CancellationTokenSource cancellationTokenSourceWasExceptionThrown = new CancellationTokenSource();
            int N_TO_SEND = 1000;
            CountdownLatch countdownLatch = new CountdownLatch(N_TO_SEND);
            CountdownLatch countdownLatchGotAllResponses = new CountdownLatch(N_TO_SEND);
            TypedTicketedMessage[] responses = new TypedTicketedMessage[N_TO_SEND];
            Func<int, Action> createInstance = (int n) =>
            {
                return () =>
                {
                    TypedTicketedMessage toSend1 = new TypedTicketedMessage() { };
                    new Thread(() =>
                    {
                        try
                        {
                            responses[n] = _TicketedSender.Send<TypedTicketedMessage, TypedTicketedMessage>(toSend1
                            , 1000000, cancellationTokenSourceWasExceptionThrown.Token, (Action<string>)((string beingSent) =>
                            {
                                countdownLatch.Signal();
                                countdownLatch.Wait();
                                _TicketedSender.HandleMessage((TicketedMessageBase)Json.Deserialize<TypedTicketedMessage>(beingSent), beingSent);
                            }));
                        }
                        catch (Exception ex)
                        {
                            cancellationTokenSourceWasExceptionThrown.Cancel();
                        }
                        finally {

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
            HashSet<string> seenTokens = new HashSet<string>();
            foreach (TypedTicketedMessage response in responses)
            {
                Assert.IsFalse(seenTokens.Contains(response.Ticket));
                seenTokens.Add(response.Ticket);
            }    
        }

        [TestCleanup]
        public void Cleanup()
        {
            _TicketedSender.Dispose();
        }
    }
}
