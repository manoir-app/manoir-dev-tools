﻿using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Intrinsics.Arm;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Manoir.DevTools
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            //ExtendsContentIntoTitleBar = true;
        }

        private void myButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void nvMainNav_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            var t = (args.SelectedItemContainer.Tag as string)?.ToLowerInvariant();
            if (t == null)
                return;
            FrameNavigationOptions navOptions = new FrameNavigationOptions();
            navOptions.TransitionInfoOverride = args.RecommendedNavigationTransitionInfo;

            switch (t)
            {
                case "home":
                    DoNav(typeof(HomePage), null, navOptions) ;
                    break;
                case "local-debug":
                    DoNav(typeof(LocalDebugPage), null, navOptions);
                    break;
            }
        }

        private void DoNav(Type type, object value, FrameNavigationOptions navOptions)
        {
            var curr = frmMainNav.CurrentSourcePageType;
            if (type != curr)
                frmMainNav.NavigateToType(type, value, navOptions);
        }

        private void nvMainNav_Loaded(object sender, RoutedEventArgs e)
        {
            nvMainNav.SelectedItem = nvMainNav.MenuItems[0];
            frmMainNav.Navigate(typeof(HomePage));
        }
    }
}