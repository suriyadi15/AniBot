using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BotAni.Model
{
    class DataAnime
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("releaseDate")]
        public string ReleaseDate { get; set; }

        [JsonProperty("producer")]
        public string Producer { get; set; }

        [JsonProperty("episode")]
        public List<Episode> Episode { get; set; }
    }
}
