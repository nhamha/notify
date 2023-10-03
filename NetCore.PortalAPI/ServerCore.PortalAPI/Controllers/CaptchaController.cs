using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ServerCore.Utilities.Captcha;
using ServerCore.Utilities.Utils;

namespace ServerCore.PortalAPI.Controllers
{
    [Route("Captcha")]
    [ApiController]
    public class CaptchaController : ControllerBase
    {
        private readonly Captcha _captcha;

        public CaptchaController(Captcha captcha)
        {
            _captcha = captcha;
        }

        [HttpGet("Get")]
        public ActionResult<CaptchaModel> Get(int length = 3, int width = 110, int height = 50)
        {
            try
            {
                if (length < 3)
                {
                    length = 3;
                }
                if (width > 200)
                {
                    width = 200;
                }
                if (height > 100)
                {
                    height = 100;
                }
                var captcha = _captcha.GetCaptcha(length, width, height);
                if (captcha != null)
                {
                    return captcha;
                }
            }
            catch(Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return new CaptchaModel(null, null);
        }

        [HttpGet("CaptchaVerify")]
        public int CaptchaVerify(string captchaText, string verify)
        {
            try
            {
                return _captcha.VerifyCaptcha(captchaText, verify);
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return -1;
        }
    }
}
