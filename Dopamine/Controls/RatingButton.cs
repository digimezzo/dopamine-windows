using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Dopamine.Controls
{
    public class RatingButton : Control
    {
        private Button ratingButton;
        private StackPanel ratingStars;
        private StackPanel adjustmentStars;
        private TextBlock adjustmentStar1;
        private TextBlock adjustmentStar2;
        private TextBlock adjustmentStar3;
        private TextBlock adjustmentStar4;
        private TextBlock adjustmentStar5;

        public int Rating
        {
            get { return Convert.ToInt32(GetValue(RatingProperty)); }
            set { SetValue(RatingProperty, value); }
        }

        public static readonly DependencyProperty RatingProperty = 
            DependencyProperty.Register(nameof(Rating), typeof(int), typeof(RatingButton), new PropertyMetadata(null));

        public new double FontSize
        {
            get { return Convert.ToDouble(GetValue(FontSizeProperty)); }
            set { SetValue(FontSizeProperty, value); }
        }

        public static new readonly DependencyProperty FontSizeProperty =
            DependencyProperty.Register(nameof(FontSize), typeof(double), typeof(RatingButton), new PropertyMetadata(11.0));

        static RatingButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RatingButton), new FrameworkPropertyMetadata(typeof(RatingButton)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.ratingButton = (Button)GetTemplateChild("PART_RatingButton");

            this.ratingStars = (StackPanel)GetTemplateChild("PART_RatingStars");
            this.adjustmentStars = (StackPanel)GetTemplateChild("PART_AdjustmentStars");

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

        private void ApplyRating(int newRating)
        {
            if(this.Rating.Equals(newRating))
            {
                // Clear rating
                this.Rating = 0;
                return;
            }

            this.Rating = newRating;
        }

        private void RatingButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.adjustmentStar1.IsMouseOver)
            {
                this.ApplyRating(1);
            }
            else if (this.adjustmentStar2.IsMouseOver)
            {
                this.ApplyRating(2);
            }
            else if (this.adjustmentStar3.IsMouseOver)
            {
                this.ApplyRating(3);
            }
            else if (this.adjustmentStar4.IsMouseOver)
            {
                this.ApplyRating(4);
            }
            else if (this.adjustmentStar5.IsMouseOver)
            {
                this.ApplyRating(5);
            }
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
    }
}
