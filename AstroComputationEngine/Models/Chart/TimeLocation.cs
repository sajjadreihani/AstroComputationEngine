using System;
using System.Collections.Generic;
using System.Text;

namespace AstroComputationEngine.Models.Chart;

public class TimeLocation(DateTime date, double latitude, double longitude, string city, string timeZone)
{
    public DateTime Date { get; set; } = date;
    public double Latitude { get; set; } = latitude;
    public double Longitude { get; set; } = longitude;
    public string City { get; set; } = city;
    public string TimeZone { get; set; } = timeZone;
}