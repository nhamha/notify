using System;
using System.Collections.Generic;
using System.Text;

namespace ServerCore.Utilities.Models
{
    class ErrorMessage
    {
        public int ErrorCode { set; get; }
        public string Message { set; get; }

        public ErrorMessage(int erroreCode, string message)
        {
            ErrorCode = erroreCode;
            Message = message;
        }
    }
}
