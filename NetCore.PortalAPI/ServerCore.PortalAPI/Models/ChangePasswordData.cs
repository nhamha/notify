﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServerCore.PortalAPI.Models
{
    public class ChangePasswordData
    {
        public string OldPassword{get;set;}
        public string NewPassword{get;set;}
        public int PlatformId{get;set;}
        public string CaptchaText{get;set;}
        public string CaptchaToken{get;set;}
    }
}