using Digimezzo.WPFControls;
using Dopamine.Common.Base;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Dopamine.Common.Controls
{
    public class DopamineWindow : BorderlessWindows8Window
    {
        private bool oldTopMost;

        public Brush Accent
        {
            get { return (Brush)GetValue(AccentProperty); }

            set { SetValue(AccentProperty, value); }
        }

        public static readonly DependencyProperty AccentProperty = DependencyProperty.Register("Accent", typeof(Brush), typeof(DopamineWindow), new PropertyMetadata(null));

        static DopamineWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DopamineWindow), new FrameworkPropertyMetadata(typeof(DopamineWindow)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.TitleBarHeight = Convert.ToInt32(Constants.DefaultWindowButtonHeight);
            this.InitializeWindow();
        }

        public void ForceActivate()
        {
            // Prevent calling Activate() before Show() was called. Otherwise Activate() fails 
            // with an exception: "Cannot call DragMove or Activate before a Window is shown".
            if (!this.IsLoaded)
            {
                return;
            }

            this.oldTopMost = this.Topmost;

            if (this.WindowState == WindowState.Minimized)
            {
                this.WindowState = WindowState.Normal;
            }

            this.Activate();
            this.Topmost = true;
            this.Deactivate();
        }

        private async void Deactivate()
        {
            await Task.Delay(500);
            Application.Current.Dispatcher.Invoke(() => this.Topmost = this.oldTopMost);
        }
    }
}
