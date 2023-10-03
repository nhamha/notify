using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PortalAPI.Models
{
    [Serializable]
    public class ResponseMessage
    {
        public int ResponseStatus { get; set; }

        public string Message { get; set; }

        public ResponseMessage()
        {
        }

        public ResponseMessage(int responseStatus, string message)
        {
            this.ResponseStatus = responseStatus;
            this.Message = message;
        }
    }
}