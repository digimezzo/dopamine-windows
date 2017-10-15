using System.Windows;
using System.Windows.Controls;

namespace Dopamine.Common.Presentation.Views
{
    public partial class NowPlayingPlaybackControls : UserControl
    {
        public int SelectedNowPlayingItemIndex
        {
            get { return (int)GetValue(SelectedNowPlayingItemIndexProperty); }
            set { SetValue(SelectedNowPlayingItemIndexProperty, value); }
        }

        public static readonly DependencyProperty SelectedNowPlayingItemIndexProperty =
            DependencyProperty.Register(nameof(SelectedNowPlayingItemIndex), typeof(int), typeof(NowPlayingPlaybackControls), new PropertyMetadata(0));

        public NowPlayingPlaybackControls()
        {
            InitializeComponent();
        }
    }
}
