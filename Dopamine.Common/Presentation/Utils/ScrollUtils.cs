using Dopamine.Common.Presentation.ViewModels;
using Dopamine.Core.Utils;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Dopamine.Common.Presentation.Utils
{
    public sealed class ScrollUtils
    {
        protected static bool IsFullyVisible(FrameworkElement child, FrameworkElement scrollViewer)
        {
            GeneralTransform childTransform = child.TransformToAncestor(scrollViewer);
            Rect childRectangle = childTransform.TransformBounds(new Rect(new Point(0, 0), child.RenderSize));
            Rect ownerRectangle = new Rect(new Point(0, 0), scrollViewer.RenderSize);

            return ownerRectangle.Contains(childRectangle.TopLeft) & ownerRectangle.Contains(childRectangle.BottomRight);
        }

        public static void ScrollToListBoxItem(ListBox box, object itemObject, bool scrollOnlyIfNotInView)
        {
            // Verify that the item is not visible. Only scroll if it is not visible.
            // ----------------------------------------------------------------------
            if (scrollOnlyIfNotInView)
            {
                ScrollViewer scrollViewer = (ScrollViewer)VisualTreeUtils.GetDescendantByType(box, typeof(ScrollViewer));
                dynamic listBoxItem = (FrameworkElement)box.ItemContainerGenerator.ContainerFromItem(itemObject);

                if (scrollViewer != null && listBoxItem != null)
                {
                    if (IsFullyVisible(listBoxItem, scrollViewer))
                    {
                        return;
                    }
                }
            }

            // Scroll to the bottom of the list. This is a workaround which puts the 
            // desired Item at the top of the list when executing ScrollIntoView
            // ---------------------------------------------------------------------
            box.UpdateLayout(); // This seems required to get correct positioning.
            box.ScrollIntoView(box.Items[box.Items.Count - 1]);

            // Scroll to the desired Item
            // --------------------------
            box.UpdateLayout(); // This seems required to get correct positioning.
            box.ScrollIntoView(itemObject);
        }



        public static void ScrollToDataGridItem(DataGrid grid, object itemObject, bool scrollOnlyIfNotInView)
        {
            // Verify that the item is not visible. Only scroll if it is not visible.
            // ----------------------------------------------------------------------
            if (scrollOnlyIfNotInView)
            {
                ScrollViewer scrollViewer = (ScrollViewer)VisualTreeUtils.GetDescendantByType(grid, typeof(ScrollViewer));
                dynamic listBoxItem = (FrameworkElement)grid.ItemContainerGenerator.ContainerFromItem(itemObject);

                if (scrollViewer != null && listBoxItem != null)
                {
                    if (IsFullyVisible(listBoxItem, scrollViewer))
                    {
                        return;
                    }
                }
            }

            // Scroll to the bottom of the list. This is a workaround which puts the 
            // desired Item at the top of the list when executing ScrollIntoView
            // ---------------------------------------------------------------------
            grid.UpdateLayout(); // This seems required to get correct positioning.
            grid.ScrollIntoView(grid.Items[grid.Items.Count - 1]);

            // Scroll to the desired Item
            // --------------------------
            grid.UpdateLayout(); // This seems required to get correct positioning.
            grid.ScrollIntoView(itemObject);
        }

        public static async Task ScrollToPlayingTrackAsync(ListBox box)
        {
            if (box == null) return;

            Object itemobject = null;

            await Task.Run(() =>
            {
                try
                {
                    for (int i = 0; i <= box.Items.Count - 1; i++)
                    {
                        if (((TrackInfoViewModel)box.Items[i]).IsPlaying)
                        {
                            itemobject = box.Items[i];
                            break;
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            });

            if (itemobject == null) return;

            try
            {
                ScrollToListBoxItem(box, itemobject, true);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static async Task ScrollToPlayingTrackAsync(DataGrid grid)
        {
            if (grid == null) return;

            Object itemobject = null;

            await Task.Run(() =>
            {
                try
                {
                    for (int i = 0; i <= grid.Items.Count - 1; i++)
                    {
                        if (((TrackInfoViewModel)grid.Items[i]).IsPlaying)
                        {
                            itemobject = grid.Items[i];
                            break;
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            });

            if (itemobject == null) return;

            try
            {
                ScrollToDataGridItem(grid, itemobject, true);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
