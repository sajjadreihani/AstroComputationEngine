using AstroComputationEngine.Models.Astrology;
using SwissEphNet;
using System;
using System.Collections.Generic;
using System.Text;

namespace AstroComputationEngine.Utility;

public static class ChartHelper
{
    public static List<AspectDto> FindAspectsBetweenCharts(List<PlanetDto> natalPositions, List<PlanetDto> transitPositions, List<AspectDto> aspects, bool sameChart = false, bool synastry = false)
    {
        foreach (var natal in natalPositions)
        {
            foreach (var transit in transitPositions)
            {
                var planetA = $"{natal.Planet}{(natal.Retro ? " *R" : "")}";
                var planetB = $"{transit.Planet}{(transit.Retro ? " *R" : "")}";
                planetA = sameChart ? planetA : $" {(synastry ? "first chart" : "natal")} {planetA} ";
                planetB = sameChart ? planetB : $" {(synastry ? "second chart" : "transit")} {planetB} ";

                if (aspects.Any(a => (a.PlanetA.Equals(planetA) && a.PlanetB.Equals(planetB)) || (a.PlanetB.Equals(planetA) && a.PlanetA.Equals(planetB))))
                    continue;

                if (sameChart && natal.Planet.Equals(transit.Planet))
                    continue;

                double angle = Math.Abs(natal.Degree - transit.Degree);
                if (angle > 180) angle = 360 - angle;

                string aspectType = ChartHelper.GetAspectType(angle, 1.5);
                if (!string.IsNullOrWhiteSpace(aspectType))
                {
                    aspects.Add(new AspectDto
                    {
                        PlanetA = planetA,
                        PlanetB = planetB,
                        Orb = angle,
                        Type = aspectType
                    });
                }
            }
        }

        return aspects;
    }

    public static List<PlanetDto> BuildCompositeChart(IEnumerable<PlanetDto> chartA, IEnumerable<PlanetDto> chartB)
    {
        var composite = new List<PlanetDto>();

        foreach (var planet in chartA)
        {
            if (chartB.Any(c => c.Planet.Equals(planet.Planet)))
            {
                var planetB = chartB.First(c => c.Planet.Equals(planet.Planet));
                composite.Add(new PlanetDto
                {
                    Planet = planet.Planet,
                    Degree = GetMidpoint(planet.Degree, planetB.Degree),
                    House = 0,
                    Retro = planet.Retro && planetB.Retro,
                    Sign = planet.Sign
                });
            }
        }

        return composite;
    }

    public static double[] BuildCompositeHouse(double[] chartA, double[] chartB)
    {
        var composite = new double[13];

        for (int i = 1; i < 13; i++)
        {
            composite[i] = GetMidpoint(chartA[i], chartB[i]);
        }

        return composite;
    }

    public static string GetAspectType(double angle, double orb)
    {
        if (Math.Abs(angle - 0) <= orb) return "conjunction"; //"Conjunction"
        if (Math.Abs(angle - 60) <= orb) return "sextile"; //"Sextile"
        if (Math.Abs(angle - 90) <= orb) return "square"; //"Square"
        if (Math.Abs(angle - 120) <= orb) return "trine"; //"Trine"
        if (Math.Abs(angle - 180) <= orb) return "opposition"; //"Opposition"
        return string.Empty;
    }

    public static List<PlanetDto> AssignPlanetsToHouses(List<PlanetDto> planetPositions, double[] houseCusps)
    {
        var planetHouses = new List<PlanetDto>();

        foreach (var planet in planetPositions)
        {
            var planetDto = new PlanetDto
            {
                Degree = planet.Degree % 30,
                Planet = planet.Planet,
                Sign = GetSignName(planet.Degree),
                Retro = planet.Retro,
            };

            double planetLon = planet.Degree;

            for (int i = 1; i <= 12; i++)
            {
                double start = houseCusps[i];
                double end = houseCusps[i % 12 + 1];

                bool inHouse;

                if (start < end)
                {
                    inHouse = planetLon >= start && planetLon < end;
                }
                else
                {
                    inHouse = planetLon >= start || planetLon < end;
                }

                if (inHouse)
                {
                    planetDto.House = i;
                    break;
                }
            }

            planetHouses.Add(planetDto);
        }

        return planetHouses;
    }

    public static void EnrichChartWithAngles(List<PlanetDto> chart, double[] angles)
    {
        double sun = chart.First(p => p.Planet == "Sun").Degree;
        double moon = chart.First(p => p.Planet == "Moon").Degree;

        chart.Add(new PlanetDto { Planet = "Fortune", Degree = CalculatePartOfFortune(angles[0], sun, moon) });
        chart.Add(new PlanetDto { Planet = "ASC", Degree = angles[0] });
        chart.Add(new PlanetDto { Planet = "MC", Degree = angles[1] });
        chart.Add(new PlanetDto { Planet = "Vertex", Degree = angles[3] });
    }

    public static string GetPlanetName(int id)
    {
        return id switch
        {
            SwissEph.SE_SUN => "Sun", //"Sun"
            SwissEph.SE_MOON => "Moon", //"Moon"
            SwissEph.SE_MERCURY => "Mercury", //"Mercury"
            SwissEph.SE_VENUS => "Venus", //"Venus"
            SwissEph.SE_MARS => "Mars", //"Mars"
            SwissEph.SE_JUPITER => "Jupiter", //"Jupiter"
            SwissEph.SE_SATURN => "Saturn", //"Saturn"
            SwissEph.SE_URANUS => "Uranus", //"Uranus"
            SwissEph.SE_NEPTUNE => "Neptune", //"Neptune"
            SwissEph.SE_PLUTO => "Pluto", //"Pluto"
            SwissEph.SE_MEAN_NODE => "True Node", //"True Node"
            SwissEph.SE_MEAN_APOG => "Lilith", //"Lilith"
            SwissEph.SE_CHIRON => "Chiron", //"Chiron"
            _ => $"Planet_{id}"
        };
    }

    public static string GetSignName(double degree)
    {
        int house = (int)(degree / 30) % 12;
        return house switch
        {
            0 => "Aries",//"Aries",
            1 => "Taurus",//"Taurus",
            2 => "Gemini", //"Gemini",
            3 => "Cancer", //"Cancer",
            4 => "Leo", //"Leo",
            5 => "Virgo", //"Virgo",
            6 => "Libra", //"Libra",
            7 => "Scorpio", //"Scorpio",
            8 => "Sagittarius", //"Sagittarius",
            9 => "Capricorn", //"Capricorn",
            10 => "Aquarius", //"Aquarius",
            _ => "Pisces", //"Pisces"
        };
    }

    public static double Normalize(double degree)
    {
        degree %= 360;
        return degree < 0 ? degree + 360 : degree;
    }

    public static double CalculatePartOfFortune(double asc, double sun, double moon)
    {
        double fortune;
        if (IsDayChart(sun, asc))
            fortune = asc + moon - sun;
        else
            fortune = asc - moon + sun;

        return Normalize(fortune);
    }

    public static bool IsDayChart(double sun, double ascendant)
    {
        double descendant = (ascendant + 180) % 360;

        bool isAboveHorizon = IsAngleBetween(sun, descendant, ascendant);

        return isAboveHorizon;
    }

    public static bool IsAngleBetween(double angle, double start, double end)
    {
        angle = (angle + 360) % 360;
        start = (start + 360) % 360;
        end = (end + 360) % 360;

        if (start < end)
            return angle >= start && angle <= end;
        else
            return angle >= start || angle <= end;
    }

    public static double GetMidpoint(double deg1, double deg2)
    {
        double diff = Math.Abs(deg1 - deg2);
        if (diff > 180)
        {
            double mid = (deg1 + deg2 + 360) / 2;
            if (mid >= 360) mid -= 360;
            return mid;
        }
        return (deg1 + deg2) / 2;
    }
}
