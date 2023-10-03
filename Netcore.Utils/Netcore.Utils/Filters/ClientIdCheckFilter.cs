using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using NetCore.Utils.Extensions;
using NetCore.Utils.Log;
using System;
using System.Linq;
using System.Net;

namespace NetCore.Utils.Filters
{
    public class ClientIdCheckFilter : ActionFilterAttribute
    {
        private readonly string _safelist;

        public ClientIdCheckFilter
            (IConfiguration configuration)
        {
            _safelist = configuration["AdminSafeList"];
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            try
            {
                var remoteIp = context.HttpContext.GetRemoteIPAddress();
                //NLogManager.LogInfo(string.Format(
                //    "Remote IpAddress: {0} SafeList: {1}", remoteIp, _safelist));

                string[] ip = _safelist.Split(';');

                // mở cho tất cả ip vào check
                if (ip.Any(i => i == "all"))
                {
                    base.OnActionExecuting(context);
                    return;
                }
                var bytes = remoteIp.GetAddressBytes();
                var badIp = true;
                foreach (var address in ip)
                {
                    var testIp = IPAddress.Parse(address);
                    if (testIp.GetAddressBytes().SequenceEqual(bytes))
                    {
                        badIp = false;
                        break;
                    }
                }

                if (badIp)
                {
                    NLogManager.LogInfo(string.Format(
                        "Forbidden Request from Remote IP address: {0}", remoteIp));
                    context.Result = new StatusCodeResult(401);
                    return;
                }

                base.OnActionExecuting(context);
            }
            catch (Exception ex)
            {
                NLogManager.LogException(ex);
            }
        }
    }
}