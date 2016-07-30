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
        private StackPanel mSpectrumPanel;
        private Storyboard mSpectrumStoryBoard;
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

            mSpectrumPanel = (StackPanel)GetTemplateChild("SpectrumPanel");
            mSpectrumStoryBoard = (Storyboard)mSpectrumPanel.TryFindResource("SpectrumStoryBoard");

            DependencyPropertyDescriptor d1 = DependencyPropertyDescriptor.FromProperty(SpectrumAnimation.IsActiveProperty, typeof(SpectrumAnimation));
            d1.AddValueChanged(this, new EventHandler((_,__) => this.ToggleAnimation()));

            this.ToggleAnimation();
        }
        #endregion

        #region Private
        private void ToggleAnimation()
        {
            if (this.IsActive)
            {
                mSpectrumStoryBoard.Begin();
            }
            else
            {
                mSpectrumStoryBoard.Stop();
            }
        }
        #endregion
    }
}
