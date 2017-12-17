using System;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Dopamine.Presentation.Controls
{
    public class TransitioningContentControl : ContentControl
    {
        private Timer timer;

        public static readonly DependencyProperty FadeInProperty = DependencyProperty.Register("FadeIn", typeof(bool), typeof(TransitioningContentControl), new PropertyMetadata(null));
        public static readonly DependencyProperty FadeInTimeoutProperty = DependencyProperty.Register("FadeInTimeout", typeof(double), typeof(TransitioningContentControl), new PropertyMetadata(null));
        public static readonly DependencyProperty SlideInProperty = DependencyProperty.Register("SlideIn", typeof(bool), typeof(TransitioningContentControl), new PropertyMetadata(null));
        public static readonly DependencyProperty SlideInTimeoutProperty = DependencyProperty.Register("SlideInTimeout", typeof(double), typeof(TransitioningContentControl), new PropertyMetadata(null));
        public static readonly DependencyProperty SlideInFromProperty = DependencyProperty.Register("SlideInFrom", typeof(int), typeof(TransitioningContentControl), new PropertyMetadata(null));
        public static readonly DependencyProperty SlideInToProperty = DependencyProperty.Register("SlideInTo", typeof(int), typeof(TransitioningContentControl), new PropertyMetadata(null));
        public static readonly DependencyProperty RightToLeftProperty = DependencyProperty.Register("RightToLeft", typeof(bool), typeof(TransitioningContentControl), new PropertyMetadata(null));

        public static readonly RoutedEvent ContentChangedEvent = EventManager.RegisterRoutedEvent("ContentChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TransitioningContentControl));
        
        public event RoutedEventHandler ContentChanged
        {
            add { this.AddHandler(ContentChangedEvent, value); }

            remove { this.RemoveHandler(ContentChangedEvent, value); }


        }
        private void RaiseContentChangedEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(TransitioningContentControl.ContentChangedEvent);
            base.RaiseEvent(newEventArgs);
        }

        public bool FadeIn
        {
            get { return Convert.ToBoolean(GetValue(FadeInProperty)); }

            set { SetValue(FadeInProperty, value); }
        }

        public double FadeInTimeout
        {
            get { return Convert.ToDouble(GetValue(FadeInTimeoutProperty)); }

            set { SetValue(FadeInTimeoutProperty, value); }
        }

        public bool SlideIn
        {
            get { return Convert.ToBoolean(GetValue(SlideInProperty)); }

            set { SetValue(SlideInProperty, value); }
        }

        public double SlideInTimeout
        {
            get { return Convert.ToDouble(GetValue(SlideInTimeoutProperty)); }

            set { SetValue(SlideInTimeoutProperty, value); }
        }

        public int SlideInFrom
        {
            get { return Convert.ToInt32(GetValue(SlideInFromProperty)); }

            set { SetValue(SlideInFromProperty, value); }
        }

        public int SlideInTo
        {
            get { return Convert.ToInt32(GetValue(SlideInToProperty)); }

            set { SetValue(SlideInToProperty, value); }
        }

        public bool RightToLeft
        {
            get { return Convert.ToBoolean(GetValue(RightToLeftProperty)); }

            set { SetValue(RightToLeftProperty, value); }
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            this.DoAnimation();
        }

        private void DoAnimation()
        {
            if (this.FadeInTimeout != null && this.FadeIn)
            {
                var da = new DoubleAnimation();
                da.From = 0;
                da.To = 1;
                da.Duration = new Duration(TimeSpan.FromSeconds(this.FadeInTimeout));
                this.BeginAnimation(OpacityProperty, da);
            }


            if (this.SlideInTimeout != null && this.SlideInTimeout > 0 && this.SlideIn)
            {
                if (!this.RightToLeft)
                {
                    var ta = new ThicknessAnimation();
                    ta.From = new Thickness(this.SlideInFrom, this.Margin.Top, 2 * this.SlideInTo - this.SlideInFrom, this.Margin.Bottom);
                    ta.To = new Thickness(this.SlideInTo, this.Margin.Top, this.SlideInTo, this.Margin.Bottom);
                    ta.Duration = new Duration(TimeSpan.FromSeconds(this.SlideInTimeout));
                    this.BeginAnimation(MarginProperty, ta);
                }
                else
                {
                    var ta = new ThicknessAnimation();
                    ta.From = new Thickness(2 * this.SlideInTo - this.SlideInFrom, this.Margin.Top, this.SlideInFrom, this.Margin.Bottom);
                    ta.To = new Thickness(this.SlideInTo, this.Margin.Top, this.SlideInTo, this.Margin.Bottom);
                    ta.Duration = new Duration(TimeSpan.FromSeconds(this.SlideInTimeout));
                    this.BeginAnimation(MarginProperty, ta);
                }
            }

            if (this.timer != null)
            {
                this.timer.Stop();
                this.timer.Elapsed -= new ElapsedEventHandler(this.TimerElapsedHandler);
            }

            this.timer = new Timer();

            double biggestTimeout = this.SlideInTimeout;

            if (this.FadeInTimeout > this.SlideInTimeout)
            {
                biggestTimeout = this.FadeInTimeout;
            }

            this.timer.Interval = TimeSpan.FromSeconds(biggestTimeout).TotalMilliseconds;

            this.timer.Elapsed += new ElapsedEventHandler(this.TimerElapsedHandler);

            this.timer.Start();
        }

        private void TimerElapsedHandler(object sender, ElapsedEventArgs e)
        {
            this.timer.Stop();

            try
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() => this.RaiseContentChangedEvent()));
            }
            catch (Exception)
            {
            }
        }
    }
}
