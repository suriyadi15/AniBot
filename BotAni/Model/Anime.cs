using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BotAni.Model
{
    class Anime
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
