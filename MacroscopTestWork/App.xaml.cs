using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Windows;
using MacroscopTestWork.Options;
using MacroscopTestWork.Services;
using MacroscopTestWork.ViewModels;

namespace MacroscopTestWork
{
    public partial class App : Application
    {
        private IHost _host = null!;
        private bool _hostStarted = false;

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                base.OnStartup(e);

                _host = Host.CreateDefaultBuilder()
                    .ConfigureAppConfiguration((context, config) =>
                    {
                        config.SetBasePath(AppContext.BaseDirectory);
                        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
                    })
                    .ConfigureServices((context, services) => ConfigureServices(context, services))
                    .Build();

                _host.Start();
                _hostStarted = true;

                var mainWindow = _host.Services.GetRequiredService<MainWindow>();
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Приложение не смогло запуститься:\n\n{ex.Message}\n\nВнутренняя ошибка:\n{ex.InnerException?.Message}",
                    "Ошибка инициализации",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Shutdown();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                if (_hostStarted && _host != null)
                    _host.StopAsync().GetAwaiter().GetResult();
            }
            finally
            {
                _host?.Dispose();
                base.OnExit(e);
            }
        }

        private void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            var configuration = context.Configuration;

            // 1. Привязка конфигурации
            services.Configure<LoaderOptions>(configuration.GetSection("Loader"));

            // 2. HttpClient с таймаутом из конфига
            services.AddHttpClient<IImageDownloadService, ImageDownloadService>((sp, client) =>
            {
                var opts = sp.GetRequiredService<IOptions<LoaderOptions>>().Value;
                client.Timeout = TimeSpan.FromSeconds(opts.HttpTimeoutSeconds);
            });

            // 3. Фабрика слотов
            services.AddSingleton<Func<ImageItemViewModel>>(sp =>
                () => ActivatorUtilities.CreateInstance<ImageItemViewModel>(sp));

            // 4. ViewModel и View
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<MainWindow>();
        }
    }
}