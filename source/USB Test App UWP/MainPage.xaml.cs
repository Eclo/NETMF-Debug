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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Test_App_UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Engine debugEngine;

        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            //bool connectResult = await App.usbDebugClient.MFDevices[0].ConnectAsync();
            bool connectResult = await App.usbDebugClient.MFDevices[0].DebugEngine.TryToConnectAsync(3, 1000);

            //if(connectResult)
            //{
            //    //debugEngine = new Engine<MFUsbDevice>(App.usbDebugClient, App.usbDebugClient.MFDevices[0]);
            //}
        }

        private async void button1_Click(object sender, RoutedEventArgs e)
        {
            // await App.usbDebugClient.SendRawBufferAsync(new byte[] { (byte)'x', (byte)'x' }, new System.Threading.CancellationToken());


            // var r = await App.usbDebugClient.ReadRawBufferAsync(10, new System.Threading.CancellationToken());


            //App.usbDebugClient.MFDevices[0].cre

        }

        private async void button_Copy_Click(object sender, RoutedEventArgs e)
        {
            //debugEngine = new Engine<MFUsbDevice>(App.usbDebugClient, App.usbDebugClient.MFDevices[0]);

            //var p = await App.usbDebugClient.MFDevices[0].PingAsync();
            var mm = await App.usbDebugClient.MFDevices[0].DebugEngine.MemoryMapAsync();
            var dm = await App.usbDebugClient.MFDevices[0].DebugEngine.DeploymentMapAsync();
            //var oemInfo = await App.usbDebugClient.MFDevices[0].DebugEngine.GetMonitorOemInfo();
            var fsm = await App.usbDebugClient.MFDevices[0].DebugEngine.GetFlashSectorMapAsync();
            //var cs = await App.usbDebugClient.MFDevices[0].DebugEngine.CheckSignatureAsync();
            //await App.usbDebugClient.MFDevices[0].DebugEngine.RebootDeviceAsync();
            //var connect = await debugEngine.TryToConnectAsync(3, 1000);

            //var mm = await App.usbDebugClient.MFDevices[0].DebugEngine.MemoryMapAsync();

            //bool connectResult = await App.usbDebugClient.MFDevices[0].ConnectAsync();
            //var r = await App.usbDebugClient.MFDevices[0].DebugEngine.Ping();

            //var connect = await debugEngine.DummyQuery();
        }
    }
}
