using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Media3D;
using static System.Windows.Forms.AxHost;
using System.Windows.Interop;
using System.Drawing.Printing;

namespace DUOsaber
{
    /// <summary>
    /// MagnifierPage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MagnifierPage : Page
    {

        MainWindow mainWindow;
        RadialGradientBrush gradientBrush;
        ImageBrush imageBrush;
        List<Button> buttonList;

        bool isButton = false;

        public MagnifierPage(MainWindow main)
        {
            InitializeComponent();
            this.mainWindow = main;
            gradientBrush = (RadialGradientBrush)MiniMagnifier.OpacityMask;
            
            /*ScaleTransform.ScaleX = 1;
            ScaleTransform.ScaleY = 1;*/
            

            //buttonList = new List<Button> { mag_1x, mag_1_5x, mag_2x, mag_2_5x, mag_3x};
            SetSampleView();
            //LanguageSet();
        }

        /*public void LanguageSet()
        {
            MagLabel.Content = Properties.Resources.magnifier;

            SizeLabel.Content = Properties.Resources.size;
            MagnificationLabel.Content = Properties.Resources.Magnification;
        }*/

        public void SetSampleView()
        {
            int index = Array.IndexOf(mainWindow.flag, 0);
            int margin = 44;
            int sampleWidth = 80;
            double mag = MagnificationSlider.Value;
            Point point;

            switch (index)
            {
                case 0:
                    point = new Point((margin + (sampleWidth / 2)) / MagnifierSample.Width, (margin + (sampleWidth /2)) / MiniMagnifier.Height);
                    gradientBrush.Center = point;
                    gradientBrush.GradientOrigin = point;
                    ScaleTransform.CenterX = point.X;
                    ScaleTransform.CenterY = point.Y;
                    SampleViewSize(MagnifierSizeSlider.Value); 
                    break;
                case 1:
                    point = new Point((MagnifierSample.Width / 2) / MagnifierSample.Width, (margin + (sampleWidth / 2)) / MiniMagnifier.Height);
                    gradientBrush.Center = point;
                    gradientBrush.GradientOrigin = point;
                    ScaleTransform.CenterX = point.X;
                    ScaleTransform.CenterY = point.Y;
                    SampleViewSize(MagnifierSizeSlider.Value); 
                    break;
                case 2:
                    point = new Point((MagnifierSample.Width - (sampleWidth / 2) - margin) / MagnifierSample.Width, (margin + (sampleWidth / 2)) / MiniMagnifier.Height);
                    gradientBrush.Center = point;
                    gradientBrush.GradientOrigin = point;
                    ScaleTransform.CenterX = point.X;
                    ScaleTransform.CenterY = point.Y;
                    SampleViewSize(MagnifierSizeSlider.Value); 
                    break;
                default:
                    point = new Point((MagnifierSample.Width / 2) / MagnifierSample.Width, (margin + (sampleWidth / 2)) / MiniMagnifier.Height);
                    gradientBrush.Center = point;
                    gradientBrush.GradientOrigin = point;
                    ScaleTransform.CenterX = point.X;
                    ScaleTransform.CenterY = point.Y;
                    SampleViewSize(MagnifierSizeSlider.Value); 
                    break;
            }
            ScaleTransform.ScaleX = mag;
            ScaleTransform.ScaleY = mag;
        }

        public void SampleViewSize(double size)
        {
            int index = Array.IndexOf(mainWindow.flag, 0);

            magSampleView.RadiusX = size / 18 * 0.2;
            magSampleView.RadiusY = size / 18 * 0.6;
            MiniMagStroke.Width = gradientBrush.RadiusX * MagnifierSample.Width;
            MiniMagStroke.Height = gradientBrush.RadiusX * MagnifierSample.Width;

            
            Point point = new Point(gradientBrush.Center.X * MiniMagnifier.Width, gradientBrush.Center.Y);

            //Point point = new Point(ScaleTransform.CenterX * MiniMagnifier.Width, ScaleTransform.CenterY * MiniMagnifier.Height);
            Canvas.SetLeft(MiniMagStroke, point.X - MiniMagStroke.Width * 0.5);
            Canvas.SetTop(MiniMagStroke, point.Y - MiniMagStroke.Height * 0.5 - 10);
        }

        private void MagnifierSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (mainWindow == null) return;
            mainWindow.SetMagnifier(MagnifierSizeSlider.Value,MagnificationSlider.Value);
            SampleViewSize(MagnifierSizeSlider.Value);
            mainWindow?.SavePaeameter();
        }

        private void MagnificationSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (mainWindow == null) return;
            mainWindow.SetMagnifier(MagnifierSizeSlider.Value, MagnificationSlider.Value);
            ScaleTransform.ScaleX = MagnificationSlider.Value;
            ScaleTransform.ScaleY = MagnificationSlider.Value;

            if (isButton)
            {
                foreach (Button button1 in buttonList)
                {
                    button1.Background = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255));
                }
                /*switch(MagnificationSlider.Value)*/
                isButton = false;
            }
            mainWindow?.SavePaeameter();

        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            foreach(Button button1 in buttonList)
            {
                button1.Background = new SolidColorBrush(Color.FromArgb(0, 255, 255, 255));
            }
            button.Background = new SolidColorBrush(Color.FromArgb(127,255,255,255));
            isButton = true;
            MagnificationSlider.Value = double.Parse(button.Tag.ToString());
        }

        private void Label_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MagnificationSlider.Value = MagnificationSlider.Value + 1;
        }

        private void Label_MouseLeftButtonUp_1(object sender, MouseButtonEventArgs e)
        {
            MagnificationSlider.Value = MagnificationSlider.Value - 1;

        }

        private void Label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MagnifierSizeSlider.Value = MagnifierSizeSlider.Value + 1;
        }

        private void Label_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MagnifierSizeSlider.Value = MagnifierSizeSlider.Value - 1;
        }
    }
}
