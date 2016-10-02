using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Dopamine.Common.Controls
{
    public class SpectrumAnimation : Control
    {
        #region Variables
        private StackPanel spectrumPanel;
        private Storyboard spectrumStoryBoard;
        private DependencyPropertyDescriptor isActiveDescriptor;
        #endregion

        #region Properties
        public Brush Accent
        {
            get { return (Brush)GetValue(AccentProperty); }

            set { SetValue(AccentProperty, value); }
        }

        public bool IsActive
        {
            get { return Convert.ToBoolean(GetValue(IsActiveProperty)); }

            set { SetValue(IsActiveProperty, value); }
        }
        #endregion

        #region "Dependency Properties"
        public static readonly DependencyProperty AccentProperty = DependencyProperty.Register("Accent", typeof(Brush), typeof(SpectrumAnimation), new PropertyMetadata(new BrushConverter().ConvertFromString("#1D7DD4") as SolidColorBrush));
        public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register("IsActive", typeof(bool), typeof(SpectrumAnimation), new PropertyMetadata(null));
        #endregion

        #region Construction
        static SpectrumAnimation()
        {
            //This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
            //This style is defined in themes\generic.xaml
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SpectrumAnimation), new FrameworkPropertyMetadata(typeof(SpectrumAnimation)));
        }
        #endregion

        #region Overrides
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            
            spectrumPanel = (StackPanel)GetTemplateChild("SpectrumPanel");
            spectrumStoryBoard = (Storyboard)spectrumPanel.TryFindResource("SpectrumStoryBoard");

            this.isActiveDescriptor = DependencyPropertyDescriptor.FromProperty(SpectrumAnimation.IsActiveProperty, typeof(SpectrumAnimation));
            this.isActiveDescriptor.AddValueChanged(this, this.ValueChangedEventHandler);

            this.Unloaded += SpectrumAnimation_Unloaded;

            this.ToggleAnimation();
        }
        #endregion

        #region Private
        private void ToggleAnimation()
        {
            if (this.IsActive)
            {
                spectrumStoryBoard.Begin();
            }
            else
            {
                spectrumStoryBoard.Stop();
            }
        }

        private void SpectrumAnimation_Unloaded(object sender, RoutedEventArgs e)
        {
            // This prevents a memory leak.
            this.isActiveDescriptor.RemoveValueChanged(this, this.ValueChangedEventHandler);
        }
        #endregion

        #region Event handlers
        private void ValueChangedEventHandler(object sender, EventArgs e)
        {
            this.ToggleAnimation();
        }
        #endregion
    }
}
