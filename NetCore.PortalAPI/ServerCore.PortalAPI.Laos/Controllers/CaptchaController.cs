using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
            //string lang = Utils.GetLanguage(Request.HttpContext);
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
                    //return new ResponseBuilder(ErrorCodes.SUCCESS, lang, captcha);
                }
            }
            catch(Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return new CaptchaModel(null, null);
        }

        [HttpGet("check")]
        public ActionResult<int> check(string text, string verify)
        {
            int captcha = _captcha.VerifyCaptcha(text, verify);
            return captcha;
        }
    }
}
