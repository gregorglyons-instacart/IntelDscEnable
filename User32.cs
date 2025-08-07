using System.Runtime.InteropServices;

namespace IntelDSCEnable;

public static class User32 {

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern bool EnumDisplayDevices(string? lpDevice, uint iDevNum, ref DisplayDevice lpDisplayDevice, uint dwFlags);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern bool EnumDisplaySettings(string lpszDeviceName, int iModeNum, ref Devmode lpDevMode);

    [DllImport("powrprof.dll", SetLastError = true)]
    private static extern uint CallNtPowerInformation(int InformationLevel, IntPtr lpInputBuffer, uint nInputBufferSize, IntPtr lpOutputBuffer, uint nOutputBufferSize);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct DisplayDevice
    {
        public int cb;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string DeviceName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceString;
        public uint StateFlags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceKey;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct Devmode
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmDeviceName;
        public short dmSpecVersion;
        public short dmDriverVersion;
        public short dmSize;
        public short dmDriverExtra;
        public int dmFields;
        public int dmPositionX;
        public int dmPositionY;
        public int dmDisplayOrientation;
        public int dmDisplayFixedOutput;
        public short dmColor;
        public short dmDuplex;
        public short dmYResolution;
        public short dmTTOption;
        public short dmCollate;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmFormName;
        public short dmLogPixels;
        public short dmBitsPerPel;
        public int dmPelsWidth;
        public int dmPelsHeight;
        public int dmDisplayFlags;
        public int dmDisplayFrequency;
        public int dmICMMethod;
        public int dmICMIntent;
        public int dmMediaType;
        public int dmDitherType;
        public int dmReserved1;
        public int dmReserved2;
        public int dmPanningWidth;
        public int dmPanningHeight;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct SystemBatteryState
    {
        public byte AcOnLine;
        public byte BatteryPresent;
        public byte Charging;
        public byte Discharging;
        public byte Spare1;
        public byte Spare2;
        public byte Spare3;
        public byte Spare4;
        public uint MaxCapacity;
        public uint RemainingCapacity;
        public int Rate; // Charging/discharging rate in mW
        public uint EstimatedTime;
        public uint DefaultAlert1;
        public uint DefaultAlert2;
    }

    public static IEnumerable<MonitorDetail> GetMonitorDetails()
    {
        var displayDevice = new DisplayDevice();
        displayDevice.cb = Marshal.SizeOf(displayDevice);
        
        for (uint deviceIndex = 0; EnumDisplayDevices(null, deviceIndex, ref displayDevice, 0); deviceIndex++)
        {
            // Only process active displays
            if ((displayDevice.StateFlags & 0x1) != 0)
            {
                var devMode = new Devmode();
                devMode.dmSize = (short)Marshal.SizeOf(devMode);
                
                if (EnumDisplaySettings(displayDevice.DeviceName, -1, ref devMode))
                {
                    //yeah, you can probably jam it directly in from the above. I'm lazy.
                    yield return new MonitorDetail
                    {
                        Name = displayDevice.DeviceName,
                        Width = devMode.dmPelsWidth,
                        Height = devMode.dmPelsHeight,
                        RefreshRate = devMode.dmDisplayFrequency,
                        BitsPerPixel = devMode.dmBitsPerPel,
                        PositionX = devMode.dmPositionX,
                        PositionY = devMode.dmPositionY
                    };
                }
            }
            
            // Reset for next iteration
            displayDevice.cb = Marshal.SizeOf(displayDevice);
        }
    }

    public static double? GetChargingRateInWatts()
    {
        var batteryState = new SystemBatteryState();
        var size = Marshal.SizeOf(batteryState);
        var ptr = Marshal.AllocHGlobal(size);
        
        try
        {
            var result = CallNtPowerInformation(5, IntPtr.Zero, 0, ptr, (uint)size);
            
            if (result == 0) // STATUS_SUCCESS
            {
                batteryState = Marshal.PtrToStructure<SystemBatteryState>(ptr);
                
                // Rate is in milliwatts, convert to watts
                // Positive rate means charging, negative means discharging
                return batteryState.Rate / 1000.0;
            }
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
        
        return null; // Failed to get battery information
    }
}