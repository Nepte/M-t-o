using System;
using System.Windows;
using System.Windows.Input;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using Weather.Services;
using System.Linq;
using System.Windows.Controls;
using System.Globalization;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using static Weather.Services.WeatherService;

namespace Weather
{
    public partial class MainWindow : Window
    {
        private readonly WeatherService _weatherService;
        private CityManager _cityManager;

        public MainWindow()
        {
            InitializeComponent();
            _weatherService = new WeatherService();
            LoadCitiesToComboBox();
            _cityManager = new CityManager("C:\\Users\\Erzen\\Desktop\\Météo\\villes\\cities.txt");


            // Chargez les villes et définissez-les comme source pour la ComboBox
            var cities = _cityManager.LoadCities();
            CitiesComboBox.ItemsSource = cities;

            UpdateWeatherInfo("Annecy");
            UpdateForecast("Annecy");
            UpdateUvIndex("Annecy");
            double latitude = 45.899247; // Latitude pour Annecy
            double longitude = 6.129384; // Longitude pour Annecy
            UpdateAirQuality(latitude, longitude);
        }

        private async void UpdateWeatherInfo(string city)
        {
            try
            {
                var weatherData = await _weatherService.GetWeatherDataAsync(city);

                // Mise à jour de la température et de la ville
                TemperatureTextBlock.Text = $"{(int)Math.Round(weatherData.Main.Temp)}°C";
                CityTextBlock.Text = weatherData.Name;

                // Mise à jour du jour et de l'heure actuels
                WeatherConditionTextBlock.Text = $"{DateTime.Now:dddd, HH:mm}";

                // Mise à jour de l'état de la météo
                var textInfo = new CultureInfo("fr-FR", false).TextInfo;
                WeatherDescriptionTextBlock.Text = weatherData.Weather[0].Description;
                WeatherDescriptionTextBlock.Text = char.ToUpper(WeatherDescriptionTextBlock.Text[0]) + WeatherDescriptionTextBlock.Text.Substring(1);

                // Mise à jour de l'humidité
                int humidity = weatherData.Main.Humidity;
                HumidityTextBlock.Text = $"{weatherData.Main.Humidity}";
                HumiditySlider.Value = humidity;

                // Mise à jour de la visibilité
                int visibilityKm = weatherData.Main.Visibility / 1000; // La visibilité est généralement retournée en mètres, donc la convertir en kilomètres
                VisibilityTextBlock.Text = $"{visibilityKm}";
                VisibilitySlider.Value = visibilityKm; // Assurez-vous que le maximum du Slider est configuré correctement pour représenter la visibilité maximale possible.

                // Mise à jour de l'état du vent
                WindSpeedTextBlock.Text = $"{weatherData.Wind.Speed:F1}"; // Utilisez "F1" pour afficher un chiffre après la virgule

                TimeZoneInfo franceTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");

                // Convertissez les timestamps de lever et coucher du soleil de Unix en DateTime
                DateTime sunriseUtc = DateTimeOffset.FromUnixTimeSeconds(weatherData.Sys.Sunrise).UtcDateTime;
                DateTime sunsetUtc = DateTimeOffset.FromUnixTimeSeconds(weatherData.Sys.Sunset).UtcDateTime;

                // Convertissez les heures UTC en heure locale française
                DateTime sunriseLocal = TimeZoneInfo.ConvertTimeFromUtc(sunriseUtc, franceTimeZone);
                DateTime sunsetLocal = TimeZoneInfo.ConvertTimeFromUtc(sunsetUtc, franceTimeZone);

                // Mise à jour des TextBlocks pour le lever et le coucher du soleil
                SunriseTextBlock.Text = sunriseLocal.ToString("HH:mm"); // Format 24 heures
                SunsetTextBlock.Text = sunsetLocal.ToString("HH:mm"); // Format 24 heures

                if (weatherData.Rain != null)
                {
                    double rainVolume = weatherData.Rain.Last1h; // Vous devez vérifier le nom exact de la propriété dans la réponse JSON
                    RainPercentageTextBlock.Text = $"Pluie - {rainVolume}%";
                }
                else
                {
                    RainPercentageTextBlock.Text = "Pluie - 0%";
                }

                // Mise à jour de l'icône météo
                foreach (var weather in weatherData.Weather)
                {
                    UpdateWeatherIcon(weather, WeatherIcon); // WeatherIcon serait votre contrôle Image dans XAML

                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la récupération des données météorologiques : {ex.Message}", "Erreur");
            }
        }


        private async void UpdateAirQuality(double latitude, double longitude)
        {
            try
            {
                var airQualityData = await _weatherService.GetAirQualityAsync(latitude, longitude);
                int aqi = airQualityData.List[0].Main.Aqi;

                // Mettre à jour l'interface utilisateur
                Dispatcher.Invoke(() =>
                {
                    AirQualityTextBlock.Text = $"{aqi}";
                    // Vous pourriez vouloir ajuster la valeur du Slider pour correspondre à l'échelle AQI
                    AirQualitySlider.Value = aqi;
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la récupération de la qualité de l'air : {ex.Message}", "Erreur");
            }
        }


        // La méthode UpdateForecast pourrait être utilisée pour mettre à jour un autre élément de l'interface utilisateur qui ne dépend pas de CardDay

        //private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.Key == Key.Enter)
        //    {
        //        UpdateWeatherInfo(txtSearch.Text);
        //    }
        //}

        private void ConvertToFahrenheit(object sender, RoutedEventArgs e)
        {
            // Conversion de la température actuelle
            double tempCelsius = double.Parse(TemperatureTextBlock.Text.Replace("°C", ""));
            double tempFahrenheit = tempCelsius * 9 / 5 + 32;
            TemperatureTextBlock.Text = $"{(int)Math.Round(tempFahrenheit)}°F";

            // Conversion des prévisions
            ConvertForecastToFahrenheit();
        }

        private void ConvertForecastToFahrenheit()
        {
            ConvertSingleDayForecastToFahrenheit(MaxTemperatureSundayTextBlock);
            ConvertSingleDayForecastToFahrenheit(MinTemperatureSundayTextBlock);

            ConvertSingleDayForecastToFahrenheit(MaxTemperatureMondayTextBlock);
            ConvertSingleDayForecastToFahrenheit(MinTemperatureMondayTextBlock);

            ConvertSingleDayForecastToFahrenheit(MaxTemperatureTuesdayTextBlock);
            ConvertSingleDayForecastToFahrenheit(MinTemperatureTuesdayTextBlock);

            ConvertSingleDayForecastToFahrenheit(MaxTemperatureWednesdayTextBlock);
            ConvertSingleDayForecastToFahrenheit(MinTemperatureWednesdayTextBlock);

            ConvertSingleDayForecastToFahrenheit(MaxTemperatureThursdayTextBlock);
            ConvertSingleDayForecastToFahrenheit(MinTemperatureThursdayTextBlock);

            ConvertSingleDayForecastToFahrenheit(MaxTemperatureFridayTextBlock);
            ConvertSingleDayForecastToFahrenheit(MinTemperatureFridayTextBlock);

            ConvertSingleDayForecastToFahrenheit(MaxTemperatureSaturdayTextBlock);
            ConvertSingleDayForecastToFahrenheit(MinTemperatureSaturdayTextBlock);
        }

        private void ConvertToCelsius(object sender, RoutedEventArgs e)
        {
            // Vérifiez si la température est déjà en Celsius pour éviter une conversion inutile
            if (!TemperatureTextBlock.Text.EndsWith("°C"))
            {
                // Conversion de la température actuelle
                double tempFahrenheit = double.Parse(TemperatureTextBlock.Text.Replace("°F", ""));
                double tempCelsius = (tempFahrenheit - 32) * 5 / 9;
                TemperatureTextBlock.Text = $"{(int)Math.Round(tempCelsius)}°C";

                // Conversion des prévisions
                ConvertForecastToCelsius();
            }
        }

        private void ConvertForecastToCelsius()
        {
            // Conversion des températures des prévisions pour chaque jour de la semaine
            ConvertSingleDayForecastToCelsius(MaxTemperatureSundayTextBlock);
            ConvertSingleDayForecastToCelsius(MinTemperatureSundayTextBlock);

            ConvertSingleDayForecastToCelsius(MaxTemperatureMondayTextBlock);
            ConvertSingleDayForecastToCelsius(MinTemperatureMondayTextBlock);

            ConvertSingleDayForecastToCelsius(MaxTemperatureTuesdayTextBlock);
            ConvertSingleDayForecastToCelsius(MinTemperatureTuesdayTextBlock);

            ConvertSingleDayForecastToCelsius(MaxTemperatureWednesdayTextBlock);
            ConvertSingleDayForecastToCelsius(MinTemperatureWednesdayTextBlock);

            ConvertSingleDayForecastToCelsius(MaxTemperatureThursdayTextBlock);
            ConvertSingleDayForecastToCelsius(MinTemperatureThursdayTextBlock);

            ConvertSingleDayForecastToCelsius(MaxTemperatureFridayTextBlock);
            ConvertSingleDayForecastToCelsius(MinTemperatureFridayTextBlock);

            ConvertSingleDayForecastToCelsius(MaxTemperatureSaturdayTextBlock);
            ConvertSingleDayForecastToCelsius(MinTemperatureSaturdayTextBlock);
        }

        private void ConvertSingleDayForecastToCelsius(TextBlock temperatureTextBlock)
        {
            if (temperatureTextBlock.Text.EndsWith("°")) // Vérifie si le TextBlock a une température
            {
                double tempFahrenheit = double.Parse(temperatureTextBlock.Text.Replace("°", ""));
                double tempCelsius = (tempFahrenheit - 32) * 5 / 9;
                temperatureTextBlock.Text = $"{(int)Math.Round(tempCelsius)}°";
            }
        }


        private void ConvertSingleDayForecastToFahrenheit(TextBlock temperatureTextBlock)
        {
            double tempCelsius = double.Parse(temperatureTextBlock.Text.Replace("°", ""));
            double tempFahrenheit = tempCelsius * 9 / 5 + 32;
            temperatureTextBlock.Text = $"{(int)Math.Round(tempFahrenheit)}°";
        }

        private void ConvertToCelsiusStyle(object sender, RoutedEventArgs e)
        {
            // ... Code pour convertir en Celsius ...

            // Mise à jour du style du bouton pour indiquer qu'il est actif
            CelsiusButton.Background = new SolidColorBrush(Color.FromRgb(26, 26, 26)); // Arrière-plan actif
            CelsiusButton.Foreground = new SolidColorBrush(Colors.White); // Texte actif

            // Mise à jour du style du bouton pour indiquer qu'il est inactif
            FahrenheitButton.Background = new SolidColorBrush(Colors.Transparent); // Arrière-plan inactif
            FahrenheitButton.Foreground = new SolidColorBrush(Color.FromRgb(26, 26, 26)); // Texte inactif
        }

        private void ConvertToFahrenheitStyle(object sender, RoutedEventArgs e)
        {
            // ... Code pour convertir en Fahrenheit ...

            // Mise à jour du style du bouton pour indiquer qu'il est actif
            FahrenheitButton.Background = new SolidColorBrush(Color.FromRgb(26, 26, 26)); // Arrière-plan actif
            FahrenheitButton.Foreground = new SolidColorBrush(Colors.White); // Texte actif

            // Mise à jour du style du bouton pour indiquer qu'il est inactif
            CelsiusButton.Background = new SolidColorBrush(Colors.Transparent); // Arrière-plan inactif
            CelsiusButton.Foreground = new SolidColorBrush(Color.FromRgb(26, 26, 26)); // Texte inactif
        }

        private async void UpdateUvIndex(string city)
        {
            try
            {
                var uvIndexData = await _weatherService.GetUvIndexAsync(city);
                double uvIndexValue = uvIndexData.Value; // Voici la ligne correcte qui récupère la valeur double.
                UvIndexSlider1.Value = uvIndexValue; // Utilisez uvIndexValue qui est un double ici.
                UvIndexSlider2.Value = uvIndexValue;
                UvIndexSlider3.Value = uvIndexValue;

                // Formattez la chaîne pour l'affichage
                UvIndexAverageTextBlock.Text = $"Moyenne de {uvIndexValue:F1}"; // F1 pour un chiffre après la virgule
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la récupération de l'indice UV : {ex.Message}", "Erreur");
            }
        }


        private void UvIndexSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Votre code ici pour gérer l'événement.
        }


        private async Task UpdateForecast(string city)
        {
            try
            {
                var forecastData = await _weatherService.GetForecastDataAsync(city);
                UpdateDailyWeatherIcons(forecastData); // Ici, forecastData est reconnu car il vient d'être déclaré

                var dailyForecasts = forecastData.List
                    .GroupBy(f => DateTime.Parse(f.Dt_txt).Date)
                    .Select(group => new
                    {
                        Date = group.Key,
                        MinTemp = group.Min(g => g.Main.Temp_min),
                        MaxTemp = group.Max(g => g.Main.Temp_max)
                    }).ToList();

                foreach (var forecast in dailyForecasts)
                {
                    switch (forecast.Date.DayOfWeek)
                    {
                        case DayOfWeek.Sunday:
                            MaxTemperatureSundayTextBlock.Text = $"{(int)forecast.MaxTemp}°";
                            MinTemperatureSundayTextBlock.Text = $"{(int)forecast.MinTemp}°";
                            break;
                        case DayOfWeek.Monday:
                            MaxTemperatureMondayTextBlock.Text = $"{(int)forecast.MaxTemp}°";
                            MinTemperatureMondayTextBlock.Text = $"{(int)forecast.MinTemp}°";
                            break;
                        // Ajoutez ici les cas pour les autres jours de la semaine
                        case DayOfWeek.Tuesday:
                            MaxTemperatureTuesdayTextBlock.Text = $"{(int)forecast.MaxTemp}°";
                            MinTemperatureTuesdayTextBlock.Text = $"{(int)forecast.MinTemp}°";
                            break;
                        case DayOfWeek.Wednesday:
                            MaxTemperatureWednesdayTextBlock.Text = $"{(int)forecast.MaxTemp}°";
                            MinTemperatureWednesdayTextBlock.Text = $"{(int)forecast.MinTemp}°";
                            break;
                        case DayOfWeek.Thursday:
                            MaxTemperatureThursdayTextBlock.Text = $"{(int)forecast.MaxTemp}°";
                            MinTemperatureThursdayTextBlock.Text = $"{(int)forecast.MinTemp}°";
                            break;
                        case DayOfWeek.Friday:
                            MaxTemperatureFridayTextBlock.Text = $"{(int)forecast.MaxTemp}°";
                            MinTemperatureFridayTextBlock.Text = $"{(int)forecast.MinTemp}°";
                            break;
                        case DayOfWeek.Saturday:
                            MaxTemperatureSaturdayTextBlock.Text = $"{(int)forecast.MaxTemp}°";
                            MinTemperatureSaturdayTextBlock.Text = $"{(int)forecast.MinTemp}°";
                            break;
                            // Assurez-vous d'ajouter des cas pour tous les jours de la semaine
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la récupération des prévisions : {ex.Message}", "Erreur");
            }
        }

        private void textSearch_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Votre code ici
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Votre code ici
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Votre code ici
        }

        private void UpdateDailyWeatherIcons(ForecastData forecastData)
        {
            // Ici, vous parcourrez les prévisions météorologiques pour chaque jour et mettrez à jour les icônes en conséquence.
            foreach (var forecast in forecastData.List)
            {
                var date = DateTime.Parse(forecast.Dt_txt);
                var weather = forecast.Weather[0]; // Prenez le premier état météo de la journée.

                // Utilisez le jour de la semaine pour déterminer quel contrôle Image mettre à jour.
                Image weatherIconControl = null;
                switch (date.DayOfWeek)
                {
                    case DayOfWeek.Monday:
                        weatherIconControl = WeatherIconMonday;
                        break;
                    case DayOfWeek.Tuesday:
                        weatherIconControl = WeatherIconTuesday;
                        break;
                    case DayOfWeek.Wednesday:
                        weatherIconControl = WeatherIconWednesday;
                        break;
                    case DayOfWeek.Thursday:
                        weatherIconControl = WeatherIconThursday;
                        break;
                    case DayOfWeek.Friday:
                        weatherIconControl = WeatherIconFriday;
                        break;
                    case DayOfWeek.Saturday:
                        weatherIconControl = WeatherIconSaturday;
                        break;
                    case DayOfWeek.Sunday:
                        weatherIconControl = WeatherIconSunday;
                        break;
                }

                if (weatherIconControl != null)
                {
                    UpdateWeatherIcon(weather, weatherIconControl);
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var cities = File.ReadAllLines("C:\\Users\\Erzen\\Desktop\\Météo\\villes\\cities.txt");
                CitiesComboBox.ItemsSource = cities;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors du chargement des villes: " + ex.Message);
            }
        }

        private void CitiesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CitiesComboBox.SelectedItem != null)
            {
                string selectedCity = CitiesComboBox.SelectedItem.ToString();
                UpdateWeatherInfo(selectedCity);
                UpdateForecast(selectedCity);
                // Ajoutez d'autres mises à jour si nécessaire, comme UpdateUvIndex ou UpdateAirQuality.
            }
        }


        private void AddCityButton_Click(object sender, RoutedEventArgs e)
        {
            string newCity = NewCityTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(newCity) && !_cityManager.Cities.Contains(newCity))
            {
                _cityManager.AddCity(newCity);
                CitiesComboBox.ItemsSource = null; // Déconnectez la source de données précédente
                CitiesComboBox.ItemsSource = _cityManager.Cities; // Connectez la nouvelle source de données
                CitiesComboBox.SelectedItem = newCity; // Sélectionnez la nouvelle ville ajoutée
                NewCityTextBox.Text = ""; // Réinitialisez le champ de texte
            }
            else
            {
                MessageBox.Show("Veuillez entrer un nom de ville valide ou qui n'est pas déjà dans la liste.");
            }
        }



        private void RemoveCityButton_Click(object sender, RoutedEventArgs e)
        {
            if (CitiesComboBox.SelectedItem != null)
            {
                string cityToRemove = CitiesComboBox.SelectedItem.ToString();

                // Supprime la ville de la collection de données
                _cityManager.RemoveCity(cityToRemove);

                // Rafraîchit la source de données de la ComboBox
                CitiesComboBox.ItemsSource = null;
                CitiesComboBox.ItemsSource = _cityManager.Cities;

                // Vous pourriez vouloir réinitialiser le SelectedItem si nécessaire
                CitiesComboBox.SelectedItem = null;
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner une ville à supprimer.");
            }
        }


        //private void CitiesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    if (CitiesComboBox.SelectedItem != null)
        //    {
        //        string selectedCity = CitiesComboBox.SelectedItem.ToString();
        //        UpdateWeatherInfo(selectedCity); // Met à jour les informations météorologiques pour la ville sélectionnée
        //    }
        //}


        private void LoadCitiesToComboBox()
        {
            // Code pour charger les villes depuis le fichier texte dans le ComboBox
        }


        private string GetIconFileName(string weatherCondition)
        {
            switch (weatherCondition.ToLower())
            {
                case "clear":
                    return "sun.png";
                case "clouds":
                    return "cloud.png";
                case "rain":
                    return "rain.png";
                case "drizzle":
                    return "rain.png";
                case "thunderstorm":
                    return "storm.png";
                case "snow":
                    return "snow.png";
                // Ajoutez plus de conditions et d'images selon vos besoins
                default:
                    return "cloud.png"; // Une image par défaut si la condition est inconnue
            }
        }

        private string GetImagePath(string fileName)
        {
            return $"pack://application:,,,/Images/{fileName}";
        }


        private void UpdateWeatherIcon(WeatherDescription weather, Image weatherIconControl)
        {
            string iconFileName = GetIconFileName(weather.Main);
            string imagePath = GetImagePath(iconFileName);
            var imageUri = new Uri(imagePath, UriKind.Absolute);
            BitmapImage bitmapImage = new BitmapImage(imageUri);

            Dispatcher.Invoke(() =>
            {
                weatherIconControl.Source = bitmapImage;
            });
        }




    }
}
