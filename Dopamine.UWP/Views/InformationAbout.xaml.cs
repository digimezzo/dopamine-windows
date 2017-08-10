using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Dopamine.UWP.ViewModels;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Dopamine.UWP.Views
{
    public sealed partial class InformationAbout : UserControl
    {
        public InformationAboutViewModel ViewModel => (InformationAboutViewModel) this.DataContext;

        public InformationAbout()
        {
            this.InitializeComponent();
        }
    }
}
