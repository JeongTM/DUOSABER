using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// SettingPage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SettingPage : Page
    {
        MainWindow mainWindow;
        FlagSet flagSet = new FlagSet();
        SelectMainDevice mainDevice;
        string CheckFilePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DUOSABER\\Setting\\CheckSetting");

        public SettingPage(MainWindow main)
        {
            InitializeComponent();
            this.mainWindow = main;
            //LanguageSet();
        }

        /*public void LanguageSet()
        {
            SettingLabel.Content = Properties.Resources.setting;

            QuickModeSwap.Content = Properties.Resources.enableQuickMode;
            CheckBox0.Content = Properties.Resources.imagePoointer;
            CheckBox1.Content = Properties.Resources.colorPointer;
            CheckBox2.Content = Properties.Resources.magnifier;
            CheckBox3.Content = Properties.Resources.spotlight;

            MoveToHome.Content = Properties.Resources.moveToHome;
        }*/


        private void BackToSetting_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.SettingPageOn(false);
        }

        private void ChangeMaster_Click(object sender, RoutedEventArgs e)
        {
            if (mainWindow.device != null)
                mainDevice = new SelectMainDevice(mainWindow);
            mainWindow.PointerFrame.Content = mainDevice;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;

            if (int.TryParse(checkBox.Tag.ToString(), out int tagValue))
            {
                // 태그 값을 정수로 변환하여 사용
                if (mainWindow != null)
                    mainWindow.modes[tagValue] = (bool)checkBox.IsChecked;
            }
            if (mainWindow == null) return;

            mainWindow.SavePaeameter();
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {

            CheckBox checkBox = (CheckBox)sender;
            
            if(mainWindow.modes.Count(value => value == true) <= 1)
            {
                checkBox.IsChecked = true;
                return;
            }

            if (int.TryParse(checkBox.Tag.ToString(), out int tagValue))
            {
                // 태그 값을 정수로 변환하여 사용
                if (mainWindow != null)
                    mainWindow.modes[tagValue] = (bool)checkBox.IsChecked;
            }
            if (mainWindow == null) return;

            mainWindow.SavePaeameter();
        }


        private void MatchingStartButton_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.SettingPageOn(false);
        }

        private void languageSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            ComboBoxItem selectedItem = (ComboBoxItem)comboBox.SelectedItem;
            if (selectedItem == null) return;
            Console.WriteLine(selectedItem.Tag.ToString());
            mainWindow.LanguageSet(selectedItem.Tag.ToString());
        }


        private void Slider_ValueChanged_1(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (mainWindow == null) return;
            Slider slider = (Slider)sender;
            mainWindow.magRefresh = 12 - (int)slider.Value;
            mainWindow.magnifier.TimerInterval = (12 - (int)slider.Value) * 10;
            mainWindow.SavePaeameter();

        }                                                                                                                                                        

        

        
        private void checkboxJustMaster_Click(object sender, RoutedEventArgs e)
        {
            if (mainWindow == null) return;
            CheckBox checkBox = (CheckBox)sender;
            if (checkBox.IsChecked == true && checkboxEvryone != null)
            {
                checkboxEvryone.IsChecked = false;
                mainWindow.isOldMag = true;
                mainWindow.SavePaeameter();
                int masterIndex = Array.IndexOf(mainWindow.flag, 0) == -1 ? 0 : Array.IndexOf(mainWindow.flag, 0);
                int cnt = 0;
                foreach (PointerView pointer in mainWindow.pointers)
                {
                    if(pointer.GetPointerMode() == 2 && cnt != masterIndex)
                    {
                        pointer.PointingModeSet(1);
                    }
                    cnt++;
                }
            }
            else
            {
                checkBox.IsChecked = true;
            }
        }

        private void checkboxEvryone_Click(object sender, RoutedEventArgs e)
        {
            if (mainWindow == null) return;
            CheckBox checkBox = (CheckBox)sender;
            if (checkBox.IsChecked == true && checkboxJustMaster != null)
            {
                checkboxJustMaster.IsChecked = false;
                mainWindow.isOldMag = false;
                mainWindow.SavePaeameter();
                
            }
            else
            {
                checkBox.IsChecked = true;
            }
        }

        private void cursorHide_Checked(object sender, RoutedEventArgs e)
        {
            if (mainWindow == null) return;
            mainWindow.ismaster = false;
            mainWindow.SavePaeameter();
        }

        private void cursorHide_Unchecked(object sender, RoutedEventArgs e)
        {
            if (mainWindow == null) return;
            mainWindow.ismaster = true;
            mainWindow.SavePaeameter();
        }

        private void alwaysCenterCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (mainWindow == null) return;
            mainWindow.alwaysCenter = true;
            mainWindow.SavePaeameter();
        }

        private void alwaysCenterCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (mainWindow == null) return;
            mainWindow.alwaysCenter = false;
            mainWindow.SavePaeameter();
        }
    }
}
