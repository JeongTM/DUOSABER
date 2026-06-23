using HidLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Interop;
using System.IO;
using System.Windows.Forms;
using System.Windows.Threading;
using Label = System.Windows.Controls.Label;
using Pointer;
using Button = System.Windows.Controls.Button;
using MessageBox = System.Windows.MessageBox;
using System.Globalization;
using System.Resources;

namespace DUOsaber
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        //마우스 위치 변경
        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        //마우스 동작
        [DllImport("user32.dll")]
        static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInf);

        private const uint LBUTTONDOWN = 0x0002; // 왼쪽 마우스 버튼 눌림
        private const uint LBUTTONUP = 0x0004; // 왼쪽 마우스 버튼 떼어짐

        private const uint RBUTTONDOWN = 0x0008; // 오른쪽 마우스 버튼 눌림
        private const uint RBUTTONUP = 0x00010; // 오른쪽 마우스 버튼 떼어짐

        private const uint ABSOLUTEMOVE = 0x8000; // 전역 위치

        //디바이스 객체
        public HidDevice device;

        //페이지
        ColorPointerPage colorPointerPage;
        ImagePointerPage imagePointerPage;
        MagnifierPage magnifierPage;
        SpotLightPage spotLightPage;
        public SettingPage settingPage;
        SelectMainDevice selectMainDevice;
        List<Button> PageButtons;
        ChMatchPage1 ChMatchPage1;
        public FormSpotlight spotlightForm = new FormSpotlight();

        ModeLabel modeLabel = new ModeLabel();


        //포인팅 이미지
        public PointerView pointer1 = new PointerView();
        public PointerView pointer2 = new PointerView();
        public PointerView pointer3 = new PointerView();
        public List<PointerView> pointers;

        public FormMagnifier magnifier1 = new FormMagnifier();
        public FormMagnifier magnifier2 = new FormMagnifier();
        public FormMagnifier magnifier3 = new FormMagnifier();
        public List<FormMagnifier> magnifiers;


        //연결 디바이스 정보
        public int[] flag = new int[3] { 4, 4, 4 };
        bool[] isInput = new bool[3] { false, false, false };
        public int[] deviceID = new int[3] { 500, 500, 500 };
        //string folderPath = System.AppDomain.CurrentDomain.BaseDirectory + "Setting/";
        //string filePath = System.AppDomain.CurrentDomain.BaseDirectory + "Setting/FlagSetting";
        public string folderPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DUOSABER\\Setting");
        string filePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DUOSABER\\Setting\\FlagSetting");
        string parameterFilePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DUOSABER\\Setting\\ParameterSetting");

        public Dictionary<string, int[]> DeviceFlag = new Dictionary<string, int[]>();
        List<Label> flagLabal;
        List<Label> arrowLabel;

        //USB 이벤트
        private const int WM_DEVICECHANGE = 0x0219;

        //기본 포인팅이미지
        string basePath = System.AppDomain.CurrentDomain.BaseDirectory + "CustomPointer/";

        //포인터 모드
        public const int MODE_IMAGE = 0;
        public const int MODE_COLOR = 1;
        public const int MODE_MAGNIFIER = 2;
        public const int MODE_SPOTLIGHT = 3;
        public bool[] modes = new bool[4] { true, true, true, true };
        public int[] modeIndex = new int[4] { MODE_IMAGE, MODE_COLOR, MODE_MAGNIFIER, MODE_SPOTLIGHT };
        //디바이스 권한
        const byte FLAG_MASTER = 0;
        const byte FLAG_SLAVE = 1;
        const byte FLAG_BLOCK = 2;

        const int PTMODE_ON = 0;
        const int PTMODE_OFF = 1;
        const int PTMODE_CHANGE = 2;

        public delegate void ReadHandlerDelegate(HidReport report);
        FlagSet flagSet = new FlagSet();

        DispatcherTimer timerChangeMode = new DispatcherTimer();
        DispatcherTimer timerMoveWindow = new DispatcherTimer();
        DispatcherTimer timerAutoHide = new DispatcherTimer();
        DispatcherTimer timerAutoHideSub = new DispatcherTimer();


        bool isFull = false;
        bool isPTmode = false;
        bool isReConnect = false;
        bool isDevice = false;
        bool isDown = false;

        int idIndex = 0;
        int dx = 0, dy = 0;
        int cntPTstack = 0;
        int cntTimer = 0;
        int[] indexCnt = new int[3] { 0, 0, 0 };

        //특수포인터
        public MagnifierGlass magnifier = new MagnifierGlass();
        //public SpotlightBase spotlight = new SpotlightBase();

        //시스템 트레이
        NotifyIcon ni = new NotifyIcon();
        OverlapBlock overlap;


        public MainWindow()
        {
            InitializeComponent();

            try
            {
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }

            pointers = new List<PointerView> { pointer1, pointer2, pointer3 };
            magnifiers = new List<FormMagnifier> { magnifier1, magnifier2, magnifier3 };
            flagLabal = new List<Label> { Flag_A, Flag_B, Flag_C };
            arrowLabel = new List<Label> { arrow0, arrow1, arrow2, arrow3 };

            imagePointerPage = new ImagePointerPage(this);
            colorPointerPage = new ColorPointerPage(this);
            settingPage = new SettingPage(this);
            selectMainDevice = new SelectMainDevice(this);
            ChMatchPage1 = new ChMatchPage1(this);
            PageButtons = new List<Button> { ImagePageButton, PointPageButton, MagnifierPageButton, SpotLightPageButton };

            overlap = new OverlapBlock(this);



            timerChangeMode.Interval = TimeSpan.FromMilliseconds(1000);
            timerChangeMode.Tick += TimerChangeMode_Tick;

            timerMoveWindow.Interval = TimeSpan.FromMilliseconds(1);
            timerMoveWindow.Tick += TimerMoveWindow_Tick;

            timerAutoHide.Interval = TimeSpan.FromMilliseconds(1000);
            timerAutoHide.Tick += TimerAutoHide_Tick;

            timerAutoHideSub.Interval = TimeSpan.FromMilliseconds(1000);
            timerAutoHideSub.Tick += TimerAutoHideSub_Tick;

            ConnectDevice();

            SetNotification();

            spotLightPage = new SpotLightPage(this);
            magnifierPage = new MagnifierPage(this);
            //spotlightForm.setSpotlight(100, 0.4);
            //spotlightForm.Show();

            overlap.OverlapSearch();
            PTmodeSetFuntion(PTMODE_OFF);
            LodePaeameter();
            ChangePointerMode(MODE_IMAGE);
            LanguageSet();
            LoadFlagSet();

            /*magnifier.ResizeMagnifier(500);
            magnifier.Magnification = 3.0f;
            magnifier.TimerInterval = 30;
            MagPointerOld(true);*/
        }




        private void LoadFlagSet()
        {

            try
            {
                if (File.Exists(filePath))
                {
                    string[] lines = File.ReadAllLines(filePath);
                    // 각 줄을 사용하는 로직을 추가합니다.
                    foreach (string line in lines)
                    {
                        string[] words = line.Split(new char[] { '-', '=' });
                        if (words.Length != 3) continue;
                        DeviceFlag[words[0]] = new int[] { int.Parse(words[1]), int.Parse(words[2]) };

                        if (isInput[DeviceFlag[words[0]][0]] == false)
                        {
                            deviceID[DeviceFlag[words[0]][0]] = int.Parse(words[0]);
                            flag[DeviceFlag[words[0]][0]] = DeviceFlag[words[0]][1];
                            isInput[DeviceFlag[words[0]][0]] = true;
                            FlagTextSet(words[0]);
                        }
                    }


                }
                else
                {
                    // 파일이 존재하지 않는 경우 처리할 로직을 추가합니다.
                    using (StreamWriter writer = new StreamWriter(filePath))
                    {
                        // 파일에 작성할 내용을 여기에 추가합니다.
                        /*writer.WriteLine("500=0");
                        writer.WriteLine("500=1");
                        writer.WriteLine("500=1");*/
                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    Console.WriteLine("초기화");
                }
            }
        }

        public Dictionary<int, List<double>> paeameterInfo = new Dictionary<int, List<double>>();


        private void LodePaeameter()
        {
            try
            {
                if (File.Exists(parameterFilePath))
                {
                    string[] lines = File.ReadAllLines(parameterFilePath);

                    // 각 줄을 사용하는 로직을 추가합니다.
                    foreach (string line in lines)
                    {
                        string[] words = line.Split('-');
                        paeameterInfo[int.Parse(words[0])] = new List<double>();

                        for (int i = 1; i < words.Length; i++)
                        {
                            //Console.WriteLine(words[i]);
                            paeameterInfo[int.Parse(words[0])].Add(double.Parse(words[i]));
                            Console.WriteLine(double.Parse(words[i]));

                        }

                    }
                    imagePointerPage.ImageSizeSlider.Value = paeameterInfo[MODE_IMAGE][0];
                    imagePointerPage.ImageOpacitySlider.Value = paeameterInfo[MODE_IMAGE][1];
                    colorPointerPage.ColorSizeSlider.Value = paeameterInfo[MODE_COLOR][0];
                    colorPointerPage.ColorOpacitySlider.Value = paeameterInfo[MODE_COLOR][1];
                    magnifierPage.MagnifierSizeSlider.Value = paeameterInfo[MODE_MAGNIFIER][0];
                    magnifierPage.MagnificationSlider.Value = paeameterInfo[MODE_MAGNIFIER][1];
                    spotLightPage.SpotlightSizeSlider.Value = paeameterInfo[MODE_SPOTLIGHT][0];
                    spotLightPage.BaseOpacitySlider.Value = paeameterInfo[MODE_SPOTLIGHT][1];
                    settingPage.languageSelect.SelectedIndex = (int)paeameterInfo[4][0];
                    settingPage.refreshSlider.Value = (int)paeameterInfo[5][0];
                    if ((int)paeameterInfo[6][0] == 0)
                    {
                        settingPage.checkboxEvryone.IsChecked = true;
                        settingPage.checkboxJustMaster.IsChecked = false;
                        isOldMag = false;

                    }
                    switch ((int)paeameterInfo[7][0])
                    {
                        case 0: 
                            settingPage.customPointerCheckBox.IsChecked = false;
                            break;
                        case 1:
                            settingPage.customPointerCheckBox.IsChecked = true;
                            break;
                    }
                    switch ((int)paeameterInfo[7][1])
                    {
                        case 0:
                            settingPage.colorPointerCheckBox.IsChecked = false;
                            break;
                        case 1:
                            settingPage.colorPointerCheckBox.IsChecked = true;
                            break;
                    }
                    switch ((int)paeameterInfo[7][2])
                    {
                        case 0:
                            settingPage.highliterCheckBox.IsChecked = false;
                            break;
                        case 1:
                            settingPage.highliterCheckBox.IsChecked = true;
                            break;
                    }
                    switch ((int)paeameterInfo[7][3])
                    {
                        case 0:
                            settingPage.magnifireCheckBox.IsChecked = false;
                            break;
                        case 1:
                            settingPage.magnifireCheckBox.IsChecked = true;
                            break;
                    }

                    switch ((int)paeameterInfo[8][0])
                    {
                        case 0:
                            settingPage.cursorHide.IsChecked = false;
                            break;
                        case 1:
                            settingPage.cursorHide.IsChecked = true;
                            break;
                    }

                    switch ((int)paeameterInfo[8][1])
                    {
                        case 0:
                            settingPage.alwaysCenterCheckBox.IsChecked = false;
                            break;
                        case 1:
                            settingPage.alwaysCenterCheckBox.IsChecked = true;
                            break;
                    }
                    Console.WriteLine((int)paeameterInfo[6][0] + "adasdasd");
                }
                else
                {
                    using (StreamWriter writer = new StreamWriter(parameterFilePath))
                    {
                        Console.WriteLine("초기화");

                        writer.WriteLine(MODE_IMAGE + "-" + imagePointerPage.ImageSizeSlider.Value + "-" + imagePointerPage.ImageOpacitySlider.Value);
                        writer.WriteLine(MODE_COLOR + "-" + colorPointerPage.ColorSizeSlider.Value + "-" + colorPointerPage.ColorOpacitySlider.Value);
                        writer.WriteLine(MODE_MAGNIFIER + "-" + magnifierPage.MagnifierSizeSlider.Value + "-" + magnifierPage.MagnificationSlider.Value);
                        writer.WriteLine(MODE_SPOTLIGHT + "-" + spotLightPage.SpotlightSizeSlider.Value + "-" + spotLightPage.BaseOpacitySlider.Value);
                        if (Thread.CurrentThread.CurrentUICulture.Name == "ko-KR")
                            writer.WriteLine(4 + "-" + 0);
                        else if (Thread.CurrentThread.CurrentUICulture.Name == "en-US")
                            writer.WriteLine(4 + "-" + 1);
                        else if (Thread.CurrentThread.CurrentUICulture.Name == "ja-JP")
                            writer.WriteLine(4 + "-" + 2);
                        else
                            writer.WriteLine(4 + "-" + 1);
                        writer.WriteLine(5 + "-" + 3);
                        writer.WriteLine(6 + "-" + 0);
                        writer.WriteLine(7 + "-" + 1 + "-" + 1 + "-" + 1 + "-" + 1);
                        writer.WriteLine(8 + "-" + 0 + "-" + 0);


                    }
                }
            }
            catch (Exception e)
            {
                using (StreamWriter writer = new StreamWriter(parameterFilePath))
                {
                    Console.WriteLine("초기화111");

                    writer.WriteLine(MODE_IMAGE + "-" + imagePointerPage.ImageSizeSlider.Value + "-" + imagePointerPage.ImageOpacitySlider.Value);
                    writer.WriteLine(MODE_COLOR + "-" + colorPointerPage.ColorSizeSlider.Value + "-" + colorPointerPage.ColorOpacitySlider.Value);
                    writer.WriteLine(MODE_MAGNIFIER + "-" + magnifierPage.MagnifierSizeSlider.Value + "-" + magnifierPage.MagnificationSlider.Value);
                    writer.WriteLine(MODE_SPOTLIGHT + "-" + spotLightPage.SpotlightSizeSlider.Value + "-" + spotLightPage.BaseOpacitySlider.Value);
                    if (Thread.CurrentThread.CurrentUICulture.Name == "ko-KR")
                        writer.WriteLine(4 + "-" + 0);
                    else if (Thread.CurrentThread.CurrentUICulture.Name == "en-US")
                        writer.WriteLine(4 + "-" + 1);
                    else if (Thread.CurrentThread.CurrentUICulture.Name == "ja-JP")
                        writer.WriteLine(4 + "-" + 2);
                    else
                        writer.WriteLine(4 + "-" + 1);
                    writer.WriteLine(5 + "-" + 3);
                    writer.WriteLine(6 + "-" + 0);
                    writer.WriteLine(7 + "-" + 1 + "-" + 1 + "-" + 1 + "-" + 1);
                    writer.WriteLine(8 + "-" + 0 + "-" + 0);

                }
                //MessageBox.Show(e.Message);
            }
        }

        public void SavePaeameter()
        {
            List<string> lines = new List<string>();
            lines.Add(MODE_IMAGE + "-" + imagePointerPage.ImageSizeSlider.Value + "-" + imagePointerPage.ImageOpacitySlider.Value);
            lines.Add(MODE_COLOR + "-" + colorPointerPage.ColorSizeSlider.Value + "-" + colorPointerPage.ColorOpacitySlider.Value);
            lines.Add(MODE_MAGNIFIER + "-" + magnifierPage.MagnifierSizeSlider.Value + "-" + magnifierPage.MagnificationSlider.Value);
            lines.Add(MODE_SPOTLIGHT + "-" + spotLightPage.SpotlightSizeSlider.Value + "-" + spotLightPage.BaseOpacitySlider.Value);
            lines.Add(4 + "-" + (settingPage.languageSelect.SelectedIndex == -1 ? 0 : settingPage.languageSelect.SelectedIndex));
            lines.Add(5 + "-" + settingPage.refreshSlider.Value);
            if (isOldMag == true) { lines.Add(6 + "-" + 1); }
            else { lines.Add(6 + "-" + 0); }

            string quickCheckBox = "7";
            switch (settingPage.customPointerCheckBox.IsChecked)
            {
                case true:
                    quickCheckBox = quickCheckBox + "-1";
                    break;
                case false:
                    quickCheckBox = quickCheckBox + "-0";
                    break;
            }
            switch (settingPage.colorPointerCheckBox.IsChecked)
            {
                case true:
                    quickCheckBox = quickCheckBox + "-1";
                    break;
                case false:
                    quickCheckBox = quickCheckBox + "-0";
                    break;
            }
            switch (settingPage.highliterCheckBox.IsChecked)
            {
                case true:
                    quickCheckBox = quickCheckBox + "-1";
                    break;
                case false:
                    quickCheckBox = quickCheckBox + "-0";
                    break;
            }
            switch (settingPage.magnifireCheckBox.IsChecked)
            {
                case true:
                    quickCheckBox = quickCheckBox + "-1";
                    break;
                case false:
                    quickCheckBox = quickCheckBox + "-0";
                    break;
            }

            lines.Add(quickCheckBox);

            string basicSetting = "8";

            switch (settingPage.cursorHide.IsChecked)
            {
                case true:
                    basicSetting = basicSetting + "-1";
                    break;
                case false:
                    basicSetting = basicSetting + "-0";
                    break;
            }
            switch (settingPage.alwaysCenterCheckBox.IsChecked)
            {
                case true:
                    basicSetting = basicSetting + "-1";
                    break;
                case false:
                    basicSetting = basicSetting + "-0";
                    break;
            }

            lines.Add(basicSetting);

            try
            {
                if (File.Exists(parameterFilePath))
                {
                    File.WriteAllLines(parameterFilePath, lines.ToArray());
                }
                else
                {
                    using (StreamWriter writer = new StreamWriter(parameterFilePath))
                    {
                        foreach (string line in lines)
                            writer.WriteLine(line);
                    }
                }
            }
            catch (Exception ex)
            {
                using (StreamWriter writer = new StreamWriter(parameterFilePath))
                {
                    Console.WriteLine("초기화");

                    writer.WriteLine(MODE_IMAGE + "-" + imagePointerPage.ImageSizeSlider.Value + "-" + imagePointerPage.ImageOpacitySlider.Value);
                    writer.WriteLine(MODE_COLOR + "-" + colorPointerPage.ColorSizeSlider.Value + "-" + colorPointerPage.ColorOpacitySlider.Value);
                    writer.WriteLine(MODE_MAGNIFIER + "-" + magnifierPage.MagnifierSizeSlider.Value + "-" + magnifierPage.MagnificationSlider.Value);
                    writer.WriteLine(MODE_SPOTLIGHT + "-" + spotLightPage.SpotlightSizeSlider.Value + "-" + spotLightPage.BaseOpacitySlider.Value);
                    if (Thread.CurrentThread.CurrentUICulture.Name == "ko-KR")
                        writer.WriteLine(4 + "-" + 0);
                    else if (Thread.CurrentThread.CurrentUICulture.Name == "en-US")
                        writer.WriteLine(4 + "-" + 1);
                    else if (Thread.CurrentThread.CurrentUICulture.Name == "ja-JP")
                        writer.WriteLine(4 + "-" + 2);
                    else
                        writer.WriteLine(4 + "-" + 1);
                    writer.WriteLine(5 + "-" + 3);
                    writer.WriteLine(6 + "-" + 0);
                    writer.WriteLine(7 + "-" + 1 + "-" + 1 + "-" + 1 + "-" + 1);
                    writer.WriteLine(8 + "-" + 0 + "-" + 0);
                }
            }

        }

        private void SaveFlagSet()
        {
            List<string> lines = new List<string>();

            foreach (int key in deviceID)
            {
                if (key == 500) continue;
                string flagSet = key.ToString();
                lines.Add(flagSet + '-' + DeviceFlag[flagSet][0] + '=' + DeviceFlag[flagSet][1]);
            }


            if (!File.Exists(filePath))
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    foreach (string line in lines)
                        writer.WriteLine(line);
                }
            }
            else File.WriteAllLines(filePath, lines.ToArray());


        }

        public void LanguageSet(string language = null)
        {
            if (language != null)
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(language);

            EditButton.Content = Properties.Resources.home;
            SettingButton.Content = Properties.Resources.setting;

            ImagePageButton.Content = "  " + Properties.Resources.imagePoointer;
            PointPageButton.Content = "  " + Properties.Resources.colorPointer;
            MagnifierPageButton.Content = "  " + Properties.Resources.magnifier;
            SpotLightPageButton.Content = "  " + Properties.Resources.spotlight;

            PTmodeSet.Content = Properties.Resources.PTmode;
            MeetingModeSet.Content = Properties.Resources.generalMode;

            settingMain.Content = Properties.Resources.setting;
            SettingPanelButton.Content = Properties.Resources.settingMain;
            SelectMainDevButton.Content = Properties.Resources.changePresenter;
            NewDeviceButton.Content = Properties.Resources.deviceMatching;

            plzConnection.Content = Properties.Resources.connectionFailedMent;
            connectionFaild.Content = Properties.Resources.connectionFailed;

            foreach (var item in flagLabal)
            {
                item.Content = Properties.Resources.none;
            }

            //컬러포인터
            colorPointerPage.ColorLabel.Content = Properties.Resources.colorPointer;

            colorPointerPage.ColorChangeButton0.Content = Properties.Resources.colorChange;
            colorPointerPage.ColorChangeButton1.Content = Properties.Resources.colorChange;
            colorPointerPage.ColorChangeButton2.Content = Properties.Resources.colorChange;

            colorPointerPage.SizeLabel.Content = Properties.Resources.size;
            colorPointerPage.OpacityLabel.Content = Properties.Resources.Opacity;

            //이미지포인터
            imagePointerPage.ImageLabel.Content = Properties.Resources.imagePoointer;

            imagePointerPage.ImgChangeButton0.Content = Properties.Resources.imageChange;
            imagePointerPage.ImgChangeButton1.Content = Properties.Resources.imageChange;
            imagePointerPage.ImgChangeButton2.Content = Properties.Resources.imageChange;

            imagePointerPage.SizeLabel.Content = Properties.Resources.size;
            imagePointerPage.OpacityLabel.Content = Properties.Resources.Opacity;

            //확대경
            magnifierPage.MagLabel.Content = Properties.Resources.magnifier;

            magnifierPage.SizeLabel.Content = Properties.Resources.size;
            magnifierPage.MagnificationLabel.Content = Properties.Resources.Magnification;

            //스포트라이트
            spotLightPage.SpotlightLabel.Content = Properties.Resources.spotlight;

            spotLightPage.SizeLabel.Content = Properties.Resources.size;
            spotLightPage.OpacityLabel.Content = Properties.Resources.Opacity;

            //설정화면
            settingPage.SettingLabel.Content = Properties.Resources.setting;

            settingPage.QuickModeSwap.Content = Properties.Resources.enableQuickMode;
            settingPage.CheckBox0.Content = Properties.Resources.imagePoointer;
            settingPage.CheckBox1.Content = Properties.Resources.colorPointer;
            settingPage.CheckBox2.Content = Properties.Resources.magnifier;
            settingPage.CheckBox3.Content = Properties.Resources.spotlight;

            settingPage.magSetLabel.Content = Properties.Resources.magnifierSetting;
            settingPage.refreshLabel.Content = Properties.Resources.refreshRate;
            settingPage.refreshLabel_High.Content = Properties.Resources.high;
            settingPage.refreshLabel_Low.Content = Properties.Resources.low;
            settingPage.useableLabel.Content = Properties.Resources.usable;
            settingPage.magUserSelectLabel1.Content = Properties.Resources.masterOnly;
            settingPage.magUserSelectLabel2.Content = Properties.Resources.evryone;

            settingPage.MoveToHome.Content = Properties.Resources.moveToHome;
            settingPage.LanguageLabel.Content = Properties.Resources.Language;
            settingPage.cursorSetting.Content = Properties.Resources.cursorSetting;
            settingPage.cursorHideLabel.Content = Properties.Resources.cursorHide;
            settingPage.alwaysCenterLabel.Content = Properties.Resources.alwaysCenter;

            //발표자 설정
            selectMainDevice.pwd.Content = Properties.Resources.setting + ">" + Properties.Resources.changePresenter;

            selectMainDevice.Radio_0.Content = Properties.Resources.selectedAsPresenter;
            selectMainDevice.Radio_1.Content = Properties.Resources.selectedAsPresenter;
            selectMainDevice.Radio_2.Content = Properties.Resources.selectedAsPresenter;

            selectMainDevice.MoveToHome.Content = Properties.Resources.moveToHome;

            selectMainDevice.NotConnectLabel.Content = Properties.Resources.SPtext;

            //체널매칭페이지
            ChMatchPage1.pwd.Content = Properties.Resources.setting + ">" + Properties.Resources.deviceMatching;

            ChMatchPage1.CMtext1.Content = Properties.Resources.preparingForDeviceMatching;
            ChMatchPage1.CMtext1_1.Content = Properties.Resources.CMtext1_1;
            ChMatchPage1.CMtext1_2.Content = Properties.Resources.CMtext1_2;

            ChMatchPage1.CMtext2.Content = Properties.Resources.checkForDeviceMatching;
            ChMatchPage1.CMtext2_1.Content = Properties.Resources.CMtext2_1;
            ChMatchPage1.CMtext2_2.Content = Properties.Resources.CMtext2_2;

            ChMatchPage1.MatchingStartButton.Content = Properties.Resources.next;
            ChMatchPage1.MatchingEndButton.Content = Properties.Resources.next;

            //모드라벨
            modeLabel.GenText.Content = Properties.Resources.generalMode;
            modeLabel.PTlText.Content = Properties.Resources.PTmode;

            SetNotification();
        }

        private string[] GetDevicePath()
        {
            HidDevice[] deviceList = HidDevices.Enumerate(0x0ebc, 0x2501, 0).ToArray();
            List<string> devices = new List<string>();
            foreach (HidDevice hid in deviceList)
            {
                if (!hid.DevicePath.Contains("mi_02")) continue;
                devices.Add(hid.DevicePath);
            }
            return devices.ToArray();
        }


        public void ConnectDevice()
        {
            //현재 연결되어있는 디바이스 목록
            HidDevice[] deviceList = HidDevices.Enumerate(0x0ebc, 0x2501, 0).ToArray();
            string[] devName = GetDevicePath();
            isDevice = false;


            foreach (HidDevice dev in deviceList)
            {

                if (!dev.DevicePath.Contains("mi_02")) continue;


                if (device != null)
                {
                    if (Array.IndexOf(devName, dev.DevicePath) != -1)
                    {
                        deviceID = new int[3] { 500, 500, 500 };
                        flag = new int[3] { 4, 4, 4 };
                        isFull = false;
                        isDevice = true;
                        break;
                    }
                    selectMainDevice.ResetView();

                    device.Dispose();
                    device = null;
                }

                flagSet.SetFlagHandler(1, dev);

                //MessageBox.Show(ReceiverVer(dev));


                dev.OpenDevice();
                dev.ReadReport(OnReport);
                device = dev;

                deviceID = new int[3] { 500, 500, 500 };
                flag = new int[3] { 4, 4, 4 };
                isFull = false;




                isDevice = true;
                break;

            }
            //Console.WriteLine(isDevice);

            if (isDevice == false)
            {
                if (device != null)
                {
                    device.Dispose();
                    device = null;
                }


                System.IO.Stream iconStream = System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/Resources/DUONicon.ico")).Stream;
                System.Drawing.Icon icon = new System.Drawing.Icon(iconStream);
                ni.Icon = icon;
            }
            else
            {
                System.IO.Stream iconStream = System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/Resources/DUOicon.ico")).Stream;
                System.Drawing.Icon icon = new System.Drawing.Icon(iconStream);
                ni.Icon = icon;
            }

            if (device != null)
            {
                NotConnect.Visibility = Visibility.Hidden;
                //FrameResize();
                if (isPTmode) { PTmodeSetFuntion(PTMODE_ON); }
                else { PTmodeSetFuntion(PTMODE_OFF); }


            }
            else
            {
                if (PointerFrame.Content == ChMatchPage1)
                    ChangePointerMode(MODE_IMAGE);
                NotConnect.Visibility = Visibility.Visible;
                Height = 600;
                modeLabel.Visibility = Visibility.Collapsed;
            }

            isReConnect = false;


        }

        string receiverVer;    //수신기 버전


        private string ReceiverVer(HidDevice hid)
        {
            byte[] bs;
            hid.ReadProduct(out bs);

            try
            {
                string ps = "";                                 //수신기 모델 명
                foreach (byte b in bs)
                {
                    if (b > 0)
                    {
                        ps += ((char)b).ToString();
                    }
                }
                receiverVer = ps.Substring(ps.Length - 3, 3);    //수신기 버전

                return receiverVer;

            }
            catch (Exception e)
            {
                return receiverVer = "none";

            }


        }

        private void SetNotification()
        {
            // NotifyIcon 생성


            // NotifyIcon에 사용할 ico 파일 경로입니다. 절대 경로로 설정해 주시면 됩니다.
            // 설정하지 않을 경우 아이콘이 보이지 않습니다.
            System.IO.Stream iconStream = System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/Resources/DUOicon.ico")).Stream;
            System.Drawing.Icon icon = new System.Drawing.Icon(iconStream);
            ni.Icon = icon;


            // 팝업 메뉴 설정
            ContextMenuStrip contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add(Properties.Resources.setting, null, OnOpen);
            contextMenu.Items.Add(Properties.Resources.exit, null, OnExit);
            ni.DoubleClick += Ni_DoubleClick;
            ni.ContextMenuStrip = contextMenu;
            // NotifyIcon이 보일 수 있도록 Visible 항목을 true로 설정합니다.
            // 기본설정이 false이기 때문에 반드시 true로 설정해주셔야합니다.
            ni.Visible = true;

            // NotifyIcon에 마우스를 올릴 경우 나타나는 글입니다.
            // 설정 안해주셔도됩니다.
            ni.Text = "DUOSABER_v1.2";

        }

        private void Ni_DoubleClick(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = WindowState.Normal;
            Topmost = true;
            Topmost = false;
            ShowInTaskbar = true;
        }

        private void OnOpen(object sender, EventArgs e)
        {
            // 프로그램 열기 메뉴를 클릭했을 때 처리할 작업
            this.Show();
            this.WindowState = WindowState.Normal;
            Topmost = true;
            Topmost = false;
            ShowInTaskbar = true;
        }

        private void OnExit(object sender, EventArgs e)
        {
            // 종료 메뉴를 클릭했을 때 처리할 작업
            flagSet.ExitFuntion();
            ni.Visible = false;
            SavePaeameter();
            Close();
            System.Windows.Forms.Application.Exit();
            //System.Windows.Application.Current.Shutdown();
        }

        private void OnReport(HidReport report)
        {
            while (isReConnect != false) ;
            object[] myArray = new object[1];
            myArray[0] = report;
            Dispatcher.BeginInvoke(new ReadHandlerDelegate(ReadHandler), myArray);
        }

        public bool ismaster = true;

        private void ReadHandler(HidReport report)
        {
            byte[] data = report.Data;
            /*foreach (byte b in data)
                Console.WriteLine(b);
            Console.WriteLine("--------------");*/

            idIndex = Array.IndexOf(deviceID, data[2]);
            if (data[3] != 0 && idIndex == -1 && !isFull)
            {
                //Console.WriteLine(data[0] + " | "+ data[7]);
                if (data[3] != 0)
                    NewDevice(data[2]);
            }
            else if (idIndex != -1 && !isInput[idIndex])
            {
                isInput[idIndex] = true;

            }



            if (idIndex != -1)
            {
                if (flag[idIndex] == FLAG_MASTER && ismaster)
                {
                    Multipoint(pointers[idIndex], data, idIndex);
                    flagLabal[idIndex].Content = Properties.Resources.presenter;

                }
                else if (flag[idIndex] == FLAG_SLAVE || !ismaster)
                {
                    SlavePointer(pointers[idIndex], data, idIndex);
                    flagLabal[idIndex].Content = Properties.Resources.audience + idIndex;

                }


            }


            if (device != null)
            {
                device.ReadReport(OnReport);
            }
        }

        private void KeyInput(byte data)
        {

            switch (data)
            {
                case 80:
                    SendKeys.SendWait("{LEFT}");
                    break;
                case 79:
                    SendKeys.SendWait("{RIGHT}");
                    break;
                case 62:
                    SendKeys.SendWait("{F5}");
                    break;
                case 55:
                    SendKeys.SendWait(".");
                    break;
                case 41:
                    SendKeys.SendWait("{ESC}");
                    break;
                case 5:
                    SendKeys.SendWait("b");
                    break;
            }

        }
        bool isMainPointerOn = false;
        bool isSpotlightOn = false;

        private void MagPointerOld(bool isOn)
        {
            if (isOn) magnifier.MagnifierOn();
            else magnifier.MagnifierOff();

            isMainPointerOn = isOn;
        }

        private void MagPointer(bool isOn, int index)
        {

            /*if (isOn) magnifier.MagnifierOn();
            else magnifier.MagnifierOff();*/

            if (isOn)
            {
                magnifiers[index].subMagnifierOn();
            }
            else
            {
                magnifiers[index].subMagnifierOff();

                for (int i = 0; i < 3; i++)
                {
                    if (magnifiers[i].isOn == true)
                    {
                        if (i != index)
                        {
                            magnifiers[i].TopMost = true;
                            magnifiers[i].stroke.TopMost = true;
                        }
                    }
                }

            }

            magnifiers[index].isOn = isOn;
        }

        private void SpotPointer(bool isOn)
        {

            if (isOn) spotlightForm.SpotlightOn();
            else spotlightForm.SpotlightOff();
            isSpotlightOn = isOn;
        }

        public bool isOldMag = true;
        public bool isDialog = false;
        public bool alwaysCenter = false;
        private bool[] isOn = { false, false, false };
        public int masterID = 0;
        public int magRefresh = 6;
        private void Multipoint(PointerView multi, byte[] data, int index)
        {
            masterID = index;
            int mode = multi.GetPointerMode();

            System.Drawing.Point mousePosition = System.Windows.Forms.Control.MousePosition;

            
            timerAutoHide.Start();
            isPointOn = true;
            
            /*if (data[7] != 0)
            {
            }*/

            if (data[7] == 0x0e)
            {
                if (alwaysCenter && !isOn[index])
                {
                    Screen currentScreen = Screen.FromPoint(mousePosition);

                    Console.WriteLine(currentScreen.Bounds.Width / 2);
                    SetCursorPos(currentScreen.Bounds.X + currentScreen.Bounds.Width / 2, currentScreen.Bounds.Y + currentScreen.Bounds.Height / 2);
                    isOn[index] = true;

                    return;
                }

                SetCursorPos(mousePosition.X + (sbyte)data[5], mousePosition.Y + (sbyte)data[6]);

                //multi.MovePointer((sbyte)data[5], (sbyte)data[6]);
                if (mode == MODE_MAGNIFIER)
                {
                    if (Math.Abs((sbyte)data[5]) < 2) data[5] = 0;
                    if (Math.Abs((sbyte)data[6]) < 2) data[6] = 0;

                    if (isOldMag == false)
                    {
                        if (magnifiers[index].isOn == false)
                        {
                            if (isSpotlightOn)
                                SpotPointer(false);

                            for (int i = 0; i < 3; i++)
                            {
                                magnifiers[i].Hide();
                                magnifiers[i].stroke.PointerOff();
                                pointers[i].Hide();

                            }

                            MagPointer(true, index);
                            //if (multi.IsVisible != true || multi.Opacity != 1) multi.PointerOn();


                            for (int i = 0; i < 3; i++)
                            {
                                if (magnifiers[i].isOn == true)
                                {
                                    if (i != index)
                                    {
                                        magnifiers[i].Show();
                                        magnifiers[i].stroke.PointerOn();
                                    }

                                }

                                if (pointers[i].isOn == true)
                                {
                                    pointers[i].Show();
                                }
                            }

                            magnifiers[index].TopMost = true;
                            magnifiers[index].stroke.TopMost = true;

                            if (isSpotlightOn)
                            {
                                SpotPointer(true);
                            }


                        }
                        else if (data[0] % magRefresh == 0)
                        {


                            magnifiers[index].moveMagnifier((sbyte)data[5], (sbyte)data[6], true);

                            spotlightForm.pictureBoxes[index].Visible = true;
                            spotlightForm.MoveMainSpotLight(index);

                        }
                    }
                    else
                    {
                        if (!isMainPointerOn)
                        {
                            MagPointerOld(true);
                        }
                        else
                        {
                            spotlightForm.pictureBoxes[index].Visible = true;
                            spotlightForm.MoveMainSpotLight(index);
                        }
                    }

                }
                else if (mode == MODE_SPOTLIGHT)
                {
                    if (isSpotlightOn == false && data[4] != 1)
                    {
                        SpotPointer(true);
                    }
                    else if (data[0] % 2 == 0)
                    {
                        //spotlight.MoveSpotLight();
                        spotlightForm.pictureBoxes[index].Visible = true;
                        spotlightForm.MoveMainSpotLight(index);
                    }
                }
                else
                {
                    if (multi.IsVisible != true || multi.Opacity != 1) multi.PointerOn();

                    PresentationSource presentationSource = PresentationSource.FromVisual(this);
                    if (presentationSource == null) return;
                    System.Windows.Point screenPosition = presentationSource.CompositionTarget.TransformFromDevice.Transform(new Point(mousePosition.X, mousePosition.Y));
                    multi.Left = screenPosition.X + (sbyte)data[5];
                    multi.Top = screenPosition.Y + (sbyte)data[6];
                }


            }
            else if (data[7] == 0x0f)
            {
                isOn[index] = false;
                multi.PointerOff();
                if (magnifiers[index].isOn == true || isMainPointerOn == true)
                {
                    spotlightForm.pictureBoxes[index].Visible = false;
                    MagPointer(false, index);
                    MagPointerOld(false);

                }
                else if (isSpotlightOn == true)
                {
                    spotlightForm.pictureBoxes[index].Visible = false;
                    SpotPointer(false);


                }
                foreach (PointerView pointer in pointers)
                {
                    //pointer.Topmost = false;
                    pointer.Topmost = true;
                }
                GC.Collect();
            }

            if (data[3] == 0xfd && data[7] == 0x08 && indexCnt[index] != data[0])
            {
                spotlightForm.ismag = false;
                ChangePointerMode(mode + 1, index);
                //multi.Visibility = Visibility.Hidden;
            }
            else if (data[3] == 0xff && indexCnt[index] != data[0])
            {

                KeyInput(data[5]);
            }
            else if (data[3] == 0xfe && indexCnt[index] != data[0])
            {

                if (data[4] == 2 && indexCnt[index] != data[0])
                {
                    PTmodeSetFuntion(PTMODE_CHANGE);
                }

                if (isDown == false && data[4] == 1)
                {
                    if (mode == MODE_SPOTLIGHT)
                    {
                        SpotPointer(false);
                        mouse_event(LBUTTONDOWN | LBUTTONUP, 0, 0, 0, 0);
                        isDown = true;
                        //SpotPointer(true);
                    }
                    else if (mode == MODE_MAGNIFIER)
                    {
                        SendKeys.SendWait("{RIGHT}");
                        MagPointer(false, index);
                        isDown = true;
                    }
                    else if (!isDialog)
                    {
                        mouse_event(LBUTTONDOWN, 0, 0, 0, 0);
                        isDown = true;
                    }
                    else
                    {
                        mouse_event(LBUTTONDOWN | LBUTTONUP, 0, 0, 0, 0);
                        isDown = false;
                        if (mode == MODE_SPOTLIGHT)
                        {
                            SpotPointer(true);
                        }
                    }



                }
                else if (isDown == true && data[4] == 0)
                {
                    /*if (mode == MODE_SPOTLIGHT)
                    {
                        //SpotPointer(true);
                    }
                    else */
                    if (mode == MODE_MAGNIFIER)
                    {
                        return;
                    }
                    mouse_event(LBUTTONUP, 0, 0, 0, 0);
                    isDown = false;
                }
            }

            indexCnt[index] = data[0];
        }

        bool isSubDown = false;
        Point[] subMagPos = new Point[3] { new Point(0, 0), new Point(0, 0), new Point(0, 0) };

        private void SlavePointer(PointerView multi, byte[] data, int index)
        {
            timerAutoHideSub.Start();
            isSubPointer = true;


            if (isPTmode)
            {
                multi.PointerOff();
                return;
            }

            

            int mode = multi.GetPointerMode();

            if (data[7] == 0x0e)
            {
                

                if (mode == MODE_MAGNIFIER)
                {
                    

                    if (Math.Abs((sbyte)data[5]) < 2) data[5] = 0;
                    if (Math.Abs((sbyte)data[6]) < 2) data[6] = 0;

                    if (magnifiers[index].isOn == false)
                    {
                        if (isSpotlightOn)
                            SpotPointer(false);

                        for (int i = 0; i < 3; i++)
                        {
                            magnifiers[i].Hide();
                            magnifiers[i].stroke.PointerOff();

                            pointers[i].Hide();
                        }

                        

                        MagPointer(true, index);
                        //if (multi.IsVisible != true || multi.Opacity != 1) multi.PointerOn();

                        for (int i = 0; i < 3; i++)
                        {
                            if (magnifiers[i].isOn == true)
                            {
                                if (i != index)
                                {
                                    magnifiers[i].Show();
                                    magnifiers[i].stroke.PointerOn();
                                }
                            }

                            if (pointers[i].isOn == true)
                            {
                                pointers[i].Show();

                            }
                        }

                        if (isSpotlightOn)
                        {
                            SpotPointer(true);
                        }
                        spotlightForm.pictureBoxes[index].Visible = true;

                        

                        magnifiers[index].TopMost = true;
                        magnifiers[index].stroke.TopMost = true;
                    }
                    else if (data[0] % magRefresh == 0)
                    {

                        if (alwaysCenter && !isOn[index])
                        {
                            Screen currentScreen = Screen.FromPoint(System.Windows.Forms.Control.MousePosition);
                            spotlightForm.setCenter(index);
                            magnifiers[index].setCenter();
                            isOn[index] = true;
                            subMagPos[index].X = 0; subMagPos[index].Y = 0;
                            return;
                        }

                        //magnifiers[index].moveMagnifier((sbyte)data[5], (sbyte)data[6]);
                        magnifiers[index].moveMagnifier(subMagPos[index].X + (sbyte)data[5], subMagPos[index].Y + (sbyte)data[6]);
                        subMagPos[index].X = 0; subMagPos[index].Y = 0;
                        spotlightForm.magTracking(index, magnifiers[index].Bounds.X, magnifiers[index].Bounds.Y);
                    }
                    else
                    {
                        subMagPos[index].X += (sbyte)data[5];
                        subMagPos[index].Y += (sbyte)data[6];
                    }

                    
                }
                else if (mode == MODE_SPOTLIGHT)
                {
                    if (isSpotlightOn == false)
                    {
                        SpotPointer(true);
                    }
                    else /*if (data[0] % 2 == 0)*/
                    {
                        if (alwaysCenter && !isOn[index])
                        {
                            spotlightForm.setCenter(index);
                            isOn[index] = true;
                        }
                        //if(index == 1)
                        {
                            spotlightForm.setDxDy(index, (sbyte)data[5], (sbyte)data[6]);
                            spotlightForm.pictureBoxes[index].Visible = true;

                        }

                    }
                }
                else
                {
                    if (multi.IsVisible != true || multi.Opacity != 1) multi.PointerOn();

                    if (alwaysCenter && !isOn[index])
                    {
                        Screen currentScreen = Screen.FromPoint(System.Windows.Forms.Control.MousePosition);

                        multi.Left = currentScreen.Bounds.X + currentScreen.Bounds.Width / 2;
                        multi.Top = currentScreen.Bounds.Y + currentScreen.Bounds.Height / 2;
                        isOn[index] = true;
                    }


                    multi.MovePointer((sbyte)data[5], (sbyte)data[6]);
                }


            }
            else if (data[7] == 0x0f || flag[0] == 2)
            {
                isOn[index] = false;

                multi.PointerOff();
                if (mode == MODE_MAGNIFIER)
                {
                    if (magnifiers[index].isOn == true)
                    {
                        MagPointer(false, index);

                        spotlightForm.pictureBoxes[index].Visible = false;
                    }
                    if (isSpotlightOn == true)
                    {
                        spotlightForm.pictureBoxes[index].Visible = false;
                        SpotPointer(false);

                    }


                }
                else if (mode == MODE_SPOTLIGHT)
                {
                    if (isSpotlightOn == true)
                    {
                        spotlightForm.pictureBoxes[index].Visible = false;
                        SpotPointer(false);
                    }
                }

                foreach (PointerView pointer in pointers)
                {
                    pointer.Topmost = true;
                }
                GC.Collect();
            }
            else if (data[3] == 0xfd && data[7] == 0x08 && indexCnt[index] != data[0])
            {
                ChangePointerMode(mode + 1, index);
            }

            if (data[3] == 0xff && indexCnt[index] != data[0])
            {
                KeyInput(data[5]);
            }
            else if (data[3] == 0xfe && indexCnt[index] != data[0])
            {

                if (data[4] == 1 && isSubDown == false)
                {
                    SendKeys.SendWait("{RIGHT}");
                    MagPointer(false, index);
                    isSubDown = true;
                }
                else if (data[4] == 0 && isSubDown == true)
                {
                    isSubDown = false;
                }

            }

            indexCnt[index] = data[0];
        }



        public void SetMagnifier(double size, double magnification)
        {

            int cnt = 0;
            foreach (FormMagnifier mag in magnifiers)
            {
                mag.setMagnification((int)magnification);
                mag.setMagSize((int)(size * size));
            }

            foreach (PointerView pointer in pointers)
            {
                //pointer.SetStrokeSize(magnifierPage.MagnifierSizeSlider.Value);
                if (pointer.GetPointerMode() == MODE_MAGNIFIER)
                {
                    //spotlightForm.setSpotlight(cnt, (int)(size * size), cnt);
                    spotlightForm.SetMagSize(cnt, (int)(magnifierPage.MagnifierSizeSlider.Value * magnifierPage.MagnifierSizeSlider.Value), spotLightPage.BaseOpacitySlider.Value / 100);
                }
                cnt++;
            }
            magnifierPage.SampleViewSize(magnifierPage.MagnifierSizeSlider.Value);

            magnifier.ResizeMagnifier((int)(size * size));
            magnifier.Magnification = (float)magnification;
        }

        public void SetSpotlight(double size, double opacity, int magIndex = 4)
        {

            spotlightForm.setSpotlight((int)(size * size), opacity / 100);
            int cnt = 0;
            foreach (PointerView pointer in pointers)
            {
                if (pointer.GetPointerMode() == MODE_MAGNIFIER)
                {
                    Console.WriteLine("true");
                    spotlightForm.SetMagSize(cnt, (int)(magnifierPage.MagnifierSizeSlider.Value * magnifierPage.MagnifierSizeSlider.Value), opacity / 100);
                }
                cnt++;
            }
        }

        public void MainDeviceSelect(int index)
        {
            //bool isMagnifier = false;
            int masterIndex = Array.IndexOf(flag, FLAG_MASTER);
            if (masterIndex != -1 && pointers[masterIndex].GetPointerMode() == MODE_MAGNIFIER)
            {
                pointers[masterIndex].PointingModeSet(pointers[Array.IndexOf(flag, FLAG_SLAVE)].GetPointerMode());
            }

            for (int i = 0; i < flag.Length; i++)
            {

                if (i == index)
                {
                    DeviceFlag[deviceID[i].ToString()][1] = FLAG_MASTER;
                    flag[i] = FLAG_MASTER;

                }
                else if (deviceID[i] != 500)
                {
                    DeviceFlag[deviceID[i].ToString()][1] = FLAG_SLAVE;
                    flag[i] = FLAG_SLAVE;
                    //Console.WriteLine(DeviceFlag[deviceID[i].ToString()][1]);
                    //Console.WriteLine("----");
                }
            }
            foreach (int devId in deviceID)
            {
                FlagTextSet(devId.ToString());
            }
            SaveFlagSet();
        }

        private void NewDevice(byte data)
        {
            int newIndex = Array.IndexOf(deviceID, 500);
            if (newIndex == -1)
            {
                isFull = true;
                return;
            }
            string devID = data.ToString();
            if (!DeviceFlag.ContainsKey(devID))
            {
                if (Array.IndexOf(flag, FLAG_MASTER) == -1)
                    DeviceFlag[devID] = new int[] { newIndex, FLAG_MASTER };
                else
                    DeviceFlag[devID] = new int[] { newIndex, FLAG_SLAVE };
                deviceID[newIndex] = data;

                if (isPTmode && DeviceFlag[devID][1] != FLAG_MASTER)
                    flag[newIndex] = FLAG_BLOCK;
                else
                    flag[newIndex] = DeviceFlag[devID][1];
            }
            else
            {


                if (deviceID[DeviceFlag[devID][0]] != 500)
                {
                    DeviceFlag[deviceID[DeviceFlag[devID][0]].ToString()][0] = newIndex;

                    if (DeviceFlag[devID][1] == FLAG_MASTER)
                    {
                        DeviceFlag[deviceID[DeviceFlag[devID][0]].ToString()][1] = FLAG_SLAVE;
                    }


                    flag[newIndex] = DeviceFlag[deviceID[DeviceFlag[devID][0]].ToString()][1];
                    deviceID[newIndex] = deviceID[DeviceFlag[devID][0]];
                }



                deviceID[DeviceFlag[devID][0]] = data;

                if (isPTmode && DeviceFlag[devID][1] != FLAG_MASTER)
                    flag[DeviceFlag[devID][0]] = FLAG_BLOCK;
                else
                    flag[DeviceFlag[devID][0]] = DeviceFlag[devID][1];
            }
            SaveFlagSet();
            FlagTextSet(devID);
            selectMainDevice.ResetView();
        }



        private void FlagTextSet(string devID)
        {
            if (devID == "500") return;
            switch (flag[DeviceFlag[devID][0]])
            {
                case FLAG_MASTER:
                    flagLabal[DeviceFlag[devID][0]].Content = Properties.Resources.presenter;
                    break;

                case FLAG_SLAVE:
                    flagLabal[DeviceFlag[devID][0]].Content = Properties.Resources.audience + DeviceFlag[devID][0];
                    break;

                case FLAG_BLOCK:
                    flagLabal[DeviceFlag[devID][0]].Content = Properties.Resources.block;
                    break;

                case 4:
                    flagLabal[DeviceFlag[devID][0]].Content = Properties.Resources.none;
                    break;

                default:
                    break;
            }
        }

        public void EnableMode(int mode, bool enable)
        {
            modes[mode] = enable;
        }

        int modeNow = 0;

        public void ChangePointerMode(int mode, int pointerindex = 4)
        {
            int masterIndex = Array.IndexOf(flag, FLAG_MASTER) == -1 ? 0 : Array.IndexOf(flag, FLAG_MASTER);

            SolidColorBrush enableColor = new SolidColorBrush(Color.FromArgb(255, 255, 107, 0));
            SolidColorBrush defaultColor = new SolidColorBrush(Colors.White);

            if (SettingPanel.Visibility == Visibility.Visible) { SettingPageOn(false); }


            if (pointerindex != masterIndex && pointerindex != 4 && isOldMag == true)
            {
                /*if (pointers[masterIndex].GetPointerMode() != MODE_SPOTLIGHT)
                    mode = mode > MODE_COLOR ? MODE_IMAGE : MODE_COLOR;
                else
                    mode = mode == MODE_MAGNIFIER ? MODE_SvvvxPOTLIGHT : mode;*/

                mode = mode == MODE_MAGNIFIER ? MODE_SPOTLIGHT : mode;
            }
            else
            {
                magnifier.MagnifierOff();
                spotlightForm.Hide();
            }


            if (modes.Length > mode && modes[mode] == false)
            {
                ChangePointerMode(mode += 1, pointerindex);
                return;
            }


            foreach (Button button in PageButtons)
            {
                button.Foreground = defaultColor;
            }
            foreach (Label label in arrowLabel)
            {
                label.Foreground = defaultColor;
                label.Content = "▷";
            }

            Console.WriteLine(pointerindex);

            switch (mode)
            {
                case MODE_IMAGE:

                    ImagePageButton.Foreground = enableColor;
                    PointerFrame.Content = imagePointerPage;
                    if (/*masterIndex == pointerindex ||*/ pointerindex == 4)
                    {
                        foreach (PointerView pointer in pointers)
                        {
                            pointer.SetImageSize(imagePointerPage.ImageSizeSlider.Value, imagePointerPage.ImageOpacitySlider.Value);
                            pointer.PointingModeSet(MODE_IMAGE);
                        }
                    }
                    else
                    {
                        pointers[pointerindex].SetImageSize(imagePointerPage.ImageSizeSlider.Value, imagePointerPage.ImageOpacitySlider.Value);
                        pointers[pointerindex].PointingModeSet(MODE_IMAGE);

                    }
                    arrowLabel[mode].Foreground = enableColor;
                    arrowLabel[mode].Content = "▶";
                    modeNow = MODE_IMAGE;
                    break;

                case MODE_COLOR:

                    PointPageButton.Foreground = enableColor;
                    PointerFrame.Content = colorPointerPage;
                    if (/*masterIndex == pointerindex ||*/ pointerindex == 4)
                    {
                        foreach (PointerView pointer in pointers)
                        {
                            pointer.SetColorSize(colorPointerPage.ColorSizeSlider.Value, colorPointerPage.ColorOpacitySlider.Value);
                            pointer.PointingModeSet(MODE_COLOR);
                        }
                    }
                    else
                    {
                        pointers[pointerindex].SetColorSize(colorPointerPage.ColorSizeSlider.Value, colorPointerPage.ColorOpacitySlider.Value);
                        pointers[pointerindex].PointingModeSet(MODE_COLOR);

                    }
                    arrowLabel[mode].Foreground = enableColor;
                    arrowLabel[mode].Content = "▶";
                    modeNow = MODE_COLOR;
                    break;
                case MODE_MAGNIFIER:
                    MagnifierPageButton.Foreground = enableColor;
                    PointerFrame.Content = magnifierPage;
                    magnifierPage.SetSampleView();

                    //SetSpotlight(magnifierPage.MagnifierSizeSlider.Value, spotLightPage.BaseOpacitySlider.Value, pointerindex);
                    if (pointerindex != 4)
                    {

                        pointers[pointerindex].PointingModeSet(MODE_MAGNIFIER);
                        //pointers[pointerindex].SetStrokeSize(magnifierPage.MagnifierSizeSlider.Value);
                        spotlightForm.SetMagSize(pointerindex, (int)(magnifierPage.MagnifierSizeSlider.Value * magnifierPage.MagnifierSizeSlider.Value), spotLightPage.BaseOpacitySlider.Value / 100);

                    }
                    else
                    {
                        if (isOldMag == true)
                        {
                            foreach (PointerView pointer in pointers)
                            {
                                if (pointer.GetPointerMode() == MODE_MAGNIFIER)
                                    pointer.PointingModeSet(MODE_COLOR);
                            }
                            pointers[masterIndex].PointingModeSet(MODE_MAGNIFIER);
                        }
                        else
                        {
                            foreach (PointerView pointer in pointers)
                            {
                                pointer.SetStrokeSize(magnifierPage.MagnifierSizeSlider.Value);
                                pointer.PointingModeSet(MODE_MAGNIFIER);
                            }
                        }

                    }
                    SetMagnifier(magnifierPage.MagnifierSizeSlider.Value, magnifierPage.MagnificationSlider.Value);

                    //MagPointer(true);
                    spotlightForm.ismag = true;
                    arrowLabel[mode].Foreground = enableColor;
                    arrowLabel[mode].Content = "▶";
                    modeNow = MODE_MAGNIFIER;
                    break;
                case MODE_SPOTLIGHT:
                    spotLightPage.SetSampleView();
                    SpotLightPageButton.Foreground = enableColor;
                    PointerFrame.Content = spotLightPage;
                    if (/*masterIndex == pointerindex || */pointerindex == 4)
                    {
                        foreach (PointerView pointer in pointers)
                        {
                            pointer.PointingModeSet(MODE_SPOTLIGHT);
                        }
                        SetSpotlight(spotLightPage.SpotlightSizeSlider.Value, spotLightPage.BaseOpacitySlider.Value, pointerindex);
                    }
                    else
                    {
                        pointers[pointerindex].PointingModeSet(MODE_SPOTLIGHT);
                        spotlightForm.pictureBoxes[pointerindex].Visible = true;
                        spotlightForm.SetMagSize(pointerindex, (int)(spotLightPage.SpotlightSizeSlider.Value * spotLightPage.SpotlightSizeSlider.Value), spotLightPage.BaseOpacitySlider.Value / 100);
                        //SpotPointer(true);
                    }
                    //pointers[masterIndex].PointingModeSet(MODE_SPOTLIGHT);
                    spotLightPage.BaseOpacitySlider.Value += 0.1;
                    arrowLabel[mode].Foreground = enableColor;
                    arrowLabel[mode].Content = "▶";
                    modeNow = MODE_SPOTLIGHT;
                    break;
                default:
                    ChangePointerMode(Array.IndexOf(modes, true), pointerindex);
                    break;
            }
            timerAutoHide.Start();

        }


        public void SettingPageOn(bool isOn)
        {
            FrameResize(isOn);
            if (/*SettingPanel.Visibility != Visibility.Visible*/isOn)
            {
                SettingPanel.Visibility = Visibility.Visible;
                verLabel.Visibility = Visibility.Visible;
                SettingButton.Background = new SolidColorBrush(Color.FromArgb(230, 123, 123, 123));
                EditButton.Background = new SolidColorBrush(Color.FromArgb(0, 123, 123, 123));
                SelectMainDevButton.Foreground = new SolidColorBrush(Colors.White);
                NewDeviceButton.Foreground = new SolidColorBrush(Colors.White);
                foreach (Label label in flagLabal)
                {
                    label.Visibility = Visibility.Hidden;
                }
                if (PointerFrame.Content != settingPage)
                {
                    //PointerFrame.Content = settingPage;
                    SettingPageView(0);
                    PTmodePanel.Visibility = Visibility.Hidden;
                }
            }
            else
            {
                SettingPanel.Visibility = Visibility.Collapsed;
                verLabel.Visibility = Visibility.Collapsed;
                SettingButton.Background = new SolidColorBrush(Color.FromArgb(0, 123, 123, 123));
                EditButton.Background = new SolidColorBrush(Color.FromArgb(230, 123, 123, 123));
                if (Array.IndexOf(flag, FLAG_MASTER) != -1)
                    ChangePointerMode(pointers[Array.IndexOf(flag, FLAG_MASTER)].GetPointerMode());
                else if (Array.IndexOf(flag, FLAG_SLAVE) != -1)
                    ChangePointerMode(pointers[Array.IndexOf(flag, FLAG_SLAVE)].GetPointerMode());
                else
                    ChangePointerMode(MODE_IMAGE);

                foreach (Label label in flagLabal)
                {
                    label.Visibility = Visibility.Visible;
                }
                PTmodePanel.Visibility = Visibility.Visible;
            }
        }

        public void SettingPageView(int page)
        {
            switch (page)
            {
                case 0:
                    SelectMainDevButton.Foreground = new SolidColorBrush(Colors.White);
                    NewDeviceButton.Foreground = new SolidColorBrush(Colors.White);
                    SettingPanelButton.Foreground = new SolidColorBrush(Color.FromRgb(255, 107, 0));
                    PointerFrame.Content = settingPage;
                    break;
                case 1:
                    SelectMainDevButton.Foreground = new SolidColorBrush(Color.FromRgb(255, 107, 0));
                    NewDeviceButton.Foreground = new SolidColorBrush(Colors.White);
                    SettingPanelButton.Foreground = new SolidColorBrush(Colors.White);
                    PointerFrame.Content = selectMainDevice;
                    selectMainDevice.ResetView();
                    break;
                case 2:
                    NewDeviceButton.Foreground = new SolidColorBrush(Color.FromRgb(255, 107, 0));
                    SelectMainDevButton.Foreground = new SolidColorBrush(Colors.White);
                    SettingPanelButton.Foreground = new SolidColorBrush(Colors.White);
                    if (ChMatchPage1 != null)
                    {
                        ChMatchPage1 = null;
                        ChMatchPage1 = new ChMatchPage1(this);
                    }
                    PointerFrame.Content = ChMatchPage1;
                    break;

            }
        }

        private void PTmodeSetFuntion(int isOn)
        {
            SolidColorBrush select = new SolidColorBrush(Color.FromArgb(255, 255, 107, 0));
            SolidColorBrush notSelect = new SolidColorBrush(Color.FromArgb(0, 255, 107, 0));
            modeLabel.Opacity = 1;
            switch (isOn)
            {
                case PTMODE_ON:
                    PTmodeSet.Background = select;
                    PTmodeSet.BorderBrush = select;
                    MeetingModeSet.Background = notSelect;
                    MeetingModeSet.BorderBrush = new SolidColorBrush(Colors.White);
                    for (int i = 0; i < flag.Length; i++)
                        if (flag[i] == FLAG_SLAVE)
                        {
                            flag[i] = FLAG_BLOCK;
                            pointers[i].Visibility = Visibility.Hidden;
                            spotlightForm.pictureBoxes[i].Visible = false;
                            FlagTextSet(deviceID[i].ToString());
                        }
                    modeLabel.GenText.Visibility = Visibility.Hidden;
                    modeLabel.PTlText.Visibility = Visibility.Visible;
                    isPTmode = true;
                    break;
                case PTMODE_OFF:
                    PTmodeSet.Background = notSelect;
                    PTmodeSet.BorderBrush = new SolidColorBrush(Colors.White);
                    MeetingModeSet.Background = select;
                    MeetingModeSet.BorderBrush = select;
                    for (int i = 0; i < flag.Length; i++)
                        if (flag[i] == FLAG_BLOCK)
                        {
                            flag[i] = FLAG_SLAVE;
                            FlagTextSet(deviceID[i].ToString());
                        }
                    modeLabel.PTlText.Visibility = Visibility.Hidden;
                    modeLabel.GenText.Visibility = Visibility.Visible;
                    isPTmode = false;
                    break;
                case PTMODE_CHANGE:
                    if (isPTmode) PTmodeSetFuntion(PTMODE_OFF);
                    else PTmodeSetFuntion(PTMODE_ON);
                    break;
            }



            modeLabel.PosSet();
            if (device == null)
            {
                modeLabel.Hide();
                return;
            }
            modeLabel.Show();
            ptModeCnt = 0;
            timerChangeMode.Start();
        }

        //-----------------------------------
        //USB 연결
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource hwndSource = PresentationSource.FromVisual(this) as HwndSource;
            hwndSource.AddHook(new HwndSourceHook(WndProc));
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {

            if (msg == WM_DEVICECHANGE)
            {
                if (isReConnect == false)
                {
                    isReConnect = true;
                    ConnectDevice();
                }

            }
            return IntPtr.Zero;
        }
        //-----------------------------------

        private void PointPageButton_Click(object sender, RoutedEventArgs e)
        {
            ChangePointerMode(MODE_COLOR);

        }
        private void ImagePageButton_Click(object sender, RoutedEventArgs e)
        {
            ChangePointerMode(MODE_IMAGE);
        }

        private void MagnifierPageButton_Click(object sender, RoutedEventArgs e)
        {
            ChangePointerMode(MODE_MAGNIFIER);
        }

        private void SpotLightPageButton_Click(object sender, RoutedEventArgs e)
        {
            ChangePointerMode(MODE_SPOTLIGHT);

        }




        private int ptModeCnt = 0;
        private void TimerChangeMode_Tick(object sender, EventArgs e)
        {
            //Console.WriteLine("모드변경");
            modeLabel.PosSet();
            if (device == null) modeLabel.Hide();

            if (ptModeCnt < 5)
            {
                ptModeCnt += 1;
                return;
            }
            else
            {
                ptModeCnt = 0;
                timerChangeMode.Stop();
                modeLabel.timer.Start();
            }

        }

        private void PTmodeSet_Click(object sender, RoutedEventArgs e)
        {
            PTmodeSetFuntion(PTMODE_ON);
        }
        private void MeetingModeSet_Click(object sender, RoutedEventArgs e)
        {
            PTmodeSetFuntion(PTMODE_OFF);

        }
        private void SettingButton_Click(object sender, RoutedEventArgs e)
        {
            if (device == null) return;
            SettingPageOn(true);


        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (device == null) return;
            SettingPageOn(false);
        }

        private bool isDragging = false;
        private Point startPoint;
        private void TopBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                isDragging = true;
                startPoint = e.GetPosition(this);

                timerMoveWindow.Start();

            }
        }

        private void TimerMoveWindow_Tick(object sender, EventArgs e)
        {
            if (isDragging)
            {
                System.Drawing.Point mousePosition = System.Windows.Forms.Control.MousePosition;
                PresentationSource presentationSource = PresentationSource.FromVisual(this);
                System.Windows.Point screenPosition = presentationSource.CompositionTarget.TransformFromDevice.Transform(new Point(mousePosition.X, mousePosition.Y));

                this.Left = screenPosition.X - startPoint.X;
                this.Top = screenPosition.Y - startPoint.Y;
                //this.Left = mousePosition.X - startPoint.X;
                //this.Top = mousePosition.Y - startPoint.Y;
            }
            else
                timerMoveWindow.Stop();
        }

        bool isPointOn = false;
        bool isSubPointer = false;

        private void TimerAutoHide_Tick(object sender, EventArgs e)
        {
            int cnt = 0;
            if (isPointOn)
            {
                isPointOn = false;
            }
            else
            {
                int master = Array.IndexOf(flag, FLAG_MASTER);
                timerAutoHide.Stop();
                foreach (PointerView pointer in pointers)
                {
                    if (cnt == master)
                    {
                        pointer.PointerOff();
                        MagPointer(false, cnt);
                    }
                    cnt++;
                }



                if (isSpotlightOn) SpotPointer(false);

                for (int i = 0; i < spotlightForm.pictureBoxes.Count; i++)
                {
                    spotlightForm.pictureBoxes[i].Visible = false;
                }



            }



        }

        private void TimerAutoHideSub_Tick(object sender, EventArgs e)
        {
            if (isSubPointer)
            {
                isSubPointer = false;
            }
            else
            {
                int master = Array.IndexOf(flag, FLAG_MASTER);

                timerAutoHideSub.Stop();
                for (int i = 0; i < pointers.Count; i++)
                {
                    if (i == master)
                        continue;
                    pointers[i].Hide();
                    MagPointer(false, i);
                }

                for (int i = 0; i < spotlightForm.pictureBoxes.Count; i++)
                {
                    if (i == master)
                        continue;
                    spotlightForm.pictureBoxes[i].Visible = false;
                }



            }
        }

        private void TopBar_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (isDragging)
            {

                System.Drawing.Point mousePosition = System.Windows.Forms.Control.MousePosition;
                PresentationSource presentationSource = PresentationSource.FromVisual(this);
                System.Windows.Point screenPosition = presentationSource.CompositionTarget.TransformFromDevice.Transform(new Point(mousePosition.X, mousePosition.Y));

                this.Left = screenPosition.X - startPoint.X;
                this.Top = screenPosition.Y - startPoint.Y;
            }
        }

        private void TopBar_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                isDragging = false;
            }
        }
        private void Window_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (isDragging)
            {
                System.Drawing.Point mousePosition = System.Windows.Forms.Control.MousePosition;
                PresentationSource presentationSource = PresentationSource.FromVisual(this);
                System.Windows.Point screenPosition = presentationSource.CompositionTarget.TransformFromDevice.Transform(new Point(mousePosition.X, mousePosition.Y));

                this.Left = screenPosition.X - startPoint.X;
                this.Top = screenPosition.Y - startPoint.Y;
                //isDragging = false;
            }
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
            SavePaeameter();
            ShowInTaskbar = false;
        }

        private void MinimizedButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        public void FrameResize(bool isSetting)
        {

            if (!isSetting)
            {
                Height = 596;
            }
            else
                Height = 596;
        }

        private void PointerFrame_SizeChanged(object sender, SizeChangedEventArgs e)
        {

            /*double frameHeight;
            Point controlPosition = PTmodePanel.TranslatePoint(new Point(0, 0), MainGrid);
            Point minPosition = PTmodePanel.TranslatePoint(new Point(0, 0), SpotLightPageButton);

            if (SettingPanel.Visibility == Visibility.Visible)
                frameHeight = PointerFrame.ActualHeight + PTmodeSet.ActualHeight + PTmodePanel.Margin.Bottom + TopBar.ActualHeight ;
                //frameHeight = PointerFrame.ActualHeight + PTmodePanel.ActualHeight + PTmodePanel.Margin.Bottom + TopBar.ActualHeight + 8;
            else
                frameHeight = PointerFrame.ActualHeight + PTmodePanel.ActualHeight + PTmodePanel.Margin.Bottom + TopBar.ActualHeight + 8;

            Height = frameHeight;*/



        }
        private void SettingPanelButton_Click(object sender, RoutedEventArgs e)
        {
            SettingPageView(0);
            //SettingPageView(1);
        }

        private void SelectMainDevButton_Click(object sender, RoutedEventArgs e)
        {
            SettingPageView(1);
        }

        private void NewDeviceButton_Click(object sender, RoutedEventArgs e)
        {
            SettingPageView(2);

        }

        Page beforePage;
        private void PointerFrame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            Page currentPage = e.Content as Page;
            if (beforePage != null)
            {
                //Console.WriteLine(beforePage.Title);
                if (beforePage.Title == "ChMatchPage1")
                {
                    if (device != null)
                        flagSet.ChannelMode(0x00, device);
                }
            }
            beforePage = currentPage;
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
        }

        private void CloseButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Button button = sender as Button;
            button.Background = new SolidColorBrush(Colors.Red);
        }

        private void CloseButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Button button = sender as Button;
            button.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
        }

        private void MinimizedButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Button button = sender as Button;
            button.Background = new SolidColorBrush(Colors.Gray);
        }

        private void MinimizedButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Button button = sender as Button;
            button.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
        }

        private void ImagePageButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Button button = sender as Button;
            button.Background = new SolidColorBrush(Color.FromArgb(255, 255, 107, 0));
            button.Foreground = new SolidColorBrush(Colors.White);

        }

        private void ImagePageButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Button button = sender as Button;
            button.Background = new SolidColorBrush(Color.FromArgb(1, 255, 107, 0));
            if (modeNow == int.Parse(button.Tag.ToString()))
            {
                button.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 107, 0));
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //SaveFlagSet();
            flagSet.ExitFuntion();
            ni.Dispose();
            SavePaeameter();
            System.Windows.Forms.Application.Exit();
            Environment.Exit(0);

            //System.Windows.Application.Current.Shutdown();
        }


        //--------------------------------------------
        //low
        //---------------
        /*[StructLayout(LayoutKind.Sequential)]
        struct INPUT
        {
            public uint type;
            public InputUnion u;
        }

        [StructLayout(LayoutKind.Explicit)]
        struct InputUnion
        {
            [FieldOffset(0)] public MOUSEINPUT mi;
            [FieldOffset(0)] public KEYBDINPUT ki;
            [FieldOffset(0)] public HARDWAREINPUT hi;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct HARDWAREINPUT
        {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }

        const uint INPUT_KEYBOARD = 1;
        const uint KEYEVENTF_KEYUP = 0x0002;

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        public void SendAKey()
        {
            INPUT[] inputs = new INPUT[2];

            // A 키 누르기
            inputs[0].type = INPUT_KEYBOARD;
            inputs[0].u.ki = new KEYBDINPUT
            {
                wVk = 0x41, // 'A'
                wScan = 0,
                dwFlags = 0,
                time = 0,
                dwExtraInfo = IntPtr.Zero
            };

            // A 키 떼기
            inputs[1].type = INPUT_KEYBOARD;
            inputs[1].u.ki = new KEYBDINPUT
            {
                wVk = 0x41,
                wScan = 0,
                dwFlags = KEYEVENTF_KEYUP,
                time = 0,
                dwExtraInfo = IntPtr.Zero
            };

            // 입력 실행
            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
        }
*/
    }
 }
