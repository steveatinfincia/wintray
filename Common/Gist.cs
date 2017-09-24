using System;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Common.Github {
    public class Gist {
        [JsonProperty("html_url")]
        public string Location { get; set; }

        public static void Create(string log, string title, Action<Gist> success, Action<string> failure) {
            Task.Factory.StartNew(() => {
                try {

                    var fileName = $"FreenetTray - {title}.txt";

                    var file = new JObject(
                        new JProperty("content", log)
                    );

                    JArray files = new JArray {
                        new JProperty(fileName, file)
                    };

                    JObject payLoad = new JObject(
                        new JProperty("files", files),
                        new JProperty("description", title),
                        new JProperty("public", true)
                    );

                    var headers = new WebHeaderCollection() {
                        ["Content-Type"] = "application/vnd.github.v3+json",
                        ["Accept"] = "application/json",
                        ["User-Agent"] = "FreenetTray"
                    };

                    var httpWebRequest = (HttpWebRequest)WebRequest.Create($"https://{Constants.FNGithubAPI}/gists");
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Method = "POST";
                    httpWebRequest.Headers = headers;

                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                    using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream())) {

                        string json = payLoad.ToString();

                        streamWriter.Write(json);
                        streamWriter.Flush();
                        streamWriter.Close();
                    }

                    var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream())) {
                        var result = streamReader.ReadToEnd();

                        Gist response = JsonConvert.DeserializeObject<Gist>(result);

                        success(response);
                    }
                } catch (Exception ex) {
                    failure(ex.Message);
                }
            });
        }
    }
}