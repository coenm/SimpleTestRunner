namespace TestRunViewer;

using System;
using System.Windows;
using TestRunViewer.View;
using TestRunViewer.ViewModel;

public partial class App : Application
{
    [STAThread]
    public static void Main()
    {
        var app = new App();
        app.InitializeComponent();
        app.Run();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Application is running
        // Process command line args
        var startMinimized = false;
        for (var i = 0; i != e.Args.Length; ++i)
        {
            if (e.Args[i] == "/StartMinimized")
            {
                startMinimized = true;
            }
        }

        var vm = new MainViewModel();
        var mainWindow = new MainWindow(vm);
        if (startMinimized)
        {
            mainWindow.WindowState = WindowState.Minimized;
        }

        mainWindow.Show();
    }
}