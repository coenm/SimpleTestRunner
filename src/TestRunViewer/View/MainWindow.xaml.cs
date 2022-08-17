namespace TestRunViewer.View;

using System;
using System.Windows;
using MahApps.Metro.Controls;
using TestRunViewer.Misc;
using TestRunViewer.ViewModel;

public partial class MainWindow : MetroWindow
{
    public MainWindow(MainViewModel vm)
    {
        InitializeComponent();

        Loaded += OnLoaded;
        Closed += OnClosed;
        DataContext = vm;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        (DataContext as IInitializable)?.Initialize();
    }

    private void OnClosed(object sender, EventArgs e)
    {
        (DataContext as IDisposable)?.Dispose();
    }
}