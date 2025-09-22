using Core.Exceptions;
using Core.Maths.Enums;
using System;
namespace Core.Maths.Exceptions
{
    public class ConvergenceException : LazyMessageException
    {
        public ConvergenceFailureType FailureType { get; }
        public ConvergenceException(
            ConvergenceFailureType failureType, 
            Func<string> generateMessage):base(generateMessage)
        {
            FailureType = failureType;
        }
        public ConvergenceException(
            ConvergenceFailureType failureType, 
            Func<string> generateMessage, 
            Exception innerException)
            :base(generateMessage, innerException) 
        {
            FailureType = failureType;
        }
        public ConvergenceException(
            Func<string> generateMessage,
            ConvergenceException innerException)
            : base(generateMessage, innerException)
        {
            FailureType = innerException.FailureType;
        }
    }
}