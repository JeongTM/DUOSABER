using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Pointer
{
    public partial class MagnifierGlass : IDisposable
    {
        delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        private WndProc delegWndProc;
        private Timer timer;
        private float magnification = 0.0f;
        private int timerInterval = 60;

        IntPtr hInstance;
        private IntPtr hwndMag;
        private IntPtr hWnd;
        private RECT magWindowRect;

        private int width;
        private int height;
        private int sourceWidth;
        private int sourceHeight;

        private Screen[] sc;

        public bool define_layer = true;

        public MagnifierGlass()
        {
            // Windows 11 DPI 인식 설정 추가
            SetProcessDpiAwareness();

            hInstance = Process.GetCurrentProcess().Handle;
            delegWndProc = myWndProc;

            if (SetupMagnifier() == false)
            {
                return;
            }

            Magnification = 2.0f;
            updateDimensions();

            timer = new Timer();
            timer.Tick += new EventHandler(timer_Tick);
            timer.Interval = timerInterval;
            timer.Enabled = false;
        }

        // Windows 11 DPI 인식 설정 메서드 추가
        private void SetProcessDpiAwareness()
        {
            try
            {
                // Windows 10 버전 1703 이상에서 사용 가능한 새로운 API
                if (Environment.OSVersion.Version.Major >= 10)
                {
                    // Per-Monitor V2 DPI 인식 설정 시도
                    if (SetProcessDpiAwarenessContext(DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2) == false)
                    {
                        // 실패 시 기본 Per-Monitor DPI 인식 설정
                        SetProcessDpiAwareness(PROCESS_DPI_AWARENESS.Process_Per_Monitor_DPI_Aware);
                    }
                }
                else
                {
                    // 이전 버전용 설정
                    SetProcessDpiAwareness(PROCESS_DPI_AWARENESS.Process_Per_Monitor_DPI_Aware);
                }
            }
            catch (Exception ex)
            {
                // DPI 설정 실패 시 로그 출력
                Console.WriteLine($"Failed to set dpi awareness.: {ex.Message}");
            }
        }

        private void updateDimensions()
        {
            NativeMethods.GetClientRect(hWnd, ref magWindowRect);
            width = height = magWindowRect.right - magWindowRect.left;
            sourceWidth = sourceHeight = (int)(width / magnification);
        }

        public void MagnifierOn()
        {
            POINT mousePoint = new POINT();
            RECT sourceRect = new RECT();

            NativeMethods.GetCursorPos(ref mousePoint);

            sc = Screen.AllScreens;
            int CurState = GetCurrentScreenIndex(mousePoint);

            int magnifierXpos = mousePoint.x - width / 2;

            sourceRect.left = mousePoint.x - (int)(sourceWidth / 2);
            sourceRect.top = mousePoint.y - (int)(sourceHeight / 2);
            sourceRect.right = sourceRect.left + sourceWidth;
            sourceRect.bottom = sourceRect.top + sourceHeight;

            // 화면 경계 체크 및 조정
            AdjustSourceRect(ref sourceRect, CurState);

            NativeMethods.MagSetWindowSource(hwndMag, sourceRect);

            if (define_layer)
            {
                SetWindowPos(hWnd, HWND_TOPMOST,
                    magnifierXpos,
                    mousePoint.y - (int)(height / 2),
                    width,
                    height,
                    SetWindowPosFlags.SWP_NOACTIVATE | SetWindowPosFlags.SWP_SHOWWINDOW);
            }
            else
            {
                SetWindowPos(hWnd, HWND_TOP,
                    magnifierXpos,
                    mousePoint.y - (int)(height / 2),
                    width,
                    height,
                    SetWindowPosFlags.SWP_NOACTIVATE | SetWindowPosFlags.SWP_SHOWWINDOW);
            }

            ShowWindow(hWnd, SW_SHOWNORMAL);

            // 윈도우 강제 업데이트
            UpdateWindow(hWnd);
            InvalidateRect(hwndMag, IntPtr.Zero, true);

            timer.Enabled = true;
        }

        // 현재 화면 인덱스 가져오기
        private int GetCurrentScreenIndex(POINT mousePoint)
        {
            for (int i = 0; i < sc.Length; i++)
            {
                if (sc[i].Bounds.Contains(mousePoint.x, mousePoint.y))
                {
                    return i;
                }
            }
            return 0; // 기본값
        }

        // 소스 영역을 화면 경계 내로 조정
        private void AdjustSourceRect(ref RECT sourceRect, int screenIndex)
        {
            if (screenIndex >= 0 && screenIndex < sc.Length)
            {
                var screenBounds = sc[screenIndex].Bounds;

                if (sourceRect.left < screenBounds.Left)
                {
                    sourceRect.left = screenBounds.Left;
                    sourceRect.right = sourceRect.left + sourceWidth;
                }
                else if (sourceRect.right > screenBounds.Right)
                {
                    sourceRect.right = screenBounds.Right;
                    sourceRect.left = sourceRect.right - sourceWidth;
                }

                if (sourceRect.top < screenBounds.Top)
                {
                    sourceRect.top = screenBounds.Top;
                    sourceRect.bottom = sourceRect.top + sourceHeight;
                }
                else if (sourceRect.bottom > screenBounds.Bottom)
                {
                    sourceRect.bottom = screenBounds.Bottom;
                    sourceRect.top = sourceRect.bottom - sourceHeight;
                }
            }
        }

        public void MagnifierOff()
        {
            ShowWindow(hWnd, SW_HIDE);
            define_layer = true;
            if (timer != null) { timer.Enabled = false; }
        }

        void timer_Tick(object sender, EventArgs e)
        {
            UpdateMaginifier();
        }

        public void FilpLayerdValue(bool value)
        {
            define_layer = value;
        }

        public virtual void UpdateMaginifier()
        {
            if (hwndMag == IntPtr.Zero)
            {
                return;
            }

            POINT mousePoint = new POINT();
            RECT sourceRect = new RECT();

            NativeMethods.GetCursorPos(ref mousePoint);

            int screenIndex = GetCurrentScreenIndex(mousePoint);

            sourceRect.left = mousePoint.x - (int)(sourceWidth / 2);
            sourceRect.top = mousePoint.y - (int)(sourceHeight / 2);
            sourceRect.right = sourceRect.left + sourceWidth;
            sourceRect.bottom = sourceRect.top + sourceHeight;

            // 화면 경계 조정
            AdjustSourceRect(ref sourceRect, screenIndex);

            // 소스 영역 설정
            NativeMethods.MagSetWindowSource(hwndMag, sourceRect);

            // 확대경 위치 업데이트
            if (define_layer)
            {
                SetWindowPos(hWnd, HWND_TOPMOST,
                    mousePoint.x - (int)(width / 2),
                    mousePoint.y - (int)(height / 2),
                    width,
                    height,
                    SetWindowPosFlags.SWP_NOACTIVATE);
            }
            else
            {
                SetWindowPos(hWnd, HWND_TOP,
                    mousePoint.x - (int)(width / 2),
                    mousePoint.y - (int)(height / 2),
                    width,
                    height,
                    SetWindowPosFlags.SWP_NOACTIVATE);
            }

            // 강제 다시 그리기
            NativeMethods.InvalidateRect(hwndMag, IntPtr.Zero, true);
            UpdateWindow(hWnd);
        }

        bool SetupMagnifier()
        {
            if (NativeMethods.MagInitialize() == false)
            {
                uint error = GetLastError();
                Console.WriteLine($"MagInitialize 실패: {error}");
                return false;
            }

            WNDCLASSEX wind_class = new WNDCLASSEX();
            wind_class.cbSize = Marshal.SizeOf(typeof(WNDCLASSEX));
            wind_class.style = 0;
            wind_class.hbrBackground = (IntPtr)(COLOR_GRAYTEXT + 1);
            wind_class.cbClsExtra = 0;
            wind_class.cbWndExtra = 0;
            wind_class.hInstance = hInstance;
            wind_class.hIcon = IntPtr.Zero;
            wind_class.lpszMenuName = null;
            wind_class.lpszClassName = "MagnifierClass";
            wind_class.lpfnWndProc = Marshal.GetFunctionPointerForDelegate(delegWndProc);
            wind_class.hIconSm = IntPtr.Zero;

            ushort regResult = RegisterClassEx(ref wind_class);
            if (regResult == 0)
            {
                uint error = GetLastError();
                Console.WriteLine($"RegisterClassEx 실패: {error}");
                return false;
            }

            // 윈도우 생성 - Windows 11 호환성 개선
            hWnd = CreateWindowEx(
                WS_EX_TOPMOST | WS_EX_LAYERED | WS_EX_NOACTIVATE,
                regResult,
                "Magnifier",
                WS_POPUP | WS_CLIPCHILDREN,
                0, 0, 400, 400,
                IntPtr.Zero, IntPtr.Zero, hInstance, IntPtr.Zero);

            if (hWnd == IntPtr.Zero)
            {
                uint error = GetLastError();
                Console.WriteLine($"CreateWindowEx 실패: {error}");
                return false;
            }

            // 레이어 윈도우 속성 설정 (투명도)
            NativeMethods.SetLayeredWindowAttributes(hWnd, 0, 254, (LayeredWindowAttributeFlags)0x00000002);

            // 확대경 윈도우 생성
            NativeMethods.GetClientRect(hWnd, ref magWindowRect);

            hwndMag = CreateWindow(0, NativeMethods.WC_MAGNIFIER, "MagnifierWindow",
                WS_CHILD | WS_VISIBLE,
                magWindowRect.left, magWindowRect.top,
                (magWindowRect.right - magWindowRect.left),
                (magWindowRect.bottom - magWindowRect.top),
                hWnd, IntPtr.Zero, hInstance, IntPtr.Zero);

            if (hwndMag == IntPtr.Zero)
            {
                uint error = GetLastError();
                Console.WriteLine($"확대경 윈도우 생성 실패: {error}");
                return false;
            }

            // 윈도우 스타일 설정
            var style = GetWindowLong(hWnd, GWL_EXSTYLE);
            SetWindowLong(hWnd, GWL_EXSTYLE, style | WS_EX_LAYERED | WS_EX_NOACTIVATE);

            return true;
        }

        // 나머지 메서드들은 기존과 동일...
        public float Magnification
        {
            get { return magnification; }
            set
            {
                if (magnification != value)
                {
                    magnification = value;
                    Transformation matrix = new Transformation(magnification);
                    NativeMethods.MagSetWindowTransform(hwndMag, ref matrix);
                    updateDimensions();
                }
            }
        }

        public int TimerInterval
        {
            get { return timerInterval; }
            set
            {
                if (timerInterval != value)
                {
                    timerInterval = value;
                    if (timer != null)
                    {
                        timer.Interval = timerInterval;
                    }
                }
            }
        }

        public void ResizeMagnifier(int size)
        {
            if (hWnd != IntPtr.Zero)
            {
                SetWindowPos(hWnd, HWND_TOP, 0, 0, size, size, SetWindowPosFlags.SWP_NOMOVE);
                SetWindowPos(hwndMag, HWND_TOP, 0, 0, size, size, SetWindowPosFlags.SWP_NOZORDER);
                NativeMethods.GetClientRect(hWnd, ref magWindowRect);

                IntPtr hRgn = CreateEllipticRgn(5, 5, size - 5, size - 5);
                SetWindowRgn(hwndMag, hRgn, true);

                IntPtr hRgn2 = CreateEllipticRgn(0, 0, size, size);
                SetWindowRgn(hWnd, hRgn2, true);

                updateDimensions();
            }
        }

        private IntPtr myWndProc(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            switch (msg)
            {
                case WM_PAINT:
                    OnPaint(hwnd);
                    break;
                case WM_KEYDOWN:
                case WM_GETDLGCODE:
                    return (IntPtr)0;
                case WM_LBUTTONDBLCLK:
                    break;
                case WM_DESTROY:
                    DestroyWindow(hwnd);
                    break;
                default:
                    break;
            }
            return DefWindowProc(hwnd, msg, wParam, lParam);
        }

        private void OnPaint(IntPtr hwnd)
        {
            IntPtr hdc;
            PAINTSTRUCT ps;

            hdc = BeginPaint(hwnd, out ps);
            Graphics g = Graphics.FromHdc(hdc);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            Pen pen = new Pen(Color.DimGray, 5.0f);
            g.DrawEllipse(pen,
                (int)((magWindowRect.left + magWindowRect.right) / 2),
                (int)((magWindowRect.top + magWindowRect.bottom) / 2),
                magWindowRect.right - magWindowRect.left + 1,
                magWindowRect.bottom - magWindowRect.top + 1);

            pen.Dispose();
            g.Dispose();
            EndPaint(hwnd, ref ps);
        }

        // DPI 인식 관련 추가 정의
        private enum PROCESS_DPI_AWARENESS
        {
            Process_DPI_Unaware = 0,
            Process_System_DPI_Aware = 1,
            Process_Per_Monitor_DPI_Aware = 2
        }

        private static readonly IntPtr DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = new IntPtr(-4);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetProcessDpiAwarenessContext(IntPtr dpiContext);

        [DllImport("SHCore.dll", SetLastError = true)]
        private static extern bool SetProcessDpiAwareness(PROCESS_DPI_AWARENESS awareness);

        [DllImport("user32.dll")]
        private static extern bool InvalidateRect(IntPtr hWnd, IntPtr lpRect, bool bErase);

        // 기존 P/Invoke 선언들...
        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, SetWindowPosFlags uFlags);

        [DllImport("user32.dll")]
        static extern int SetWindowRgn(IntPtr hWnd, IntPtr hRgn, bool bRedraw);

        [DllImport("gdi32.dll")]
        static extern IntPtr CreateEllipticRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect);

        [DllImport("user32.dll")]
        static extern IntPtr BeginPaint(IntPtr hwnd, out PAINTSTRUCT lpPaint);

        [DllImport("user32.dll")]
        static extern bool EndPaint(IntPtr hWnd, [In] ref PAINTSTRUCT lpPaint);

        [DllImport("user32.dll")]
        static extern bool UpdateWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool DestroyWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true, EntryPoint = "CreateWindowEx")]
        public static extern IntPtr CreateWindowEx(int dwExStyle, UInt16 regResult, string lpWindowName, int dwStyle, int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

        [DllImport("user32.dll", EntryPoint = "CreateWindowExW", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.StdCall)]
        public extern static IntPtr CreateWindow(int dwExStyle, string lpClassName, string lpWindowName, int dwStyle, int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true, EntryPoint = "RegisterClassEx")]
        static extern System.UInt16 RegisterClassEx([In] ref WNDCLASSEX lpWndClass);

        [DllImport("kernel32.dll")]
        static extern uint GetLastError();

        [DllImport("user32.dll")]
        static extern IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32", EntryPoint = "UnregisterClass")]
        public static extern int UnregisterClass(string lpClassName, int hInstance);

        // 상수 정의
        private const int SW_HIDE = 0;
        private const int SW_SHOWNORMAL = 1;
        private const int WS_EX_TOPMOST = 0x00000008;
        private const int WS_EX_LAYERED = 0x00080000;
        private const int WS_EX_NOACTIVATE = 0x08000000;
        private const int WS_CHILD = 0x40000000;
        private const int WS_VISIBLE = 0x10000000;
        private const int WS_POPUP = unchecked((int)0x80000000);
        private const int WS_CLIPCHILDREN = 0x02000000;
        private const int GWL_EXSTYLE = -20;

        private static IntPtr HWND_TOPMOST = new IntPtr(-1);
        private static IntPtr HWND_TOP = new IntPtr(0);

        const UInt32 COLOR_GRAYTEXT = 17;
        const UInt32 WM_DESTROY = 2;
        const UInt32 WM_PAINT = 0x0f;
        const UInt32 WM_KEYDOWN = 0x0100;
        const UInt32 WM_GETDLGCODE = 0x0087;
        const UInt32 WM_LBUTTONDBLCLK = 0x0203;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct WNDCLASSEX
        {
            [MarshalAs(UnmanagedType.U4)]
            public int cbSize;
            [MarshalAs(UnmanagedType.U4)]
            public int style;
            public IntPtr lpfnWndProc;
            public int cbClsExtra;
            public int cbWndExtra;
            public IntPtr hInstance;
            public IntPtr hIcon;
            public IntPtr hCursor;
            public IntPtr hbrBackground;
            public string lpszMenuName;
            public string lpszClassName;
            public IntPtr hIconSm;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct PAINTSTRUCT
        {
            public IntPtr hdc;
            public bool fErase;
            public RECT rcPaint;
            public bool fRestore;
            public bool fIncUpdate;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] public byte[] rgbReserved;
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (timer != null)
                    {
                        timer.Dispose();
                        timer = null;
                    }

                    if (hwndMag != IntPtr.Zero)
                    {
                        NativeMethods.MagUninitialize();
                    }

                    if (hWnd != IntPtr.Zero)
                    {
                        DestroyWindow(hWnd);
                    }

                    if (UnregisterClass("MagnifierClass", hInstance.ToInt32()) == 0)
                    {
                        uint error = GetLastError();
                        Console.WriteLine($"UnregisterClass 실패: {error}");
                    }
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}