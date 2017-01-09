using Digimezzo.Utilities.Settings;
using Digimezzo.WPFControls;
using Dopamine.Common.Base;
using System;
using System.Windows;
using System.Windows.Media;

namespace Dopamine.Common.Controls
{
    public class DopamineWindow : BorderlessWindows8Window
    {
        #region Variables
        private bool oldTopMost;
        private bool hasBorder;
        #endregion

        #region Properties
        public Brush Accent
        {
            get { return (Brush)GetValue(AccentProperty); }

            set { SetValue(AccentProperty, value); }
        }

        public bool HasBorder
        {
            get { return this.hasBorder; }
        }
        #endregion

        #region Dependency Properties
        public static readonly DependencyProperty AccentProperty = DependencyProperty.Register("Accent", typeof(Brush), typeof(DopamineWindow), new PropertyMetadata(null));
        #endregion

        #region Construction
        static DopamineWindow()
        {
            //This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
            //This style is defined in themes\generic.xaml
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DopamineWindow), new FrameworkPropertyMetadata(typeof(DopamineWindow)));
        }
        #endregion

        #region Overrides
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.TitleBarHeight = Convert.ToInt32(Constants.DefaultWindowButtonHeight);
            this.SetWindowBorder(SettingsClient.Get<bool>("Appearance", "ShowWindowBorder"));
            this.InitializeWindow();

            base.MinimizeToolTipChanged += BorderlessWindowBase_MinimizeToolTipChanged;
            base.MaximizeToolTipChanged += BorderlessWindowBase_MaximizeRestoreToolTipChanged;
            base.RestoreToolTipChanged += BorderlessWindowBase_MaximizeRestoreToolTipChanged;
            base.CloseToolTipChanged += BorderlessWindowBase_CloseToolTipChanged;
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);

            this.SetWindowBorder(this.hasBorder);
        }

        protected override void BorderlessWindowBase_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            base.BorderlessWindowBase_SizeChanged(sender, e);
        }
        #endregion

        #region Event Handlers
        private void BorderlessWindowBase_MinimizeToolTipChanged(object sender, EventArgs e)
        {
            this.minimizeButton.ToolTip = this.MinimizeToolTip;
        }


        private void BorderlessWindowBase_MaximizeRestoreToolTipChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.maximizeButton.ToolTip = this.RestoreToolTip;
            }
            else
            {
                this.maximizeButton.ToolTip = this.MaximizeToolTip;
            }
        }

        private void BorderlessWindowBase_CloseToolTipChanged(object sender, EventArgs e)
        {
            this.closeButton.ToolTip = this.CloseToolTip;
        }
        #endregion

        #region Public
        /// <summary>
        /// Custom Activate function because the real Activate function doesn't always bring the window on top.
        /// </summary>
        /// <remarks></remarks>
        public void ActivateNow()
        {
            // Prevent calling Activate() before Show() was called. Otherwise Activate() fails 
            // with an exception: "Cannot call DragMove or Activate before a Window is shown".
            if (!this.IsLoaded) return;

            this.oldTopMost = this.Topmost;

            if (this.WindowState == WindowState.Minimized)
            {
                this.WindowState = WindowState.Normal;
            }

            this.Activate();
            this.Topmost = true;

            System.Threading.Thread t = new System.Threading.Thread(Deactivate);
            t.Start();
        }

        /// <summary>
        /// Sets the border
        /// </summary>
        /// <remarks></remarks>
        public void SetWindowBorder(bool hasBorder)
        {
            this.hasBorder = hasBorder;

            if (this.windowBorder == null) return;

            if (this.WindowState == WindowState.Maximized)
            {
                this.SetBorderThickness(new Thickness(6));
            }
            else
            {
                if (this.HasBorder)
                {
                    this.SetBorderThickness(new Thickness(1));
                }
                else
                {
                    this.SetBorderThickness(new Thickness(0));
                }
            }
        }
        #endregion

        #region Private
        private void SetBorderThickness(Thickness borderThickness)
        {
            this.windowBorder.BorderThickness = borderThickness;
            this.previousBorderThickness = borderThickness;
        }

        /// <summary>
        /// The Deactivate function which goes together with ActivateNow
        /// </summary>
        /// <remarks></remarks>
        private void Deactivate()
        {
            System.Threading.Thread.Sleep(500);
            Application.Current.Dispatcher.Invoke(() => this.Topmost = this.oldTopMost);
            System.Threading.Thread.CurrentThread.Abort();
        }
        #endregion
    }
}
