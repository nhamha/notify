using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace ServerCore.Utilities.Utils
{
    public class MessageBuilder
    {
        //public static ConcurrentDictionary<string, string> _viMap;
        //public static ConcurrentDictionary<string, string> _laoMap;

        //public static JObject _viJson;
        //public static JObject _laoJson;
        public static JObject _langJson;
        //public static JObject _enJson;
        static MessageBuilder()
        {
            string path = Directory.GetCurrentDirectory();
            string viText = File.ReadAllText(path + "/Languages/vi.json");
            string laoText = File.ReadAllText(path + "/Languages/lao.json");
            string enText = File.ReadAllText(path + "/Languages/en.json");
            StringBuilder jsonBuilder = new StringBuilder();
            jsonBuilder.Append("{\"vi\":");
            jsonBuilder.Append(viText);

            jsonBuilder.Append(",\"lao\":");
            jsonBuilder.Append(laoText);

            jsonBuilder.Append(",\"en\":");
            jsonBuilder.Append(enText);

            jsonBuilder.Append("}");
            //string enText = File.ReadAllText(path + "/Languages/en.json");
            //string kmText = File.ReadAllText(path + "/Languages/km.json");
            //string thText = File.ReadAllText(path + "/Languages/th.json");

            //_viJson = JObject.Parse(viText);
            //_laoJson = JObject.Parse(laoText);
            _langJson = JObject.Parse(jsonBuilder.ToString());
            //_enJson = JObject.Parse(enText);
            //JObject kmJson = JObject.Parse(kmText);
            //JObject thJson = JObject.Parse(thText);

            //_mapLng = new Dictionary<string, JObject>();
            //_mapLng.Add("vi", viJson);
            //_mapLng.Add("lao", laoJson);
            //_mapLng.Add("en", enJson);
            //_mapLng.Add("km", kmJson);
            //_mapLng.Add("th", thJson);

            //_viMap = new ConcurrentDictionary<string, string>();
            //_laoMap = new ConcurrentDictionary<string, string>();
            //var allKeys = _viJson.Properties().Select(p => p.Name).ToList();
            //foreach(var key in allKeys)
            //{
            //    _viMap.TryAdd(key, (string)_viJson[key]);
            //    _laoMap.TryAdd(key, (string)_laoJson[key]);
            //}
        }

        public static string Build(ErrorCodes code, string language)
        {
            return Build((int)code, language);
        }

        public static string Build(int code, string language)
        {
            try
            {
                //var watch = Stopwatch.StartNew();
                //var builder = new ConfigurationBuilder()
                // .SetBasePath(Directory.GetCurrentDirectory())
                // .AddJsonFile($"Languages/{lng}.json", optional: true, reloadOnChange: true);

                //IConfigurationRoot configuration = builder.Build();
                //string msg = configuration[code.ToString()];
                //if (msg != null)
                //    return msg;

                //return configuration[((int)ErrorCodes.SERVER_ERROR).ToString()];

                string lng = string.IsNullOrEmpty(language) ? "vi" : language;
                string msg = "language undefine";
                if (lng.Equals("vi") || lng.Equals("lao") || lng.Equals("en"))
                {
                    msg = (string)_langJson[lng][code.ToString()];
                }

                //if (lng.Equals("vi"))
                //    msg = (string)_viJson[code.ToString()];
                //else if (lng.Equals("lao"))
                //    msg = (string)_laoJson[code.ToString()];
                //else if (lng.Equals("en"))
                //    msg = (string)_enJson[code.ToString()];

                //if (lng.Equals("vi"))
                //    _viMap.TryGetValue(code.ToString(), out msg);
                //else if (lng.Equals("lao"))
                //    _laoMap.TryGetValue(code.ToString(), out msg);

                //watch.Stop();
                //long elapsedMs = watch.ElapsedMilliseconds;
                //NLogManager.Info(string.Format("Build: {0}", elapsedMs));
                return msg;
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return "language undefine";
        }

        //public MessageBuilder(int code, string language)
        //{
        //    try
        //    {
        //        var builder = new ConfigurationBuilder()
        //     .SetBasePath(Directory.GetCurrentDirectory())
        //     .AddJsonFile($"Languages/{language}.json", optional: true, reloadOnChange: true);

        //        IConfigurationRoot configuration = builder.Build();

        //        Message = configuration[code.ToString()];
        //    }
        //    catch (Exception ex)
        //    {
        //        NLogManager.Exception(ex);
        //        Message = ex.ToString();
        //    }
        //}

        //public MessageBuilder(ErrorCodes code, string language)
        //{
        //    try
        //    {
        //        var builder = new ConfigurationBuilder()
        //     .SetBasePath(Directory.GetCurrentDirectory())
        //     .AddJsonFile($"Languages/{language}.json", optional: true, reloadOnChange: true);

        //        IConfigurationRoot configuration = builder.Build();

        //        Message = configuration[code.ToString()];
        //    }
        //    catch (Exception ex)
        //    {
        //        NLogManager.Exception(ex);
        //        Message = ex.ToString();
        //    }
        //}
    }
}
