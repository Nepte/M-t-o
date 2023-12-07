using System.Collections.Generic;
using System;
using System.IO;


public class CityManager
{
    private readonly string _filePath;
    private List<string> _cities;

    public CityManager(string filePath)
    {
        _filePath = filePath;
        _cities = LoadCities(); // Initialise _cities avec le résultat de LoadCities
    }

    public List<string> Cities
    {
        get { return _cities; }
    }

    public void AddCity(string city)
    {
        if (!_cities.Contains(city))
        {
            _cities.Add(city);
            SaveCities();
        }
    }

    public void RemoveCity(string city)
    {
        if (_cities.Contains(city))
        {
            _cities.Remove(city);
            SaveCities();
        }
    }

    public List<string> LoadCities()
    {
        if (File.Exists(_filePath))
        {
            _cities = new List<string>(File.ReadAllLines(_filePath));
        }
        else
        {
            _cities = new List<string>(); // Initialise _cities avec une nouvelle liste vide
        }
        return _cities;
    }




    private void SaveCities()
    {
        // Votre code pour sauvegarder la liste des villes dans le fichier
        File.WriteAllLines(_filePath, _cities);
    }
}
