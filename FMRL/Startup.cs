using FMRL.Services;
using FMRL.Services.Impl;
using Microsoft.AspNetCore.Blazor.Builder;
using Microsoft.Extensions.DependencyInjection;
using Sotsera.Blazor.Toaster.Core.Models;

namespace FMRL
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ICrypto, BclCrypto>();
            //services.AddSingleton<IRepo, FirebaseRepo>();
            services.AddSingleton<IRepo, ParseRepo>();
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
        }
    }
}
