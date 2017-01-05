using Dopamine.Common.Base;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Dopamine.Common.Presentation.Views
{
    public partial class PlaybackInfoControl : UserControl
    {
        #region Dependency Properties
        public static readonly DependencyProperty TextAlignmentProperty = DependencyProperty.Register("TextAlignment", typeof(TextAlignment), typeof(PlaybackInfoControl), new PropertyMetadata(TextAlignment.Left));
        public static readonly DependencyProperty TitleFontSizeProperty = DependencyProperty.Register("TitleFontSize", typeof(double), typeof(PlaybackInfoControl), new PropertyMetadata(Constants.GlobalFontSize));
        public static readonly DependencyProperty TitleFontWeightProperty = DependencyProperty.Register("TitleFontWeight", typeof(FontWeight), typeof(PlaybackInfoControl), new PropertyMetadata(FontWeights.Normal));
        public static readonly DependencyProperty ArtistFontSizeProperty = DependencyProperty.Register("ArtistFontSize", typeof(double), typeof(PlaybackInfoControl), new PropertyMetadata(Constants.GlobalFontSize));
        public static readonly DependencyProperty ArtistFontWeightProperty = DependencyProperty.Register("ArtistFontWeight", typeof(FontWeight), typeof(PlaybackInfoControl), new PropertyMetadata(FontWeights.Normal));
        public static readonly DependencyProperty AlbumFontSizeProperty = DependencyProperty.Register("AlbumFontSize", typeof(double), typeof(PlaybackInfoControl), new PropertyMetadata(Constants.GlobalFontSize));
        public static readonly DependencyProperty AlbumFontWeightProperty = DependencyProperty.Register("AlbumFontWeight", typeof(FontWeight), typeof(PlaybackInfoControl), new PropertyMetadata(FontWeights.Normal));
        public static readonly DependencyProperty YearFontSizeProperty = DependencyProperty.Register("YearFontSize", typeof(double), typeof(PlaybackInfoControl), new PropertyMetadata(Constants.GlobalFontSize));
        public static readonly DependencyProperty YearFontWeightProperty = DependencyProperty.Register("YearFontWeight", typeof(FontWeight), typeof(PlaybackInfoControl), new PropertyMetadata(FontWeights.Normal));
        public static readonly DependencyProperty TimeFontSizeProperty = DependencyProperty.Register("TimeFontSize", typeof(double), typeof(PlaybackInfoControl), new PropertyMetadata(Constants.GlobalFontSize));
        public static readonly DependencyProperty ShowAlbumInfoProperty = DependencyProperty.Register("ShowAlbumInfo", typeof(bool), typeof(PlaybackInfoControl), new PropertyMetadata(false));
        public static readonly DependencyProperty IsTopAlignedProperty = DependencyProperty.Register("IsTopAligned", typeof(bool), typeof(PlaybackInfoControl), new PropertyMetadata(false));
        #endregion

        #region Properties
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

        public FontWeight TitleFontWeight
        {
            get { return (FontWeight)GetValue(TitleFontWeightProperty); }

            set { SetValue(TitleFontWeightProperty, value); }
        }

        public double ArtistFontSize
        {
            get { return Convert.ToDouble(GetValue(ArtistFontSizeProperty)); }

            set { SetValue(ArtistFontSizeProperty, value); }
        }

        public FontWeight ArtistFontWeight
        {
            get { return (FontWeight)GetValue(ArtistFontWeightProperty); }

            set { SetValue(ArtistFontWeightProperty, value); }
        }

        public double AlbumFontSize
        {
            get { return Convert.ToDouble(GetValue(AlbumFontSizeProperty)); }

            set { SetValue(AlbumFontSizeProperty, value); }
        }

        public FontWeight AlbumFontWeight
        {
            get { return (FontWeight)GetValue(AlbumFontWeightProperty); }

            set { SetValue(AlbumFontWeightProperty, value); }
        }

        public double YearFontSize
        {
            get { return Convert.ToDouble(GetValue(YearFontSizeProperty)); }

            set { SetValue(YearFontSizeProperty, value); }
        }

        public FontWeight YearFontWeight
        {
            get { return (FontWeight)GetValue(YearFontWeightProperty); }

            set { SetValue(YearFontWeightProperty, value); }
        }

        public double TimeFontSize
        {
            get { return Convert.ToDouble(GetValue(TimeFontSizeProperty)); }

            set { SetValue(TimeFontSizeProperty, value); }
        }

        public bool ShowAlbumInfo
        {
            get { return Convert.ToBoolean(GetValue(ShowAlbumInfoProperty)); }

            set { SetValue(ShowAlbumInfoProperty, value); }
        }

        public bool IsTopAligned
        {
            get { return Convert.ToBoolean(GetValue(IsTopAlignedProperty)); }

            set { SetValue(IsTopAlignedProperty, value); }
        }
        #endregion

        #region Construction
        public PlaybackInfoControl()
        {
            InitializeComponent();
        }
        #endregion
    }
}
