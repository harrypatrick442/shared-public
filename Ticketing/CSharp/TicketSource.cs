using System;
using System.Timers;
using Core.Interfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Ticketing
{
    /// <summary>
    /// Speaks JSON. Pleaes use JSON it's nicer than parameters in a url
    /// </summary>
    public class TicketSource
    {
        private static long _NTicket = 1;
        private static object _NTicketLockObject = new object();
        public static long GetNextTicket()
        {
            lock (_NTicketLockObject)
            {
                long ticket = _NTicket++;
                if (_NTicket >= long.MaxValue)
                {
                    _NTicket = 1;
                }
                return ticket;
            }
        }
    }
}
