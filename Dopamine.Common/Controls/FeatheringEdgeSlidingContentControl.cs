using System.Windows;
using System.Windows.Media;
using Digimezzo.WPFControls;
using Dopamine.Common.Presentation.Effects;
using Digimezzo.WPFControls.Enums;

namespace Dopamine.Common.Controls
{
    public class FeatheringEdgeSlidingContentControl : SlidingContentControl
    {
        #region Variables
        private Window parentWindow;
        private FeatheringEffect effect;
        #endregion

        #region Properties
        public double FeatheringRadius
        {
            get => (double)GetValue(FeatheringRadiusProperty);
            set => SetValue(FeatheringRadiusProperty, value);
        }
        #endregion

        #region Dependency Properties
        public static DependencyProperty FeatheringRadiusProperty = DependencyProperty.Register("FeatheringRadius",
            typeof(double), typeof(FeatheringEdgeSlidingContentControl), new PropertyMetadata(default(double)));
        #endregion

        #region Constructor
        static FeatheringEdgeSlidingContentControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FeatheringEdgeSlidingContentControl), new FrameworkPropertyMetadata(typeof(FeatheringEdgeSlidingContentControl)));
        }
        #endregion

        #region Override
        public override void OnApplyTemplate()
        {
            this.parentWindow = this.GetWindow(this);
            this.effect = new FeatheringEffect(){FeatheringRadius = this.FeatheringRadius};
            base.OnApplyTemplate();
        }

        protected override void BeginAnimateContentReplacement()
        {
            // Disable shader effect to optimize performance when window is maximized
            if (this.parentWindow.WindowState == WindowState.Normal)
            {
                this.effect.TexWidth = ActualWidth;
                this.Effect = effect;
            }
            var newContentTransform = new TranslateTransform();
            var oldContentTransform = new TranslateTransform();
            this.paintArea.RenderTransform = oldContentTransform;
            this.mainContent.RenderTransform = newContentTransform;
            this.paintArea.Visibility = Visibility.Visible;

            switch (this.SlideDirection)
            {
                case SlideDirection.LeftToRight:
                    newContentTransform.BeginAnimation(TranslateTransform.XProperty, this.CreateSlideAnimation(-this.ActualWidth, 0));
                    oldContentTransform.BeginAnimation(TranslateTransform.XProperty, this.CreateSlideAnimation(0, this.ActualWidth,
                        (s, e) =>
                        {
                            this.paintArea.Visibility = Visibility.Hidden;
                            this.Effect = null;
                        }));
                    break;
                case SlideDirection.RightToLeft:
                    newContentTransform.BeginAnimation(TranslateTransform.XProperty, this.CreateSlideAnimation(this.ActualWidth, 0));
                    oldContentTransform.BeginAnimation(TranslateTransform.XProperty, this.CreateSlideAnimation(0, -this.ActualWidth, (s, e) =>
                    {
                        this.paintArea.Visibility = Visibility.Hidden;
                        this.Effect = null;
                    }));
                    break;
                case SlideDirection.UpToDown:
                    newContentTransform.BeginAnimation(TranslateTransform.YProperty, this.CreateSlideAnimation(-this.ActualHeight, 0));
                    oldContentTransform.BeginAnimation(TranslateTransform.YProperty, this.CreateSlideAnimation(0, this.ActualHeight, (s, e) =>
                    {
                        this.paintArea.Visibility = Visibility.Hidden;
                        this.Effect = null;
                    }));
                    break;
                case SlideDirection.DownToUp:
                    newContentTransform.BeginAnimation(TranslateTransform.YProperty, this.CreateSlideAnimation(this.ActualHeight, 0));
                    oldContentTransform.BeginAnimation(TranslateTransform.YProperty, this.CreateSlideAnimation(0, -this.ActualHeight, (s, e) =>
                    {
                        this.paintArea.Visibility = Visibility.Hidden;
                        this.Effect = null;
                    }));
                    break;
            }

            if (this.FadeOnSlide)
            {
                this.mainContent.BeginAnimation(OpacityProperty, this.CreateFadeAnimation(0, 1, this.FadeInDuration));
                this.paintArea.BeginAnimation(OpacityProperty, this.CreateFadeAnimation(1, 0, this.FadeOutDuration));
            }
        }
        #endregion

        #region Private

        private Window GetWindow(DependencyObject dObj)
        {
            var d = VisualTreeHelper.GetParent(dObj);
            switch (d)
            {
                case Window w: return w;
                default: return GetWindow(d);
            }
        }

        #endregion
    }
}