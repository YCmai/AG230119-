using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TaskSim
{
    class HttpClientHelper
    {
        public static  TResult Post<TResult>(string url, object ParaObject)
        {
            #region 有点问题，引起网络断开，原因不明
            //HttpClient client = new HttpClient();
            //var content = new StringContent(JsonConvert.SerializeObject(ParaObject), Encoding.UTF8, "application/json");
            //try
            //{
            //    var response =  client.PostAsync(url, content).Result;
            //    response.EnsureSuccessStatusCode();
            //    var str =  response.Content.ReadAsStringAsync().Result;
            //    return JsonConvert.DeserializeObject<TResult>(str);
            //}
            //catch (Exception ex)
            //{
            //    System.Diagnostics.Debug.WriteLine(ex.ToString());
            //}

            //return default(TResult);
            #endregion

            try
            {
            byte[] buffer = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(ParaObject));
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = buffer.Length;
            var postData = request.GetRequestStream();
            postData.Write(buffer, 0, buffer.Length);
            postData.Close();
            HttpWebResponse response;
      
                response = (HttpWebResponse)request.GetResponse();
                var responseStream = response.GetResponseStream();
                var reader = new StreamReader(responseStream);
                var result = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<TResult>(result);
            }
            catch (Exception ex)
            {
                throw;
            }

        }



       
    }
}
