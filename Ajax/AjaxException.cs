using System;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
namespace Ajax
{
    [DataContract]
    public class AjaxException : Exception
    {
        private AjaxExceptionType _AjaxExceptionType;
        [JsonPropertyName("ajaxExceptionType")]
        [JsonInclude]
        [DataMember(Name = "ajaxExceptionType")]
        public AjaxExceptionType AjaxExceptionType { get { return _AjaxExceptionType; } protected set { _AjaxExceptionType = value; } }

        

        private AjaxException(string message, Exception innerParsingException, AjaxExceptionType ajaxExceptionType) : base(message, innerParsingException)
        {
            _AjaxExceptionType = ajaxExceptionType;
        }
        protected AjaxException() { 
        
        }
        public static AjaxException Request(Exception innerException)
        {
            return new AjaxException("Request", innerException, AjaxExceptionType.Request);
        }
        public static AjaxException Remote(Exception innerException)
        {
            return new AjaxException("Remote", innerException, AjaxExceptionType.Remote);
        }
        public static AjaxException ParsingPayload(Exception innerException)
        {
            return new AjaxException("Parsing payload", innerException, AjaxExceptionType.ParsingPayload);
        }
        public static AjaxException Internal(Exception innerException)
        {
            return new AjaxException("Internal", innerException, AjaxExceptionType.Internal);
        }
        public static AjaxException Cancelled(Exception innerException)
        {
            return new AjaxException("Cancelled", innerException, AjaxExceptionType.Cancelled);
        }
    }
}
