using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ServerCore.Utilities.Utils
{
    public class ResponseBuilder
    {
        public int Code { get; set; }
        public string Description { get; set; }
        public dynamic Data { get; set; }

        public ResponseBuilder()
        {
            
        }

        public ResponseBuilder(int code, string lng, dynamic data)
        {
            Description = MessageBuilder.Build(code, lng);
            Code = code;
            Data = data;
        }

        public ResponseBuilder(ErrorCodes code, string lng, dynamic data)
        {
            Description = MessageBuilder.Build((int)code, lng);
            Code = (int)code;
            Data = data;
        }

        public ResponseBuilder(int code, string lng)
        {
            Description = MessageBuilder.Build(code, lng);
            Code = code;
            Data = null;
        }

        public ResponseBuilder(ErrorCodes code, string lng)
        {
            Description = MessageBuilder.Build((int)code, lng);
            Code = (int)code;
            Data = null;
        }
    }
}
