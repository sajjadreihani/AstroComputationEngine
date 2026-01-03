using AstroComputationEngine.Interfaces;
using AstroComputationEngine.Services;
using Microsoft.Extensions.Logging;

namespace AstroComputationEngine
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddScoped<IAIService, AIService>();
            builder.Services.AddScoped<IChartService, ChartService>();
            builder.Services.AddScoped<ICityService, CityService>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
