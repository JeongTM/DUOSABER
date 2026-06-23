using Pointer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Point = System.Windows.Point;

namespace DUOsaber
{
    /// <summary>
    /// SpotlightBase.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SpotlightBase : Window
    {
        Screen[] monitors = Screen.AllScreens;
        double totalWidth;
        double totalHeight;
        int maxHeight = 0;
        int minLeft = 0;

        double nowSize;

        DispatcherTimer spotlightTimer = new DispatcherTimer();
        double percentageX, percentageY;
        RadialGradientBrush gradientBrush ;
        public SpotlightBase()
        {
            InitializeComponent();

            gradientBrush = (RadialGradientBrush)spot.Fill;

            totalWidth = monitors.Sum(m => m.Bounds.Width);
            totalHeight = monitors.Sum(m => m.Bounds.Top);
            foreach (Screen screen in monitors)
            {
                int screenHeight = screen.Bounds.Height;
                if (screenHeight > maxHeight)
                {
                    maxHeight = screenHeight;
                }
                int screenLeft = screen.Bounds.Left;
                if (screenLeft < minLeft)
                {
                    minLeft = screenLeft;
                }
            }

            System.Drawing.Point mousePosition = System.Windows.Forms.Cursor.Position;
            screen = Screen.FromPoint(mousePosition);
            setFullScreen(screen);

            /*spotlightTimer.Interval = TimeSpan.FromMilliseconds(49);
            spotlightTimer.Tick += SpotlightTimer_Tick;
            spotlightTimer.Start();*/
        }

        private void setFullScreen(Screen screen)
        {
            WindowStartupLocation = WindowStartupLocation.Manual;
            WindowState = WindowState.Normal;


            /*System.Drawing.Rectangle fullScrenn_bounds = System.Drawing.Rectangle.Empty;
            foreach (var screen in Screen.AllScreens)
            {
                fullScrenn_bounds = System.Drawing.Rectangle.Union(fullScrenn_bounds, screen.Bounds);
            }*/
            /*Width = totalWidth;
            Height = maxHeight;
            Left = minLeft;
            Top = totalHeight;*/
            Width = screen.Bounds.Width;
            Height = screen.Bounds.Height;
            Left = screen.Bounds.Left;
            Top = screen.Bounds.Top;
            double radiusX = nowSize / ActualWidth;
            double radiusY = nowSize / ActualHeight;
            gradientBrush.RadiusX = radiusX;
            gradientBrush.RadiusY = radiusY;


        }

        Screen screen;

        public void MoveSpotLight()
        {

            System.Drawing.Point mousePosition = System.Windows.Forms.Cursor.Position;

            if(screen != Screen.FromPoint(mousePosition))
            {
                screen = Screen.FromPoint(mousePosition);
                setFullScreen(screen);
            }


            PresentationSource presentationSource = PresentationSource.FromVisual(this);
            if (presentationSource == null) return;
            System.Windows.Point screenPosition = presentationSource.CompositionTarget.TransformFromDevice.Transform(new Point(mousePosition.X, mousePosition.Y));

            percentageX = (screenPosition.X - Left) / (ActualWidth); // X 좌표의 백분율 계산
            percentageY = (screenPosition.Y - Top) / (ActualHeight); // Y 좌표의 백분율 계산

            Point point = new Point(percentageX, percentageY);
            //gradientBrush = (RadialGradientBrush)spot.Fill;
            gradientBrush.GradientOrigin = point;
            gradientBrush.Center = point;

        }

        public void SetSpotlightOption(int size, double opacity)
        {
            double radiusX = size / ActualWidth;
            double radiusY = size / ActualHeight;
            gradientBrush.RadiusX = radiusX;
            gradientBrush.RadiusY = radiusY;

            gradientBrush.Opacity = opacity;

            nowSize = size;
        }


        private void SpotlightTimer_Tick(object sender, EventArgs e)
        {
            //POINT mousePosition = new POINT();
            //NativeMethods.GetCursorPos(ref mousePosition);
            //Point mousePosition = Mouse.GetPosition(null);

            System.Drawing.Point mousePosition = System.Windows.Forms.Cursor.Position;


            /*Console.WriteLine(mousePosition.X + "," + mousePosition.y);
            Console.WriteLine(ActualWidth + "," + ActualHeight);*/

            percentageX = (mousePosition.X - Left )/ (ActualWidth ); // X 좌표의 백분율 계산
            percentageY = (mousePosition.Y - Top) / (ActualHeight ) ; // Y 좌표의 백분율 계산

            Point point = new Point( percentageX, percentageY );
            //gradientBrush = (RadialGradientBrush)spot.Fill;
            gradientBrush.GradientOrigin = point;
            gradientBrush.Center = point;
            
        }


        private void Window_MouseMove(object sender, MouseEventArgs e)
        {

            Point mousePosition = e.GetPosition(this); // 상대 좌표 가져오기
            double percentageX = mousePosition.X / ActualWidth; // X 좌표의 백분율 계산
            double percentageY = mousePosition.Y / ActualHeight; // Y 좌표의 백분율 계산

            // 백분율 출력 또는 원하는 작업 수행



            RadialGradientBrush gradientBrush = (RadialGradientBrush)spot.Fill;
            gradientBrush.GradientOrigin = new Point(percentageX, percentageY);
            gradientBrush.Center = new Point(percentageX, percentageY);


            double radiusX = 400 / ActualWidth;
            double radiusY = 400 / ActualHeight;
            gradientBrush.RadiusX = radiusX;
            gradientBrush.RadiusY = radiusY;
            // 그라데이션을 적용할 대상 UIElement의 Background에 설정
            spot.Fill = gradientBrush;
        }



    }
}
