using Pointer;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Controls;

namespace DUOsaber
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    /// 
    public partial class App : Application
    {
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr handle);

        [DllImport("user32.dll")]
        private static extern int ShowWindow(IntPtr hwnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern void BringWindowToTop(IntPtr hWnd);

        /*[DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string IpClassName, string IpWindowName);*/

        FlagSet flagSet = new FlagSet();
        private static Mutex mutex;
        bool isRun = false;

        OverlapBlock overlap = new OverlapBlock();
        protected override void OnStartup(StartupEventArgs e)
        {

            base.OnStartup(e);


            string mutexName = "DUOSABER";
            string imagePointer = "ChoisPointer2018";
            bool createNew;
            bool isImagePoint;

            mutex = new Mutex(true, mutexName, out createNew);

            Mutex imagePointerMutex = new Mutex(true, imagePointer, out isImagePoint);


            if (!isImagePoint)
            {
                foreach (Process process in Process.GetProcesses())
                {
                    if (process.ProcessName == "X-PointerV2_1.5.2")
                    {
                        process.Kill();
                    }
                }
            }

            if (!createNew)
            {
                foreach(Process process in Process.GetProcesses())
                {
                    if(process.ProcessName == Process.GetCurrentProcess().ProcessName)
                    {
                        overlap.OverlapEvent();
                        ShowWindow(process.MainWindowHandle, 4);
                        BringWindowToTop(process.MainWindowHandle);
                        SetForegroundWindow(process.MainWindowHandle);
                    }
                }
                Shutdown();
            }
            else
            {
                isRun = true;
            }
        }


        private void Application_Exit(object sender, ExitEventArgs e)
        {
            if (isRun)
            {
                flagSet.ExitFuntion();
            }
        }


    }
}
