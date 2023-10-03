using System;
using System.Collections.Generic;
using System.Text;

namespace ServerCore.Utilities.Captcha
{
    public class CaptchaModel
    {
        public string Token { get; set; }
        public string Image { get; set; }

        public CaptchaModel(string token, string image)
        {
            Token = token;
            Image = image;
        }
    }
}
