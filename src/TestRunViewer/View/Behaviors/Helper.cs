namespace TestRunViewer.View.Behaviors
{
    using System;
    using System.Collections;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    public static class Helper
    {
        public static void AddItem(ItemsControl itemsControl, object item, int insertIndex)
        {
            if (itemsControl.ItemsSource == null)
            {
                itemsControl.Items.Insert(insertIndex, item);
                return;
            }
            
            var iList = itemsControl.ItemsSource as IList;
            if (iList != null)
            {
                iList.Insert(insertIndex, item);
                return;
            }
            
            Type type = itemsControl.ItemsSource.GetType();
            Type genericList = type.GetInterface("IList`1");
            if (genericList != null)
            {
                type.GetMethod("Insert").Invoke(itemsControl.ItemsSource, new[] {insertIndex, item});
            }
        }

        public static void RemoveItem(ItemsControl itemsControl, object itemToRemove)
        {
            if (itemToRemove == null)
                return;
            
            int index = itemsControl.Items.IndexOf(itemToRemove);
            if (index == -1)
                return;
            
            RemoveItem(itemsControl, index);
        }

        public static void RemoveItem(ItemsControl itemsControl, int removeIndex)
        {
            if (removeIndex == -1 || removeIndex >= itemsControl.Items.Count)
                return;

            if (itemsControl.ItemsSource == null)
            {
                itemsControl.Items.RemoveAt(removeIndex);
                return;
            }
            
            var iList = itemsControl.ItemsSource as IList;
            if (iList != null)
            {
                iList.RemoveAt(removeIndex);
                return;
            }
            
            Type type = itemsControl.ItemsSource.GetType();
            Type genericList = type.GetInterface("IList`1");
            if (genericList != null)
            {
                type.GetMethod("RemoveAt").Invoke(itemsControl.ItemsSource, new object[] {removeIndex});
            }
        }

        public static object GetDataObjectFromItemsControl(ItemsControl itemsControl, Point p)
        {
            var element = itemsControl.InputHitTest(p) as UIElement;
            while (element != null)
            {
                if (element == itemsControl)
                    return null;

                object data = itemsControl.ItemContainerGenerator.ItemFromContainer(element);
                if (data != DependencyProperty.UnsetValue)
                    return data;
                
                element = VisualTreeHelper.GetParent(element) as UIElement;
            }

            return null;
        }

        public static UIElement GetItemContainerFromPoint(ItemsControl itemsControl, Point p)
        {
            var element = itemsControl.InputHitTest(p) as UIElement;
            while (element != null)
            {
                if (element == itemsControl)
                    return null;

                object data = itemsControl.ItemContainerGenerator.ItemFromContainer(element);
                if (data != DependencyProperty.UnsetValue)
                    return element;
                
                element = VisualTreeHelper.GetParent(element) as UIElement;
            }

            return null;
        }

        public static int FindInsertionIndex(ItemsControl itemsControl, DragEventArgs e)
        {
            UIElement dropTargetContainer = GetItemContainerFromPoint(itemsControl, e.GetPosition(itemsControl));
            if (dropTargetContainer == null)
                return itemsControl.Items.Count;

            int index = itemsControl.ItemContainerGenerator.IndexFromContainer(dropTargetContainer);

            if (IsPointInTopHalf(itemsControl, e))
                return index;

            return index + 1;
        }

        public static bool IsPointInTopHalf(ItemsControl itemsControl, DragEventArgs e)
        {
            UIElement selectedItemContainer = GetItemContainerFromPoint(itemsControl, e.GetPosition(itemsControl));
            Point relativePosition = e.GetPosition(selectedItemContainer);
            
            if (IsItemControlOrientationHorizontal(itemsControl))
                return relativePosition.Y < ((FrameworkElement) selectedItemContainer).ActualHeight/2;
            
            return relativePosition.X < ((FrameworkElement)selectedItemContainer).ActualWidth / 2;
        }

        public static bool IsItemControlOrientationHorizontal(ItemsControl itemsControl)
        {
            if (itemsControl is TabControl)
                return false;

            return true;
        }

        public static bool? IsMousePointerAtTop(FrameworkElement element, Point position)
        {
            if (position.Y > 0.0 && position.Y < 25)
                return true;
            
            if (position.Y > element.ActualHeight - 25 && position.Y < element.ActualHeight)
                return false;
            
            return null;
        }

        public static ScrollViewer FindScrollViewer(UIElement element)
        {
            while (element != null)
            {
                if (VisualTreeHelper.GetChildrenCount(element) == 0)
                {
                    element = null;
                }
                else
                {
                    element = VisualTreeHelper.GetChild(element, 0) as UIElement;
                    if (element != null && element is ScrollViewer)
                        return element as ScrollViewer;
                }
            }
            return null;
        }
    }
}