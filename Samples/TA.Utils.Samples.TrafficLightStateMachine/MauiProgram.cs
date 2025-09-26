using LiveChartsCore.SkiaSharpView.Maui;
using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;
using CommunityToolkit.Maui;

namespace TA.Utils.Samples.TrafficLightStateMachine;
public static class MauiProgram
{

	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseSkiaSharp()
			.UseLiveCharts()
			.UseMauiCommunityToolkit()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if ANDROID
        // Expand Android slider hit area to improve touchability without changing visuals
        Microsoft.Maui.Handlers.SliderHandler.Mapper.AppendToMapping("IncreaseHitTarget", (handler, view) =>
        {
            var seekBar = handler.PlatformView;
            seekBar.Post(() =>
            {
                var parent = seekBar.Parent as Android.Views.View;
                if (parent == null) return;
                var rect = new Android.Graphics.Rect();
                seekBar.GetHitRect(rect);
                int extra = DpToPx(seekBar.Context, 16);
                rect.Top -= extra;
                rect.Bottom += extra;
                rect.Left -= extra;
                rect.Right += extra;
                parent.TouchDelegate = new Android.Views.TouchDelegate(rect, seekBar);
            });
        });

        static int DpToPx(Android.Content.Context context, int dp)
        {
            var scale = context.Resources?.DisplayMetrics?.Density ?? 1f;
            return (int)(dp * scale + 0.5f);
        }
#endif

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
