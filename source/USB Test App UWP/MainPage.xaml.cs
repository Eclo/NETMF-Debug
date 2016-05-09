using Microsoft.NetMicroFramework.Tools;
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
using Microsoft.SPOT.Debugger;
using System.Diagnostics;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Test_App_UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void connectButton_Click(object sender, RoutedEventArgs e)
        {
            // disable button
            (sender as Button).IsEnabled = false;

            bool connectResult = await App.NETMFUsbDebugClient.MFDevices[0].DebugEngine.ConnectAsync(3, 1000);

            var di = await App.NETMFUsbDebugClient.MFDevices[0].GetDeviceInfoAsync();

            // enable button
            (sender as Button).IsEnabled = true;
        }

        private async void button1_Click(object sender, RoutedEventArgs e)
        {
            var s = await App.NETMFUsbDebugClient.MFDevices[0].DebugEngine.SendBufferAsync(new byte[] { (byte)'x', (byte)'x' }, new System.Threading.CancellationToken());

            var r = await App.NETMFUsbDebugClient.MFDevices[0].DebugEngine.ReadBufferAsync(10, TimeSpan.FromMilliseconds(1000), new System.Threading.CancellationToken());
        }

        private async void pingButton_Click(object sender, RoutedEventArgs e)
        {
            // disable button
            (sender as Button).IsEnabled = false;
            var p = await App.NETMFUsbDebugClient.MFDevices[0].PingAsync();

            Debug.WriteLine("");
            Debug.WriteLine("");
            Debug.WriteLine("Ping response: " + p.ToString());
            Debug.WriteLine("");
            Debug.WriteLine("");

            // enable button
            (sender as Button).IsEnabled = true;
        }

        private async void printMemoryMapButton_Click(object sender, RoutedEventArgs e)
        {
            // disable button
            (sender as Button).IsEnabled = false;

            var mm = await App.NETMFUsbDebugClient.MFDevices[0].DebugEngine.MemoryMapAsync();

            Debug.WriteLine("");
            Debug.WriteLine("");
            Debug.WriteLine(mm.ToStringForOutput());
            Debug.WriteLine("");
            Debug.WriteLine("");

            // enable button
            (sender as Button).IsEnabled = true;
        }

        private async void printFlashSectorMapButton_Click(object sender, RoutedEventArgs e)
        {
            // disable button
            (sender as Button).IsEnabled = false;

            var fm = await App.NETMFUsbDebugClient.MFDevices[0].DebugEngine.GetFlashSectorMapAsync();

            Debug.WriteLine("");
            Debug.WriteLine("");
            Debug.WriteLine(fm.ToStringForOutput());
            Debug.WriteLine("");
            Debug.WriteLine("");

            // enable button
            (sender as Button).IsEnabled = true;
        }
    }
}
