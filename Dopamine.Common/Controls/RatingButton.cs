using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Dopamine.Common.Controls
{
    public class RatingButton : Control
    {
        #region Variables
        private Button ratingButton;
        private StackPanel ratingStars;
        private StackPanel adjustmentStars;
        private TextBlock adjustmentStar0;
        private TextBlock adjustmentStar1;
        private TextBlock adjustmentStar2;
        private TextBlock adjustmentStar3;
        private TextBlock adjustmentStar4;
        private TextBlock adjustmentStar5;
        #endregion

        #region Properties
        public int Rating
        {
            get { return Convert.ToInt32(GetValue(RatingProperty)); }
            set { SetValue(RatingProperty, value); }
        }
        #endregion

        #region Dependency Properties
        public static readonly DependencyProperty RatingProperty = DependencyProperty.Register("Rating", typeof(int), typeof(RatingButton), new PropertyMetadata(null));
        #endregion

        #region Construction
        static RatingButton()
        {
            //This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
            //This style is defined in themes\generic.xaml
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RatingButton), new FrameworkPropertyMetadata(typeof(RatingButton)));
        }
        #endregion

        #region Overrides
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.ratingButton = (Button)GetTemplateChild("PART_RatingButton");

            this.ratingStars = (StackPanel)GetTemplateChild("PART_RatingStars");
            this.adjustmentStars = (StackPanel)GetTemplateChild("PART_AdjustmentStars");

            this.adjustmentStar0 = (TextBlock)GetTemplateChild("PART_AdjustmentStar0");
            this.adjustmentStar1 = (TextBlock)GetTemplateChild("PART_AdjustmentStar1");
            this.adjustmentStar2 = (TextBlock)GetTemplateChild("PART_AdjustmentStar2");
            this.adjustmentStar3 = (TextBlock)GetTemplateChild("PART_AdjustmentStar3");
            this.adjustmentStar4 = (TextBlock)GetTemplateChild("PART_AdjustmentStar4");
            this.adjustmentStar5 = (TextBlock)GetTemplateChild("PART_AdjustmentStar5");

            if (this.ratingButton != null)
            {
                this.ratingButton.Click += RatingButton_Click;
                this.ratingButton.MouseEnter += RatingButton_MouseEnter;
                this.ratingButton.MouseLeave += RatingButton_MouseLeave;
                this.ratingButton.PreviewMouseDoubleClick += RatingButton_PreviewMouseDoubleClick;
            }

            if (this.adjustmentStar0 != null)
            {
                this.adjustmentStar0.MouseEnter += AdjustmentStar0_MouseEnter;
            }

            if (this.adjustmentStar1 != null)
            {
                this.adjustmentStar1.MouseEnter += AdjustmentStar1_MouseEnter;
            }

            if (this.adjustmentStar2 != null)
            {
                this.adjustmentStar2.MouseEnter += AdjustmentStar2_MouseEnter;
            }

            if (this.adjustmentStar3 != null)
            {
                this.adjustmentStar3.MouseEnter += AdjustmentStar3_MouseEnter;
            }

            if (this.adjustmentStar4 != null)
            {
                this.adjustmentStar4.MouseEnter += AdjustmentStar4_MouseEnter;
            }

            if (this.adjustmentStar5 != null)
            {
                this.adjustmentStar5.MouseEnter += AdjustmentStar5_MouseEnter;
            }

            this.ratingStars.Visibility = Visibility.Visible;
            this.adjustmentStars.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region Event Handlers
        private void RatingButton_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // This prevents other double click actions while rating, like playing the selected song.
            e.Handled = true;
        }

        private void RatingButton_MouseEnter(object sender, MouseEventArgs e)
        {
            this.ratingStars.Visibility = Visibility.Hidden;
            this.adjustmentStars.Visibility = Visibility.Visible;
        }

        private void RatingButton_MouseLeave(object sender, MouseEventArgs e)
        {
            this.ratingStars.Visibility = Visibility.Visible;
            this.adjustmentStars.Visibility = Visibility.Hidden;
        }

        private void RatingButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.adjustmentStar0.IsMouseOver)
            {
                this.Rating = 0;
            }
            else if (this.adjustmentStar1.IsMouseOver)
            {
                this.Rating = 1;
            }
            else if (this.adjustmentStar2.IsMouseOver)
            {
                this.Rating = 2;
            }
            else if (this.adjustmentStar3.IsMouseOver)
            {
                this.Rating = 3;
            }
            else if (this.adjustmentStar4.IsMouseOver)
            {
                this.Rating = 4;
            }
            else if (this.adjustmentStar5.IsMouseOver)
            {
                this.Rating = 5;
            }
        }

        private void AdjustmentStar0_MouseEnter(object sender, MouseEventArgs e)
        {
            this.adjustmentStar1.Opacity = 0.2;
            this.adjustmentStar2.Opacity = 0.2;
            this.adjustmentStar3.Opacity = 0.2;
            this.adjustmentStar4.Opacity = 0.2;
            this.adjustmentStar5.Opacity = 0.2;
        }


        private void AdjustmentStar1_MouseEnter(object sender, MouseEventArgs e)
        {
            this.adjustmentStar1.Opacity = 1.0;
            this.adjustmentStar2.Opacity = 0.2;
            this.adjustmentStar3.Opacity = 0.2;
            this.adjustmentStar4.Opacity = 0.2;
            this.adjustmentStar5.Opacity = 0.2;
        }

        private void AdjustmentStar2_MouseEnter(object sender, MouseEventArgs e)
        {
            this.adjustmentStar1.Opacity = 1.0;
            this.adjustmentStar2.Opacity = 1.0;
            this.adjustmentStar3.Opacity = 0.2;
            this.adjustmentStar4.Opacity = 0.2;
            this.adjustmentStar5.Opacity = 0.2;
        }

        private void AdjustmentStar3_MouseEnter(object sender, MouseEventArgs e)
        {
            this.adjustmentStar1.Opacity = 1.0;
            this.adjustmentStar2.Opacity = 1.0;
            this.adjustmentStar3.Opacity = 1.0;
            this.adjustmentStar4.Opacity = 0.2;
            this.adjustmentStar5.Opacity = 0.2;
        }

        private void AdjustmentStar4_MouseEnter(object sender, MouseEventArgs e)
        {
            this.adjustmentStar1.Opacity = 1.0;
            this.adjustmentStar2.Opacity = 1.0;
            this.adjustmentStar3.Opacity = 1.0;
            this.adjustmentStar4.Opacity = 1.0;
            this.adjustmentStar5.Opacity = 0.2;
        }

        private void AdjustmentStar5_MouseEnter(object sender, MouseEventArgs e)
        {
            this.adjustmentStar1.Opacity = 1.0;
            this.adjustmentStar2.Opacity = 1.0;
            this.adjustmentStar3.Opacity = 1.0;
            this.adjustmentStar4.Opacity = 1.0;
            this.adjustmentStar5.Opacity = 1.0;
        }
        #endregion
    }
}
