using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

namespace CornellSunNewsreader.Views
{
    public partial class Colophon : PhoneApplicationPage
    {
        public Colophon()
        {
            InitializeComponent();
            LayoutRoot.DataContext = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // this will crash if there is no backstack, but that should never happen
            // just to be safe, we'll check anyway
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }
    }
}