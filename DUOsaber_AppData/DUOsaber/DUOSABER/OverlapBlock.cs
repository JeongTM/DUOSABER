using System;
using System.Threading;
using System.Threading.Tasks;

namespace DUOsaber
{
    public class OverlapBlock
    {
        static MainWindow mainWindow;
        public OverlapBlock(MainWindow main = null)
        {
            mainWindow = main;
        }

        public void OverlapSearch()
        {
            _ = WaitEvent();

        }
        public void OverlapEvent()
        {
            _ = PushEvent();
        }



        static async Task WaitEvent()
        {
            using (EventWaitHandle eventHandle = new EventWaitHandle(false, EventResetMode.AutoReset, "DUOSABER_Event"))
            {
                //Console.WriteLine("Second Process: Waiting for event...");
                //eventHandle.WaitOne(); // 이벤트 대기
                await Task.Run(() => eventHandle.WaitOne()); // 비동기로 이벤트 대기
                                                             // 이벤트가 발생하면 함수 호출
                await CallFunction();
                await WaitEvent();
            }
        }

        static async Task PushEvent()
        {
            using (EventWaitHandle eventHandle = new EventWaitHandle(false, EventResetMode.AutoReset, "DUOSABER_Event"))
            {
                //Console.WriteLine("First Process: Sending event...");
                await Task.Run(() => eventHandle.Set()); // 이벤트 발생
            }
        }

        static Task CallFunction()
        {
            mainWindow.ShowInTaskbar = true;
            /*mainWindow.Visibility = System.Windows.Visibility.Visible;
            mainWindow.Topmost = true;*/
            return Task.CompletedTask;
        }
    }
}
