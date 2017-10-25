using System;
using System.Windows;
using System.Windows.Controls;

namespace Dopamine.Common.Controls
{
    public class TotalsInformation : Control
    {
        public string TotalDurationInformation
        {
            get { return Convert.ToString(GetValue(TotalDurationInformationProperty)); }

            set { SetValue(TotalDurationInformationProperty, value); }
        }

        public string TotalSizeInformation
        {
            get { return Convert.ToString(GetValue(TotalSizeInformationProperty)); }

            set { SetValue(TotalSizeInformationProperty, value); }
        }

        public static readonly DependencyProperty TotalDurationInformationProperty = DependencyProperty.Register("TotalDurationInformation", typeof(string), typeof(TotalsInformation), new PropertyMetadata(null));
        public static readonly DependencyProperty TotalSizeInformationProperty = DependencyProperty.Register("TotalSizeInformation", typeof(string), typeof(TotalsInformation), new PropertyMetadata(null));

        static TotalsInformation()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TotalsInformation), new FrameworkPropertyMetadata(typeof(TotalsInformation)));
        }
    }
}
