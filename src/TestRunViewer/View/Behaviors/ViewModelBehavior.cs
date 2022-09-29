namespace TestRunViewer.View.Behaviors;

using System.Linq;
using System.Windows;
using Microsoft.Xaml.Behaviors;

public abstract class ViewModelBehavior<TAssociatedObject, TBehaviorContext, TBehavior> : Behavior<TAssociatedObject>
    where TAssociatedObject : DependencyObject
    where TBehaviorContext : class
    where TBehavior : ViewModelBehavior<TAssociatedObject, TBehaviorContext, TBehavior>, new()
{
    #region BehaviorContext Property

    public abstract TBehaviorContext BehaviorContext { get; set; }

    protected static void OnBehaviorContextChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        var @this = sender as TBehavior;

        if (@this == null)
        {
            if (e.NewValue != null)
                AttachBehavior(sender, (TBehaviorContext)e.NewValue);
            else
                DetachBehavior(sender);

            return;
        }

        @this.UnsubscribeEvents(e.OldValue as TBehaviorContext);
        @this.SubscribeEvents(e.NewValue as TBehaviorContext);
    }

    #endregion // BehaviorContext Property

    #region Attachment

    protected override void OnAttached()
    {
        UnsubscribeEvents(BehaviorContext);
        SubscribeEvents(BehaviorContext);
    }

    protected override void OnDetaching()
    {
        UnsubscribeEvents(BehaviorContext);
    }

    private static void AttachBehavior(DependencyObject target, TBehaviorContext behaviorContext)
    {
        var behaviors = Interaction.GetBehaviors(target);

        if (behaviors.OfType<TBehavior>().Any())
            return;

        behaviors.Add(new TBehavior { BehaviorContext = behaviorContext });
    }

    private static void DetachBehavior(DependencyObject target)
    {
        var behaviors = Interaction.GetBehaviors(target);

        foreach (var behavior in behaviors.OfType<TBehavior>())
            behaviors.Remove(behavior);
    }

    #endregion

    protected abstract void SubscribeEvents(TBehaviorContext behaviorContext);

    protected abstract void UnsubscribeEvents(TBehaviorContext behaviorContext);
}