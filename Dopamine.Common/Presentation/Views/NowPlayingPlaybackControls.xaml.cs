using System.Windows;
using System.Windows.Controls;

namespace Dopamine.Common.Presentation.Views
{
    public partial class NowPlayingPlaybackControls : UserControl
    {
        public int SelectedNowPlayingSubPageIndex
        {
            get { return (int)GetValue(SelectedNowPlayingSubPageIndexProperty); }
            set { SetValue(SelectedNowPlayingSubPageIndexProperty, value); }
        }

        public static readonly DependencyProperty SelectedNowPlayingSubPageIndexProperty =
            DependencyProperty.Register(nameof(SelectedNowPlayingSubPageIndex), typeof(int), typeof(NowPlayingPlaybackControls), new PropertyMetadata(0, new PropertyChangedCallback(SelectedNowPlayingSubPageIndexPropertyChanged)));

        private static void SelectedNowPlayingSubPageIndexPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // TODO: save setting
        }

        public NowPlayingPlaybackControls()
        {
            InitializeComponent();
            // TODO: load from setting
        }
    }
}
