using Newtonsoft.Json;
using System;
using System.Net;

namespace CTrader
{
    public sealed class SlackClient
    {
        public static readonly string URL = string.Empty;

        private readonly Uri _webHookUri;

        public SlackClient(string url)
        {
            this._webHookUri = new Uri(url);
        }

        public void SendSlackMessage(SlackMessage message)
        {
            using (WebClient webClient = new WebClient())
            {
                webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                byte[] request = System.Text.Encoding.UTF8.GetBytes("payload=" + JsonConvert.SerializeObject(message));
                byte[] response = webClient.UploadData(this._webHookUri, "POST", request);
            }
        }

        public sealed class SlackMessage
        {
            [JsonProperty("channel")]
            public string Channel { get { return "#correlation"; } }

            [JsonProperty("username")]
            public string UserName { get { return "CTrader"; } }

            [JsonProperty("text")]
            public string Text { get; set; }

            [JsonProperty("icon_emoji")]
            public string Icon
            {
                get { return ":computer:"; }
            }
        }
    }
}
