using AstroComputationEngine.Models.City;

namespace AstroComputationEngine.Interfaces;
public interface ICityService
{
    Task<IEnumerable<CitySearchResponseData>> Search(string name);
}
