using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BotAni.Model
{
    class Episode
    {
        [JsonProperty("no")]
        public string No { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("releaseDate")]
        public string ReleaseDate { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
