using System;
using System.Windows;
using System.Windows.Media;

namespace Dopamine.Core.Utils
{
    public static class VisualTreeUtils
    {
        public static T FindAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            do
            {
                if (current is T)
                {
                    return (T)current;
                }

                current = VisualTreeHelper.GetParent(current);

            } while (current != null);

            return null;
        }

        public static Visual GetDescendantByType(Visual element, Type type)
        {
            if (element == null)
            {
                return null;
            }
            if (element.GetType() == type)
            {
                return element;
            }

            Visual foundElement = null;

            if (element is FrameworkElement)
            {
                (element as FrameworkElement).ApplyTemplate();
            }

            for (int i = 0; i <= VisualTreeHelper.GetChildrenCount(element) - 1; i++)
            {
                Visual visual = VisualTreeHelper.GetChild(element, i) as Visual;
                foundElement = GetDescendantByType(visual, type);

                if (foundElement != null)
                {
                    break;
                }
            }

            return foundElement;
        }

    }
}
