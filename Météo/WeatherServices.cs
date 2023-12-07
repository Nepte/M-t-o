using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using static Weather.Services.WeatherService;

namespace Weather.Services
{
    public class WeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public WeatherService()
        {
            _httpClient = new HttpClient();
            _apiKey = "151ae2856dc904c1294c5278645d76fb"; // Remplacez par votre clé API personnelle
        }

        public async Task<WeatherData> GetWeatherDataAsync(string city)
        {
            var url = $"http://api.openweathermap.org/data/2.5/weather?q={city}&appid={_apiKey}&units=metric&lang=fr";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Cannot retrieve weather data.");
            }
            var content = await response.Content.ReadAsStringAsync();
            var weatherData = JsonConvert.DeserializeObject<WeatherData>(content);
            return weatherData;
        }


        public async Task<ForecastData> GetForecastDataAsync(string city)
        {
            var forecastUrl = $"http://api.openweathermap.org/data/2.5/forecast?q={city}&appid={_apiKey}&units=metric&lang=fr";
            var response = await _httpClient.GetAsync(forecastUrl);
            var content = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<ForecastData>(content);
        }

        public async Task<AirQualityData> GetAirQualityAsync(double latitude, double longitude)
        {
            var airQualityUrl = $"http://api.openweathermap.org/data/2.5/air_pollution?lat={latitude}&lon={longitude}&appid={_apiKey}";
            var response = await _httpClient.GetAsync(airQualityUrl);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Cannot retrieve air quality data.");
            }

            return JsonConvert.DeserializeObject<AirQualityData>(content);
        }



        public async Task<UvIndexData> GetUvIndexAsync(string city)
        {
            // Récupérez d'abord les données météorologiques pour obtenir les coordonnées
            var weatherData = await GetWeatherDataAsync(city);
            double latitude = weatherData.Coord.Lat;
            double longitude = weatherData.Coord.Lon;

            // Maintenant, utilisez ces coordonnées pour obtenir l'indice UV
            var uvUrl = $"http://api.openweathermap.org/data/2.5/uvi?appid={_apiKey}&lat={latitude}&lon={longitude}";
            var uvResponse = await _httpClient.GetAsync(uvUrl);
            if (uvResponse.IsSuccessStatusCode)
            {
                var uvContent = await uvResponse.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<UvIndexData>(uvContent);
            }
            else
            {
                // Gérer l'erreur ou retourner une valeur par défaut
                return new UvIndexData { Value = 0 };
            }
        }



        public class WeatherData
        {
            public MainData Main { get; set; }
            public string Name { get; set; }
            public List<WeatherDescription> Weather { get; set; }
            // Ajoutez ici les propriétés pour la latitude et la longitude si elles sont disponibles dans la réponse de l'API
            public Coord Coord { get; set; }
            // Autres propriétés...
            public Wind Wind { get; set; } // Assurez-vous que cette propriété est présente et correspond à la réponse de l'API
            public Sys Sys { get; set; }
            public Rain Rain { get; set; }

        }

        public class Rain
        {
            // Cette propriété correspond à la clé "1h" ou "3h" dans la réponse JSON qui donne la quantité de pluie pour la dernière heure ou les 3 dernières heures.
            [JsonProperty("1h")]
            public double Last1h { get; set; }
        }

        public class Sys
        {
            [JsonProperty("sunrise")]
            public long Sunrise { get; set; }

            [JsonProperty("sunset")]
            public long Sunset { get; set; }
            // ...
        }


        public class MainData
        {
            public double Temp { get; set; }
            public double Temp_min { get; set; }
            public double Temp_max { get; set; }
            public int Humidity { get; set; }
            public int Visibility { get; set; }
            public double WindSpeed { get; set; }



        }

        public class Wind
        {
            [JsonProperty("speed")] // Utilisez l'attribut JsonProperty pour mapper le nom de champ JSON au membre de la classe
            public double Speed { get; set; } // Assurez-vous que cette propriété est nommée comme le champ dans la réponse JSON
                                              // Vous pouvez également ajouter d'autres propriétés comme 'direction' si elles sont fournies par l'API
        }

        public class ForecastData
        {
            public List<ForecastTimeSlot> List { get; set; }
            // Autres propriétés...
        }

        public class ForecastTimeSlot
        {
            public MainData Main { get; set; }
            public List<WeatherDescription> Weather { get; set; }
            public string Dt_txt { get; set; }
            // Autres propriétés...
        }

        public class WeatherDescription
        {
            public string Main { get; set; }
            public string Description { get; set; }
            // Autres propriétés...
        }

        public class AirQualityData
        {
            public List<AirQualityIndex> List { get; set; }
        }

        public class AirQualityIndex
        {
            public MainPollutionData Main { get; set; }
        }

        public class MainPollutionData
        {
            public int Aqi { get; set; }
        }


        public class UvIndexData
        {
            public double Value { get; set; }
        }

        // La classe pour les coordonnées géographiques
        public class Coord
        {
            [JsonProperty("lat")]
            public double Lat { get; set; }

            [JsonProperty("lon")]
            public double Lon { get; set; }
        }
    }
}
