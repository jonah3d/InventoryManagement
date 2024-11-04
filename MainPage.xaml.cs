using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace InventoryManagement
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private const int WinWidth = 1024;
        private const int WinHeight = 768;

        public MainPage()
        {
            this.InitializeComponent();
            var view = ApplicationView.GetForCurrentView();
            var desiredSize = new Size(WinWidth, WinHeight);
            view.TryResizeView(desiredSize);

        }

        private void btn_newgame_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Game));
        }
    }
}
