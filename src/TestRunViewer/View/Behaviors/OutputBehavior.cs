namespace TestRunViewer.View.Behaviors
{
    using System;
    using System.Windows;
    using TestRunViewer.Model;
    using TestRunViewer.View;

    public class OutputBehavior : ViewModelBehavior<OutputControl, IOutput, OutputBehavior>
    {
        #region BehaviorContext Property

        public static DependencyProperty BehaviorContextProperty = DependencyProperty.RegisterAttached("BehaviorContext", typeof(IOutput), typeof(OutputBehavior), new PropertyMetadata(default(IOutput), OnBehaviorContextChanged));

        public static IOutput GetBehaviorContext(DependencyObject target)
        {
            return (IOutput)target.GetValue(BehaviorContextProperty);
        }

        public static void SetBehaviorContext(DependencyObject target, IOutput value)
        {
            target.SetValue(BehaviorContextProperty, value);
        }

        public override IOutput BehaviorContext
        {
            get { return GetBehaviorContext(this); }
            set { SetBehaviorContext(this, value); }
        }

        #endregion // BehaviorContext Property

        protected override void SubscribeEvents(IOutput behaviorContext)
        {
            if (behaviorContext == null)
                return;

            // behaviorContext.Info += OnInfo;
            // behaviorContext.Success += OnSuccess;
            // behaviorContext.Warning += OnWarning;
            // behaviorContext.Error += OnError;
            // behaviorContext.Clear += OnClear;
        }

        protected override void UnsubscribeEvents(IOutput behaviorContext)
        {
            if (behaviorContext == null)
                return;

            // behaviorContext.Info -= OnInfo;
            // behaviorContext.Warning -= OnWarning;
            // behaviorContext.Error -= OnError;
            // behaviorContext.Clear -= OnClear;
        }

        private void OnInfo(object sender, OutputEventArgs e)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(new Action(() => OnInfo(sender, e)));
                return;
            }

            AssociatedObject.WriteLine(e.Message);
        }

        private void OnSuccess(object sender, OutputEventArgs e)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(new Action(() => OnSuccess(sender, e)));
                return;
            }

            AssociatedObject.WriteSuccessLine(e.Message);
        }

        private void OnWarning(object sender, OutputEventArgs e)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(new Action(() => OnWarning(sender, e)));
                return;
            }

            AssociatedObject.WriteWarningLine(e.Message);
        }

        private void OnError(object sender, OutputEventArgs e)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(new Action(() => OnError(sender, e)));
                return;
            }

            AssociatedObject.WriteErrorLine(e.Message);
        }

        private void OnClear(object sender, EventArgs e)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(new Action(() => OnClear(sender, e)));
                return;
            }

            AssociatedObject.Clear();
        }
    }
}