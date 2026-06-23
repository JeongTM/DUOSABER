using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;
using Brush = System.Windows.Media.Brush;
using System.Windows.Forms;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using Pointer;
using WpfAnimatedGif;
using Brushes = System.Windows.Media.Brushes;
using Pen = System.Windows.Media.Pen;
using Point = System.Windows.Point;
using System.Drawing.Drawing2D;
using System.IO.Ports;

namespace DUOsaber
{
    /// <summary>
    /// PointerView.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PointerView : Window
    {
        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        //화면 크기
        Screen[] monitors = Screen.AllScreens;
        Screen screen;
        double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
        double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
        double totalWidth;
        double totalHeight;
        Screen screenIn;
        double screenLeft = 0;
        double screenRight = 0;
        //포인터 모드
        public const int MODE_IMAGE = 0;
        public const int MODE_POINT = 1;
        public const int MODE_MAGNIFIER = 2;
        public const int MODE_SPOTLGHT = 3;

        //현재 모드
        private int pointerMode;

        //MagnifierGlass magnifier;
        public bool isMagnifierOn = false;
        public bool isMagnifierTurnOn = false;

        const Int32 SW_SHOWNOACTIVATE = 4;
        IntPtr mainWindowHandle;

        private double imageOpacity;
        private double circleOpacity;

        [DllImport("User32.dll")]
        public extern static int ShowWindow(IntPtr hWnd, Int32 cmdShow);

        public PointerView()
        {
            InitializeComponent();

            totalWidth = monitors.Sum(m => m.Bounds.Width);
            totalHeight = monitors.Sum(m => m.Bounds.Top);
            foreach (Screen screen in monitors)
            {
                if(screen.Bounds.Left < 0)
                {
                    if (screen.Bounds.Left < screenLeft)
                        screenLeft = screen.Bounds.Left;
                }

                if (screen.Bounds.Right > screenRight)
                {
                    screenRight = screen.Bounds.Right;
                }
            }
            //magnifier.MagnifierOn();
            mainWindowHandle = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            System.Drawing.Rectangle fullScrenn_bounds = System.Drawing.Rectangle.Empty;
            PointingModeSet(MODE_IMAGE);
        }

        public void MovePointer(int dx,  int dy)
        {
            if (Math.Abs(dx) < 2 || Math.Abs(dx) < 2) return;
            ShowWindow(mainWindowHandle, SW_SHOWNOACTIVATE);

            System.Drawing.Point pointerPos = new System.Drawing.Point((int)this.Left , (int)this.Top);
            screen = Screen.FromPoint(pointerPos);
            if ((this.Top + dy) > screen.Bounds.Bottom)
            {
                dy = dy < 0 ? dy : 0;
            }
            else if((this.Top + dy) < screen.Bounds.Top)
            {
                dy = dy > 0 ? dy : 0;
            }
            if ((this.Left + dx) > screenRight)
            {
                dx = dx < 0 ? dx : 0;
            }
            else if((this.Left + dx) < screenLeft)
            {
                dx = dx > 0 ? dx : 0;
            }

            this.Left += dx;
            this.Top += dy;

        }

        public bool isOn = false;
        
        public void PointerOn()
        {
            System.Drawing.Point pointerPos = new System.Drawing.Point((int)this.Left, (int)this.Top);
            screen = Screen.FromPoint(pointerPos);

            Left = bLeft == screenLeft - Width + 1 ? screen.Bounds.Width / 2 : bLeft;
            Top = bTop == screen.Bounds.Top - Width + 1 ? screen.Bounds.Height / 2 : bTop;
            switch (pointerMode)
            {
                case MODE_POINT:
                    pointPointer.Visibility = Visibility.Visible;
                    imagePointer.Visibility = Visibility.Collapsed;
                    magCircle.Visibility = Visibility.Collapsed;
                    break;

                case MODE_IMAGE:
                    imagePointer.Visibility = Visibility.Visible;
                    pointPointer.Visibility = Visibility.Collapsed;
                    magCircle.Visibility = Visibility.Collapsed;
                    break;

                case MODE_MAGNIFIER:
                    pointPointer.Visibility = Visibility.Collapsed;
                    imagePointer.Visibility = Visibility.Collapsed;
                    magCircle.Visibility = Visibility.Visible;
                    break;

            }
            if (Opacity != 1) { Opacity = 1; }
            Show();
            isOn = true;
            //InvalidateVisual();
        }

        public void PointerOff()
        {
            bLeft = Left;
            bTop = Top;
            pointPointer.Visibility = Visibility.Collapsed;
            imagePointer.Visibility = Visibility.Collapsed;
            
            Hide();
            isOn = false;
            //InvalidateVisual();
        }

        double bLeft = 9999;
        double bTop = 9999;

        public void PointingModeSet(int mode)
        {
            pointerMode = mode;
            System.Drawing.Point pointerPos = new System.Drawing.Point((int)this.Left, (int)this.Top);
            screen = Screen.FromPoint(pointerPos);

            switch (mode)
            {
                case MODE_POINT:
                    pointPointer.Visibility = Visibility.Visible;
                    imagePointer.Visibility = Visibility.Collapsed;
                    Opacity = 0.01;

                    bLeft = Left;
                    bTop = Top;

                    Left = screenLeft - Width + 1;
                    Top = screen.Bounds.Top - Width + 1;
                    Show();
                    break;

                case MODE_IMAGE:
                    imagePointer.Visibility = Visibility.Visible;
                    pointPointer.Visibility = Visibility.Collapsed;
                    Opacity = 0.01;

                    bLeft = Left;
                    bTop = Top;

                    Left = screenLeft - Width + 1;
                    Top = screen.Bounds.Top - Width + 1;
                    Show();
                    break;
                case MODE_MAGNIFIER:
                    pointPointer.Visibility = Visibility.Collapsed;
                    imagePointer.Visibility = Visibility.Collapsed;
                    break;
                case MODE_SPOTLGHT:
                    pointPointer.Visibility = Visibility.Collapsed;
                    imagePointer.Visibility = Visibility.Collapsed;
                    break;
            }

            
            //InvalidateVisual();

        }


        public int GetPointerMode()
        {
            return pointerMode;
        }

        public void SetImagePointer(string path)
        {
            string imageExtension = GetFileExtension(path);
            Console.WriteLine(imageExtension);
            ImageBehavior.SetAnimatedSource(imagePointer, new BitmapImage(new Uri(path)));
            imagePointer.Source = new BitmapImage(new Uri(path));
            

        }
        static string GetFileExtension(string url)
        {
            int lastIndex = url.LastIndexOf('.');
            if (lastIndex != -1)
            {
                return url.Substring(lastIndex + 1);
            }
            return string.Empty;
        }

        public void SetImageSize(double size , double opacity)
        {
            double _size = size * size;

            imagePointer.Width = _size;
            imagePointer.Height = _size;
            this.Height = _size;
            this.Width = _size;
            imagePointer.Opacity = opacity / 100;
            imageOpacity = opacity / 100;
        }


        public void SetColorPointer(Brush color)
        {
            circlePoint.Fill = color;
            //magStroke.Stroke = color;
        }
        public void SetColorSize(double size, double opasity)
        {
            double _size = size * size;

            this.Width = circlePoint.Width =  _size;
            this.Height = circlePoint.Height =  _size;

            circlePoint.Opacity = opasity / 100;
            circleOpacity = opasity / 100;
        }

        public void SetStrokeSize(double size)
        {
            double _size = size * size;

            this.Width = magStroke.Width = _size;
            this.Height = magStroke.Height = _size;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            /*imagePointer.Width = e.NewSize.Width;
            imagePointer.Height = e.NewSize.Height;

            circlePoint.Width = e.NewSize.Width;
            circlePoint.Height = e.NewSize.Height;*/

        }
    }
}
