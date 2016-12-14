using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace TOUCH_BOX
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var rnd = new Random();

            //if (rnd.NextDouble() > 0.5)
            //StartupUri = new Uri("/WpfKb.TestClient;component/MainWidow.xaml", UriKind.Relative);
            StartupUri = new Uri("Start_lotto.xaml", UriKind.Relative);

            //else
            //    StartupUri = new Uri("/WpfKb.TestClient;component/Stat_lotto.xaml", UriKind.Relative);

        }
    }


}
