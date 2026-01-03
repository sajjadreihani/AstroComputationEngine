using AstroComputationEngine.Models.Astrology;
using System;
using System.Collections.Generic;
using System.Text;

namespace AstroComputationEngine.Utility;

public static class ReportHelper
{
    public static string ToPlanetSentence(IEnumerable<PlanetDto> planets, string title = "Natal Chart :")
    {
        if (planets == null || !planets.Any()) return "";

        var sb = new StringBuilder();
        sb.AppendLine(title);

        foreach (var planet in planets)
        {
            int degree = (int)Math.Round(planet.Degree);
            sb.AppendLine($"{planet.Planet}{(planet.Retro ? " *R" : "")} {degree} degree in {planet.Sign} in House {planet.House} ,");
        }

        if (sb.Length > 2)
            sb.Length -= 2;

        return sb.ToString();
    }

    public static string ToPlanetSentence(IEnumerable<PlanetDto> morning, IEnumerable<PlanetDto> night)
    {
        if (morning == null || !morning.Any() || night == null || !night.Any()) return "";

        var sb = new StringBuilder();
        sb.AppendLine("Transit Chart : ");

        foreach (var planet in morning)
        {
            var nightPlanet = night.First(n => n.Planet.Equals(planet.Planet));
            int nightDegree = (int)Math.Round(nightPlanet.Degree);
            int degree = (int)Math.Round(planet.Degree);
            sb.Append($"{planet.Planet}{(planet.Retro ? " *R" : "")} {degree} degree in {planet.Sign} in House {planet.House}");

            if (nightDegree != degree)
                sb.Append($" to {nightDegree} degree");

            if (!planet.Sign.Equals(nightPlanet.Sign))
                sb.Append($" in {nightPlanet.Sign}");

            if (planet.House != nightPlanet.House)
                sb.Append($" in House {nightPlanet.House}");

            sb.Append(" ,");
            sb.AppendLine();
        }

        if (sb.Length > 2)
            sb.Length -= 2;

        return sb.ToString();
    }

    public static string ToAspectSentence(List<AspectDto> aspects, string label)
    {
        if (aspects == null || aspects.Count == 0) return "";

        var sb = new StringBuilder();
        sb.Append(label + ": ");

        foreach (var asp in aspects)
        {
            sb.AppendLine($"{asp.PlanetA} {asp.Type} {asp.PlanetB} ,");
        }

        if (sb.Length > 2)
            sb.Length -= 2;

        return sb.ToString();
    }
}
