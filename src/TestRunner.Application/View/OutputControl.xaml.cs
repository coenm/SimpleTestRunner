namespace TestRunner.Application.View
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Media;

    public partial class OutputControl : UserControl
    {
        private RichTextBox _richTextBox;
        private TextBox _textBox;
        private object _lock = new object();

        public OutputControl()
        {
            InitializeComponent();

            this.Loaded += OnLoaded;
        }

        public void WriteLine(string line)
        {
            // if (line.StartsWith("Ended"))
                WriteLine(line, Brushes.Black);
            // WriteLine("Aap", Brushes.Black);
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

        private void WriteLine(string line, Brush color)
        {
            _textBox.AppendText(line + Environment.NewLine);
            _textBox.ScrollToEnd();
        }

        private void WriteLine1(string line, Brush color)
        {
            lock (_lock)
            {
                Paragraph paragraph = _richTextBox.CaretPosition.Paragraph;
                if (paragraph == null)
                {
                    paragraph = new Paragraph();
                    _richTextBox.Document.Blocks.Add(paragraph);
                    _richTextBox.CaretPosition = paragraph.ContentEnd;
                }

                var run = new Run(line)/* { Foreground = color }*/;

                paragraph.Inlines.Add(run);
                paragraph.Inlines.Add(new Run(Environment.NewLine));
                // run.ElementEnd.InsertLineBreak();

                _richTextBox.ScrollToEnd();
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            IEnumerable<RichTextBox> richTextBoxes = FindVisualChildren<RichTextBox>(this);
            //_richTextBox = richTextBoxes.Single();
            _textBox = FindVisualChildren<TextBox>(this).Single();
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject reference) where T : DependencyObject
        {
            if (reference == null)
            {
                yield break;
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(reference); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(reference, i);
                if (child is T dependencyObject)
                {
                    yield return dependencyObject;
                }

                foreach (T childOfChild in FindVisualChildren<T>(child))
                {
                    yield return childOfChild;
                }
            }
        }
    }
}
