using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Manoir.DevTools
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public ToolsBll.Settings AllSettings { get; set; }
        DispatcherTimer _tmr;

        public SettingsPage()
        {
            AllSettings = ToolsBll.Load();
            this.InitializeComponent();
            _tmr = new DispatcherTimer();
            _tmr.Interval = TimeSpan.FromMilliseconds(250);
            _tmr.Tick += _tmr_Tick;
        }

        private void _tmr_Tick(object sender, object e)
        {
            _tmr.Stop();
            ToolsBll.Save(AllSettings);
        }

        private void SomeText_TextChanged(object sender, TextChangedEventArgs e)
        {
            _tmr.Stop();
            _tmr.Start();
        }

        private void btnAddEnv_Click(object sender, RoutedEventArgs e)
        {
            AllSettings.Add(new SettingsEnvironnement()
            {
                Name = "NEW"
            });
            _tmr.Stop();
            _tmr.Start();
        }

        private void btnSuppr_Click(object sender, RoutedEventArgs e)
        {
            var t = (e.OriginalSource as FrameworkElement)?.DataContext as SettingsEnvironnement;
            if (t != null)
            {
                AllSettings.Remove(t);
                _tmr.Stop();
                _tmr.Start();
            }
        }
    }
}
