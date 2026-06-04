using LibraryManagement.Application;
using LibraryManagement.Application.Abstractions;
using LibraryManagement.Data;
using LibraryManagement.WinForms.Forms;
using LibraryManagement.WinForms.Helpers;
using LibraryManagement.WinForms.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryManagement.WinForms;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            try
            {
                if (e.ExceptionObject is Exception ex)
                    File.AppendAllText(Path.Combine(AppContext.BaseDirectory, "crashlog.txt"),
                        $"[{DateTime.Now:O}] AppDomain unhandled\n{ex}\n\n");
            }
            catch { }
        };

        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            try
            {
                File.AppendAllText(Path.Combine(AppContext.BaseDirectory, "crashlog.txt"),
                    $"[{DateTime.Now:O}] UnobservedTaskException\n{e.Exception}\n\n");
            }
            catch { }
            e.SetObserved();
        };

        try
        {
            RunApp();
        }
        catch (Exception ex)
        {
            try
            {
                File.AppendAllText(Path.Combine(AppContext.BaseDirectory, "crashlog.txt"),
                    $"[{DateTime.Now:O}] Main caught\n{ex}");
            }
            catch { }
            throw;
        }
    }

    private static void RunApp()
    {
        ApplicationConfiguration.Initialize();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .Build();

        var connectionString = configuration.GetConnectionString("Library")
            ?? throw new InvalidOperationException("Не задана строка подключения 'Library' в appsettings.json.");

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        ConfigureServices(services, connectionString);
        var provider = services.BuildServiceProvider();
        AppHost.SetServices(provider);

        System.Windows.Forms.Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        System.Windows.Forms.Application.ThreadException += (_, e) =>
        {
            try
            {
                File.AppendAllText(Path.Combine(AppContext.BaseDirectory, "crashlog.txt"),
                    $"[{DateTime.Now:O}] ThreadException\n{e.Exception}");
            }
            catch { }
            Ui.ShowError(null, "Необработанная ошибка:\n" + e.Exception.Message, "Сбой");
        };

        try
        {
            AppDbInitializer.InitializeAsync(provider).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            try
            {
                File.AppendAllText(Path.Combine(AppContext.BaseDirectory, "crashlog.txt"),
                    $"[{DateTime.Now:O}] DB init failed\n{ex}");
            }
            catch { }
            Ui.ShowError(null, "Не удалось инициализировать базу данных:\n" + ex.Message, "Ошибка запуска");
            return;
        }

        var mainForm = provider.GetRequiredService<MainForm>();
        System.Windows.Forms.Application.Run(mainForm);
    }

    private static void ConfigureServices(IServiceCollection services, string connectionString)
    {
        services.AddData(connectionString);
        services.AddApplication();

        services.AddSingleton<ICurrentUserService, CurrentUserService>();

        services.AddTransient<MainForm>();
    }
}
