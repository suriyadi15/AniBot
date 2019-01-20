using BotAni.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;



namespace BotAni
{
    class AniService
    {
        private AniService() { }

        static HttpClient client = new HttpClient();
        private const string HOST = "https://nontonanime.herokuapp.com/";

        public static async Task<List<Anime>> GetAnimes()
        {
            List<Anime> allData = new List<Anime>();
            HttpResponseMessage response = await client.GetAsync($"{HOST}list");
            if (response.IsSuccessStatusCode)
            {
                allData = JsonConvert.DeserializeObject<List<Anime>>(await response.Content.ReadAsStringAsync());
            }
            return allData;
        }
        public static async Task<DataAnime> GetAnime(string name)
        {
            HttpResponseMessage response = await client.GetAsync($"{HOST}get/{name}");
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<DataAnime>(await response.Content.ReadAsStringAsync());
            }
            return null;
        }
        public static async Task<DataEpisode> GetEpisode(string name)
        {
            HttpResponseMessage response = await client.GetAsync($"{HOST}get/eps/{name}");
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<DataEpisode>(await response.Content.ReadAsStringAsync());
            }
            return null;
        }
    }
}
