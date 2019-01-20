using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BotAni.Model
{
    class DataEpisode
    {
        [JsonProperty("title")]
        public string Title { get; set; }


        [JsonProperty("video")]
        public List<string> Video { get; set; }

    }
}
