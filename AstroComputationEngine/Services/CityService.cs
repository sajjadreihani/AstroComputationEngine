using AstroComputationEngine.Interfaces;
using AstroComputationEngine.Models.City;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AstroComputationEngine.Services;
public class CityService : ICityService
{
    private readonly JsonSerializerOptions serializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<IEnumerable<CitySearchResponseData>> Search(string name)
    {
        try
        {
            using HttpClient client = new();

            var response = await client.GetStringAsync($"https://geocoding-api.open-meteo.com/v1/search?name={name}&count=10&language=en&format=json");
            var cities = JsonSerializer.Deserialize<CitySearchResponse>(response, serializerOptions);

            return cities.Results;
        }
        catch
        {
            return [];
        }
    }
}
