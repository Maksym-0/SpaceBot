using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SpaceBot;
using SpaceBot.Models;
namespace SpaceBot.Clients
{
    internal class SpaceClient
    {
        private HttpClient _httpClient;
        private static string _apiadress;
        private static string _apihost;
        public SpaceClient()
        {
            _httpClient = new HttpClient();
            _apiadress = Constants.ApiAdress;
            _apihost = Constants.ApiHost;
        }
        public async Task<SpacePhoto> GetPhoto()
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"{_apiadress}{_apihost}/GetPhoto");
            string json = await response.Content.ReadAsStringAsync();
            SpacePhoto photo = JsonConvert.DeserializeObject<SpacePhoto>(json);

            return photo;
        }
        public async Task<SpacePhoto> GetRandomPhoto()
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"{_apiadress}{_apihost}/GetRandomPhoto");
            string json = await response.Content.ReadAsStringAsync();
            SpacePhoto photo = JsonConvert.DeserializeObject<SpacePhoto>(json);

            return photo;
        }
        public async Task<SpacePhoto[]> GetDatabasePhoto()
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"{_apiadress}{_apihost}/GetDatabasePhoto");
            string json = await response.Content.ReadAsStringAsync();
            SpacePhoto[] photos = JsonConvert.DeserializeObject<SpacePhoto[]>(json);

            return photos;
        }
        public async Task PostDatabasePhoto(string date)
        {
            HttpResponseMessage response = await _httpClient.PostAsync($"{_apiadress}{_apihost}/PostPhotoByDate?date={date}", null);
            string json = await response.Content.ReadAsStringAsync();
            SpacePhoto[] photos = JsonConvert.DeserializeObject<SpacePhoto[]>(json);
            return;
        }
        public async Task PutDatabasePhoto(string title, string explanation, string date)
        {
            HttpResponseMessage response = await _httpClient.PutAsync($"{_apiadress}{_apihost}/PutPhotoByDate?title={title}&explanation={explanation}&date={date}", null);
            string json = await response.Content.ReadAsStringAsync();
            SpacePhoto[] photos = JsonConvert.DeserializeObject<SpacePhoto[]>(json);
            return;
        }
        public async Task DeleteDatabasePhoto(string date)
        {
            HttpResponseMessage response = await _httpClient.DeleteAsync($"{_apiadress}{_apihost}/DeletePhotoByDate?date={date}");
            string json = await response.Content.ReadAsStringAsync();
            SpacePhoto[] photos = JsonConvert.DeserializeObject<SpacePhoto[]>(json);
            return;
        }
    }
}
