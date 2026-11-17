using Microsoft.Extensions.Logging;

namespace RevisaAi
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
                    // Texto geral
                    fonts.AddFont("Barlow-Regular.ttf", "Barlow");
                    fonts.AddFont("Barlow-SemiBold.ttf", "BarlowSemiBold");
                    // Títulos
                    fonts.AddFont("BarlowCondensed-SemiBold.ttf", "BarlowCondSemiBold");
                    fonts.AddFont("BarlowCondensed-Bold.ttf", "BarlowCondBold");
                    // Números (hodômetro, valores, datas)
                    fonts.AddFont("IBMPlexMono-Medium.ttf", "PlexMono");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
