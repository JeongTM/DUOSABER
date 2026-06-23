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

namespace DUOsaber
{
    /// <summary>
    /// SpotLightPage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SpotLightPage : Page
    {
        MainWindow mainWindow;
        RadialGradientBrush gradientBrush;
        public SpotLightPage(MainWindow main)
        {
            InitializeComponent();
            mainWindow = main;
            gradientBrush = (RadialGradientBrush)SpotlighSampleView.Fill;
            gradientBrush.RadiusX = 100 / SpotlighSampleView.Width;
            gradientBrush.RadiusY = 100 / SpotlighSampleView.Height;
            gradientBrush.Opacity = BaseOpacitySlider.Value / 100;

            SetSampleView();
            //LanguageSet();
        }

        /*public void LanguageSet()
        {
            SpotlightLabel.Content = Properties.Resources.spotlight;

            SizeLabel.Content = Properties.Resources.size;
            OpacityLabel.Content = Properties.Resources.Opacity;
        }*/

        public void SetSampleView()
        {
            int index = Array.IndexOf(mainWindow.flag, 0);
            int margin = 42;
            int sampleWidth = 80;
            Point point;
            switch (index)
            {
                case 0:
                    point = new Point((margin +(sampleWidth / 2)) / SpotlighSampleView.Width, (margin + (sampleWidth)) / SpotlighSampleView.Height);
                    gradientBrush.Center = point;
                    gradientBrush.GradientOrigin = point;
                    break;
                case 1:
                    point = new Point((SpotlighSampleView.Width/2)/ SpotlighSampleView.Width, (margin + (sampleWidth )) / SpotlighSampleView.Height);
                    gradientBrush.Center = point;
                    gradientBrush.GradientOrigin = point;
                    break;
                case 2:
                    point = new Point((SpotlighSampleView.Width - (sampleWidth / 2) - margin) / SpotlighSampleView.Width, (margin + (sampleWidth)) / SpotlighSampleView.Height);
                    gradientBrush.Center = point;
                    gradientBrush.GradientOrigin = point;
                    break;
                default:
                    point = new Point((SpotlighSampleView.Width / 2) / SpotlighSampleView.Width, (margin + (sampleWidth)) / SpotlighSampleView.Height);
                    gradientBrush.Center = point;
                    gradientBrush.GradientOrigin = point;
                    break;
            }
        }

        public void SampleViewSize(double size)
        {
            spotSampleBrush.RadiusX = size / 20 * 0.2;
            spotSampleBrush.RadiusY = size / 20 * 0.4;
        }

        private void SpotlightSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mainWindow?.SetSpotlight(SpotlightSizeSlider.Value, BaseOpacitySlider.Value);
            SampleViewSize(SpotlightSizeSlider.Value);
            mainWindow?.SavePaeameter();
        }

        private void BaseOpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mainWindow?.SetSpotlight(SpotlightSizeSlider.Value, BaseOpacitySlider.Value);
            if(gradientBrush != null) 
                gradientBrush.Opacity = BaseOpacitySlider.Value / 100;
            mainWindow?.SavePaeameter();
        }

        private void Label_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SpotlightSizeSlider.Value = SpotlightSizeSlider.Value + 1;
        }

        private void Label_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            SpotlightSizeSlider.Value = SpotlightSizeSlider.Value - 1;
        }

        private void Label_MouseLeftButtonDown_2(object sender, MouseButtonEventArgs e)
        {
            BaseOpacitySlider.Value = BaseOpacitySlider.Value + 1;
        }

        private void Label_MouseLeftButtonDown_3(object sender, MouseButtonEventArgs e)
        {
            BaseOpacitySlider.Value = BaseOpacitySlider.Value - 1;
        }
    }
}
