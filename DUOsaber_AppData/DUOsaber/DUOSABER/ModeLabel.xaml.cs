using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
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
using System.Xml.Linq;

namespace DUOsaber
{
    /// <summary>
    /// ModeLabel.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ModeLabel : Window
    {

        Screen screen;
        double wall;
        double ceiling;
        double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
        double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;

        public DispatcherTimer timer;
        public ModeLabel()
        {
            InitializeComponent();
            LanguageSet();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(10);
            timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            Opacity = Opacity * 0.98;
            if(Opacity <= 0.025) 
            {
                PTlText.Visibility = Visibility.Hidden;
                GenText.Visibility = Visibility.Hidden;
                Hide();
                timer.Stop();
                Opacity = 1;
            }
        }

        public void LanguageSet()
        {
            PTlText.Content = Properties.Resources.PTmode;
            GenText.Content = Properties.Resources.generalMode;
        }

        public void PosSet()
        {
            System.Drawing.Point mousePosition = System.Windows.Forms.Control.MousePosition;
            screen = Screen.FromPoint(mousePosition);

            if(screenWidth < mousePosition.X)
                this.Left = screenWidth;
            else this.Left = 0;

            this.Top = screen.Bounds.Top;
            //Console.WriteLine(this.Left + " " + this.Top);
        }
    }
}
