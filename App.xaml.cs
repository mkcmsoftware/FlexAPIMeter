using Flex.Smoothlake.FlexLib;
using System.Windows;

namespace FlexApiMeter
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            API.ProgramName = "FlexApiMeter";
            API.IsGUI = false;
            API.Init();
        }
    }
}
