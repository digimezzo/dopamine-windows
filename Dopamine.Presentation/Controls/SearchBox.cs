using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Dopamine.Presentation.Controls
{
    public class SearchBox : TextBox
    {
        private TextBlock searchIconCross;
        private TextBlock searchIconGlass;
        private Border searchBorder;

        public bool HasText
        {
            get { return Convert.ToBoolean(GetValue(HasTextProperty)); }

            set { SetValue(HasTextProperty, value); }
        }

        public bool HasFocus
        {
            get { return Convert.ToBoolean(GetValue(HasFocusProperty)); }

            set { SetValue(HasFocusProperty, value); }
        }

        public Brush Accent
        {
            get { return (Brush)GetValue(AccentProperty); }

            set { SetValue(AccentProperty, value); }
        }

        public Brush SearchGlassForeground
        {
            get { return (Brush)GetValue(SearchGlassForegroundProperty); }

            set { SetValue(SearchGlassForegroundProperty, value); }
        }

        public Brush VisibleBackground
        {
            get { return (Brush)GetValue(VisibleBackgroundProperty); }

            set { SetValue(VisibleBackgroundProperty, value); }
        }

        public static readonly DependencyProperty HasTextProperty = DependencyProperty.Register("HasText", typeof(bool), typeof(SearchBox), new PropertyMetadata(false));
        public static readonly DependencyProperty HasFocusProperty = DependencyProperty.Register("HasFocus", typeof(bool), typeof(SearchBox), new PropertyMetadata(false));
        public static readonly DependencyProperty AccentProperty = DependencyProperty.Register("Accent", typeof(Brush), typeof(SearchBox), new PropertyMetadata(null));
        public static readonly DependencyProperty SearchGlassForegroundProperty = DependencyProperty.Register("SearchGlassForeground", typeof(Brush), typeof(SearchBox), new PropertyMetadata(null));
        public static readonly DependencyProperty VisibleBackgroundProperty = DependencyProperty.Register("VisibleBackground", typeof(Brush), typeof(SearchBox), new PropertyMetadata(null));

        static SearchBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SearchBox), new FrameworkPropertyMetadata(typeof(SearchBox)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.searchIconCross = (TextBlock)GetTemplateChild("PART_SearchIconCross");
            this.searchIconGlass = (TextBlock)GetTemplateChild("PART_SearchIconGlass");
            this.searchBorder = (Border)GetTemplateChild("PART_SearchBorder");

            if (this.searchBorder != null)
            {
                this.searchBorder.MouseLeftButtonUp += SearchButton_MouseLeftButtonUphandler;
            }

            this.SetButtonState();
        }


        private void SetButtonState()
        {

            if (this.searchIconCross != null && this.searchIconGlass != null)
            {
                if (this.HasText)
                {
                    this.searchIconCross.Visibility = Visibility.Visible;
                    this.searchIconGlass.Visibility = Visibility.Collapsed;
                }
                else
                {
                    this.searchIconCross.Visibility = Visibility.Collapsed;
                    this.searchIconGlass.Visibility = Visibility.Visible;
                }
            }
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);
            this.HasText = this.Text.Length > 0;
            this.SetButtonState();
        }

        private void SearchButton_MouseLeftButtonUphandler(object sender, MouseButtonEventArgs e)
        {
            if (this.HasText)
            {
                this.Text = string.Empty;
            }
            this.SetButtonState();
        }
    }
}
