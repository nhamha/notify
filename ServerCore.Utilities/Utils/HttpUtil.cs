using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore.Utilities.Utils
{
    public class HttpUtil
    {
        public static async Task<T> GetAsync<T>(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            HttpClient client = HttpClientFactory.Create();
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var res = await response.Content.ReadAsAsync<T>();
            return res;
        }

        public static async Task<string> GetAsync(string url)
        {
            //var request = new HttpRequestMessage(HttpMethod.Get, url);
            HttpClient client = HttpClientFactory.Create();
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var res = await response.Content.ReadAsStringAsync();
            return res;
        }

        public static async Task<T> PostAsync<T>(string url, object data)
        {
            StringContent httpContent = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            //var request = new HttpRequestMessage(HttpMethod.Post, url);
            HttpClient client = HttpClientFactory.Create();
            var response = await client.PostAsync(url, httpContent);
            response.EnsureSuccessStatusCode();
            var res = await response.Content.ReadAsAsync<T>();
            return res;
        }

        public static async Task<T> PostAsync<T>(string uri, dynamic data, IDictionary<string, string> dictionary, bool isLog = true)
        {
            try
            {
                var timeout = 30000;//Convert.ToInt32(ConfigurationManager.AppSettings["APITimeout"]);
                using (var cts = new CancellationTokenSource())
                {
                    cts.CancelAfter(timeout);
                    using (var client = new HttpClient())
                    {
                        NLogManager.Info(string.Format("Đầu vào {0}: {1}", uri, JsonConvert.SerializeObject(data)));
                        var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                        client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
                        if (dictionary.Count > 0)
                        {
                            foreach (var dic in dictionary)
                            {
                                client.DefaultRequestHeaders.TryAddWithoutValidation(dic.Key, dic.Value);
                            }
                        }
                        var result = await client.PostAsync(uri, content, cts.Token).ConfigureAwait(false);
                        string resultContent = await result.Content.ReadAsStringAsync();
                        if (string.IsNullOrWhiteSpace(resultContent)) return default(T);
                        if (isLog)
                            NLogManager.Info("Đầu ra: " + resultContent);
                        return JsonConvert.DeserializeObject<T>(resultContent);
                    }
                }
            }
            catch (TaskCanceledException ex)
            {
                NLogManager.Exception(ex);
                return default(T);
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
                return default(T);
            }
        }
        public static async Task<JToken> PostAsync(string uri, dynamic data, IDictionary<string, string> dictionary, bool isLog = true)
        {
            try
            {
                var timeout = 30000;// Convert.ToInt32(ConfigurationManager.AppSettings["APITimeout"]);
                using (var cts = new CancellationTokenSource())
                {
                    cts.CancelAfter(timeout);
                    using (var client = new HttpClient())
                    {
                        NLogManager.Info(string.Format("Đầu vào {0}: {1}", uri, JsonConvert.SerializeObject(data)));
                        var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                        client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
                        if (dictionary.Count > 0)
                        {
                            foreach (var dic in dictionary)
                            {
                                client.DefaultRequestHeaders.TryAddWithoutValidation(dic.Key, dic.Value);
                            }
                        }
                        var result = await client.PostAsync(uri, content, cts.Token).ConfigureAwait(false);
                        //await Task.Delay(100);
                        string resultContent = await result.Content.ReadAsStringAsync();
                        //var resultContent = "{\"responseStatus\":1,\"cardSeri\":\"58402319329\",\"cardCode\":\"0571788012759\",\"cardType\":\"VTT\",\"transDate\":\"26/08/2017 12:35\",\"desc\":\"\",\"data\":[{\"responseStatus\":1,\"cardSeri\":\"58402319329\",\"cardCode\":\"0571788012759\",\"cardType\":\"\",\"desc\":\"VNPT EPay\",\"data\":[]}]}";
                        if (isLog)
                            NLogManager.Info("Đầu ra: " + resultContent);
                        return JObject.Parse(resultContent);
                    }
                }
            }
            catch (TaskCanceledException ex)
            {
                NLogManager.Exception(ex);
            }
            catch (Exception ex)
            {
                NLogManager.Exception(ex);
            }
            return string.Empty;
        }
        /// <summary>
        /// Send post common json
        /// </summary>
        /// <param name="postData"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string SendPost(string postData, string url)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            byte[] data = encoding.GetBytes(postData);
            ServicePointManager.Expect100Continue = false;
            CookieContainer cookie = new CookieContainer();
            HttpWebRequest myRequest = (HttpWebRequest) WebRequest.Create(url);
            myRequest.Method = "POST";
            myRequest.ContentLength = data.Length;
            myRequest.ContentType = "application/json; encoding='utf-8'";
            myRequest.KeepAlive = false;
            myRequest.CookieContainer = cookie;
            myRequest.AllowAutoRedirect = false;
            using (Stream requestStream = myRequest.GetRequestStream())
            {
                requestStream.Write(data, 0, data.Length);
            }

            string response = string.Empty;
            try
            {
                using (HttpWebResponse myResponse = (HttpWebResponse)myRequest.GetResponse())
                {
                    using (Stream respStream = myResponse.GetResponseStream())
                    {
                        using (StreamReader respReader = new StreamReader(respStream))
                        {
                            response = respReader.ReadToEnd();
                        }
                    }
                }
            }
            catch (WebException webEx)
            {
                if (webEx.Response != null)
                {
                    using (HttpWebResponse exResponse = (HttpWebResponse)webEx.Response)
                    {
                        using (StreamReader sr = new StreamReader(exResponse.GetResponseStream()))
                        {
                            response = sr.ReadToEnd();
                        }
                    }
                }
            }
            return response;
        }

        public static string SendPostWithHeader(string postData, string url, WebHeaderCollection headers)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            byte[] data = encoding.GetBytes(postData);
            ServicePointManager.Expect100Continue = false;
            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(url);
            myRequest.Method = "POST";
            // Set header
            myRequest.Headers = headers;

            myRequest.ContentLength = data.Length;
            myRequest.ContentType = "application/json; encoding='utf-8'";
            myRequest.KeepAlive = false;
            myRequest.AllowAutoRedirect = false;
           
            using (Stream requestStream = myRequest.GetRequestStream())
            {
                requestStream.Write(data, 0, data.Length);
            }
            string response = null;
            try
            {
                using (HttpWebResponse myResponse = (HttpWebResponse)myRequest.GetResponse())
                {
                    using (Stream respStream = myResponse.GetResponseStream())
                    {
                        using (StreamReader respReader = new StreamReader(respStream))
                        {
                            response = respReader.ReadToEnd();
                        }
                    }
                }
            }
            catch (WebException webEx)
            {
                if (webEx.Response != null)
                {
                    using (HttpWebResponse exResponse = (HttpWebResponse)webEx.Response)
                    {
                        using (StreamReader sr = new StreamReader(exResponse.GetResponseStream()))
                        {
                            response = sr.ReadToEnd();
                        }
                    }
                }
            }
            return response;
        }

        /// <summary>Send the Message to Merchant</summary>
        public static string GetStringHttpResponse(string url)
        {
            string response = null;
            try
            {
                HttpWebRequest myRequest = (HttpWebRequest) WebRequest.Create(url);
                myRequest.Method = "GET";
                //myRequest.ContentLength = data.Length;
                myRequest.CookieContainer = new CookieContainer();
                //myRequest.ContentType = "application/x-www-form-urlencoded";
                myRequest.ContentType = "application/json; encoding='utf-8'";
                //myRequest.ContentType = "application/x-www-form-urlencoded";
                myRequest.KeepAlive = false;
                using (HttpWebResponse myResponse = (HttpWebResponse)myRequest.GetResponse())
                {
                    using (var reader = new StreamReader(myResponse.GetResponseStream()))
                    {
                        if (reader != null)
                        {
                            response = reader.ReadToEnd();
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return response;
        }
    }
}
