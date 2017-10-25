using Dopamine.Common.Base;
using Prism.Mvvm;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Dopamine.Common.Presentation.Views
{
    public partial class CoverPlaybackInfoControl : UserControl
    {
        public static readonly DependencyProperty TextAlignmentProperty = DependencyProperty.Register("TextAlignment", typeof(TextAlignment), typeof(CoverPlaybackInfoControl), new PropertyMetadata(TextAlignment.Left));
        public static readonly DependencyProperty TitleFontSizeProperty = DependencyProperty.Register("TitleFontSize", typeof(double), typeof(CoverPlaybackInfoControl), new PropertyMetadata(Constants.GlobalFontSize));
        public static readonly DependencyProperty ArtistFontSizeProperty = DependencyProperty.Register("ArtistFontSize", typeof(double), typeof(CoverPlaybackInfoControl), new PropertyMetadata(Constants.GlobalFontSize));
  
        public new object DataContext
        {
            get { return base.DataContext; }
            set { base.DataContext = value; }
        }

        public TextAlignment TextAlignment
        {
            get { return (TextAlignment)GetValue(TextAlignmentProperty); }

            set { SetValue(TextAlignmentProperty, value); }
        }

        public double TitleFontSize
        {
            get { return Convert.ToDouble(GetValue(TitleFontSizeProperty)); }

            set { SetValue(TitleFontSizeProperty, value); }
        }

        public double ArtistFontSize
        {
            get { return Convert.ToDouble(GetValue(ArtistFontSizeProperty)); }

            set { SetValue(ArtistFontSizeProperty, value); }
        }
      
        public CoverPlaybackInfoControl()
        {
            InitializeComponent();
        }
    }
}
