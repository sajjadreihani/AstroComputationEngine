using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AstroComputationEngine.Models.City;

public class CitySearchResponse
{
    public List<CitySearchResponseData> Results { get; set; } = [];
}

public class CitySearchResponseData
{
    public int Id { get; set; }
    public string Name { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Timezone { get; set; }
    public string Country { get; set; }

    [JsonIgnore]
    public string DisplayName => $"{Name}, {Country}";
}
