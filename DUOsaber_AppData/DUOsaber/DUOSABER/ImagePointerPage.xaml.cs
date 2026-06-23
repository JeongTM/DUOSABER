using Microsoft.Win32;
using Pointer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
using static System.Windows.Forms.LinkLabel;
using Path = System.IO.Path;

namespace DUOsaber
{
    /// <summary>
    /// ImagePointerPage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ImagePointerPage : Page
    {
        //이미지
        string basePath = System.AppDomain.CurrentDomain.BaseDirectory + "CustomImage/";
        string defaultimage = System.AppDomain.CurrentDomain.BaseDirectory + "CustomImage/defaultImage.png";
        string defaultimage2 = System.AppDomain.CurrentDomain.BaseDirectory + "CustomImage/defaultImage_cyan.png";
        string defaultimage3 = System.AppDomain.CurrentDomain.BaseDirectory + "CustomImage/defaultImage_red.png";

        string folderPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DUOsaber\\CustomImage");
        //세팅 저장
        //string ImageSettingPath = System.AppDomain.CurrentDomain.BaseDirectory + "Setting/ImageSetting";
        string ImageSettingPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DUOsaber\\Setting\\ImageSetting");

        //포인팅 이미지 경로
        string[] imagePath;

        List<Button> buttons;
        List<string> imageNames;

        MainWindow mainWindow;
        public ImagePointerPage(MainWindow window)
        {
            InitializeComponent();
            mainWindow = window;

            try
            {
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                    string[] files = Directory.GetFiles(basePath);
                    
                    foreach (string file in files)
                    {
                        string fileName = Path.GetFileName(file);
                        string destFilePath = Path.Combine(folderPath, fileName);
                        File.Copy(file, destFilePath, true);
                    }

                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }

            buttons = new List<Button> { ImageButton1, ImageButton2, ImageButton3 };
            imageNames = new List<string> { defaultimage,defaultimage2,defaultimage3};

            LoadImageSet();
            //LanguageSet();
        }

        /*public void LanguageSet()
        {
            ImageLabel.Content = Properties.Resources.imagePoointer;

            ImgChangeButton0.Content = Properties.Resources.imageChange;
            ImgChangeButton1.Content = Properties.Resources.imageChange;
            ImgChangeButton2.Content = Properties.Resources.imageChange;

            SizeLabel.Content = Properties.Resources.size;
            OpacityLabel.Content = Properties.Resources.Opacity;
        }*/

        private void LoadImageSet()
        {

            try
            {
                if (File.Exists(ImageSettingPath))
                {
                    int i = 0;
                    imagePath = File.ReadAllLines(ImageSettingPath);

                    // 각 줄을 사용하는 로직을 추가합니다.
                    for (i = 0; i < buttons.Count; i++)
                    {
                        ImageSet(i, imagePath[i]);

                    }

                }
                else
                {
                    // 파일이 존재하지 않는 경우 처리할 로직을 추가합니다.
                    using (StreamWriter writer = new StreamWriter(ImageSettingPath))
                    {
                        // 파일에 작성할 내용을 여기에 추가합니다.
                        writer.WriteLine(defaultimage);
                        writer.WriteLine(defaultimage2);
                        writer.WriteLine(defaultimage3);

                    }

                    imagePath = File.ReadAllLines(ImageSettingPath);

                    for (int j = 0; j < buttons.Count; j++)
                    {
                        ImageSet(j, imageNames[j]);
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("오류");
                using (StreamWriter writer = new StreamWriter(ImageSettingPath))
                {
                    // 파일에 작성할 내용을 여기에 추가합니다.
                    writer.WriteLine(defaultimage);
                    writer.WriteLine(defaultimage2);
                    writer.WriteLine(defaultimage3);

                }
                imagePath = File.ReadAllLines(ImageSettingPath);

                for (int j = 0; j < buttons.Count; j++)
                {
                    ImageSet(j, imageNames[j]);
                }
                
            }
        }

        private void ImageSet(int pointerIndex, string path)
        {
            //버튼의 이미지를 포인팅 이미지로 적용
            /*string imagePath;
            ImageBrush brush = buttons[pointerIndex].Background as ImageBrush;
            if (brush != null)
            {
                Uri imageUri = (brush.ImageSource as BitmapImage)?.UriSource;
                if (imageUri != null)
                {
                    imagePath = imageUri.ToString();

                    pointers[pointerIndex].SetImagePointer(imagePath);

                    return;
                }
            }*/

            //버튼 이미지
            ImageBrush imageBrush = new ImageBrush();
            imageBrush.ImageSource = new BitmapImage(new Uri(path, UriKind.Relative));
            imageBrush.Stretch = Stretch.Uniform;
            buttons[pointerIndex].Background = imageBrush;

            //포인팅 이미지
            mainWindow.pointers[pointerIndex].SetImagePointer(path);



        }
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private void SetButtonView(int pointerIndex)
        {
            //이미지 선택 창
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = System.IO.Path.GetFullPath(folderPath);
            openFileDialog.Filter = "Image.Files (*.jpg, *.png, *.gif) | *.jpg; *.png; *.gif;| All files (*,*) | *.*";
            mainWindow.isDialog = true;
            try
            {
                if (openFileDialog.ShowDialog() == true)
                {
                    // 선택한 파일 경로를 이용하여 작업 수행
                    string filePath = openFileDialog.FileName;

                    imagePath[pointerIndex] = filePath;
                    File.WriteAllLines(ImageSettingPath, imagePath);


                    ImageSet(pointerIndex, filePath);
                    mainWindow.isDialog = false;

                }
            }
            catch
            {

            }

            

            
        }
        private void ImageButton1_Click(object sender, RoutedEventArgs e)
        {
            SetButtonView(0);
        }

        private void ImageButton2_Click(object sender, RoutedEventArgs e)
        {
            SetButtonView(1);
        }

        private void ImageButton3_Click(object sender, RoutedEventArgs e)
        {
            SetButtonView(2);
        }

       

     
        private void ImageSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (mainWindow == null) return;
            foreach(PointerView pointer in mainWindow.pointers)
            {
                pointer.SetImageSize(ImageSizeSlider.Value, ImageOpacitySlider.Value);
            }
            mainWindow?.SavePaeameter();
        }

        private void ImageOpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (mainWindow == null) return;
            foreach (PointerView pointer in mainWindow.pointers)
            {
                pointer.SetImageSize(ImageSizeSlider.Value, ImageOpacitySlider.Value);
            }
            foreach (Button button in buttons)
            {
                button.Opacity = ImageOpacitySlider.Value / 100;
            }
            mainWindow?.SavePaeameter();
        }

        private void MeetingModeSet_MouseEnter(object sender, MouseEventArgs e)
        {
            Button button = sender as Button;
            button.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 255, 107, 0));
            button.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 107, 0));

        }

        private void MeetingModeSet_MouseLeave(object sender, MouseEventArgs e)
        {
            Button button = sender as Button;
            button.BorderBrush = new SolidColorBrush(Colors.White);
            button.Foreground = new SolidColorBrush(Colors.White);
        }

        private void Label_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ImageSizeSlider.Value = ImageSizeSlider.Value + 1;
        }

        private void Label_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            ImageSizeSlider.Value = ImageSizeSlider.Value - 1;

        }


        private void Label_MouseLeftButtonDown_2(object sender, MouseButtonEventArgs e)
        {
            ImageOpacitySlider.Value = ImageOpacitySlider.Value - 1;
        }

        private void Label_MouseLeftButtonDown_3(object sender, MouseButtonEventArgs e)
        {
            ImageOpacitySlider.Value = ImageOpacitySlider.Value + 1;
        }
    }
    
}
