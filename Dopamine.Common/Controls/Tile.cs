using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Dopamine.Common.Controls
{
    public class Tile : Label
    {
        #region Variables
        private Border mTile;
        #endregion

        #region Properties
        [EditorBrowsable(EditorBrowsableState.Never)]
        public double IconSize
        {
            get { return Convert.ToDouble(GetValue(IconSizeProperty)); }

            set { SetValue(IconSizeProperty, value); }
        }

        public double IconSizePercent
        {
            get { return Convert.ToDouble(GetValue(IconSizePercentProperty)); }

            set { SetValue(IconSizePercentProperty, value); }
        }
        #endregion

        #region Dependency properties
        public static readonly DependencyProperty IconSizeProperty = DependencyProperty.Register("IconSize", typeof(double), typeof(Tile), new PropertyMetadata(null));
        public static readonly DependencyProperty IconSizePercentProperty = DependencyProperty.Register("IconSizePercent", typeof(double), typeof(Tile), new PropertyMetadata(null));
        #endregion


        #region Construction
        static Tile()
        {
            //This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
            //This style is defined in themes\generic.xaml
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Tile), new FrameworkPropertyMetadata(typeof(Tile)));
        }
        #endregion

        #region Overrides
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.mTile = (Border)GetTemplateChild("PART_Tile");

            if (this.mTile != null)
            {
                this.SetIconSize(this.mTile);

                this.mTile.SizeChanged += SizeChangedHandler;
            }
        }
        #endregion

        #region Event Handlers
        private void SizeChangedHandler(object sender, SizeChangedEventArgs e)
        {
            this.SetIconSize((Border)sender);
        }
         #endregion

        #region Private
        private void SetIconSize(Border iTile)
        {
            try
            {
                // For some reason, "ActualHeight" is always 0 when arriving here, so we use "Height"
                this.IconSize = iTile.Height * (this.IconSizePercent / 100);
            }
            catch (Exception)
            {
                this.IconSize = 0;
            }
        }
        #endregion
    }
}
