namespace TestRunViewer.View.Behaviors
{
    using System.Windows;
    using TestRunViewer.Model;
    using TestRunViewer.View;

    public class OutputBehavior : ViewModelBehavior<OutputControl, IConsoleOutput2, OutputBehavior>
    {
        #region BehaviorContext Property

        public static DependencyProperty BehaviorContextProperty = DependencyProperty.RegisterAttached("BehaviorContext", typeof(IConsoleOutput2), typeof(OutputBehavior), new PropertyMetadata(default(IConsoleOutput2), OnBehaviorContextChanged));

        public static IConsoleOutput2 GetBehaviorContext(DependencyObject target)
        {
            return (IConsoleOutput2)target.GetValue(BehaviorContextProperty);
        }

        public static void SetBehaviorContext(DependencyObject target, IConsoleOutput2 value)
        {
            target.SetValue(BehaviorContextProperty, value);
        }

        public override IConsoleOutput2 BehaviorContext
        {
            get { return GetBehaviorContext(this); }
            set
            {
                SetBehaviorContext(this, value);
            }
        }

        #endregion // BehaviorContext Property

        protected override void SubscribeEvents(IConsoleOutput2 behaviorContext)
        {
            if (behaviorContext == null)
                return;

            behaviorContext.StdOut += OnStdOut;
            behaviorContext.StdErr += OnStdErr;
        }

        protected override void UnsubscribeEvents(IConsoleOutput2 behaviorContext)
        {
            if (behaviorContext == null)
                return;

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