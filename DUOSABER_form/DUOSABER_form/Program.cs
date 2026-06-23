using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DUOsaber;
namespace DUOSABER_form
{
    internal static class Program
    {
        /// <summary>
        /// 해당 애플리케이션의 주 진입점입니다.
        /// </summary>
        /// 

        private static Mutex mutex = null;

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr handle);

        [DllImport("user32.dll")]
        private static extern int ShowWindow(IntPtr hwnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern void BringWindowToTop(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [STAThread]
        static void Main()
        {
            OverlapBlock overlap = new OverlapBlock();

            const string appName = "DUOSABER";
            string imagePointer = "X-Pointer";
            bool createdNew;
            bool isImagePoint;


            mutex = new Mutex(true, appName, out createdNew);
            Mutex imagePointerMutex = new Mutex(true, imagePointer, out isImagePoint);


            if (!isImagePoint)
            {
                foreach (Process process in Process.GetProcesses())
                {
                    Console.WriteLine(process.ProcessName);
                    Console.WriteLine("-----------------");
                    if (process.ProcessName == "X-PointerV2_1.6.4")
                    {
                        process.Kill();
                    }
                }
                imagePointerMutex.Dispose();
            }
            else
            {
                imagePointerMutex.Dispose();
            }

            if (!createdNew)
            {
                foreach (Process process in Process.GetProcesses())
                {
                    if (process.ProcessName == Process.GetCurrentProcess().ProcessName)
                    {
                        overlap.OverlapEvent();
                        IntPtr notepadHandle = FindWindow(null, "DUOSABER_v1.2");
                        ShowWindow(notepadHandle, 4);
                        BringWindowToTop(notepadHandle);
                        SetForegroundWindow(notepadHandle);
                    }
                }
                return;
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormDUOSABER());
        }
    }
}
