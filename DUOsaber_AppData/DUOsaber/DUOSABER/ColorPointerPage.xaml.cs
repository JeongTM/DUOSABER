using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Brush = System.Windows.Media.Brush;
using Button = System.Windows.Controls.Button;

namespace DUOsaber
{
    /// <summary>
    /// ColorPointerPage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ColorPointerPage : Page
    {
        MainWindow mainWindow;

        //string ColorSettingPath = System.AppDomain.CurrentDomain.BaseDirectory + "Setting/ColorSetting";
        string ColorSettingPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DUOsaber\\Setting\\ColorSetting");
        string[] buttonColor;


        List<Button> buttonList;
        public ColorPointerPage(MainWindow window)
        {
            InitializeComponent();
            mainWindow = window;

            buttonList = new List<Button> { ColorButton1, ColorButton2, ColorButton3};
            LoadImageSet();
            LoadButtonColor();

            //LanguageSet();
        }

        /*public void LanguageSet()
        {
            ColorLabel.Content = Properties.Resources.colorPointer;

            ColorChangeButton0.Content = Properties.Resources.colorChange;
            ColorChangeButton1.Content = Properties.Resources.colorChange;
            ColorChangeButton2.Content = Properties.Resources.colorChange;

            SizeLabel.Content = Properties.Resources.size;
            OpacityLabel.Content = Properties.Resources.Opacity;
        }*/


        private void LoadImageSet()
        {
            try
            {
                BrushConverter brushConverter = new BrushConverter();
                if (File.Exists(ColorSettingPath))
                {
                    int i = 0;
                    buttonColor = File.ReadAllLines(ColorSettingPath);

                    // 각 줄을 사용하는 로직을 추가합니다.
                    foreach (string line in buttonColor)
                    {
                        Brush customBrush = brushConverter.ConvertFromString(line) as Brush;

                        buttonList[i].Background = customBrush;

                        int a = Convert.ToInt32(line.Substring(1, 2), 16); // 알파
                        int r = Convert.ToInt32(line.Substring(3, 2), 16); // 레드
                        int g = Convert.ToInt32(line.Substring(5, 2), 16); // 그린
                        int b = Convert.ToInt32(line.Substring(7, 2), 16); // 블루
                        mainWindow.magnifiers[i].strokeColor = System.Drawing.Color.FromArgb(255,r,g,b);

                        i++;
                    }
                }
                else
                {
                    // 파일이 존재하지 않는 경우 처리할 로직을 추가합니다.
                    using (StreamWriter writer = new StreamWriter(ColorSettingPath))
                    {
                        // 파일에 작성할 내용을 여기에 추가합니다.
                        writer.WriteLine(Colors.Lime.ToString());
                        writer.WriteLine(Colors.Blue.ToString());
                        writer.WriteLine(Colors.Red.ToString());
                    }
                    buttonColor = File.ReadAllLines(ColorSettingPath);
                    mainWindow.magnifiers[0].strokeColor = System.Drawing.Color.Lime;
                    mainWindow.magnifiers[1].strokeColor = System.Drawing.Color.Blue;
                    mainWindow.magnifiers[2].strokeColor = System.Drawing.Color.Red;



                }
            }
            catch (Exception)
            {
                using (StreamWriter writer = new StreamWriter(ColorSettingPath))
                {
                    // 파일에 작성할 내용을 여기에 추가합니다.
                    writer.WriteLine(Colors.Lime.ToString());
                    writer.WriteLine(Colors.Blue.ToString());
                    writer.WriteLine(Colors.Red.ToString());
                }
                buttonColor = File.ReadAllLines(ColorSettingPath);

            }
        }


        private void LoadButtonColor()
        {
            for(int i = 0; i < buttonList.Count; i++)
            {
                Brush buttonBackground = buttonList[i].Background;

                mainWindow.pointers[i].SetColorPointer(buttonBackground);
            }
        }
        
        private void SelectButtonColor(int index)
        {
            ColorDialog colorDialog = new ColorDialog();
            DialogResult result = colorDialog.ShowDialog();
            mainWindow.isDialog = true;

            if (result == DialogResult.OK)
            {
                // 선택한 색상 가져오기
                System.Drawing.Color selectedColor = colorDialog.Color;

                // WPF의 Color로 변환
                System.Windows.Media.Color wpfColor = System.Windows.Media.Color.FromArgb(
                    selectedColor.A, selectedColor.R, selectedColor.G, selectedColor.B);

                SolidColorBrush colorBrush = new SolidColorBrush(wpfColor);

                // 버튼의 배경색으로 설정
                buttonList[index].Background = colorBrush;
                buttonColor[index] = wpfColor.ToString();
                File.WriteAllLines(ColorSettingPath, buttonColor);

                mainWindow.pointers[index].SetColorPointer(colorBrush);
                mainWindow.magnifiers[index].strokeColor = selectedColor;

                mainWindow.isDialog = false;

            }
        }

        private void ColorButton1_Click(object sender, RoutedEventArgs e)
        {
            SelectButtonColor(0);
        }

        private void ColorButton2_Click(object sender, RoutedEventArgs e)
        {
            SelectButtonColor(1);
        }

        private void ColorButton3_Click(object sender, RoutedEventArgs e)
        {
            SelectButtonColor(2);
        }

     

        private void ColorSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (mainWindow == null) return;
            foreach (PointerView pointer in mainWindow.pointers)
            {
                if(pointer.GetPointerMode() != 2)
                {
                    pointer.SetColorSize(ColorSizeSlider.Value, ColorOpacitySlider.Value);

                }
            }
            mainWindow.SavePaeameter();

        }

        private void ColorOpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (mainWindow == null) return;
            foreach (PointerView pointer in mainWindow.pointers)
            {
                if(pointer.GetPointerMode() != 2)
                {
                    pointer.SetColorSize(ColorSizeSlider.Value, ColorOpacitySlider.Value);

                }
            }
            foreach(Button button in buttonList)
            {
                button.Opacity = ColorOpacitySlider.Value / 100;
            }
            mainWindow.SavePaeameter();

        }

        private void MeetingModeSet_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Button button = sender as Button;
            button.BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 107, 0));
            button.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 107, 0));

        }

        private void MeetingModeSet_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Button button = sender as Button;
            button.BorderBrush = new SolidColorBrush(Colors.White);
            button.Foreground = new SolidColorBrush(Colors.White);
        }

        private void Label_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ColorSizeSlider.Value = ColorSizeSlider.Value - 1;
        }

        private void Label_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            ColorSizeSlider.Value = ColorSizeSlider.Value + 1;

        }

        private void Label_MouseLeftButtonDown_2(object sender, MouseButtonEventArgs e)
        {
            ColorOpacitySlider.Value = ColorOpacitySlider.Value + 1;
        }

        private void Label_MouseLeftButtonDown_3(object sender, MouseButtonEventArgs e)
        {
            ColorOpacitySlider.Value = ColorOpacitySlider.Value - 1;

        }
    }
}
