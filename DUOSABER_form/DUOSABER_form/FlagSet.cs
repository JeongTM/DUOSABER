using HidLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DUOsaber
{
    internal class FlagSet
    {
        const byte CMD_SET_FLAG = 1;
        const byte CMD_GET_FLAG = 2;

        const byte CMD_CH_ON = 0x0f;
        const byte CMD_CH_OFF = 0x00;

        const byte FLAG_MASTER = 0;
        const byte FLAG_SLAVE = 1;
        const byte FLAG_BLOCK = 2;

        private HidDevice device;
       
        
        public FlagSet()
        {
            
        }

        private void ConnectDevice()
        {
            //현재 연결되어있는 디바이스 목록
            HidDevice[] deviceList = HidDevices.Enumerate(0x0ebc, 0x2501, 0).ToArray();

            foreach (HidDevice dev in deviceList)
            {
                if (!dev.DevicePath.Contains("mi_02")) continue;

                device = dev;
                break;

            }


        }

        public void SetFlagHandler(byte mode, HidDevice hid)
        {
            //수신기 권한 입력
            byte[] data = new byte[8];
            data[0] = CMD_SET_FLAG;
            data[1] = mode;

            HidReport _report = new HidReport(data.Length);
            _report.Data = data;
            hid.WriteReport(_report);
        }

        public void ChannelMode(byte mode, HidDevice hid)
        {
            byte[] data = new byte[8];
            data[0] = 0xe8;
            data[1] = mode;

            HidReport _report = new HidReport(data.Length);
            _report.Data = data;
            hid.CloseDevice();
            hid.WriteReport(_report);
            hid.OpenDevice();
        }

        public void ExitFuntion()
        {
            ConnectDevice();
            if(device != null)
            {
                ChannelMode(CMD_CH_OFF, device);
                SetFlagHandler(FLAG_MASTER, device);
              
            }
            
        }

    }
}
