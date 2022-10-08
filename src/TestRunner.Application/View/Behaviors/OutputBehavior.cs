namespace TestRunner.Application.View.Behaviors
{
    using System.Windows;
    using TestRunner.Application.Model;

    public class OutputBehavior : ViewModelBehavior<OutputControl, IConsoleOutput, OutputBehavior>
    {
        #region BehaviorContext Property

        public static readonly DependencyProperty BehaviorContextProperty = DependencyProperty.RegisterAttached("BehaviorContext", typeof(IConsoleOutput), typeof(OutputBehavior), new PropertyMetadata(default(IConsoleOutput), OnBehaviorContextChanged));

        public static IConsoleOutput GetBehaviorContext(DependencyObject target)
        {
            return (IConsoleOutput)target.GetValue(BehaviorContextProperty);
        }

        public static void SetBehaviorContext(DependencyObject target, IConsoleOutput value)
        {
            target.SetValue(BehaviorContextProperty, value);
        }

        public override IConsoleOutput BehaviorContext
        {
            get => GetBehaviorContext(this);
            set => SetBehaviorContext(this, value);
        }

        #endregion // BehaviorContext Property

        protected override void SubscribeEvents(IConsoleOutput behaviorContext)
        {
            if (behaviorContext == null)
            {
                return;
            }

            behaviorContext.StdOut += OnStdOut;
            behaviorContext.StdErr += OnStdErr;
        }

        protected override void UnsubscribeEvents(IConsoleOutput behaviorContext)
        {
            if (behaviorContext == null)
            {
                return;
            }

            behaviorContext.StdOut -= OnStdOut;
            behaviorContext.StdErr -= OnStdErr;
        }

        private void OnStdOut(object sender, string e)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => OnStdOut(sender, e));
                return;
            }

            AssociatedObject.WriteLine(e);
        }

        private void OnStdErr(object sender, string e)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => OnStdErr(sender, e));
                return;
            }

            AssociatedObject.WriteLine(e);
        }
    }
}