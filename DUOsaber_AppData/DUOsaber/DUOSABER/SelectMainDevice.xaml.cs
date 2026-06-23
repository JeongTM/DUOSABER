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
using static System.Net.WebRequestMethods;

namespace DUOsaber
{
    /// <summary>
    /// SelectMainDevice.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SelectMainDevice : Page
    {
        MainWindow mainWindow;
        List<RadioButton> radioButtonList;
        List<Label> labelList;
        List<Image> imageList;
        public SelectMainDevice(MainWindow main)
        {
            InitializeComponent();
            mainWindow = main;
            radioButtonList = new List<RadioButton> { Radio_0, Radio_1, Radio_2 };
            labelList = new List<Label> { FlagLabel_0, FlagLabel_1, FlagLabel_2 };
            imageList = new List<Image> { imageView0,imageView1,imageView2};
            NewSet();
            //LanguageSet();
        }

        /*public void LanguageSet()
        {
            pwd.Content = Properties.Resources.setting + ">" + Properties.Resources.changePresenter;

            Radio_0.Content = Properties.Resources.selectedAsPresenter;
            Radio_1.Content = Properties.Resources.selectedAsPresenter;
            Radio_2.Content = Properties.Resources.selectedAsPresenter;

            MoveToHome.Content = Properties.Resources.moveToHome;

            NotConnectLabel.Content = Properties.Resources.SPtext;
        }*/

        public void ResetView()
        {
            NewSet();
        }

        private void NewSet()
        {
            int cnt= 0;
            for(int i = 0; i < radioButtonList.Count; i++)
            {
                switch (mainWindow.flag[i])
                {
                    case 0:
                        radioButtonList[i].Content = Properties.Resources.selectedAsPresenter;
                        radioButtonList[i].IsChecked = true;
                        radioButtonList[i].Visibility = Visibility.Visible;
                        labelList[i].Content = Properties.Resources.presenter;
                        labelList[i].Visibility = Visibility.Visible;
                        imageList[i].Visibility= Visibility.Visible;
                        break;
                    case 4:
                        radioButtonList[i].Content = "NONE";
                        radioButtonList[i].Visibility = Visibility.Hidden;
                        labelList[i].Content = "NONE";
                        labelList[i].Visibility = Visibility.Hidden;
                        imageList[i].Visibility =Visibility.Hidden;
                        cnt++;
                        break;
                    default:
                        radioButtonList[i].Content = Properties.Resources.selectedAsPresenter;
                        radioButtonList[i].Visibility = Visibility.Visible;
                        labelList[i].Content = Properties.Resources.audience + i;
                        labelList[i].Visibility = Visibility.Visible;
                        imageList[i].Visibility = Visibility.Visible;
                        break;
                }
                imageList[i].Source = mainWindow.pointers[i].imagePointer.Source;
            }
            if(cnt == 0) NotConnectLabel.Visibility = Visibility.Hidden;
            else NotConnectLabel.Visibility = Visibility.Visible;
           
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject dependencyObject) where T : DependencyObject
        {
            if (dependencyObject != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dependencyObject); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(dependencyObject, i);
                    if (child != null && child is T t)
                    {
                        yield return t;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        


        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            
            //var selectRadioButton = MyRadioButton.Children.OfType<RadioButton>().FirstOrDefault(r => r.IsChecked == true);
            /*selectRadioButton.Content = "발표자로 선택";

            foreach (var radioButton in FindVisualChildren<RadioButton>(this))
            {
                if (radioButton.IsChecked == false)
                {
                    radioButton.Content = "발표자로 선택";
                }
            }*/
            RadioButton selectedRadioButton = (RadioButton)sender;
            int.TryParse(selectedRadioButton.Tag.ToString(), out int buttonIndex);
            for (int i = 0; i  < radioButtonList.Count; i++)
            {
                if (i == buttonIndex)
                    labelList[i].Content = Properties.Resources.presenter;
                else
                    labelList[i].Content = Properties.Resources.audience + i;

            }
            mainWindow.MainDeviceSelect(buttonIndex);

            // 선택된 라디오 버튼의 내용을 출력하거나 원하는 작업 수행
        }

        private void BackToSetting_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.SettingPageView(0);
            //mainWindow.SettingPageOn(false);
        }

        private void MatchingStartButton_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.SettingPageOn(false);
        }

        private void FlagLabel_0_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Label selectedLabel = (Label)sender;
            int.TryParse(selectedLabel.Tag.ToString(), out int buttonIndex);
            foreach (RadioButton radioButton in radioButtonList)
            {

                if (radioButton.Tag.ToString() == selectedLabel.Tag.ToString())
                {
                    radioButton.IsChecked = true;
                }
            }
        }

        private void imageView0_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Image selectedLabel = (Image)sender;
            int.TryParse(selectedLabel.Tag.ToString(), out int buttonIndex);
            foreach (RadioButton radioButton in radioButtonList)
            {

                if (radioButton.Tag.ToString() == selectedLabel.Tag.ToString())
                {
                    radioButton.IsChecked = true;
                }
            }
        }
    }
}
