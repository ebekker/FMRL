using Blazor.Extensions.Storage;
using FMRL.Services;
using FMRL.Services.Impl;
using Microsoft.AspNetCore.Blazor.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Sotsera.Blazor.Toaster.Core.Models;

namespace FMRL
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ICrypto, BclCrypto>();
            services.AddSingleton<BclCrypto>();

#if USE_LOCAL_REPO
            // Useful for local/disconnected development/testing;
            // enable this by building with:
            //    dotnet build /p:USE_LOCAL_REPO=1
#warning Building with LOCAL REPO
            services.AddStorage();
            services.AddSingleton<IRepo, LocalStorageRepo>();
#else
            //services.AddSingleton<IRepo, FirebaseRepo>();
            services.AddSingleton<IRepo, ParseRepo>();
#endif

            services.AddToaster(config =>
            {
                // Use the sample app to experiment with different options:
                //    https://sotsera.github.io/sotsera.blazor.toaster/
                config.PositionClass = Defaults.Classes.Position.TopCenter;
                config.PreventDuplicates = true;
                config.NewestOnTop = true;
                config.ShowTransitionDuration = 500;
                config.ShowStepDuration = 100;
                config.VisibleStateDuration = 5000;
                config.ProgressBarStepDuration = 50;
                config.HideTransitionDuration = 500;
                config.HideStepDuration = 100;
            });
        }

        public void Configure(IBlazorApplicationBuilder app)
        {
            app.AddComponent<App>("app");
            ((IJSInProcessRuntime)JSRuntime.Current).Invoke<bool>("_blazorXstartup.dnSetReady");
        }
    }
}
