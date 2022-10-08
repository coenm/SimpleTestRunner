using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace TestRunViewer.View
{
    using System;

    public partial class OutputControl : UserControl
    {
        private RichTextBox _richTextBox;
        private TextBox _textBox;

        public OutputControl()
        {
            InitializeComponent();

            this.Loaded += OnLoaded;
        }

        public void WriteLine(string line)
        {
            WriteLine(line, null);
        }

        public void WriteSuccessLine(string line)
        {
            WriteLine(line, Brushes.Green);
        }

        public void WriteWarningLine(string line)
        {
            WriteLine(line, Brushes.Orange);
        }

        public void WriteErrorLine(string line)
        {
            WriteLine(line, Brushes.Red);
        }

        public void Clear()
        {
            _richTextBox.SelectAll();
            _richTextBox.Document.Blocks.Clear();
        }

        private void WriteLine(string line, Brush? color)
        {
            _textBox.AppendText(line + Environment.NewLine);
            _textBox.ScrollToEnd();
            return;
            Paragraph paragraph = _richTextBox.CaretPosition.Paragraph;
            if (paragraph == null)
            {
                paragraph = new Paragraph();
                _richTextBox.Document.Blocks.Add(paragraph);
                _richTextBox.CaretPosition = paragraph.ContentEnd;
            }

            var run = new Run(line);
            if (color != null)
                run.Foreground = color;

            paragraph.Inlines.Add(run);
            run.ElementEnd.InsertLineBreak();

            _richTextBox.ScrollToEnd();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            IEnumerable<TextBox> richTextBoxes = FindVisualChildren<TextBox>(this);
            _textBox = richTextBoxes.Single();
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject reference) where T : DependencyObject
        {
            if (reference != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(reference); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(reference, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
    }
}
