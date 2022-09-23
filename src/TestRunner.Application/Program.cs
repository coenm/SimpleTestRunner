namespace TestRunner.Application;

using System;
using SimpleInjector;
using SimpleInjector.Lifestyles;
using TestRunner.Application.View;
using TestRunner.Application.ViewModel;

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

        container.Register<MainWindow>(Lifestyle.Singleton);
        container.Register<MainViewModel>(Lifestyle.Singleton);
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
            app.Run(mainWindow);
        }
        catch (Exception ex)
        {
            //Log the exception and exit
            throw;
        }
    }
}