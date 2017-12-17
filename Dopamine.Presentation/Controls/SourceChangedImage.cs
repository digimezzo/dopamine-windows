using System.Windows;
using System.Windows.Controls;

namespace Dopamine.Presentation.Controls
{
    public class SourceChangedImage : Image
    {
        public static readonly RoutedEvent SourceChangedEvent = EventManager.RegisterRoutedEvent("SourceChanged",
            RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(SourceChangedImage));

        static SourceChangedImage()
        {
            SourceProperty.OverrideMetadata(typeof(SourceChangedImage), new FrameworkPropertyMetadata(SourcePropertyChanged));
        }

        public event RoutedEventHandler SourceChanged = delegate { };

        private static void SourcePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            (obj as SourceChangedImage)?.RaiseEvent(new RoutedEventArgs(SourceChangedEvent));
        }
    }
}