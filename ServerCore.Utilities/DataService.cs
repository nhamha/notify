using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ServerCore.Utilities.Interfaces;
using ServerCore.Utilities.Utils;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore.Utilities
{
    public class DataService : IDataService
    {
        private readonly HttpClient _httpClient;
		private readonly IHttpContextAccessor _httpContextAccessor;
		public DataService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
            _httpContextAccessor = httpContextAccessor;
        }
        /// <summary>
        /// PostAsync with Partner-Key in config
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uri"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<T> PostAsync<T>(string uri, dynamic data, bool isLog = true, int timeout = 30000)
        {
            try
            {
                using (var cts = new CancellationTokenSource())
                {
                    cts.CancelAfter(timeout);
                    string logMessage = string.Format("Đầu vào {0}: {1}", uri, JsonConvert.SerializeObject(data));
                    NLogManager.Info(logMessage);
                    var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                    var result = await _httpClient.PostAsync(uri, content, cts.Token).ConfigureAwait(false);
                    string resultContent = await result.Content.ReadAsStringAsync();
                    if (string.IsNullOrWhiteSpace(resultContent)) return default(T);
                    if (isLog)
                        NLogManager.Info("Đầu ra: " + resultContent);
                    return JsonConvert.DeserializeObject<T>(resultContent);
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

        public async Task<T> PostAsync<T>(string uri, dynamic data, IDictionary<string, string> dictionary, bool isLog = true, int timeout = 30000)
        {
            try
            {
                using (var cts = new CancellationTokenSource())
                {
                    cts.CancelAfter(timeout);

                    string logMessage = string.Format("Đầu vào {0}: {1}", uri, JsonConvert.SerializeObject(data));
                    NLogManager.Info(logMessage);
                    var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                    _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
                    if (dictionary.Count > 0)
                    {
                        foreach (var dic in dictionary)
                        {
                            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation(dic.Key, dic.Value);
                        }
                    }
                    var result = await _httpClient.PostAsync(uri, content, cts.Token).ConfigureAwait(false);
                    string resultContent = await result.Content.ReadAsStringAsync();
                    if (string.IsNullOrWhiteSpace(resultContent)) return default(T);
                    if (isLog)
                        NLogManager.Info("Đầu ra: " + resultContent);
                    return JsonConvert.DeserializeObject<T>(resultContent);

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

        public async Task<string> PostAsync(string uri, dynamic data, bool isLog = true, int timeout = 30000)
        {
            try
            {
                using (var cts = new CancellationTokenSource())
                {
                    cts.CancelAfter(timeout);

                    string logMessage = string.Format("Đầu vào {0}: {1}", uri, JsonConvert.SerializeObject(data));
                    NLogManager.Info(logMessage);
                    var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                    _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
                    var result = await _httpClient.PostAsync(uri, content, cts.Token).ConfigureAwait(false);
                    string resultContent = await result.Content.ReadAsStringAsync();
                    if (isLog)
                        NLogManager.Info("Đầu ra: " + resultContent);
                    return resultContent;
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

        public async Task<string> PostAsync(string uri, dynamic data, IDictionary<string, string> dictionary, bool isLog = true, int timeout = 30000)
        {
            try
            {
                using (var cts = new CancellationTokenSource())
                {
                    cts.CancelAfter(timeout);

                    {
                        string logMessage = string.Format("Đầu vào {0}: {1}", uri, JsonConvert.SerializeObject(data));
                        NLogManager.Info(logMessage);
                        var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
                        if (dictionary.Count > 0)
                        {
                            foreach (var dic in dictionary)
                            {
                                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation(dic.Key, dic.Value);
                            }
                        }
                        var result = await _httpClient.PostAsync(uri, content, cts.Token).ConfigureAwait(false);
                        string resultContent = await result.Content.ReadAsStringAsync();
                        if (isLog)
                            NLogManager.Info("Đầu ra: " + resultContent);
                        return resultContent;
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
        ///  GetAsync with Partner-Key in config
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uri"></param>
        /// <returns></returns>
        public async Task<T> GetAsync<T>(string uri, bool isLog = true, int timeout = 30000)
        {
            try
            {
                using (var cts = new CancellationTokenSource())
                {
                    cts.CancelAfter(timeout);
                    using (var httpClient = new HttpClient())
                    {
                        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
                        var response = await httpClient.GetAsync(uri, cts.Token).ConfigureAwait(false);
                        var content = await response.Content.ReadAsStringAsync();
                        if (string.IsNullOrWhiteSpace(content)) throw new Exception();
                        if (isLog)
                            NLogManager.Info(content);
                        return JsonConvert.DeserializeObject<T>(content);
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

        public async Task<string> GetAsync(string uri, bool isLog = true, int timeout = 30000)
        {
            try
            {
                using (var cts = new CancellationTokenSource())
                {
                    cts.CancelAfter(timeout);

                    _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
                    var response = await _httpClient.GetAsync(uri, cts.Token).ConfigureAwait(false);
                    var content = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrWhiteSpace(content)) throw new Exception();
                    if (isLog)
                        NLogManager.Info(content);
                    return content;

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
		public async Task<string> GetApiAsync(string uri, dynamic token, bool isLog = true,bool isAuthen=false,int timeout = 30000)
		{
			try
			{
				using (var cts = new CancellationTokenSource())
				{
					cts.CancelAfter(timeout);

					_httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
                    if (isAuthen)
                        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.ToString());
                    var response = await _httpClient.GetAsync(uri, cts.Token).ConfigureAwait(false);
					var content = await response.Content.ReadAsStringAsync();
					if (string.IsNullOrWhiteSpace(content)) throw new Exception();
					if (isLog)
						NLogManager.Info(content);
					return content;

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
		//public T GetHTML<T>(string URL, bool isLog = true)
		//{
		//    string connectionString = URL;
		//    string pageContent = string.Empty;
		//    try
		//    {
		//        var myRequest = (HttpWebRequest)WebRequest.Create(connectionString);
		//        myRequest.Credentials = CredentialCache.DefaultCredentials;
		//        //// Get the response
		//        using (var respStream = myRequest.())
		//        {
		//            if (respStream != null)
		//            {
		//                using (var ioStream = new StreamReader(respStream))
		//                {
		//                    pageContent = ioStream.ReadToEnd();
		//                    if (string.IsNullOrWhiteSpace(pageContent)) return default(T);
		//                    if (isLog)
		//                        NLogManager.Info("Đầu ra: " + pageContent);
		//                    return JsonConvert.DeserializeObject<T>(pageContent);
		//                }
		//            }
		//        }
		//    }
		//    catch (Exception ex)
		//    {
		//        NLogManager.Exception(ex);
		//    }
		//    return default(T);
		//}
		//public string GetHTML(string URL, bool isLog = true)
		//{
		//    string connectionString = URL;
		//    string pageContent = string.Empty;
		//    try
		//    {
		//        var myRequest = (HttpWebRequest)WebRequest.Create(connectionString);
		//        myRequest.Credentials = CredentialCache.DefaultCredentials;
		//        //// Get the response
		//        var httpWebResponse = (HttpWebResponse)myRequest.GetResponseAsync();
		//        WebResponse webResponse = myRequest.GetResponse();
		//        using (var respStream = webResponse.GetResponseStream())
		//        {
		//            if (respStream != null)
		//            {
		//                using (var ioStream = new StreamReader(respStream))
		//                {
		//                    pageContent = ioStream.ReadToEnd();
		//                    if (isLog)
		//                        NLogManager.Info("Đầu ra: " + pageContent);
		//                    return pageContent;
		//                }
		//            }
		//        }
		//    }
		//    catch (Exception ex)
		//    {
		//        NLogManager.Exception(ex);
		//    }
		//    return string.Empty;
		//}

		//public T PostHTML<T>(string uri, dynamic postData, bool isLog = true)
		//{
		//    try
		//    {
		//        var request = (HttpWebRequest)WebRequest.Create(uri);
		//        var partnerCode = ConfigurationManager.AppSettings["Partner-Key"].ToString();
		//        string data = string.Empty;
		//        if (postData != null)
		//            data = JsonConvert.SerializeObject(postData);
		//        request.Method = "POST";
		//        request.ContentType = "application/json";
		//        request.Headers.Add("Partner-Key", partnerCode);
		//        if (!string.IsNullOrWhiteSpace(data))
		//            using (var stream = new StreamWriter(request.GetRequestStream()))
		//            {
		//                stream.Write(data);
		//            }
		//        using (var response = (HttpWebResponse)request.GetResponse())
		//        {
		//            using (var responseStream = response.GetResponseStream())
		//            {
		//                var responseString = new StreamReader(responseStream).ReadToEnd();
		//                if (string.IsNullOrWhiteSpace(responseString)) return default(T);
		//                if (isLog)
		//                    NLogManager.Info("Đầu ra: " + responseString);
		//                return JsonConvert.DeserializeObject<T>(responseString);
		//            }
		//        }
		//    }
		//    catch (Exception ex)
		//    {
		//        NLogManager.Exception(ex);
		//        return default(T);
		//    }
		//}

		//public string PostHTML(string uri, dynamic postData, bool isLog = true)
		//{
		//    try
		//    {
		//        var request = (HttpWebRequest)WebRequest.Create(uri);
		//        var partnerCode = ConfigurationManager.AppSettings["Partner-Key"].ToString();
		//        string data = string.Empty;
		//        if (postData != null)
		//            data = JsonConvert.SerializeObject(postData);
		//        request.Method = "POST";
		//        request.ContentType = "application/json";
		//        request.Headers.Add("Partner-Key", partnerCode);
		//        if (!string.IsNullOrWhiteSpace(data))
		//            using (var stream = new StreamWriter(request.GetRequestStream()))
		//            {
		//                stream.Write(data);
		//            }
		//        using (var response = (HttpWebResponse)request.GetResponse())
		//        {
		//            using (var responseStream = response.GetResponseStream())
		//            {
		//                var responseString = new StreamReader(responseStream).ReadToEnd();
		//                if (isLog)
		//                    NLogManager.Info("Đầu ra: " + responseString);
		//                return responseString;
		//            }
		//        }
		//    }
		//    catch (Exception ex)
		//    {
		//        NLogManager.Exception(ex);

		//    }
		//    return string.Empty;
		//}
	}
}
