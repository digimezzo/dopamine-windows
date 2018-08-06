using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Dopamine.Controls
{
    public class SearchBox : TextBox
    {
        private TextBlock searchHint;
        private Border searchBorder;
        private ScrollViewer contentHost;
        private bool hasKeyboardFocus;

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

        public static readonly DependencyProperty HasTextProperty = 
            DependencyProperty.Register(nameof(HasText), typeof(bool), typeof(SearchBox), new PropertyMetadata(false));
        public static readonly DependencyProperty HasFocusProperty = 
            DependencyProperty.Register(nameof(HasFocus), typeof(bool), typeof(SearchBox), new PropertyMetadata(false));

        static SearchBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SearchBox), new FrameworkPropertyMetadata(typeof(SearchBox)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.searchHint = (TextBlock)GetTemplateChild("PART_SearchHint");
            this.searchBorder = (Border)GetTemplateChild("PART_SearchBorder");
            this.contentHost = (ScrollViewer)GetTemplateChild("PART_ContentHost");

            if (this.searchBorder != null)
            {
                this.searchBorder.MouseLeftButtonUp += SearchButton_MouseLeftButtonUphandler;
            }

            if(this.contentHost != null)
            {
                this.contentHost.PreviewMouseLeftButtonUp += ContentHost_MouseLeftButtonUp;
                this.contentHost.PreviewLostKeyboardFocus += ContentHost_PreviewLostKeyboardFocus;
                this.contentHost.PreviewGotKeyboardFocus += ContentHost_PreviewGotKeyboardFocus;
            }
        }

        private void ContentHost_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            this.hasKeyboardFocus = true;
        }

        private void ContentHost_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            this.hasKeyboardFocus = false;

            if (!this.HasText)
            {
                this.searchHint.Visibility = Visibility.Visible;
            }
        }

        private void ContentHost_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.searchHint.Visibility = Visibility.Collapsed;
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);
            this.HasText = this.Text.Length > 0;
        }

        private void SearchButton_MouseLeftButtonUphandler(object sender, MouseButtonEventArgs e)
        {
            if (this.HasText)
            {
                this.Text = string.Empty;

                if (!this.hasKeyboardFocus)
                {
                    this.searchHint.Visibility = Visibility.Visible;
                }
            }
        }

        public void SetKeyboardFocus()
        {
            this.Focus();

            // This function is activated by a keyboard shortcut. So, search boxes which have not yet been initialized 
            // can try to call this function. If that happens, searchHint is null. We're trying to avoid a crash here.
            if (this.IsVisible && this.searchHint != null)
            {
                this.searchHint.Visibility = Visibility.Collapsed;
            }
        }
    }
}
