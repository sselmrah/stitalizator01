using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace stitalizator01.Models
{
    public class TelegramParameters
    {
        [JsonProperty("text")]
        public string text { get; set; }
        [JsonProperty("parse_mode")]
        public string parse_mode{ get; set; }

        [JsonProperty("reply_markup")]
        public string reply_markup { get; set; }
    }
}