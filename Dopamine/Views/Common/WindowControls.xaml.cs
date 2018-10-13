using CommonServiceLocator;
using Dopamine.Services.Shell;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Dopamine.Views.Common
{
    public partial class WindowControls : UserControl
    {
        private IShellService shellService;

        public bool EnableHighContrast
        {
            get { return Convert.ToBoolean(GetValue(EnableHighContrastProperty)); }

            set { SetValue(EnableHighContrastProperty, value); }
        }

        public static readonly DependencyProperty EnableHighContrastProperty =
            DependencyProperty.Register(nameof(EnableHighContrast), typeof(bool), typeof(WindowControls), new PropertyMetadata(false));

        public double ButtonWidth
        {
            get { return Convert.ToDouble(GetValue(ButtonWidthProperty)); }

            set { SetValue(ButtonWidthProperty, value); }
        }

        public static readonly DependencyProperty ButtonWidthProperty =
            DependencyProperty.Register(nameof(ButtonWidth), typeof(double), typeof(WindowControls), new PropertyMetadata(null));

        public double ButtonHeight
        {
            get { return Convert.ToDouble(GetValue(ButtonHeightProperty)); }

            set { SetValue(ButtonHeightProperty, value); }
        }

        public static readonly DependencyProperty ButtonHeightProperty =
            DependencyProperty.Register(nameof(ButtonHeight), typeof(double), typeof(WindowControls), new PropertyMetadata(null));

        public bool ShowMaximizeRestoreButton
        {
            get { return Convert.ToBoolean(GetValue(ShowMaximizeRestoreButtonProperty)); }

            set { SetValue(ShowMaximizeRestoreButtonProperty, value); }
        }

        public static readonly DependencyProperty ShowMaximizeRestoreButtonProperty =
            DependencyProperty.Register(nameof(ShowMaximizeRestoreButton), typeof(bool), typeof(WindowControls), new PropertyMetadata(false));

        public WindowControls()
        {
            InitializeComponent();

            this.shellService = ServiceLocator.Current.GetInstance<IShellService>();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (this.EnableHighContrast)
            {
                this.PART_TogglePlayer.SetResourceReference(StyleProperty, "WindowButtonHighContrast");
                this.PART_Minimize.SetResourceReference(StyleProperty, "WindowButtonHighContrast");
                this.PART_Maximize.SetResourceReference(StyleProperty, "WindowButtonHighContrast");
                this.PART_Restore.SetResourceReference(StyleProperty, "WindowButtonHighContrast");
                this.PART_Close.SetResourceReference(StyleProperty, "WindowButtonHighContrast");
            }
            else
            {
                this.PART_TogglePlayer.SetResourceReference(StyleProperty, "WindowButton");
                this.PART_Minimize.SetResourceReference(StyleProperty, "WindowButton");
                this.PART_Maximize.SetResourceReference(StyleProperty, "WindowButton");
                this.PART_Restore.SetResourceReference(StyleProperty, "WindowButton");
                this.PART_Close.SetResourceReference(StyleProperty, "WindowButton");
            }

            this.HandleWindowStateChange(this.shellService.WindowState);

            this.shellService.WindowStateChanged += (_, e) => this.HandleWindowStateChange(e.WindowState);
        }

        public void HandleWindowStateChange(WindowState state)
        {
            this.PART_Maximize.Visibility = state.Equals(WindowState.Maximized) ? Visibility.Collapsed : Visibility.Visible;
            this.PART_Restore.Visibility = state.Equals(WindowState.Maximized) ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
