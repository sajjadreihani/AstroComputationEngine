using AstroComputationEngine.Interfaces;
using AstroComputationEngine.Models.Astrology;
using AstroComputationEngine.Models.Chart;
using AstroComputationEngine.Utility;
using Microsoft.Extensions.Primitives;
using SwissEphNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AstroComputationEngine.Services;

public class ChartService : IChartService
{
    private readonly string ephemerisPath;

    public ChartService()
    {
        #if __ANDROID__
                // On Android, try multiple possible locations
                // 1. Check files directory (where app data is stored)
                var filesPath = Path.Combine(AppContext.BaseDirectory, "Ephe");
                if (Directory.Exists(filesPath))
                {
                    ephemerisPath = filesPath;
                }
        
                // 2. Check cache directory
                if (ephemerisPath == null)
                {
                    var cachePath = Path.Combine(Android.App.Application.Context.CacheDir.AbsolutePath, "Ephe");
                    if (Directory.Exists(cachePath))
                    {
                        ephemerisPath = cachePath;
                    }
                }
        
                // 3. Try getting from assets (APK)
                if (ephemerisPath == null)
                {
                    try
                    {
                        var assets = Android.App.Application.Context.Assets;
                        var assetFiles = assets.List("Ephe");
                        if (assetFiles != null && assetFiles.Length > 0)
                        {
                            // Extract assets to cache on first run
                            ephemerisPath = ExtractEphemerisAssetsFromApk();
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error checking assets: {ex.Message}");
                    }
                }
        #else
                ephemerisPath = Path.Combine(AppContext.BaseDirectory, "Resources", "Ephe");
        #endif

        if (ephemerisPath == null)
        {
            ephemerisPath = Path.Combine(AppContext.BaseDirectory, "Ephe");
        }

    }

    #if __ANDROID__
        private static string ExtractEphemerisAssetsFromApk()
        {
            var cacheDir = Android.App.Application.Context.CacheDir;
            var ephemerisDir = Path.Combine(cacheDir.AbsolutePath, "Ephe");
    
            // Create directory if it doesn't exist
            Directory.CreateDirectory(ephemerisDir);
    
            try
            {
                var assets = Android.App.Application.Context.Assets;
                // Recursively extract all files under the Ephe asset folder
                ExtractAssetFolderRecursive(assets, "Ephe", ephemerisDir);
    
                // Report files extracted
                var extracted = Directory.GetFiles(ephemerisDir, "*", SearchOption.AllDirectories);
                if (extracted.Length > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"Extracted ephemeris files to: {ephemerisDir}");
                    return ephemerisDir;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error extracting ephemeris files: {ex.Message}");
            }
    
            return null;
        }
    #endif
    
    #if __ANDROID__
        private static void ExtractAssetFolderRecursive(global::Android.Content.Res.AssetManager assets, string assetFolder, string destFolder)
        {
            try
            {
                var list = assets.List(assetFolder);
                if (list == null || list.Length == 0)
                    return;
    
                foreach (var entry in list)
                {
                    // Construct paths using forward slashes for assets
                    var assetPath = string.IsNullOrEmpty(assetFolder) ? entry : assetFolder + "/" + entry;
                    // If entry has a dot and no sublist, treat as file; else check if it's a directory
                    var subList = assets.List(assetPath);
                    if (subList != null && subList.Length > 0)
                    {
                        // It's a directory
                        var subDest = Path.Combine(destFolder, entry);
                        Directory.CreateDirectory(subDest);
                        ExtractAssetFolderRecursive(assets, assetPath, subDest);
                    }
                    else
                    {
                        // It's a file
                        var destPath = Path.Combine(destFolder, entry);
                        if (File.Exists(destPath))
                            continue;
    
                        using (var input = assets.Open(assetPath))
                        using (var output = new System.IO.FileStream(destPath, System.IO.FileMode.Create))
                        {
                            input.CopyTo(output);
                        }
                    }
                }
            }
            catch (Java.Lang.Exception jex)
            {
                System.Diagnostics.Debug.WriteLine($"Android asset extraction error: {jex.Message}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Asset extraction error: {ex.Message}");
            }
        }
    #endif

    public string GenerateDaily(DailyChartInput input)
    {
        var morningDate = input.Current.Date.Date.AddHours(6);
        var noonDate = input.Current.Date.Date.AddHours(14);
        var nightDate = input.Current.Date.Date.AddHours(22);

        var birthChart = CalculateChart(input.Birth.Date.ToUtcFromTimeZone(input.Birth.TimeZone));

        var morningChart = CalculateChart(morningDate);

        var noonChart = CalculateChart(noonDate);

        var nightChart = CalculateChart(nightDate);

        var (house, angles) = CalculateHouses(input.Birth.Date.ToUtcFromTimeZone(input.Birth.TimeZone), input.Birth.Latitude, input.Birth.Longitude);

        var (noonHouse, noonAngles) = CalculateHouses(noonDate, input.Current.Latitude, input.Current.Longitude);

        ChartHelper.EnrichChartWithAngles(birthChart, angles);

        noonChart.Add(new PlanetDto { Planet = "Vertex", Degree = noonAngles[3] });

        var morningBirthAspects = ChartHelper.FindAspectsBetweenCharts(birthChart, morningChart, []);
        var noonBirthAspects = ChartHelper.FindAspectsBetweenCharts(birthChart, noonChart, morningBirthAspects);
        var aspects = ChartHelper.FindAspectsBetweenCharts(birthChart, nightChart, noonBirthAspects);

        var morningAspects = ChartHelper.FindAspectsBetweenCharts(morningChart, morningChart, [], true);
        var noonAspects = ChartHelper.FindAspectsBetweenCharts(noonChart, noonChart, morningAspects, true);
        var nightAspects = ChartHelper.FindAspectsBetweenCharts(nightChart, nightChart, noonAspects, true);

        var birthAspects = ChartHelper.FindAspectsBetweenCharts(birthChart, birthChart, [], true);

        StringBuilder reportBuilder = new();

        reportBuilder.AppendLine($"Birthdate {input.Birth.Date.ToString("yyyy-MM-dd HH:mm")} in {input.Birth.City}");
        reportBuilder.AppendLine(ReportHelper.ToPlanetSentence(ChartHelper.AssignPlanetsToHouses(birthChart, house)));
        reportBuilder.AppendLine($"Today: {morningDate.ToString("yyyy-MM-dd")} from 6 A.M. to 8 P.M. in {input.Current.City}");
        reportBuilder.AppendLine(ReportHelper.ToPlanetSentence(ChartHelper.AssignPlanetsToHouses(morningChart, house), ChartHelper.AssignPlanetsToHouses(nightChart, house)));
        reportBuilder.AppendLine(ReportHelper.ToAspectSentence(aspects, "Aspects"));
        reportBuilder.AppendLine(ReportHelper.ToAspectSentence(birthAspects, "Natal Aspects"));
        reportBuilder.AppendLine(ReportHelper.ToAspectSentence(nightAspects, "Internal Transit Aspects"));

        return reportBuilder.ToString();

    }

    public string GenerateMoment(DailyChartInput input)
    {
        var birthChart = CalculateChart(input.Birth.Date.ToUtcFromTimeZone(input.Birth.TimeZone));

        var currentChart = CalculateChart(input.Current.Date.ToUtcFromTimeZone(input.Current.TimeZone));

        var (house, angles) = CalculateHouses(input.Birth.Date.ToUtcFromTimeZone(input.Birth.TimeZone), input.Birth.Latitude, input.Birth.Longitude);

        var (currentHouse, currentAngles) = CalculateHouses(input.Current.Date.ToUtcFromTimeZone(input.Current.TimeZone), input.Current.Latitude, input.Current.Longitude);

        ChartHelper.EnrichChartWithAngles(birthChart, angles);

        currentChart.Add(new PlanetDto { Planet = "Vertex", Degree = currentAngles[3] });

        var aspects = ChartHelper.FindAspectsBetweenCharts(birthChart, currentChart, []);

        var currentAspects = ChartHelper.FindAspectsBetweenCharts(currentChart, currentChart, [], true);

        var birthAspects = ChartHelper.FindAspectsBetweenCharts(birthChart, birthChart, [], true);

        StringBuilder reportBuilder = new();

        reportBuilder.AppendLine($"Birthdate {input.Birth.Date.ToString("yyyy-MM-dd HH:mm")} in {input.Birth.City}");
        reportBuilder.AppendLine(ReportHelper.ToPlanetSentence(ChartHelper.AssignPlanetsToHouses(birthChart, house)));
        reportBuilder.AppendLine($"Today: {input.Current.Date.ToString("yyyy-MM-dd HH:mm")} in {input.Current.City}");
        reportBuilder.AppendLine(ReportHelper.ToPlanetSentence(ChartHelper.AssignPlanetsToHouses(currentChart, house)));
        reportBuilder.AppendLine(ReportHelper.ToAspectSentence(aspects, "Aspects"));
        reportBuilder.AppendLine(ReportHelper.ToAspectSentence(birthAspects, "Natal Aspects"));
        reportBuilder.AppendLine(ReportHelper.ToAspectSentence(currentAspects, "Internal Transit Aspects"));

        return reportBuilder.ToString();

    }

    public string GenerateComposite(RelationalChartInput input)
    {
        var morningDate = input.CurrentDate.Date.AddHours(6);
        var noonDate = input.CurrentDate.Date.AddHours(14);
        var nightDate = input.CurrentDate.Date.AddHours(22);

        var morningChart = CalculateChart(morningDate);

        var noonChart = CalculateChart(noonDate);

        var nightChart = CalculateChart(nightDate);

        var firstChart = CalculateChart(input.First.Date.ToUtcFromTimeZone(input.First.TimeZone));

        var secondChart = CalculateChart(input.Second.Date.ToUtcFromTimeZone(input.First.TimeZone));

        var (firstChartHouses, firstChartAngles) = CalculateHouses(input.First.Date.ToUtcFromTimeZone(input.First.TimeZone), input.First.Latitude, input.First.Longitude);

        var (secondChartHouses, secondChartAngles) = CalculateHouses(input.Second.Date.ToUtcFromTimeZone(input.Second.TimeZone), input.Second.Latitude, input.Second.Longitude);

        ChartHelper.EnrichChartWithAngles(firstChart, firstChartAngles);

        ChartHelper.EnrichChartWithAngles(secondChart, secondChartAngles);

        var compositeChart = ChartHelper.BuildCompositeChart(firstChart, secondChart);

        var compositeHouse = ChartHelper.BuildCompositeHouse(firstChartHouses, secondChartHouses);

        var morningBirthAspects = ChartHelper.FindAspectsBetweenCharts(compositeChart, morningChart, []);
        var noonBirthAspects = ChartHelper.FindAspectsBetweenCharts(compositeChart, noonChart, morningBirthAspects);
        var aspects = ChartHelper.FindAspectsBetweenCharts(compositeChart, nightChart, noonBirthAspects);

        var morningAspects = ChartHelper.FindAspectsBetweenCharts(morningChart, morningChart, [], true);
        var noonAspects = ChartHelper.FindAspectsBetweenCharts(noonChart, noonChart, morningAspects, true);
        var nightAspects = ChartHelper.FindAspectsBetweenCharts(nightChart, nightChart, noonAspects, true);

        var birthAspects = ChartHelper.FindAspectsBetweenCharts(compositeChart, compositeChart, [], true);

        StringBuilder reportBuilder = new();

        reportBuilder.AppendLine(ReportHelper.ToPlanetSentence(ChartHelper.AssignPlanetsToHouses(compositeChart, compositeHouse), "Composite Chart :"));
        reportBuilder.AppendLine($"Today: {morningDate.ToString("yyyy-MM-dd")} from 6 A.M. to 8 P.M. ");
        reportBuilder.AppendLine(ReportHelper.ToPlanetSentence(ChartHelper.AssignPlanetsToHouses(morningChart, compositeHouse), ChartHelper.AssignPlanetsToHouses(nightChart, compositeHouse)));
        reportBuilder.AppendLine(ReportHelper.ToAspectSentence(aspects, "Aspects"));
        reportBuilder.AppendLine(ReportHelper.ToAspectSentence(birthAspects, "Internal Composite Aspects"));
        reportBuilder.AppendLine(ReportHelper.ToAspectSentence(nightAspects, "Internal Transit Aspects"));


        return reportBuilder.ToString();

    }

    public string GenerateDavison(RelationalChartInput input)
    {
        var firstUtcDate = input.First.Date.ToUtcFromTimeZone(input.First.TimeZone);
        var secondUtcDate = input.Second.Date.ToUtcFromTimeZone(input.Second.TimeZone);
        var middleDate = firstUtcDate <= secondUtcDate ? firstUtcDate.AddSeconds((firstUtcDate - secondUtcDate).TotalSeconds / 2) : secondUtcDate.AddSeconds((firstUtcDate - secondUtcDate).TotalSeconds / 2);
        var middleLat = (input.First.Latitude + input.Second.Latitude) / 2;
        var middleLon = (input.First.Longitude + input.Second.Longitude) / 2;

        var morningDate = input.CurrentDate.Date.AddHours(6);
        var noonDate = input.CurrentDate.Date.AddHours(14);
        var nightDate = input.CurrentDate.Date.AddHours(22);

        var davisonChart = CalculateChart(middleDate);

        var morningChart = CalculateChart(morningDate);

        var noonChart = CalculateChart(noonDate);

        var nightChart = CalculateChart(nightDate);

        var (house, angles) = CalculateHouses(middleDate, middleLat, middleLon);

        ChartHelper.EnrichChartWithAngles(davisonChart, angles);

        var morningBirthAspects = ChartHelper.FindAspectsBetweenCharts(davisonChart, morningChart, []);
        var noonBirthAspects = ChartHelper.FindAspectsBetweenCharts(davisonChart, noonChart, morningBirthAspects);
        var aspects = ChartHelper.FindAspectsBetweenCharts(davisonChart, nightChart, noonBirthAspects);

        var morningAspects = ChartHelper.FindAspectsBetweenCharts(morningChart, morningChart, [], true);
        var noonAspects = ChartHelper.FindAspectsBetweenCharts(noonChart, noonChart, morningAspects, true);
        var nightAspects = ChartHelper.FindAspectsBetweenCharts(nightChart, nightChart, noonAspects, true);

        var birthAspects = ChartHelper.FindAspectsBetweenCharts(davisonChart, davisonChart, [], true);

        StringBuilder reportBuilder = new();

        reportBuilder.AppendLine($"Davison Date {middleDate.ToString("yyyy-MM-dd HH:mm")} UTC");
        reportBuilder.AppendLine(ReportHelper.ToPlanetSentence(ChartHelper.AssignPlanetsToHouses(davisonChart, house), "Davison Chart :"));
        reportBuilder.AppendLine($"Today: {morningDate.ToString("yyyy-MM-dd")} from 6 A.M. to 8 P.M. ");
        reportBuilder.AppendLine(ReportHelper.ToPlanetSentence(ChartHelper.AssignPlanetsToHouses(morningChart, house), ChartHelper.AssignPlanetsToHouses(nightChart, house)));
        reportBuilder.AppendLine(ReportHelper.ToAspectSentence(aspects, "Aspects"));
        reportBuilder.AppendLine(ReportHelper.ToAspectSentence(birthAspects, "Davison Aspects"));
        reportBuilder.AppendLine(ReportHelper.ToAspectSentence(nightAspects, "Internal Transit Aspects"));

        return reportBuilder.ToString();

    }


    private List<PlanetDto> CalculateChart(DateTime date)
    {
        var result = new List<PlanetDto>();
        using (var swe = new SwissEphNet.SwissEph())
        {
            swe.swe_set_ephe_path(ephemerisPath);

            swe.OnLoadFile += (s, e) =>
            {
                try
                {
                    e.File = File.OpenRead(e.FileName.Replace("\\", Path.DirectorySeparatorChar.ToString()));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading ephemeris file: {ex.Message}");
                }
            };
            double julianDay = swe.swe_julday(date.Year, date.Month, date.Day, date.Hour + date.Minute / 60.0, SwissEph.SE_GREG_CAL);


            // Planets to calculate
            int[] planetIds =
            [
                SwissEph.SE_SUN,
            SwissEph.SE_MOON,
            SwissEph.SE_MERCURY,
            SwissEph.SE_VENUS,
            SwissEph.SE_MARS,
            SwissEph.SE_JUPITER,
            SwissEph.SE_SATURN,
            SwissEph.SE_URANUS,
            SwissEph.SE_NEPTUNE,
            SwissEph.SE_PLUTO,
            SwissEph.SE_MEAN_NODE,
            SwissEph.SE_MEAN_APOG
            ];

            foreach (int planetId in planetIds)
            {
                double[] xx = new double[6];
                string serr = string.Empty;
                int iflag = SwissEph.SEFLG_SWIEPH | SwissEph.SEFLG_SPEED;

                swe.swe_calc_ut(julianDay, planetId, iflag, xx, ref serr);

                result.Add(new PlanetDto
                {
                    Planet = ChartHelper.GetPlanetName(planetId),
                    Degree = xx[0],
                    Retro = xx[3] < 0
                });
            }
        }

        return result;
    }

    private (double[] HouseCusps, double[] Angles) CalculateHouses(DateTime date, double latitude, double longitude)
    {
        using (var swe = new SwissEphNet.SwissEph())
        {
            swe.swe_set_ephe_path(ephemerisPath);

            swe.OnLoadFile += (s, e) =>
            {
                try
                {
                    e.File = File.OpenRead(e.FileName.Replace("\\", Path.DirectorySeparatorChar.ToString()));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading ephemeris file: {ex.Message}");
                }
            };

            double jd_ut = swe.swe_julday(date.Year, date.Month, date.Day, date.Hour + date.Minute / 60.0, SwissEph.SE_GREG_CAL);

            double[] cusps = new double[13];
            double[] ascmc = new double[10];

            swe.swe_houses(jd_ut, latitude, longitude, 'P', cusps, ascmc);

            return (cusps, ascmc);
        }
    }
}
