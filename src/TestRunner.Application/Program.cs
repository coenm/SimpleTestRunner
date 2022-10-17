namespace TestRunner.Application;

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Linq;
using Serialization;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using TestRunner.Application.View;
using TestRunner.Application.ViewModel;
using TestRunner.Core;

public class Program
{
    [STAThread]
    public static void Main()
    {
        Container container = Bootstrap();
        RunApplication(container);
    }

    private static Container Bootstrap()
    {
        var args = new Args(Environment.GetCommandLineArgs());
        _ = SetupConfiguration(args.ApplicationArgs); //todo

        var container = new Container();
        container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
        container.Register<DotNetExecutable>(Lifestyle.Singleton);
        container.RegisterInstance(args);

        container.Register<MainWindow>(Lifestyle.Singleton);
        container.Register<MainViewModel>(Lifestyle.Singleton);
        container.Register<LogViewModel>(Lifestyle.Singleton);

        container.Register<DotNetTestExecutor>(Lifestyle.Singleton);
        container.Register<Serialization>(Lifestyle.Singleton);
        container.Register<ConsoleOutputProcessor>(Lifestyle.Singleton);
        // container.Verify();

        return container;
    }

    private static void RunApplication(Container container)
    {
        try
        {
            var app = new App();
            app.InitializeComponent();
            MainWindow mainWindow = container.GetInstance<MainWindow>();

            Args args = container.GetInstance<Args>();

            if (TryGetTitle(args, out var title))
            {
                mainWindow.Title = title;
            }

            app.Run(mainWindow);
        }
        catch (Exception ex)
        {
            //Log the exception and exit
            throw;
        }
    }

    private static bool TryGetTitle(Args args, [NotNullWhen(true)] out string? title)
    {
        title = null;

        var item = args.ApplicationArgs.FirstOrDefault(x => x.StartsWith("--title="));
        if (item == null)
        {
            return false;
        }

        var titleString = item["--title=".Length..];
        if (string.IsNullOrWhiteSpace(titleString))
        {
            return false;
        }

        title = titleString.Trim();
        return true;
    }

    private static IConfiguration SetupConfiguration(string[] args)
    {
        IConfigurationBuilder builder = new ConfigurationBuilder()
                                        .AddCommandLine(args)
                                        .SetBasePath(Directory.GetCurrentDirectory())
                                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                                        .AddJsonFile("logging.json", optional: true, reloadOnChange: false)
                                        .AddEnvironmentVariables();

        return builder.Build();
    }
}