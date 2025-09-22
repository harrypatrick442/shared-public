using System;
using Core.Exceptions;

namespace Core.Queueing
{
    public class QueueBackedUpException:OperationFailedException
    {
        public QueueBackedUpException(string queueNameOrType, int currentNEntries) : base(GetMessage(queueNameOrType, currentNEntries))
        {

        }
        public QueueBackedUpException(string queueNameOrType, int currentNEntries, Exception innerException) : base(GetMessage(queueNameOrType, currentNEntries), innerException)
        {

        }
        private static string GetMessage(string queueNameOrType, int currentNEntries) { 
            return $"The queue {queueNameOrType} is backed up with {currentNEntries} entries";
        }
    }
}