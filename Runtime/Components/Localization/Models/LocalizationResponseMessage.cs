using SturfeeVPS.Core.Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SturfeeVPS.SDK
{
    /// <summary>
    /// Server response for localization request
    /// </summary>
    [Serializable]
    public class LocalizationResponseMessage
    {
        public int requestId;
        public string trackingId;
        public LocalizationResponse response;
        public LocalizationError error;

        public static LocalizationResponseMessage ParseProtobufResponseMessage(ResponseMessage responseMessage)
        {
            LocalizationResponseMessage localizationResponseMessage = new LocalizationResponseMessage();
            if(responseMessage.Error == null)
            {
                localizationResponseMessage.requestId = (int)responseMessage.RequestId;
                localizationResponseMessage.trackingId = responseMessage.TrackingId;
                localizationResponseMessage.response = LocalizationResponse.ParseProtobufResponse(responseMessage.Response);                
            }
            else
            {
                localizationResponseMessage.error = new LocalizationError
                {
                    message = responseMessage.Error.Message,
                    code = (ErrorCodes)responseMessage.Error.Code
                };
            }

            return localizationResponseMessage;
        }

    }


    [Serializable]
    public class LocalizationError
    {
        public string message;
        public ErrorCodes code;
    }


    public enum ErrorCodes
    {
        NoError = 0,
        UnExpectedServerError = 5211,
        ProtocolError = 5212,
        ValidationError = 5213,
        OutOfCoverageError = 5214
    }

}
