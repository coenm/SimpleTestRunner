namespace TestRunner.Application.View;

using System;
using System.Windows;
using System.Windows.Media;
using MahApps.Metro.Controls;
using TestRunner.Application.Misc;
using TestRunner.Application.ViewModel;

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
        WriteLine("Test test test", Brushes.Red);
        WriteLine("Rian", Brushes.Black);
        WriteLine("Coen", Brushes.Green);
    }

    private void OnClosed(object sender, EventArgs e)
    {
        (DataContext as IDisposable)?.Dispose();
    }

    private void WriteLine(string line, Brush color)
    {
        // Paragraph paragraph = Rtb.CaretPosition.Paragraph;
        // if (paragraph == null)
        // {
        //     paragraph = new Paragraph();
        //     Rtb.Document.Blocks.Add(paragraph);
        //     Rtb.CaretPosition = paragraph.ContentEnd;
        // }
        //
        // var run = new Run(line) { Foreground = color, };
        // paragraph.Inlines.Add(run);
        // run.ElementEnd.InsertLineBreak();
        //
        // Rtb.ScrollToEnd();
    }
}