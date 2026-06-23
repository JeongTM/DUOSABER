using Pointer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using Point = System.Drawing.Point;

namespace DUOsaber
{
    public partial class FormSpotlight : Form
    {
        public List<PictureBox> pictureBoxes;
        double screenLeft = 0;
        double screenRight = 0;
        double screenTop = 0;
        double screenBottom = 0;
        double screenWidth = 0;
        double screenHeight = 0;
        int minX = 0;
        int minY = 0;

        public bool ismag = false;
        public int magSize = 30;
        public int pointerIndex = 4;
        public FormSpotlight()
        {
            InitializeComponent();
            pictureBoxes = new List<PictureBox>() { ovalPictureBox1, ovalPictureBox2, ovalPictureBox3} ;
            //timerSpotlight.Start();
            

            foreach (PictureBox pictureBox in pictureBoxes)
            {
                pictureBox.Visible = false;
            }

            foreach (Screen screen in Screen.AllScreens)
            {

                if (screen.Bounds.X < minX)
                {
                    minX = screen.Bounds.X;
                }

                if (screen.Bounds.Y < minY)
                {
                    minY = screen.Bounds.Y;
                }

                if (screen.Bounds.Left < 0)
                {
                    if (screen.Bounds.Left < screenLeft)
                        screenLeft = screen.Bounds.Left;
                }

                if (screen.Bounds.Right > screenRight)
                {
                    screenRight = screen.Bounds.Right;
                }

                if (screen.Bounds.Top < screenTop)
                {
                    if (screen.Bounds.Top < screenTop)
                        screenTop = screen.Bounds.Top;
                }

                if (screen.Bounds.Bottom > screenBottom)
                {
                    if (screen.Bounds.Bottom > screenBottom)
                        screenBottom = screen.Bounds.Bottom;
                }
            }
            //Console.WriteLine("좌표" +screenRight + "," + screenBottom);

        }

        private void setFullScreen(Screen screen = null)
        {
            if (screen != null)
                this.screen = screen;
            StartPosition = FormStartPosition.Manual;
            WindowState = FormWindowState.Normal;

            System.Drawing.Rectangle fullScrenn_bounds = System.Drawing.Rectangle.Empty;
            foreach (var screens in Screen.AllScreens)
            {
                fullScrenn_bounds = System.Drawing.Rectangle.Union(fullScrenn_bounds, screens.Bounds);
            }
           
            ClientSize = new System.Drawing.Size(fullScrenn_bounds.Width, fullScrenn_bounds.Height);
            Location = new System.Drawing.Point(fullScrenn_bounds.Left, fullScrenn_bounds.Top);

            screenWidth = fullScrenn_bounds.Width;
            screenHeight = fullScrenn_bounds.Height;
            
        }

        private Screen screen;
        private int nRadius;

        public void SetMagSize(int index, int radius, double opacity)
        {
            /*pointerIndex = index;
            magSize = (int)(radius);*/
            Opacity = opacity;

            pictureBoxes[index].Size = new System.Drawing.Size(radius, radius);
            //pictureBoxes[index].SizeMode = PictureBoxSizeMode.AutoSize;
            pictureBoxes[index].BackColor = TransparencyKey;
        }

        public void setSpotlight(int size, double opacity, int index = 4)
        {
            screen = Screen.FromPoint(MousePosition);
            
            nRadius = size / 2;  // this determines the size of the highlight circle.


            Opacity = opacity;
            
            //Bitmap bmp = new Bitmap(size, size);
            
            foreach (PictureBox picture in pictureBoxes)
            {
                picture.Size = new System.Drawing.Size(size, size);
                //picture.SizeMode = PictureBoxSizeMode.AutoSize;
                picture.BackColor = TransparencyKey;
                //picture.Image = bmp; 
            }
/*
            if (index != 4)
            {
                pictureBoxes[index].Size = new System.Drawing.Size(size, size);
                pictureBoxes[index].SizeMode = PictureBoxSizeMode.AutoSize;
                pictureBoxes[index].BackColor = TransparencyKey;
            }*/

            setFullScreen(screen);
        }

        public void MoveSpotLight(int index, sbyte dx, sbyte dy)
        {
            screen = Screen.FromPoint(pictureBoxes[index].Location);
            var currentPictureBox = pictureBoxes[index];
            int harfX = currentPictureBox.Location.X + (currentPictureBox.Width / 2);
            int harfY = currentPictureBox.Location.Y + (currentPictureBox.Height / 2);
            
            if ((harfY + dy) > screenHeight || (harfY + dy) < 0)
                dy = 0;
            if ((harfX + dx) > screenWidth || (harfX + dx) < 0)
                dx = 0;

            currentPictureBox.Location = new System.Drawing.Point(
                currentPictureBox.Location.X + dx, 
                currentPictureBox.Location.Y + dy);

        }

        public void setCenter(int index)
        {
            screen = Screen.FromPoint(pictureBoxes[index].Location);
            var currentPictureBox = pictureBoxes[index];
            int harfX = screen.Bounds.X + screen.Bounds.Width / 2 - (currentPictureBox.Width / 2);
            int harfY = screen.Bounds.Y + screen.Bounds.Height / 2 + (currentPictureBox.Height / 2);


            currentPictureBox.Location = new Point(harfX, harfY);
        }

        public void MoveMainSpotLight(int index)
        {
            /*if (screen != Screen.FromPoint(MousePosition))
                setFullScreen(Screen.FromPoint(MousePosition));*/
           
            Point point = new Point(MousePosition.X,MousePosition.Y);
            var currentPictureBox = pictureBoxes[index];
            currentPictureBox.Location = new System.Drawing.Point((int)(point.X - (currentPictureBox.Width / 2) - screenLeft), (int)(point.Y - (currentPictureBox.Height / 2) - screenTop));

        }

        int[,] subSpotDxDy = { { 0, 0 }, { 0, 0 }, { 0, 0 } };

        int qweDx = 0;
        int qweDy = 0;

        public void setDxDy(int index ,int dx,int dy) {

            subSpotDxDy[index, 0] += dx;
            subSpotDxDy[index, 1] += dy;
            /*qweDx += dx;
            qweDy += dy;*/

        }

        Point point = Point.Empty;
        public void magTracking(int index, int xPos, int yPos)
        {
            point.X = xPos - minX;
            point.Y = yPos - minY;

            //pictureBoxes[index].Location = new System.Drawing.Point(xPos - minX, yPos - minY);
            pictureBoxes[index].Location = point;
        }

        public void SpotlightOn()
        {
            setFullScreen();
            timerSpotlight.Start();
            Show();
        }

        public void SpotlightOff()
        {
            timerSpotlight.Stop();
            Hide();
        }


        private void timerSpotlight_Tick(object sender, EventArgs e)
        {
            

            for (int i = 0; i < 3; i++)
            {
                if (subSpotDxDy[i, 0] != 0 || subSpotDxDy[i, 1] != 0)
                {
                    var currentPictureBox = pictureBoxes[i];
                    if(currentPictureBox.Location.X + subSpotDxDy[i, 0] < 0 - currentPictureBox.Width/2 || currentPictureBox.Location.X + subSpotDxDy[i, 0] > screenWidth - currentPictureBox.Width / 2)
                    {
                        subSpotDxDy[i, 0] = 0;
                    }

                    if (currentPictureBox.Location.Y + subSpotDxDy[i, 1] < 0 - currentPictureBox.Width / 2 || currentPictureBox.Location.Y + subSpotDxDy[i, 1] > screenHeight - currentPictureBox.Width / 2)
                    {
                        subSpotDxDy[i, 1] = 0;
                    }
                    currentPictureBox.Location = new System.Drawing.Point(
                        currentPictureBox.Location.X + subSpotDxDy[i, 0],
                        currentPictureBox.Location.Y + subSpotDxDy[i, 1]);
                    subSpotDxDy[i, 0] = 0;
                    subSpotDxDy[i, 1] = 0;
                }

            }

            

        }
        class Win32
        {
            public enum Bool
            {
                False = 0,
                True
            };


            [StructLayout(LayoutKind.Sequential)]
            public struct Point
            {
                public Int32 x;
                public Int32 y;

                public Point(Int32 x, Int32 y) { this.x = x; this.y = y; }
            }


            [StructLayout(LayoutKind.Sequential)]
            public struct Size
            {
                public Int32 cx;
                public Int32 cy;

                public Size(Int32 cx, Int32 cy) { this.cx = cx; this.cy = cy; }
            }


            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            struct ARGB
            {
                public byte Blue;
                public byte Green;
                public byte Red;
                public byte Alpha;
            }


            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            public struct BLENDFUNCTION
            {
                public byte BlendOp;
                public byte BlendFlags;
                public byte SourceConstantAlpha;
                public byte AlphaFormat;
            }


            public const Int32 ULW_COLORKEY = 0x00000001;
            public const Int32 ULW_ALPHA = 0x00000002;
            public const Int32 ULW_OPAQUE = 0x00000004;

            public const byte AC_SRC_OVER = 0x00;
            public const byte AC_SRC_ALPHA = 0x01;


            [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
            public static extern Bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst, ref Point pptDst, ref Size psize, IntPtr hdcSrc, ref Point pprSrc, Int32 crKey, ref BLENDFUNCTION pblend, Int32 dwFlags);

            [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
            public static extern IntPtr GetDC(IntPtr hWnd);

            [DllImport("user32.dll", ExactSpelling = true)]
            public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

            [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
            public static extern IntPtr CreateCompatibleDC(IntPtr hDC);

            [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
            public static extern Bool DeleteDC(IntPtr hdc);

            [DllImport("gdi32.dll", ExactSpelling = true)]
            public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

            [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
            public static extern Bool DeleteObject(IntPtr hObject);
        }

        bool isDown = false;

        [DllImport("user32.dll")]
        static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInf);
        private const uint LBUTTONDOWN = 0x0002; // 왼쪽 마우스 버튼 눌림
        private const uint LBUTTONUP = 0x0004; // 왼쪽 마우스 버튼 떼어짐

        private const uint RBUTTONDOWN = 0x0008; // 오른쪽 마우스 버튼 눌림
        private const uint RBUTTONUP = 0x00010; // 오른쪽 마우스 버튼 떼어짐
        private void FormSpotlight_MouseDown(object sender, MouseEventArgs e)
        {
            /*SpotlightOff();
            *//*if (isDown == false )
            {*//*
                    mouse_event(LBUTTONDOWN*//* | LBUTTONUP*//*, 0, 0, 0, 0);
                    isDown = false;
            *//*    
            }*//*
            SpotlightOn();  */
            //Console.WriteLine("dlkajfsfl");

        }

        private void FormSpotlight_Click(object sender, EventArgs e)
        {
            //Console.WriteLine("dlkajfsfl");
            SpotlightOff();
            mouse_event(LBUTTONDOWN | LBUTTONUP, 0, 0, 0, 0);
          
            SpotlightOn();
        }

       
    }
}
