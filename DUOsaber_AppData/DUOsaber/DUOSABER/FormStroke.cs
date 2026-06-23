using Pointer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Media3D;

namespace DUOsaber
{
    public partial class FormStroke : Form
    {
        Color circleColor = Color.White;
        int nRadius = 0;
        int circleSize = 110;
        Point point = Point.Empty;

        public FormStroke()
        {
            InitializeComponent();
            //PointerOn();

        }



        public void setCircle(int size, int thickness, double opacity, Color color)
        {
            Opacity = opacity;
            int bmp_width = (int)(size);
            int bmp_height = (int)(size);
            //int bmp_thickness = (int)((2.0 / 6.0) * (bmp_width) * (thickness / 100.0));
            int bmp_thickness = thickness;
            nRadius = (bmp_thickness + bmp_width) / 2;

            Bitmap bmp = new Bitmap(bmp_width + 2 * bmp_thickness, bmp_height + 2 * bmp_thickness);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                SolidBrush circleBrush = new SolidBrush(color);
                Pen circlePen = new Pen(circleBrush, bmp_thickness);
                g.DrawEllipse(circlePen, new Rectangle(bmp_thickness, bmp_thickness, bmp_width - bmp_thickness, bmp_height - bmp_thickness));
            }
            Size = bmp.Size;
            pointerBox.Size = bmp.Size;
            pointerBox.SizeMode = PictureBoxSizeMode.Zoom;
            pointerBox.Image = bmp;
            pointerBox.Location = Point.Empty;
            circleColor = color;
            circleSize = size;

            // transparecy의 색과 Circle의 색이 같은 경우 예외처리
            if (circleColor == Color.FromArgb(255, 0, 0, 0) || circleColor == Color.White)
            {

                BackColor = Color.Blue;
                TransparencyKey = Color.Blue;
            }
            else
            {
                BackColor = Color.White;
                TransparencyKey = Color.White;
            }

            //Console.WriteLine("set Circle :배경색  "+BackColor+ "원 색  " + CircleColor+ "투명화 색  " + TransparencyKey);
            //pointerMode = MODE_CIRCLE;
        }

        public void PointerOn()
        {
            setCircle(circleSize, 10, 1, circleColor);
            //TopMost = false;
            TopMost = true;
            Show();
        }

        public void PointerOff()
        {
            Hide();
        }
        public void trackingMag(int xPos, int yPos)
        {
            //point = new Point(xPos - 5, yPos - 5);
            point.X = xPos - 5;
            point.Y = yPos - 5;
            this.Location = point;
        }
    }
}
