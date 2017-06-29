﻿using System;
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
        [JsonProperty("parameters")]
        public TelegramParameters parameters{get;set;}
    }
}
