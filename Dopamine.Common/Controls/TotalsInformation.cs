using System;
using System.Windows;
using System.Windows.Controls;

namespace Dopamine.Common.Controls
{
    public class TotalsInformation : Control
    {
        #region Properties
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
        #endregion

        #region Dependency Properties
        public static readonly DependencyProperty TotalDurationInformationProperty = DependencyProperty.Register("TotalDurationInformation", typeof(string), typeof(TotalsInformation), new PropertyMetadata(null));
        public static readonly DependencyProperty TotalSizeInformationProperty = DependencyProperty.Register("TotalSizeInformation", typeof(string), typeof(TotalsInformation), new PropertyMetadata(null));
        #endregion

        #region Construction
        static TotalsInformation()
        {
            //This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
            //This style is defined in themes\generic.xaml
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TotalsInformation), new FrameworkPropertyMetadata(typeof(TotalsInformation)));
        }
        #endregion
    }
}
