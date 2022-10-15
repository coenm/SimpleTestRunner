namespace TestRunner.Application;

using System;
using Microsoft.Extensions.Configuration;
using System.IO;
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
        var container = new Container();
        container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
        container.Register<DotNetExecutable>(Lifestyle.Singleton);
        container.RegisterInstance(new Args(Environment.GetCommandLineArgs()));

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
            mainWindow.Title = "Tjoeps";
            app.Run(mainWindow);
        }
        catch (Exception ex)
        {
            //Log the exception and exit
            throw;
        }
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