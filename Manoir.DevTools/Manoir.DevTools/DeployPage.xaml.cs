using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.AllJoyn;
using Windows.Foundation;
using Windows.Foundation.Collections;
using static Manoir.DevTools.ToolsBll;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Manoir.DevTools
{

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DeployPage : Page
    {
        public BuildData AllData { get; set; }
        public Settings AllSettings { get; set; }


        public DeployPage()
        {
            this.AllData = BuildBll.Get();
            this.AllSettings = ToolsBll.Load();
            this.InitializeComponent();
        }

        private async void bntBuild_Click(object sender, RoutedEventArgs e)
        {
            var t = (e.OriginalSource as FrameworkElement)?.DataContext as BuildResult;
            pnlNormal.IsEnabled = false;
            pnlProgess.Visibility = Visibility.Visible;
            prgProgress.IsActive = true;
            try
            {
                await BuildBll.BuildAndDeploy(t);
            }
            catch(Exception ex)
            {
                pnlError.Message = ex.Message;
                pnlError.IsOpen = true;
            }
            finally
            {
                pnlNormal.IsEnabled = true;
                pnlProgess.Visibility = Visibility.Collapsed;
                prgProgress.IsActive = false;
            }
        }
    }
}
