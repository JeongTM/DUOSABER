using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
//using System.Windows;
using System.Windows.Forms;
using System.Windows.Ink;

namespace DUOsaber
{
    public partial class FormMagnifier : Form
    {
        int minX = 0;
        int minY = 0;
        private int maxX = 0;
        private int maxY = 0;
        int allScreenWidth = 0;
        int allScreenHeight = 0;
        public bool isOn = false;
        private int nRadius;
        private int magnificatoin = 2;
        Rectangle screenBounds;
        public FormStroke stroke = new FormStroke();
        public Color strokeColor = Color.White;

        private Bitmap allCanvas = null;
        public FormMagnifier()
        {
            InitializeComponent();
            SetRoundedRegion(this,this.Width/2);
        }

        private Bitmap CaptureScreen()
        {
            // 전체 화면 캡처
            //Screen[] sc = Screen.AllScreens;
            minX = 0;
            minY = 0;
            maxX = 0;
            maxY = 0;

            allScreenWidth = 0;
            allScreenHeight = 0;
            foreach (var screen in Screen.AllScreens)
            {
                int monitorMaxX = screen.Bounds.X + screen.Bounds.Width;
                int monitorMaxY = screen.Bounds.Y + screen.Bounds.Height;

                if (screen.Bounds.Contains(MousePosition))
                {
                    screenBounds = screen.Bounds;
                    //break;
                }

                if (screen.Bounds.X < minX)
                {
                    minX = screen.Bounds.X;
                }

                if (screen.Bounds.Y < minY)
                {
                    minY = screen.Bounds.Y;
                }

                if (monitorMaxX > maxX)
                {
                    maxX = monitorMaxX;
                }

                if (monitorMaxY > maxY)
                {
                    maxY = monitorMaxY;
                }
                

            }
            nRadius = this.Width / 2;

            allScreenWidth += maxX - minX;
            allScreenHeight += maxY - minY;
            screenBounds.Width = allScreenWidth;
            screenBounds.Height = allScreenHeight;
            Point captureStart = new Point(minX, minY);
            Bitmap bitmap = new Bitmap(screenBounds.Width, screenBounds.Height);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(captureStart,Point.Empty, screenBounds.Size);
            }

            return bitmap;
        }

        static void SetRoundedRegion(Control control, int cornerRadius)
        {
            // 모서리를 깎기 위한 GraphicsPath 생성
            GraphicsPath path = new GraphicsPath();
            path.AddArc(0, 0, cornerRadius * 2, cornerRadius * 2, 180, 90);
            path.AddArc(control.Width - cornerRadius * 2, 0, cornerRadius * 2, cornerRadius * 2, 270, 90);
            path.AddArc(control.Width - cornerRadius * 2, control.Height - cornerRadius * 2, cornerRadius * 2, cornerRadius * 2, 0, 90);
            path.AddArc(0, control.Height - cornerRadius * 2, cornerRadius * 2, cornerRadius * 2, 90, 90);
            path.CloseFigure();
            // Region 설정
            control.Region = new Region(path);

        }

        public void MagnifierOn()
        {
            Bitmap screenCapture = allCanvas = CaptureScreen();
            pictureBox1.Image = screenCapture;
            pictureBox1.Size = new Size(screenBounds.Width * magnificatoin, screenBounds.Height * magnificatoin);


            moveTimer.Enabled = true;
            Show();

            stroke.PointerOn();
        }

        public void MagnifierOff()
        {
            moveTimer.Enabled = false;
            Hide();
            stroke.PointerOff();

        }

        public void setMagnification(int getMag)
        {
            magnificatoin = getMag;
        }

        public int getMagSize()
        {
            return this.Width;
        }

        Size canvasSize = Size.Empty;

        public void subMagnifierOn()
        {
            Bitmap screenCapture = allCanvas = CaptureScreen();
            //pictureBox1.Image = screenCapture;
            canvasSize.Width = screenBounds.Width * magnificatoin;
            canvasSize.Height = screenBounds.Height * magnificatoin;
            //pictureBox1.Size = new Size(screenBounds.Width * magnificatoin, screenBounds.Height * magnificatoin);
            //pictureBox1.Size = canvusSize;
            //moveTimer.Enabled = true;
            Show();
            stroke.PointerOn();
        }

        public void subMagnifierOff()
        {
            //moveTimer.Enabled = false;
            pictureBox1.Image = null;
            allCanvas = null;
            stroke.PointerOff();
            Hide();
        }

        public void setMagSize(int getSize)
        {
            this.Size = new Size((int)getSize, (int)getSize);
            pictureBox2.Size = new Size((int)getSize, (int)getSize);
            SetRoundedRegion(this, this.Width / 2);
            nRadius = this.Width / 2;
            stroke.setCircle((int)getSize,10,1, strokeColor);
        }

        Rectangle cropArea = Rectangle.Empty;
        Rectangle magGlass = Rectangle.Empty;
        public void subCanvas(int xPos, int yPos)
        {

            //Rectangle cropArea = new Rectangle(xPos - minX, yPos - minY, this.Width, this.Height);
            double correction = 0;
            switch (magnificatoin)
            {
                case 2: correction = 2; break;
                case 3: correction = 1; break;
                case 4: correction = 0.6; break;
                case 5: correction = 0.5; break;
                case 6: correction = 0.4; break;          
                default: correction = 0; break;
            }

            pictureBox2.Image = null;
            cropArea.X = xPos - minX + (int)(pictureBox2.Width / (2 + correction));
            cropArea.Y = yPos - minY + (int)(pictureBox2.Height / (2 + correction));

            cropArea.Width = (int)(this.Width * (1.0 / magnificatoin));
            cropArea.Height = (int)(this.Height * (1.0 / magnificatoin));
            magGlass.Width = this.Width;
            magGlass.Height = this.Height;
            Bitmap croppedImage = new Bitmap(this.Width, this.Height);

            // Graphics 객체를 사용해 잘라내기
            using (Graphics g = Graphics.FromImage(croppedImage))
            {
                g.DrawImage(allCanvas, magGlass, cropArea, GraphicsUnit.Pixel);
            }
            //return croppedImage;
            pictureBox2.Image = croppedImage;
            //croppedImage = null;
        }



        private System.Drawing.Point movePoint;
        private System.Drawing.Point bgPoint;
        private void moveTimer_Tick(object sender, EventArgs e)
        {

            /*Bitmap screenCapture = CaptureScreen();
            pictureBox1.Image = screenCapture;*/
            Point point = Control.MousePosition;
            movePoint = point;
            movePoint.X -= nRadius;
            movePoint.Y -= nRadius;

            double x = (double)point.X / screenBounds.Width * pictureBox1.Width;
            double y = (double)point.Y / screenBounds.Height * pictureBox1.Height;
            bgPoint = System.Drawing.Point.Empty;
            bgPoint.X -= (int)x - nRadius - (minX * magnificatoin);
            bgPoint.Y -= (int)y - nRadius - (minY * magnificatoin);


            pictureBox1.Location = bgPoint;
            this.Location = movePoint;


            Invalidate();
        }

        int refrashCnt = 0;
        Point point = Point.Empty;
        Point locate = new Point(300,300);

        public void moveMagnifier(double dx, double dy,bool isMain = false)
        {
            //Screen screen = Screen.FromPoint(point);

            if ((/*this.Top*/locate.Y + dy + nRadius) > maxY)
            {
                dy = (dy < 0 ? dy : 0);
            }
            else if ((/*this.Top*/locate.Y + dy + nRadius) < minY)
            {
                dy = (dy > 0 ? dy : 0);
            }


            if ((/*this.Left*/locate.X + dx + nRadius) > maxX)
            {
                dx =(dx < 0 ? dx : 0);
            }
            else if ((/*this.Left*/locate.X + dx + nRadius) < minX)
            {
                dx = (dx > 0 ? dx : 0);
            }

            if(Math.Abs(dx) <= 1 && Math.Abs(dy) <= 1) { return; }
            //if (dx == 0 && dy == 0) { return; }

            if (!isMain)
            {
                //point = new Point(this.Location.X, this.Location.Y);
                /*point.X = Location.X;
                point.Y = Location.Y;
                movePoint = point;
                movePoint.X += dx;
                movePoint.Y += dy;*/

                point.X = locate.X;
                point.Y = locate.Y;
                movePoint = point;
                movePoint.X += (int)dx;
                movePoint.Y += (int)dy;
                locate = movePoint;

                /*if (refrashCnt >= 2)*/
                {
                    double x = ((double)point.X + nRadius) / screenBounds.Width * pictureBox1.Width;
                    double y = ((double)point.Y + nRadius) / screenBounds.Height * pictureBox1.Height;
                    bgPoint = System.Drawing.Point.Empty;
                    bgPoint.X -= (int)x - (nRadius) - (minX * magnificatoin);
                    bgPoint.Y -= (int)y - (nRadius) - (minY * magnificatoin);
                    refrashCnt = 0;
                    //pictureBox2.Image = subCanvas(movePoint.X, movePoint.Y);
                    subCanvas(movePoint.X, movePoint.Y);
                }

            }
            else
            {
                point = Control.MousePosition;
                movePoint = point;
                movePoint.X -= nRadius;
                movePoint.Y -= nRadius;

                {
                    double x = ((double)point.X/* + nRadius*/) / screenBounds.Width * pictureBox1.Width;
                    double y = ((double)point.Y/* + nRadius*/) / screenBounds.Height * pictureBox1.Height;
                    bgPoint = System.Drawing.Point.Empty;
                    bgPoint.X -= (int)x - (nRadius) - (minX * magnificatoin);
                    bgPoint.Y -= (int)y - (nRadius) - (minY * magnificatoin);
                    refrashCnt = 0;
                    //pictureBox2.Image = subCanvas(movePoint.X, movePoint.Y);
                    subCanvas(movePoint.X, movePoint.Y);
                }
            }
            
            //pictureBox1.Location = bgPoint;
            this.Location = movePoint;
            refrashCnt ++;
            stroke.trackingMag(movePoint.X, movePoint.Y);
            //Invalidate();
        }


        public void setCenter()
        {
            Screen currentScreen = Screen.FromPoint(Cursor.Position);
            movePoint = new Point(currentScreen.Bounds.X + currentScreen.Bounds.Width / 2, currentScreen.Bounds.Y + currentScreen.Bounds.Height / 2);
            movePoint.X -= nRadius;
            movePoint.Y -= nRadius;
            locate = movePoint;
            {
                double x = ((double)point.X/* + nRadius*/) / screenBounds.Width * pictureBox1.Width;
                double y = ((double)point.Y/* + nRadius*/) / screenBounds.Height * pictureBox1.Height;
                bgPoint = System.Drawing.Point.Empty;
                bgPoint.X -= (int)x - (nRadius) - (minX * magnificatoin);
                bgPoint.Y -= (int)y - (nRadius) - (minY * magnificatoin);
                refrashCnt = 0;
                //pictureBox2.Image = subCanvas(movePoint.X, movePoint.Y);
                subCanvas(movePoint.X, movePoint.Y);
            }
            this.Location = movePoint;
            stroke.trackingMag(movePoint.X, movePoint.Y);
        }

    }
}
