using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace stitalizator01.Models
{
    public class TelegramChannelData
    {
        [JsonProperty("method")]
        public string method { get; set; }
        [JsonProperty("text")]
        public string text { get; set; }
        [JsonProperty("keyboard")]
        public string keyboard { get; set; }
    }
}
