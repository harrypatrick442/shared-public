using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace Core.Enums
{
    [Flags]
    public enum DaysOfWeek
    {
        None=0,
        Monday=1,//As user goes from A to B and closes A removed from B.
        Tuesday=2,//Connection from A to B can be seen by user
        Wednesday=4,
        Thursday= 8,
        Friday= 16,
        Saturday= 32,
        Sunday= 64
    }
}

