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
    /// ChMatchPage1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ChMatchPage1 : Page
    {
        MainWindow mainWindow;
        FlagSet flagSet = new FlagSet();
        public ChMatchPage1(MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
            LanguageSet();
        }

        public void LanguageSet()
        {
            pwd.Content = Properties.Resources.setting + ">" + Properties.Resources.deviceMatching;

            CMtext1.Content = Properties.Resources.preparingForDeviceMatching;
            CMtext1_1.Content = Properties.Resources.CMtext1_1;
            CMtext1_2.Content = Properties.Resources.CMtext1_2;

            CMtext2.Content = Properties.Resources.checkForDeviceMatching;
            CMtext2_1.Content = Properties.Resources.CMtext2_1;
            CMtext2_2.Content = Properties.Resources.CMtext2_2;

            MatchingStartButton.Content = Properties.Resources.next;
            MatchingEndButton.Content = Properties.Resources.next;


        }



        private void MatchingStartButton_Click(object sender, RoutedEventArgs e)
        {
            if (mainWindow.device != null)
            {
                foreach (PointerView pointerView in mainWindow.pointers)
                {
                    switch (pointerView.GetPointerMode())
                    {
                        case 2:
                            mainWindow.magnifier.MagnifierOff(); 
                            break;
                        case 3:
                            mainWindow.spotlightForm.Hide();
                            break;
                        default:
                            pointerView.Visibility = Visibility.Collapsed;
                            break;

                    }
                }
                flagSet.ChannelMode(0x0f, mainWindow.device);
                MatchingStartButton.Visibility = Visibility.Hidden;
                MatchingLabel1.Visibility = Visibility.Hidden;
                MatchingEndButton.Visibility = Visibility.Visible;
                MatchingLabel2.Visibility = Visibility.Visible;

            }
            else
                Console.WriteLine("디바이스 없음");
        }

        private void MatchingEndButton_Click(object sender, RoutedEventArgs e)
        {
            if (mainWindow.device != null)
            {
                flagSet.ChannelMode(0x00, mainWindow.device);
                MatchingEndButton.Visibility = Visibility.Hidden;
                MatchingLabel2.Visibility = Visibility.Hidden;
                MatchingExitButton.Visibility = Visibility.Visible;
                MatchingLabel3.Visibility = Visibility.Visible;
                mainWindow.ConnectDevice();
                mainWindow.SettingPageView(1);
            }
            else
                Console.WriteLine("디바이스 인식 안됨");
        }

        private void MatchingExitButton_Click(object sender, RoutedEventArgs e)
        {
            //mainWindow.PointerFrame.Content = mainWindow.settingPage;
            MatchingStartButton.Visibility = Visibility.Visible;
            MatchingLabel1.Visibility = Visibility.Visible;
            MatchingExitButton.Visibility = Visibility.Hidden;
            MatchingLabel3.Visibility = Visibility.Hidden;

            mainWindow.SettingPageOn(false);
        }

        private void BackToSetting_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.SettingPageView(0);
            //mainWindow.SettingPageOn(false);

        }
    }
}
